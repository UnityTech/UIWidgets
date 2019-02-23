using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public interface CustomPainter : Listenable {
        void paint(Canvas canvas, Size size);

        bool shouldRepaint(CustomPainter oldDelegate);

        bool? hitTest(Offset position);
    }

    public abstract class AbstractCustomPainter : CustomPainter {
        public AbstractCustomPainter(Listenable repaint = null) {
            this._repaint = repaint;
        }

        readonly Listenable _repaint;

        public void addListener(VoidCallback listener) {
            this._repaint?.addListener(listener);
        }

        public void removeListener(VoidCallback listener) {
            this._repaint?.removeListener(listener);
        }

        public abstract void paint(Canvas canvas, Size size);

        public abstract bool shouldRepaint(CustomPainter oldDelegate);

        public virtual bool? hitTest(Offset position) {
            return null;
        }

        public override string ToString() {
            return $"{Diagnostics.describeIdentity(this)}({this._repaint?.ToString() ?? ""})";
        }
    }

    public class RenderCustomPaint : RenderProxyBox {
        public RenderCustomPaint(
            CustomPainter painter = null,
            CustomPainter foregroundPainter = null,
            Size preferredSize = null,
            bool isComplex = false,
            bool willChange = false,
            RenderBox child = null
        ) : base(child) {
            preferredSize = preferredSize ?? Size.zero;
            this.preferredSize = preferredSize;
            this._painter = painter;
            this._foregroundPainter = foregroundPainter;
            this.isComplex = isComplex;
            this.willChange = willChange;
        }

        CustomPainter _painter;

        public CustomPainter painter {
            get { return this._painter; }
            set {
                if (this._painter == value) {
                    return;
                }

                CustomPainter oldPainter = this._painter;
                this._painter = value;
                this._didUpdatePainter(this._painter, oldPainter);
            }
        }

        CustomPainter _foregroundPainter;

        public CustomPainter foregroundPainter {
            get { return this._foregroundPainter; }
            set {
                if (this._foregroundPainter == value) {
                    return;
                }

                CustomPainter oldPainter = this._foregroundPainter;
                this._foregroundPainter = value;
                this._didUpdatePainter(this._foregroundPainter, oldPainter);
            }
        }

        void _didUpdatePainter(CustomPainter newPainter, CustomPainter oldPainter) {
            if (newPainter == null) {
                D.assert(oldPainter != null);
                this.markNeedsPaint();
            }
            else if (oldPainter == null ||
                     newPainter.GetType() != oldPainter.GetType() ||
                     newPainter.shouldRepaint(oldPainter)) {
                this.markNeedsPaint();
            }

            if (this.attached) {
                oldPainter?.removeListener(this.markNeedsPaint);
                newPainter?.addListener(this.markNeedsPaint);
            }
        }

        Size _preferredSize;

        public Size preferredSize {
            get { return this._preferredSize; }
            set {
                D.assert(value != null);
                if (this.preferredSize == value) {
                    return;
                }

                this._preferredSize = value;
                this.markNeedsLayout();
            }
        }

        public bool isComplex;

        public bool willChange;

        public override void attach(object owner) {
            base.attach(owner);
            this._painter?.addListener(this.markNeedsPaint);
            this._foregroundPainter?.addListener(this.markNeedsPaint);
        }

        public override void detach() {
            this._painter?.removeListener(this.markNeedsPaint);
            this._foregroundPainter?.removeListener(this.markNeedsPaint);
            base.detach();
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position) {
            if (this._foregroundPainter != null && ((this._foregroundPainter.hitTest(position)) ?? false)) {
                return true;
            }

            return base.hitTestChildren(result, position: position);
        }


        protected override bool hitTestSelf(Offset position) {
            return this._painter != null && (this._painter.hitTest(position) ?? true);
        }

        protected override void performResize() {
            this.size = this.constraints.constrain(this.preferredSize);
        }

        void _paintWithPainter(Canvas canvas, Offset offset, CustomPainter painter) {
            int debugPreviousCanvasSaveCount = 0;
            canvas.save();
            D.assert(() => {
                debugPreviousCanvasSaveCount = canvas.getSaveCount();
                return true;
            });
            if (offset != Offset.zero) {
                canvas.translate(offset.dx, offset.dy);
            }

            painter.paint(canvas, this.size);
            D.assert(() => {
                int debugNewCanvasSaveCount = canvas.getSaveCount();
                if (debugNewCanvasSaveCount > debugPreviousCanvasSaveCount) {
                    throw new UIWidgetsError(
                        $"{debugNewCanvasSaveCount - debugPreviousCanvasSaveCount} more " +
                        $"time{((debugNewCanvasSaveCount - debugPreviousCanvasSaveCount == 1) ? "" : "s")} " +
                        "than it called canvas.restore().\n" +
                        "This leaves the canvas in an inconsistent state and will probably result in a broken display.\n" +
                        "You must pair each call to save()/saveLayer() with a later matching call to restore()."
                    );
                }

                if (debugNewCanvasSaveCount < debugPreviousCanvasSaveCount) {
                    throw new UIWidgetsError(
                        $"The {painter} custom painter called canvas.restore() " +
                        $"{debugPreviousCanvasSaveCount - debugNewCanvasSaveCount} more " +
                        $"time{(debugPreviousCanvasSaveCount - debugNewCanvasSaveCount == 1 ? "" : "s")} " +
                        "than it called canvas.save() or canvas.saveLayer().\n" +
                        "This leaves the canvas in an inconsistent state and will result in a broken display.\n" +
                        "You should only call restore() if you first called save() or saveLayer()."
                    );
                }

                return debugNewCanvasSaveCount == debugPreviousCanvasSaveCount;
            });
            canvas.restore();
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._painter != null) {
                this._paintWithPainter(context.canvas, offset, this._painter);
                this._setRasterCacheHints(context);
            }

            base.paint(context, offset);
            if (this._foregroundPainter != null) {
                this._paintWithPainter(context.canvas, offset, this._foregroundPainter);
                this._setRasterCacheHints(context);
            }
        }

        void _setRasterCacheHints(PaintingContext context) {
            if (this.isComplex) {
                context.setIsComplexHint();
            }

            if (this.willChange) {
                context.setWillChangeHint();
            }
        }
    }
}