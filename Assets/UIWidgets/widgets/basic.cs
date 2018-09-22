using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEngine;
using Color = UIWidgets.ui.Color;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.widgets {
    public class Directionality : InheritedWidget {
        public Directionality(
            Widget child,
            TextDirection textDirection,
            Key key = null
        ) : base(key, child) {
            this.textDirection = textDirection;
        }

        public TextDirection textDirection;

        public static TextDirection of(BuildContext context) {
            Directionality widget = context.inheritFromWidgetOfExactType(typeof(Directionality)) as Directionality;
            return widget == null ? TextDirection.ltr : widget.textDirection;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return textDirection != ((Directionality) oldWidget).textDirection;
        }
    }

    public class Opacity : SingleChildRenderObjectWidget {
        public Opacity(double opacity, Key key = null, Widget child = null) : base(key, child) {
            this.opacity = opacity;
        }

        public double opacity;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderOpacity(opacity: opacity);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderOpacity) renderObject).opacity = opacity;
        }
    }

    public class LimitedBox : SingleChildRenderObjectWidget {
        public LimitedBox(
            Key key = null,
            Widget child = null,
            double maxWidth = double.MaxValue,
            double maxHeight = double.MaxValue
        ) : base(key, child) {
            this.maxHeight = maxHeight;
            this.maxWidth = maxWidth;
        }

        public double maxWidth;
        public double maxHeight;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderLimitedBox(
                maxWidth: maxWidth,
                maxHeight: maxHeight
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderLimitedBox) renderObject).maxWidth = maxWidth;
            ((RenderLimitedBox) renderObject).maxHeight = maxHeight;
        }
    }

    public class ConstrainedBox : SingleChildRenderObjectWidget {
        public ConstrainedBox(
            Key key = null,
            BoxConstraints constraints = null,
            Widget child = null
        ) : base(key, child) {
            this.constraints = constraints;
        }

        public BoxConstraints constraints;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderConstrainedBox(additionalConstraints: constraints);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderConstrainedBox) renderObject)._additionalConstraints = constraints;
        }
    }

    public class Padding : SingleChildRenderObjectWidget {
        public Padding(
            EdgeInsets padding,
            Key key = null,
            Widget child = null
        ) : base(key, child) {
            this.padding = padding;
        }

        public EdgeInsets padding;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPadding(
                padding: padding
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderPadding) renderObject).padding = padding;
        }
    }

    public class Transform : SingleChildRenderObjectWidget {
        public Transform(
            Matrix4x4 transform,
            Offset origin = null,
            Alignment alignment = null,
            bool transformHitTests = false,
            Key key = null,
            Widget child = null
        ) : base(key, child) {
            this.alignment = alignment ?? Alignment.center;
            this.origin = origin;
            this.transformHitTests = transformHitTests;
            this.transform = transform;
        }

        // scale
        public Transform(
            double scale,
            Offset origin,
            Alignment alignment,
            bool transformHitTests = false,
            Key key = null,
            Widget child = null
        ) : base(key, child) {
            this.alignment = alignment ?? Alignment.center;
            this.origin = origin;
            this.transformHitTests = transformHitTests;
            this.transform = Matrix4x4.Scale(new Vector3((float) scale, (float) scale, (float) 1.0));
        }

        public Matrix4x4 transform;
        public Offset origin;
        public Alignment alignment;
        public bool transformHitTests;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderTransform(
                transform: transform,
                origin: origin,
                alignment: alignment,
                textDirection: Directionality.of(context),
                transformHitTests: transformHitTests
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderTransform) renderObject).transform = transform;
            ((RenderTransform) renderObject).origin = origin;
            ((RenderTransform) renderObject).alignment = alignment;
            ((RenderTransform) renderObject).textDirection = Directionality.of(context);
            ((RenderTransform) renderObject).transformHitTests = transformHitTests;
        }
    }


    public class Align : SingleChildRenderObjectWidget {
        public Align(
            double widthFactor = 0.0,
            double heightFactor = 0.0,
            Key key = null,
            Widget child = null,
            Alignment alignment = null
        ) : base(key, child) {
            this.alignment = alignment ?? Alignment.center;
            this.widthFactor = widthFactor;
            this.heightFactor = heightFactor;
        }

        public Alignment alignment;

        public double widthFactor;

        public double heightFactor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPositionedBox(
                alignment: alignment,
                widthFactor: widthFactor,
                heightFactor: heightFactor
            );
        }
    }

    public class SliverPadding : SingleChildRenderObjectWidget {
        public SliverPadding(
            Key key = null,
            EdgeInsets padding = null,
            Widget sliver = null
        ) : base(key: key, child: sliver) {
            D.assert(padding != null);
            this.padding = padding;
        }

        public readonly EdgeInsets padding;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverPadding(
                padding: this.padding
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderSliverPadding) renderObjectRaw;
            renderObject.padding = this.padding;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding));
        }
    }

    public class RichText : LeafRenderObjectWidget
    {
        public RichText(TextSpan text, Key key = null, 
            TextAlign textAlign = TextAlign.left, TextDirection? textDirection = null,
            bool softWrap = true, TextOverflow overflow = TextOverflow.clip, double textScaleFactor = 1.0,
            int maxLines = 0): base(key)
        {
            D.assert(text != null);
            this.text = text;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
        }
        
        
         public override RenderObject createRenderObject(BuildContext context) {
            D.assert(textDirection != null || WidgetsD.debugCheckHasDirectionality(context));
            return new RenderParagraph(text,
                textAlign: textAlign,
                textDirection: textDirection ?? Directionality.of(context),
                softWrap: softWrap,
                overflow: overflow,
                textScaleFactor: textScaleFactor,
                maxLines: maxLines
            );
        }
        
        public override void updateRenderObject(BuildContext context, RenderObject r) {
            D.assert(textDirection != null || WidgetsD.debugCheckHasDirectionality(context));
            var renderObject = (RenderParagraph) (r);
            renderObject.text = text;
            renderObject.textAlign = textAlign;
            renderObject.textDirection = textDirection ?? Directionality.of(context);
            renderObject.softWrap = softWrap;
            renderObject.overflow = overflow;
            renderObject.textScaleFactor = textScaleFactor;
            renderObject.maxLines = maxLines;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<TextAlign>("textAlign", textAlign, defaultValue: TextAlign.left));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add(new FlagProperty("softWrap", value: softWrap, ifTrue: "wrapping at box width", ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow>("overflow", overflow, defaultValue: TextOverflow.clip));
            properties.add(new DoubleProperty("textScaleFactor", textScaleFactor, defaultValue: 1.0));
            properties.add(new IntProperty("maxLines", maxLines, ifNull: "unlimited"));
            properties.add(new StringProperty("text", text.toPlainText()));
        }
        
        public readonly TextSpan text;
        public readonly  TextAlign textAlign;
        public readonly  TextDirection? textDirection;
        public readonly  bool softWrap;
        public readonly  TextOverflow overflow;
        public readonly  double textScaleFactor;
        public readonly  int maxLines;
    }

    public class RawImage : LeafRenderObjectWidget {
        public RawImage(Key key, ui.Image image, double width, double height, double scale, Color color,
            BlendMode colorBlendMode, BoxFit fit, Rect centerSlice, Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat) : base(key) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.scale = scale;
            this.color = color;
            this.blendMode = colorBlendMode;
            this.centerSlice = centerSlice;
            this.fit = fit;
            this.alignment = alignment == null ? Alignment.center : alignment;
            this.repeat = repeat;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderImage(
                this.image,
                this.width,
                this.height,
                this.color,
                this.blendMode,
                this.fit,
                this.repeat,
                this.centerSlice,
                this.alignment
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderImage) renderObject).image = this.image;
            ((RenderImage) renderObject).width = this.width;
            ((RenderImage) renderObject).height = this.height;
            ((RenderImage) renderObject).color = this.color;
            ((RenderImage) renderObject).fit = this.fit;
            ((RenderImage) renderObject).repeat = this.repeat;
            ((RenderImage) renderObject).centerSlice = this.centerSlice;
            ((RenderImage) renderObject).alignment = this.alignment;
        }

        public ui.Image image;
        public double width;
        public double height;
        public double scale;
        public Color color;
        public BlendMode blendMode;
        public BoxFit fit;
        public Alignment alignment;
        public ImageRepeat repeat;
        public Rect centerSlice;
    }

    public class Listener : SingleChildRenderObjectWidget {
        public Listener(
            Key key = null,
            PointerDownEventListener onPointerDown = null,
            PointerMoveEventListener onPointerMove = null,
            PointerUpEventListener onPointerUp = null,
            PointerCancelEventListener onPointerCancel = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            Widget child = null
        ) : base(key: key, child: child) {
            this.onPointerDown = onPointerDown;
            this.onPointerMove = onPointerMove;
            this.onPointerUp = onPointerUp;
            this.onPointerCancel = onPointerCancel;
            this.behavior = behavior;
        }

        public readonly PointerDownEventListener onPointerDown;

        public readonly PointerMoveEventListener onPointerMove;

        public readonly PointerUpEventListener onPointerUp;

        public readonly PointerCancelEventListener onPointerCancel;

        public readonly HitTestBehavior behavior;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPointerListener(
                onPointerDown: this.onPointerDown,
                onPointerMove: this.onPointerMove,
                onPointerUp: this.onPointerUp,
                onPointerCancel: this.onPointerCancel,
                behavior: this.behavior
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderPointerListener) renderObjectRaw;
            renderObject.onPointerDown = this.onPointerDown;
            renderObject.onPointerMove = this.onPointerMove;
            renderObject.onPointerUp = this.onPointerUp;
            renderObject.onPointerCancel = this.onPointerCancel;
            renderObject.behavior = this.behavior;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            List<string> listeners = new List<string>();
            if (this.onPointerDown != null) {
                listeners.Add("down");
            }

            if (this.onPointerMove != null) {
                listeners.Add("move");
            }

            if (this.onPointerUp != null) {
                listeners.Add("up");
            }

            if (this.onPointerCancel != null) {
                listeners.Add("cancel");
            }

            properties.add(new EnumerableProperty<string>("listeners", listeners, ifEmpty: "<none>"));
            properties.add(new EnumProperty<HitTestBehavior>("behavior", this.behavior));
        }
    }

    public class RepaintBoundary : SingleChildRenderObjectWidget {
        public RepaintBoundary(Key key = null, Widget child = null) : base(key: key, child: child) {
        }

        public static RepaintBoundary wrap(Widget child, int childIndex) {
            D.assert(child != null);
            Key key = child.key != null ? (Key) new ValueKey<Key>(child.key) : new ValueKey<int>(childIndex);
            return new RepaintBoundary(key: key, child: child);
        }

        public static List<RepaintBoundary> wrapAll(List<Widget> widgets) {
            List<RepaintBoundary> result = new List<RepaintBoundary>(widgets.Count);
            for (int i = 0; i < result.Count; ++i) {
                result[i] = RepaintBoundary.wrap(widgets[i], i);
            }

            return result;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderRepaintBoundary();
        }
    }

    public class IgnorePointer : SingleChildRenderObjectWidget {
        public IgnorePointer(
            Key key = null,
            bool ignoring = true,
            Widget child = null
        ) : base(key: key, child: child) {
            this.ignoring = ignoring;
        }

        public readonly bool ignoring;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderIgnorePointer(
                ignoring: this.ignoring
            );
        }

        public override
            void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderIgnorePointer) renderObjectRaw;
            renderObject.ignoring = this.ignoring;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("ignoring", this.ignoring));
        }
    }

    public class Builder : StatelessWidget
    {
        public Builder(WidgetBuilder builder, Key key = null) : base(key)
        {
            D.assert(builder != null);
            this.builder = builder;
        }

        public readonly WidgetBuilder builder;

        public override Widget build(BuildContext context)
        {
            return builder(context);
        }
    }
}