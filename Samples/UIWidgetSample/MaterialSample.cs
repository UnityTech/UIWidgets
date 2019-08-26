using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class MaterialSample : UIWidgetsSamplePanel {
        const int testCaseId = 6;

        readonly List<Widget> testCases = new List<Widget> {
            new MaterialButtonWidget(),
            new MaterialInkWellWidget(),
            new MaterialAppBarWidget(),
            new MaterialTabBarWidget(),
            new TableWidget(),
            new BottomAppBarWidget(),
            new MaterialSliderWidget(),
            new MaterialNavigationBarWidget(),
            new MaterialReorderableListViewWidget(),
        };

        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: this.testCases[testCaseId]);
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }

    public class BottomAppBarWidget : StatelessWidget {
        public BottomAppBarWidget(Key key = null) : base(key) {

        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                backgroundColor: Color.clear,
                bottomNavigationBar: new BottomAppBar(
                    child: new Row(
                        mainAxisSize: MainAxisSize.max,
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget> {
                            new IconButton(icon: new Icon(Unity.UIWidgets.material.Icons.menu), onPressed: () => { }),
                            new IconButton(icon: new Icon(Unity.UIWidgets.material.Icons.account_balance),
                                onPressed: () => { })
                        })));
        }
    }


    public class TableWidget : StatelessWidget {
        public TableWidget(Key key = null) : base(key) {
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                body: new Table(
                    children: new List<TableRow> {
                        new TableRow(
                            decoration: new BoxDecoration(color: Colors.blue),
                            children: new List<Widget> {
                                new Text("item 1"),
                                new Text("item 2")
                            }
                        ),
                        new TableRow(children: new List<Widget> {
                                new Text("item 3"),
                                new Text("item 4")
                            }
                        )
                    },
                    defaultVerticalAlignment: TableCellVerticalAlignment.middle));
        }
    }


    public class MaterialTabBarWidget : StatefulWidget {
        public MaterialTabBarWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialTabBarWidgetState();
        }
    }

    public class MaterialTabBarWidgetState : SingleTickerProviderStateMixin<MaterialTabBarWidget> {
        TabController _tabController;

        public override void initState() {
            base.initState();
            this._tabController = new TabController(vsync: this, length: Choice.choices.Count);
        }

        public override void dispose() {
            this._tabController.dispose();
            base.dispose();
        }

        void _nextPage(int delta) {
            int newIndex = this._tabController.index + delta;
            if (newIndex < 0 || newIndex >= this._tabController.length) {
                return;
            }

            this._tabController.animateTo(newIndex);
        }

        public override Widget build(BuildContext context) {
            List<Widget> tapChildren = new List<Widget>();
            foreach (Choice choice in Choice.choices) {
                tapChildren.Add(
                    new Padding(
                        padding: EdgeInsets.all(16.0f),
                        child: new ChoiceCard(choice: choice)));
            }

            return new Scaffold(
                appBar: new AppBar(
                    title: new Center(
                        child: new Text("AppBar Bottom Widget")
                    ),
                    leading: new IconButton(
                        tooltip: "Previous choice",
                        icon: new Icon(Unity.UIWidgets.material.Icons.arrow_back),
                        onPressed: () => { this._nextPage(-1); }
                    ),
                    actions: new List<Widget> {
                        new IconButton(
                            icon: new Icon(Unity.UIWidgets.material.Icons.arrow_forward),
                            tooltip: "Next choice",
                            onPressed: () => { this._nextPage(1); })
                    },
                    bottom: new PreferredSize(
                        preferredSize: Size.fromHeight(48.0f),
                        child: new Theme(
                            data: Theme.of(context).copyWith(accentColor: Colors.white),
                            child: new Container(
                                height: 48.0f,
                                alignment: Alignment.center,
                                child: new TabPageSelector(
                                    controller: this._tabController))))
                ),
                body: new TabBarView(
                    controller: this._tabController,
                    children: tapChildren
                ));
        }
    }

    public class MaterialAppBarWidget : StatefulWidget {
        public MaterialAppBarWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialAppBarWidgetState();
        }
    }

    public class MaterialAppBarWidgetState : State<MaterialAppBarWidget> {
        Choice _selectedChoice = Choice.choices[0];

        GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();

        VoidCallback _showBottomSheetCallback;

        public override void initState() {
            base.initState();
            this._showBottomSheetCallback = this._showBottomSheet;
        }

        void _showBottomSheet() {
            this.setState(() => { this._showBottomSheetCallback = null; });

            this._scaffoldKey.currentState.showBottomSheet((BuildContext subContext) => {
                ThemeData themeData = Theme.of(subContext);
                return new Container(
                    decoration: new BoxDecoration(
                        border: new Border(
                            top: new BorderSide(
                                color: themeData.disabledColor))),
                    child: new Padding(
                        padding: EdgeInsets.all(32.0f),
                        child: new Text("This is a Material persistent bottom sheet. Drag downwards to dismiss it.",
                            textAlign: TextAlign.center,
                            style: new TextStyle(
                                color: themeData.accentColor,
                                fontSize: 16.0f))
                    )
                );
            }).closed.Then((object obj) => {
                if (this.mounted) {
                    this.setState(() => { this._showBottomSheetCallback = this._showBottomSheet; });
                }

                return new Promise();
            });
        }

        void _select(Choice choice) {
            this.setState(() => { this._selectedChoice = choice; });
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                key: this._scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Basic AppBar"),
                    actions: new List<Widget> {
                        new IconButton(
                            icon: new Icon(Choice.choices[0].icon),
                            //color: Colors.blue,
                            onPressed: () => { this._select((Choice.choices[0])); }
                        ),
                        new IconButton(
                            icon: new Icon(Choice.choices[1].icon),
                            //color: Colors.blue,
                            onPressed: () => { this._select((Choice.choices[1])); }
                        ),

                        new PopupMenuButton<Choice>(
                            onSelected: this._select,
                            itemBuilder: (BuildContext subContext) => {
                                List<PopupMenuEntry<Choice>> popupItems = new List<PopupMenuEntry<Choice>>();
                                for (int i = 2; i < Choice.choices.Count; i++) {
                                    popupItems.Add(new PopupMenuItem<Choice>(
                                        value: Choice.choices[i],
                                        child: new Text(Choice.choices[i].title)));
                                }

                                return popupItems;
                            }
                        )
                    }
                ),
                body: new Padding(
                    padding: EdgeInsets.all(16.0f),
                    child: new ChoiceCard(choice: this._selectedChoice)
                ),
                floatingActionButton: new FloatingActionButton(
                    backgroundColor: Colors.redAccent,
                    child: new Icon(Unity.UIWidgets.material.Icons.add_alert),
                    onPressed: this._showBottomSheetCallback
                ),
                drawer: new Drawer(
                    child: new ListView(
                        padding: EdgeInsets.zero,
                        children: new List<Widget> {
                            new ListTile(
                                leading: new Icon(Unity.UIWidgets.material.Icons.account_circle),
                                title: new Text("Login"),
                                onTap: () => { }
                            ),
                            new Divider(
                                height: 2.0f),
                            new ListTile(
                                leading: new Icon(Unity.UIWidgets.material.Icons.account_balance_wallet),
                                title: new Text("Wallet"),
                                onTap: () => { }
                            ),
                            new Divider(
                                height: 2.0f),
                            new ListTile(
                                leading: new Icon(Unity.UIWidgets.material.Icons.accessibility),
                                title: new Text("Balance"),
                                onTap: () => { }
                            )
                        }
                    )
                )
            );
        }
    }


    class Choice {
        public Choice(string title, IconData icon) {
            this.title = title;
            this.icon = icon;
        }

        public readonly string title;
        public readonly IconData icon;

        public static List<Choice> choices = new List<Choice> {
            new Choice("Car", Unity.UIWidgets.material.Icons.directions_car),
            new Choice("Bicycle", Unity.UIWidgets.material.Icons.directions_bike),
            new Choice("Boat", Unity.UIWidgets.material.Icons.directions_boat),
            new Choice("Bus", Unity.UIWidgets.material.Icons.directions_bus),
            new Choice("Train", Unity.UIWidgets.material.Icons.directions_railway),
            new Choice("Walk", Unity.UIWidgets.material.Icons.directions_walk)
        };
    }

    class ChoiceCard : StatelessWidget {
        public ChoiceCard(Key key = null, Choice choice = null) : base(key: key) {
            this.choice = choice;
        }

        public readonly Choice choice;

        public override Widget build(BuildContext context) {
            TextStyle textStyle = Theme.of(context).textTheme.display1;
            return new Card(
                color: Colors.white,
                child: new Center(
                    child: new Column(
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget> {
                            new Icon(this.choice.icon, size: 128.0f, color: textStyle.color),
                            new RaisedButton(
                                child: new Text(this.choice.title, style: textStyle),
                                onPressed: () => {
                                    SnackBar snackBar = new SnackBar(
                                        content: new Text(this.choice.title + " is chosen !"),
                                        action: new SnackBarAction(
                                            label: "Ok",
                                            onPressed: () => { }));

                                    Scaffold.of(context).showSnackBar(snackBar);
                                })
                        }
                    )
                )
            );
        }
    }

    public class MaterialInkWellWidget : StatefulWidget {
        public MaterialInkWellWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialInkWidgetState();
        }
    }

    public class MaterialInkWidgetState : State<MaterialInkWellWidget> {
        public override Widget build(BuildContext context) {
            return new Material(
                //color: Colors.blue,
                child: new Center(
                    child: new Container(
                        width: 200,
                        height: 200,
                        child: new InkWell(
                            borderRadius: BorderRadius.circular(2.0f),
                            highlightColor: new Color(0xAAFF0000),
                            splashColor: new Color(0xAA0000FF),
                            onTap: () => { Debug.Log("on tap"); }
                        )
                    )
                )
            );
        }
    }

    public class MaterialButtonWidget : StatefulWidget {
        public MaterialButtonWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialButtonWidgetState();
        }
    }

    public class MaterialButtonWidgetState : State<MaterialButtonWidget> {
        public override Widget build(BuildContext context) {
            return new Stack(
                children: new List<Widget> {
                    new Material(
                        child: new Center(
                            child: new Column(
                                children: new List<Widget> {
                                    new Padding(padding: EdgeInsets.only(top: 30f)),
                                    new MaterialButton(
                                        shape: new RoundedRectangleBorder(borderRadius: BorderRadius.all(20.0f)),
                                        color: new Color(0xFF00FF00),
                                        splashColor: new Color(0xFFFF0011),
                                        highlightColor: new Color(0x88FF0011),
                                        child: new Text("Click Me"),
                                        onPressed: () => { Debug.Log("pressed flat button"); }
                                    ),
                                    new Padding(padding: EdgeInsets.only(top: 30f)),
                                    new MaterialButton(
                                        shape: new RoundedRectangleBorder(borderRadius: BorderRadius.all(20.0f)),
                                        color: new Color(0xFFFF00FF),
                                        splashColor: new Color(0xFFFF0011),
                                        highlightColor: new Color(0x88FF0011),
                                        elevation: 4.0f,
                                        child: new Text("Click Me"),
                                        onPressed: () => { Debug.Log("pressed raised button"); }
                                    )
                                }
                            )
                        )
                    ),
                    new PerformanceOverlay()
                }
            );
        }
    }

    public class MaterialSliderWidget : StatefulWidget {
        public override State createState() {
            return new MaterialSliderState();
        }
    }

    public class MaterialSliderState : State<MaterialSliderWidget> {

        float _value = 0.8f;

        void onChanged(float value) {
            this.setState(() => { this._value = value; });
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Slider and Indicators")),
                body: new Column(
                    children: new List<Widget> {
                        new Padding(
                            padding: EdgeInsets.only(top: 100.0f),
                            child: new Container(
                            child: new Slider(
                                divisions: 10,
                                min: 0.4f,
                                label: "Here",
                                value: this._value,
                                onChanged: this.onChanged))
                            )
                    }
                )
            );
        }
    }

    class MaterialNavigationBarWidget : StatefulWidget {
        public MaterialNavigationBarWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialNavigationBarWidgetState();
        }
    }

    class MaterialNavigationBarWidgetState : SingleTickerProviderStateMixin<MaterialNavigationBarWidget> {
        int _currentIndex = 0;

        public MaterialNavigationBarWidgetState() {
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                bottomNavigationBar: new Container(
                    height: 100,
                    color: Colors.blue,
                    child: new Center(
                        child: new BottomNavigationBar(
                            type: BottomNavigationBarType.shifting,
                            // type: BottomNavigationBarType.fix,
                            items: new List<BottomNavigationBarItem> {
                                new BottomNavigationBarItem(
                                    icon: new Icon(icon: Unity.UIWidgets.material.Icons.work, size: 30),
                                    title: new Text("Work"),
                                    activeIcon: new Icon(icon: Unity.UIWidgets.material.Icons.work, size: 50),
                                    backgroundColor: Colors.blue
                                ),
                                new BottomNavigationBarItem(
                                    icon: new Icon(icon: Unity.UIWidgets.material.Icons.home, size: 30),
                                    title: new Text("Home"),
                                    activeIcon: new Icon(icon: Unity.UIWidgets.material.Icons.home, size: 50),
                                    backgroundColor: Colors.blue
                                ),
                                new BottomNavigationBarItem(
                                    icon: new Icon(icon: Unity.UIWidgets.material.Icons.shop, size: 30),
                                    title: new Text("Shop"),
                                    activeIcon: new Icon(icon: Unity.UIWidgets.material.Icons.shop, size: 50),
                                    backgroundColor: Colors.blue
                                ),
                                new BottomNavigationBarItem(
                                    icon: new Icon(icon: Unity.UIWidgets.material.Icons.school, size: 30),
                                    title: new Text("School"),
                                    activeIcon: new Icon(icon: Unity.UIWidgets.material.Icons.school, size: 50),
                                    backgroundColor: Colors.blue
                                ),
                            },
                            currentIndex: this._currentIndex,
                            onTap: (value) => { this.setState(() => { this._currentIndex = value; }); }
                        )
                    )
                )
            );
        }
    }

    class MaterialReorderableListViewWidget : StatefulWidget {
        public MaterialReorderableListViewWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialReorderableListViewWidgetState();
        }
    }

    class MaterialReorderableListViewWidgetState : State<MaterialReorderableListViewWidget> {
        List<string> items = new List<string> {"First", "Second", "Third"};

        public override Widget build(BuildContext context) {
            return new Stack(
                children: new List<Widget> {
                    new Scaffold(
                        body: new Scrollbar(
                            child: new ReorderableListView(
                                header: new Text("Header of list"),
                                children: this.items.Select<string, Widget>((item) => {
                                    return new Container(
                                        key: Key.key(item),
                                        width: 300.0f,
                                        height: 50.0f,
                                        decoration: new BoxDecoration(
                                            color: Colors.blue,
                                            border: Border.all(
                                                color: Colors.black
                                            )
                                        ),
                                        child: new Center(
                                            child: new Text(
                                                item,
                                                style: new TextStyle(
                                                    fontSize: 32
                                                )
                                            )
                                        )
                                    );
                                }).ToList(),
                                onReorder: (int oldIndex, int newIndex) => {
                                    this.setState(() => {
                                        if (newIndex > oldIndex) {
                                            newIndex -= 1;
                                        }
                                        string item = this.items[oldIndex];
                                        this.items.RemoveAt(oldIndex);
                                        this.items.Insert(newIndex, item);
                                    });
                                }
                            )
                        )
                    ),
                    new PerformanceOverlay()
                }
            );
        }
    }
}