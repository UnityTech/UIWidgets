using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    // TODO: probably we don't need this cache.
    public class MeshKey : IEquatable<MeshKey> {
        public readonly string text;
        public readonly int fontId;
        public readonly int textureVersion;
        public readonly int fontSize;
        public readonly UnityEngine.FontStyle fontStyle;
        public readonly float pixelPerPoint;
        public readonly UnityEngine.Color color;

        public MeshKey(string text, int fontId, int textureVersion, int fontSize,
            UnityEngine.FontStyle fontStyle, float pixelPerPoint, UnityEngine.Color color) {
            this.text = text;
            this.fontId = fontId;
            this.textureVersion = textureVersion;
            this.fontSize = fontSize;
            this.fontStyle = fontStyle;
            this.pixelPerPoint = pixelPerPoint;
            this.color = color;
        }

        public bool Equals(MeshKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return string.Equals(this.text, other.text) && this.fontId == other.fontId &&
                   this.textureVersion == other.textureVersion && this.fontSize == other.fontSize &&
                   this.fontStyle == other.fontStyle && this.pixelPerPoint.Equals(other.pixelPerPoint) &&
                   this.color.Equals(other.color);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return this.Equals((MeshKey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.text != null ? this.text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.fontId;
                hashCode = (hashCode * 397) ^ this.textureVersion;
                hashCode = (hashCode * 397) ^ this.fontSize;
                hashCode = (hashCode * 397) ^ (int) this.fontStyle;
                hashCode = (hashCode * 397) ^ this.pixelPerPoint.GetHashCode();
                hashCode = (hashCode * 397) ^ this.color.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MeshKey left, MeshKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(MeshKey left, MeshKey right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return
                $"Text: {this.text}, FontId: {this.fontId}, TextureVersion: {this.textureVersion}, FontSize: {this.fontSize}, FontStyle: {this.fontStyle}, PixelPerPoint: {this.pixelPerPoint}, Color: {this.color}";
        }
    }

    public class MeshInfo {
        public readonly Mesh mesh;
        public readonly MeshKey key;
        public long _timeToLive;

        public MeshInfo(MeshKey key, Mesh mesh, int timeToLive = 5) {
            this.mesh = mesh;
            this.key = key;
            this.touch(timeToLive);
        }

        public long timeToLive {
            get { return this._timeToLive; }
        }

        public void touch(long timeTolive = 5) {
            this._timeToLive = timeTolive + MeshGenrator.frameCount;
        }
    }


    public static class MeshGenrator {
        static Dictionary<MeshKey, MeshInfo> _meshes = new Dictionary<MeshKey, MeshInfo>();

        static long _frameCount = 0;

        public static long frameCount {
            get { return _frameCount; }
        }

        public static int meshCount {
            get { return _meshes.Count; }
        }

        public static void tickNextFrame() {
            _frameCount++;
            var keysToRemove = _meshes.Values.Where(info => info.timeToLive < _frameCount)
                .Select(info => info.key).ToList();
            foreach (var key in keysToRemove) {
                _meshes.Remove(key);
            }
        }

        public static Mesh generateMesh(TextBlob textBlob, float[] xform, float devicePixelRatio) {
            var style = textBlob.style;
            var fontInfo = FontManager.instance.getOrCreate(style.fontFamily);
            var font = fontInfo.font;
            var length = textBlob.end - textBlob.start;

            var text = textBlob.text;
            var scale = XformUtils.getAverageScale(xform) * devicePixelRatio;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * scale);
            var subText = textBlob.text.Substring(textBlob.start, textBlob.end - textBlob.start);
            font.RequestCharactersInTexture(subText, fontSizeToLoad, style.UnityFontStyle);

            var vertices = new List<Vector3>(length * 4);
            var triangles = new List<int>(length * 6);
            var uv = new List<Vector2>(length * 4);
            Mesh mesh = new Mesh();

            for (int charIndex = 0; charIndex < length; ++charIndex) {
                var ch = text[charIndex + textBlob.start];
                // first char as origin for mesh position 
                var position = textBlob.positions[charIndex + textBlob.start];
                if (Paragraph.isWordSpace(ch) || Paragraph.isLineEndSpace(ch) || ch == '\t') {
                    continue;
                }


                CharacterInfo charInfo;
                font.GetCharacterInfo(ch, out charInfo, fontSizeToLoad, style.UnityFontStyle);
                var minX = charInfo.minX / scale;
                var maxX = charInfo.maxX / scale;
                var minY = charInfo.minY / scale;
                var maxY = charInfo.maxY / scale;

                var baseIndex = vertices.Count;

                float x, y;
                PathUtils.transformPoint(out x, out y, xform, (float) (position.x + minX), (float) (position.y - maxY));
                vertices.Add(new Vector3(x, y, 0));
                PathUtils.transformPoint(out x, out y, xform, (float) (position.x + maxX), (float) (position.y - maxY));
                vertices.Add(new Vector3(x, y, 0));
                PathUtils.transformPoint(out x, out y, xform, (float) (position.x + maxX), (float) (position.y - minY));
                vertices.Add(new Vector3(x, y, 0));
                PathUtils.transformPoint(out x, out y, xform, (float) (position.x + minX), (float) (position.y - minY));
                vertices.Add(new Vector3(x, y, 0));

                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);

                uv.Add(charInfo.uvTopLeft);
                uv.Add(charInfo.uvTopRight);
                uv.Add(charInfo.uvBottomRight);
                uv.Add(charInfo.uvBottomLeft);
            }

            mesh.SetVertices(vertices);
            mesh.SetIndices(triangles.ToArray(), MeshTopology.Triangles, 0);
            mesh.SetUVs(0, uv);
            return mesh;
        }
    }
}
