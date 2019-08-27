using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    static class MaterialConstantsUtils {
        public static readonly Dictionary<MaterialType, BorderRadius> kMaterialEdges =
            new Dictionary<MaterialType, BorderRadius> {
                {MaterialType.canvas, null},
                {MaterialType.card, BorderRadius.circular(2.0f)},
                {MaterialType.circle, null},
                {MaterialType.button, BorderRadius.circular(2.0f)},
                {MaterialType.transparency, null}
            };
    }


    public delegate Rect RectCallback();

    public enum MaterialType {
        canvas,
        card,
        circle,
        button,
        transparency
    }


    public interface MaterialInkController {
        Color color { get; set; }

        TickerProvider vsync { get; }

        void addInkFeature(InkFeature feature);

        void markNeedsPaint();
    }


    public class Material : StatefulWidget {
        public Material(
            Key key = null,
            MaterialType type = MaterialType.canvas,
            float elevation = 0.0f,
            Color color = null,
            Color shadowColor = null,
            TextStyle textStyle = null,
            BorderRadius borderRadius = null,
            ShapeBorder shape = null,
            bool borderOnForeground = true,
            Clip clipBehavior = Clip.none,
            TimeSpan? animationDuration = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(elevation >= 0.0f);
            D.assert(!(shape != null && borderRadius != null));
            D.assert(!(type == MaterialType.circle && (borderRadius != null || shape != null)));

            this.type = type;
            this.elevation = elevation;
            this.color = color;
            this.shadowColor = shadowColor ?? new Color(0xFF000000);
            this.textStyle = textStyle;
            this.borderRadius = borderRadius;
            this.shape = shape;
            this.borderOnForeground = borderOnForeground;
            this.clipBehavior = clipBehavior;
            this.animationDuration = animationDuration ?? Constants.kThemeChangeDuration;
            this.child = child;
        }

        public readonly Widget child;

        public readonly MaterialType type;

        public readonly float elevation;

        public readonly Color color;

        public readonly Color shadowColor;

        public readonly TextStyle textStyle;

        public readonly ShapeBorder shape;

        public readonly bool borderOnForeground;

        public readonly Clip clipBehavior;

        public readonly TimeSpan animationDuration;

        public readonly BorderRadius borderRadius;


        public static MaterialInkController of(BuildContext context) {
            _RenderInkFeatures result =
                (_RenderInkFeatures) context.ancestorRenderObjectOfType(new TypeMatcher<_RenderInkFeatures>());
            return result;
        }

        public override State createState() {
            return new _MaterialState();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<MaterialType>("type", this.type));
            properties.add(new FloatProperty("elevation", this.elevation, defaultValue: 0.0f));
            properties.add(new DiagnosticsProperty<Color>("color", this.color, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("shadowColor", this.shadowColor,
                defaultValue: new Color(0xFF000000)));
            this.textStyle?.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("borderOnForeground", this.borderOnForeground,
                defaultValue: null));
            properties.add(
                new DiagnosticsProperty<BorderRadius>("borderRadius", this.borderRadius, defaultValue: null));
        }

        public const float defaultSplashRadius = 35.0f;
    }


    class _MaterialState : TickerProviderStateMixin<Material> {
        readonly GlobalKey _inkFeatureRenderer = GlobalKey.key(debugLabel: "ink renderer");

        Color _getBackgroundColor(BuildContext context) {
            if (this.widget.color != null) {
                return this.widget.color;
            }

            switch (this.widget.type) {
                case MaterialType.canvas:
                    return Theme.of(context).canvasColor;
                case MaterialType.card:
                    return Theme.of(context).cardColor;
                default:
                    return null;
            }
        }

        public override Widget build(BuildContext context) {
            Color backgroundColor = this._getBackgroundColor(context);
            D.assert(backgroundColor != null || this.widget.type == MaterialType.transparency,
                () => "If Material type is not MaterialType.transparency, a color must" +
                      "either be passed in through the 'color' property, or be defined " +
                      "in the theme (ex. canvasColor != null if type is set to " +
                      "MaterialType.canvas");
            Widget contents = this.widget.child;
            if (contents != null) {
                contents = new AnimatedDefaultTextStyle(
                    style: this.widget.textStyle ?? Theme.of(context).textTheme.body1,
                    duration: this.widget.animationDuration,
                    child: contents
                );
            }

            contents = new NotificationListener<LayoutChangedNotification>(
                onNotification: (LayoutChangedNotification notification) => {
                    _RenderInkFeatures renderer =
                        (_RenderInkFeatures) this._inkFeatureRenderer.currentContext.findRenderObject();
                    renderer._didChangeLayout();
                    return true;
                },
                child: new _InkFeatures(
                    key: this._inkFeatureRenderer,
                    color: backgroundColor,
                    child: contents,
                    vsync: this
                )
            );

            if (this.widget.type == MaterialType.canvas && this.widget.shape == null &&
                this.widget.borderRadius == null) {
                return new AnimatedPhysicalModel(
                    curve: Curves.fastOutSlowIn,
                    duration: this.widget.animationDuration,
                    shape: BoxShape.rectangle,
                    clipBehavior: this.widget.clipBehavior,
                    borderRadius: BorderRadius.zero,
                    elevation: this.widget.elevation,
                    color: backgroundColor,
                    shadowColor: this.widget.shadowColor,
                    animateColor: false,
                    child: contents
                );
            }

            ShapeBorder shape = this._getShape();

            if (this.widget.type == MaterialType.transparency) {
                return _transparentInterior(
                    context: context,
                    shape: shape,
                    clipBehavior: this.widget.clipBehavior,
                    contents: contents);
            }

            return new _MaterialInterior(
                curve: Curves.fastOutSlowIn,
                duration: this.widget.animationDuration,
                shape: shape,
                borderOnForeground: this.widget.borderOnForeground,
                clipBehavior: this.widget.clipBehavior,
                elevation: this.widget.elevation,
                color: backgroundColor,
                shadowColor: this.widget.shadowColor,
                child: contents
            );
        }


        static Widget _transparentInterior(
            BuildContext context = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            Widget contents = null) {
            _ShapeBorderPaint child = new _ShapeBorderPaint(
                child: contents,
                shape: shape);

            if (clipBehavior == Clip.none) {
                return child;
            }

            return new ClipPath(
                child: child,
                clipper: new ShapeBorderClipper(shape: shape),
                clipBehavior: clipBehavior ?? Clip.none
            );
        }


        ShapeBorder _getShape() {
            if (this.widget.shape != null) {
                return this.widget.shape;
            }

            if (this.widget.borderRadius != null) {
                return new RoundedRectangleBorder(borderRadius: this.widget.borderRadius);
            }

            switch (this.widget.type) {
                case MaterialType.canvas:
                case MaterialType.transparency:
                    return new RoundedRectangleBorder();
                case MaterialType.card:
                case MaterialType.button:
                    return new RoundedRectangleBorder(
                        borderRadius: this.widget.borderRadius ??
                                      MaterialConstantsUtils.kMaterialEdges[this.widget.type]);
                case MaterialType.circle:
                    return new CircleBorder();
            }

            return new RoundedRectangleBorder();
        }
    }


    public class _RenderInkFeatures : RenderProxyBox, MaterialInkController {
        public _RenderInkFeatures(
            RenderBox child = null,
            TickerProvider vsync = null,
            Color color = null) : base(child: child) {
            D.assert(vsync != null);
            this._vsync = vsync;
            this._color = color;
        }

        public TickerProvider vsync {
            get { return this._vsync; }
        }

        readonly TickerProvider _vsync;

        public Color color {
            get { return this._color; }
            set { this._color = value; }
        }

        Color _color;

        List<InkFeature> _inkFeatures;

        public void addInkFeature(InkFeature feature) {
            D.assert(!feature._debugDisposed);
            D.assert(feature._controller == this);
            this._inkFeatures = this._inkFeatures ?? new List<InkFeature>();
            D.assert(!this._inkFeatures.Contains(feature));
            this._inkFeatures.Add(feature);
            this.markNeedsPaint();
        }

        public void _removeFeature(InkFeature feature) {
            D.assert(this._inkFeatures != null);
            this._inkFeatures.Remove(feature);
            this.markNeedsPaint();
        }

        public void _didChangeLayout() {
            if (this._inkFeatures != null && this._inkFeatures.isNotEmpty()) {
                this.markNeedsPaint();
            }
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._inkFeatures != null && this._inkFeatures.isNotEmpty()) {
                Canvas canvas = context.canvas;
                canvas.save();
                canvas.translate(offset.dx, offset.dy);
                canvas.clipRect(Offset.zero & this.size);
                foreach (InkFeature inkFeature in this._inkFeatures) {
                    inkFeature._paint(canvas);
                }

                canvas.restore();
            }

            base.paint(context, offset);
        }
    }


    public class _InkFeatures : SingleChildRenderObjectWidget {
        public _InkFeatures(
            Key key = null,
            Color color = null,
            TickerProvider vsync = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(vsync != null);
            this.color = color;
            this.vsync = vsync;
        }

        public readonly Color color;

        public readonly TickerProvider vsync;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderInkFeatures(
                color: this.color,
                vsync: this.vsync);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderInkFeatures _renderObject = (_RenderInkFeatures) renderObject;
            _renderObject.color = this.color;
            D.assert(this.vsync == _renderObject.vsync);
        }
    }

    public abstract class InkFeature {
        public InkFeature(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            VoidCallback onRemoved = null) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            this._controller = (_RenderInkFeatures) controller;
            this.referenceBox = referenceBox;
            this.onRemoved = onRemoved;
        }

        public MaterialInkController controller {
            get { return this._controller; }
        }

        public _RenderInkFeatures _controller;

        public readonly RenderBox referenceBox;

        public readonly VoidCallback onRemoved;

        public bool _debugDisposed = false;

        public virtual void dispose() {
            D.assert(!this._debugDisposed);
            D.assert(() => {
                this._debugDisposed = true;
                return true;
            });
            this._controller._removeFeature(this);
            if (this.onRemoved != null) {
                this.onRemoved();
            }
        }

        public void _paint(Canvas canvas) {
            D.assert(this.referenceBox.attached);
            D.assert(!this._debugDisposed);

            List<RenderObject> descendants = new List<RenderObject> {this.referenceBox};
            RenderObject node = this.referenceBox;
            while (node != this._controller) {
                node = (RenderObject) node.parent;
                D.assert(node != null);
                descendants.Add(node);
            }

            Matrix3 transform = Matrix3.I();
            D.assert(descendants.Count >= 2);
            for (int index = descendants.Count - 1; index > 0; index -= 1) {
                descendants[index].applyPaintTransform(descendants[index - 1], transform);
            }

            this.paintFeature(canvas, transform);
        }

        protected abstract void paintFeature(Canvas canvas, Matrix3 transform);

        public override string ToString() {
            return this.GetType() + "";
        }
    }

    public class ShapeBorderTween : Tween<ShapeBorder> {
        public ShapeBorderTween(
            ShapeBorder begin = null,
            ShapeBorder end = null) : base(begin: begin, end: end) {
        }

        public override ShapeBorder lerp(float t) {
            return ShapeBorder.lerp(this.begin, this.end, t);
        }
    }

    public class _MaterialInterior : ImplicitlyAnimatedWidget {
        public _MaterialInterior(
            Key key = null,
            Widget child = null,
            ShapeBorder shape = null,
            bool borderOnForeground = true,
            Clip clipBehavior = Clip.none,
            float? elevation = null,
            Color color = null,
            Color shadowColor = null,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key, curve: curve ?? Curves.linear, duration: duration) {
            D.assert(child != null);
            D.assert(shape != null);
            D.assert(elevation != null && elevation >= 0.0f);
            D.assert(color != null);
            D.assert(shadowColor != null);
            D.assert(duration != null);
            this.child = child;
            this.shape = shape;
            this.borderOnForeground = borderOnForeground;
            this.clipBehavior = clipBehavior;
            this.elevation = elevation ?? 0.0f;
            this.color = color;
            this.shadowColor = shadowColor;
        }

        public readonly Widget child;

        public readonly ShapeBorder shape;

        public readonly bool borderOnForeground;

        public readonly Clip clipBehavior;

        public readonly float elevation;

        public readonly Color color;

        public readonly Color shadowColor;

        public override State createState() {
            return new _MaterialInteriorState();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape));
            description.add(new FloatProperty("elevation", this.elevation));
            description.add(new DiagnosticsProperty<Color>("color", this.color));
            description.add(new DiagnosticsProperty<Color>("shadowColor", this.shadowColor));
        }
    }

    public class _MaterialInteriorState : AnimatedWidgetBaseState<_MaterialInterior> {
        FloatTween _elevation;
        ColorTween _shadowColor;
        ShapeBorderTween _border;

        protected override void forEachTween(TweenVisitor visitor) {
            this._elevation = (FloatTween) visitor.visit(this, this._elevation, this.widget.elevation,
                (float value) => new FloatTween(begin: value, end: value));
            this._shadowColor = (ColorTween) visitor.visit(this, this._shadowColor, this.widget.shadowColor,
                (Color value) => new ColorTween(begin: value));
            this._border = (ShapeBorderTween) visitor.visit(this, this._border, this.widget.shape,
                (ShapeBorder value) => new ShapeBorderTween(begin: value));
        }

        public override Widget build(BuildContext context) {
            ShapeBorder shape = this._border.evaluate(this.animation);
            return new PhysicalShape(
                child: new _ShapeBorderPaint(
                    child: this.widget.child,
                    shape: shape,
                    borderOnForeground: this.widget.borderOnForeground),
                clipper: new ShapeBorderClipper(
                    shape: shape),
                clipBehavior: this.widget.clipBehavior,
                elevation: this._elevation.evaluate(this.animation),
                color: this.widget.color,
                shadowColor: this._shadowColor.evaluate(this.animation)
            );
        }
    }

    class _ShapeBorderPaint : StatelessWidget {
        public _ShapeBorderPaint(
            Widget child = null,
            ShapeBorder shape = null,
            bool borderOnForeground = true) {
            D.assert(child != null);
            D.assert(shape != null);
            this.child = child;
            this.shape = shape;
            this.borderOnForeground = borderOnForeground;
        }

        public readonly Widget child;

        public readonly ShapeBorder shape;

        public readonly bool borderOnForeground;

        public override Widget build(BuildContext context) {
            return new CustomPaint(
                child: this.child,
                painter: this.borderOnForeground ? null : new _ShapeBorderPainter(this.shape),
                foregroundPainter: this.borderOnForeground ? new _ShapeBorderPainter(this.shape) : null);
        }
    }

    class _ShapeBorderPainter : AbstractCustomPainter {
        public _ShapeBorderPainter(ShapeBorder border = null) : base(null) {
            this.border = border;
        }

        public readonly ShapeBorder border;


        public override void paint(Canvas canvas, Size size) {
            this.border.paint(canvas, Offset.zero & size);
        }

        public override bool shouldRepaint(CustomPainter oldDelegate) {
            _ShapeBorderPainter _oldDelegate = (_ShapeBorderPainter) oldDelegate;
            return _oldDelegate.border != this.border;
        }
    }
}