using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsSample {
    public class CustomPaintSample : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new WidgetsApp(
                home: new Unity.UIWidgets.widgets.CustomPaint(
                    child: new Container(width: 300, height: 300, color: new Color(0XFFFFFFFF)),
                    foregroundPainter: new GridPainter(null)
                ),
                pageRouteBuilder: this.pageRouteBuilder);
        }
    }

    public class GridPainter : AbstractCustomPainter {
        public GridPainter(Listenable repaint) : base(repaint) {
        }

        public override void paint(Canvas canvas, Size size) {
            int numGrid = 4;
            var paint = new Paint();
            paint.color = new Color(0xFFFF0000);
            paint.strokeWidth = 2;
            paint.style = PaintingStyle.stroke;
            for (int i = 1; i < numGrid; i++) {
                float offsetY = size.height * i / numGrid;
                canvas.drawLine(new Offset(0, offsetY), new Offset(size.width, offsetY),
                    paint);
            }

            for (int i = 1; i < numGrid; i++) {
                float offsetx = size.width * i / numGrid;
                canvas.drawLine(new Offset(offsetx, 0), new Offset(offsetx, size.height),
                    paint);
            }


            // draw a arrow line
            canvas.save();
            canvas.rotate(0.4f);
            canvas.scale(2, 2);
            canvas.translate(50, 50);
            canvas.drawLine(new Offset(0, 0), new Offset(100, 0),
                new Paint() {
                    color = new Color(0xFFFF0000),
                    strokeWidth = 2,
                    style = PaintingStyle.stroke
                });
            var path = new Path();
            var arrowPaint = new Paint() {
                color = new Color(0xFFFF0000),
                style = PaintingStyle.fill
            };
            path.moveTo(100, 0);
            path.lineTo(100, 5);
            path.lineTo(120, 0);
            path.lineTo(100, -5);
            path.close();
            canvas.drawPath(path, arrowPaint);
            canvas.restore();
        }

        public override bool shouldRepaint(CustomPainter oldDelegate) {
            return false;
        }
    }
}