using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoTabScaffold : StatefulWidget {
        public CupertinoTabScaffold(
            Key key = null,
            CupertinoTabBar tabBar = null,
            IndexedWidgetBuilder tabBuilder = null,
            Color backgroundColor = null,
            bool resizeToAvoidBottomInset = true
        ) : base(key: key) {
            D.assert(tabBar != null);
            D.assert(tabBuilder != null);
            this.tabBar = tabBar;
            this.tabBuilder = tabBuilder;
            this.backgroundColor = backgroundColor;
            this.resizeToAvoidBottomInset = resizeToAvoidBottomInset;
        }


        public readonly CupertinoTabBar tabBar;

        public readonly IndexedWidgetBuilder tabBuilder;

        public readonly Color backgroundColor;

        public readonly bool resizeToAvoidBottomInset;

        public override State createState() {
            return new _CupertinoTabScaffoldState();
        }
    }

    class _CupertinoTabScaffoldState : State<CupertinoTabScaffold> {
        int _currentPage;

        public override void initState() {
            base.initState();
            this._currentPage = this.widget.tabBar.currentIndex;
            
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            CupertinoTabScaffold oldWidget = _oldWidget as CupertinoTabScaffold;
            base.didUpdateWidget(oldWidget);
            if (this._currentPage >= this.widget.tabBar.items.Count) {
                this._currentPage = this.widget.tabBar.items.Count - 1;
                D.assert(this._currentPage >= 0,
                    () => "CupertinoTabBar is expected to keep at least 2 tabs after updating"
                );
            }

            if (this.widget.tabBar.currentIndex != oldWidget.tabBar.currentIndex) {
                this._currentPage = this.widget.tabBar.currentIndex;
            }
        }

        public override Widget build(BuildContext context) {
            List<Widget> stacked = new List<Widget> { };

            MediaQueryData existingMediaQuery = MediaQuery.of(context);
            MediaQueryData newMediaQuery = MediaQuery.of(context);

            Widget content = new _TabSwitchingView(
                currentTabIndex: this._currentPage,
                tabNumber: this.widget.tabBar.items.Count,
                tabBuilder: this.widget.tabBuilder
            );
            EdgeInsets contentPadding = EdgeInsets.zero;

            if (this.widget.resizeToAvoidBottomInset) {
                newMediaQuery = newMediaQuery.removeViewInsets(removeBottom: true);
                contentPadding = EdgeInsets.only(bottom: existingMediaQuery.viewInsets.bottom);
            }

            if (this.widget.tabBar != null &&
                (!this.widget.resizeToAvoidBottomInset ||
                 this.widget.tabBar.preferredSize.height > existingMediaQuery.viewInsets.bottom)) {
                float bottomPadding = this.widget.tabBar.preferredSize.height + existingMediaQuery.padding.bottom;

                if (this.widget.tabBar.opaque(context)) {
                    contentPadding = EdgeInsets.only(bottom: bottomPadding);
                }
                else {
                    newMediaQuery = newMediaQuery.copyWith(
                        padding: newMediaQuery.padding.copyWith(
                            bottom: bottomPadding
                        )
                    );
                }
            }

            content = new MediaQuery(
                data: newMediaQuery,
                child: new Padding(
                    padding: contentPadding,
                    child: content
                )
            );

            stacked.Add(content);

            if (this.widget.tabBar != null) {
                stacked.Add(new Align(
                    alignment: Alignment.bottomCenter,
                    child: this.widget.tabBar.copyWith(
                        currentIndex: this._currentPage,
                        onTap: (int newIndex) => {
                            this.setState(() => { this._currentPage = newIndex; });
                            if (this.widget.tabBar.onTap != null) {
                                this.widget.tabBar.onTap(newIndex);
                            }
                        }
                    )
                ));
            }

            return new DecoratedBox(
                decoration: new BoxDecoration(
                    color: this.widget.backgroundColor ?? CupertinoTheme.of(context).scaffoldBackgroundColor
                ),
                child: new Stack(
                    children: stacked
                )
            );
        }
    }

    class _TabSwitchingView : StatefulWidget {
        public _TabSwitchingView(
            int currentTabIndex,
            int tabNumber,
            IndexedWidgetBuilder tabBuilder
        ) {
            D.assert(tabNumber > 0);
            D.assert(tabBuilder != null);
            this.currentTabIndex = currentTabIndex;
            this.tabNumber = tabNumber;
            this.tabBuilder = tabBuilder;
        }

        public readonly int currentTabIndex;
        public readonly int tabNumber;
        public readonly IndexedWidgetBuilder tabBuilder;

        public override State createState() {
            return new _TabSwitchingViewState();
        }
    }

    class _TabSwitchingViewState : State<_TabSwitchingView> {
        List<Widget> tabs;
        List<FocusScopeNode> tabFocusNodes;

        public override void initState() {
            base.initState();
            this.tabs = new List<Widget>(this.widget.tabNumber);
            for (int i = 0; i < this.widget.tabNumber; i++) {
                this.tabs.Add(null);
            }
            this.tabFocusNodes = Enumerable.Repeat(new FocusScopeNode(), this.widget.tabNumber).ToList();
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this._focusActiveTab();
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            _TabSwitchingView oldWidget = _oldWidget as _TabSwitchingView;
            base.didUpdateWidget(oldWidget);
            this._focusActiveTab();
        }

        void _focusActiveTab() {
            FocusScope.of(this.context).setFirstFocus(this.tabFocusNodes[this.widget.currentTabIndex]);
        }

        public override void dispose() {
            foreach (FocusScopeNode focusScopeNode in this.tabFocusNodes) {
                focusScopeNode.detach();
            }

            base.dispose();
        }

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget>();
            for (int index = 0; index < this.widget.tabNumber; index++) {
                bool active = index == this.widget.currentTabIndex;

                var tabIndex = index;
                if (active || this.tabs[index] != null) {
                    this.tabs[index] = this.widget.tabBuilder(context, tabIndex);
                }

                children.Add(new Offstage(
                    offstage: !active,
                    child: new TickerMode(
                        enabled: active,
                        child: new FocusScope(
                            node: this.tabFocusNodes[index],
                            child: this.tabs[index] ?? new Container()
                        )
                    )
                ));
            }

            return new Stack(
                fit: StackFit.expand,
                children: children
            );
        }
    }
}