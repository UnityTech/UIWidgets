using UIWidgets.foundation;
using UIWidgets.rendering;
using UIWidgets.painting;

namespace UIWidgets.widgets {
    public class DecoratedBox : SingleChildRenderObjectWidget {
        public DecoratedBox(
            Decoration decoration,
            Widget child,
            Key key = null,
            DecorationPosition position = DecorationPosition.background
        ) : base(key, child) {
            this.position = position;
            this.decoration = decoration;
        }

        public Decoration decoration;

        public DecorationPosition position;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderDecoratedBox(
                decoration: decoration,
                position: position,
                configuration: ImageUtil.createLocalImageConfiguration(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderDecoratedBox) renderObject).decoration = decoration;
            ((RenderDecoratedBox) renderObject).configuration = ImageUtil.createLocalImageConfiguration(context);
            ((RenderDecoratedBox) renderObject).position = position;
        }
    }

    public class Container : StatelessWidget {
        // todo transform
        public Container(
            Key key = null,
            Alignment alignment = null,
            EdgeInsets padding = null,
            ui.Color color = null,
            Decoration decoration = null,
            Decoration forgroundDecoration = null,
            double width = 0.0,
            double height = 0.0,
            BoxConstraints constraints = null,
            EdgeInsets margin = null,
//            Matrix4x4 transfrom = default(Matrix4x4),
            Widget child = null
        ) : base(key) {
            this.alignment = alignment;
            this.foregroundDecoration = forgroundDecoration;
//            this.transform = transfrom;
            this.margin = margin;
            this.child = child;
            this.padding = padding;

            this.decoration = decoration ?? (color != null ? new BoxDecoration(color) : null);
            this.constraints = (width != 0.0 || height != 0.0)
                ? ((constraints == null ? null : constraints.tighten(width, height))
                   ?? BoxConstraints.tightFor(width, height))
                : constraints;
        }

        public Widget child;
        public Alignment alignment;
        public EdgeInsets padding;
        public Decoration decoration;
        public Decoration foregroundDecoration;
        public BoxConstraints constraints;
        public EdgeInsets margin;
//        public Matrix4x4 transform;

        EdgeInsets _paddingIncludingDecoration {
            get {
                if (decoration == null || decoration.padding == null)
                    return padding;
                EdgeInsets decorationPadding = decoration.padding;
                if (padding == null)
                    return decorationPadding;
                return padding.add(decorationPadding);
            }
        }

        public override Widget build(BuildContext context) {
            Widget current = child;

            if (child == null && (constraints == null || !constraints.isTight)) {
                current = new LimitedBox(
                    maxWidth: 0.0,
                    maxHeight: 0.0,
                    child: new ConstrainedBox(constraints: BoxConstraints.expand())
                );
            }

            if (alignment != null) {
                current = new Align(alignment: alignment, child: current);
            }

            EdgeInsets effetivePadding = _paddingIncludingDecoration;
            if (effetivePadding != null) {
                current = new Padding(padding: effetivePadding, child: current);
            }

            if (decoration != null) {
                current = new DecoratedBox(decoration: decoration, child: current);
            }

            if (foregroundDecoration != null) {
                current = new DecoratedBox(
                    decoration: decoration,
                    position: DecorationPosition.foreground,
                    child: current
                );
            }

            if (constraints != null) {
                current = new ConstrainedBox(constraints: constraints, child: current);
            }

            if (margin != null) {
                current = new Padding(padding: margin, child: current);
            }

//            if (transform != null) {
//                current = new Transform(transform: transform, child: current);
//            }

            return current;
        }
    }
}