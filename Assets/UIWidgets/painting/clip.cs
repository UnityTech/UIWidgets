using System;
using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.painting {
    public abstract class ClipContext {
        public abstract Canvas canvas { get; }

        void _clipAndPaint(Action<bool> canvasClipCall, Clip clipBehavior, Rect bounds, Action painter) {
            D.assert(canvasClipCall != null);

            this.canvas.save();

            switch (clipBehavior) {
                case Clip.none:
                    break;
                case Clip.hardEdge:
                    canvasClipCall(false);
                    break;
                case Clip.antiAlias:
                    canvasClipCall(true);
                    break;
                case Clip.antiAliasWithSaveLayer:
                    canvasClipCall(true);
                    this.canvas.saveLayer(bounds, new Paint());
                    break;
            }

            painter();

            if (clipBehavior == Clip.antiAliasWithSaveLayer) {
                this.canvas.restore();
            }

            this.canvas.restore();
        }

        public void clipRRectAndPaint(RRect rrect, Clip clipBehavior, Rect bounds, Action painter) {
            this._clipAndPaint(doAntiAias => this.canvas.clipRRect(rrect, doAntiAlias: doAntiAias),
                clipBehavior, bounds, painter);
        }

        public void clipRectAndPaint(Rect rect, Clip clipBehavior, Rect bounds, Action painter) {
            this._clipAndPaint(doAntiAias => this.canvas.clipRect(rect, doAntiAlias: doAntiAias),
                clipBehavior, bounds, painter);
        }
    }
}