using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.ui.txt;
using UnityEditor;
using UnityEngine;

namespace UIWidgets.ui.painting.txt
{
    public class MeshKey : IEquatable<MeshKey>
    {
        public readonly string text;
        public readonly int fontId;
        public readonly int textureVersion;
        public readonly int fontSize;
        public readonly UnityEngine.FontStyle fontStyle;
        public readonly float pixelPerPoint;
        public readonly UnityEngine.Color color;

        public MeshKey(string text, int fontId, int textureVersion, int fontSize,
            UnityEngine.FontStyle fontStyle, float pixelPerPoint, UnityEngine.Color color)
        {
            this.text = text;
            this.fontId = fontId;
            this.textureVersion = textureVersion;
            this.fontSize = fontSize;
            this.fontStyle = fontStyle;
            this.pixelPerPoint = pixelPerPoint;
            this.color = color;
        }

        public bool Equals(MeshKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(text, other.text) && fontId == other.fontId &&
                   textureVersion == other.textureVersion && fontSize == other.fontSize &&
                   fontStyle == other.fontStyle && pixelPerPoint.Equals(other.pixelPerPoint) &&
                   color.Equals(other.color);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MeshKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (text != null ? text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ fontId;
                hashCode = (hashCode * 397) ^ textureVersion;
                hashCode = (hashCode * 397) ^ fontSize;
                hashCode = (hashCode * 397) ^ (int) fontStyle;
                hashCode = (hashCode * 397) ^ pixelPerPoint.GetHashCode();
                hashCode = (hashCode * 397) ^ color.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MeshKey left, MeshKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MeshKey left, MeshKey right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format(
                "Text: {0}, FontId: {1}, TextureVersion: {2}, FontSize: {3}, FontStyle: {4}, PixelPerPoint: {5}, Color: {6}",
                text, fontId, textureVersion, fontSize, fontStyle, pixelPerPoint, color);
        }
    }

    public class MeshInfo
    {
        public readonly Mesh mesh;
        public readonly MeshKey key;
        public long _timeToLive;

        public MeshInfo(MeshKey key, Mesh mesh, int timeToLive = 5)
        {
            this.mesh = mesh;
            this.key = key;
            this.touch(timeToLive);
        }

        public long timeToLive
        {
            get { return _timeToLive; }
        }

        public void touch(long timeTolive = 5)
        {
            this._timeToLive = timeTolive + MeshGenrator.frameCount;
        }
    }
    

    public static class MeshGenrator
    {
        private static Dictionary<MeshKey, MeshInfo> _meshes = new Dictionary<MeshKey, MeshInfo>();

        private static long _frameCount = 0;

        public static long frameCount
        {
            get { return _frameCount; }
        }
        
        public static long MeshCount
        {
            get { return _meshes.Count; }
        }

        public static void tickNextFrame()
        {
            _frameCount++;
            var keysToRemove = _meshes.Values.Where(info => info.timeToLive < _frameCount).Select(info => info.key).ToList();
            foreach (var key in keysToRemove)
            {
                _meshes.Remove(key);
            }
        }
        
        public static Mesh generateMesh(TextBlob textBlob)
        {
            var style = textBlob.style;
            var fontInfo = FontManager.instance.getOrCreate(style.fontFamily);
            var font = fontInfo.font;
            var length = textBlob.end - textBlob.start;

            var text = textBlob.text;
            var scale = EditorGUIUtility.pixelsPerPoint;
            var fontSizeToLoad = (int) scale * style.UnityFontSize;
            var subText = textBlob.text.Substring(textBlob.start, textBlob.end - textBlob.start);
            font.RequestCharactersInTexture(subText,
                fontSizeToLoad, style.UnityFontStyle);

            MeshInfo meshInfo;
            var key = new MeshKey(subText, font.GetInstanceID(),
                fontInfo.textureVersion,
                style.UnityFontSize, style.UnityFontStyle, scale, style.UnityColor);

            Mesh mesh = null;
            _meshes.TryGetValue(key, out meshInfo);
            if (meshInfo != null)
            {
                mesh = meshInfo.mesh;
                meshInfo.touch();
            }
            if (mesh != null)
            {
                return mesh;
            }

            var vertices = new Vector3[length * 4];
            var triangles = new int[length * 6];
            var uv = new Vector2[length * 4];
            mesh = new Mesh();
            _meshes[key] = new MeshInfo(key, mesh);
            
            for (int charIndex = 0; charIndex < length; ++charIndex)
            {
                var ch = text[charIndex + textBlob.start];
                // first char as origin for mesh position 
                var position = textBlob.positions[charIndex + textBlob.start] - textBlob.positions[textBlob.start];
                if (Paragraph.isWordSpace(ch) || Paragraph.isLineEndSpace(ch) || ch == '\t')
                {
                    vertices[4 * charIndex + 0] = vertices[4 * charIndex + 1] =
                        vertices[4 * charIndex + 2] = vertices[4 * charIndex + 3] = Vector3.zero;
                    uv[4 * charIndex + 0] = Vector2.zero;
                    uv[4 * charIndex + 1] = Vector2.zero;
                    uv[4 * charIndex + 2] = Vector2.zero;
                    uv[4 * charIndex + 3] = Vector2.zero;
                }
                else
                {
                    CharacterInfo charInfo;
                    font.GetCharacterInfo(ch, out charInfo, fontSizeToLoad, style.UnityFontStyle);
                    var minX = charInfo.minX / scale;
                    var maxX = charInfo.maxX / scale;
                    var minY = charInfo.minY / scale;
                    var maxY = charInfo.maxY / scale;
                    vertices[4 * charIndex + 0] = new Vector3((float) (position.x + minX),
                        (float) (position.y - maxY), 0);
                    vertices[4 * charIndex + 1] = new Vector3((float) (position.x + maxX),
                        (float) (position.y - maxY), 0);
                    vertices[4 * charIndex + 2] = new Vector3(
                        (float) (position.x + maxX), (float) (position.y - minY), 0);
                    vertices[4 * charIndex + 3] = new Vector3(
                        (float) (position.x + minX), (float) (position.y - minY), 0);


                    uv[4 * charIndex + 0] = charInfo.uvTopLeft;
                    uv[4 * charIndex + 1] = charInfo.uvTopRight;
                    uv[4 * charIndex + 2] = charInfo.uvBottomRight;
                    uv[4 * charIndex + 3] = charInfo.uvBottomLeft;
                }

                triangles[6 * charIndex + 0] = 4 * charIndex + 0;
                triangles[6 * charIndex + 1] = 4 * charIndex + 1;
                triangles[6 * charIndex + 2] = 4 * charIndex + 2;

                triangles[6 * charIndex + 3] = 4 * charIndex + 0;
                triangles[6 * charIndex + 4] = 4 * charIndex + 2;
                triangles[6 * charIndex + 5] = 4 * charIndex + 3;
            }

            var colors = new UnityEngine.Color[vertices.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = style.UnityColor;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.uv = uv;
            return mesh;
        }
    }
}