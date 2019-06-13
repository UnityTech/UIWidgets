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
        public readonly uiMeshMesh mesh;
        long _timeToLive;

        public MeshInfo(MeshKey key, uiMeshMesh mesh, long textureVersion, int timeToLive = 5) {
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

    
    class TextBlobMesh : PoolItem {
        
        static readonly Dictionary<MeshKey, MeshInfo> _meshes = new Dictionary<MeshKey, MeshInfo>();
        static long _frameCount = 0;
        
        public TextBlob textBlob;
        public float scale;
        public uiMatrix3 matrix;
        
        uiMeshMesh _mesh;
        bool _resolved;

        public TextBlobMesh() {
        }

        public override void clear() {
            this.textBlob = null;
            this.matrix = null;
            this._mesh?.dispose();
            this._mesh = null;
            this._resolved = false;
        }

        public static TextBlobMesh create(TextBlob textBlob, float scale, uiMatrix3 matrix) {
            TextBlobMesh newMesh = ItemPoolManager.alloc<TextBlobMesh>();
            newMesh.textBlob = textBlob;
            newMesh.scale = scale;
            newMesh.matrix = matrix;
            return newMesh;
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
                _meshes[key].mesh.dispose();
                _meshes.Remove(key);
            }
        }

        public uiMeshMesh resovleMesh() {
            if (this._resolved) {
                return this._mesh;
            }

            this._resolved = true;
            
            var style = this.textBlob.style;
            var fontInfo = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle);
            var key = new MeshKey(this.textBlob.instanceId, this.scale);

            _meshes.TryGetValue(key, out var meshInfo);
            if (meshInfo != null && meshInfo.textureVersion == fontInfo.textureVersion) {
                meshInfo.touch();
                this._mesh = meshInfo.mesh.transform(this.matrix);
                return this._mesh;
            }

            var font = fontInfo.font;
            var length = this.textBlob.textSize;
            var text = this.textBlob.text;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * this.scale);

            var vertices = ItemPoolManager.alloc<uiList<Vector3>>();
            vertices.SetCapacity(length * 4);
            
            var triangles = ItemPoolManager.alloc<uiList<int>>();
            triangles.SetCapacity(length * 6);

            var uv = ItemPoolManager.alloc<uiList<Vector2>>();
            uv.SetCapacity(length * 4);
            
            for (int charIndex = 0; charIndex < length; ++charIndex) {
                var ch = text[charIndex + this.textBlob.textOffset];
                // first char as origin for mesh position 
                var position = this.textBlob.positions[charIndex];
                if (LayoutUtils.isWordSpace(ch) || LayoutUtils.isLineEndSpace(ch) || ch == '\t') {
                    continue;
                }

                if (fontSizeToLoad == 0) {
                    continue;
                }
                
                font.getGlyphInfo(ch, out var glyphInfo, fontSizeToLoad, style.UnityFontStyle);
                
                var minX = glyphInfo.minX / this.scale;
                var maxX = glyphInfo.maxX / this.scale;
                var minY = -glyphInfo.maxY / this.scale;
                var maxY = -glyphInfo.minY / this.scale;

                var baseIndex = vertices.Count;

                vertices.Add(new Vector3((position.x + minX), (position.y + minY), 0));
                vertices.Add(new Vector3((position.x + maxX), (position.y + minY), 0));
                vertices.Add(new Vector3((position.x + maxX), (position.y + maxY), 0));
                vertices.Add(new Vector3((position.x + minX), (position.y + maxY), 0));

                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);

                uv.Add(glyphInfo.uvTopLeft);
                uv.Add(glyphInfo.uvTopRight);
                uv.Add(glyphInfo.uvBottomRight);
                uv.Add(glyphInfo.uvBottomLeft);
            }

            if (vertices.Count == 0) {
                this._mesh = null;
                vertices.dispose();
                uv.dispose();
                triangles.dispose();
                
                return null;
            }

            uiMeshMesh mesh = vertices.Count > 0 ? uiMeshMesh.create(null, vertices, triangles, uv) : null;
            //_meshes[key] = new MeshInfo(key, mesh, fontInfo.textureVersion);

            this._mesh = mesh.transform(this.matrix);
            mesh.dispose();
            return this._mesh;
        }
    }
}