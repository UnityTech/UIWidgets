using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public class ContinuousRectangleBorder : ShapeBorder {

        public ContinuousRectangleBorder(
            BorderSide side = null,
            BorderRadius borderRadius = null) {
            this.side = side ?? BorderSide.none;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
        }

        public readonly BorderRadius borderRadius;

        public readonly BorderSide side;

        public override EdgeInsets dimensions {
            get {
                return EdgeInsets.all(this.side.width);
            }
        }

        public override ShapeBorder scale(float t) {
            return new ContinuousRectangleBorder(
                side: this.side.scale(t),
                borderRadius: this.borderRadius * t
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is ContinuousRectangleBorder) {
                return new ContinuousRectangleBorder(
                    side: BorderSide.lerp((a as ContinuousRectangleBorder).side, this.side, t),
                    borderRadius: BorderRadius.lerp((a as ContinuousRectangleBorder).borderRadius,
                        this.borderRadius, t)
                );
            }
            return base.lerpFrom(a, t);
        }
        
        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is ContinuousRectangleBorder) {
                return new ContinuousRectangleBorder(
                    side: BorderSide.lerp(this.side, (b as ContinuousRectangleBorder).side, t),
                    borderRadius: BorderRadius.lerp(this.borderRadius,
                        (b as ContinuousRectangleBorder).borderRadius, t)
                );
            }
            return base.lerpTo(b, t);
        }

        float _clampToShortest(RRect rrect, float value) {
            return value > rrect.shortestSide ? rrect.shortestSide : value;
        }

        Path _getPath(RRect rrect) {
            float left = rrect.left;
            float right = rrect.right;
            float top = rrect.top;
            float bottom = rrect.bottom;
            
            float tlRadiusX = Mathf.Max(0.0f, this._clampToShortest(rrect, rrect.tlRadiusX));
            float tlRadiusY = Mathf.Max(0.0f, this._clampToShortest(rrect, rrect.tlRadiusY));
            float trRadiusX = Mathf.Max(0.0f, this._clampToShortest(rrect, rrect.trRadiusX));
            float trRadiusY = Mathf.Max(0.0f, this._clampToShortest(rrect, rrect.trRadiusY));
            float blRadiusX = Mathf.Max(0.0f, this._clampToShortest(rrect, rrect.blRadiusX));
            float blRadiusY = Mathf.Max(0.0f, this._clampToShortest(rrect, rrect.blRadiusY));
            float brRadiusX = Mathf.Max(0.0f, this._clampToShortest(rrect, rrect.brRadiusX));
            float brRadiusY = Mathf.Max(0.0f, this._clampToShortest(rrect, rrect.brRadiusY));

            Path path = new Path();
            path.moveTo(left, top + tlRadiusX);
            path.cubicTo(left, top, left, top, left + tlRadiusY, top);
            path.lineTo(right - trRadiusX, top);
            path.cubicTo(right, top, right, top, right, top + trRadiusY);
            path.lineTo(right, bottom - blRadiusX);
            path.cubicTo(right, bottom, right, bottom, right - blRadiusY, bottom);
            path.lineTo(left + brRadiusX, bottom);
            path.cubicTo(left, bottom, left, bottom, left, bottom - brRadiusY);
            path.close();
            return path;
        }

        public override Path getInnerPath(Rect rect) {
            return this._getPath(this.borderRadius.toRRect(rect).deflate(this.side.width));
        }

        public override Path getOuterPath(Rect rect) {
            return this._getPath(this.borderRadius.toRRect(rect));
        }

        public override void paint(Canvas canvas, Rect rect) {
            if (rect.isEmpty) {
                return;
            }

            switch (this.side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    Path path = this.getOuterPath(rect);
                    Paint paint = this.side.toPaint();
                    canvas.drawPath(path, paint);
                    break;
            }
        }
        public bool Equals(ContinuousRectangleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.side == other.side && this.borderRadius == other.borderRadius;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((ContinuousRectangleBorder) obj);
        }

        public override int GetHashCode() {
            var hashCode = (this.side != null ? this.side.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.borderRadius != null ? this.borderRadius.GetHashCode() : 0);
            return hashCode;
        }

        public static bool operator ==(ContinuousRectangleBorder left, ContinuousRectangleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(ContinuousRectangleBorder left, ContinuousRectangleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.side}, {this.borderRadius})";
        }
    }
}