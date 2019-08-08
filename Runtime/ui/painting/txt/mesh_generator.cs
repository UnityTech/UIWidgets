using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class MeshKey : PoolObject, IEquatable<MeshKey> {
        public long textBlobId;
        public float scale;

        public MeshKey() {
        }

        public static MeshKey create(long textBlobId, float scale) {
            var newKey = ObjectPool<MeshKey>.alloc();
            newKey.textBlobId = textBlobId;
            newKey.scale = scale;
            return newKey;
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

    class MeshInfo : PoolObject {
        public MeshKey key;
        public long textureVersion;
        public uiMeshMesh mesh;
        long _timeToLive;

        public MeshInfo() {
        }

        public static MeshInfo create(MeshKey key, uiMeshMesh mesh, long textureVersion, int timeToLive = 5) {
            var meshInfo = ObjectPool<MeshInfo>.alloc();
            meshInfo.mesh = mesh;
            meshInfo.key = key;
            meshInfo.textureVersion = textureVersion;
            meshInfo.touch(timeToLive);
            return meshInfo;
        }

        public override void clear() {
            ObjectPool<MeshKey>.release(this.key);
            ObjectPool<uiMeshMesh>.release(this.mesh);
        }

        public long timeToLive {
            get { return this._timeToLive; }
        }

        public void touch(long timeTolive = 5) {
            this._timeToLive = timeTolive + TextBlobMesh.frameCount;
        }
    }


    class TextBlobMesh : PoolObject {
        static readonly Dictionary<MeshKey, MeshInfo> _meshes = new Dictionary<MeshKey, MeshInfo>();
        static long _frameCount = 0;

        public TextBlob? textBlob;
        public float scale;
        public uiMatrix3 matrix;

        uiMeshMesh _mesh;
        bool _resolved;

        public TextBlobMesh() {
        }

        public override void clear() {
            ObjectPool<uiMeshMesh>.release(this._mesh);
            this._mesh = null;
            this._resolved = false;
            this.textBlob = null;
        }

        public static TextBlobMesh create(TextBlob textBlob, float scale, uiMatrix3 matrix) {
            TextBlobMesh newMesh = ObjectPool<TextBlobMesh>.alloc();
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

        static List<MeshKey> _keysToRemove = new List<MeshKey>();

        public static void tickNextFrame() {
            _frameCount++;
            D.assert(_keysToRemove.Count == 0);
            foreach (var key in _meshes.Keys) {
                if (_meshes[key].timeToLive < _frameCount) {
                    _keysToRemove.Add(key);
                }
            }

            foreach (var key in _keysToRemove) {
                ObjectPool<MeshInfo>.release(_meshes[key]);
                _meshes.Remove(key);
            }

            _keysToRemove.Clear();
        }

        public uiMeshMesh resolveMesh() {
            if (this._resolved) {
                return this._mesh;
            }

            this._resolved = true;

            var style = this.textBlob.Value.style;

            var text = this.textBlob.Value.text;
            var key = MeshKey.create(this.textBlob.Value.instanceId, this.scale);
            var fontInfo = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle);
            var font = fontInfo.font;

            _meshes.TryGetValue(key, out var meshInfo);
            if (meshInfo != null && meshInfo.textureVersion == fontInfo.textureVersion) {
                ObjectPool<MeshKey>.release(key);
                meshInfo.touch();
                this._mesh = meshInfo.mesh.transform(this.matrix);
                return this._mesh;
            }

            // Handling Emoji
            char startingChar = text[this.textBlob.Value.textOffset];
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                var vert = ObjectPool<uiList<Vector3>>.alloc();
                var tri = ObjectPool<uiList<int>>.alloc();
                var uvCoord = ObjectPool<uiList<Vector2>>.alloc();

                var metrics = FontMetrics.fromFont(font, style.UnityFontSize);
                var minMaxRect = EmojiUtils.getMinMaxRect(style.fontSize, metrics.ascent, metrics.descent);
                var minX = minMaxRect.left;
                var maxX = minMaxRect.right;
                var minY = minMaxRect.top;
                var maxY = minMaxRect.bottom;

                for (int i = 0; i < this.textBlob.Value.textSize; i++) {
                    char a = text[this.textBlob.Value.textOffset + i];
                    int code = a;
                    if (char.IsHighSurrogate(a)) {
                        D.assert(i + 1 < this.textBlob.Value.textSize);
                        D.assert(this.textBlob.Value.textOffset + i + 1 < this.textBlob.Value.text.Length);
                        char b = text[this.textBlob.Value.textOffset + i + 1];
                        D.assert(char.IsLowSurrogate(b));
                        code = char.ConvertToUtf32(a, b);
                    }
                    else if (char.IsLowSurrogate(a) || EmojiUtils.isEmptyEmoji(a)) {
                        continue;
                    }

                    var uvRect = EmojiUtils.getUVRect(code);

                    var positionX = this.textBlob.Value.getPositionX(i);

                    int baseIndex = vert.Count;
                    vert.Add(new Vector3(positionX + minX, minY, 0));
                    vert.Add(new Vector3(positionX + maxX, minY, 0));
                    vert.Add(new Vector3(positionX + maxX, maxY, 0));
                    vert.Add(new Vector3(positionX + minX, maxY, 0));
                    tri.Add(baseIndex);
                    tri.Add(baseIndex + 1);
                    tri.Add(baseIndex + 2);
                    tri.Add(baseIndex);
                    tri.Add(baseIndex + 2);
                    tri.Add(baseIndex + 3);
                    uvCoord.Add(uvRect.bottomLeft.toVector());
                    uvCoord.Add(uvRect.bottomRight.toVector());
                    uvCoord.Add(uvRect.topRight.toVector());
                    uvCoord.Add(uvRect.topLeft.toVector());

                    if (char.IsHighSurrogate(a)) {
                        i++;
                    }
                }

                uiMeshMesh meshMesh = uiMeshMesh.create(null, vert, tri, uvCoord);

                if (_meshes.ContainsKey(key)) {
                    ObjectPool<MeshInfo>.release(_meshes[key]);
                    _meshes.Remove(key);
                }

                _meshes[key] = MeshInfo.create(key, meshMesh, 0);

                this._mesh = meshMesh.transform(this.matrix);
                return this._mesh;
            }

            var length = this.textBlob.Value.textSize;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * this.scale);

            var vertices = ObjectPool<uiList<Vector3>>.alloc();
            vertices.SetCapacity(length * 4);

            var triangles = ObjectPool<uiList<int>>.alloc();
            triangles.SetCapacity(length * 6);

            var uv = ObjectPool<uiList<Vector2>>.alloc();
            uv.SetCapacity(length * 4);

            for (int charIndex = 0; charIndex < length; ++charIndex) {
                var ch = text[charIndex + this.textBlob.Value.textOffset];
                // first char as origin for mesh position 
                var positionX = this.textBlob.Value.getPositionX(charIndex);
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

                vertices.Add(new Vector3(positionX + minX, minY, 0));
                vertices.Add(new Vector3(positionX + maxX, minY, 0));
                vertices.Add(new Vector3(positionX + maxX, maxY, 0));
                vertices.Add(new Vector3(positionX + minX, maxY, 0));

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
                ObjectPool<uiList<Vector3>>.release(vertices);
                ObjectPool<uiList<Vector2>>.release(uv);
                ObjectPool<uiList<int>>.release(triangles);
                ObjectPool<MeshKey>.release(key);
                return null;
            }

            uiMeshMesh mesh = vertices.Count > 0 ? uiMeshMesh.create(null, vertices, triangles, uv) : null;

            if (_meshes.ContainsKey(key)) {
                ObjectPool<MeshInfo>.release(_meshes[key]);
                _meshes.Remove(key);
            }

            _meshes[key] = MeshInfo.create(key, mesh, fontInfo.textureVersion);

            this._mesh = mesh.transform(this.matrix);
            return this._mesh;
        }
    }
}