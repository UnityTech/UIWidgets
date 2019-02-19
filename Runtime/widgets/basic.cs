using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.utils;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class Directionality : InheritedWidget {
        public Directionality(
            Widget child,
            TextDirection textDirection,
            Key key = null
        ) : base(key, child) {
            this.textDirection = textDirection;
        }

        public readonly TextDirection textDirection;

        public static TextDirection of(BuildContext context) {
            Directionality widget = context.inheritFromWidgetOfExactType(typeof(Directionality)) as Directionality;
            return widget == null ? TextDirection.ltr : widget.textDirection;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.textDirection != ((Directionality) oldWidget).textDirection;
        }
    }

    public class Opacity : SingleChildRenderObjectWidget {
        public Opacity(double opacity, Key key = null, Widget child = null) : base(key, child) {
            this.opacity = opacity;
        }

        public readonly double opacity;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderOpacity(opacity: this.opacity);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderOpacity) renderObject).opacity = this.opacity;
        }
    }

    public class CustomPaint : SingleChildRenderObjectWidget {
        public CustomPaint(
            Key key = null,
            CustomPainter painter = null,
            CustomPainter foregroundPainter = null,
            Size size = null,
            bool isComplex = false,
            bool willChange = false,
            Widget child = null
        ) : base(key: key, child: child) {
            size = size ?? Size.zero;
            this.size = size;
            this.painter = painter;
            this.foregroundPainter = foregroundPainter;
            this.isComplex = isComplex;
            this.willChange = willChange;
        }

        public readonly CustomPainter painter;
        public readonly CustomPainter foregroundPainter;
        public readonly Size size;
        public readonly bool isComplex;
        public readonly bool willChange;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderCustomPaint(
                painter: this.painter,
                foregroundPainter: this.foregroundPainter,
                preferredSize: this.size,
                isComplex: this.isComplex,
                willChange: this.willChange
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderCustomPaint) renderObject).painter = this.painter;
            ((RenderCustomPaint) renderObject).foregroundPainter = this.foregroundPainter;
            ((RenderCustomPaint) renderObject).preferredSize = this.size;
            ((RenderCustomPaint) renderObject).isComplex = this.isComplex;
            ((RenderCustomPaint) renderObject).willChange = this.willChange;
        }

        public override void didUnmountRenderObject(RenderObject renderObject) {
            ((RenderCustomPaint) renderObject).painter = null;
            ((RenderCustomPaint) renderObject).foregroundPainter = null;
        }
    }

    public class ClipRect : SingleChildRenderObjectWidget {
        public ClipRect(
            Key key = null,
            CustomClipper<Rect> clipper = null,
            Clip clipBehavior = Clip.hardEdge,
            Widget child = null
        ) : base(key: key, child: child) {
            this.clipper = clipper;
            this.clipBehavior = clipBehavior;
        }

        public readonly CustomClipper<Rect> clipper;

        public readonly Clip clipBehavior;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderClipRect(
                clipper: this.clipper,
                clipBehavior: this.clipBehavior);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderClipRect _renderObject = (RenderClipRect) renderObject;
            _renderObject.clipper = this.clipper;
        }

        public override void didUnmountRenderObject(RenderObject renderObject) {
            RenderClipRect _renderObject = (RenderClipRect) renderObject;
            _renderObject.clipper = null;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<CustomClipper<Rect>>("clipper", this.clipper, defaultValue: null));
        }
    }

    public class ClipPath : SingleChildRenderObjectWidget {
        public ClipPath(
            Key key = null,
            CustomClipper<Path> clipper = null,
            Clip clipBehavior = Clip.antiAlias,
            Widget child = null
        ) : base(key: key, child: child) {
            this.clipper = clipper;
            this.clipBehavior = clipBehavior;
        }

        public readonly CustomClipper<Path> clipper;

        public readonly Clip clipBehavior;


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderClipPath(clipper: this.clipper, clipBehavior: this.clipBehavior);
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderClipPath _renderObject = (RenderClipPath) renderObject;
            _renderObject.clipper = this.clipper;
        }


        public override void didUnmountRenderObject(RenderObject renderObject) {
            RenderClipPath _renderObject = (RenderClipPath) renderObject;
            _renderObject.clipper = null;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<CustomClipper<Path>>("clipper", this.clipper, defaultValue: null));
        }
    }

    public class LimitedBox : SingleChildRenderObjectWidget {
        public LimitedBox(
            Key key = null,
            double maxWidth = double.MaxValue,
            double maxHeight = double.MaxValue,
            Widget child = null
        ) : base(key, child) {
            D.assert(maxWidth >= 0.0);
            D.assert(maxHeight >= 0.0);

            this.maxHeight = maxHeight;
            this.maxWidth = maxWidth;
        }

        public readonly double maxWidth;
        public readonly double maxHeight;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderLimitedBox(
                maxWidth: this.maxWidth,
                maxHeight: this.maxHeight
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderLimitedBox) renderObjectRaw;
            renderObject.maxWidth = this.maxWidth;
            renderObject.maxHeight = this.maxHeight;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DoubleProperty("maxWidth", this.maxWidth, defaultValue: double.PositiveInfinity));
            properties.add(new DoubleProperty("maxHeight", this.maxHeight, defaultValue: double.PositiveInfinity));
        }
    }

    public class SizedBox : SingleChildRenderObjectWidget {
        public SizedBox(Key key = null, double? width = null, double? height = null, Widget child = null)
            : base(key: key, child: child) {
            this.width = width;
            this.height = height;
        }

        public static SizedBox expand(Key key = null, Widget child = null) {
            return new SizedBox(key, double.PositiveInfinity, double.PositiveInfinity, child);
        }

        public static SizedBox shrink(Key key = null, Widget child = null) {
            return new SizedBox(key, 0, 0, child);
        }

        public static SizedBox fromSize(Key key = null, Widget child = null, Size size = null) {
            return new SizedBox(key,
                size == null ? (double?) null : size.width,
                size == null ? (double?) null : size.height, child);
        }

        public readonly double? width;

        public readonly double? height;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderConstrainedBox(
                additionalConstraints: this._additionalConstraints
            );
        }

        BoxConstraints _additionalConstraints {
            get { return BoxConstraints.tightFor(width: this.width, height: this.height); }
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderConstrainedBox) renderObjectRaw;
            renderObject.additionalConstraints = this._additionalConstraints;
        }

        public override string toStringShort() {
            string type;
            if (this.width == double.PositiveInfinity && this.height == double.PositiveInfinity) {
                type = this.GetType() + "expand";
            }
            else if (this.width == 0.0 && this.height == 0.0) {
                type = this.GetType() + "shrink";
            }
            else {
                type = this.GetType() + "";
            }

            return this.key == null ? type : type + "-" + this.key;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            DiagnosticLevel level;
            if ((this.width == double.PositiveInfinity && this.height == double.PositiveInfinity) ||
                (this.width == 0.0 && this.height == 0.0)) {
                level = DiagnosticLevel.hidden;
            }
            else {
                level = DiagnosticLevel.info;
            }

            properties.add(new DoubleProperty("width", this.width,
                defaultValue: Diagnostics.kNullDefaultValue,
                level: level));
            properties.add(new DoubleProperty("height", this.height,
                defaultValue: Diagnostics.kNullDefaultValue,
                level: level));
        }
    }


    public class ConstrainedBox : SingleChildRenderObjectWidget {
        public ConstrainedBox(
            Key key = null,
            BoxConstraints constraints = null,
            Widget child = null
        ) : base(key, child) {
            D.assert(constraints != null);
            D.assert(constraints.debugAssertIsValid());

            this.constraints = constraints;
        }

        public readonly BoxConstraints constraints;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderConstrainedBox(additionalConstraints: this.constraints);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderConstrainedBox) renderObjectRaw;
            renderObject.additionalConstraints = this.constraints;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<BoxConstraints>("constraints",
                this.constraints, showName: false));
        }
    }

    public class Flex : MultiChildRenderObjectWidget {
        public Flex(
            Axis direction = Axis.vertical,
            TextDirection? textDirection = null,
            TextBaseline? textBaseline = null,
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

        public readonly Axis direction;
        public readonly MainAxisAlignment mainAxisAlignment;
        public readonly MainAxisSize mainAxisSize;
        public readonly CrossAxisAlignment crossAxisAlignment;
        public readonly TextDirection? textDirection;
        public readonly VerticalDirection verticalDirection;
        public readonly TextBaseline? textBaseline;

        bool _needTextDirection {
            get {
                switch (this.direction) {
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
            return this.textDirection ?? (this._needTextDirection ? Directionality.of(context) : TextDirection.ltr);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderFlex(
                direction: this.direction,
                mainAxisAlignment: this.mainAxisAlignment,
                mainAxisSize: this.mainAxisSize,
                crossAxisAlignment: this.crossAxisAlignment,
                textDirection: this.getEffectiveTextDirection(context),
                verticalDirection: this.verticalDirection,
                textBaseline: this.textBaseline ?? TextBaseline.alphabetic
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

    public class Offstage : SingleChildRenderObjectWidget {
        public Offstage(Key key = null, bool offstage = true, Widget child = null) : base(key: key, child: child) {
            this.offstage = offstage;
        }

        public readonly bool offstage;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderOffstage(offstage: this.offstage);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderOffstage) renderObject).offstage = this.offstage;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("offstage", this.offstage));
        }

        public override Element createElement() {
            return new _OffstageElement(this);
        }
    }

    class _OffstageElement : SingleChildRenderObjectElement {
        internal _OffstageElement(Offstage widget) : base(widget) {
        }

        new Offstage widget {
            get { return (Offstage) base.widget; }
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            if (!this.widget.offstage) {
                base.debugVisitOnstageChildren(visitor);
            }
        }
    }

    public class AspectRatio : SingleChildRenderObjectWidget {
        public AspectRatio(
            Key key = null,
            double aspectRatio = 1.0,
            Widget child = null
        ) : base(key: key, child: child) {
            this.aspectRatio = aspectRatio;
        }

        public readonly double aspectRatio;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAspectRatio(aspectRatio: this.aspectRatio);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderAspectRatio) renderObject).aspectRatio = this.aspectRatio;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DoubleProperty("aspectRatio", this.aspectRatio));
        }
    }


    public class ListBody : MultiChildRenderObjectWidget {
        public ListBody(
            Key key = null,
            Axis mainAxis = Axis.vertical,
            bool reverse = false,
            List<Widget> children = null
        ) : base(key: key, children: children ?? new List<Widget>()) {
            this.mainAxis = mainAxis;
            this.reverse = reverse;
        }

        public readonly Axis mainAxis;

        public readonly bool reverse;


        AxisDirection _getDirection(BuildContext context) {
            return AxisDirectionUtils.getAxisDirectionFromAxisReverseAndDirectionality(context, this.mainAxis,
                       this.reverse) ?? AxisDirection.right;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderListBody(
                axisDirection: this._getDirection(context));
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderListBody _renderObject = (RenderListBody) renderObject;
            _renderObject.axisDirection = this._getDirection(context);
        }
    }

    public class Stack : MultiChildRenderObjectWidget {
        public Stack(
            Key key = null,
            Alignment alignment = null,
            StackFit fit = StackFit.loose,
            Overflow overflow = Overflow.clip,
            List<Widget> children = null
        ) : base(key: key, children: children) {
            this.alignment = alignment ?? Alignment.bottomLeft;
            this.fit = fit;
            this.overflow = overflow;
        }

        public readonly Alignment alignment;
        public readonly StackFit fit;
        public readonly Overflow overflow;


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderStack(
                alignment: this.alignment,
                fit: this.fit,
                overflow: this.overflow
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderStack) renderObjectRaw;
            renderObject.alignment = this.alignment;
            renderObject.fit = this.fit;
            renderObject.overflow = this.overflow;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment));
            properties.add(new EnumProperty<StackFit>("fit", this.fit));
            properties.add(new EnumProperty<Overflow>("overflow", this.overflow));
        }
    }

    public class Positioned : ParentDataWidget<Stack> {
        public Positioned(Widget child, Key key = null, double? left = null, double? top = null,
            double? right = null, double? bottom = null, double? width = null, double? height = null) :
            base(key, child) {
            D.assert(left == null || right == null || width == null);
            D.assert(top == null || bottom == null || height == null);
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.width = width;
            this.height = height;
        }

        public static Positioned fromRect(Rect rect, Widget child, Key key = null) {
            return new Positioned(child, key: key, left: rect.left,
                top: rect.top, width: rect.width, height: rect.height);
        }

        public static Positioned fromRelativeRect(Rect rect, Widget child, Key key = null) {
            return new Positioned(child, key: key, left: rect.left,
                top: rect.top, right: rect.right, bottom: rect.bottom);
        }

        public static Positioned fill(Widget child, Key key = null) {
            return new Positioned(child, key: key, left: 0.0,
                top: 0.0, right: 0.0, bottom: 0.0);
        }

        public static Positioned directional(Widget child, TextDirection textDirection, Key key = null,
            double? start = null, double? top = null,
            double? end = null, double? bottom = null, double? width = null, double? height = null) {
            double? left = null;
            double? right = null;
            switch (textDirection) {
                case TextDirection.rtl:
                    left = end;
                    right = start;
                    break;
                case TextDirection.ltr:
                    left = start;
                    right = end;
                    break;
            }

            return new Positioned(child, key: key, left: left, top: top, right: right, bottom: bottom, width: width,
                height: height);
        }

        public readonly double? left;

        public readonly double? top;

        public readonly double? right;

        public readonly double? bottom;

        public readonly double? width;

        public readonly double? height;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is StackParentData);
            StackParentData parentData = (StackParentData) renderObject.parentData;
            bool needsLayout = false;

            if (parentData.left != this.left) {
                parentData.left = this.left;
                needsLayout = true;
            }

            if (parentData.top != this.top) {
                parentData.top = this.top;
                needsLayout = true;
            }

            if (parentData.right != this.right) {
                parentData.right = this.right;
                needsLayout = true;
            }

            if (parentData.bottom != this.bottom) {
                parentData.bottom = this.bottom;
                needsLayout = true;
            }

            if (parentData.width != this.width) {
                parentData.width = this.width;
                needsLayout = true;
            }

            if (parentData.height != this.height) {
                parentData.height = this.height;
                needsLayout = true;
            }

            if (needsLayout) {
                var targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsLayout();
                }
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DoubleProperty("left", this.left, defaultValue: null));
            properties.add(new DoubleProperty("top", this.top, defaultValue: null));
            properties.add(new DoubleProperty("right", this.right, defaultValue: null));
            properties.add(new DoubleProperty("bottom", this.bottom, defaultValue: null));
            properties.add(new DoubleProperty("width", this.width, defaultValue: null));
            properties.add(new DoubleProperty("height", this.height, defaultValue: null));
        }
    }

    public class Row : Flex {
        public Row(
            TextDirection? textDirection = null,
            TextBaseline? textBaseline = null,
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
        ) {
        }
    }

    public class Column : Flex {
        public Column(
            TextDirection? textDirection = null,
            TextBaseline? textBaseline = null,
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
        ) {
        }
    }

    public class Flexible : ParentDataWidget<Flex> {
        public Flexible(
            Key key = null,
            int flex = 1,
            FlexFit fit = FlexFit.loose,
            Widget child = null
        ) : base(key: key, child: child) {
            this.flex = flex;
            this.fit = fit;
        }

        public readonly int flex;

        public readonly FlexFit fit;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is FlexParentData);
            FlexParentData parentData = (FlexParentData) renderObject.parentData;
            bool needsLayout = false;

            if (parentData.flex != this.flex) {
                parentData.flex = this.flex;
                needsLayout = true;
            }

            if (parentData.fit != this.fit) {
                parentData.fit = this.fit;
                needsLayout = true;
            }

            if (needsLayout) {
                var targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsLayout();
                }
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("flex", this.flex));
        }
    }

    public class Expanded : Flexible {
        public Expanded(
            Key key = null,
            int flex = 1,
            Widget child = null
        ) : base(key: key, flex: flex, fit: FlexFit.tight, child: child) {
            D.assert(child != null);
        }
    }


    public class PhysicalModel : SingleChildRenderObjectWidget {
        public PhysicalModel(
            Key key = null,
            BoxShape shape = BoxShape.rectangle,
            Clip clipBehavior = Clip.none,
            BorderRadius borderRadius = null,
            double elevation = 0.0,
            Color color = null,
            Color shadowColor = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(color != null);

            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.borderRadius = borderRadius;
            this.elevation = elevation;
            this.color = color;
            this.shadowColor = shadowColor ?? new Color(0xFF000000);
        }

        public readonly BoxShape shape;

        public readonly Clip clipBehavior;

        public readonly BorderRadius borderRadius;

        public readonly double elevation;

        public readonly Color color;

        public readonly Color shadowColor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPhysicalModel(
                shape: this.shape,
                clipBehavior: this.clipBehavior,
                borderRadius: this.borderRadius,
                elevation: this.elevation,
                color: this.color,
                shadowColor: this.shadowColor);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderPhysicalModel _renderObject = (RenderPhysicalModel) renderObject;
            _renderObject.shape = this.shape;
            _renderObject.borderRadius = this.borderRadius;
            _renderObject.elevation = this.elevation;
            _renderObject.color = this.color;
            _renderObject.shadowColor = this.shadowColor;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<BoxShape>("shape", this.shape));
            properties.add(new DiagnosticsProperty<BorderRadius>("borderRadius", this.borderRadius));
            properties.add(new DoubleProperty("elevation", this.elevation));
            properties.add(new DiagnosticsProperty<Color>("color", this.color));
            properties.add(new DiagnosticsProperty<Color>("shadowColor", this.shadowColor));
        }
    }


    public class PhysicalShape : SingleChildRenderObjectWidget {
        public PhysicalShape(
            Key key = null,
            CustomClipper<Path> clipper = null,
            Clip clipBehavior = Clip.none,
            double elevation = 0.0,
            Color color = null,
            Color shadowColor = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(clipper != null);
            D.assert(color != null);
            this.clipper = clipper;
            this.clipBehavior = clipBehavior;
            this.elevation = elevation;
            this.color = color;
            this.shadowColor = shadowColor ?? new Color(0xFF000000);
        }

        public readonly CustomClipper<Path> clipper;

        public readonly Clip clipBehavior;

        public readonly double elevation;

        public readonly Color color;

        public readonly Color shadowColor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPhysicalShape(
                clipper: this.clipper,
                clipBehavior: this.clipBehavior,
                elevation: this.elevation,
                color: this.color,
                shadowColor: this.shadowColor);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderPhysicalShape _renderObject = (RenderPhysicalShape) renderObject;
            _renderObject.clipper = this.clipper;
            _renderObject.elevation = this.elevation;
            _renderObject.color = this.color;
            _renderObject.shadowColor = this.shadowColor;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<CustomClipper<Path>>("clipper", this.clipper));
            properties.add(new DoubleProperty("elevation", this.elevation));
            properties.add(new DiagnosticsProperty<Color>("color", this.color));
            properties.add(new DiagnosticsProperty<Color>("shadowColor", this.shadowColor));
        }
    }


    public class Padding : SingleChildRenderObjectWidget {
        public Padding(
            Key key = null,
            EdgeInsets padding = null,
            Widget child = null
        ) : base(key, child) {
            D.assert(padding != null);
            this.padding = padding;
        }

        public readonly EdgeInsets padding;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPadding(
                padding: this.padding
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderPadding) renderObjectRaw;
            renderObject.padding = this.padding;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding));
        }
    }

    public class Transform : SingleChildRenderObjectWidget {
        public Transform(
            Key key = null,
            Matrix3 transform = null,
            Offset origin = null,
            Alignment alignment = null,
            bool transformHitTests = true,
            Widget child = null
        ) : base(key, child) {
            D.assert(transform != null);
            this.transform = new Matrix3(transform);
            this.origin = origin;
            this.alignment = alignment;
            this.transformHitTests = transformHitTests;
        }

        Transform(
            Key key = null,
            Offset origin = null,
            Alignment alignment = null,
            bool transformHitTests = true,
            Widget child = null,
            double degree = 0.0
        ) : base(key: key, child: child) {
            this.transform = Matrix3.makeRotate((float) degree);
            this.origin = origin;
            this.alignment = alignment;
            this.transformHitTests = transformHitTests;
        }

        public static Transform rotate(
            Key key = null,
            double degree = 0.0,
            Offset origin = null,
            Alignment alignment = null,
            bool transformHitTests = true,
            Widget child = null
        ) {
            return new Transform(key, origin, alignment, transformHitTests, child, degree);
        }

        Transform(
            Key key = null,
            Offset offset = null,
            bool transformHitTests = true,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(offset != null);
            this.transform = Matrix3.makeTrans((float) offset.dx, (float) offset.dy);
            this.origin = null;
            this.alignment = null;
            this.transformHitTests = transformHitTests;
        }

        public static Transform translate(
            Key key = null,
            Offset offset = null,
            bool transformHitTests = true,
            Widget child = null
        ) {
            return new Transform(key, offset, transformHitTests, child);
        }

        Transform(
            Key key = null,
            double scale = 1.0,
            Offset origin = null,
            Alignment alignment = null,
            bool transformHitTests = true,
            Widget child = null
        ) : base(key: key, child: child) {
            this.transform = Matrix3.makeScale((float) scale, (float) scale);
            this.origin = origin;
            this.alignment = alignment;
            this.transformHitTests = transformHitTests;
        }

        public static Transform scale(
            Key key = null,
            double scale = 1.0,
            Offset origin = null,
            Alignment alignment = null,
            bool transformHitTests = true,
            Widget child = null
        ) {
            return new Transform(key, scale, origin, alignment, transformHitTests, child);
        }

        public readonly Matrix3 transform;
        public readonly Offset origin;
        public readonly Alignment alignment;
        public readonly bool transformHitTests;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderTransform(
                transform: this.transform,
                origin: this.origin,
                alignment: this.alignment,
                transformHitTests: this.transformHitTests
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderTransform) renderObjectRaw;
            renderObject.transform = this.transform;
            renderObject.origin = this.origin;
            renderObject.alignment = this.alignment;
            renderObject.transformHitTests = this.transformHitTests;
        }
    }

    public class CompositedTransformTarget : SingleChildRenderObjectWidget {
        public CompositedTransformTarget(
            Key key = null,
            LayerLink link = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(link != null);
            this.link = link;
        }

        public readonly LayerLink link;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderLeaderLayer(
                link: this.link
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderLeaderLayer) renderObject).link = this.link;
        }
    }

    public class CompositedTransformFollower : SingleChildRenderObjectWidget {
        public CompositedTransformFollower(
            Key key = null,
            LayerLink link = null,
            bool showWhenUnlinked = true,
            Offset offset = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(link != null);
            this.showWhenUnlinked = showWhenUnlinked;
            this.offset = offset ?? Offset.zero;
            this.link = link;
        }

        public readonly LayerLink link;
        public readonly bool showWhenUnlinked;
        public readonly Offset offset;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderFollowerLayer(
                link: this.link,
                showWhenUnlinked: this.showWhenUnlinked,
                offset: this.offset
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderFollowerLayer) renderObject).link = this.link;
            ((RenderFollowerLayer) renderObject).showWhenUnlinked = this.showWhenUnlinked;
            ((RenderFollowerLayer) renderObject).offset = this.offset;
        }
    }

    public class FractionalTranslation : SingleChildRenderObjectWidget {
        public FractionalTranslation(Key key = null, Offset translation = null,
            bool transformHitTests = true, Widget child = null) : base(key: key, child: child) {
            this.translation = translation;
            this.transformHitTests = transformHitTests;
        }

        public readonly Offset translation;
        public readonly bool transformHitTests;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderFractionalTranslation(
                translation: this.translation,
                transformHitTests: this.transformHitTests
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderFractionalTranslation) renderObject).translation = this.translation;
            ((RenderFractionalTranslation) renderObject).transformHitTests = this.transformHitTests;
        }
    }

    public class Align : SingleChildRenderObjectWidget {
        public Align(
            Key key = null,
            Alignment alignment = null,
            double? widthFactor = null,
            double? heightFactor = null,
            Widget child = null
        ) : base(key, child) {
            D.assert(widthFactor == null || widthFactor >= 0.0);
            D.assert(heightFactor == null || heightFactor >= 0.0);

            this.alignment = alignment ?? Alignment.center;
            this.widthFactor = widthFactor;
            this.heightFactor = heightFactor;
        }

        public readonly Alignment alignment;

        public readonly double? widthFactor;

        public readonly double? heightFactor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPositionedBox(
                alignment: this.alignment,
                widthFactor: this.widthFactor,
                heightFactor: this.heightFactor
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderPositionedBox) renderObjectRaw;
            renderObject.alignment = this.alignment;
            renderObject.widthFactor = this.widthFactor;
            renderObject.heightFactor = this.heightFactor;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment));
            properties.add(new DoubleProperty("widthFactor",
                this.widthFactor, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DoubleProperty("heightFactor",
                this.heightFactor, defaultValue: Diagnostics.kNullDefaultValue));
        }
    }

    public class Center : Align {
        public Center(
            Key key = null,
            double? widthFactor = null,
            double? heightFactor = null,
            Widget child = null)
            : base(
                key: key,
                widthFactor: widthFactor,
                heightFactor: heightFactor,
                child: child) {
        }
    }

    public class CustomSingleChildLayout : SingleChildRenderObjectWidget {
        public CustomSingleChildLayout(Key key = null,
            SingleChildLayoutDelegate layoutDelegate = null, Widget child = null) : base(key: key, child: child) {
            D.assert(layoutDelegate != null);
            this.layoutDelegate = layoutDelegate;
        }

        public readonly SingleChildLayoutDelegate layoutDelegate;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderCustomSingleChildLayoutBox(layoutDelegate: this.layoutDelegate);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderCustomSingleChildLayoutBox) renderObject).layoutDelegate = this.layoutDelegate;
        }
    }

    public class LayoutId : ParentDataWidget<CustomMultiChildLayout> {
        public LayoutId(
            Key key = null,
            object id = null,
            Widget child = null
        ) : base(key: key ?? new ValueKey<object>(id), child: child) {
            D.assert(child != null);
            D.assert(id != null);
            this.id = id;
        }

        public readonly object id;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is MultiChildLayoutParentData);
            MultiChildLayoutParentData parentData = (MultiChildLayoutParentData) renderObject.parentData;
            if (parentData.id != this.id) {
                parentData.id = this.id;
                var targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsLayout();
                }
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("id", this.id));
        }
    }

    public class CustomMultiChildLayout : MultiChildRenderObjectWidget {
        public CustomMultiChildLayout(
            Key key = null,
            MultiChildLayoutDelegate layoutDelegate = null,
            List<Widget> children = null
        ) : base(key: key, children: children ?? new List<Widget>()) {
            D.assert(layoutDelegate != null);
            this.layoutDelegate = layoutDelegate;
        }

        public readonly MultiChildLayoutDelegate layoutDelegate;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderCustomMultiChildLayoutBox(layoutDelegate: this.layoutDelegate);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderCustomMultiChildLayoutBox) renderObject).layoutDelegate = this.layoutDelegate;
        }
    }

    public static class LayoutUtils {
        public static AxisDirection getAxisDirectionFromAxisReverseAndDirectionality(
            BuildContext context,
            Axis axis,
            bool reverse
        ) {
            switch (axis) {
                case Axis.horizontal:
                    D.assert(WidgetsD.debugCheckHasDirectionality(context));
                    TextDirection textDirection = Directionality.of(context);
                    AxisDirection axisDirection = AxisUtils.textDirectionToAxisDirection(textDirection);
                    return reverse ? AxisUtils.flipAxisDirection(axisDirection) : axisDirection;
                case Axis.vertical:
                    return reverse ? AxisDirection.up : AxisDirection.down;
            }

            throw new Exception("unknown axisDirection");
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

    public class RichText : LeafRenderObjectWidget {
        public RichText(
            Key key = null,
            TextSpan text = null,
            TextAlign textAlign = TextAlign.left,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            double textScaleFactor = 1.0,
            int? maxLines = null
        ) : base(key: key) {
            D.assert(text != null);
            D.assert(maxLines == null || maxLines > 0);

            this.text = text;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
        }

        public readonly TextSpan text;
        public readonly TextAlign textAlign;
        public readonly bool softWrap;
        public readonly TextOverflow overflow;
        public readonly double textScaleFactor;
        public readonly int? maxLines;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderParagraph(
                this.text,
                textAlign: this.textAlign,
                softWrap: this.softWrap,
                overflow: this.overflow,
                textScaleFactor: this.textScaleFactor,
                maxLines: this.maxLines
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderParagraph) renderObjectRaw;
            renderObject.text = this.text;
            renderObject.textAlign = this.textAlign;
            renderObject.softWrap = this.softWrap;
            renderObject.overflow = this.overflow;
            renderObject.textScaleFactor = this.textScaleFactor;
            renderObject.maxLines = this.maxLines;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<TextAlign>("textAlign", this.textAlign, defaultValue: TextAlign.left));
            properties.add(new FlagProperty("softWrap", value: this.softWrap, ifTrue: "wrapping at box width",
                ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow>("overflow", this.overflow, defaultValue: TextOverflow.clip));
            properties.add(new DoubleProperty("textScaleFactor", this.textScaleFactor, defaultValue: 1.0));
            properties.add(new IntProperty("maxLines", this.maxLines, ifNull: "unlimited"));
            properties.add(new StringProperty("text", this.text.toPlainText()));
        }
    }

    public class RawImage : LeafRenderObjectWidget {
        public RawImage(
            Key key = null,
            ui.Image image = null,
            double? width = null,
            double? height = null,
            double scale = 1.0,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool invertColors = false,
            FilterMode filterMode = FilterMode.Point
        ) : base(key) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.scale = scale;
            this.color = color;
            this.colorBlendMode = colorBlendMode;
            this.fit = fit;
            this.alignment = alignment ?? Alignment.center;
            this.repeat = repeat;
            this.centerSlice = centerSlice;
            this.invertColors = invertColors;
            this.filterMode = filterMode;
        }

        public readonly ui.Image image;
        public readonly double? width;
        public readonly double? height;
        public readonly double scale;
        public readonly Color color;
        public readonly FilterMode filterMode;
        public readonly BlendMode colorBlendMode;
        public readonly BoxFit? fit;
        public readonly Alignment alignment;
        public readonly ImageRepeat repeat;
        public readonly Rect centerSlice;
        public readonly bool invertColors;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderImage(
                image: this.image,
                width: this.width,
                height: this.height,
                scale: this.scale,
                color: this.color,
                colorBlendMode: this.colorBlendMode,
                fit: this.fit,
                alignment: this.alignment,
                repeat: this.repeat,
                centerSlice: this.centerSlice,
                invertColors: this.invertColors,
                filterMode: this.filterMode
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var renderImage = (RenderImage) renderObject;

            renderImage.image = this.image;
            renderImage.width = this.width;
            renderImage.height = this.height;
            renderImage.scale = this.scale;
            renderImage.color = this.color;
            renderImage.colorBlendMode = this.colorBlendMode;
            renderImage.alignment = this.alignment;
            renderImage.fit = this.fit;
            renderImage.repeat = this.repeat;
            renderImage.centerSlice = this.centerSlice;
            renderImage.invertColors = this.invertColors;
            renderImage.filterMode = this.filterMode;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ui.Image>("image", this.image));
            properties.add(new DoubleProperty("width", this.width, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DoubleProperty("height", this.height, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DoubleProperty("scale", this.scale, defaultValue: 1.0));
            properties.add(new DiagnosticsProperty<Color>("color", this.color,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<BlendMode>("colorBlendMode", this.colorBlendMode,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<BoxFit?>("fit", this.fit, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<ImageRepeat>("repeat", this.repeat, defaultValue: ImageRepeat.noRepeat));
            properties.add(new DiagnosticsProperty<Rect>("centerSlice", this.centerSlice,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<bool>("invertColors", this.invertColors));
            properties.add(new EnumProperty<FilterMode>("filterMode", this.filterMode));
        }
    }

    public class DefaultAssetBundle : InheritedWidget {
        public DefaultAssetBundle(
            Key key = null,
            AssetBundle bundle = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(bundle != null);
            D.assert(child != null);
            this.bundle = bundle;
        }

        public readonly AssetBundle bundle;

        public static AssetBundle of(BuildContext context) {
            DefaultAssetBundle result =
                (DefaultAssetBundle) context.inheritFromWidgetOfExactType(typeof(DefaultAssetBundle));
            return result?.bundle;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.bundle != ((DefaultAssetBundle) oldWidget).bundle;
        }
    }

    public class Listener : SingleChildRenderObjectWidget {
        public Listener(
            Key key = null,
            PointerDownEventListener onPointerDown = null,
            PointerMoveEventListener onPointerMove = null,
            PointerUpEventListener onPointerUp = null,
            PointerCancelEventListener onPointerCancel = null,
            PointerHoverEventListener onPointerHover = null,
            PointerLeaveEventListener onPointerLeave = null,
            PointerEnterEventListener onPointerEnter = null,
            PointerScrollEventListener onPointerScroll = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            Widget child = null
        ) : base(key: key, child: child) {
            this.onPointerDown = onPointerDown;
            this.onPointerMove = onPointerMove;
            this.onPointerUp = onPointerUp;
            this.onPointerCancel = onPointerCancel;
            this.onPointerHover = onPointerHover;
            this.onPointerLeave = onPointerLeave;
            this.onPointerEnter = onPointerEnter;
            this.onPointerScroll = onPointerScroll;
            this.behavior = behavior;
        }

        public readonly PointerDownEventListener onPointerDown;

        public readonly PointerMoveEventListener onPointerMove;

        public readonly PointerUpEventListener onPointerUp;

        public readonly PointerCancelEventListener onPointerCancel;

        public readonly PointerHoverEventListener onPointerHover;

        public readonly PointerEnterEventListener onPointerEnter;

        public readonly PointerLeaveEventListener onPointerLeave;

        public readonly PointerScrollEventListener onPointerScroll;

        public readonly HitTestBehavior behavior;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPointerListener(
                onPointerDown: this.onPointerDown,
                onPointerMove: this.onPointerMove,
                onPointerUp: this.onPointerUp,
                onPointerCancel: this.onPointerCancel,
                onPointerEnter: this.onPointerEnter,
                onPointerLeave: this.onPointerLeave,
                onPointerHover: this.onPointerHover,
                onPointerScroll: this.onPointerScroll,
                behavior: this.behavior
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderPointerListener) renderObjectRaw;
            renderObject.onPointerDown = this.onPointerDown;
            renderObject.onPointerMove = this.onPointerMove;
            renderObject.onPointerUp = this.onPointerUp;
            renderObject.onPointerCancel = this.onPointerCancel;
            renderObject.onPointerEnter = this.onPointerEnter;
            renderObject.onPointerHover = this.onPointerHover;
            renderObject.onPointerLeave = this.onPointerLeave;
            renderObject.onPointerScroll = this.onPointerScroll;
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

            if (this.onPointerEnter != null) {
                listeners.Add("enter");
            }

            if (this.onPointerHover != null) {
                listeners.Add("hover");
            }

            if (this.onPointerLeave != null) {
                listeners.Add("leave");
            }

            if (this.onPointerScroll != null) {
                listeners.Add("scroll");
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
            List<RepaintBoundary> result = Enumerable.Repeat((RepaintBoundary) null, widgets.Count).ToList();
            for (int i = 0; i < result.Count; ++i) {
                result[i] = wrap(widgets[i], i);
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

    public class AbsorbPointer : SingleChildRenderObjectWidget {
        public AbsorbPointer(
            Key key = null,
            bool absorbing = true,
            Widget child = null
        ) : base(key: key, child: child) {
        }

        public readonly bool absorbing;


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAbsorbPointer(
                absorbing: this.absorbing
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderAbsorbPointer) renderObject).absorbing = this.absorbing;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("absorbing", this.absorbing));
        }
    }

    public class MetaData : SingleChildRenderObjectWidget {
        public MetaData(
            object metaData,
            Key key = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            Widget child = null) : base(key: key, child: child) {
            this.metaData = metaData;
            this.behavior = behavior;
        }

        public readonly object metaData;

        public readonly HitTestBehavior behavior;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderMetaData(
                metaData: this.metaData,
                behavior: this.behavior);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var renderObj = (RenderMetaData) renderObject;
            renderObj.metaData = this.metaData;
            renderObj.behavior = this.behavior;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<HitTestBehavior>("behavior", this.behavior));
            properties.add(new DiagnosticsProperty<object>("metaData", this.metaData));
        }
    }

    public class Builder : StatelessWidget {
        public Builder(
            Key key = null,
            WidgetBuilder builder = null
        ) : base(key: key) {
            D.assert(builder != null);
            this.builder = builder;
        }

        public readonly WidgetBuilder builder;

        public override Widget build(BuildContext context) {
            return this.builder(context);
        }
    }
}