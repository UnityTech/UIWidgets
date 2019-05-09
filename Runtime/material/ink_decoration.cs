using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using ImageUtils = Unity.UIWidgets.widgets.ImageUtils;

namespace Unity.UIWidgets.material {
    public class Ink : StatefulWidget {
        public Ink(
            Key key = null,
            EdgeInsets padding = null,
            Color color = null,
            Decoration decoration = null,
            float? width = null,
            float? height = null,
            Widget child = null) : base(key: key) {
            D.assert(padding == null || padding.isNonNegative);
            D.assert(decoration == null || decoration.debugAssertIsValid());
            D.assert(color == null || decoration == null,
                () => "Cannot provide both a color and a decoration\n" +
                "The color argument is just a shorthand for \"decoration: new BoxDecoration(color: color)\".");
            decoration = decoration ?? (color != null ? new BoxDecoration(color: color) : null);
            this.padding = padding;
            this.width = width;
            this.height = height;
            this.child = child;
            this.decoration = decoration;
        }

        public static Ink image(
            Key key = null,
            EdgeInsets padding = null,
            ImageProvider image = null,
            ColorFilter colorFilter = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            Rect centerSlice = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            float? width = null,
            float? height = null,
            Widget child = null
        ) {
            D.assert(padding == null || padding.isNonNegative);
            D.assert(image != null);

            alignment = alignment ?? Alignment.center;
            Decoration decoration = new BoxDecoration(
                image: new DecorationImage(
                    image: image,
                    colorFilter: colorFilter,
                    fit: fit,
                    alignment: alignment,
                    centerSlice: centerSlice,
                    repeat: repeat)
            );

            return new Ink(
                key: key,
                padding: padding,
                decoration: decoration,
                width: width,
                height: height,
                child: child);
        }


        public readonly Widget child;

        public readonly EdgeInsets padding;

        public readonly Decoration decoration;

        public readonly float? width;

        public readonly float? height;

        public EdgeInsets _paddingIncludingDecoration {
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

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding, defaultValue: null));
            properties.add(new DiagnosticsProperty<Decoration>("bg", this.decoration, defaultValue: null));
        }

        public override State createState() {
            return new _InkState();
        }
    }


    class _InkState : State<Ink> {
        InkDecoration _ink;

        void _handleRemoved() {
            this._ink = null;
        }

        public override void deactivate() {
            this._ink?.dispose();
            D.assert(this._ink == null);
            base.deactivate();
        }

        public Widget _build(BuildContext context, BoxConstraints constraints) {
            if (this._ink == null) {
                this._ink = new InkDecoration(
                    decoration: this.widget.decoration,
                    configuration: ImageUtils.createLocalImageConfiguration(context),
                    controller: Material.of(context),
                    referenceBox: (RenderBox) context.findRenderObject(),
                    onRemoved: this._handleRemoved
                );
            }
            else {
                this._ink.decoration = this.widget.decoration;
                this._ink.configuration = ImageUtils.createLocalImageConfiguration(context);
            }

            Widget current = this.widget.child;
            EdgeInsets effectivePadding = this.widget._paddingIncludingDecoration;
            if (effectivePadding != null) {
                current = new Padding(
                    padding: effectivePadding,
                    child: current);
            }

            return current;
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            Widget result = new LayoutBuilder(
                builder: this._build
            );
            if (this.widget.width != null || this.widget.height != null) {
                result = new SizedBox(
                    width: this.widget.width,
                    height: this.widget.height,
                    child: result);
            }

            return result;
        }
    }


    class InkDecoration : InkFeature {
        public InkDecoration(
            Decoration decoration = null,
            ImageConfiguration configuration = null,
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            VoidCallback onRemoved = null
        ) : base(controller: controller, referenceBox: referenceBox, onRemoved: onRemoved) {
            D.assert(configuration != null);
            D.assert(decoration != null);
            D.assert(controller != null);
            D.assert(referenceBox != null);
            this._configuration = configuration;
            this.decoration = decoration;
            this.controller.addInkFeature(this);
        }

        BoxPainter _painter;

        public Decoration decoration {
            get { return this._decoration; }
            set {
                if (value == this._decoration) {
                    return;
                }

                this._decoration = value;
                this._painter?.Dispose();
                this._painter = this._decoration?.createBoxPainter(this._handleChanged);
                this.controller.markNeedsPaint();
            }
        }

        Decoration _decoration;

        public ImageConfiguration configuration {
            get { return this._configuration; }
            set {
                D.assert(value != null);
                if (value == this._configuration) {
                    return;
                }

                this._configuration = value;
                this.controller.markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        void _handleChanged() {
            this.controller.markNeedsPaint();
        }

        public override void dispose() {
            this._painter?.Dispose();
            base.dispose();
        }

        protected override void paintFeature(Canvas canvas, Matrix3 transform) {
            if (this._painter == null) {
                return;
            }

            Offset originOffset = transform.getAsTranslation();
            ImageConfiguration sizedConfiguration = this.configuration.copyWith(
                size: this.referenceBox.size);

            if (originOffset == null) {
                canvas.save();
                canvas.concat(transform);
                this._painter.paint(canvas, Offset.zero, sizedConfiguration);
                canvas.restore();
            }
            else {
                this._painter.paint(canvas, originOffset, sizedConfiguration);
            }
        }
    }
}