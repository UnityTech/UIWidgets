using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        
        struct uiVertex
        {
            public Vector2 position;
            public Vector2 uv;
        }

        ComputeBuffer _computeBuffer;
        List<uiVertex> _vertices;

        ComputeBuffer _indexBuffer;
        List<int> _indices;
        
        int _startVertex;
        int _startIndex;

        public const int COMPUTE_BUFFER_MAX_ITEM_NUM = 1024 * 1024;   // maxsize = 1M vertex/index

        bool supportComputeBuffer {
            get { return CanvasShader.supportComputeBuffer; }
        }

        void _releaseComputeBuffer() {
            if (!this.supportComputeBuffer) {
                return;
            }

            if (this._computeBuffer == null) {
                return;
            }

            this._computeBuffer.Dispose();
            this._indexBuffer.Dispose();
            this._vertices = null;
            this._indices = null;
            this._computeBuffer = null;
            this._indexBuffer = null;
        }

        void _initComputeBuffer() {
            var stride = Marshal.SizeOf(typeof(uiVertex));
            var strideIndex = Marshal.SizeOf(typeof(int));
            this._computeBuffer = new ComputeBuffer(COMPUTE_BUFFER_MAX_ITEM_NUM, stride);
            this._vertices = new List<uiVertex>();
                
            this._indexBuffer = new ComputeBuffer(COMPUTE_BUFFER_MAX_ITEM_NUM, strideIndex);
            this._indices = new List<int>();
        }

        void _resetComputeBuffer() {
            if (!this.supportComputeBuffer) return;

            if (this._computeBuffer == null) {
                this._initComputeBuffer();
            }
            
            this._vertices.Clear();
            this._indices.Clear();
            this._startVertex = 0;
            this._startIndex = 0;
        }

        void _bindComputeBuffer() {
            if (!this.supportComputeBuffer) return;
            
            this._computeBuffer.SetData(this._vertices);
            this._indexBuffer.SetData(this._indices);
        }

        void _addMeshToComputeBuffer(List<Vector3> vertex, List<Vector2> uv, List<int> triangles) {
            if (!this.supportComputeBuffer) return;
            
            this._startVertex = this._vertices.Count;
            this._startIndex = this._indices.Count;

            var hasUv = uv != null;

            for (int i = 0; i < vertex.Count; i++) {
                this._vertices.Add(new uiVertex {
                    position = new Vector2(vertex[i].x, vertex[i].y),
                    uv = hasUv ? uv[i] : Vector2.zero
                });
            }

            foreach (var triangleId in triangles) {
                this._indices.Add(triangleId + this._startVertex);
            }
        }
    }
}