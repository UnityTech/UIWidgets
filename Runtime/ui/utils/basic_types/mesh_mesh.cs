using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class uiMeshMesh : PoolItem {
        public uiList<Vector3> vertices;
        public uiList<int> triangles;
        public uiList<Vector2> uv;
        public uiMatrix3 matrix;
        public uiRect rawBounds;

        uiRect? _bounds;

        public uiRect bounds {
            get {
                if (this._bounds == null) {
                    this._bounds = this.matrix != null ? this.matrix.mapRect(this.rawBounds) : this.rawBounds;
                }

                return this._bounds.Value;
            }
        }

        static readonly List<int> _boundsTriangles = new List<int>(6) {
            0, 2, 1, 1, 2, 3
        };

        public uiMeshMesh boundsMesh {
            get {
                return create(this.bounds);
            }
        }

        public uiMeshMesh() {
        }

        public override void clear() {
            this.vertices?.dispose();
            this.triangles?.dispose();
            this.uv?.dispose();
            this.vertices = null;
            this.triangles = null;
            this.uv = null;
            this.matrix = null;
            this._bounds = null;
        }

        public static uiMeshMesh create(uiRect rect) {
            uiMeshMesh newMesh = ItemPoolManager.alloc<uiMeshMesh>();

            newMesh.vertices = ItemPoolManager.alloc<uiList<Vector3>>();
            newMesh.vertices.Add(new Vector3(rect.right, rect.bottom));
            newMesh.vertices.Add(new Vector3(rect.right, rect.top));
            newMesh.vertices.Add(new Vector3(rect.left, rect.bottom));
            newMesh.vertices.Add(new Vector3(rect.left, rect.top));

            newMesh.triangles = ItemPoolManager.alloc<uiList<int>>();
            newMesh.triangles.AddRange(_boundsTriangles);
            newMesh.rawBounds = rect;

            newMesh._bounds = newMesh.rawBounds;

            return newMesh;
        }

        public static uiMeshMesh create(uiMatrix3 matrix, uiList<Vector3> vertices, uiList<int> triangles, uiList<Vector2> uv = null,
            uiRect? rawBounds = null) {
            D.assert(vertices != null);
            D.assert(vertices.Count >= 0);
            D.assert(triangles != null);
            D.assert(triangles.Count >= 0);
            D.assert(uv == null || uv.Count == vertices.Count);
            
            uiMeshMesh newMesh = ItemPoolManager.alloc<uiMeshMesh>();
            newMesh.matrix = matrix;
            newMesh.vertices = vertices;
            newMesh.triangles = triangles;
            if (uv != null) {
                newMesh.uv = uv;
            }

            if (rawBounds == null) {
                if (vertices.Count > 0) {
                    float minX = vertices[0].x;
                    float maxX = vertices[0].x;
                    float minY = vertices[0].y;
                    float maxY = vertices[0].y;

                    for (int i = 1; i < vertices.Count; i++) {
                        var vertex = vertices[i];
                        if (vertex.x < minX) {
                            minX = vertex.x;
                        }

                        if (vertex.x > maxX) {
                            maxX = vertex.x;
                        }

                        if (vertex.y < minY) {
                            minY = vertex.y;
                        }

                        if (vertex.y > maxY) {
                            maxY = vertex.y;
                        }
                    }

                    rawBounds = uiRectHelper.fromLTRB(minX, minY, maxX, maxY);
                }
                else {
                    rawBounds = uiRectHelper.zero;
                }
            }

            newMesh.rawBounds = rawBounds.Value;

            return newMesh;
        }

        public uiMeshMesh duplicate() {
            return this.transform(this.matrix);
        }

        public uiMeshMesh transform(uiMatrix3 matrix) {
            var vertices = ItemPoolManager.alloc<uiList<Vector3>>();
            vertices.SetCapacity(this.vertices.Count);
            vertices.AddRange(this.vertices.data);
            
            var triangles = ItemPoolManager.alloc<uiList<int>>();
            triangles.SetCapacity(this.triangles.Count);
            triangles.AddRange(this.triangles.data);


            uiList<Vector2> uv = null;

            if (this.uv != null) {
                uv = ItemPoolManager.alloc<uiList<Vector2>>();
                uv.SetCapacity(this.uv.Count);
                uv.AddRange(this.uv.data);
            }


            var ret = create(matrix, vertices, triangles, uv, this.rawBounds);
            return ret;
        }
    }
}