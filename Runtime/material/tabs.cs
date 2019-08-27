using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    static class TabsUtils {
        public const float _kTabHeight = 46.0f;
        public const float _kTextAndIconTabHeight = 72.0f;

        public static float _indexChangeProgress(TabController controller) {
            float controllerValue = controller.animation.value;
            float previousIndex = controller.previousIndex;
            float currentIndex = controller.index;

            if (!controller.indexIsChanging) {
                return (currentIndex - controllerValue).abs().clamp(0.0f, 1.0f);
            }

            return (controllerValue - currentIndex).abs() / (currentIndex - previousIndex).abs();
        }

        public static readonly PageScrollPhysics _kTabBarViewPhysics =
            (PageScrollPhysics) new PageScrollPhysics().applyTo(new ClampingScrollPhysics());
    }

    public enum TabBarIndicatorSize {
        tab,
        label
    }

    public class Tab : StatelessWidget {
        public Tab(
            Key key = null,
            string text = null,
            Widget icon = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(text != null || child != null || icon != null);
            D.assert(!(text != null && child != null));
            this.text = text;
            this.icon = icon;
            this.child = child;
        }

        public readonly string text;

        public readonly Widget child;

        public readonly Widget icon;

        Widget _buildLabelText() {
            return this.child ?? new Text(this.text, softWrap: false, overflow: TextOverflow.fade);
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));

            float height = 0f;
            Widget label = null;

            if (this.icon == null) {
                height = TabsUtils._kTabHeight;
                label = this._buildLabelText();
            }
            else if (this.text == null && this.child == null) {
                height = TabsUtils._kTabHeight;
                label = this.icon;
            }
            else {
                height = TabsUtils._kTextAndIconTabHeight;
                label = new Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        new Container(
                            child: this.icon,
                            margin: EdgeInsets.only(bottom: 10.0f)
                        ),
                        this._buildLabelText()
                    }
                );
            }

            return new SizedBox(
                height: height,
                child: new Center(
                    child: label,
                    widthFactor: 1.0f)
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("text", this.text, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Widget>("icon", this.icon,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }


    class _TabStyle : AnimatedWidget {
        public _TabStyle(
            Key key = null,
            Animation<float> animation = null,
            bool? selected = null,
            Color labelColor = null,
            Color unselectedLabelColor = null,
            TextStyle labelStyle = null,
            TextStyle unselectedLabelStyle = null,
            Widget child = null
        ) : base(key: key, listenable: animation) {
            D.assert(child != null);
            D.assert(selected != null);
            this.selected = selected.Value;
            this.labelColor = labelColor;
            this.unselectedLabelColor = unselectedLabelColor;
            this.labelStyle = labelStyle;
            this.unselectedLabelStyle = unselectedLabelStyle;
            this.child = child;
        }

        public readonly TextStyle labelStyle;

        public readonly TextStyle unselectedLabelStyle;

        public readonly bool selected;

        public readonly Color labelColor;

        public readonly Color unselectedLabelColor;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            ThemeData themeData = Theme.of(context);
            TabBarTheme tabBarTheme = TabBarTheme.of(context);
            Animation<float> animation = (Animation<float>) this.listenable;
            
            TextStyle defaultStyle = (this.labelStyle
                                      ?? tabBarTheme.labelStyle
                                      ?? themeData.primaryTextTheme.body2).copyWith(inherit: true);
            TextStyle defaultUnselectedStyle = (this.unselectedLabelStyle
                                               ?? tabBarTheme.unselectedLabelStyle
                                               ?? this.labelStyle
                                               ?? themeData.primaryTextTheme.body2).copyWith(inherit: true);
            TextStyle textStyle = this.selected
                ? TextStyle.lerp(defaultStyle, defaultUnselectedStyle, animation.value)
                : TextStyle.lerp(defaultUnselectedStyle, defaultStyle, animation.value);

            Color selectedColor = this.labelColor ?? tabBarTheme.labelColor ?? themeData.primaryTextTheme.body2.color;
            Color unselectedColor = this.unselectedLabelColor ??
                                    tabBarTheme.unselectedLabelColor ?? selectedColor.withAlpha(0xB2);
            Color color = this.selected
                ? Color.lerp(selectedColor, unselectedColor, animation.value)
                : Color.lerp(unselectedColor, selectedColor, animation.value);

            return new DefaultTextStyle(
                style: textStyle.copyWith(color: color),
                child: IconTheme.merge(
                    data: new IconThemeData(
                        size: 24.0f,
                        color: color),
                    child: this.child
                )
            );
        }
    }

    delegate void _LayoutCallback(List<float> xOffsets, float width);


    class _TabLabelBarRenderer : RenderFlex {
        public _TabLabelBarRenderer(
            List<RenderBox> children = null,
            Axis? direction = null,
            MainAxisSize? mainAxisSize = null,
            MainAxisAlignment? mainAxisAlignment = null,
            CrossAxisAlignment? crossAxisAlignment = null,
            VerticalDirection? verticalDirection = null,
            _LayoutCallback onPerformLayout = null
        ) : base(
            children: children,
            direction: direction.Value,
            mainAxisSize: mainAxisSize.Value,
            mainAxisAlignment: mainAxisAlignment.Value,
            crossAxisAlignment: crossAxisAlignment.Value,
            verticalDirection: verticalDirection.Value
        ) {
            D.assert(direction != null);
            D.assert(mainAxisSize != null);
            D.assert(mainAxisAlignment != null);
            D.assert(crossAxisAlignment != null);
            D.assert(verticalDirection != null);

            D.assert(onPerformLayout != null);
            this.onPerformLayout = onPerformLayout;
        }

        public _LayoutCallback onPerformLayout;

        protected override void performLayout() {
            base.performLayout();

            RenderBox child = this.firstChild;
            List<float> xOffsets = new List<float>();

            while (child != null) {
                FlexParentData childParentData = (FlexParentData) child.parentData;
                xOffsets.Add(childParentData.offset.dx);
                D.assert(child.parentData == childParentData);
                child = childParentData.nextSibling;
            }

            xOffsets.Add(this.size.width);
            this.onPerformLayout(xOffsets, this.size.width);
        }
    }


    class _TabLabelBar : Flex {
        public _TabLabelBar(
            Key key = null,
            List<Widget> children = null,
            _LayoutCallback onPerformLayout = null
        ) : base(
            key: key,
            children: children ?? new List<Widget>(),
            direction: Axis.horizontal,
            mainAxisSize: MainAxisSize.max,
            mainAxisAlignment: MainAxisAlignment.start,
            crossAxisAlignment: CrossAxisAlignment.center,
            verticalDirection: VerticalDirection.down
        ) {
            this.onPerformLayout = onPerformLayout;
        }

        public readonly _LayoutCallback onPerformLayout;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _TabLabelBarRenderer(
                direction: this.direction,
                mainAxisAlignment: this.mainAxisAlignment,
                mainAxisSize: this.mainAxisSize,
                crossAxisAlignment: this.crossAxisAlignment,
                verticalDirection: this.verticalDirection,
                onPerformLayout: this.onPerformLayout
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            base.updateRenderObject(context, renderObject);
            _TabLabelBarRenderer _renderObject = (_TabLabelBarRenderer) renderObject;
            _renderObject.onPerformLayout = this.onPerformLayout;
        }
    }

    class _IndicatorPainter : AbstractCustomPainter {
        public _IndicatorPainter(
            TabController controller = null,
            Decoration indicator = null,
            TabBarIndicatorSize? indicatorSize = null,
            List<GlobalKey> tabKeys = null,
            _IndicatorPainter old = null
        ) : base(repaint: controller.animation) {
            D.assert(controller != null);
            D.assert(indicator != null);
            this.controller = controller;
            this.indicator = indicator;
            this.indicatorSize = indicatorSize;
            this.tabKeys = tabKeys;
            if (old != null) {
                this.saveTabOffsets(old._currentTabOffsets);
            }
        }

        public readonly TabController controller;

        public readonly Decoration indicator;

        public readonly TabBarIndicatorSize? indicatorSize;

        public readonly List<GlobalKey> tabKeys;

        List<float> _currentTabOffsets;
        Rect _currentRect;
        BoxPainter _painter;
        bool _needsPaint = false;

        void markNeedsPaint() {
            this._needsPaint = true;
        }

        public void dispose() {
            this._painter?.Dispose();
        }

        public void saveTabOffsets(List<float> tabOffsets) {
            this._currentTabOffsets = tabOffsets;
        }

        public int maxTabIndex {
            get { return this._currentTabOffsets.Count - 2; }
        }

        public float centerOf(int tabIndex) {
            D.assert(this._currentTabOffsets != null);
            D.assert(this._currentTabOffsets.isNotEmpty());
            D.assert(tabIndex >= 0);
            D.assert(tabIndex <= this.maxTabIndex);
            return (this._currentTabOffsets[tabIndex] + this._currentTabOffsets[tabIndex + 1]) / 2.0f;
        }

        public Rect indicatorRect(Size tabBarSize, int tabIndex) {
            D.assert(this._currentTabOffsets != null);
            D.assert(this._currentTabOffsets.isNotEmpty());
            D.assert(tabIndex >= 0);
            D.assert(tabIndex <= this.maxTabIndex);
            float tabLeft = this._currentTabOffsets[tabIndex];
            float tabRight = this._currentTabOffsets[tabIndex + 1];

            if (this.indicatorSize == TabBarIndicatorSize.label) {
                float tabWidth = this.tabKeys[tabIndex].currentContext.size.width;
                float delta = ((tabRight - tabLeft) - tabWidth) / 2.0f;
                tabLeft += delta;
                tabRight -= delta;
            }

            return Rect.fromLTWH(tabLeft, 0.0f, tabRight - tabLeft, tabBarSize.height);
        }

        public override void paint(Canvas canvas, Size size) {
            this._needsPaint = false;
            this._painter = this._painter ?? this.indicator.createBoxPainter(this.markNeedsPaint);

            if (this.controller.indexIsChanging) {
                Rect targetRect = this.indicatorRect(size, this.controller.index);
                this._currentRect = Rect.lerp(targetRect, this._currentRect ?? targetRect,
                    TabsUtils._indexChangeProgress(this.controller));
            }
            else {
                int currentIndex = this.controller.index;
                Rect previous = currentIndex > 0 ? this.indicatorRect(size, currentIndex - 1) : null;
                Rect middle = this.indicatorRect(size, currentIndex);
                Rect next = currentIndex < this.maxTabIndex ? this.indicatorRect(size, currentIndex + 1) : null;
                float index = this.controller.index;
                float value = this.controller.animation.value;
                if (value == index - 1.0f) {
                    this._currentRect = previous ?? middle;
                }
                else if (value == index + 1.0f) {
                    this._currentRect = next ?? middle;
                }
                else if (value == index) {
                    this._currentRect = middle;
                }
                else if (value < index) {
                    this._currentRect = previous == null ? middle : Rect.lerp(middle, previous, index - value);
                }
                else {
                    this._currentRect = next == null ? middle : Rect.lerp(middle, next, value - index);
                }
            }

            D.assert(this._currentRect != null);

            ImageConfiguration configuration = new ImageConfiguration(
                size: this._currentRect.size
            );

            this._painter.paint(canvas, this._currentRect.topLeft, configuration);
        }

        static bool _tabOffsetsEqual(List<float> a, List<float> b) {
            if (a?.Count != b?.Count) {
                return false;
            }

            for (int i = 0; i < a.Count; i++) {
                if (a[i] != b[i]) {
                    return false;
                }
            }

            return true;
        }

        public override bool shouldRepaint(CustomPainter old) {
            _IndicatorPainter _old = (_IndicatorPainter) old;
            return this._needsPaint
                   || this.controller != _old.controller
                   || this.indicator != _old.indicator
                   || this.tabKeys.Count != _old.tabKeys.Count
                   || !_tabOffsetsEqual(this._currentTabOffsets, _old._currentTabOffsets);
        }
    }

    class _ChangeAnimation : AnimationWithParentMixin<float, float> {
        public _ChangeAnimation(
            TabController controller) {
            this.controller = controller;
        }

        public readonly TabController controller;

        public override Animation<float> parent {
            get { return this.controller.animation; }
        }

        public override float value {
            get { return TabsUtils._indexChangeProgress(this.controller); }
        }
    }


    class _DragAnimation : AnimationWithParentMixin<float, float> {
        public _DragAnimation(
            TabController controller,
            int index) {
            this.controller = controller;
            this.index = index;
        }

        public readonly TabController controller;

        public readonly int index;

        public override Animation<float> parent {
            get { return this.controller.animation; }
        }

        public override float value {
            get {
                D.assert(!this.controller.indexIsChanging);
                return (this.controller.animation.value - this.index).abs().clamp(0.0f, 1.0f);
            }
        }
    }


    class _TabBarScrollPosition : ScrollPositionWithSingleContext {
        public _TabBarScrollPosition(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            ScrollPosition oldPosition = null,
            _TabBarState tabBar = null
        ) : base(
            physics: physics,
            context: context,
            initialPixels: null,
            oldPosition: oldPosition) {
            this.tabBar = tabBar;
        }

        public readonly _TabBarState tabBar;

        bool _initialViewportDimensionWasZero;

        public override bool applyContentDimensions(float minScrollExtent, float maxScrollExtent) {
            bool result = true;
            if (this._initialViewportDimensionWasZero != true) {
                this._initialViewportDimensionWasZero = this.viewportDimension != 0.0;
                this.correctPixels(this.tabBar._initialScrollOffset(this.viewportDimension, minScrollExtent,
                    maxScrollExtent));
                result = false;
            }

            return base.applyContentDimensions(minScrollExtent, maxScrollExtent) && result;
        }
    }


    class _TabBarScrollController : ScrollController {
        public _TabBarScrollController(_TabBarState tabBar) {
            this.tabBar = tabBar;
        }

        public readonly _TabBarState tabBar;

        public override ScrollPosition createScrollPosition(ScrollPhysics physics, ScrollContext context,
            ScrollPosition oldPosition) {
            return new _TabBarScrollPosition(
                physics: physics,
                context: context,
                oldPosition: oldPosition,
                tabBar: this.tabBar
            );
        }
    }


    public class TabBar : PreferredSizeWidget {
        public TabBar(
            Key key = null,
            List<Widget> tabs = null,
            TabController controller = null,
            bool isScrollable = false,
            Color indicatorColor = null,
            float indicatorWeight = 2.0f,
            EdgeInsets indicatorPadding = null,
            Decoration indicator = null,
            TabBarIndicatorSize? indicatorSize = null,
            Color labelColor = null,
            TextStyle labelStyle = null,
            EdgeInsets labelPadding = null,
            Color unselectedLabelColor = null,
            TextStyle unselectedLabelStyle = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            ValueChanged<int> onTap = null
        ) : base(key: key) {
            indicatorPadding = indicatorPadding ?? EdgeInsets.zero;
            D.assert(tabs != null);
            D.assert(indicator != null || indicatorWeight > 0.0f);
            D.assert(indicator != null || indicatorPadding != null);
            this.tabs = tabs;
            this.controller = controller;
            this.isScrollable = isScrollable;
            this.indicatorColor = indicatorColor;
            this.indicatorWeight = indicatorWeight;
            this.indicatorPadding = indicatorPadding;
            this.indicator = indicator;
            this.indicatorSize = indicatorSize;
            this.labelColor = labelColor;
            this.labelStyle = labelStyle;
            this.labelPadding = labelPadding;
            this.unselectedLabelColor = unselectedLabelColor;
            this.unselectedLabelStyle = unselectedLabelStyle;
            this.dragStartBehavior = dragStartBehavior;
            this.onTap = onTap;
        }

        public readonly List<Widget> tabs;

        public readonly TabController controller;

        public readonly bool isScrollable;

        public readonly Color indicatorColor;

        public readonly float indicatorWeight;

        public readonly EdgeInsets indicatorPadding;

        public readonly Decoration indicator;

        public readonly TabBarIndicatorSize? indicatorSize;

        public readonly Color labelColor;

        public readonly Color unselectedLabelColor;

        public readonly TextStyle labelStyle;

        public readonly EdgeInsets labelPadding;

        public readonly TextStyle unselectedLabelStyle;

        public readonly DragStartBehavior dragStartBehavior;

        public readonly ValueChanged<int> onTap;

        public override Size preferredSize {
            get {
                foreach (Widget item in this.tabs) {
                    if (item is Tab) {
                        Tab tab = (Tab) item;
                        if (tab.text != null && tab.icon != null) {
                            return Size.fromHeight(TabsUtils._kTextAndIconTabHeight + this.indicatorWeight);
                        }
                    }
                }

                return Size.fromHeight(TabsUtils._kTabHeight + this.indicatorWeight);
            }
        }


        public override State createState() {
            return new _TabBarState();
        }
    }


    class _TabBarState : State<TabBar> {
        ScrollController _scrollController;
        TabController _controller;
        _IndicatorPainter _indicatorPainter;
        int _currentIndex;
        List<GlobalKey> _tabKeys;


        public override void initState() {
            base.initState();
            this._tabKeys = new List<GlobalKey>();
            foreach (Widget tab in this.widget.tabs) {
                this._tabKeys.Add(GlobalKey.key());
            }
        }

        Decoration _indicator {
            get {
                if (this.widget.indicator != null) {
                    return this.widget.indicator;
                }

                TabBarTheme tabBarTheme = TabBarTheme.of(this.context);
                if (tabBarTheme.indicator != null) {
                    return tabBarTheme.indicator;
                }

                Color color = this.widget.indicatorColor ?? Theme.of(this.context).indicatorColor;
                if (color.value == Material.of(this.context).color?.value) {
                    color = Colors.white;
                }

                return new UnderlineTabIndicator(
                    insets: this.widget.indicatorPadding,
                    borderSide: new BorderSide(
                        width: this.widget.indicatorWeight,
                        color: color));
            }
        }

        void _updateTabController() {
            TabController newController = this.widget.controller ?? DefaultTabController.of(this.context);
            D.assert(() => {
                if (newController == null) {
                    throw new UIWidgetsError(
                        "No TabController for " + this.widget.GetType() + ".\n" +
                        "When creating a " + this.widget.GetType() + ", you must either provide an explicit " +
                        "TabController using the \"controller\" property, or you must ensure that there " +
                        "is a DefaultTabController above the " + this.widget.GetType() + ".\n" +
                        "In this case, there was neither an explicit controller nor a default controller."
                    );
                }

                return true;
            });
            D.assert(() => {
                if (newController.length != this.widget.tabs.Count) {
                    throw new UIWidgetsError(
                        $"Controller's length property {newController.length} does not match the\n" +
                        $"number of tab elements {this.widget.tabs.Count} present in TabBar's tabs property."
                    );
                }

                return true;
            });
            if (newController == this._controller) {
                return;
            }

            if (this._controller != null) {
                this._controller.animation.removeListener(this._handleTabControllerAnimationTick);
                this._controller.removeListener(this._handleTabControllerTick);
            }

            this._controller = newController;
            if (this._controller != null) {
                this._controller.animation.addListener(this._handleTabControllerAnimationTick);
                this._controller.addListener(this._handleTabControllerTick);
                this._currentIndex = this._controller.index;
            }
        }

        void _initIndicatorPainter() {
            this._indicatorPainter = this._controller == null
                ? null
                : new _IndicatorPainter(
                    controller: this._controller,
                    indicator: this._indicator,
                    indicatorSize: this.widget.indicatorSize ?? TabBarTheme.of(this.context).indicatorSize,
                    tabKeys: this._tabKeys,
                    old: this._indicatorPainter
                );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            D.assert(MaterialD.debugCheckHasMaterial(this.context));
            this._updateTabController();
            this._initIndicatorPainter();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            TabBar _oldWidget = (TabBar) oldWidget;
            if (this.widget.controller != _oldWidget.controller) {
                this._updateTabController();
                this._initIndicatorPainter();
            }
            else if (this.widget.indicatorColor != _oldWidget.indicatorColor ||
                     this.widget.indicatorWeight != _oldWidget.indicatorWeight ||
                     this.widget.indicatorSize != _oldWidget.indicatorSize ||
                     this.widget.indicator != _oldWidget.indicator) {
                this._initIndicatorPainter();
            }

            if (this.widget.tabs.Count > _oldWidget.tabs.Count) {
                int delta = this.widget.tabs.Count - _oldWidget.tabs.Count;
                for (int i = 0; i < delta; i++) {
                    this._tabKeys.Add(GlobalKey.key());
                }
            }
            else if (this.widget.tabs.Count < _oldWidget.tabs.Count) {
                int delta = _oldWidget.tabs.Count - this.widget.tabs.Count;
                this._tabKeys.RemoveRange(this.widget.tabs.Count, delta);
            }
        }

        public override void dispose() {
            this._indicatorPainter.dispose();
            if (this._controller != null) {
                this._controller.animation.removeListener(this._handleTabControllerAnimationTick);
                this._controller.removeListener(this._handleTabControllerTick);
            }

            base.dispose();
        }

        public int maxTabIndex {
            get { return this._indicatorPainter.maxTabIndex; }
        }

        float _tabScrollOffset(int index, float viewportWidth, float minExtent, float maxExtent) {
            if (!this.widget.isScrollable) {
                return 0.0f;
            }

            float tabCenter = this._indicatorPainter.centerOf(index);
            return (tabCenter - viewportWidth / 2.0f).clamp(minExtent, maxExtent);
        }

        float _tabCenteredScrollOffset(int index) {
            ScrollPosition position = this._scrollController.position;
            return this._tabScrollOffset(index, position.viewportDimension, position.minScrollExtent,
                position.maxScrollExtent);
        }

        internal float _initialScrollOffset(float viewportWidth, float minExtent, float maxExtent) {
            return this._tabScrollOffset(this._currentIndex, viewportWidth, minExtent, maxExtent);
        }

        void _scrollToCurrentIndex() {
            float offset = this._tabCenteredScrollOffset(this._currentIndex);
            this._scrollController.animateTo(offset, duration: Constants.kTabScrollDuration, curve: Curves.ease);
        }

        void _scrollToControllerValue() {
            float? leadingPosition = this._currentIndex > 0
                ? (float?) this._tabCenteredScrollOffset(this._currentIndex - 1)
                : null;
            float middlePosition = this._tabCenteredScrollOffset(this._currentIndex);
            float? trailingPosition = this._currentIndex < this.maxTabIndex
                ? (float?) this._tabCenteredScrollOffset(this._currentIndex + 1)
                : null;

            float index = this._controller.index;
            float value = this._controller.animation.value;
            float offset = 0.0f;
            if (value == index - 1.0f) {
                offset = leadingPosition ?? middlePosition;
            }
            else if (value == index + 1.0f) {
                offset = trailingPosition ?? middlePosition;
            }
            else if (value == index) {
                offset = middlePosition;
            }
            else if (value < index) {
                offset = leadingPosition == null
                    ? middlePosition
                    : MathUtils.lerpNullableFloat(middlePosition, leadingPosition, index - value).Value;
            }
            else {
                offset = trailingPosition == null
                    ? middlePosition
                    : MathUtils.lerpNullableFloat(middlePosition, trailingPosition, value - index).Value;
            }

            this._scrollController.jumpTo(offset);
        }


        void _handleTabControllerAnimationTick() {
            D.assert(this.mounted);
            if (!this._controller.indexIsChanging && this.widget.isScrollable) {
                this._currentIndex = this._controller.index;
                this._scrollToControllerValue();
            }
        }

        void _handleTabControllerTick() {
            if (this._controller.index != this._currentIndex) {
                this._currentIndex = this._controller.index;
                if (this.widget.isScrollable) {
                    this._scrollToCurrentIndex();
                }
            }

            this.setState(() => { });
        }

        void _saveTabOffsets(List<float> tabOffsets, float width) {
            this._indicatorPainter?.saveTabOffsets(tabOffsets);
        }

        void _handleTap(int index) {
            D.assert(index >= 0 && index < this.widget.tabs.Count);
            this._controller.animateTo(index);
            if (this.widget.onTap != null) {
                this.widget.onTap(index);
            }
        }

        Widget _buildStyledTab(Widget child, bool selected, Animation<float> animation) {
            return new _TabStyle(
                animation: animation,
                selected: selected,
                labelColor: this.widget.labelColor,
                unselectedLabelColor: this.widget.unselectedLabelColor,
                labelStyle: this.widget.labelStyle,
                unselectedLabelStyle: this.widget.unselectedLabelStyle,
                child: child
            );
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            if (this._controller.length == 0) {
                return new Container(
                    height: TabsUtils._kTabHeight + this.widget.indicatorWeight
                );
            }

            TabBarTheme tabBarTheme = TabBarTheme.of(context);

            List<Widget> wrappedTabs = new List<Widget>();
            for (int i = 0; i < this.widget.tabs.Count; i++) {
                wrappedTabs.Add(new Center(
                        heightFactor: 1.0f,
                        child: new Padding(
                            padding: this.widget.labelPadding ?? tabBarTheme.labelPadding ?? Constants.kTabLabelPadding,
                            child: new KeyedSubtree(
                                key: this._tabKeys[i],
                                child: this.widget.tabs[i]
                            )
                        )
                    )
                );
            }

            if (this._controller != null) {
                int previousIndex = this._controller.previousIndex;

                if (this._controller.indexIsChanging) {
                    D.assert(this._currentIndex != previousIndex);
                    Animation<float> animation = new _ChangeAnimation(this._controller);
                    wrappedTabs[this._currentIndex] =
                        this._buildStyledTab(wrappedTabs[this._currentIndex], true, animation);
                    wrappedTabs[previousIndex] = this._buildStyledTab(wrappedTabs[previousIndex], false, animation);
                }
                else {
                    int tabIndex = this._currentIndex;
                    Animation<float> centerAnimation = new _DragAnimation(this._controller, tabIndex);
                    wrappedTabs[tabIndex] = this._buildStyledTab(wrappedTabs[tabIndex], true, centerAnimation);
                    if (this._currentIndex > 0) {
                        int previousTabIndex = this._currentIndex - 1;
                        Animation<float> previousAnimation =
                            new ReverseAnimation(new _DragAnimation(this._controller, previousTabIndex));
                        wrappedTabs[previousTabIndex] =
                            this._buildStyledTab(wrappedTabs[previousTabIndex], false, previousAnimation);
                    }

                    if (this._currentIndex < this.widget.tabs.Count - 1) {
                        int nextTabIndex = this._currentIndex + 1;
                        Animation<float> nextAnimation =
                            new ReverseAnimation(new _DragAnimation(this._controller, nextTabIndex));
                        wrappedTabs[nextTabIndex] =
                            this._buildStyledTab(wrappedTabs[nextTabIndex], false, nextAnimation);
                    }
                }
            }

            int tabCount = this.widget.tabs.Count;
            for (int index = 0; index < tabCount; index++) {
                int tabIndex = index;
                wrappedTabs[index] = new InkWell(
                    onTap: () => { this._handleTap(tabIndex); },
                    child: new Padding(
                        padding: EdgeInsets.only(bottom: this.widget.indicatorWeight),
                        child: wrappedTabs[index]
                    )
                );
                if (!this.widget.isScrollable) {
                    wrappedTabs[index] = new Expanded(
                        child: wrappedTabs[index]);
                }
            }

            Widget tabBar = new CustomPaint(
                painter: this._indicatorPainter,
                child: new _TabStyle(
                    animation: Animations.kAlwaysDismissedAnimation,
                    selected: false,
                    labelColor: this.widget.labelColor,
                    unselectedLabelColor: this.widget.unselectedLabelColor,
                    labelStyle: this.widget.labelStyle,
                    unselectedLabelStyle: this.widget.unselectedLabelStyle,
                    child: new _TabLabelBar(
                        onPerformLayout: this._saveTabOffsets,
                        children: wrappedTabs
                    )
                )
            );

            if (this.widget.isScrollable) {
                this._scrollController = this._scrollController ?? new _TabBarScrollController(this);
                tabBar = new SingleChildScrollView(
                    dragStartBehavior: this.widget.dragStartBehavior,
                    scrollDirection: Axis.horizontal,
                    controller: this._scrollController,
                    child: tabBar);
            }

            return tabBar;
        }
    }


    public class TabBarView : StatefulWidget {
        public TabBarView(
            Key key = null,
            List<Widget> children = null,
            TabController controller = null,
            ScrollPhysics physics = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(children != null);
            this.children = children;
            this.controller = controller;
            this.physics = physics;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly TabController controller;

        public readonly List<Widget> children;

        public readonly ScrollPhysics physics;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _TabBarViewState();
        }
    }

    class _TabBarViewState : State<TabBarView> {
        TabController _controller;
        PageController _pageController;
        List<Widget> _children;
        int? _currentIndex = null;
        int _warpUnderwayCount = 0;

        void _updateTabController() {
            TabController newController = this.widget.controller ?? DefaultTabController.of(this.context);
            D.assert(() => {
                if (newController == null) {
                    throw new UIWidgetsError(
                        "No TabController for " + this.widget.GetType() + "\n" +
                        "When creating a " + this.widget.GetType() + ", you must either provide an explicit " +
                        "TabController using the \"controller\" property, or you must ensure that there " +
                        "is a DefaultTabController above the " + this.widget.GetType() + ".\n" +
                        "In this case, there was neither an explicit controller nor a default controller."
                    );
                }

                return true;
            });
            D.assert(() => {
                if (newController.length != this.widget.children.Count) {
                    throw new UIWidgetsError(
                        $"Controller's length property {newController.length} does not match the\n" +
                        $"number of elements {this.widget.children.Count} present in TabBarView's children property."
                    );
                }

                return true;
            });
            if (newController == this._controller) {
                return;
            }

            if (this._controller != null) {
                this._controller.animation.removeListener(this._handleTabControllerAnimationTick);
            }

            this._controller = newController;
            if (this._controller != null) {
                this._controller.animation.addListener(this._handleTabControllerAnimationTick);
            }
        }


        public override void initState() {
            base.initState();
            this._children = this.widget.children;
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this._updateTabController();
            this._currentIndex = this._controller?.index;
            this._pageController = new PageController(initialPage: this._currentIndex ?? 0);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            TabBarView _oldWidget = (TabBarView) oldWidget;
            if (this.widget.controller != _oldWidget.controller) {
                this._updateTabController();
            }

            if (this.widget.children != _oldWidget.children && this._warpUnderwayCount == 0) {
                this._children = this.widget.children;
            }
        }

        public override void dispose() {
            if (this._controller != null) {
                this._controller.animation.removeListener(this._handleTabControllerAnimationTick);
            }

            base.dispose();
        }

        void _handleTabControllerAnimationTick() {
            if (this._warpUnderwayCount > 0 || !this._controller.indexIsChanging) {
                return;
            }

            if (this._controller.index != this._currentIndex) {
                this._currentIndex = this._controller.index;
                this._warpToCurrentIndex();
            }
        }

        void _warpToCurrentIndex() {
            if (!this.mounted) {
                return;
            }

            if (this._pageController.page == this._currentIndex) {
                return;
            }

            int previousIndex = this._controller.previousIndex;
            if ((this._currentIndex.Value - previousIndex).abs() == 1) {
                this._pageController.animateToPage(this._currentIndex.Value, duration: Constants.kTabScrollDuration,
                    curve: Curves.ease);
                return;
            }

            D.assert((this._currentIndex.Value - previousIndex).abs() > 1);
            int initialPage = 0;
            this.setState(() => {
                this._warpUnderwayCount += 1;
                this._children = new List<Widget>(this.widget.children);
                if (this._currentIndex > previousIndex) {
                    this._children[this._currentIndex.Value - 1] = this._children[previousIndex];
                    initialPage = this._currentIndex.Value - 1;
                }
                else {
                    this._children[this._currentIndex.Value + 1] = this._children[previousIndex];
                    initialPage = this._currentIndex.Value + 1;
                }
            });

            this._pageController.jumpToPage(initialPage);
            this._pageController.animateToPage(this._currentIndex.Value, duration: Constants.kTabScrollDuration,
                curve: Curves.ease).Then(() => {
                if (!this.mounted) {
                    return new Promise();
                }

                this.setState(() => {
                    this._warpUnderwayCount -= 1;
                    this._children = this.widget.children;
                });

                return new Promise();
            });
        }

        bool _handleScrollNotification(ScrollNotification notification) {
            if (this._warpUnderwayCount > 0) {
                return false;
            }

            if (notification.depth != 0) {
                return false;
            }

            this._warpUnderwayCount += 1;
            if (notification is ScrollUpdateNotification && !this._controller.indexIsChanging) {
                if ((this._pageController.page - this._controller.index).abs() > 1.0) {
                    this._controller.index = this._pageController.page.floor();
                    this._currentIndex = this._controller.index;
                }

                this._controller.offset = (this._pageController.page - this._controller.index).clamp(-1.0f, 1.0f);
            }
            else if (notification is ScrollEndNotification) {
                this._controller.index = this._pageController.page.round();
                this._currentIndex = this._controller.index;
            }

            this._warpUnderwayCount -= 1;

            return false;
        }

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: this._handleScrollNotification,
                child: new PageView(
                    dragStartBehavior: this.widget.dragStartBehavior,
                    controller: this._pageController,
                    physics: this.widget.physics == null
                        ? TabsUtils._kTabBarViewPhysics
                        : TabsUtils._kTabBarViewPhysics.applyTo(this.widget.physics),
                    children: this._children
                )
            );
        }
    }


    public class TabPageSelectorIndicator : StatelessWidget {
        public TabPageSelectorIndicator(
            Key key = null,
            Color backgroundColor = null,
            Color borderColor = null,
            float? size = null
        ) : base(key: key) {
            D.assert(backgroundColor != null);
            D.assert(borderColor != null);
            D.assert(size != null);

            this.backgroundColor = backgroundColor;
            this.borderColor = borderColor;
            this.size = size.Value;
        }

        public readonly Color backgroundColor;

        public readonly Color borderColor;

        public readonly float size;

        public override Widget build(BuildContext context) {
            return new Container(
                width: this.size,
                height: this.size,
                margin: EdgeInsets.all(4.0f),
                decoration: new BoxDecoration(
                    color: this.backgroundColor,
                    border: Border.all(color: this.borderColor),
                    shape: BoxShape.circle
                )
            );
        }
    }


    public class TabPageSelector : StatelessWidget {
        public TabPageSelector(
            Key key = null,
            TabController controller = null,
            float indicatorSize = 12.0f,
            Color color = null,
            Color selectedColor = null
        ) : base(key: key) {
            D.assert(indicatorSize > 0.0f);
            this.controller = controller;
            this.indicatorSize = indicatorSize;
            this.color = color;
            this.selectedColor = selectedColor;
        }

        public readonly TabController controller;

        public readonly float indicatorSize;

        public readonly Color color;

        public readonly Color selectedColor;

        Widget _buildTabIndicator(
            int tabIndex,
            TabController tabController,
            ColorTween selectedColorTween,
            ColorTween previousColorTween) {
            Color background = null;
            if (tabController.indexIsChanging) {
                float t = 1.0f - TabsUtils._indexChangeProgress(tabController);
                if (tabController.index == tabIndex) {
                    background = selectedColorTween.lerp(t);
                }
                else if (tabController.previousIndex == tabIndex) {
                    background = previousColorTween.lerp(t);
                }
                else {
                    background = selectedColorTween.begin;
                }
            }
            else {
                float offset = tabController.offset;
                if (tabController.index == tabIndex) {
                    background = selectedColorTween.lerp(1.0f - offset.abs());
                }
                else if (tabController.index == tabIndex - 1 && offset > 0.0) {
                    background = selectedColorTween.lerp(offset);
                }
                else if (tabController.index == tabIndex + 1 && offset < 0.0) {
                    background = selectedColorTween.lerp(-offset);
                }
                else {
                    background = selectedColorTween.begin;
                }
            }

            return new TabPageSelectorIndicator(
                backgroundColor: background,
                borderColor: selectedColorTween.end,
                size: this.indicatorSize
            );
        }

        public override Widget build(BuildContext context) {
            Color fixColor = this.color ?? Colors.transparent;
            Color fixSelectedColor = this.selectedColor ?? Theme.of(context).accentColor;
            ColorTween selectedColorTween = new ColorTween(begin: fixColor, end: fixSelectedColor);
            ColorTween previousColorTween = new ColorTween(begin: fixSelectedColor, end: fixColor);
            TabController tabController = this.controller ?? DefaultTabController.of(context);
            D.assert(() => {
                if (tabController == null) {
                    throw new UIWidgetsError(
                        "No TabController for " + this.GetType() + ".\n" +
                        "When creating a " + this.GetType() + ", you must either provide an explicit TabController " +
                        "using the \"controller\" property, or you must ensure that there is a " +
                        "DefaultTabController above the " + this.GetType() + ".\n" +
                        "In this case, there was neither an explicit controller nor a default controller."
                    );
                }

                return true;
            });

            Animation<float> animation = new CurvedAnimation(
                parent: tabController.animation,
                curve: Curves.fastOutSlowIn
            );

            return new AnimatedBuilder(
                animation: animation,
                builder: (BuildContext subContext, Widget child) => {
                    List<Widget> children = new List<Widget>();

                    for (int tabIndex = 0; tabIndex < tabController.length; tabIndex++) {
                        children.Add(this._buildTabIndicator(
                            tabIndex,
                            tabController,
                            selectedColorTween,
                            previousColorTween)
                        );
                    }

                    return new Row(
                        mainAxisSize: MainAxisSize.min,
                        children: children
                    );
                }
            );
        }
    }
}