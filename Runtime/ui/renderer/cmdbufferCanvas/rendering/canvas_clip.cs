using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class ClipElement : PoolObject {
        public int saveCount;
        public uiMeshMesh mesh;
        public bool convex;
        public bool isRect;
        public uiRect? rect { get; private set; }

        uint _genId;
        bool _isIntersectionOfRects;
        uiRect _bound;
        uiMatrix3? _invMat;

        public ClipElement() {
        }

        public override void clear() {
            ObjectPool<uiMeshMesh>.release(this.mesh);
            this.saveCount = 0;
            this.mesh = null;
            this.convex = false;
            this.isRect = false;
            this._genId = 0;
            this._isIntersectionOfRects = false;
            this._invMat = null;
        }

        public static ClipElement create(int saveCount, uiPath uiPath, uiMatrix3 matrix, float scale) {
            ClipElement newElement = ObjectPool<ClipElement>.alloc();

            newElement.saveCount = saveCount;

            var pathCache = uiPath.flatten(scale);
            pathCache.computeFillMesh(0.0f, out newElement.convex);
            var fillMesh = pathCache.fillMesh;
            newElement.mesh = fillMesh.transform(matrix);

            var vertices = newElement.mesh.vertices;
            if (newElement.convex && vertices.Count == 4 && matrix.rectStaysRect() &&
                (Mathf.Abs(vertices[0].x - vertices[1].x) < 1e-6 && Mathf.Abs(vertices[1].y - vertices[2].y) < 1e-6 &&
                 Mathf.Abs(vertices[2].x - vertices[3].x) < 1e-6 && Mathf.Abs(vertices[3].y - vertices[0].y) < 1e-6 ||
                 Mathf.Abs(vertices[0].y - vertices[1].y) < 1e-6 && Mathf.Abs(vertices[1].x - vertices[2].x) < 1e-6 &&
                 Mathf.Abs(vertices[2].y - vertices[3].y) < 1e-6 && Mathf.Abs(vertices[3].x - vertices[0].x) < 1e-6)) {
                newElement.isRect = true;
                newElement.rect = newElement.mesh.bounds;
            }
            else {
                newElement.isRect = false;
                newElement.rect = null;
            }

            return newElement;
        }

        public void setRect(uiRect rect) {
            D.assert(ClipStack.invalidGenID != this._genId);
            D.assert(this.isRect && uiRectHelper.contains(this.rect.Value, rect));
            this.rect = rect;
        }

        public void setEmpty() {
            this._genId = ClipStack.emptyGenID;
            this._isIntersectionOfRects = false;
            this._bound = uiRectHelper.zero;
        }

        public void updateBoundAndGenID(ClipElement prior) {
            this._genId = ClipStack.getNextGenID();
            this._isIntersectionOfRects = false;

            if (this.isRect) {
                this._bound = this.rect.Value;
                if (prior == null || prior.isIntersectionOfRects()) {
                    this._isIntersectionOfRects = true;
                }
            }
            else {
                this._bound = this.mesh.bounds;
            }

            if (prior != null) {
                this._bound = uiRectHelper.intersect(this._bound, prior.getBound());
            }

            if (this._bound.isEmpty) {
                this.setEmpty();
            }
        }

        public bool isEmpty() {
            D.assert(ClipStack.invalidGenID != this._genId);
            return this.getGenID() == ClipStack.emptyGenID;
        }

        public uiRect getBound() {
            D.assert(ClipStack.invalidGenID != this._genId);
            return this._bound;
        }

        public bool isIntersectionOfRects() {
            D.assert(ClipStack.invalidGenID != this._genId);
            return this._isIntersectionOfRects;
        }

        public uint getGenID() {
            D.assert(ClipStack.invalidGenID != this._genId);
            return this._genId;
        }

        bool _convexContains(uiRect rect) {
            if (this.mesh.vertices.Count <= 2) {
                return false;
            }

            for (var i = 0; i < this.mesh.vertices.Count; i++) {
                var p1 = this.mesh.vertices[i];
                var p0 = this.mesh.vertices[i == this.mesh.vertices.Count - 1 ? 0 : i + 1];

                var v = p1 - p0;
                if (v.x == 0.0 && v.y == 0.0) {
                    continue;
                }

                float yL = v.y * (rect.left - p0.x);
                float xT = v.x * (rect.top - p0.y);
                float yR = v.y * (rect.right - p0.x);
                float xB = v.x * (rect.bottom - p0.y);

                if ((xT < yL) || (xT < yR) || (xB < yL) || (xB < yR)) {
                    return false;
                }
            }

            return true;
        }

        public bool contains(uiRect rect) {
            if (this.isRect) {
                return uiRectHelper.contains(this.rect.Value, rect);
            }

            if (this.convex) {
                if (this.mesh.matrix != null && !this.mesh.matrix.Value.isIdentity()) {
                    if (this._invMat == null) {
                        this._invMat = this.mesh.matrix.Value.invert();
                    }

                    rect = this._invMat.Value.mapRect(rect);
                }

                return this._convexContains(rect);
            }

            return false;
        }
    }

    class ClipStack : PoolObject {
        static uint _genId = wideOpenGenID;

        public static uint getNextGenID() {
            return ++_genId;
        }

        public const uint invalidGenID = 0;

        public const uint emptyGenID = 1;

        public const uint wideOpenGenID = 2;

        public readonly List<ClipElement> stack = new List<ClipElement>(32);

        ClipElement _lastElement;
        uiRect _bound;
        int _saveCount;

        public static ClipStack create() {
            return ObjectPool<ClipStack>.alloc();
        }

        public override void clear() {
            this._saveCount = 0;
            this._lastElement = null;
            foreach (var clipelement in this.stack) {
                ObjectPool<ClipElement>.release(clipelement);
            }

            this.stack.Clear();
        }

        public void save() {
            this._saveCount++;
        }

        public void restore() {
            this._saveCount--;
            this._restoreTo(this._saveCount);
        }

        void _restoreTo(int saveCount) {
            while (this._lastElement != null) {
                if (this._lastElement.saveCount <= saveCount) {
                    break;
                }

                var lastelement = this.stack[this.stack.Count - 1];
                ObjectPool<ClipElement>.release(lastelement);

                this.stack.RemoveAt(this.stack.Count - 1);
                this._lastElement = this.stack.Count == 0 ? null : this.stack[this.stack.Count - 1];
            }
        }

        public void clipPath(uiPath uiPath, uiMatrix3 matrix, float scale) {
            var element = ClipElement.create(this._saveCount, uiPath, matrix, scale);
            this._pushElement(element);
        }

        void _pushElement(ClipElement element) {
            ClipElement prior = this._lastElement;
            if (prior != null) {
                if (prior.isEmpty()) {
                    ObjectPool<ClipElement>.release(element);
                    return;
                }

                if (prior.saveCount == this._saveCount) {
                    // can not update prior if it's cross save count.
                    if (prior.isRect && element.isRect) {
                        var isectRect = uiRectHelper.intersect(prior.rect.Value, element.rect.Value);
                        if (isectRect.isEmpty) {
                            prior.setEmpty();
                            ObjectPool<ClipElement>.release(element);
                            return;
                        }

                        prior.setRect(isectRect);
                        var priorprior = this.stack.Count > 1 ? this.stack[this.stack.Count - 2] : null;
                        prior.updateBoundAndGenID(priorprior);
                        ObjectPool<ClipElement>.release(element);
                        return;
                    }

                    if (!uiRectHelper.overlaps(prior.getBound(), element.getBound())) {
                        prior.setEmpty();
                        ObjectPool<ClipElement>.release(element);
                        return;
                    }
                }
            }

            this.stack.Add(element);
            this._lastElement = element;
            element.updateBoundAndGenID(prior);
        }

        public void getBounds(out uiRect? bound, out bool isIntersectionOfRects) {
            if (this._lastElement == null) {
                bound = null;
                isIntersectionOfRects = false;
                return;
            }

            var element = this._lastElement;
            bound = element.getBound();
            isIntersectionOfRects = element.isIntersectionOfRects();
        }
    }

    class ReducedClip : PoolObject {
        public uiRect? scissor;
        public List<ClipElement> maskElements = new List<ClipElement>();
        ClipElement _lastElement;

        public bool isEmpty() {
            return this.scissor != null && this.scissor.Value.isEmpty;
        }

        public ReducedClip() {
        }

        public override void clear() {
            this.scissor = null;
            this.maskElements.Clear();
            this._lastElement = null;
        }

        public uint maskGenID() {
            var element = this._lastElement;
            if (element == null) {
                return ClipStack.wideOpenGenID;
            }

            return element.getGenID();
        }

        public static ReducedClip create(ClipStack stack, uiRect layerBounds, uiRect queryBounds) {
            ReducedClip clip = ObjectPool<ReducedClip>.alloc();
            uiRect? stackBounds;
            bool iior;
            stack.getBounds(out stackBounds, out iior);

            if (stackBounds == null) {
                clip.scissor = layerBounds;
                return clip;
            }

            stackBounds = uiRectHelper.intersect(layerBounds, stackBounds.Value);
            if (iior) {
                clip.scissor = stackBounds;
                return clip;
            }

            queryBounds = uiRectHelper.intersect(stackBounds.Value, queryBounds);
            if (queryBounds.isEmpty) {
                clip.scissor = uiRectHelper.zero;
                return clip;
            }

            clip.scissor = queryBounds;
            clip._walkStack(stack, clip.scissor.Value);
            return clip;
        }

        void _walkStack(ClipStack stack, uiRect queryBounds) {
            foreach (var element in stack.stack) {
                if (element.isRect) {
                    continue;
                }

                if (element.contains(queryBounds)) {
                    continue;
                }

                this.maskElements.Add(element);
                this._lastElement = element;
            }
        }
    }
}