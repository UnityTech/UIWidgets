using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

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
                context.frameTime.visualize(canvas,
                    Rect.fromLTWH(x, y + fpsHeight, width, height - padding - fpsHeight));
            }

            canvas.restore();
        }


        void _drawFPS(Canvas canvas, float x, float y) {
            var pb = new ParagraphBuilder(new ParagraphStyle { });
            pb.addText("FPS = " + Window.instance.getFPS());
            var paragraph = pb.build();
            paragraph.layout(new ParagraphConstraints(width: 300));

            canvas.drawParagraph(paragraph, new Offset(x, y));
            Paragraph.release(ref paragraph);
        }
    }
}