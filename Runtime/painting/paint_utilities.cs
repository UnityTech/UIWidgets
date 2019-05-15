using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;

namespace Unity.UIWidgets.painting {
    public static class PaintingUtilities {
        public static void paintZigZag(Canvas canvas, Paint paint, Offset start, Offset end, int zigs, float width) {
            D.assert(MathUtils.isFinite(zigs));
            D.assert(zigs > 0);
            canvas.save();
            canvas.translate(start.dx, start.dy);
            end = end - start;
            canvas.rotate(Mathf.Atan2(end.dy, end.dx));
            float length = end.distance;
            float spacing = length / (zigs * 2.0f);
            Path path = new Path();
            path.moveTo(0.0f, 0.0f);
            for (int index = 0; index < zigs; index += 1) {
                float x = (index * 2.0f + 1.0f) * spacing;
                float y = width * ((index % 2.0f) * 2.0f - 1.0f);
                path.lineTo(x, y);
            }

            path.lineTo(length, 0.0f);
            canvas.drawPath(path, paint);
            canvas.restore();
        }
    }
}