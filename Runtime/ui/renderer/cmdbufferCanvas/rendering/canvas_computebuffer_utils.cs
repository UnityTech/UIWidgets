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

        static ComputeBuffer _computeBuffer;
        static List<uiVertex> _vertices;

        static ComputeBuffer _indexBuffer;
        static List<int> _indices;
        
        static int _startVertex;
        static int _startIndex;

        static int _instanceNum;

        public const int COMPUTE_BUFFER_MAX_ITEM_NUM = 1024 * 1024;   // maxsize = 1M vertex/index

        static void tryReleaseComputeBuffer() {
            if (!CanvasShader.supportComputeBuffer) {
                return;
            }

            if (_computeBuffer == null) {
                return;
            }

            if (_instanceNum != 0) {
                return;
            }

            _computeBuffer.Dispose();
            _indexBuffer.Dispose();
            _vertices = null;
            _indices = null;
            _computeBuffer = null;
            _indexBuffer = null;
        }

        void initComputeBuffer() {
            var stride = Marshal.SizeOf(typeof(uiVertex));
            var strideIndex = Marshal.SizeOf(typeof(int));
            _computeBuffer = new ComputeBuffer(COMPUTE_BUFFER_MAX_ITEM_NUM, stride);
            _vertices = new List<uiVertex>();
                
            _indexBuffer = new ComputeBuffer(COMPUTE_BUFFER_MAX_ITEM_NUM, strideIndex);
            _indices = new List<int>();
        }

        void resetComputeBuffer() {
            if (!CanvasShader.supportComputeBuffer) return;

            if (_computeBuffer == null) {
                this.initComputeBuffer();
            }
            
            _vertices.Clear();
            _indices.Clear();
            _startVertex = 0;
            _startIndex = 0;
        }

        void bindComputeBuffer() {
            if (!CanvasShader.supportComputeBuffer) return;
            
            _computeBuffer.SetData(_vertices);
            _indexBuffer.SetData(_indices);
        }

        void addMeshToComputeBuffer(List<Vector3> vertex, List<Vector2> uv, List<int> triangles) {
            if (!CanvasShader.supportComputeBuffer) return;
            
            _startVertex = _vertices.Count;
            _startIndex = _indices.Count;

            var hasUv = uv != null;

            for (int i = 0; i < vertex.Count; i++) {
                _vertices.Add(new uiVertex {
                    position = new Vector2(vertex[i].x, vertex[i].y),
                    uv = hasUv ? uv[i] : Vector2.zero
                });
            }

            foreach (var triangleId in triangles) {
                _indices.Add(triangleId + _startVertex);
            }
        }
    }
}