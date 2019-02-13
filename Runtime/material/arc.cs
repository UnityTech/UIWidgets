using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
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
            double deltaX = delta.dx.abs();
            double deltaY = delta.dy.abs();
            double distanceFromAtoB = delta.distance;
            Offset c = new Offset(this.end.dx, this.begin.dy);

            double sweepAngle() {
                return 2.0 * Math.Asin(distanceFromAtoB / (2.0 * this._radius));
            }

            if (deltaX > ArcUtils._kOnAxisDelta && deltaY > ArcUtils._kOnAxisDelta) {
                if (deltaX < deltaY) {
                    this._radius = distanceFromAtoB * distanceFromAtoB / (c - this.begin).distance / 2.0;
                    this._center = new Offset(this.end.dx + this._radius * (this.begin.dx - this.end.dx).sign(),
                        this.end.dy);
                    if (this.begin.dx < this.end.dx) {
                        this._beginAngle = sweepAngle() * (this.begin.dy - this.end.dy).sign();
                        this._endAngle = 0.0;
                    }
                    else {
                        this._beginAngle = Math.PI + sweepAngle() * (this.end.dy - this.begin.dy).sign();
                        this._endAngle = Math.PI;
                    }
                }
                else {
                    this._radius = distanceFromAtoB * distanceFromAtoB / (c - this.end).distance / 2.0;
                    this._center = new Offset(this.begin.dx,
                        this.begin.dy + (this.end.dy - this.begin.dy).sign() * this._radius);
                    if (this.begin.dy < this.end.dy) {
                        this._beginAngle = -Math.PI / 2.0;
                        this._endAngle = this._beginAngle + sweepAngle() * (this.end.dx - this.begin.dx).sign();
                    }
                    else {
                        this._beginAngle = Math.PI / 2.0;
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

        public double? radius {
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

        double _radius;

        public double? beginAngle {
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

        double? _beginAngle;

        public double? endAngle {
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

        double? _endAngle;

        public new Offset begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    this._dirty = true;
                }
            }
        }

        public new Offset end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    this._dirty = true;
                }
            }
        }

        public override Offset lerp(double t) {
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

            double angle = MathUtils.lerpNullableDouble(this._beginAngle, this._endAngle, t) ?? 0.0;
            double x = Math.Cos(angle) * this._radius;
            double y = Math.Sin(angle) * this._radius;
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

        double _diagonalSupport(Offset centersVector, _Diagonal diagonal) {
            Offset delta = this._cornerFor(this.begin, diagonal.endId) - this._cornerFor(this.begin, diagonal.beginId);
            double length = delta.distance;
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

        public new Rect begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    this._dirty = true;
                }
            }
        }

        public new Rect end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    this._dirty = true;
                }
            }
        }

        public override Rect lerp(double t) {
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


        public new Rect begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    this._dirty = true;
                }
            }
        }

        public new Rect end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    this._dirty = true;
                }
            }
        }

        public override Rect lerp(double t) {
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
            double width = MathUtils.lerpDouble(this.begin.width, this.end.width, t);
            double height = MathUtils.lerpDouble(this.begin.height, this.end.height, t);
            return Rect.fromLTWH(
                center.dx - width / 2.0,
                center.dy - height / 2.0,
                width,
                height);
        }

        public override string ToString() {
            return this.GetType() + "(" + this.begin + "->" + this.end + "); centerArc=" + this.centerArc;
        }
    }
}