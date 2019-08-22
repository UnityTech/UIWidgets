using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Gradient = Unity.UIWidgets.ui.Gradient;
using Image = Unity.UIWidgets.ui.Image;
using UIWidgetRect = Unity.UIWidgets.ui.Rect;


public class AntialiasSVGSample : UIWidgetsPanel {
    protected override void OnEnable() {
        FontManager.instance.addFont(Resources.Load<Font>("MaterialIcons-Regular"), "Material Icons");
        FontManager.instance.addFont(Resources.Load<Font>("GalleryIcons"), "GalleryIcons");

        base.OnEnable();
    }

    protected override Widget createWidget() {
        Debug.Log("[SVG Testbed Panel Created]");

        return new SVGTestbedPanelWidget();
    }
}

public class SVGTestbedPanelWidget : StatefulWidget {
    public SVGTestbedPanelWidget(Key key = null) : base(key) { }

    public override State createState() {
        return new SVGTestbedPanelWidgetState();
    }
}

public class SVGTestbedPanelWidgetState : State<SVGTestbedPanelWidget> {
    static Texture2D texture6;

    public class CurvePainter : AbstractCustomPainter {
        public override void paint(Canvas canvas, Size size) {
            var paint = new Paint()
            {
                color = new Color(0xFFFF0000),
            };

            paint.color = Colors.yellow;
            paint.style = PaintingStyle.stroke;
            paint.strokeWidth = 3;

            var startPoint = new Offset(0, size.height / 6);
            var controlPoint1 = new Offset(size.width / 4, 0);
            var controlPoint2 = new Offset(3 * size.width / 4, 0);
            var endPoint = new Offset(size.width, size.height / 6);
            
            var path = new Path();
            path.moveTo(startPoint.dx, startPoint.dy);
            path.cubicTo(
                controlPoint1.dx, controlPoint1.dy,
                controlPoint2.dx, controlPoint2.dy,
                endPoint.dx, endPoint.dy
            );

            path.moveTo(10, 10);
            path.lineTo(90, 10);
            path.lineTo(10, 90);
            path.lineTo(90, 90);
            path.winding(PathWinding.clockwise);
            path.close();

            path.moveTo(110, 10);
            path.lineTo(190, 10);
            path.lineTo(110, 90);
            path.lineTo(190, 90);
            path.close();

            path.addRect(UIWidgetRect.fromLTWH(10, 25, 180, 50));

            path.addRect(UIWidgetRect.fromLTWH(200, 0, 100, 100));
            path.addRRect(RRect.fromRectAndRadius(UIWidgetRect.fromLTWH(225, 25, 50, 50), 10));
            path.winding(PathWinding.clockwise);
            path.addOval(UIWidgetRect.fromLTWH(200, 50, 100, 100));
            path.winding(PathWinding.clockwise);


            canvas.drawPath(path, paint);

            paint = new Paint {
                color = new Color(0xFFFF0000),
                shader = Gradient.linear(
                    new Offset(0, 0),
                    new Offset(size.width, 200),
                    new List<Color>() {
                        Colors.red, Colors.black, Colors.green
                    }, null, TileMode.clamp),
            };
            canvas.translate(0, 200);
            canvas.drawPath(path, paint);

            canvas.translate(0, 200);
            // paint.maskFilter = MaskFilter.blur(BlurStyle.normal, 5);
            paint.shader = new ImageShader(new Image(texture6, true), TileMode.mirror);
            canvas.drawPath(path, paint);
            
            canvas.translate(0, 200);
            paint = new Paint {
                color = new Color(0xFF00FF00),
                shader = Gradient.sweep(
                    new Offset(size.width / 2, 100),
                    new List<Color>() {
                        Colors.red, Colors.black, Colors.green, Colors.red,
                    }, null, TileMode.clamp, 0 * Mathf.PI / 180, 360 * Mathf.PI / 180),
            };
            canvas.drawPath(path, paint);


            paint.shader = Gradient.radial(
                new Offset(size.width / 2, 100), 200f,
                new List<Color>()
                {
                    Colors.red, Colors.black, Colors.green
                }, null, TileMode.clamp);
            canvas.translate(0, 200);
            canvas.drawPath(path, paint);
        }

        public override bool shouldRepaint(CustomPainter oldDelegate) {
            return true;
        }
    }

    public override Widget build(BuildContext context) {
        texture6 = Resources.Load<Texture2D>("6");

        return new SingleChildScrollView(
            child: new Container(
                child: new CustomPaint(
                    painter: new CurvePainter(),
                    child: new Container()
                ),
                height: 1500
            )
        );
    }
}