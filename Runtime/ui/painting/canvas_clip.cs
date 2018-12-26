using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UnityEngine;

namespace UIWidgets.ui {
    internal class ClipElement {
        public readonly int saveCount;
        public readonly Mesh mesh;
        public readonly bool convex;
        public readonly bool isRect;
        public Rect rect { get; private set; }

        uint _genId;
        bool _isIntersectionOfRects;
        Rect _bound;

        public ClipElement(int saveCount, Path path, float[] xform, float devicePixelRatio) {
            this.saveCount = saveCount;

            var pathCache = path.flatten(xform, devicePixelRatio);
            this.mesh = pathCache.getFillMesh(out this.convex);

            var vertices = this.mesh.vertices;
            if (this.convex && vertices.Length == 4 &&
                (Mathf.Abs(vertices[0].x - vertices[1].x) < 1e-6 && Mathf.Abs(vertices[1].y - vertices[2].y) < 1e-6 &&
                 Mathf.Abs(vertices[2].x - vertices[3].x) < 1e-6 && Mathf.Abs(vertices[3].y - vertices[0].y) < 1e-6 ||
                 Mathf.Abs(vertices[0].y - vertices[1].y) < 1e-6 && Mathf.Abs(vertices[1].x - vertices[2].x) < 1e-6 &&
                 Mathf.Abs(vertices[2].y - vertices[3].y) < 1e-6 && Mathf.Abs(vertices[3].x - vertices[0].x) < 1e-6)) {
                var minx = Mathf.Min(vertices[0].x, vertices[1].x, vertices[2].x, vertices[3].x);
                var miny = Mathf.Min(vertices[0].y, vertices[1].y, vertices[2].y, vertices[3].y);
                var maxx = Mathf.Max(vertices[0].x, vertices[1].x, vertices[2].x, vertices[3].x);
                var maxy = Mathf.Max(vertices[0].y, vertices[1].y, vertices[2].y, vertices[3].y);
                this.isRect = true;
                this.rect = Rect.fromLTRB(minx, miny, maxx, maxy);
            } else {
                this.isRect = false;
                this.rect = null;
            }
        }

        public void setRect(Rect rect) {
            D.assert(ClipStack.invalidGenID != this._genId);
            D.assert(this.isRect && this.rect.contains(rect));
            this.rect = rect;
        }

        public void setEmpty() {
            this._genId = ClipStack.emptyGenID;
            this._isIntersectionOfRects = false;
            this._bound = Rect.zero;
        }

        public void updateBoundAndGenID(ClipElement prior) {
            this._genId = ClipStack.getNextGenID();
            this._isIntersectionOfRects = false;

            if (this.isRect) {
                this._bound = this.rect;
                if (prior == null || prior.isIntersectionOfRects()) {
                    this._isIntersectionOfRects = true;
                }
            } else {
                this._bound = this.mesh.getBounds();
            }

            if (prior != null) {
                this._bound = this._bound.intersect(prior.getBound());
            }

            if (this._bound.isEmpty) {
                this.setEmpty();
            }
        }

        public bool isEmpty() {
            D.assert(ClipStack.invalidGenID != this._genId);
            return this.getGenID() == ClipStack.emptyGenID;
        }

        public Rect getBound() {
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

        bool _convexContains(float x, float y) {
            if (this.mesh.vertexCount <= 2) {
                return false;
            }

            for (var i = 0; i < this.mesh.vertexCount; i++) {
                var p0 = this.mesh.vertices[i];
                var p1 = this.mesh.vertices[i == this.mesh.vertexCount - 1 ? 0 : i + 1];

                if (PathUtils.triarea2(p0.x, p0.y, p1.x, p1.y, x, y) < 0.0f) {
                    return false;
                }
            }

            return true;
        }

        public bool contains(Rect rect) {
            if (this.isRect) {
                return this.rect.contains(rect);
            }

            if (this.convex) {
                return this._convexContains((float) rect.left, (float) rect.top) &&
                       this._convexContains((float) rect.left, (float) rect.bottom) &&
                       this._convexContains((float) rect.right, (float) rect.top) &&
                       this._convexContains((float) rect.right, (float) rect.bottom);
            }

            return false;
        }
    }

