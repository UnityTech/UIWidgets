using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public enum ListTileStyle {
        list,
        drawer
    }

    public class ListTileTheme : InheritedWidget {
        public ListTileTheme(
            Key key = null,
            bool dense = false,
            ListTileStyle style = ListTileStyle.list,
            Color selectedColor = null,
            Color iconColor = null,
            Color textColor = null,
            EdgeInsets contentPadding = null,
            Widget child = null) : base(key: key, child: child) {
            this.dense = dense;
            this.style = style;
            this.selectedColor = selectedColor;
            this.iconColor = iconColor;
            this.textColor = textColor;
            this.contentPadding = contentPadding;
        }

        public static Widget merge(
            Key key = null,
            bool? dense = null,
            ListTileStyle? style = null,
            Color selectedColor = null,
            Color iconColor = null,
            Color textColor = null,
            EdgeInsets contentPadding = null,
            Widget child = null) {
            D.assert(child != null);
            return new Builder(
                builder: (BuildContext context) => {
                    ListTileTheme parent = of(context);
                    return new ListTileTheme(
                        key: key,
                        dense: dense ?? parent.dense,
                        style: style ?? parent.style,
                        selectedColor: selectedColor ?? parent.selectedColor,
                        iconColor: iconColor ?? parent.iconColor,
                        textColor: textColor ?? parent.textColor,
                        contentPadding: contentPadding ?? parent.contentPadding,
                        child: child);
                }
            );
        }

        public readonly bool dense;

        public readonly ListTileStyle style;

        public readonly Color selectedColor;

        public readonly Color iconColor;

        public readonly Color textColor;

        public readonly EdgeInsets contentPadding;

        public static ListTileTheme of(BuildContext context) {
            ListTileTheme result = (ListTileTheme) context.inheritFromWidgetOfExactType(typeof(ListTileTheme));
            return result ?? new ListTileTheme();
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            ListTileTheme _oldWidget = (ListTileTheme) oldWidget;
            return this.dense != _oldWidget.dense ||
                   this.style != _oldWidget.style ||
                   this.selectedColor != _oldWidget.selectedColor ||
                   this.iconColor != _oldWidget.iconColor ||
                   this.textColor != _oldWidget.textColor ||
                   this.contentPadding != _oldWidget.contentPadding;
        }
    }

    public enum ListTileControlAffinity {
        leading,
        trailing,
        platform
    }

    public class ListTile : StatelessWidget {
        public ListTile(
            Key key = null,
            Widget leading = null,
            Widget title = null,
            Widget subtitle = null,
            Widget trailing = null,
            bool isThreeLine = false,
            bool? dense = null,
            EdgeInsets contentPadding = null,
            bool enabled = true,
            GestureTapCallback onTap = null,
            GestureLongPressCallback onLongPress = null,
            bool selected = false
        ) : base(key: key) {
            D.assert(!isThreeLine || subtitle != null);
            this.leading = leading;
            this.title = title;
            this.subtitle = subtitle;
            this.trailing = trailing;
            this.isThreeLine = isThreeLine;
            this.dense = dense;
            this.contentPadding = contentPadding;
            this.enabled = enabled;
            this.onTap = onTap;
            this.onLongPress = onLongPress;
            this.selected = selected;
        }

        public readonly Widget leading;

        public readonly Widget title;

        public readonly Widget subtitle;

        public readonly Widget trailing;

        public readonly bool isThreeLine;

        public readonly bool? dense;

        public readonly EdgeInsets contentPadding;

        public readonly bool enabled;

        public readonly GestureTapCallback onTap;

        public readonly GestureLongPressCallback onLongPress;

        public readonly bool selected;


        Color _iconColor(ThemeData theme, ListTileTheme tileTheme) {
            if (!this.enabled) {
                return theme.disabledColor;
            }

            if (this.selected && tileTheme?.selectedColor != null) {
                return tileTheme.selectedColor;
            }

            if (!this.selected && tileTheme?.iconColor != null) {
                return tileTheme.iconColor;
            }

            switch (theme.brightness) {
                case Brightness.light:
                    return this.selected ? theme.primaryColor : Colors.black45;
                case Brightness.dark:
                    return this.selected ? theme.accentColor : null;
            }

            return null;
        }

        Color _textColor(ThemeData theme, ListTileTheme tileTheme, Color defaultColor) {
            if (!this.enabled) {
                return theme.disabledColor;
            }

            if (this.selected && tileTheme?.selectedColor != null) {
                return tileTheme.selectedColor;
            }

            if (!this.selected && tileTheme?.textColor != null) {
                return tileTheme.textColor;
            }

            if (this.selected) {
                switch (theme.brightness) {
                    case Brightness.light:
                        return theme.primaryColor;
                    case Brightness.dark:
                        return theme.accentColor;
                }
            }

            return defaultColor;
        }

        bool _isDenseLayout(ListTileTheme tileTheme) {
            return this.dense != null ? this.dense ?? false : (tileTheme?.dense ?? false);
        }

        TextStyle _titleTextStyle(ThemeData theme, ListTileTheme tileTheme) {
            TextStyle style = null;
            if (tileTheme != null) {
                switch (tileTheme.style) {
                    case ListTileStyle.drawer:
                        style = theme.textTheme.body2;
                        break;
                    case ListTileStyle.list:
                        style = theme.textTheme.subhead;
                        break;
                }
            }
            else {
                style = theme.textTheme.subhead;
            }

            Color color = this._textColor(theme, tileTheme, style.color);
            return this._isDenseLayout(tileTheme)
                ? style.copyWith(fontSize: 13.0, color: color)
                : style.copyWith(color: color);
        }

        TextStyle _subtitleTextStyle(ThemeData theme, ListTileTheme tileTheme) {
            TextStyle style = theme.textTheme.body1;
            Color color = this._textColor(theme, tileTheme, theme.textTheme.caption.color);
            return this._isDenseLayout(tileTheme)
                ? style.copyWith(color: color, fontSize: 12.0)
                : style.copyWith(color: color);
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialDebug.debugCheckHasMaterial(context));
            ThemeData theme = Theme.of(context);
            ListTileTheme tileTheme = ListTileTheme.of(context);

            IconThemeData iconThemeData = null;
            if (this.leading != null || this.trailing != null) {
                iconThemeData = new IconThemeData(color: this._iconColor(theme, tileTheme));
            }

            Widget leadingIcon = null;
            if (this.leading != null) {
                leadingIcon = IconTheme.merge(
                    data: iconThemeData,
                    child: this.leading);
            }

            TextStyle titleStyle = this._titleTextStyle(theme, tileTheme);
            Widget titleText = new AnimatedDefaultTextStyle(
                style: titleStyle,
                duration: Constants.kThemeChangeDuration,
                child: this.title ?? new SizedBox()
            );

            Widget subtitleText = null;
            TextStyle subtitleStyle = null;
            if (this.subtitle != null) {
                subtitleStyle = this._subtitleTextStyle(theme, tileTheme);
                subtitleText = new AnimatedDefaultTextStyle(
                    style: subtitleStyle,
                    duration: Constants.kThemeChangeDuration,
                    child: this.subtitle);
            }

            Widget trailingIcon = null;
            if (this.trailing != null) {
                trailingIcon = IconTheme.merge(
                    data: iconThemeData,
                    child: this.trailing);
            }

            EdgeInsets _defaultContentPadding = EdgeInsets.symmetric(horizontal: 16.0);
            EdgeInsets resolvedContentPadding =
                this.contentPadding ?? tileTheme?.contentPadding ?? _defaultContentPadding;

            return new InkWell(
                onTap: this.enabled ? this.onTap : null,
                onLongPress: this.enabled ? this.onLongPress : null,
                child: new SafeArea(
                    top: false,
                    bottom: false,
                    mininum: resolvedContentPadding,
                    child: new _ListTile(
                        leading: leadingIcon,
                        title: titleText,
                        subtitle: subtitleText,
                        trailing: trailingIcon,
                        isDense: this._isDenseLayout(tileTheme),
                        isThreeLine: this.isThreeLine,
                        titleBaselineType: titleStyle.textBaseline,
                        subtitleBaselineType: subtitleStyle?.textBaseline
                    )
                )
            );
        }
    }

    public enum _ListTileSlot {
        leading,
        title,
        subtitle,
        trailing
    }

    public class _ListTile : RenderObjectWidget {
        public _ListTile(
            Key key = null,
            Widget leading = null,
            Widget title = null,
            Widget subtitle = null,
            Widget trailing = null,
            bool? isThreeLine = null,
            bool? isDense = null,
            TextBaseline? titleBaselineType = null,
            TextBaseline? subtitleBaselineType = null) : base(key: key) {
            D.assert(isThreeLine != null);
            D.assert(isDense != null);
            D.assert(titleBaselineType != null);
            this.leading = leading;
            this.title = title;
            this.subtitle = subtitle;
            this.trailing = trailing;
            this.isThreeLine = isThreeLine ?? false;
            this.isDense = isDense ?? false;
            this.titleBaselineType = titleBaselineType ?? TextBaseline.alphabetic;
            this.subtitleBaselineType = subtitleBaselineType;
        }

        public readonly Widget leading;

        public readonly Widget title;

        public readonly Widget subtitle;

        public readonly Widget trailing;

        public readonly bool isThreeLine;

        public readonly bool isDense;

        public readonly TextBaseline titleBaselineType;

        public readonly TextBaseline? subtitleBaselineType;

        public override Element createElement() {
            return new _ListTileElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderListTile(
                isThreeLine: this.isThreeLine,
                isDense: this.isDense,
                titleBaselineType: this.titleBaselineType,
                subtitleBaselineType: this.subtitleBaselineType
            );
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderListTile _renderObject = (_RenderListTile) renderObject;
            _renderObject.isThreeLine = this.isThreeLine;
            _renderObject.isDense = this.isDense;
            _renderObject.titleBaselineType = this.titleBaselineType;
            _renderObject.subtitleBaselineType = this.subtitleBaselineType;
        }
    }


    public class _ListTileElement : RenderObjectElement {
        public _ListTileElement(RenderObjectWidget widget) : base(widget) {
        }

        readonly Dictionary<_ListTileSlot, Element> slotToChild = new Dictionary<_ListTileSlot, Element>();
        readonly Dictionary<Element, _ListTileSlot> childToSlot = new Dictionary<Element, _ListTileSlot>();

        public new _ListTile widget {
            get { return (_ListTile) base.widget; }
        }

        public new _RenderListTile renderObject {
            get { return (_RenderListTile) base.renderObject; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            foreach (var element in this.slotToChild.Values) {
                visitor(element);
            }
        }

        protected override void forgetChild(Element child) {
            D.assert(this.slotToChild.Values.Contains(child));
            D.assert(this.childToSlot.Keys.Contains(child));
            _ListTileSlot slot = this.childToSlot[child];
            this.childToSlot.Remove(child);
            this.slotToChild.Remove(slot);
        }

        void _mountChild(Widget widget, _ListTileSlot slot) {
            Element oldChild = this.slotToChild[slot];
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
            this._mountChild(this.widget.leading, _ListTileSlot.leading);
            this._mountChild(this.widget.title, _ListTileSlot.title);
            this._mountChild(this.widget.subtitle, _ListTileSlot.subtitle);
            this._mountChild(this.widget.trailing, _ListTileSlot.trailing);
        }

        void _updateChild(Widget widget, _ListTileSlot slot) {
            Element oldChild = this.slotToChild[slot];
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
            this._updateChild(this.widget.leading, _ListTileSlot.leading);
            this._updateChild(this.widget.title, _ListTileSlot.title);
            this._updateChild(this.widget.subtitle, _ListTileSlot.subtitle);
            this._updateChild(this.widget.trailing, _ListTileSlot.trailing);
        }

        void _updateRenderObject(RenderObject child, _ListTileSlot slot) {
            switch (slot) {
                case _ListTileSlot.leading:
                    this.renderObject.leading = (RenderBox) child;
                    break;
                case _ListTileSlot.title:
                    this.renderObject.title = (RenderBox) child;
                    break;
                case _ListTileSlot.subtitle:
                    this.renderObject.subtitle = (RenderBox) child;
                    break;
                case _ListTileSlot.trailing:
                    this.renderObject.trailing = (RenderBox) child;
                    break;
            }
        }

        protected override void insertChildRenderObject(RenderObject child, object slotValue) {
            D.assert(child is RenderBox);
            D.assert(slotValue is _ListTileSlot);
            _ListTileSlot slot = (_ListTileSlot) slotValue;
            this._updateRenderObject(child, slot);
            D.assert(this.renderObject.childToSlot.Keys.Contains(child));
            D.assert(this.renderObject.slotToChild.Keys.Contains(slot));
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child is RenderBox);
            D.assert(this.renderObject.childToSlot.Keys.Contains(child));
            _ListTileSlot slot = this.renderObject.childToSlot[(RenderBox) child];
            this._updateRenderObject(null, slot);
            D.assert(!this.renderObject.childToSlot.Keys.Contains(child));
            D.assert(!this.renderObject.slotToChild.Keys.Contains(slot));
        }

        protected override void moveChildRenderObject(RenderObject child, object slotValue) {
            D.assert(false, "not reachable");
        }
    }


    public class _RenderListTile : RenderBox {
        public _RenderListTile(
            bool? isDense = null,
            bool? isThreeLine = null,
            TextBaseline? titleBaselineType = null,
            TextBaseline? subtitleBaselineType = null) {
            D.assert(isDense != null);
            D.assert(isThreeLine != null);
            D.assert(titleBaselineType != null);
            this._isDense = isDense ?? false;
            this._isThreeLine = isThreeLine ?? false;
            this._titleBaselineType = titleBaselineType ?? TextBaseline.alphabetic;
            this._subtitleBaselineType = subtitleBaselineType;
        }

        const double _minLeadingWidth = 40.0;

        const double _horizontalTitleGap = 16.0;

        const double _minVerticalPadding = 4.0;

        public readonly Dictionary<_ListTileSlot, RenderBox> slotToChild = new Dictionary<_ListTileSlot, RenderBox>();
        public readonly Dictionary<RenderBox, _ListTileSlot> childToSlot = new Dictionary<RenderBox, _ListTileSlot>();

        RenderBox _updateChild(RenderBox oldChild, RenderBox newChild, _ListTileSlot slot) {
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

        RenderBox _leading;

        public RenderBox leading {
            get { return this._leading; }
            set { this._leading = this._updateChild(this._leading, value, _ListTileSlot.leading); }
        }

        RenderBox _title;

        public RenderBox title {
            get { return this._title; }
            set { this._title = this._updateChild(this._title, value, _ListTileSlot.title); }
        }

        RenderBox _subtitle;

        public RenderBox subtitle {
            get { return this._subtitle; }
            set { this._subtitle = this._updateChild(this._subtitle, value, _ListTileSlot.subtitle); }
        }

        RenderBox _trailing;

        public RenderBox trailing {
            get { return this._trailing; }
            set { this._trailing = this._updateChild(this._trailing, value, _ListTileSlot.trailing); }
        }

        List<RenderObject> _children {
            get {
                List<RenderObject> ret = new List<RenderObject>();
                if (this.leading != null) {
                    ret.Add(this.leading);
                }

                if (this.title != null) {
                    ret.Add(this.title);
                }

                if (this.subtitle != null) {
                    ret.Add(this.subtitle);
                }

                if (this.trailing != null) {
                    ret.Add(this.trailing);
                }

                return ret;
            }
        }

        public bool isDense {
            get { return this._isDense; }
            set {
                if (this._isDense == value) {
                    return;
                }

                this._isDense = value;
                this.markNeedsLayout();
            }
        }

        bool _isDense;

        public bool isThreeLine {
            get { return this._isThreeLine; }
            set {
                if (this._isThreeLine == value) {
                    return;
                }

                this._isThreeLine = value;
                this.markNeedsLayout();
            }
        }

        bool _isThreeLine;

        public TextBaseline titleBaselineType {
            get { return this._titleBaselineType; }
            set {
                if (this._titleBaselineType == value) {
                    return;
                }

                this._titleBaselineType = value;
                this.markNeedsLayout();
            }
        }

        TextBaseline _titleBaselineType;

        public TextBaseline? subtitleBaselineType {
            get { return this._subtitleBaselineType; }
            set {
                if (this._subtitleBaselineType == value) {
                    return;
                }

                this._subtitleBaselineType = value;
                this.markNeedsLayout();
            }
        }

        TextBaseline? _subtitleBaselineType;

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
            foreach (var child in this._children) {
                this.redepthChild(child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            foreach (var child in this._children) {
                visitor(child);
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            List<DiagnosticsNode> value = new List<DiagnosticsNode>();

            void add(RenderBox child, string name) {
                if (child != null) {
                    value.Add(child.toDiagnosticsNode(name: name));
                }
            }

            add(this.leading, "leading");
            add(this.title, "title");
            add(this.subtitle, "subtitle");
            add(this.trailing, "trailing");
            return value;
        }

        public new bool sizedByParent {
            get { return false; }
        }

        static double _minWidth(RenderBox box, double height) {
            return box == null ? 0.0 : box.getMinIntrinsicWidth(height);
        }

        static double _maxWidth(RenderBox box, double height) {
            return box == null ? 0.0 : box.getMaxIntrinsicWidth(height);
        }

        protected override double computeMinIntrinsicWidth(double height) {
            double leadingWidth = this.leading != null
                ? Math.Max(this.leading.getMinIntrinsicWidth(height), _minLeadingWidth) + _horizontalTitleGap
                : 0.0;
            return leadingWidth + Math.Max(_minWidth(this.title, height), _minWidth(this.subtitle, height)) +
                   _maxWidth(this.trailing, height);
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            double leadingWidth = this.leading != null
                ? Math.Max(this.leading.getMaxIntrinsicWidth(height), _minLeadingWidth) + _horizontalTitleGap
                : 0.0;
            return leadingWidth + Math.Max(_maxWidth(this.title, height), _maxWidth(this.subtitle, height)) +
                   _maxWidth(this.trailing, height);
        }

        double _defaultTileHeight {
            get {
                bool hasSubtitle = this.subtitle != null;
                bool isTwoLine = !this.isThreeLine && hasSubtitle;
                bool isOneLine = !this.isThreeLine && !hasSubtitle;

                if (isOneLine) {
                    return this.isDense ? 48.0 : 56.0;
                }

                if (isTwoLine) {
                    return this.isDense ? 64.0 : 72.0;
                }

                return this.isDense ? 76.0 : 88.0;
            }
        }

        protected override double computeMinIntrinsicHeight(double width) {
            return Math.Max(
                this._defaultTileHeight,
                this.title.getMinIntrinsicHeight(width) + this.subtitle?.getMinIntrinsicHeight(width) ?? 0.0);
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            return this.computeMinIntrinsicHeight(width);
        }

        protected override double? computeDistanceToActualBaseline(TextBaseline baseline) {
            D.assert(this.title != null);
            BoxParentData parentData = (BoxParentData) this.title.parentData;
            return parentData.offset.dy + this.title.getDistanceToActualBaseline(baseline);
        }

        static double _boxBaseline(RenderBox box, TextBaseline baseline) {
            return box.getDistanceToBaseline(baseline) ?? 0.0;
        }

        static Size _layoutBox(RenderBox box, BoxConstraints constraints) {
            if (box == null) {
                return Size.zero;
            }

            box.layout(constraints, parentUsesSize: true);
            return box.size;
        }

        static void _positionBox(RenderBox box, Offset offset) {
            BoxParentData parentData = (BoxParentData) box.parentData;
            parentData.offset = offset;
        }

        protected override void performLayout() {
            bool hasLeading = this.leading != null;
            bool hasSubtitle = this.subtitle != null;
            bool hasTrailing = this.trailing != null;
            bool isTwoLine = !this.isThreeLine && hasSubtitle;
            bool isOneLine = !this.isThreeLine && !hasSubtitle;
            BoxConstraints looseConstraints = this.constraints.loosen();

            double tileWidth = looseConstraints.maxWidth;
            Size leadingSize = _layoutBox(this.leading, looseConstraints);
            Size trailingSize = _layoutBox(this.trailing, looseConstraints);

            double titleStart = hasLeading ? Math.Max(_minLeadingWidth, leadingSize.width) + _horizontalTitleGap : 0.0;
            BoxConstraints textConstraints = looseConstraints.tighten(
                width: tileWidth - titleStart - (hasTrailing ? trailingSize.width + _horizontalTitleGap : 0.0));
            Size titleSize = _layoutBox(this.title, textConstraints);
            Size subtitleSize = _layoutBox(this.subtitle, textConstraints);

            double titleBaseline = 0.0;
            double subtitleBaseline = 0.0;
            if (isTwoLine) {
                titleBaseline = this.isDense ? 28.0 : 32.0;
                subtitleBaseline = this.isDense ? 48.0 : 52.0;
            }
            else if (this.isThreeLine) {
                titleBaseline = this.isDense ? 22.0 : 28.0;
                subtitleBaseline = this.isDense ? 42.0 : 48.0;
            }
            else {
                D.assert(isOneLine);
            }

            double tileHeight = 0.0;
            double titleY = 0.0;
            double subtitleY = 0.0;
            if (!hasSubtitle) {
                tileHeight = Math.Max(this._defaultTileHeight, titleSize.height + 2.0 * _minVerticalPadding);
                titleY = (tileHeight - titleSize.height) / 2.0;
            }
            else {
                D.assert(this.subtitleBaselineType != null);
                titleY = titleBaseline - _boxBaseline(this.title, this.titleBaselineType);
                subtitleY = subtitleBaseline -
                            _boxBaseline(this.subtitle, this.subtitleBaselineType ?? TextBaseline.alphabetic);
                tileHeight = this._defaultTileHeight;

                double titleOverlap = titleY + titleSize.height - subtitleY;
                if (titleOverlap > 0.0) {
                    titleY -= titleOverlap / 2.0;
                    subtitleY += titleOverlap / 2.0;
                }

                if (titleY < _minVerticalPadding ||
                    (subtitleY + subtitleSize.height + _minVerticalPadding) > tileHeight) {
                    tileHeight = titleSize.height + subtitleSize.height + 2.0 * _minVerticalPadding;
                    titleY = _minVerticalPadding;
                    subtitleY = titleSize.height + _minVerticalPadding;
                }
            }

            double leadingY = (tileHeight - leadingSize.height) / 2.0;
            double trailingY = (tileHeight - trailingSize.height) / 2.0;

            if (hasLeading) {
                _positionBox(this.leading, new Offset(0.0, leadingY));
            }

            _positionBox(this.title, new Offset(titleStart, titleY));
            if (hasSubtitle) {
                _positionBox(this.subtitle, new Offset(titleStart, subtitleY));
            }

            if (hasTrailing) {
                _positionBox(this.trailing, new Offset(tileWidth - trailingSize.width, trailingY));
            }

            this.size = this.constraints.constrain(new Size(tileWidth, tileHeight));
            D.assert(this.size.width == this.constraints.constrainWidth(tileWidth));
            D.assert(this.size.height == this.constraints.constrainHeight(tileHeight));
        }

        public override void paint(PaintingContext context, Offset offset) {
            void doPaint(RenderBox child) {
                if (child != null) {
                    BoxParentData parentData = (BoxParentData) child.parentData;
                    context.paintChild(child, parentData.offset + offset);
                }
            }

            doPaint(this.leading);
            doPaint(this.title);
            doPaint(this.subtitle);
            doPaint(this.trailing);
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position) {
            D.assert(position != null);
            foreach (RenderBox child in this._children) {
                BoxParentData parentData = (BoxParentData) child.parentData;
                if (child.hitTest(result, position: position - parentData.offset)) {
                    return true;
                }
            }

            return false;
        }
    }
}