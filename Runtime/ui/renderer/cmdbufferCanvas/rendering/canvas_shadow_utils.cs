using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        
        void _drawRRectShadow(uiPath path, uiPaint paint) {
            D.assert(path.isNaiveRRect, () => "Cannot draw fast Shadow for non-NaiveRRect shapes");
            D.assert(paint.style == PaintingStyle.fill, () => "Cannot draw fast Shadow for stroke lines");
            var layer = this._currentLayer;
            var state = layer.currentState;
            
            var cache = path.flatten(state.scale * this._devicePixelRatio);
            bool convex;
            
            cache.computeFillMesh(this._fringeWidth, out convex);
            var fillMesh = cache.fillMesh;
            var meshBounds = fillMesh.transform(state.matrix);
            var clipBounds = layer.layerBounds;

            uiRect? stackBounds;
            bool iior;
            layer.clipStack.getBounds(out stackBounds, out iior);

            if (stackBounds != null) {
                clipBounds = uiRectHelper.intersect(clipBounds, stackBounds.Value);
            }

            if (clipBounds.isEmpty) {
                ObjectPool<uiMeshMesh>.release(meshBounds);
                return;
            }
            
            var maskBounds = meshBounds.bounds;
            maskBounds = uiRectHelper.intersect(maskBounds, clipBounds);
            if (maskBounds.isEmpty) {
                ObjectPool<uiMeshMesh>.release(meshBounds);
                return;
            }
            
            var blurMesh = ImageMeshGenerator.imageMesh(null, uiRectHelper.one, maskBounds);
            if (!this._applyClip(blurMesh.bounds)) {
                ObjectPool<uiMeshMesh>.release(meshBounds);
                ObjectPool<uiMeshMesh>.release(blurMesh);
                return;
            }

            var bound =  path.getBounds();
            var sigma = state.scale * paint.maskFilter.Value.sigma / 3f;
            
            var vertices = ObjectPool<uiList<Vector3>>.alloc();
            vertices.SetCapacity(4);
            vertices.Add(new Vector2(0, 0));
            vertices.Add(new Vector2(1, 0));
            vertices.Add(new Vector2(0, 1));
            vertices.Add(new Vector2(1, 1));
            
            var _triangles = ObjectPool<uiList<int>>.alloc();
            _triangles.SetCapacity(6);
            _triangles.Add(0);
            _triangles.Add(1);
            _triangles.Add(2);
            _triangles.Add(2);
            _triangles.Add(1);
            _triangles.Add(3);
            
            ObjectPool<uiMeshMesh>.release(meshBounds);
            ObjectPool<uiMeshMesh>.release(blurMesh);
            var mesh = uiMeshMesh.create(state.matrix, vertices, _triangles);
            var shadowColor = paint.color.withAlpha(128);
            layer.draws.Add(CanvasShader.fastShadow(layer, mesh, sigma, path.isRect, path.isCircle, path.rRectCorner, new Vector4(bound.left, bound.top, bound.right, bound.bottom), shadowColor));
        }

    }
}