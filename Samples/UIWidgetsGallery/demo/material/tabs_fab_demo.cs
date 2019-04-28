using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class TabsFabDemoUtils {
        public const string _explanatoryText =
            "When the Scaffold's floating action button changes, the new button fades and " +
            "turns into view. In this demo, changing tabs can cause the app to be rebuilt " +
            "with a FloatingActionButton that the Scaffold distinguishes from the others " +
            "by its key.";

        public static readonly List<_Page> _allPages = new List<_Page> {
            new _Page(label: "Blue", colors: Colors.indigo, icon: Icons.add),
            new _Page(label: "Eco", colors: Colors.green, icon: Icons.create),
            new _Page(label: "No"),
            new _Page(label: "Teal", colors: Colors.teal, icon: Icons.add),
            new _Page(label: "Red", colors: Colors.red, icon: Icons.create)
        };
    }

    class _Page {
        public _Page(
            string label,
            MaterialColor colors = null,
            IconData icon = null
        ) {
            this.label = label;
            this.colors = colors;
            this.icon = icon;
        }

        public readonly string label;
        public readonly MaterialColor colors;
        public readonly IconData icon;

        public Color labelColor {
            get { return this.colors != null ? this.colors.shade300 : Colors.grey.shade300; }
        }

        public bool fabDefined {
            get { return this.colors != null && this.icon != null; }
        }

        public Color fabColor {
            get { return this.colors.shade400; }
        }

        public Icon fabIcon {
            get { return new Icon(this.icon); }
        }

        public Key fabKey {
            get { return new ValueKey<Color>(this.fabColor); }
        }
    }


    public class TabsFabDemo : StatefulWidget {
        public const string routeName = "/material/tabs-fab";

        public override State createState() {
            return new _TabsFabDemoState();
        }
    }

    class _TabsFabDemoState : SingleTickerProviderStateMixin<TabsFabDemo> {
        GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();

        TabController _controller;
        _Page _selectedPage;
        bool _extendedButtons = false;

        public override void initState() {
            base.initState();
            this._controller = new TabController(vsync: this, length: TabsFabDemoUtils._allPages.Count);
            this._controller.addListener(this._handleTabSelection);
            this._selectedPage = TabsFabDemoUtils._allPages[0];
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        void _handleTabSelection() {
            this.setState(() => { this._selectedPage = TabsFabDemoUtils._allPages[this._controller.index]; });
        }

        void _showExplanatoryText() {
            this._scaffoldKey.currentState.showBottomSheet((BuildContext context) => {
                return new Container(
                    decoration: new BoxDecoration(
                        border: new Border(top: new BorderSide(color: Theme.of(this.context).dividerColor))
                    ),
                    child: new Padding(
                        padding: EdgeInsets.all(32.0f),
                        child: new Text(TabsFabDemoUtils._explanatoryText,
                            style: Theme.of(this.context).textTheme.subhead)
                    )
                );
            });
        }

        Widget buildTabView(_Page page) {
            return new Builder(
                builder: (BuildContext context) => {
                    return new Container(
                        key: new ValueKey<string>(page.label),
                        padding: EdgeInsets.fromLTRB(48.0f, 48.0f, 48.0f, 96.0f),
                        child: new Card(
                            child: new Center(
                                child: new Text(page.label,
                                    style: new TextStyle(
                                        color: page.labelColor,
                                        fontSize: 32.0f
                                    ),
                                    textAlign: TextAlign.center
                                )
                            )
                        )
                    );
                }
            );
        }

        Widget buildFloatingActionButton(_Page page) {
            if (!page.fabDefined) {
                return null;
            }

            if (this._extendedButtons) {
                return FloatingActionButton.extended(
                    key: new ValueKey<Key>(page.fabKey),
                    tooltip: "Show explanation",
                    backgroundColor: page.fabColor,
                    icon: page.fabIcon,
                    label: new Text(page.label.ToUpper()),
                    onPressed: this._showExplanatoryText
                );
            }

            return new FloatingActionButton(
                key: page.fabKey,
                tooltip: "Show explanation",
                backgroundColor: page.fabColor,
                child: page.fabIcon,
                onPressed: this._showExplanatoryText
            );
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                key: this._scaffoldKey,
                appBar: new AppBar(
                    title: new Text("FAB per tab"),
                    bottom: new TabBar(
                        controller: this._controller,
                        tabs: TabsFabDemoUtils._allPages
                            .Select<_Page, Widget>((_Page page) => new Tab(text: page.label.ToUpper())).ToList()
                    ),
                    actions: new List<Widget> {
                        new MaterialDemoDocumentationButton(TabsFabDemo.routeName),
                        new IconButton(
                            icon: new Icon(Icons.sentiment_very_satisfied),
                            onPressed: () => {
                                this.setState(() => { this._extendedButtons = !this._extendedButtons; });
                            }
                        )
                    }
                ),
                floatingActionButton: this.buildFloatingActionButton(this._selectedPage),
                body: new TabBarView(
                    controller: this._controller,
                    children: TabsFabDemoUtils._allPages.Select<_Page, Widget>(this.buildTabView).ToList()
                )
            );
        }
    }
}