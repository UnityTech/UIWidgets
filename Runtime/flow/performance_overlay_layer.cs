using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.flow {
    public class PerformanceOverlayLayer : Layer {
        public PerformanceOverlayLayer(int options) {
            this._options = options;
        }

        readonly int _options;

        public override void paint(PaintContext context) {
            D.assert(this.needsPainting);
            const int padding = 8;
            const int fpsHeight = 20;

            Canvas canvas = context.canvas;
            canvas.save();

            float x = this.paintBounds.left + padding;
            float y = this.paintBounds.top + padding;
            float width = this.paintBounds.width - padding * 2;
            float height = this.paintBounds.height;

            this._drawFPS(canvas, x, y);

            if ((this._options & (int) PerformanceOverlayOption.drawFrameCost) == 1) {
                this._drawFrameCost(canvas, x, y + fpsHeight, width, height - padding - fpsHeight);
            }

            canvas.restore();
        }


        void _drawFPS(Canvas canvas, float x, float y) {
            var pb = new ParagraphBuilder(new ParagraphStyle { });
            pb.addText("FPS = " + PerformanceUtils.instance.getFPS());
            var paragraph = pb.build();
            paragraph.layout(new ParagraphConstraints(width: 300));

            canvas.drawParagraph(paragraph, new Offset(x, y));
        }

        void _drawFrameCost(Canvas canvas, float x, float y, float width, float height) {
            Rect visualizationRect = Rect.fromLTWH(x, y, width, height);

            Paint paint = new Paint {color = Colors.blue};
            Paint paint2 = new Paint {color = Colors.red};
            Paint paint3 = new Paint {color = Colors.green};
            Paint paint4 = new Paint {color = Colors.white70};

            float[] costFrames = PerformanceUtils.instance.getFrames();
            int curFrame = PerformanceUtils.instance.getCurFrame();

            float barWidth = Mathf.Max(1, width / costFrames.Length);
            float perHeight = height / 32.0f;

            canvas.drawRect(visualizationRect, paint4);
            canvas.drawRect(Rect.fromLTWH(x, y + perHeight * 16.0f, width, 1), paint3);

            float cur_x = x;
            Path barPath = new Path();

            for (var i = 0; i < costFrames.Length; i++) {
                if (costFrames[i] != 0) {
                    float curHeight = Mathf.Min(perHeight * costFrames[i], height);
                    Rect barRect = Rect.fromLTWH(cur_x, y + height - curHeight, barWidth, curHeight);
                    barPath.addRect(barRect);
                }

                cur_x += barWidth;
            }

            canvas.drawPath(barPath, paint);
            if (curFrame >= 0 && curFrame < costFrames.Length && costFrames[curFrame] != 0) {
                float curHeight = Mathf.Min(perHeight * costFrames[curFrame], height);
                Rect barRect = Rect.fromLTWH(x + barWidth * curFrame, y + height - curHeight, barWidth, curHeight);
                canvas.drawRect(barRect, paint2);

                var pb = new ParagraphBuilder(new ParagraphStyle { });
                pb.addText("Frame Cost: " + costFrames[curFrame] + "ms");
                var paragraph = pb.build();
                paragraph.layout(new ParagraphConstraints(width: 300));

                canvas.drawParagraph(paragraph, new Offset(x, y + height - 12));
            }
        }
    }
}