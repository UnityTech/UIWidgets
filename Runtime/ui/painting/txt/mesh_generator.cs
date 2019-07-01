using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
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
        
        public readonly TextBlob textBlob;
        public readonly float scale;
        public readonly Matrix3 matrix;
        
        MeshMesh _mesh;
        bool _resolved;

        public TextBlobMesh(TextBlob textBlob, float scale, Matrix3 matrix) {
            this.textBlob = textBlob;
            this.scale = scale;
            this.matrix = matrix;
        }
        
        public static long frameCount {
            get { return _frameCount; }
        }

        public static int meshCount {
            get { return _meshes.Count; }
        }
        
        static readonly List<MeshKey> _keysToRemove = new List<MeshKey>();

        public static void tickNextFrame() {
            _frameCount++;
            _keysToRemove.Clear();
            foreach (var info in _meshes.Values) {
                if (info.timeToLive < _frameCount) {
                    _keysToRemove.Add(info.key);
                }
            }

            foreach (var key in _keysToRemove) {
                _meshes.Remove(key);
            }
        }

        public MeshMesh resolveMesh() {
            if (this._resolved) {
                return this._mesh;
            }

            this._resolved = true;
            
            var style = this.textBlob.style;

            var text = this.textBlob.text;
            var key = new MeshKey(this.textBlob.instanceId, this.scale);
            var fontInfo = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle);
            var font = fontInfo.font;
            
            // Handling Emoji
            char startingChar = text[this.textBlob.textOffset];
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                var vert = new List<Vector3>();
                var tri = new List<int>();
                var uvCoord = new List<Vector2>();
                var metrics = FontMetrics.fromFont(font, style.UnityFontSize);
                var minMaxRect = EmojiUtils.getMinMaxRect(style.fontSize, metrics.ascent, metrics.descent);
                var minX = minMaxRect.left;
                var maxX = minMaxRect.right;
                var minY = minMaxRect.top;
                var maxY = minMaxRect.bottom;

                for (int i = 0; i < this.textBlob.textSize; i++) {
                    char a = text[this.textBlob.textOffset + i];
                    int code = a;
                    if (char.IsHighSurrogate(a)) {
                        D.assert(i+1 < this.textBlob.textSize);
                        D.assert(this.textBlob.textOffset+i+1 < this.textBlob.text.Length);
                        char b = text[this.textBlob.textOffset+i+1];
                        D.assert(char.IsLowSurrogate(b));
                        code = char.ConvertToUtf32(a, b);
                    } else if (char.IsLowSurrogate(a) || EmojiUtils.isEmptyEmoji(a)) {
                        continue;
                    }
                    var uvRect = EmojiUtils.getUVRect(code);

                    var pos = this.textBlob.getPosition(i);

                    int baseIndex = vert.Count;
                    vert.Add(new Vector3(pos.x + minX, pos.y + minY, 0));
                    vert.Add(new Vector3(pos.x + maxX, pos.y + minY, 0));
                    vert.Add(new Vector3(pos.x + maxX, pos.y + maxY, 0));
                    vert.Add(new Vector3(pos.x + minX, pos.y + maxY, 0));
                    
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
                    
                    if(char.IsHighSurrogate(a)) i++;
                }
                MeshMesh meshMesh = new MeshMesh(null, vert, tri, uvCoord);
                _meshes[key] = new MeshInfo(key, meshMesh, 0);

                this._mesh = meshMesh.transform(this.matrix);
                return this._mesh;
            }
            

            _meshes.TryGetValue(key, out var meshInfo);
            if (meshInfo != null && meshInfo.textureVersion == fontInfo.textureVersion) {
                meshInfo.touch();
                this._mesh = meshInfo.mesh.transform(this.matrix);
                return this._mesh;
            }

            var length = this.textBlob.textSize;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * this.scale);

            var vertices = new List<Vector3>(length * 4);
            var triangles = new List<int>(length * 6);
            var uv = new List<Vector2>(length * 4);
            for (int charIndex = 0; charIndex < length; ++charIndex) {
                var ch = text[charIndex + this.textBlob.textOffset];
                // first char as origin for mesh position 
                var position = this.textBlob.getPosition(charIndex);
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
                return null;
            }

            MeshMesh mesh = vertices.Count > 0 ? new MeshMesh(null, vertices, triangles, uv) : null;
            _meshes[key] = new MeshInfo(key, mesh, fontInfo.textureVersion);

            this._mesh = mesh.transform(this.matrix);
            return this._mesh;
        }
    }
}