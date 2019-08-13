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
        
        public void DrawBuffer(CommandBuffer cmdBuf)
        {
            if (this.computeBuffer == null)
            {
                var stride = Marshal.SizeOf(typeof(TVertex));
                this.computeBuffer = new ComputeBuffer(1024 * 1024, stride);
                this.tvertexes = new List<TVertex>();
                
                this.indexBuffer = new ComputeBuffer(1024 * 1024, Marshal.SizeOf(typeof(int)));
                this.indexes = new List<int>();
            }
            
            this.tvertexes.Clear();
            this.indexes.Clear();
            this.startVertex = 0;
            this.startIndex = 0;

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

                        this.startVertex = this.tvertexes.Count;
                        this.startIndex = this.indexes.Count;
                        
                    
                        this.tvertexes.AddRange(new[]
                        {
                            new TVertex
                            {
                                position = new Vector2(centerX - width / 2, centerY - height / 2),
                                uv = new Vector2(0, 0)
                            },
                            new TVertex
                            {
                                position = new Vector2(centerX + width / 2, centerY - height / 2),
                                uv = new Vector2(0, 0)
                            },
                            new TVertex
                            {
                                position = new Vector2(centerX + width / 2, centerY + height / 2),
                                uv = new Vector2(0, 0)
                            },
                            new TVertex
                            {
                                position = new Vector2(centerX - width / 2, centerY + height / 2),
                                uv = new Vector2(0, 0)
                            }
                        });
                    
                        this.indexes.AddRange(new []
                        {
                            this.startVertex, this.startVertex + 1, this.startVertex + 2, this.startVertex, this.startVertex + 2, this.startVertex + 3
                        });

                        var mpb = new MaterialPropertyBlock();
                        mpb.SetBuffer("databuffer", this.computeBuffer);
                        mpb.SetBuffer("indexbuffer", this.indexBuffer);
                        mpb.SetInt("_startVertex", this.startIndex);
                        cmdBuf.DrawProcedural(Matrix4x4.identity, this.material, 0, MeshTopology.Triangles, 6, 1, mpb);
                }
            }
            
            this.computeBuffer.SetData(this.tvertexes);
            this.indexBuffer.SetData(this.indexes);
        }
    }
}