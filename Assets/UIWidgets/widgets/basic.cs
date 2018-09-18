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

    public abstract class Flex : MultiChildRenderObjectWidget {
        public Flex(
            Axis direction,
            TextDirection? textDirection,
            TextBaseline? textBaseline,
            Key key = null,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            VerticalDirection verticalDirection = VerticalDirection.down,
            List<Widget> children = null
        ) : base(key, children) {
            this.direction = direction;
            this.mainAxisAlignment = mainAxisAlignment;
            this.mainAxisSize = mainAxisSize;
            this.crossAxisAlignment = crossAxisAlignment;
            this.textDirection = textDirection;
            this.verticalDirection = verticalDirection;
            this.textBaseline = textBaseline;
        }

        public Axis direction;
        public MainAxisAlignment mainAxisAlignment;
        public MainAxisSize mainAxisSize;
        public CrossAxisAlignment crossAxisAlignment;
        public TextDirection? textDirection;
        public VerticalDirection verticalDirection;
        public TextBaseline? textBaseline;

        private bool _needTextDirection {
            get {
                D.assert(direction != null);
                switch (direction) {
                    case Axis.horizontal:
                        return true;
                    case Axis.vertical:
                        return (this.crossAxisAlignment == CrossAxisAlignment.start ||
                                this.crossAxisAlignment == CrossAxisAlignment.end);
                }

                return false;
            }
        }

        public TextDirection getEffectiveTextDirection(BuildContext context) {
            return textDirection ?? (_needTextDirection ? Directionality.of(context) : TextDirection.ltr);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            Debug.Log("a");
            return new RenderFlex(
                direction: direction,
                mainAxisAlignment: mainAxisAlignment,
                mainAxisSize: mainAxisSize,
                crossAxisAlignment: crossAxisAlignment,
                textDirection: getEffectiveTextDirection(context),
                verticalDirection: verticalDirection,
                textBaseline: textBaseline ?? TextBaseline.alphabetic
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderFlex) renderObject).direction = this.direction;
            ((RenderFlex) renderObject).mainAxisAlignment = this.mainAxisAlignment;
            ((RenderFlex) renderObject).mainAxisSize = this.mainAxisSize;
            ((RenderFlex) renderObject).crossAxisAlignment = this.crossAxisAlignment;
            ((RenderFlex) renderObject).textDirection = this.textDirection ?? TextDirection.ltr;
            ((RenderFlex) renderObject).verticalDirection = this.verticalDirection;
            ((RenderFlex) renderObject).textBaseline = this.textBaseline ?? TextBaseline.alphabetic;
        }
    }

    public class Row : Flex {
        public Row(
            TextDirection? textDirection,
            TextBaseline? textBaseline,
            Key key = null,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            VerticalDirection verticalDirection = VerticalDirection.down,
            List<Widget> children = null
        ) : base(
            children: children,
            key: key,
            direction: Axis.horizontal,
            textDirection: textDirection,
            textBaseline: textBaseline,
            mainAxisAlignment: mainAxisAlignment,
            mainAxisSize: mainAxisSize,
            crossAxisAlignment: crossAxisAlignment,
            verticalDirection: verticalDirection
        ) {}
    }
    
    public class Column : Flex {
        public Column(
            TextDirection? textDirection,
            TextBaseline? textBaseline,
            Key key = null,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            VerticalDirection verticalDirection = VerticalDirection.down,
            List<Widget> children = null
        ) : base(
            children: children,
            key: key,
            direction: Axis.vertical,
            textDirection: textDirection,
            textBaseline: textBaseline,
            mainAxisAlignment: mainAxisAlignment,
            mainAxisSize: mainAxisSize,
            crossAxisAlignment: crossAxisAlignment,
            verticalDirection: verticalDirection
        ) {}
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
}