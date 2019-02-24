using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public class Viewport : MultiChildRenderObjectWidget {
        public Viewport(
            Key key = null,
            AxisDirection axisDirection = AxisDirection.down,
            AxisDirection? crossAxisDirection = null,
            float anchor = 0.0f,
            ViewportOffset offset = null,
            Key center = null,
            float? cacheExtent = null,
            List<Widget> slivers = null
        ) : base(key: key, children: slivers) {
            D.assert(offset != null);
            D.assert(center == null || this.children.Count(child => child.key == center) == 1);

            this.axisDirection = axisDirection;
            this.crossAxisDirection = crossAxisDirection;
            this.anchor = anchor;
            this.offset = offset;
            this.center = center;
            this.cacheExtent = cacheExtent;
        }

        public readonly AxisDirection axisDirection;

        public readonly AxisDirection? crossAxisDirection;

        public readonly float anchor;

        public readonly ViewportOffset offset;

        public readonly Key center;

        public readonly float? cacheExtent;

        public static AxisDirection getDefaultCrossAxisDirection(BuildContext context, AxisDirection axisDirection) {
            switch (axisDirection) {
                case AxisDirection.up:
                    return AxisUtils.textDirectionToAxisDirection(Directionality.of(context));
                case AxisDirection.right:
                    return AxisDirection.down;
                case AxisDirection.down:
                    return AxisUtils.textDirectionToAxisDirection(Directionality.of(context));
                case AxisDirection.left:
                    return AxisDirection.down;
            }


            throw new Exception("unknown axisDirection");
        }


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderViewport(
                axisDirection: this.axisDirection,
                crossAxisDirection: this.crossAxisDirection ??
                                    getDefaultCrossAxisDirection(context, this.axisDirection),
                anchor: this.anchor,
                offset: this.offset,
                cacheExtent: this.cacheExtent ?? RenderViewportUtils.defaultCacheExtent
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderViewport) renderObjectRaw;
            renderObject.axisDirection = this.axisDirection;
            renderObject.crossAxisDirection = this.crossAxisDirection ??
                                              getDefaultCrossAxisDirection(context, this.axisDirection);
            renderObject.anchor = this.anchor;
            renderObject.offset = this.offset;
            renderObject.cacheExtent = this.cacheExtent ?? RenderViewportUtils.defaultCacheExtent;
        }

        public override Element createElement() {
            return new _ViewportElement(this);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection>("axisDirection", this.axisDirection));
            properties.add(new EnumProperty<AxisDirection?>("crossAxisDirection", this.crossAxisDirection,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new FloatProperty("anchor", this.anchor));
            properties.add(new DiagnosticsProperty<ViewportOffset>("offset", this.offset));
            if (this.center != null) {
                properties.add(new DiagnosticsProperty<Key>("center", this.center));
            }
            else if (this.children.isNotEmpty() && this.children.First().key != null) {
                properties.add(new DiagnosticsProperty<Key>("center", this.children.First().key, tooltip: "implicit"));
            }
        }
    }


    class _ViewportElement : MultiChildRenderObjectElement {
        internal _ViewportElement(Viewport widget) : base(widget) {
        }

        public new Viewport widget {
            get { return (Viewport) base.widget; }
        }

        public new RenderViewport renderObject {
            get { return (RenderViewport) base.renderObject; }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            this._updateCenter();
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            this._updateCenter();
        }

        void _updateCenter() {
            if (this.widget.center != null) {
                this.renderObject.center = (RenderSliver) this.children.Single(
                    element => element.widget.key == this.widget.center).renderObject;
            }
            else if (this.children.Any()) {
                this.renderObject.center = (RenderSliver) this.children.First().renderObject;
            }
            else {
                this.renderObject.center = null;
            }
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            this.children.Where(e => {
                RenderSliver renderSliver = (RenderSliver) e.renderObject;
                return renderSliver.geometry.visible;
            }).ToList().ForEach(e => visitor(e));
        }
    }


    public class ShrinkWrappingViewport : MultiChildRenderObjectWidget {
        public ShrinkWrappingViewport(
            Key key = null,
            AxisDirection axisDirection = AxisDirection.down,
            AxisDirection? crossAxisDirection = null,
            ViewportOffset offset = null,
            List<Widget> slivers = null
        ) : base(key: key, children: slivers) {
            D.assert(offset != null);

            this.axisDirection = axisDirection;
            this.crossAxisDirection = crossAxisDirection;
            this.offset = offset;
        }

        public readonly AxisDirection axisDirection;

        public readonly AxisDirection? crossAxisDirection;

        public readonly ViewportOffset offset;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderShrinkWrappingViewport(
                axisDirection: this.axisDirection,
                crossAxisDirection: this.crossAxisDirection
                                    ?? Viewport.getDefaultCrossAxisDirection(context, this.axisDirection),
                offset: this.offset
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderShrinkWrappingViewport) renderObjectRaw;
            renderObject.axisDirection = this.axisDirection;
            renderObject.crossAxisDirection = this.crossAxisDirection
                                              ?? Viewport.getDefaultCrossAxisDirection(context, this.axisDirection);
            renderObject.offset = this.offset;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection>("axisDirection", this.axisDirection));
            properties.add(new EnumProperty<AxisDirection?>("crossAxisDirection", this.crossAxisDirection,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<ViewportOffset>("offset", this.offset));
        }
    }
}