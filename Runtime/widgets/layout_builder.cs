using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate Widget LayoutWidgetBuilder(BuildContext context, BoxConstraints constraints);

    public class LayoutBuilder : RenderObjectWidget {
        public LayoutBuilder(
            Key key = null,
            LayoutWidgetBuilder builder = null) : base(key: key) {
            D.assert(builder != null);
            this.builder = builder;
        }

        public readonly LayoutWidgetBuilder builder;

        public override Element createElement() {
            return new _LayoutBuilderElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderLayoutBuilder();
        }
    }

    class _LayoutBuilderElement : RenderObjectElement {
        public _LayoutBuilderElement(
            LayoutBuilder widget) : base(widget) {
        }

        new LayoutBuilder widget {
            get { return (LayoutBuilder) base.widget; }
        }

        new _RenderLayoutBuilder renderObject {
            get { return (_RenderLayoutBuilder) base.renderObject; }
        }

        Element _child;

        public override void visitChildren(ElementVisitor visitor) {
            if (this._child != null) {
                visitor(this._child);
            }
        }

        protected override void forgetChild(Element child) {
            D.assert(child == this._child);
            this._child = null;
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            this.renderObject.callback = this._layout;
        }

        public override void update(Widget newWidget) {
            D.assert(this.widget != newWidget);
            base.update(newWidget);
            D.assert(this.widget == newWidget);
            this.renderObject.callback = this._layout;
            this.renderObject.markNeedsLayout();
        }

        protected override void performRebuild() {
            this.renderObject.markNeedsLayout();
            base.performRebuild();
        }

        public override void unmount() {
            this.renderObject.callback = null;
            base.unmount();
        }

        void _layout(BoxConstraints constraints) {
            this.owner.buildScope(this, () => {
                Widget built = null;
                if (this.widget.builder != null) {
                    built = this.widget.builder(this, constraints);
                    WidgetsD.debugWidgetBuilderValue(this.widget, built);
                }

                this._child = this.updateChild(this._child, built, null);
                D.assert(this._child != null);
            });
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            _RenderLayoutBuilder renderObject = this.renderObject;
            D.assert(slot == null);
            D.assert(renderObject.debugValidateChild(child));
            renderObject.child = (RenderBox) child;
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            _RenderLayoutBuilder renderObject = this.renderObject;
            D.assert(renderObject.child == child);
            renderObject.child = null;
            D.assert(renderObject == this.renderObject);
        }
    }


    public class _RenderLayoutBuilder : RenderObjectWithChildMixinRenderBox<RenderBox> {
        public _RenderLayoutBuilder(
            LayoutCallback<BoxConstraints> callback = null) {
            this._callback = callback;
        }

        public LayoutCallback<BoxConstraints> callback {
            get { return this._callback; }
            set {
                if (value == this._callback) {
                    return;
                }

                this._callback = value;
                this.markNeedsLayout();
            }
        }

        LayoutCallback<BoxConstraints> _callback;

        bool _debugThrowIfNotCheckingIntrinsics() {
            D.assert(() => {
                if (!debugCheckingIntrinsics) {
                    throw new UIWidgetsError(
                        "LayoutBuilder does not support returning intrinsic dimensions.\n" +
                        "Calculating the intrinsic dimensions would require running the layout " +
                        "callback speculatively, which might mutate the live render object tree."
                    );
                }

                return true;
            });
            return true;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            D.assert(this._debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            D.assert(this._debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            D.assert(this._debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            D.assert(this._debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected override void performLayout() {
            D.assert(this.callback != null);
            this.invokeLayoutCallback(this.callback);
            if (this.child != null) {
                this.child.layout(this.constraints, parentUsesSize: true);
                this.size = this.constraints.constrain(this.child.size);
            }
            else {
                this.size = this.constraints.biggest;
            }
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            return this.child?.hitTest(result, position: position) ?? false;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                context.paintChild(this.child, offset);
            }
        }
    }
}