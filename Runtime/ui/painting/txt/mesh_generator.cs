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
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.textBlobId == other.textBlobId && this.scale.Equals(other.scale);
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
            this._timeToLive = timeTolive + TextBlobMesh.frameCount;
        }
    }

    
    class TextBlobMesh {
        
        static readonly Dictionary<MeshKey, MeshInfo> _meshes = new Dictionary<MeshKey, MeshInfo>();

        static long _frameCount = 0;
        readonly TextBlob _textBlob;
        readonly float _scale;
        readonly Matrix3 _transform;
        MeshMesh _mesh;
        bool _resolved;

        public TextBlobMesh(TextBlob textBlob, float scale, Matrix3 transform) {
            this._textBlob = textBlob;
            this._scale = scale;
            this._transform = transform;
        }
        
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

        public MeshMesh resovleMesh() {
            if (this._resolved) {
                return this._mesh;
            }

            this._resolved = true;
            
            var style = this._textBlob.style;
            var fontInfo = FontManager.instance.getOrCreate(style.fontFamily);
            var key = new MeshKey(this._textBlob.instanceId, this._scale);

            _meshes.TryGetValue(key, out var meshInfo);
            if (meshInfo != null && meshInfo.textureVersion == fontInfo.textureVersion) {
                meshInfo.touch();
                this._mesh = meshInfo.mesh.transform(this._transform);
                return this._mesh;
            }

            var font = fontInfo.font;
            var length = this._textBlob.textSize;
            var text = this._textBlob.text;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * this._scale);

            var vertices = new List<Vector3>(length * 4);
            var triangles = new List<int>(length * 6);
            var uv = new List<Vector2>(length * 4);
            for (int charIndex = 0; charIndex < length; ++charIndex) {
                var ch = text[charIndex + this._textBlob.textOffset];
                // first char as origin for mesh position 
                var position = this._textBlob.positions[charIndex];
                if (LayoutUtils.isWordSpace(ch) || LayoutUtils.isLineEndSpace(ch) || ch == '\t') {
                    continue;
                }

                CharacterInfo charInfo;
                font.GetCharacterInfo(ch, out charInfo, fontSizeToLoad, style.UnityFontStyle);

                var minX = charInfo.minX / this._scale;
                var maxX = charInfo.maxX / this._scale;
                var minY = charInfo.minY / this._scale;
                var maxY = charInfo.maxY / this._scale;

                var baseIndex = vertices.Count;

                vertices.Add(new Vector3((position.x + minX), (position.y - maxY), 0));
                vertices.Add(new Vector3((position.x + maxX), (position.y - maxY), 0));
                vertices.Add(new Vector3((position.x + maxX), (position.y - minY), 0));
                vertices.Add(new Vector3((position.x + minX), (position.y - minY), 0));

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

            this._mesh = mesh.transform(this._transform);
            return this._mesh;
        }
    }
}