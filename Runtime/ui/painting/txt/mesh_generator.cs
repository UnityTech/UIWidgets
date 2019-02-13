using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class MeshKey : IEquatable<MeshKey> {
        public readonly long textBlobId;
        public readonly float scale;

        public MeshKey(long textBlobId, float scale) {
            this.textBlobId = textBlobId;
            this.scale = scale;
        }
        
        public bool Equals(MeshKey other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.textBlobId == other.textBlobId && this.scale.Equals(other.scale);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((MeshKey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.textBlobId.GetHashCode() * 397) ^ this.scale.GetHashCode();
            }
        }

        public static bool operator ==(MeshKey left, MeshKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(MeshKey left, MeshKey right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{nameof(this.textBlobId)}: {this.textBlobId}, {nameof(this.scale)}: {this.scale}";
        }
    }

    class MeshInfo {
        public readonly MeshKey key;
        public readonly long textureVersion;
        public readonly MeshMesh mesh;
        long _timeToLive;

        public MeshInfo(MeshKey key, MeshMesh mesh, long textureVersion, int timeToLive = 5) {
            this.mesh = mesh;
            this.key = key;
            this.textureVersion = textureVersion;
            this.touch(timeToLive);
        }

        public long timeToLive {
            get { return this._timeToLive; }
        }

        public void touch(long timeTolive = 5) {
            this._timeToLive = timeTolive + MeshGenerator.frameCount;
        }
    }


    static class MeshGenerator {
        static readonly Dictionary<MeshKey, MeshInfo> _meshes = new Dictionary<MeshKey, MeshInfo>();

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

        public static MeshMesh generateMesh(TextBlob textBlob, float scale) {
            var style = textBlob.style;
            var fontInfo = FontManager.instance.getOrCreate(style.fontFamily);
            var key = new MeshKey(textBlob.instanceId, scale);
            
            _meshes.TryGetValue(key, out var meshInfo);
            if (meshInfo != null && meshInfo.textureVersion == fontInfo.textureVersion) {
                meshInfo.touch();
                return meshInfo.mesh;
            }

            var font = fontInfo.font;
            var length = textBlob.textSize;
            var text = textBlob.text;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * scale);
            var subText = textBlob.text.Substring(textBlob.textOffset, textBlob.textSize);
            font.RequestCharactersInTexture(subText, fontSizeToLoad, style.UnityFontStyle);


            var vertices = new List<Vector3>(length * 4);
            var triangles = new List<int>(length * 6);
            var uv = new List<Vector2>(length * 4);
            for (int charIndex = 0; charIndex < length; ++charIndex) {
                var ch = text[charIndex + textBlob.textOffset];
                // first char as origin for mesh position 
                var position = textBlob.positions[charIndex];
                if (LayoutUtils.isWordSpace(ch) || LayoutUtils.isLineEndSpace(ch) || ch == '\t') {
                    continue;
                }

                CharacterInfo charInfo;
                font.GetCharacterInfo(ch, out charInfo, fontSizeToLoad, style.UnityFontStyle);
            
                var minX = charInfo.minX / scale;
                var maxX = charInfo.maxX / scale;
                var minY = charInfo.minY / scale;
                var maxY = charInfo.maxY / scale;

                var baseIndex = vertices.Count;

                vertices.Add(new Vector3((float) (position.x + minX), (float) (position.y - maxY), 0));
                vertices.Add(new Vector3((float) (position.x + maxX), (float) (position.y - maxY), 0));
                vertices.Add(new Vector3((float) (position.x + maxX), (float) (position.y - minY), 0));
                vertices.Add(new Vector3((float) (position.x + minX), (float) (position.y - minY), 0));

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

            if (vertices.Count == 0) {
                return null;
            }
            
            MeshMesh mesh = vertices.Count > 0 ? new MeshMesh(null, vertices, triangles, uv) : null;
            _meshes[key] = new MeshInfo(key, mesh, fontInfo.textureVersion);

            return mesh;
        }
    }
}
