using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoThumbPainter {
        public CupertinoThumbPainter(
            Color color = null,
            Color shadowColor = null
        ) {
            this._shadowPaint = new BoxShadow(
                color: shadowColor,
                blurRadius: 1.0f
            ).toPaint();

            this.color = color ?? CupertinoColors.white;
            this.shadowColor = shadowColor ?? new Color(0x2C000000);
        }

        public readonly Color color;

        public readonly Color shadowColor;

        public readonly Paint _shadowPaint;

        public const float radius = 14.0f;

        public const float extension = 7.0f;

        public void paint(Canvas canvas, Rect rect) {
            RRect rrect = RRect.fromRectAndRadius(
                rect,
                Radius.circular(rect.shortestSide / 2.0f)
            );

            canvas.drawRRect(rrect, this._shadowPaint);
            canvas.drawRRect(rrect.shift(new Offset(0.0f, 3.0f)), this._shadowPaint);
            var _paint = new Paint();
            _paint.color = this.color;
            canvas.drawRRect(rrect, _paint);
        }
    }
}