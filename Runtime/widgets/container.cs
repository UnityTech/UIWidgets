using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class DecoratedBox : SingleChildRenderObjectWidget {
        public DecoratedBox(
            Key key = null,
            Decoration decoration = null,
            DecorationPosition position = DecorationPosition.background,
            Widget child = null
        ) : base(key, child) {
            D.assert(decoration != null);
            this.position = position;
            this.decoration = decoration;
        }

        public readonly Decoration decoration;

        public readonly DecorationPosition position;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderDecoratedBox(
                decoration: this.decoration,
                position: this.position,
                configuration: ImageUtils.createLocalImageConfiguration(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderDecoratedBox) renderObjectRaw;
            renderObject.decoration = this.decoration;
            renderObject.configuration = ImageUtils.createLocalImageConfiguration(context);
            renderObject.position = this.position;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            string label = "decoration";
            switch (this.position) {
                case DecorationPosition.background:
                    label = "bg";
                    break;
                case DecorationPosition.foreground:
                    label = "fg";
                    break;
            }

            properties.add(new EnumProperty<DecorationPosition>(
                "position", this.position, level: DiagnosticLevel.hidden));
            properties.add(new DiagnosticsProperty<Decoration>(
                label,
                this.decoration,
                ifNull: "no decoration",
                showName: this.decoration != null
            ));
        }
    }

    public class Container : StatelessWidget {
        public Container(
            Key key = null,
            Alignment alignment = null,
            EdgeInsets padding = null,
            Color color = null,
            Decoration decoration = null,
            Decoration forgroundDecoration = null,
            float? width = null,
            float? height = null,
            BoxConstraints constraints = null,
            EdgeInsets margin = null,
            Matrix3 transfrom = null,
            Widget child = null
        ) : base(key) {
            D.assert(margin == null || margin.isNonNegative);
            D.assert(padding == null || padding.isNonNegative);
            D.assert(decoration == null || decoration.debugAssertIsValid());
            D.assert(constraints == null || constraints.debugAssertIsValid());
            D.assert(color == null || decoration == null,
                () => "Cannot provide both a color and a decoration\n" +
                "The color argument is just a shorthand for \"decoration: new BoxDecoration(color: color)\"."
            );

            this.alignment = alignment;
            this.padding = padding;
            this.foregroundDecoration = forgroundDecoration;
            this.margin = margin;
            this.transform = transfrom;
            this.child = child;

            this.decoration = decoration ?? (color != null ? new BoxDecoration(color) : null);
            this.constraints = (width != null || height != null)
                ? (constraints != null ? constraints.tighten(width, height) : BoxConstraints.tightFor(width, height))
                : constraints;
        }

        public readonly Widget child;
        public readonly Alignment alignment;
        public readonly EdgeInsets padding;
        public readonly Decoration decoration;
        public readonly Decoration foregroundDecoration;
        public readonly BoxConstraints constraints;
        public readonly EdgeInsets margin;
        public readonly Matrix3 transform;

        EdgeInsets _paddingIncludingDecoration {
            get {
                if (this.decoration == null || this.decoration.padding == null) {
                    return this.padding;
                }

                EdgeInsets decorationPadding = this.decoration.padding;
                if (this.padding == null) {
                    return decorationPadding;
                }

                return this.padding.add(decorationPadding);
            }
        }

        public override Widget build(BuildContext context) {
            Widget current = this.child;

            if (this.child == null && (this.constraints == null || !this.constraints.isTight)) {
                current = new LimitedBox(
                    maxWidth: 0.0f,
                    maxHeight: 0.0f,
                    child: new ConstrainedBox(constraints: BoxConstraints.expand())
                );
            }

            if (this.alignment != null) {
                current = new Align(alignment: this.alignment, child: current);
            }

            EdgeInsets effetivePadding = this._paddingIncludingDecoration;
            if (effetivePadding != null) {
                current = new Padding(padding: effetivePadding, child: current);
            }

            if (this.decoration != null) {
                current = new DecoratedBox(decoration: this.decoration, child: current);
            }

            if (this.foregroundDecoration != null) {
                current = new DecoratedBox(
                    decoration: this.foregroundDecoration,
                    position: DecorationPosition.foreground,
                    child: current
                );
            }

            if (this.constraints != null) {
                current = new ConstrainedBox(constraints: this.constraints, child: current);
            }

            if (this.margin != null) {
                current = new Padding(padding: this.margin, child: current);
            }

            if (this.transform != null) {
                current = new Transform(transform: new Matrix3(this.transform), child: current);
            }

            return current;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Alignment>("alignment",
                this.alignment, showName: false, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding",
                this.padding, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Decoration>("bg",
                this.decoration, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Decoration>("fg",
                this.foregroundDecoration, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BoxConstraints>("constraints",
                this.constraints, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<EdgeInsets>("margin",
                this.margin, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(ObjectFlagProperty<Matrix3>.has("transform",
                this.transform));
        }
    }
}