using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    class _Placeholderpainter : AbstractCustomPainter {
        public _Placeholderpainter(
            Color color,
            float strokeWidth = 0f
        ) {
            this.color = color;
            this.strokeWidth = strokeWidth;
        }


        public readonly Color color;
        public readonly float strokeWidth;

        public override void paint(Canvas canvas, Size size) {
            Paint paint = new Paint();
            paint.color = this.color;
            paint.style = PaintingStyle.stroke;
            paint.strokeWidth = this.strokeWidth;

            Rect rect = Offset.zero & size;
            Path path = new Path();
            path.addRect(rect);
            path.addPolygon(new List<Offset> {rect.topRight, rect.bottomLeft}, false);
            path.addPolygon(new List<Offset> {rect.topLeft, rect.bottomRight}, false);

            canvas.drawPath(path, paint);
            return;
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            return ((_Placeholderpainter) oldPainter).color != this.color ||
                   ((_Placeholderpainter) oldPainter).strokeWidth != this.strokeWidth;
        }

        public override bool? hitTest(Offset position) {
            return false;
        }
    }

    public class Placeholder : StatelessWidget {
        public Placeholder(
            Key key = null,
            Color color = null,
            float strokeWidth = 2.0f,
            float fallbackWidth = 400.0f,
            float fallbackHeight = 400.0f
        ) : base(key) {
            this.color = color ?? new Color(0xFF455A64);
            this.strokeWidth = strokeWidth;
            this.fallbackWidth = fallbackWidth;
            this.fallbackHeight = fallbackHeight;
        }

        public readonly Color color;
        public readonly float strokeWidth;
        public readonly float fallbackWidth;
        public readonly float fallbackHeight;

        public override Widget build(BuildContext context) {
            return new LimitedBox(
                maxWidth: this.fallbackWidth,
                maxHeight: this.fallbackHeight,
                child: new CustomPaint(
                    size: Size.infinite,
                    foregroundPainter: new _Placeholderpainter(
                        color: this.color,
                        strokeWidth: this.strokeWidth
                    )
                )
            );
        }
    }
}