    internal class ClipStack {
        static uint _genId = wideOpenGenID;

        public static uint getNextGenID() {
            return ++_genId;
        }

        public const uint invalidGenID = 0;

        public const uint emptyGenID = 1;

        public const uint wideOpenGenID = 2;

        public readonly List<ClipElement> stack = new List<ClipElement>();
        Rect _bound;
        int _saveCount;

        public void save() {
            this._saveCount++;
        }

        public void restore() {
            this._saveCount--;
            this._restoreTo(this._saveCount);
        }

        void _restoreTo(int saveCount) {
            while (this.stack.Count > 0) {
                var element = this.stack[this.stack.Count - 1];
                if (element.saveCount <= saveCount) {
                    break;
                }

                this.stack.RemoveAt(this.stack.Count - 1);
            }
        }

        public void clipPath(Path path, float[] xform, float devicePixelRatio) {
            var element = new ClipElement(this._saveCount, path, xform, devicePixelRatio);
            this._pushElement(element);
        }

        void _pushElement(ClipElement element) {
            ClipElement prior = this.stack.LastOrDefault();
            if (prior != null) {
                if (prior.isEmpty()) {
                    return;
                }

                if (prior.saveCount == this._saveCount) {
                    // can not update prior if it's cross save count.
                    if (prior.isRect && element.isRect) {
                        var isectRect = prior.rect.intersect(element.rect);
                        if (isectRect.isEmpty) {
                            prior.setEmpty();
                            return;
                        }

                        prior.setRect(isectRect);
                        var priorprior = this.stack.Count > 1 ? this.stack[this.stack.Count - 2] : null;
                        prior.updateBoundAndGenID(priorprior);
                        return;
                    }

                    if (!prior.getBound().overlaps(element.getBound())) {
                        prior.setEmpty();
                        return;
                    }
                }
            }

            this.stack.Add(element);
            element.updateBoundAndGenID(prior);
        }

        public void getBounds(out Rect bound, out bool isIntersectionOfRects) {
            if (this.stack.Count == 0) {
                bound = null;
                isIntersectionOfRects = false;
                return;
            }

            var element = this.stack.Last();
            bound = element.getBound();
            isIntersectionOfRects = element.isIntersectionOfRects();
        }
    }

    internal class ReducedClip {
        public readonly Rect scissor;
        public readonly List<ClipElement> maskElements = new List<ClipElement>();

        public bool isEmpty() {
            return this.scissor != null && this.scissor.isEmpty;
        }

        public uint maskGenID() {
            var element = this.maskElements.LastOrDefault();
            if (element == null) {
                return ClipStack.wideOpenGenID;
            }

            return element.getGenID();
        }

        public ReducedClip(ClipStack stack, Rect layerBounds, Rect queryBounds) {
            Rect stackBounds;
            bool iior;
            stack.getBounds(out stackBounds, out iior);

            if (stackBounds == null) {
                this.scissor = layerBounds;
                return;
            }

            stackBounds = layerBounds.intersect(stackBounds);

            if (iior) {
                this.scissor = stackBounds;
                return;
            }

            queryBounds = stackBounds.intersect(queryBounds);
            if (queryBounds.isEmpty) {
                this.scissor = Rect.zero;
                return;
            }

            this.scissor = queryBounds;
            this._walkStack(stack, this.scissor);
        }

        void _walkStack(ClipStack stack, Rect queryBounds) {
            foreach (var element in stack.stack) {
                if (element.isRect) {
                    continue;
                }

                if (element.contains(queryBounds)) {
                    continue;
                }

                this.maskElements.Add(element);
            }
        }
    }
}