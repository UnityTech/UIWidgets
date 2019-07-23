using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        
        void _drawRRectShadow(uiPath path, uiPaint paint) {
            D.assert(path.isRRect, () => "Cannot draw Shadow for non-RRect shapes");
            D.assert(paint.style == PaintingStyle.fill, () => "Cannot draw Shadow for stroke lines");
            var bound = path.getBounds();
            if (!this._applyClip(bound)) {
                return;
            }
            
            var layer = this._currentLayer;
            var state = layer.currentState;
            float sigma = state.scale * paint.maskFilter.Value.sigma;
            
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
            
            var mesh = uiMeshMesh.create(state.matrix, vertices, _triangles);
            layer.draws.Add(CanvasShader.fastShadow(layer, mesh, sigma, path.isRect, path.isCircle, path.rCorner, new Vector4(bound.left, bound.top, bound.right, bound.bottom), paint.color));
        }

    }
}