using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        
        struct TVertex
        {
            public Vector2 position;
            public Vector2 uv;
        }

        ComputeBuffer computeBuffer;
        List<TVertex> tvertexes;

        ComputeBuffer indexBuffer;
        List<int> indexes;
        
        int startVertex;
        int startIndex;

        Material material;

        bool supportComputeBuffer;

        void setupComputeBuffer() {
            this.supportComputeBuffer = this._isMainCanvas && CanvasShader.supportComputeBuffer;
        }

        void initComputeBuffer() {
            var stride = Marshal.SizeOf(typeof(TVertex));
            this.computeBuffer = new ComputeBuffer(1024 * 1024, stride);
            this.tvertexes = new List<TVertex>();
                
            this.indexBuffer = new ComputeBuffer(1024 * 1024, Marshal.SizeOf(typeof(int)));
            this.indexes = new List<int>();
        }

        void resetComputeBuffer() {
            if (!this.supportComputeBuffer) return;

            if (this.computeBuffer == null) {
                this.initComputeBuffer();
            }
            
            this.tvertexes.Clear();
            this.indexes.Clear();
            this.startVertex = 0;
            this.startIndex = 0;
        }

        void bindComputeBuffer() {
            if (!this.supportComputeBuffer) return;
            
            this.computeBuffer.SetData(this.tvertexes);
            this.indexBuffer.SetData(this.indexes);
        }

        void addMeshToComputeBuffer(List<Vector3> vertex, List<Vector2> uv, List<int> triangles) {
            if (!this.supportComputeBuffer) return;
            
            this.startVertex = this.tvertexes.Count;
            this.startIndex = this.indexes.Count;

            var hasUv = uv != null;

            for (int i = 0; i < vertex.Count; i++) {
                this.tvertexes.Add(new TVertex {
                    position = new Vector2(vertex[i].x, vertex[i].y),
                    uv = hasUv ? uv[i] : Vector2.zero
                });
            }

            foreach (var triangleId in triangles) {
                this.indexes.Add(triangleId + this.startVertex);
            }
        }
        
        public void DrawBuffer(CommandBuffer cmdBuf)
        {
            if (this.computeBuffer == null)
            {
                this.initComputeBuffer();
            }
            
            this.resetComputeBuffer();

            if (this.material == null) {
                this.material = new Material(Shader.Find("UIWidgets/canvas_convexFill_cb"));
                this.material.SetVector("_viewport", new Vector4(0, 0, 500, 500));
            }

            var random = new Random();
            var num = 5;
            var size = 30;
            
            for (var i = 0; i < num; i++)
            {
                for (var j = 0; j < num; j++)
                {
                        var offsetY = i * size;
                        var offsetX = j * size;
                        var centerX = offsetX + size / 2;
                        var centerY = offsetY + size / 2;
                        var width = size;
                        var height = size;

                    var vert = new List<Vector3> {
                        new Vector3(centerX - width / 2, centerY - height / 2),
                        new Vector3(centerX + width / 2, centerY - height / 2),
                        new Vector3(centerX + width / 2, centerY + height / 2),
                        new Vector3(centerX - width / 2, centerY + height / 2)
                    };

                    var index = new List<int> {
                        0, 1, 2, 0, 2, 3
                    };

                        this.addMeshToComputeBuffer(vert, null, index);

                        var mpb = new MaterialPropertyBlock();
                        mpb.SetBuffer("databuffer", this.computeBuffer);
                        mpb.SetBuffer("indexbuffer", this.indexBuffer);
                        mpb.SetInt("_startVertex", this.startIndex);
                        cmdBuf.DrawProcedural(Matrix4x4.identity, this.material, 0, MeshTopology.Triangles, 6, 1, mpb);
                }
            }
            
            this.bindComputeBuffer();
        }
    }
}