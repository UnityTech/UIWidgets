using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    static class ArcUtils {
        public const float _kOnAxisDelta = 2.0f;

        public static readonly List<_Diagonal> _allDiagonals = new List<_Diagonal> {
            new _Diagonal(_CornerId.topLeft, _CornerId.bottomRight),
            new _Diagonal(_CornerId.bottomRight, _CornerId.topLeft),
            new _Diagonal(_CornerId.topRight, _CornerId.bottomLeft),
            new _Diagonal(_CornerId.bottomLeft, _CornerId.topRight)
        };

        public delegate float _KeyFunc<T>(T input);


        public static T _maxBy<T>(List<T> input, _KeyFunc<T> keyFunc) {
            T maxValue = default(T);
            float? maxKey = null;
            foreach (T value in input) {
                float key = keyFunc(value);
                if (maxKey == null || key > maxKey) {
                    maxValue = value;
                    maxKey = key;
                }
            }

            return maxValue;
        }
    }

    public class MaterialPointArcTween : Tween<Offset> {
        public MaterialPointArcTween(
            Offset begin = null,
            Offset end = null) : base(begin: begin, end: end) {
        }

        bool _dirty = true;

        void _initialze() {
            D.assert(this.begin != null);
            D.assert(this.end != null);

            Offset delta = this.end - this.begin;
            float deltaX = delta.dx.abs();
            float deltaY = delta.dy.abs();
            float distanceFromAtoB = delta.distance;
            Offset c = new Offset(this.end.dx, this.begin.dy);

            float sweepAngle() {
                return 2.0f * Mathf.Asin(distanceFromAtoB / (2.0f * this._radius));
            }

            if (deltaX > ArcUtils._kOnAxisDelta && deltaY > ArcUtils._kOnAxisDelta) {
                if (deltaX < deltaY) {
                    this._radius = distanceFromAtoB * distanceFromAtoB / (c - this.begin).distance / 2.0f;
                    this._center = new Offset(this.end.dx + this._radius * (this.begin.dx - this.end.dx).sign(),
                        this.end.dy);
                    if (this.begin.dx < this.end.dx) {
                        this._beginAngle = sweepAngle() * (this.begin.dy - this.end.dy).sign();
                        this._endAngle = 0.0f;
                    }
                    else {
                        this._beginAngle = (Mathf.PI + sweepAngle() * (this.end.dy - this.begin.dy).sign());
                        this._endAngle = Mathf.PI;
                    }
                }
                else {
                    this._radius = distanceFromAtoB * distanceFromAtoB / (c - this.end).distance / 2.0f;
                    this._center = new Offset(this.begin.dx,
                        this.begin.dy + (this.end.dy - this.begin.dy).sign() * this._radius);
                    if (this.begin.dy < this.end.dy) {
                        this._beginAngle = -Mathf.PI / 2.0f;
                        this._endAngle = this._beginAngle + sweepAngle() * (this.end.dx - this.begin.dx).sign();
                    }
                    else {
                        this._beginAngle = Mathf.PI / 2.0f;
                        this._endAngle = this._beginAngle + sweepAngle() * (this.begin.dx - this.end.dx).sign();
                    }
                }

                D.assert(this._beginAngle != null);
                D.assert(this._endAngle != null);
            }
            else {
                this._beginAngle = null;
                this._endAngle = null;
            }

            this._dirty = false;
        }

        public Offset center {
            get {
                if (this.begin == null || this.end == null) {
                    return null;
                }

                if (this._dirty) {
                    this._initialze();
                }

                return this._center;
            }
        }

        Offset _center;

        public float? radius {
            get {
                if (this.begin == null || this.end == null) {
                    return null;
                }

                if (this._dirty) {
                    this._initialze();
                }

                return this._radius;
            }
        }

        float _radius;

        public float? beginAngle {
            get {
                if (this.begin == null || this.end == null) {
                    return null;
                }

                if (this._dirty) {
                    this._initialze();
                }

                return this._beginAngle;
            }
        }

        float? _beginAngle;

        public float? endAngle {
            get {
                if (this.begin == null || this.end == null) {
                    return null;
                }

                if (this._dirty) {
                    this._initialze();
                }

                return this._endAngle;
            }
        }

        float? _endAngle;

        public override Offset begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    this._dirty = true;
                }
            }
        }

        public override Offset end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    this._dirty = true;
                }
            }
        }

        public override Offset lerp(float t) {
            if (this._dirty) {
                this._initialze();
            }

            if (t == 0.0) {
                return this.begin;
            }

            if (t == 1.0) {
                return this.end;
            }

            if (this._beginAngle == null || this._endAngle == null) {
                return Offset.lerp(this.begin, this.end, t);
            }

            float angle = MathUtils.lerpNullableFloat(this._beginAngle, this._endAngle, t) ?? 0.0f;
            float x = Mathf.Cos(angle) * this._radius;
            float y = Mathf.Sin(angle) * this._radius;
            return this._center + new Offset(x, y);
        }

        public override string ToString() {
            return this.GetType() + "(" + this.begin + "->" + this.end + "); center=" + this.center +
                   ", radius=" + this.radius + ", beginAngle=" + this.beginAngle + ", endAngle=" + this.endAngle;
        }
    }

    public enum _CornerId {
        topLeft,
        topRight,
        bottomLeft,
        bottomRight
    }

    public class _Diagonal {
        public _Diagonal(
            _CornerId beginId,
            _CornerId endId) {
            this.beginId = beginId;
            this.endId = endId;
        }

        public readonly _CornerId beginId;

        public readonly _CornerId endId;
    }

    public class MaterialRectArcTween : RectTween {
        public MaterialRectArcTween(
            Rect begin = null,
            Rect end = null) : base(begin: begin, end: end) {
        }

        bool _dirty = true;

        void _initialize() {
            D.assert(this.begin != null);
            D.assert(this.end != null);
            Offset centersVector = this.end.center - this.begin.center;
            _Diagonal diagonal = ArcUtils._maxBy(ArcUtils._allDiagonals,
                (_Diagonal d) => this._diagonalSupport(centersVector, d));
            this._beginArc = new MaterialPointArcTween(
                begin: this._cornerFor(this.begin, diagonal.beginId),
                end: this._cornerFor(this.end, diagonal.beginId));
            this._endArc = new MaterialPointArcTween(
                begin: this._cornerFor(this.begin, diagonal.endId),
                end: this._cornerFor(this.end, diagonal.endId));
            this._dirty = false;
        }

        float _diagonalSupport(Offset centersVector, _Diagonal diagonal) {
            Offset delta = this._cornerFor(this.begin, diagonal.endId) - this._cornerFor(this.begin, diagonal.beginId);
            float length = delta.distance;
            return centersVector.dx * delta.dx / length + centersVector.dy * delta.dy / length;
        }

        Offset _cornerFor(Rect rect, _CornerId id) {
            switch (id) {
                case _CornerId.topLeft: return rect.topLeft;
                case _CornerId.topRight: return rect.topRight;
                case _CornerId.bottomLeft: return rect.bottomLeft;
                case _CornerId.bottomRight: return rect.bottomRight;
            }

            return Offset.zero;
        }

        public MaterialPointArcTween beginArc {
            get {
                if (this.begin == null) {
                    return null;
                }

                if (this._dirty) {
                    this._initialize();
                }

                return this._beginArc;
            }
        }

        MaterialPointArcTween _beginArc;

        public MaterialPointArcTween endArc {
            get {
                if (this.end == null) {
                    return null;
                }

                if (this._dirty) {
                    this._initialize();
                }

                return this._endArc;
            }
        }

        MaterialPointArcTween _endArc;

        public override Rect begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    this._dirty = true;
                }
            }
        }

        public override Rect end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    this._dirty = true;
                }
            }
        }

        public override Rect lerp(float t) {
            if (this._dirty) {
                this._initialize();
            }

            if (t == 0.0) {
                return this.begin;
            }

            if (t == 1.0) {
                return this.end;
            }

            return Rect.fromPoints(this._beginArc.lerp(t), this._endArc.lerp(t));
        }

        public override string ToString() {
            return this.GetType() + "(" + this.begin + "->" + this.end + ")";
        }
    }

    public class MaterialRectCenterArcTween : RectTween {
        public MaterialRectCenterArcTween(
            Rect begin = null,
            Rect end = null) : base(begin: begin, end: end) {
        }

        bool _dirty = true;

        void _initialize() {
            D.assert(this.begin != null);
            D.assert(this.end != null);
            this._centerArc = new MaterialPointArcTween(
                begin: this.begin.center,
                end: this.end.center);
            this._dirty = false;
        }

        public MaterialPointArcTween centerArc {
            get {
                if (this.begin == null || this.end == null) {
                    return null;
                }

                if (this._dirty) {
                    this._initialize();
                }

                return this._centerArc;
            }
        }

        MaterialPointArcTween _centerArc;


        public override Rect begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    this._dirty = true;
                }
            }
        }

        public override Rect end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    this._dirty = true;
                }
            }
        }

        public override Rect lerp(float t) {
            if (this._dirty) {
                this._initialize();
            }

            if (t == 0.0) {
                return this.begin;
            }

            if (t == 1.0) {
                return this.end;
            }

            Offset center = this._centerArc.lerp(t);
            float width = MathUtils.lerpFloat(this.begin.width, this.end.width, t);
            float height = MathUtils.lerpFloat(this.begin.height, this.end.height, t);
            return Rect.fromLTWH(
                (center.dx - width / 2.0f),
                (center.dy - height / 2.0f),
                width,
                height);
        }

        public override string ToString() {
            return this.GetType() + "(" + this.begin + "->" + this.end + "); centerArc=" + this.centerArc;
        }
    }
}