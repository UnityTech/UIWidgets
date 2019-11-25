using System;
using System.Collections.Generic;
using RSG.Promises;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.material {
    class InputDecoratorConstants {
        public static readonly TimeSpan _kTransitionDuration = new TimeSpan(0, 0, 0, 0, 200);
        public static readonly Curve _kTransitionCurve = Curves.fastOutSlowIn;
    }


    class _InputBorderGap : ChangeNotifier, IEquatable<_InputBorderGap> {
        float _start;

        public float start {
            get { return this._start; }
            set {
                if (value != this._start) {
                    this._start = value;
                    this.notifyListeners();
                }
            }
        }

        float _extent = 0.0f;

        public float extent {
            get { return this._extent; }
            set {
                if (value != this._extent) {
                    this._extent = value;
                    this.notifyListeners();
                }
            }
        }

        public bool Equals(_InputBorderGap other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.start == other.start && this.extent == other._extent;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((_InputBorderGap) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this._start.GetHashCode() * 397) ^ this._extent.GetHashCode();
            }
        }

        public static bool operator ==(_InputBorderGap left, _InputBorderGap right) {
            return Equals(left, right);
        }

        public static bool operator !=(_InputBorderGap left, _InputBorderGap right) {
            return !Equals(left, right);
        }
    }

    class _InputBorderTween : Tween<InputBorder> {
        public _InputBorderTween(InputBorder begin = null, InputBorder end = null) : base(begin: begin, end: end) {
        }

        public override InputBorder lerp(float t) {
            return (InputBorder) ShapeBorder.lerp(this.begin, this.end, t);
        }
    }

    class _InputBorderPainter : AbstractCustomPainter {
        public _InputBorderPainter(
            Listenable repaint,
            Animation<float> borderAnimation = null,
            _InputBorderTween border = null,
            Animation<float> gapAnimation = null,
            _InputBorderGap gap = null,
            Color fillColor = null
        ) : base(repaint: repaint) {
            this.borderAnimation = borderAnimation;
            this.border = border;
            this.gapAnimation = gapAnimation;
            this.gap = gap;
            this.fillColor = fillColor;
        }

        public readonly Animation<float> borderAnimation;
        public readonly _InputBorderTween border;
        public readonly Animation<float> gapAnimation;
        public readonly _InputBorderGap gap;
        public readonly Color fillColor;

        public override void paint(Canvas canvas, Size size) {
            InputBorder borderValue = this.border.evaluate(this.borderAnimation);
            Rect canvasRect = Offset.zero & size;

            if (this.fillColor.alpha > 0) {
                Paint paint = new Paint();
                paint.color = this.fillColor;
                paint.style = PaintingStyle.fill;
                canvas.drawPath(
                    borderValue.getOuterPath(canvasRect),
                    paint
                );
            }

            borderValue.paint(
                canvas,
                canvasRect,
                gapStart: this.gap.start,
                gapExtent: this.gap.extent,
                gapPercentage: this.gapAnimation.value
            );
        }

        public override bool shouldRepaint(CustomPainter _oldPainter) {
            _InputBorderPainter oldPainter = _oldPainter as _InputBorderPainter;
            return this.borderAnimation != oldPainter.borderAnimation
                   || this.gapAnimation != oldPainter.gapAnimation
                   || this.border != oldPainter.border
                   || this.gap != oldPainter.gap;
        }
    }

    class _BorderContainer : StatefulWidget {
        public _BorderContainer(
            Key key = null,
            InputBorder border = null,
            _InputBorderGap gap = null,
            Animation<float> gapAnimation = null,
            Color fillColor = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(border != null);
            D.assert(gap != null);
            D.assert(fillColor != null);
            this.border = border;
            this.gap = gap;
            this.gapAnimation = gapAnimation;
            this.fillColor = fillColor;
            this.child = child;
        }

        public readonly InputBorder border;
        public readonly _InputBorderGap gap;
        public readonly Animation<float> gapAnimation;
        public readonly Color fillColor;
        public readonly Widget child;

        public override State createState() {
            return new _BorderContainerState();
        }
    }

    class _BorderContainerState : SingleTickerProviderStateMixin<_BorderContainer> {
        AnimationController _controller;
        Animation<float> _borderAnimation;
        _InputBorderTween _border;

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(
                duration: InputDecoratorConstants._kTransitionDuration,
                vsync: this
            );
            this._borderAnimation = new CurvedAnimation(
                parent: this._controller,
                curve: InputDecoratorConstants._kTransitionCurve
            );
            this._border = new _InputBorderTween(
                begin: this.widget.border,
                end: this.widget.border
            );
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            _BorderContainer oldWidget = _oldWidget as _BorderContainer;
            base.didUpdateWidget(oldWidget);
            if (this.widget.border != oldWidget.border) {
                this._border = new _InputBorderTween(
                    begin: oldWidget.border,
                    end: this.widget.border
                );
                this._controller.setValue(0.0f);
                this._controller.forward();
            }
        }

        public override Widget build(BuildContext context) {
            return new CustomPaint(
                foregroundPainter: new _InputBorderPainter(
                    repaint: ListenableUtils.merge(new List<Listenable> {this._borderAnimation, this.widget.gap}),
                    borderAnimation: this._borderAnimation,
                    border: this._border,
                    gapAnimation: this.widget.gapAnimation,
                    gap: this.widget.gap,
                    fillColor: this.widget.fillColor
                ),
                child: this.widget.child
            );
        }
    }

    class _Shaker : AnimatedWidget {
        public _Shaker(
            Key key = null,
            Animation<float> animation = null,
            Widget child = null
        ) : base(key: key, listenable: animation) {
            this.child = child;
        }

        public readonly Widget child;

        public Animation<float> animation {
            get { return (Animation<float>) this.listenable; }
        }

        public float translateX {
            get {
                const float shakeDelta = 4.0f;
                float t = this.animation.value;
                if (t <= 0.25f) {
                    return -t * shakeDelta;
                }
                else if (t < 0.75f) {
                    return (t - 0.5f) * shakeDelta;
                }
                else {
                    return (1.0f - t) * 4.0f * shakeDelta;
                }
            }
        }

        protected internal override Widget build(BuildContext context) {
            return new Transform(
                transform: Matrix3.makeTrans(this.translateX, 0.0f),
                child: this.child
            );
        }
    }

    class _HelperError : StatefulWidget {
        public _HelperError(
            Key key = null,
            TextAlign? textAlign = null,
            string helperText = null,
            TextStyle helperStyle = null,
            string errorText = null,
            TextStyle errorStyle = null,
            int? errorMaxLines = null
        ) : base(key: key) {
            this.textAlign = textAlign;
            this.helperText = helperText;
            this.helperStyle = helperStyle;
            this.errorText = errorText;
            this.errorStyle = errorStyle;
            this.errorMaxLines = errorMaxLines;
        }

        public readonly TextAlign? textAlign;
        public readonly string helperText;
        public readonly TextStyle helperStyle;
        public readonly string errorText;
        public readonly TextStyle errorStyle;
        public readonly int? errorMaxLines;

        public override State createState() {
            return new _HelperErrorState();
        }
    }

    class _HelperErrorState : SingleTickerProviderStateMixin<_HelperError> {
        static readonly Widget empty = new SizedBox();

        AnimationController _controller;
        Widget _helper;
        Widget _error;

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(
                duration: InputDecoratorConstants._kTransitionDuration,
                vsync: this
            );
            if (this.widget.errorText != null) {
                this._error = this._buildError();
                this._controller.setValue(1.0f);
            }
            else if (this.widget.helperText != null) {
                this._helper = this._buildHelper();
            }

            this._controller.addListener(this._handleChange);
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        void _handleChange() {
            this.setState(() => { });
        }

        public override void didUpdateWidget(StatefulWidget _old) {
            base.didUpdateWidget(_old);

            _HelperError old = _old as _HelperError;

            string newErrorText = this.widget.errorText;
            string newHelperText = this.widget.helperText;
            string oldErrorText = old.errorText;
            string oldHelperText = old.helperText;

            bool errorTextStateChanged = (newErrorText != null) != (oldErrorText != null);
            bool helperTextStateChanged = newErrorText == null && (newHelperText != null) != (oldHelperText != null);

            if (errorTextStateChanged || helperTextStateChanged) {
                if (newErrorText != null) {
                    this._error = this._buildError();
                    this._controller.forward();
                }
                else if (newHelperText != null) {
                    this._helper = this._buildHelper();
                    this._controller.reverse();
                }
                else {
                    this._controller.reverse();
                }
            }
        }

        Widget _buildHelper() {
            D.assert(this.widget.helperText != null);
            return new Opacity(
                opacity: 1.0f - this._controller.value,
                child: new Text(
                    this.widget.helperText,
                    style: this.widget.helperStyle,
                    textAlign: this.widget.textAlign,
                    overflow: TextOverflow.ellipsis
                )
            );
        }

        Widget _buildError() {
            D.assert(this.widget.errorText != null);
            return new Opacity(
                opacity: this._controller.value,
                child: new FractionalTranslation(
                    translation: new OffsetTween(
                        begin: new Offset(0.0f, -0.25f),
                        end: new Offset(0.0f, 0.0f)
                    ).evaluate(this._controller.view),
                    child: new Text(
                        this.widget.errorText,
                        style: this.widget.errorStyle,
                        textAlign: this.widget.textAlign,
                        overflow: TextOverflow.ellipsis,
                        maxLines: this.widget.errorMaxLines
                    )
                )
            );
        }

        public override Widget build(BuildContext context) {
            if (this._controller.isDismissed) {
                this._error = null;
                if (this.widget.helperText != null) {
                    return this._helper = this._buildHelper();
                }
                else {
                    this._helper = null;
                    return empty;
                }
            }

            if (this._controller.isCompleted) {
                this._helper = null;
                if (this.widget.errorText != null) {
                    return this._error = this._buildError();
                }
                else {
                    this._error = null;
                    return empty;
                }
            }

            if (this._helper == null && this.widget.errorText != null) {
                return this._buildError();
            }

            if (this._error == null && this.widget.helperText != null) {
                return this._buildHelper();
            }

            if (this.widget.errorText != null) {
                return new Stack(
                    children: new List<Widget> {
                        new Opacity(
                            opacity: 1.0f - this._controller.value,
                            child: this._helper
                        ),
                        this._buildError(),
                    }
                );
            }

            if (this.widget.helperText != null) {
                return new Stack(
                    children: new List<Widget> {
                        this._buildHelper(),
                        new Opacity(
                            opacity: this._controller.value,
                            child: this._error
                        )
                    }
                );
            }

            return empty;
        }
    }

    enum _DecorationSlot {
        icon,
        input,
        label,
        hint,
        prefix,
        suffix,
        prefixIcon,
        suffixIcon,
        helperError,
        counter,
        container
    }

    class _Decoration : IEquatable<_Decoration> {
        public _Decoration(
            EdgeInsets contentPadding,
            bool isCollapsed,
            float floatingLabelHeight,
            float floatingLabelProgress,
            InputBorder border = null,
            _InputBorderGap borderGap = null,
            Widget icon = null,
            Widget input = null,
            Widget label = null,
            Widget hint = null,
            Widget prefix = null,
            Widget suffix = null,
            Widget prefixIcon = null,
            Widget suffixIcon = null,
            Widget helperError = null,
            Widget counter = null,
            Widget container = null,
            bool? alignLabelWithHint = null
        ) {
            D.assert(contentPadding != null);
            this.contentPadding = contentPadding;
            this.isCollapsed = isCollapsed;
            this.floatingLabelHeight = floatingLabelHeight;
            this.floatingLabelProgress = floatingLabelProgress;
            this.border = border;
            this.borderGap = borderGap;
            this.icon = icon;
            this.input = input;
            this.label = label;
            this.hint = hint;
            this.prefix = prefix;
            this.suffix = suffix;
            this.prefixIcon = prefixIcon;
            this.suffixIcon = suffixIcon;
            this.helperError = helperError;
            this.counter = counter;
            this.container = container;
            this.alignLabelWithHint = alignLabelWithHint;
        }

        public readonly EdgeInsets contentPadding;
        public readonly bool isCollapsed;
        public readonly float floatingLabelHeight;
        public readonly float floatingLabelProgress;
        public readonly InputBorder border;
        public readonly _InputBorderGap borderGap;
        public readonly bool? alignLabelWithHint;
        public readonly Widget icon;
        public readonly Widget input;
        public readonly Widget label;
        public readonly Widget hint;
        public readonly Widget prefix;
        public readonly Widget suffix;
        public readonly Widget prefixIcon;
        public readonly Widget suffixIcon;
        public readonly Widget helperError;
        public readonly Widget counter;
        public readonly Widget container;

        public bool Equals(_Decoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.contentPadding, other.contentPadding) && this.isCollapsed == other.isCollapsed &&
                   this.floatingLabelHeight.Equals(other.floatingLabelHeight) &&
                   this.floatingLabelProgress.Equals(other.floatingLabelProgress) &&
                   Equals(this.border, other.border) && Equals(this.borderGap, other.borderGap) &&
                   Equals(this.icon, other.icon) && Equals(this.input, other.input) &&
                   Equals(this.label, other.label) && Equals(this.hint, other.hint) &&
                   Equals(this.prefix, other.prefix) && Equals(this.suffix, other.suffix) &&
                   Equals(this.prefixIcon, other.prefixIcon) && Equals(this.suffixIcon, other.suffixIcon) &&
                   Equals(this.helperError, other.helperError) && Equals(this.counter, other.counter) &&
                   Equals(this.container, other.container) && Equals(this.alignLabelWithHint, other.alignLabelWithHint);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((_Decoration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.contentPadding != null ? this.contentPadding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.isCollapsed.GetHashCode();
                hashCode = (hashCode * 397) ^ this.floatingLabelHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ this.floatingLabelProgress.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.border != null ? this.border.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.borderGap != null ? this.borderGap.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.icon != null ? this.icon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.input != null ? this.input.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.label != null ? this.label.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.hint != null ? this.hint.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.prefix != null ? this.prefix.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.suffix != null ? this.suffix.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.prefixIcon != null ? this.prefixIcon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.suffixIcon != null ? this.suffixIcon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.helperError != null ? this.helperError.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.counter != null ? this.counter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.container != null ? this.container.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^
                           (this.alignLabelWithHint != null ? this.alignLabelWithHint.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(_Decoration left, _Decoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(_Decoration left, _Decoration right) {
            return !Equals(left, right);
        }
    }

    class _RenderDecorationLayout {
        public _RenderDecorationLayout(
            Dictionary<RenderBox, float> boxToBaseline = null,
            float? inputBaseline = null,
            float? outlineBaseline = null,
            float? subtextBaseline = null,
            float? containerHeight = null,
            float? subtextHeight = null
        ) {
            this.boxToBaseline = boxToBaseline;
            this.inputBaseline = inputBaseline;
            this.outlineBaseline = outlineBaseline;
            this.subtextBaseline = subtextBaseline;
            this.containerHeight = containerHeight;
            this.subtextHeight = subtextHeight;
        }

        public readonly Dictionary<RenderBox, float> boxToBaseline;
        public readonly float? inputBaseline;
        public readonly float? outlineBaseline;
        public readonly float? subtextBaseline;
        public readonly float? containerHeight;
        public readonly float? subtextHeight;
    }

    class _RenderDecoration : RenderBox {
        public _RenderDecoration(
            _Decoration decoration,
            TextBaseline? textBaseline,
            bool isFocused,
            bool expands
        ) {
            D.assert(decoration != null);
            D.assert(textBaseline != null);
            this._decoration = decoration;
            this._textBaseline = textBaseline;
            this._isFocused = isFocused;
            this._expands = expands;
        }

        public const float subtextGap = 8.0f;

        public readonly Dictionary<_DecorationSlot, RenderBox> slotToChild =
            new Dictionary<_DecorationSlot, RenderBox>();

        public readonly Dictionary<RenderBox, _DecorationSlot> childToSlot =
            new Dictionary<RenderBox, _DecorationSlot>();

        RenderBox _updateChild(RenderBox oldChild, RenderBox newChild, _DecorationSlot slot) {
            if (oldChild != null) {
                this.dropChild(oldChild);
                this.childToSlot.Remove(oldChild);
                this.slotToChild.Remove(slot);
            }

            if (newChild != null) {
                this.childToSlot[newChild] = slot;
                this.slotToChild[slot] = newChild;
                this.adoptChild(newChild);
            }

            return newChild;
        }

        RenderBox _icon;

        public RenderBox icon {
            get { return this._icon; }
            set { this._icon = this._updateChild(this._icon, value, _DecorationSlot.icon); }
        }

        RenderBox _input;

        public RenderBox input {
            get { return this._input; }
            set { this._input = this._updateChild(this._input, value, _DecorationSlot.input); }
        }

        RenderBox _label;

        public RenderBox label {
            get { return this._label; }
            set { this._label = this._updateChild(this._label, value, _DecorationSlot.label); }
        }

        RenderBox _hint;

        public RenderBox hint {
            get { return this._hint; }
            set { this._hint = this._updateChild(this._hint, value, _DecorationSlot.hint); }
        }

        RenderBox _prefix;

        public RenderBox prefix {
            get { return this._prefix; }
            set { this._prefix = this._updateChild(this._prefix, value, _DecorationSlot.prefix); }
        }

        RenderBox _suffix;

        public RenderBox suffix {
            get { return this._suffix; }
            set { this._suffix = this._updateChild(this._suffix, value, _DecorationSlot.suffix); }
        }

        RenderBox _prefixIcon;

        public RenderBox prefixIcon {
            get { return this._prefixIcon; }
            set { this._prefixIcon = this._updateChild(this._prefixIcon, value, _DecorationSlot.prefixIcon); }
        }

        RenderBox _suffixIcon;

        public RenderBox suffixIcon {
            get { return this._suffixIcon; }
            set { this._suffixIcon = this._updateChild(this._suffixIcon, value, _DecorationSlot.suffixIcon); }
        }

        RenderBox _helperError;

        public RenderBox helperError {
            get { return this._helperError; }
            set { this._helperError = this._updateChild(this._helperError, value, _DecorationSlot.helperError); }
        }

        RenderBox _counter;

        public RenderBox counter {
            get { return this._counter; }
            set { this._counter = this._updateChild(this._counter, value, _DecorationSlot.counter); }
        }

        RenderBox _container;

        public RenderBox container {
            get { return this._container; }
            set { this._container = this._updateChild(this._container, value, _DecorationSlot.container); }
        }

        IEnumerable<RenderBox> _children {
            get {
                if (this.icon != null) {
                    yield return this.icon;
                }

                if (this.input != null) {
                    yield return this.input;
                }

                if (this.prefixIcon != null) {
                    yield return this.prefixIcon;
                }

                if (this.suffixIcon != null) {
                    yield return this.suffixIcon;
                }

                if (this.prefix != null) {
                    yield return this.prefix;
                }

                if (this.suffix != null) {
                    yield return this.suffix;
                }

                if (this.label != null) {
                    yield return this.label;
                }

                if (this.hint != null) {
                    yield return this.hint;
                }

                if (this.helperError != null) {
                    yield return this.helperError;
                }

                if (this.counter != null) {
                    yield return this.counter;
                }

                if (this.container != null) {
                    yield return this.container;
                }
            }
        }

        public _Decoration decoration {
            get { return this._decoration; }
            set {
                D.assert(value != null);
                if (this._decoration == value) {
                    return;
                }

                this._decoration = value;
                this.markNeedsLayout();
            }
        }

        _Decoration _decoration;

        public TextBaseline? textBaseline {
            get { return this._textBaseline; }
            set {
                D.assert(value != null);
                if (this._textBaseline == value) {
                    return;
                }

                this._textBaseline = value;
                this.markNeedsLayout();
            }
        }

        TextBaseline? _textBaseline;

        public bool isFocused {
            get { return this._isFocused; }
            set {
                if (this._isFocused == value) {
                    return;
                }

                this._isFocused = value;
            }
        }

        bool _isFocused;

        public bool expands {
            get { return this._expands; }
            set {
                if (this._expands == value) {
                    return;
                }

                this._expands = value;
                this.markNeedsLayout();
            }
        }

        bool _expands = false;

        public override void attach(object owner) {
            base.attach(owner);
            foreach (RenderBox child in this._children) {
                child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            foreach (RenderBox child in this._children) {
                child.detach();
            }
        }

        public override void redepthChildren() {
            this._children.Each(this.redepthChild);
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            this._children.Each((child) => { visitor(child); });
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            List<DiagnosticsNode> value = new List<DiagnosticsNode> { };

            void add(RenderBox child, string name) {
                if (child != null) {
                    value.Add(child.toDiagnosticsNode(name: name));
                }
            }

            add(this.icon, "icon");
            add(this.input, "input");
            add(this.label, "label");
            add(this.hint, "hint");
            add(this.prefix, "prefix");
            add(this.suffix, "suffix");
            add(this.prefixIcon, "prefixIcon");
            add(this.suffixIcon, "suffixIcon");
            add(this.helperError, "helperError");
            add(this.counter, "counter");
            add(this.container, "container");
            return value;
        }

        protected override bool sizedByParent {
            get { return false; }
        }

        static float _minWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMinIntrinsicWidth(height);
        }

        static float _maxWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMaxIntrinsicWidth(height);
        }

        static float _minHeight(RenderBox box, float width) {
            return box == null ? 0.0f : box.getMinIntrinsicHeight(width);
        }

        static Size _boxSize(RenderBox box) {
            return box == null ? Size.zero : box.size;
        }

        static BoxParentData _boxParentData(RenderBox box) {
            return (BoxParentData) box.parentData;
        }

        public EdgeInsets contentPadding {
            get { return this.decoration.contentPadding; }
        }

        float _layoutLineBox(RenderBox box, BoxConstraints constraints) {
            if (box == null) {
                return 0.0f;
            }

            box.layout(constraints, parentUsesSize: true);
            float baseline = box.getDistanceToBaseline(this.textBaseline.Value).Value;
            D.assert(baseline >= 0.0f);
            return baseline;
        }

        _RenderDecorationLayout _layout(BoxConstraints layoutConstraints) {
            Dictionary<RenderBox, float> boxToBaseline = new Dictionary<RenderBox, float>();
            BoxConstraints boxConstraints = layoutConstraints.loosen();
            if (this.prefix != null) {
                boxToBaseline[this.prefix] = this._layoutLineBox(this.prefix, boxConstraints);
            }
            if (this.suffix != null) {
                boxToBaseline[this.suffix] = this._layoutLineBox(this.suffix, boxConstraints);
            }
            if (this.icon != null) {
                boxToBaseline[this.icon] = this._layoutLineBox(this.icon, boxConstraints);
            }
            if (this.prefixIcon != null) {
                boxToBaseline[this.prefixIcon] = this._layoutLineBox(this.prefixIcon, boxConstraints);
            }
            if (this.suffixIcon != null) {
                boxToBaseline[this.suffixIcon] = this._layoutLineBox(this.suffixIcon, boxConstraints);
            }

            float inputWidth = Math.Max(0.0f, this.constraints.maxWidth - (
                                                  _boxSize(this.icon).width
                                                  + this.contentPadding.left
                                                  + _boxSize(this.prefixIcon).width
                                                  + _boxSize(this.prefix).width
                                                  + _boxSize(this.suffix).width
                                                  + _boxSize(this.suffixIcon).width
                                                  + this.contentPadding.right));
            if (this.label != null) {
                boxToBaseline[this.label] = this._layoutLineBox(this.label,
                    boxConstraints.copyWith(maxWidth: inputWidth)
                );
            }

            if (this.hint != null) {
                boxToBaseline[this.hint] = this._layoutLineBox(this.hint,
                    boxConstraints.copyWith(minWidth: inputWidth, maxWidth: inputWidth)
                );
            }

            if (this.counter != null) {
                boxToBaseline[this.counter] = this._layoutLineBox(this.counter, boxConstraints);
            }

            if (this.helperError != null) {
                boxToBaseline[this.helperError] = this._layoutLineBox(this.helperError,
                    boxConstraints.copyWith(
                        maxWidth: Math.Max(0.0f, boxConstraints.maxWidth
                                                 - _boxSize(this.icon).width
                                                 - _boxSize(this.counter).width
                                                 - this.contentPadding.horizontal
                        )
                    )
                );
            }

            float labelHeight = this.label == null
                ? 0
                : this.decoration.floatingLabelHeight;
            float topHeight = this.decoration.border.isOutline
                ? Math.Max(labelHeight - boxToBaseline.getOrDefault(this.label, 0), 0)
                : labelHeight;
            float counterHeight = this.counter == null
                ? 0
                : boxToBaseline.getOrDefault(this.counter, 0) + subtextGap;
            bool helperErrorExists = this.helperError?.size != null
                                     && this.helperError.size.height > 0;
            float helperErrorHeight = !helperErrorExists
                ? 0
                : this.helperError.size.height + subtextGap;
            float bottomHeight = Math.Max(
                counterHeight,
                helperErrorHeight
            );
            if (this.input != null) {
                boxToBaseline[this.input] = this._layoutLineBox(this.input,
                    boxConstraints.deflate(EdgeInsets.only(
                        top: this.contentPadding.top + topHeight,
                        bottom: this.contentPadding.bottom + bottomHeight
                    )).copyWith(
                        minWidth: inputWidth,
                        maxWidth: inputWidth
                    )
                );
            }

            // The field can be occupied by a hint or by the input itself
            float hintHeight = this.hint == null ? 0 : this.hint.size.height;
            float inputDirectHeight = this.input == null ? 0 : this.input.size.height;
            float inputHeight = Math.Max(hintHeight, inputDirectHeight);
            float inputInternalBaseline = Math.Max(
                boxToBaseline.getOrDefault(this.input, 0.0f),
                boxToBaseline.getOrDefault(this.hint, 0.0f)
            );

            // Calculate the amount that prefix/suffix affects height above and below
            // the input.
            float prefixHeight = this.prefix == null ? 0 : this.prefix.size.height;
            float suffixHeight = this.suffix == null ? 0 : this.suffix.size.height;
            float fixHeight = Math.Max(
                boxToBaseline.getOrDefault(this.prefix, 0.0f),
                boxToBaseline.getOrDefault(this.suffix, 0.0f)
            );
            float fixAboveInput = Math.Max(0, fixHeight - inputInternalBaseline);
            float fixBelowBaseline = Math.Max(
                prefixHeight - boxToBaseline.getOrDefault(this.prefix, 0.0f),
                suffixHeight - boxToBaseline.getOrDefault(this.suffix, 0.0f)
            );
            float fixBelowInput = Math.Max(
                0,
                fixBelowBaseline - (inputHeight - inputInternalBaseline)
            );

            // Calculate the height of the input text container.
            float prefixIconHeight = this.prefixIcon == null ? 0 : this.prefixIcon.size.height;
            float suffixIconHeight = this.suffixIcon == null ? 0 : this.suffixIcon.size.height;
            float fixIconHeight = Math.Max(prefixIconHeight, suffixIconHeight);
            float contentHeight = Math.Max(
                fixIconHeight,
                topHeight
                + this.contentPadding.top
                + fixAboveInput
                + inputHeight
                + fixBelowInput
                + this.contentPadding.bottom
            );
            float maxContainerHeight = boxConstraints.maxHeight - bottomHeight;
            float containerHeight = this.expands
                ? maxContainerHeight
                : Math.Min(contentHeight, maxContainerHeight);

            // Always position the prefix/suffix in the same place (baseline).
            float overflow = Math.Max(0, contentHeight - maxContainerHeight);
            float baselineAdjustment = fixAboveInput - overflow;

            // The baselines that will be used to draw the actual input text content.
            float inputBaseline = this.contentPadding.top
                                  + topHeight
                                  + inputInternalBaseline
                                  + baselineAdjustment;
            // The text in the input when an outline border is present is centered
            // within the container less 2.0 dps at the top to account for the vertical
            // space occupied by the floating label.
            float outlineBaseline = inputInternalBaseline
                                    + baselineAdjustment / 2
                                    + (containerHeight - (2.0f + inputHeight)) / 2.0f;

            // Find the positions of the text below the input when it exists.
            float subtextCounterBaseline = 0;
            float subtextHelperBaseline = 0;
            float subtextCounterHeight = 0;
            float subtextHelperHeight = 0;
            if (this.counter != null) {
                subtextCounterBaseline =
                    containerHeight + subtextGap + boxToBaseline.getOrDefault(this.counter, 0.0f);
                subtextCounterHeight = this.counter.size.height + subtextGap;
            }

            if (helperErrorExists) {
                subtextHelperBaseline =
                    containerHeight + subtextGap + boxToBaseline.getOrDefault(this.helperError, 0.0f);
                subtextHelperHeight = helperErrorHeight;
            }

            float subtextBaseline = Math.Max(
                subtextCounterBaseline,
                subtextHelperBaseline
            );
            float subtextHeight = Math.Max(
                subtextCounterHeight,
                subtextHelperHeight
            );

            return new _RenderDecorationLayout(
                boxToBaseline: boxToBaseline,
                containerHeight: containerHeight,
                inputBaseline: inputBaseline,
                outlineBaseline: outlineBaseline,
                subtextBaseline: subtextBaseline,
                subtextHeight: subtextHeight
            );
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return _minWidth(this.icon, height)
                   + this.contentPadding.left
                   + _minWidth(this.prefixIcon, height)
                   + _minWidth(this.prefix, height)
                   + Mathf.Max(_minWidth(this.input, height), _minWidth(this.hint, height))
                   + _minWidth(this.suffix, height)
                   + _minWidth(this.suffixIcon, height)
                   + this.contentPadding.right;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return _maxWidth(this.icon, height)
                   + this.contentPadding.left
                   + _maxWidth(this.prefixIcon, height)
                   + _maxWidth(this.prefix, height)
                   + Mathf.Max(_maxWidth(this.input, height), _maxWidth(this.hint, height))
                   + _maxWidth(this.suffix, height)
                   + _maxWidth(this.suffixIcon, height)
                   + this.contentPadding.right;
        }

        float _lineHeight(float width, List<RenderBox> boxes) {
            float height = 0.0f;
            foreach (RenderBox box in boxes) {
                if (box == null) {
                    continue;
                }

                height = Mathf.Max(_minHeight(box, width), height);
            }

            return height;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float subtextHeight = this._lineHeight(width, new List<RenderBox> {this.helperError, this.counter});
            if (subtextHeight > 0.0f) {
                subtextHeight += subtextGap;
            }

            return this.contentPadding.top
                   + (this.label == null ? 0.0f : this.decoration.floatingLabelHeight)
                   + this._lineHeight(width, new List<RenderBox> {this.prefix, this.input, this.suffix})
                   + subtextHeight
                   + this.contentPadding.bottom;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this.computeMinIntrinsicHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return _boxParentData(this.input).offset.dy + this.input.getDistanceToActualBaseline(baseline);
        }

        Matrix3 _labelTransform;

        protected override void performLayout() {
            this._labelTransform = null;
            _RenderDecorationLayout layout = this._layout(this.constraints);

            float overallWidth = this.constraints.maxWidth;
            float? overallHeight = layout.containerHeight + layout.subtextHeight;

            if (this.container != null) {
                BoxConstraints containerConstraints = BoxConstraints.tightFor(
                    height: layout.containerHeight,
                    width: overallWidth - _boxSize(this.icon).width
                );
                this.container.layout(containerConstraints, parentUsesSize: true);
                float x = _boxSize(this.icon).width;
                _boxParentData(this.container).offset = new Offset(x, 0.0f);
            }

            float height;

            float centerLayout(RenderBox box, float x) {
                _boxParentData(box).offset = new Offset(x, (height - box.size.height) / 2.0f);
                return box.size.width;
            }

            float baseline;

            float baselineLayout(RenderBox box, float x) {
                _boxParentData(box).offset = new Offset(x, baseline - layout.boxToBaseline[box]);
                return box.size.width;
            }

            float left = this.contentPadding.left;
            float right = overallWidth - this.contentPadding.right;

            height = layout.containerHeight ?? 0.0f;
            baseline = (this.decoration.isCollapsed || !this.decoration.border.isOutline
                           ? layout.inputBaseline
                           : layout.outlineBaseline) ?? 0.0f;

            if (this.icon != null) {
                float x = 0.0f;

                centerLayout(this.icon, x);
            }

            float start = left + _boxSize(this.icon).width;
            float end = right;
            if (this.prefixIcon != null) {
                start -= this.contentPadding.left;
                start += centerLayout(this.prefixIcon, start);
            }

            if (this.label != null) {
                if (this.decoration.alignLabelWithHint == true) {
                    baselineLayout(this.label, start);
                }
                else {
                    centerLayout(this.label, start);
                }
            }

            if (this.prefix != null) {
                start += baselineLayout(this.prefix, start);
            }

            if (this.input != null) {
                baselineLayout(this.input, start);
            }

            if (this.hint != null) {
                baselineLayout(this.hint, start);
            }

            if (this.suffixIcon != null) {
                end += this.contentPadding.right;
                end -= centerLayout(this.suffixIcon, end - this.suffixIcon.size.width);
            }

            if (this.suffix != null) {
                end -= baselineLayout(this.suffix, end - this.suffix.size.width);
            }

            if (this.helperError != null || this.counter != null) {
                height = layout.subtextHeight ?? 0.0f;
                baseline = layout.subtextBaseline ?? 0.0f;

                if (this.helperError != null) {
                    baselineLayout(this.helperError, left + _boxSize(this.icon).width);
                }

                if (this.counter != null) {
                    baselineLayout(this.counter, right - this.counter.size.width);
                }
            }

            if (this.label != null) {
                float labelX = _boxParentData(this.label).offset.dx;
                this.decoration.borderGap.start = labelX - _boxSize(this.icon).width;

                this.decoration.borderGap.extent = this.label.size.width * 0.75f;
            }
            else {
                this.decoration.borderGap.start = 0.0f;
                this.decoration.borderGap.extent = 0.0f;
            }

            this.size = this.constraints.constrain(new Size(overallWidth, overallHeight ?? 0.0f));
            D.assert(this.size.width == this.constraints.constrainWidth(overallWidth));
            D.assert(this.size.height == this.constraints.constrainHeight(overallHeight ?? 0.0f));
        }

        void _paintLabel(PaintingContext context, Offset offset) {
            context.paintChild(this.label, offset);
        }

        public override void paint(PaintingContext context, Offset offset) {
            void doPaint(RenderBox child) {
                if (child != null) {
                    context.paintChild(child, _boxParentData(child).offset + offset);
                }
            }

            doPaint(this.container);

            if (this.label != null) {
                Offset labelOffset = _boxParentData(this.label).offset;
                float labelHeight = this.label.size.height;
                float t = this.decoration.floatingLabelProgress;
                bool isOutlineBorder = this.decoration.border != null && this.decoration.border.isOutline;
                float floatingY = isOutlineBorder ? -labelHeight * 0.25f : this.contentPadding.top;
                float scale = MathUtils.lerpFloat(1.0f, 0.75f, t);
                float dx = labelOffset.dx;
                float dy = MathUtils.lerpFloat(0.0f, floatingY - labelOffset.dy, t);
                this._labelTransform = Matrix3.I();
                this._labelTransform.preTranslate(dx, labelOffset.dy + dy);
                this._labelTransform.preScale(scale, scale);
                context.pushTransform(this.needsCompositing, offset, this._labelTransform, this._paintLabel);
            }

            doPaint(this.icon);
            doPaint(this.prefix);
            doPaint(this.suffix);
            doPaint(this.prefixIcon);
            doPaint(this.suffixIcon);
            doPaint(this.hint);
            doPaint(this.input);
            doPaint(this.helperError);
            doPaint(this.counter);
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position) {
            D.assert(position != null);
            foreach (RenderBox child in this._children) {
                if (child.hitTest(result, position: position - _boxParentData(child).offset)) {
                    return true;
                }
            }

            return false;
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            if (child == this.label && this._labelTransform != null) {
                Offset labelOffset = _boxParentData(this.label).offset;
                transform.preConcat(this._labelTransform);
                transform.preTranslate(-labelOffset.dx, -labelOffset.dy);
            }

            base.applyPaintTransform(child, transform);
        }
    }

    class _RenderDecorationElement : RenderObjectElement {
        public _RenderDecorationElement(_Decorator widget) : base(widget) {
        }

        Dictionary<_DecorationSlot, Element> slotToChild = new Dictionary<_DecorationSlot, Element>();
        Dictionary<Element, _DecorationSlot> childToSlot = new Dictionary<Element, _DecorationSlot>();

        public new _Decorator widget {
            get { return (_Decorator) base.widget; }
        }

        public new _RenderDecoration renderObject {
            get { return (_RenderDecoration) base.renderObject; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            this.slotToChild.Values.Each((child) => { visitor(child); });
        }

        protected override void forgetChild(Element child) {
            D.assert(this.slotToChild.ContainsValue(child));
            D.assert(this.childToSlot.ContainsKey(child));
            _DecorationSlot slot = this.childToSlot[child];
            this.childToSlot.Remove(child);
            this.slotToChild.Remove(slot);
        }

        void _mountChild(Widget widget, _DecorationSlot slot) {
            Element oldChild = this.slotToChild.getOrDefault(slot);
            Element newChild = this.updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                this.slotToChild.Remove(slot);
                this.childToSlot.Remove(oldChild);
            }

            if (newChild != null) {
                this.slotToChild[slot] = newChild;
                this.childToSlot[newChild] = slot;
            }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            this._mountChild(this.widget.decoration.icon, _DecorationSlot.icon);
            this._mountChild(this.widget.decoration.input, _DecorationSlot.input);
            this._mountChild(this.widget.decoration.label, _DecorationSlot.label);
            this._mountChild(this.widget.decoration.hint, _DecorationSlot.hint);
            this._mountChild(this.widget.decoration.prefix, _DecorationSlot.prefix);
            this._mountChild(this.widget.decoration.suffix, _DecorationSlot.suffix);
            this._mountChild(this.widget.decoration.prefixIcon, _DecorationSlot.prefixIcon);
            this._mountChild(this.widget.decoration.suffixIcon, _DecorationSlot.suffixIcon);
            this._mountChild(this.widget.decoration.helperError, _DecorationSlot.helperError);
            this._mountChild(this.widget.decoration.counter, _DecorationSlot.counter);
            this._mountChild(this.widget.decoration.container, _DecorationSlot.container);
        }

        void _updateChild(Widget widget, _DecorationSlot slot) {
            Element oldChild = this.slotToChild.getOrDefault(slot);
            Element newChild = this.updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                this.childToSlot.Remove(oldChild);
                this.slotToChild.Remove(slot);
            }

            if (newChild != null) {
                this.slotToChild[slot] = newChild;
                this.childToSlot[newChild] = slot;
            }
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(this.widget == newWidget);
            this._updateChild(this.widget.decoration.icon, _DecorationSlot.icon);
            this._updateChild(this.widget.decoration.input, _DecorationSlot.input);
            this._updateChild(this.widget.decoration.label, _DecorationSlot.label);
            this._updateChild(this.widget.decoration.hint, _DecorationSlot.hint);
            this._updateChild(this.widget.decoration.prefix, _DecorationSlot.prefix);
            this._updateChild(this.widget.decoration.suffix, _DecorationSlot.suffix);
            this._updateChild(this.widget.decoration.prefixIcon, _DecorationSlot.prefixIcon);
            this._updateChild(this.widget.decoration.suffixIcon, _DecorationSlot.suffixIcon);
            this._updateChild(this.widget.decoration.helperError, _DecorationSlot.helperError);
            this._updateChild(this.widget.decoration.counter, _DecorationSlot.counter);
            this._updateChild(this.widget.decoration.container, _DecorationSlot.container);
        }

        void _updateRenderObject(RenderObject child, _DecorationSlot slot) {
            switch (slot) {
                case _DecorationSlot.icon:
                    this.renderObject.icon = (RenderBox) child;
                    break;
                case _DecorationSlot.input:
                    this.renderObject.input = (RenderBox) child;
                    break;
                case _DecorationSlot.label:
                    this.renderObject.label = (RenderBox) child;
                    break;
                case _DecorationSlot.hint:
                    this.renderObject.hint = (RenderBox) child;
                    break;
                case _DecorationSlot.prefix:
                    this.renderObject.prefix = (RenderBox) child;
                    break;
                case _DecorationSlot.suffix:
                    this.renderObject.suffix = (RenderBox) child;
                    break;
                case _DecorationSlot.prefixIcon:
                    this.renderObject.prefixIcon = (RenderBox) child;
                    break;
                case _DecorationSlot.suffixIcon:
                    this.renderObject.suffixIcon = (RenderBox) child;
                    break;
                case _DecorationSlot.helperError:
                    this.renderObject.helperError = (RenderBox) child;
                    break;
                case _DecorationSlot.counter:
                    this.renderObject.counter = (RenderBox) child;
                    break;
                case _DecorationSlot.container:
                    this.renderObject.container = (RenderBox) child;
                    break;
            }
        }

        protected override void insertChildRenderObject(RenderObject child, object slotValue) {
            D.assert(child is RenderBox);
            D.assert(slotValue is _DecorationSlot);
            _DecorationSlot slot = (_DecorationSlot) slotValue;
            this._updateRenderObject(child, slot);
            D.assert(this.renderObject.childToSlot.ContainsKey((RenderBox) child));
            D.assert(this.renderObject.slotToChild.ContainsKey(slot));
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child is RenderBox);
            D.assert(this.renderObject.childToSlot.ContainsKey((RenderBox) child));
            var slot = this.renderObject.childToSlot[(RenderBox) child];
            this._updateRenderObject(null, this.renderObject.childToSlot[(RenderBox) child]);
            D.assert(!this.renderObject.childToSlot.ContainsKey((RenderBox) child));
            D.assert(!this.renderObject.slotToChild.ContainsKey(slot));
        }

        protected override void moveChildRenderObject(RenderObject child, object slotValue) {
            D.assert(false, () => "not reachable");
        }
    }

    class _Decorator : RenderObjectWidget {
        public _Decorator(
            Key key = null,
            _Decoration decoration = null,
            TextBaseline? textBaseline = null,
            bool isFocused = false,
            bool? expands = null
        ) : base(key: key) {
            D.assert(decoration != null);
            D.assert(textBaseline != null);
            D.assert(expands != null);
            this.decoration = decoration;
            this.textBaseline = textBaseline;
            this.isFocused = isFocused;
            this.expands = expands.Value;
        }

        public readonly _Decoration decoration;
        public readonly TextBaseline? textBaseline;
        public readonly bool isFocused;
        public readonly bool expands;

        public override Element createElement() {
            return new _RenderDecorationElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderDecoration(
                decoration: this.decoration,
                textBaseline: this.textBaseline,
                isFocused: this.isFocused,
                expands: this.expands
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderDecoration renderObject = _renderObject as _RenderDecoration;
            renderObject.decoration = this.decoration;
            renderObject.textBaseline = this.textBaseline;
            renderObject.isFocused = this.isFocused;
            renderObject.expands = this.expands;
        }
    }

    class _AffixText : StatelessWidget {
        public _AffixText(
            bool labelIsFloating = false,
            string text = null,
            TextStyle style = null,
            Widget child = null
        ) {
            this.labelIsFloating = labelIsFloating;
            this.text = text;
            this.style = style;
            this.child = child;
        }

        public readonly bool labelIsFloating;
        public readonly string text;
        public readonly TextStyle style;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return DefaultTextStyle.merge(
                style: this.style,
                child: new AnimatedOpacity(
                    duration: InputDecoratorConstants._kTransitionDuration,
                    curve: InputDecoratorConstants._kTransitionCurve,
                    opacity: this.labelIsFloating ? 1.0f : 0.0f,
                    child: this.child ?? new Text(this.text, style: this.style)
                )
            );
        }
    }

    public class InputDecorator : StatefulWidget {
        public InputDecorator(
            Key key = null,
            InputDecoration decoration = null,
            TextStyle baseStyle = null,
            TextAlign? textAlign = null,
            bool isFocused = false,
            bool expands = false,
            bool isEmpty = false,
            Widget child = null
        ) : base(key: key) {
            this.decoration = decoration;
            this.baseStyle = baseStyle;
            this.textAlign = textAlign;
            this.isFocused = isFocused;
            this.expands = expands;
            this.isEmpty = isEmpty;
            this.child = child;
        }

        public readonly InputDecoration decoration;

        public readonly TextStyle baseStyle;

        public readonly TextAlign? textAlign;

        public readonly bool isFocused;
        
        public readonly bool expands;

        public readonly bool isEmpty;

        public readonly Widget child;

        public bool _labelShouldWithdraw {
            get { return !this.isEmpty || this.isFocused; }
        }

        public override State createState() {
            return new _InputDecoratorState();
        }

        internal static RenderBox containerOf(BuildContext context) {
            _RenderDecoration result =
                (_RenderDecoration) context.ancestorRenderObjectOfType(new TypeMatcher<_RenderDecoration>());
            return result?.container;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<InputDecoration>("decoration", this.decoration));
            properties.add(new DiagnosticsProperty<TextStyle>("baseStyle", this.baseStyle, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("isFocused", this.isFocused));
            properties.add(new DiagnosticsProperty<bool>("expands", this.expands));
            properties.add(new DiagnosticsProperty<bool>("isEmpty", this.isEmpty));
        }
    }

    class _InputDecoratorState : TickerProviderStateMixin<InputDecorator> {
        AnimationController _floatingLabelController;
        AnimationController _shakingLabelController;
        _InputBorderGap _borderGap = new _InputBorderGap();

        public override void initState() {
            base.initState();
            this._floatingLabelController = new AnimationController(
                duration: InputDecoratorConstants._kTransitionDuration,
                vsync: this,
                value: (this.widget.decoration.hasFloatingPlaceholder == true && this.widget._labelShouldWithdraw)
                    ? 1.0f
                    : 0.0f
            );
            this._floatingLabelController.addListener(this._handleChange);

            this._shakingLabelController = new AnimationController(
                duration: InputDecoratorConstants._kTransitionDuration,
                vsync: this
            );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this._effectiveDecoration = null;
        }

        public override void dispose() {
            this._floatingLabelController.dispose();
            this._shakingLabelController.dispose();
            base.dispose();
        }

        void _handleChange() {
            this.setState(() => { });
        }

        InputDecoration _effectiveDecoration;

        public InputDecoration decoration {
            get {
                this._effectiveDecoration = this._effectiveDecoration ?? this.widget.decoration.applyDefaults(
                                                Theme.of(this.context).inputDecorationTheme
                                            );
                return this._effectiveDecoration;
            }
        }

        TextAlign? textAlign {
            get { return this.widget.textAlign; }
        }

        bool isFocused {
            get { return this.widget.isFocused; }
        }

        bool isEmpty {
            get { return this.widget.isEmpty; }
        }

        public override void didUpdateWidget(StatefulWidget _old) {
            base.didUpdateWidget(_old);
            InputDecorator old = _old as InputDecorator;
            if (this.widget.decoration != old.decoration) {
                this._effectiveDecoration = null;
            }

            if (this.widget._labelShouldWithdraw != old._labelShouldWithdraw &&
                this.widget.decoration.hasFloatingPlaceholder == true) {
                if (this.widget._labelShouldWithdraw) {
                    this._floatingLabelController.forward();
                }
                else {
                    this._floatingLabelController.reverse();
                }
            }

            string errorText = this.decoration.errorText;
            string oldErrorText = old.decoration.errorText;

            if (this._floatingLabelController.isCompleted && errorText != null && errorText != oldErrorText) {
                this._shakingLabelController.setValue(0.0f);
                this._shakingLabelController.forward();
            }
        }

        Color _getActiveColor(ThemeData themeData) {
            if (this.isFocused) {
                switch (themeData.brightness) {
                    case Brightness.dark:
                        return themeData.accentColor;
                    case Brightness.light:
                        return themeData.primaryColor;
                }
            }

            return themeData.hintColor;
        }

        Color _getFillColor(ThemeData themeData) {
            if (this.decoration.filled != true) {
                return Colors.transparent;
            }

            if (this.decoration.fillColor != null) {
                return this.decoration.fillColor;
            }

            Color darkEnabled = new Color(0x1AFFFFFF);
            Color darkDisabled = new Color(0x0DFFFFFF);
            Color lightEnabled = new Color(0x0A000000);
            Color lightDisabled = new Color(0x05000000);

            switch (themeData.brightness) {
                case Brightness.dark:
                    return this.decoration.enabled == true ? darkEnabled : darkDisabled;
                case Brightness.light:
                    return this.decoration.enabled == true ? lightEnabled : lightDisabled;
            }

            return lightEnabled;
        }

        Color _getDefaultIconColor(ThemeData themeData) {
            if (!this.decoration.enabled == true) {
                return themeData.disabledColor;
            }

            switch (themeData.brightness) {
                case Brightness.dark:
                    return Colors.white70;
                case Brightness.light:
                    return Colors.black45;
                default:
                    return themeData.iconTheme.color;
            }
        }

        bool _hasInlineLabel {
            get { return !this.widget._labelShouldWithdraw && this.decoration.labelText != null; }
        }

        bool _shouldShowLabel {
            get { return this._hasInlineLabel || this.decoration.hasFloatingPlaceholder == true; }
        }


        TextStyle _getInlineStyle(ThemeData themeData) {
            return themeData.textTheme.subhead.merge(this.widget.baseStyle)
                .copyWith(color: this.decoration.enabled == true ? themeData.hintColor : themeData.disabledColor);
        }

        TextStyle _getFloatingLabelStyle(ThemeData themeData) {
            Color color = this.decoration.errorText != null
                ? this.decoration.errorStyle?.color ?? themeData.errorColor
                : this._getActiveColor(themeData);
            TextStyle style = themeData.textTheme.subhead.merge(this.widget.baseStyle);
            return style
                .copyWith(color: this.decoration.enabled == true ? color : themeData.disabledColor)
                .merge(this.decoration.labelStyle);
        }

        TextStyle _getHelperStyle(ThemeData themeData) {
            Color color = this.decoration.enabled == true ? themeData.hintColor : Colors.transparent;
            return themeData.textTheme.caption.copyWith(color: color).merge(this.decoration.helperStyle);
        }

        TextStyle _getErrorStyle(ThemeData themeData) {
            Color color = this.decoration.enabled == true ? themeData.errorColor : Colors.transparent;
            return themeData.textTheme.caption.copyWith(color: color).merge(this.decoration.errorStyle);
        }

        InputBorder _getDefaultBorder(ThemeData themeData) {
            if (this.decoration.border?.borderSide == BorderSide.none) {
                return this.decoration.border;
            }

            Color borderColor;
            if (this.decoration.enabled == true) {
                borderColor = this.decoration.errorText == null
                    ? this._getActiveColor(themeData)
                    : themeData.errorColor;
            }
            else {
                borderColor = (this.decoration.filled == true && this.decoration.border?.isOutline != true)
                    ? Colors.transparent
                    : themeData.disabledColor;
            }

            float borderWeight;
            if (this.decoration.isCollapsed || this.decoration?.border == InputBorder.none ||
                !this.decoration.enabled == true) {
                borderWeight = 0.0f;
            }
            else {
                borderWeight = this.isFocused ? 2.0f : 1.0f;
            }

            InputBorder border = this.decoration.border ?? new UnderlineInputBorder();
            return border.copyWith(borderSide: new BorderSide(color: borderColor, width: borderWeight));
        }

        public override Widget build(BuildContext context) {
            ThemeData themeData = Theme.of(context);
            TextStyle inlineStyle = this._getInlineStyle(themeData);
            TextBaseline? textBaseline = inlineStyle.textBaseline;

            TextStyle hintStyle = inlineStyle.merge(this.decoration.hintStyle);
            Widget hint = this.decoration.hintText == null
                ? null
                : new AnimatedOpacity(
                    opacity: (this.isEmpty && !this._hasInlineLabel) ? 1.0f : 0.0f,
                    duration: InputDecoratorConstants._kTransitionDuration,
                    curve: InputDecoratorConstants._kTransitionCurve,
                    child: new Text(this.decoration.hintText,
                        style: hintStyle,
                        overflow: TextOverflow.ellipsis,
                        textAlign: this.textAlign,
                        maxLines: this.decoration.hintMaxLines
                    )
                );

            bool isError = this.decoration.errorText != null;
            InputBorder border;
            if (!this.decoration.enabled == true) {
                border = isError ? this.decoration.errorBorder : this.decoration.disabledBorder;
            }
            else if (this.isFocused) {
                border = isError ? this.decoration.focusedErrorBorder : this.decoration.focusedBorder;
            }
            else {
                border = isError ? this.decoration.errorBorder : this.decoration.enabledBorder;
            }

            border = border ?? this._getDefaultBorder(themeData);

            Widget container = new _BorderContainer(
                border: border,
                gap: this._borderGap,
                gapAnimation: this._floatingLabelController.view,
                fillColor: this._getFillColor(themeData)
            );

            TextStyle inlineLabelStyle = inlineStyle.merge(this.decoration.labelStyle);
            Widget label = this.decoration.labelText == null
                ? null
                : new _Shaker(
                    animation: this._shakingLabelController.view,
                    child: new AnimatedOpacity(
                        duration: InputDecoratorConstants._kTransitionDuration,
                        curve: InputDecoratorConstants._kTransitionCurve,
                        opacity: this._shouldShowLabel ? 1.0f : 0.0f,
                        child: new AnimatedDefaultTextStyle(
                            duration: InputDecoratorConstants._kTransitionDuration,
                            curve: InputDecoratorConstants._kTransitionCurve,
                            style: this.widget._labelShouldWithdraw
                                ? this._getFloatingLabelStyle(themeData)
                                : inlineLabelStyle,
                            child: new Text(this.decoration.labelText,
                                overflow: TextOverflow.ellipsis,
                                textAlign: this.textAlign
                            )
                        )
                    )
                );

            Widget prefix = this.decoration.prefix == null && this.decoration.prefixText == null
                ? null
                : new _AffixText(
                    labelIsFloating: this.widget._labelShouldWithdraw,
                    text: this.decoration.prefixText,
                    style: this.decoration.prefixStyle ?? hintStyle,
                    child: this.decoration.prefix
                );

            Widget suffix = this.decoration.suffix == null && this.decoration.suffixText == null
                ? null
                : new _AffixText(
                    labelIsFloating: this.widget._labelShouldWithdraw,
                    text: this.decoration.suffixText,
                    style: this.decoration.suffixStyle ?? hintStyle,
                    child: this.decoration.suffix
                );

            Color activeColor = this._getActiveColor(themeData);
            bool decorationIsDense = this.decoration.isDense == true;
            float iconSize = decorationIsDense ? 18.0f : 24.0f;
            Color iconColor = this.isFocused ? activeColor : this._getDefaultIconColor(themeData);

            Widget icon = this.decoration.icon == null
                ? null
                : new Padding(
                    padding: EdgeInsets.only(right: 16.0f),
                    child: IconTheme.merge(
                        data: new IconThemeData(
                            color: iconColor,
                            size: iconSize
                        ),
                        child: this.decoration.icon
                    )
                );

            Widget prefixIcon = this.decoration.prefixIcon == null
                ? null
                : new Center(
                    widthFactor: 1.0f,
                    heightFactor: 1.0f,
                    child: new ConstrainedBox(
                        constraints: new BoxConstraints(minWidth: 48.0f, minHeight: 48.0f),
                        child: IconTheme.merge(
                            data: new IconThemeData(
                                color: iconColor,
                                size: iconSize
                            ),
                            child: this.decoration.prefixIcon
                        )
                    )
                );

            Widget suffixIcon = this.decoration.suffixIcon == null
                ? null
                : new Center(
                    widthFactor: 1.0f,
                    heightFactor: 1.0f,
                    child: new ConstrainedBox(
                        constraints: new BoxConstraints(minWidth: 48.0f, minHeight: 48.0f),
                        child: IconTheme.merge(
                            data: new IconThemeData(
                                color: iconColor,
                                size: iconSize
                            ),
                            child: this.decoration.suffixIcon
                        )
                    )
                );

            Widget helperError = new _HelperError(
                textAlign: this.textAlign,
                helperText: this.decoration.helperText,
                helperStyle: this._getHelperStyle(themeData),
                errorText: this.decoration.errorText,
                errorStyle: this._getErrorStyle(themeData),
                errorMaxLines: this.decoration.errorMaxLines
            );

            Widget counter = null;
            if (this.decoration.counter != null) {
                counter = this.decoration.counter;
            }
            else if (this.decoration.counterText != null && this.decoration.counterText != "") {
                counter = new Text(this.decoration.counterText,
                    style: this._getHelperStyle(themeData).merge(this.decoration.counterStyle),
                    overflow: TextOverflow.ellipsis
                );
            }

            EdgeInsets decorationContentPadding = this.decoration.contentPadding;
            EdgeInsets contentPadding;
            float? floatingLabelHeight;
            if (this.decoration.isCollapsed) {
                floatingLabelHeight = 0.0f;
                contentPadding = decorationContentPadding ?? EdgeInsets.zero;
            }
            else if (!border.isOutline) {
                floatingLabelHeight =
                    (4.0f + 0.75f * inlineLabelStyle.fontSize) * MediaQuery.textScaleFactorOf(context);
                if (this.decoration.filled == true) {
                    contentPadding = decorationContentPadding ?? (decorationIsDense
                                         ? EdgeInsets.fromLTRB(12.0f, 8.0f, 12.0f, 8.0f)
                                         : EdgeInsets.fromLTRB(12.0f, 12.0f, 12.0f, 12.0f));
                }
                else {
                    contentPadding = decorationContentPadding ?? (decorationIsDense
                                         ? EdgeInsets.fromLTRB(0.0f, 8.0f, 0.0f, 8.0f)
                                         : EdgeInsets.fromLTRB(0.0f, 12.0f, 0.0f, 12.0f));
                }
            }
            else {
                floatingLabelHeight = 0.0f;
                contentPadding = decorationContentPadding ?? (decorationIsDense
                                     ? EdgeInsets.fromLTRB(12.0f, 20.0f, 12.0f, 12.0f)
                                     : EdgeInsets.fromLTRB(12.0f, 24.0f, 12.0f, 16.0f));
            }

            return new _Decorator(
                decoration: new _Decoration(
                    contentPadding: contentPadding,
                    isCollapsed: this.decoration.isCollapsed,
                    floatingLabelHeight: floatingLabelHeight ?? 0.0f,
                    floatingLabelProgress: this._floatingLabelController.value,
                    border: border,
                    borderGap: this._borderGap,
                    icon: icon,
                    input: this.widget.child,
                    label: label,
                    alignLabelWithHint: this.decoration.alignLabelWithHint,
                    hint: hint,
                    prefix: prefix,
                    suffix: suffix,
                    prefixIcon: prefixIcon,
                    suffixIcon: suffixIcon,
                    helperError: helperError,
                    counter: counter,
                    container: container
                ),
                textBaseline: textBaseline,
                isFocused: this.isFocused,
                expands: this.widget.expands
            );
        }
    }

    public class InputDecoration {
        public InputDecoration(
            Widget icon = null,
            string labelText = null,
            TextStyle labelStyle = null,
            string helperText = null,
            TextStyle helperStyle = null,
            string hintText = null,
            TextStyle hintStyle = null,
            int? hintMaxLines = null,
            string errorText = null,
            TextStyle errorStyle = null,
            int? errorMaxLines = null,
            bool? hasFloatingPlaceholder = true,
            bool? isDense = null,
            EdgeInsets contentPadding = null,
            Widget prefixIcon = null,
            Widget prefix = null,
            string prefixText = null,
            TextStyle prefixStyle = null,
            Widget suffixIcon = null,
            Widget suffix = null,
            string suffixText = null,
            TextStyle suffixStyle = null,
            Widget counter = null,
            string counterText = null,
            TextStyle counterStyle = null,
            bool? filled = null,
            Color fillColor = null,
            InputBorder errorBorder = null,
            InputBorder focusedBorder = null,
            InputBorder focusedErrorBorder = null,
            InputBorder disabledBorder = null,
            InputBorder enabledBorder = null,
            InputBorder border = null,
            bool? enabled = true,
            bool? alignLabelWithHint = null
        ) {
            D.assert(enabled != null);
            D.assert(!(prefix != null && prefixText != null),
                () => "Declaring both prefix and prefixText is not supported");
            D.assert(!(suffix != null && suffixText != null),
                () => "Declaring both suffix and suffixText is not supported");
            this.isCollapsed = false;
            this.icon = icon;
            this.labelText = labelText;
            this.labelStyle = labelStyle;
            this.helperText = helperText;
            this.helperStyle = helperStyle;
            this.hintText = hintText;
            this.hintStyle = hintStyle;
            this.hintMaxLines = hintMaxLines;
            this.errorText = errorText;
            this.errorStyle = errorStyle;
            this.errorMaxLines = errorMaxLines;
            this.hasFloatingPlaceholder = hasFloatingPlaceholder;
            this.isDense = isDense;
            this.contentPadding = contentPadding;
            this.prefix = prefix;
            this.prefixText = prefixText;
            this.prefixIcon = prefixIcon;
            this.prefixStyle = prefixStyle;
            this.suffix = suffix;
            this.suffixText = suffixText;
            this.suffixIcon = suffixIcon;
            this.suffixStyle = suffixStyle;
            this.counter = counter;
            this.counterText = counterText;
            this.counterStyle = counterStyle;
            this.filled = filled;
            this.fillColor = fillColor;
            this.errorBorder = errorBorder;
            this.focusedBorder = focusedBorder;
            this.focusedErrorBorder = focusedErrorBorder;
            this.disabledBorder = disabledBorder;
            this.enabledBorder = enabledBorder;
            this.border = border;
            this.enabled = enabled;
            this.alignLabelWithHint = alignLabelWithHint;
        }

        public static InputDecoration collapsed(
            string hintText = null,
            bool hasFloatingPlaceholder = true,
            TextStyle hintStyle = null,
            bool filled = false,
            Color fillColor = null,
            InputBorder border = null,
            bool enabled = true
        ) {
            border = border ?? InputBorder.none;
            InputDecoration decoration = new InputDecoration(
                icon: null,
                labelText: null,
                labelStyle: null,
                helperText: null,
                helperStyle: null,
                hintMaxLines: null,
                errorText: null,
                errorStyle: null,
                errorMaxLines: null,
                isDense: false,
                contentPadding: EdgeInsets.zero,
                prefixIcon: null,
                prefix: null,
                prefixText: null,
                prefixStyle: null,
                suffix: null,
                suffixIcon: null,
                suffixText: null,
                suffixStyle: null,
                counter: null,
                counterText: null,
                counterStyle: null,
                errorBorder: null,
                focusedBorder: null,
                focusedErrorBorder: null,
                disabledBorder: null,
                enabledBorder: null,
                hintText: hintText,
                hasFloatingPlaceholder: hasFloatingPlaceholder,
                hintStyle: hintStyle,
                filled: filled,
                fillColor: fillColor,
                border: border,
                enabled: enabled,
                alignLabelWithHint: false
            );
            decoration.isCollapsed = true;
            return decoration;
        }

        public readonly Widget icon;

        public readonly string labelText;

        public readonly TextStyle labelStyle;

        public readonly string helperText;

        public readonly TextStyle helperStyle;

        public readonly string hintText;

        public readonly TextStyle hintStyle;

        public readonly int? hintMaxLines;

        public readonly string errorText;

        public readonly TextStyle errorStyle;

        public readonly int? errorMaxLines;

        public readonly bool? hasFloatingPlaceholder;

        public readonly bool? isDense;

        public readonly EdgeInsets contentPadding;

        public bool isCollapsed;

        public readonly Widget prefixIcon;

        public readonly Widget prefix;

        public readonly string prefixText;

        public readonly TextStyle prefixStyle;

        public readonly Widget suffixIcon;

        public readonly Widget suffix;

        public readonly string suffixText;

        public readonly TextStyle suffixStyle;

        public readonly Widget counter;

        public readonly string counterText;

        public readonly TextStyle counterStyle;

        public readonly bool? filled;

        public readonly Color fillColor;

        public readonly InputBorder errorBorder;

        public readonly InputBorder focusedBorder;

        public readonly InputBorder focusedErrorBorder;

        public readonly InputBorder disabledBorder;

        public readonly InputBorder enabledBorder;

        public readonly InputBorder border;

        public readonly bool? enabled;

        public readonly bool? alignLabelWithHint;

        public InputDecoration copyWith(
            Widget icon = null,
            string labelText = null,
            TextStyle labelStyle = null,
            string helperText = null,
            TextStyle helperStyle = null,
            string hintText = null,
            TextStyle hintStyle = null,
            int? hintMaxLines = null,
            string errorText = null,
            TextStyle errorStyle = null,
            int? errorMaxLines = null,
            bool? hasFloatingPlaceholder = null,
            bool? isDense = null,
            EdgeInsets contentPadding = null,
            Widget prefixIcon = null,
            Widget prefix = null,
            string prefixText = null,
            TextStyle prefixStyle = null,
            Widget suffixIcon = null,
            Widget suffix = null,
            string suffixText = null,
            TextStyle suffixStyle = null,
            Widget counter = null,
            string counterText = null,
            TextStyle counterStyle = null,
            bool? filled = null,
            Color fillColor = null,
            InputBorder errorBorder = null,
            InputBorder focusedBorder = null,
            InputBorder focusedErrorBorder = null,
            InputBorder disabledBorder = null,
            InputBorder enabledBorder = null,
            InputBorder border = null,
            bool? enabled = null,
            bool? alignLabelWithHint = null
        ) {
            return new InputDecoration(
                icon: icon ?? this.icon,
                labelText: labelText ?? this.labelText,
                labelStyle: labelStyle ?? this.labelStyle,
                helperText: helperText ?? this.helperText,
                helperStyle: helperStyle ?? this.helperStyle,
                hintText: hintText ?? this.hintText,
                hintStyle: hintStyle ?? this.hintStyle,
                hintMaxLines: hintMaxLines ?? this.hintMaxLines,
                errorText: errorText ?? this.errorText,
                errorStyle: errorStyle ?? this.errorStyle,
                errorMaxLines: errorMaxLines ?? this.errorMaxLines,
                hasFloatingPlaceholder: hasFloatingPlaceholder ?? this.hasFloatingPlaceholder,
                isDense: isDense ?? this.isDense,
                contentPadding: contentPadding ?? this.contentPadding,
                prefixIcon: prefixIcon ?? this.prefixIcon,
                prefix: prefix ?? this.prefix,
                prefixText: prefixText ?? this.prefixText,
                prefixStyle: prefixStyle ?? this.prefixStyle,
                suffixIcon: suffixIcon ?? this.suffixIcon,
                suffix: suffix ?? this.suffix,
                suffixText: suffixText ?? this.suffixText,
                suffixStyle: suffixStyle ?? this.suffixStyle,
                counter: counter ?? this.counter,
                counterText: counterText ?? this.counterText,
                counterStyle: counterStyle ?? this.counterStyle,
                filled: filled ?? this.filled,
                fillColor: fillColor ?? this.fillColor,
                errorBorder: errorBorder ?? this.errorBorder,
                focusedBorder: focusedBorder ?? this.focusedBorder,
                focusedErrorBorder: focusedErrorBorder ?? this.focusedErrorBorder,
                disabledBorder: disabledBorder ?? this.disabledBorder,
                enabledBorder: enabledBorder ?? this.enabledBorder,
                border: border ?? this.border,
                enabled: enabled ?? this.enabled,
                alignLabelWithHint: alignLabelWithHint ?? this.alignLabelWithHint
            );
        }

        public InputDecoration applyDefaults(InputDecorationTheme theme) {
            return this.copyWith(
                labelStyle: this.labelStyle ?? theme.labelStyle,
                helperStyle: this.helperStyle ?? theme.helperStyle,
                hintStyle: this.hintStyle ?? theme.hintStyle,
                errorStyle: this.errorStyle ?? theme.errorStyle,
                errorMaxLines: this.errorMaxLines ?? theme.errorMaxLines,
                hasFloatingPlaceholder: this.hasFloatingPlaceholder ?? theme.hasFloatingPlaceholder,
                isDense: this.isDense ?? theme.isDense,
                contentPadding: this.contentPadding ?? theme.contentPadding,
                prefixStyle: this.prefixStyle ?? theme.prefixStyle,
                suffixStyle: this.suffixStyle ?? theme.suffixStyle,
                counterStyle: this.counterStyle ?? theme.counterStyle,
                filled: this.filled ?? theme.filled,
                fillColor: this.fillColor ?? theme.fillColor,
                errorBorder: this.errorBorder ?? theme.errorBorder,
                focusedBorder: this.focusedBorder ?? theme.focusedBorder,
                focusedErrorBorder: this.focusedErrorBorder ?? theme.focusedErrorBorder,
                disabledBorder: this.disabledBorder ?? theme.disabledBorder,
                enabledBorder: this.enabledBorder ?? theme.enabledBorder,
                border: this.border ?? theme.border,
                alignLabelWithHint: this.alignLabelWithHint ?? theme.alignLabelWithHint
            );
        }

        public static bool operator ==(InputDecoration left, InputDecoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(InputDecoration left, InputDecoration right) {
            return !Equals(left, right);
        }

        public bool Equals(InputDecoration other) {
            return Equals(other.icon, this.icon)
                   && Equals(other.labelText, this.labelText)
                   && Equals(other.labelStyle, this.labelStyle)
                   && Equals(other.helperText, this.helperText)
                   && Equals(other.helperStyle, this.helperStyle)
                   && Equals(other.hintText, this.hintText)
                   && Equals(other.hintStyle, this.hintStyle)
                   && Equals(other.hintMaxLines, this.hintMaxLines)
                   && Equals(other.errorText, this.errorText)
                   && Equals(other.errorStyle, this.errorStyle)
                   && Equals(other.errorMaxLines, this.errorMaxLines)
                   && Equals(other.hasFloatingPlaceholder, this.hasFloatingPlaceholder)
                   && Equals(other.isDense, this.isDense)
                   && Equals(other.contentPadding, this.contentPadding)
                   && Equals(other.isCollapsed, this.isCollapsed)
                   && Equals(other.prefixIcon, this.prefixIcon)
                   && Equals(other.prefix, this.prefix)
                   && Equals(other.prefixText, this.prefixText)
                   && Equals(other.prefixStyle, this.prefixStyle)
                   && Equals(other.suffixIcon, this.suffixIcon)
                   && Equals(other.suffix, this.suffix)
                   && Equals(other.suffixText, this.suffixText)
                   && Equals(other.suffixStyle, this.suffixStyle)
                   && Equals(other.counter, this.counter)
                   && Equals(other.counterText, this.counterText)
                   && Equals(other.counterStyle, this.counterStyle)
                   && Equals(other.filled, this.filled)
                   && Equals(other.fillColor, this.fillColor)
                   && Equals(other.errorBorder, this.errorBorder)
                   && Equals(other.focusedBorder, this.focusedBorder)
                   && Equals(other.focusedErrorBorder, this.focusedErrorBorder)
                   && Equals(other.disabledBorder, this.disabledBorder)
                   && Equals(other.enabledBorder, this.enabledBorder)
                   && Equals(other.border, this.border)
                   && Equals(other.enabled, this.enabled)
                   && Equals(other.alignLabelWithHint, this.alignLabelWithHint);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((InputDecoration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.icon.GetHashCode();
                hashCode = (hashCode * 397) ^ this.labelText.GetHashCode();
                hashCode = (hashCode * 397) ^ this.labelStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.helperText.GetHashCode();
                hashCode = (hashCode * 397) ^ this.helperStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.hintText.GetHashCode();
                hashCode = (hashCode * 397) ^ this.hintStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.hintMaxLines.GetHashCode();
                hashCode = (hashCode * 397) ^ this.errorText.GetHashCode();
                hashCode = (hashCode * 397) ^ this.errorStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.errorMaxLines.GetHashCode();
                hashCode = (hashCode * 397) ^ this.hasFloatingPlaceholder.GetHashCode();
                hashCode = (hashCode * 397) ^ this.isDense.GetHashCode();
                hashCode = (hashCode * 397) ^ this.contentPadding.GetHashCode();
                hashCode = (hashCode * 397) ^ this.isCollapsed.GetHashCode();
                hashCode = (hashCode * 397) ^ this.filled.GetHashCode();
                hashCode = (hashCode * 397) ^ this.fillColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.border.GetHashCode();
                hashCode = (hashCode * 397) ^ this.enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ this.prefixIcon.GetHashCode();
                hashCode = (hashCode * 397) ^ this.prefix.GetHashCode();
                hashCode = (hashCode * 397) ^ this.prefixText.GetHashCode();
                hashCode = (hashCode * 397) ^ this.prefixStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.suffixIcon.GetHashCode();
                hashCode = (hashCode * 397) ^ this.suffix.GetHashCode();
                hashCode = (hashCode * 397) ^ this.suffixText.GetHashCode();
                hashCode = (hashCode * 397) ^ this.suffixStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.counter.GetHashCode();
                hashCode = (hashCode * 397) ^ this.counterText.GetHashCode();
                hashCode = (hashCode * 397) ^ this.counterStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.filled.GetHashCode();
                hashCode = (hashCode * 397) ^ this.fillColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.errorBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ this.focusedBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ this.focusedErrorBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ this.disabledBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ this.enabledBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ this.border.GetHashCode();
                hashCode = (hashCode * 397) ^ this.enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ this.alignLabelWithHint.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            List<string> description = new List<string> { };
            if (this.icon != null) {
                description.Add($"icon: ${this.icon}");
            }

            if (this.labelText != null) {
                description.Add($"labelText: ${this.labelText}");
            }

            if (this.helperText != null) {
                description.Add($"helperText: ${this.helperText}");
            }

            if (this.hintMaxLines != null) {
                description.Add($"hintMaxLines: ${this.hintMaxLines}");
            }

            if (this.hintText != null) {
                description.Add($"hintText: ${this.hintText}");
            }

            if (this.errorText != null) {
                description.Add($"errorText: ${this.errorText}");
            }

            if (this.errorStyle != null) {
                description.Add($"errorStyle: ${this.errorStyle}");
            }

            if (this.errorMaxLines != null) {
                description.Add($"errorMaxLines: ${this.errorMaxLines}");
            }

            if (this.hasFloatingPlaceholder == false) {
                description.Add($"hasFloatingPlaceholder: false");
            }

            if (this.isDense ?? false) {
                description.Add($"isDense: ${this.isDense}");
            }

            if (this.contentPadding != null) {
                description.Add($"contentPadding: ${this.contentPadding}");
            }

            if (this.isCollapsed) {
                description.Add($"isCollapsed: ${this.isCollapsed}");
            }

            if (this.prefixIcon != null) {
                description.Add($"prefixIcon: ${this.prefixIcon}");
            }

            if (this.prefix != null) {
                description.Add($"prefix: ${this.prefix}");
            }

            if (this.prefixText != null) {
                description.Add($"prefixText: ${this.prefixText}");
            }

            if (this.prefixStyle != null) {
                description.Add($"prefixStyle: ${this.prefixStyle}");
            }

            if (this.suffixIcon != null) {
                description.Add($"suffixIcon: ${this.suffixIcon}");
            }

            if (this.suffix != null) {
                description.Add($"suffix: ${this.suffix}");
            }

            if (this.suffixText != null) {
                description.Add($"suffixText: ${this.suffixText}");
            }

            if (this.suffixStyle != null) {
                description.Add($"suffixStyle: ${this.suffixStyle}");
            }

            if (this.counter != null) {
                description.Add($"counter: ${this.counter}");
            }

            if (this.counterText != null) {
                description.Add($"counterText: ${this.counterText}");
            }

            if (this.counterStyle != null) {
                description.Add($"counterStyle: ${this.counterStyle}");
            }

            if (this.filled == true) {
                description.Add($"filled: true");
            }

            if (this.fillColor != null) {
                description.Add($"fillColor: ${this.fillColor}");
            }

            if (this.errorBorder != null) {
                description.Add($"errorBorder: ${this.errorBorder}");
            }

            if (this.focusedBorder != null) {
                description.Add($"focusedBorder: ${this.focusedBorder}");
            }

            if (this.focusedErrorBorder != null) {
                description.Add($"focusedErrorBorder: ${this.focusedErrorBorder}");
            }

            if (this.disabledBorder != null) {
                description.Add($"disabledBorder: ${this.disabledBorder}");
            }

            if (this.enabledBorder != null) {
                description.Add($"enabledBorder: ${this.enabledBorder}");
            }

            if (this.border != null) {
                description.Add($"border: ${this.border}");
            }

            if (this.enabled != true) {
                description.Add("enabled: false");
            }

            if (this.alignLabelWithHint != null) {
                description.Add($"alignLabelWithHint: {this.alignLabelWithHint}");
            }

            return $"InputDecoration(${string.Join(", ", description)})";
        }
    }

    public class InputDecorationTheme : Diagnosticable {
        public InputDecorationTheme(
            TextStyle labelStyle = null,
            TextStyle helperStyle = null,
            TextStyle hintStyle = null,
            TextStyle errorStyle = null,
            int? errorMaxLines = null,
            bool? hasFloatingPlaceholder = true,
            bool? isDense = false,
            EdgeInsets contentPadding = null,
            bool? isCollapsed = false,
            TextStyle prefixStyle = null,
            TextStyle suffixStyle = null,
            TextStyle counterStyle = null,
            bool? filled = false,
            Color fillColor = null,
            InputBorder errorBorder = null,
            InputBorder focusedBorder = null,
            InputBorder focusedErrorBorder = null,
            InputBorder disabledBorder = null,
            InputBorder enabledBorder = null,
            InputBorder border = null,
            bool alignLabelWithHint = false
        ) {
            D.assert(isDense != null);
            D.assert(isCollapsed != null);
            D.assert(filled != null);
            this.labelStyle = labelStyle;
            this.helperStyle = helperStyle;
            this.hintStyle = hintStyle;
            this.errorStyle = errorStyle;
            this.errorMaxLines = errorMaxLines;
            this.hasFloatingPlaceholder = hasFloatingPlaceholder;
            this.isDense = isDense;
            this.contentPadding = contentPadding;
            this.isCollapsed = isCollapsed;
            this.prefixStyle = prefixStyle;
            this.suffixStyle = suffixStyle;
            this.counterStyle = counterStyle;
            this.filled = filled;
            this.fillColor = fillColor;
            this.errorBorder = errorBorder;
            this.focusedBorder = focusedBorder;
            this.focusedErrorBorder = focusedErrorBorder;
            this.disabledBorder = disabledBorder;
            this.enabledBorder = enabledBorder;
            this.border = border;
            this.alignLabelWithHint = alignLabelWithHint;
        }

        public readonly TextStyle labelStyle;

        public readonly TextStyle helperStyle;

        public readonly TextStyle hintStyle;

        public readonly TextStyle errorStyle;

        public readonly int? errorMaxLines;

        public readonly bool? hasFloatingPlaceholder;

        public readonly bool? isDense;

        public readonly EdgeInsets contentPadding;

        public readonly bool? isCollapsed;

        public readonly TextStyle prefixStyle;

        public readonly TextStyle suffixStyle;

        public readonly TextStyle counterStyle;

        public readonly bool? filled;

        public readonly Color fillColor;

        public readonly InputBorder errorBorder;

        public readonly InputBorder focusedBorder;

        public readonly InputBorder focusedErrorBorder;

        public readonly InputBorder disabledBorder;

        public readonly InputBorder enabledBorder;

        public readonly InputBorder border;

        public readonly bool alignLabelWithHint;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            InputDecorationTheme defaultTheme = new InputDecorationTheme();
            properties.add(new DiagnosticsProperty<TextStyle>("labelStyle", this.labelStyle,
                defaultValue: defaultTheme.labelStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("helperStyle", this.helperStyle,
                defaultValue: defaultTheme.helperStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("hintStyle", this.hintStyle,
                defaultValue: defaultTheme.hintStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("errorStyle", this.errorStyle,
                defaultValue: defaultTheme.errorStyle));
            properties.add(new DiagnosticsProperty<int?>("errorMaxLines", this.errorMaxLines,
                defaultValue: defaultTheme.errorMaxLines));
            properties.add(new DiagnosticsProperty<bool?>("hasFloatingPlaceholder", this.hasFloatingPlaceholder,
                defaultValue: defaultTheme.hasFloatingPlaceholder));
            properties.add(new DiagnosticsProperty<bool?>("isDense", this.isDense, defaultValue: defaultTheme.isDense));
            properties.add(new DiagnosticsProperty<EdgeInsets>("contentPadding", this.contentPadding,
                defaultValue: defaultTheme.contentPadding));
            properties.add(new DiagnosticsProperty<bool?>("isCollapsed", this.isCollapsed,
                defaultValue: defaultTheme.isCollapsed));
            properties.add(new DiagnosticsProperty<TextStyle>("prefixStyle", this.prefixStyle,
                defaultValue: defaultTheme.prefixStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("suffixStyle", this.suffixStyle,
                defaultValue: defaultTheme.suffixStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("counterStyle", this.counterStyle,
                defaultValue: defaultTheme.counterStyle));
            properties.add(new DiagnosticsProperty<bool?>("filled", this.filled, defaultValue: defaultTheme.filled));
            properties.add(new DiagnosticsProperty<Color>("fillColor", this.fillColor,
                defaultValue: defaultTheme.fillColor));
            properties.add(new DiagnosticsProperty<InputBorder>("errorBorder", this.errorBorder,
                defaultValue: defaultTheme.errorBorder));
            properties.add(new DiagnosticsProperty<InputBorder>("focusedBorder", this.focusedBorder,
                defaultValue: defaultTheme.focusedErrorBorder));
            properties.add(new DiagnosticsProperty<InputBorder>("focusedErrorBorder", this.focusedErrorBorder,
                defaultValue: defaultTheme.focusedErrorBorder));
            properties.add(new DiagnosticsProperty<InputBorder>("disabledBorder", this.disabledBorder,
                defaultValue: defaultTheme.disabledBorder));
            properties.add(new DiagnosticsProperty<InputBorder>("enabledBorder", this.enabledBorder,
                defaultValue: defaultTheme.enabledBorder));
            properties.add(
                new DiagnosticsProperty<InputBorder>("border", this.border, defaultValue: defaultTheme.border));
            properties.add(new DiagnosticsProperty<bool>("alignLabelWithHint", this.alignLabelWithHint,
                defaultValue: defaultTheme.alignLabelWithHint));
        }
    }
}