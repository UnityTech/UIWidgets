using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.plugins.raycast {
    class RaycastableBox : SingleChildRenderObjectWidget {
        public RaycastableBox(
            Key key = null,
            Widget child = null
        ) : base(key, child) {
            this.windowHashCode = Window.instance.GetHashCode();
        }

        readonly int windowHashCode;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderRaycastableBox(
                windowHashCode: this.windowHashCode,
                widget: this
            );
        }

        public override Element createElement() {
            return new _RaycastableBoxRenderElement(this.windowHashCode, this);
        }
    }

    class RenderRaycastableBox : RenderProxyBox {
        public RenderRaycastableBox(
            int windowHashCode,
            RenderBox child = null,
            RaycastableBox widget = null
        ) : base(child) {
            this.widget = widget;
            this.windowHashCode = windowHashCode;
        }

        readonly int windowHashCode;
        RaycastableBox widget;

        public override void detach() {
            base.detach();
            this.markNeedsPaint();
        }


        public override void paint(PaintingContext context, Offset offset) {
            // Debug.Log($"[RenderRaycastableBox] Paint {this.widget.GetHashCode()}: {this.size}@{offset}");
            RaycastManager.UpdateSizeOffset(this.widget.GetHashCode(), (int) this.windowHashCode, this.size, offset);

            base.paint(context, offset);
        }
    }

    class _RaycastableBoxRenderElement : SingleChildRenderObjectElement {
        public _RaycastableBoxRenderElement(
            int windowHashCode,
            RaycastableBox widget
        ) : base(widget) {
            this.windowHashCode = windowHashCode;
        }

        public new RaycastableBox widget {
            get { return base.widget as RaycastableBox; }
        }

        int widgetHashCode;
        int windowHashCode;

        public override void mount(Element parent, object newSlot) {
            this.widgetHashCode = this.widget.GetHashCode();

            // Debug.Log($"[RaycastableBox] Mount: {this.initHashCode}");
            RaycastManager.AddToList(this.widgetHashCode, this.windowHashCode);
            base.mount(parent, newSlot);
        }

        public override void update(Widget newWidget) {
            // Debug.Log($"[RaycastableBox] Update: {this.initHashCode}");
            RaycastManager.MarkDirty(this.widgetHashCode, this.windowHashCode);
            base.update(newWidget);
        }

        public override void unmount() {
            // Debug.Log($"[RaycastableBox] Unmount: {this.initHashCode}");
            RaycastManager.RemoveFromList(this.widgetHashCode, this.windowHashCode);
            base.unmount();
        }
    }

    public class RaycastableContainer : StatelessWidget {
        public RaycastableContainer(
            Widget child = null,
            Key key = null
        ) : base(key) {
            this.child = child;
        }

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            Widget current = this.child;

            if (this.child == null) {
                current = new LimitedBox(
                    maxWidth: 0.0f,
                    maxHeight: 0.0f,
                    child: new ConstrainedBox(constraints: BoxConstraints.expand())
                );
            }

            current = new RaycastableBox(child: current);

            return current;
        }
    }
}