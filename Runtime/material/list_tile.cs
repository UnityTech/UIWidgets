using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
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

        public static IEnumerable<Widget> divideTiles(BuildContext context = null, IEnumerable<Widget> tiles = null,
            Color color = null) {
            D.assert(tiles != null);
            D.assert(color != null || context != null);

            IEnumerator<Widget> enumerator = tiles.GetEnumerator();
            List<Widget> result = new List<Widget> { };

            Decoration decoration = new BoxDecoration(
                border: new Border(
                    bottom: Divider.createBorderSide(context, color: color)
                )
            );

            Widget tile = enumerator.Current;
            while (enumerator.MoveNext()) {
                result.Add(new DecoratedBox(
                    position: DecorationPosition.foreground,
                    decoration: decoration,
                    child: tile
                ));
                tile = enumerator.Current;
            }

            return result;
        }

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
                ? style.copyWith(fontSize: 13.0f, color: color)
                : style.copyWith(color: color);
        }

        TextStyle _subtitleTextStyle(ThemeData theme, ListTileTheme tileTheme) {
            TextStyle style = theme.textTheme.body1;
            Color color = this._textColor(theme, tileTheme, theme.textTheme.caption.color);
            return this._isDenseLayout(tileTheme)
                ? style.copyWith(color: color, fontSize: 12.0f)
                : style.copyWith(color: color);
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
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

            EdgeInsets _defaultContentPadding = EdgeInsets.symmetric(horizontal: 16.0f);
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
            this._mountChild(this.widget.leading, _ListTileSlot.leading);
            this._mountChild(this.widget.title, _ListTileSlot.title);
            this._mountChild(this.widget.subtitle, _ListTileSlot.subtitle);
            this._mountChild(this.widget.trailing, _ListTileSlot.trailing);
        }

        void _updateChild(Widget widget, _ListTileSlot slot) {
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
            D.assert(false, () => "not reachable");
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

        const float _minLeadingWidth = 40.0f;

        const float _horizontalTitleGap = 16.0f;

        const float _minVerticalPadding = 4.0f;

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

        protected override bool sizedByParent {
            get { return false; }
        }

        static float _minWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMinIntrinsicWidth(height);
        }

        static float _maxWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMaxIntrinsicWidth(height);
        }

        protected override float computeMinIntrinsicWidth(float height) {
            float leadingWidth = this.leading != null
                ? Mathf.Max(this.leading.getMinIntrinsicWidth(height), _minLeadingWidth) + _horizontalTitleGap
                : 0.0f;
            return leadingWidth + Mathf.Max(_minWidth(this.title, height), _minWidth(this.subtitle, height)) +
                   _maxWidth(this.trailing, height);
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            float leadingWidth = this.leading != null
                ? Mathf.Max(this.leading.getMaxIntrinsicWidth(height), _minLeadingWidth) + _horizontalTitleGap
                : 0.0f;
            return leadingWidth + Mathf.Max(_maxWidth(this.title, height), _maxWidth(this.subtitle, height)) +
                   _maxWidth(this.trailing, height);
        }

        float _defaultTileHeight {
            get {
                bool hasSubtitle = this.subtitle != null;
                bool isTwoLine = !this.isThreeLine && hasSubtitle;
                bool isOneLine = !this.isThreeLine && !hasSubtitle;

                if (isOneLine) {
                    return this.isDense ? 48.0f : 56.0f;
                }

                if (isTwoLine) {
                    return this.isDense ? 64.0f : 72.0f;
                }

                return this.isDense ? 76.0f : 88.0f;
            }
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return Mathf.Max(
                this._defaultTileHeight,
                this.title.getMinIntrinsicHeight(width) + this.subtitle?.getMinIntrinsicHeight(width) ?? 0.0f);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this.computeMinIntrinsicHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            D.assert(this.title != null);
            BoxParentData parentData = (BoxParentData) this.title.parentData;
            return parentData.offset.dy + this.title.getDistanceToActualBaseline(baseline);
        }

        static float _boxBaseline(RenderBox box, TextBaseline baseline) {
            return box.getDistanceToBaseline(baseline) ?? 0.0f;
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
            BoxConstraints maxIconHeightConstrains = new BoxConstraints(
                maxHeight: this.isDense ? 48.0f: 56.0f
            );
            BoxConstraints looseConstraints = this.constraints.loosen();
            BoxConstraints iconConstraints = looseConstraints.enforce(maxIconHeightConstrains);

            float tileWidth = looseConstraints.maxWidth;
            Size leadingSize = _layoutBox(this.leading, iconConstraints);
            Size trailingSize = _layoutBox(this.trailing, iconConstraints);
            D.assert(
                tileWidth != leadingSize.width,
                () => "Leading widget consumes entire width. Please use a sized widget."
            );
            D.assert(
                tileWidth != trailingSize.width,
                () => "Trailing widget consumes entire width. Please use a sized widget."
            );

            float titleStart = hasLeading
                ? Mathf.Max(_minLeadingWidth, leadingSize.width) + _horizontalTitleGap
                : 0.0f;
            BoxConstraints textConstraints = looseConstraints.tighten(
                width: tileWidth - titleStart - (hasTrailing ? trailingSize.width + _horizontalTitleGap : 0.0f));
            Size titleSize = _layoutBox(this.title, textConstraints);
            Size subtitleSize = _layoutBox(this.subtitle, textConstraints);

            float titleBaseline = 0.0f;
            float subtitleBaseline = 0.0f;
            if (isTwoLine) {
                titleBaseline = this.isDense ? 28.0f : 32.0f;
                subtitleBaseline = this.isDense ? 48.0f : 52.0f;
            }
            else if (this.isThreeLine) {
                titleBaseline = this.isDense ? 22.0f : 28.0f;
                subtitleBaseline = this.isDense ? 42.0f : 48.0f;
            }
            else {
                D.assert(isOneLine);
            }

            float defaultTileHeight = this._defaultTileHeight;

            float tileHeight = 0.0f;
            float titleY = 0.0f;
            float subtitleY = 0.0f;
            if (!hasSubtitle) {
                tileHeight = Mathf.Max(defaultTileHeight, titleSize.height + 2.0f * _minVerticalPadding);
                titleY = (tileHeight - titleSize.height) / 2.0f;
            }
            else {
                D.assert(this.subtitleBaselineType != null);
                titleY = titleBaseline - _boxBaseline(this.title, this.titleBaselineType);
                subtitleY = subtitleBaseline -
                            _boxBaseline(this.subtitle, this.subtitleBaselineType ?? TextBaseline.alphabetic);
                tileHeight = defaultTileHeight;

                float titleOverlap = titleY + titleSize.height - subtitleY;
                if (titleOverlap > 0.0f) {
                    titleY -= titleOverlap / 2.0f;
                    subtitleY += titleOverlap / 2.0f;
                }

                if (titleY < _minVerticalPadding ||
                    (subtitleY + subtitleSize.height + _minVerticalPadding) > tileHeight) {
                    tileHeight = titleSize.height + subtitleSize.height + 2.0f * _minVerticalPadding;
                    titleY = _minVerticalPadding;
                    subtitleY = titleSize.height + _minVerticalPadding;
                }
            }

            float leadingY;
            float trailingY;

            if (tileHeight > 72.0f) {
                leadingY = 16.0f;
                trailingY = 16.0f;
            }
            else {
                leadingY = Mathf.Min((tileHeight - leadingSize.height) / 2.0f, 16.0f);
                trailingY = (tileHeight - trailingSize.height) / 2.0f;
            }

            if (hasLeading) {
                _positionBox(this.leading, new Offset(0.0f, leadingY));
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