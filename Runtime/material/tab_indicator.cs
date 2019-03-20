using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public class UnderlineTabIndicator : Decoration {
        public UnderlineTabIndicator(
            BorderSide borderSide = null,
            EdgeInsets insets = null) {
            borderSide = borderSide ?? new BorderSide(width: 2.0f, color: Colors.white);
            insets = insets ?? EdgeInsets.zero;
            this.borderSide = borderSide;
            this.insets = insets;
        }

        public readonly BorderSide borderSide;

        public readonly EdgeInsets insets;

        public override Decoration lerpFrom(Decoration a, float t) {
            if (a is UnderlineTabIndicator) {
                UnderlineTabIndicator _a = (UnderlineTabIndicator) a;
                return new UnderlineTabIndicator(
                    borderSide: BorderSide.lerp(_a.borderSide, this.borderSide, t),
                    insets: EdgeInsets.lerp(_a.insets, this.insets, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override Decoration lerpTo(Decoration b, float t) {
            if (b is UnderlineTabIndicator) {
                UnderlineTabIndicator _b = (UnderlineTabIndicator) b;
                return new UnderlineTabIndicator(
                    borderSide: BorderSide.lerp(this.borderSide, _b.borderSide, t),
                    insets: EdgeInsets.lerp(this.insets, _b.insets, t)
                );
            }

            return base.lerpTo(b, t);
        }


        public override BoxPainter createBoxPainter(VoidCallback onChanged) {
            return new _UnderlinePainter(this, onChanged);
        }
    }


    class _UnderlinePainter : BoxPainter {
        public _UnderlinePainter(
            UnderlineTabIndicator decoration = null,
            VoidCallback onChanged = null
        ) : base(onChanged: onChanged) {
            D.assert(decoration != null);
            this.decoration = decoration;
        }

        public readonly UnderlineTabIndicator decoration;

        public BorderSide borderSide {
            get { return this.decoration.borderSide; }
        }

        public EdgeInsets insets {
            get { return this.decoration.insets; }
        }

        Rect _indicatorRectFor(Rect rect) {
            D.assert(rect != null);
            Rect indicator = this.insets.deflateRect(rect);
            return Rect.fromLTWH(
                indicator.left,
                indicator.bottom - this.borderSide.width,
                indicator.width,
                this.borderSide.width);
        }

        public override void paint(Canvas canvas, Offset offset, ImageConfiguration configuration) {
            D.assert(configuration != null);
            D.assert(configuration.size != null);
            Rect rect = offset & configuration.size;
            Rect indicator = this._indicatorRectFor(rect).deflate(this.borderSide.width / 2.0f);
            Paint paint = this.borderSide.toPaint();
            paint.strokeCap = StrokeCap.square;
            canvas.drawLine(indicator.bottomLeft, indicator.bottomRight, paint);
        }
    }
}