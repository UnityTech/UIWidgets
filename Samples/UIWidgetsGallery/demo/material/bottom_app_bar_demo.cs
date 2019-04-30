using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;
using Rect = Unity.UIWidgets.ui.Rect;

namespace UIWidgetsGallery.gallery {
    public class BottomAppBarDemo : StatefulWidget {
        public const string routeName = "/material/bottom_app_bar";

        public override State createState() {
            return new _BottomAppBarDemoState();
        }
    }


    class _BottomAppBarDemoState : State<BottomAppBarDemo> {
        static readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();


        static readonly _ChoiceValue<Widget> kNoFab = new _ChoiceValue<Widget>(
            title: "None",
            label: "do not show a floating action button",
            value: null
        );

        static readonly _ChoiceValue<Widget> kCircularFab = new _ChoiceValue<Widget>(
            title: "Circular",
            label: "circular floating action button",
            value: new FloatingActionButton(
                onPressed: _showSnackbar,
                child: new Icon(Icons.add),
                backgroundColor: Colors.orange
            )
        );

        static readonly _ChoiceValue<Widget> kDiamondFab = new _ChoiceValue<Widget>(
            title: "Diamond",
            label: "diamond shape floating action button",
            value: new _DiamondFab(
                onPressed: _showSnackbar,
                child: new Icon(Icons.add)
            )
        );


        static readonly _ChoiceValue<bool> kShowNotchTrue = new _ChoiceValue<bool>(
            title: "On",
            label: "show bottom appbar notch",
            value: true
        );

        static readonly _ChoiceValue<bool> kShowNotchFalse = new _ChoiceValue<bool>(
            title: "Off",
            label: "do not show bottom appbar notch",
            value: false
        );


        static readonly _ChoiceValue<FloatingActionButtonLocation> kFabEndDocked =
            new _ChoiceValue<FloatingActionButtonLocation>(
                title: "Attached - End",
                label: "floating action button is docked at the end of the bottom app bar",
                value: FloatingActionButtonLocation.endDocked
            );

        static readonly _ChoiceValue<FloatingActionButtonLocation> kFabCenterDocked =
            new _ChoiceValue<FloatingActionButtonLocation>(
                title: "Attached - Center",
                label: "floating action button is docked at the center of the bottom app bar",
                value: FloatingActionButtonLocation.centerDocked
            );

        static readonly _ChoiceValue<FloatingActionButtonLocation> kFabEndFloat =
            new _ChoiceValue<FloatingActionButtonLocation>(
                title: "Free - End",
                label: "floating action button floats above the end of the bottom app bar",
                value: FloatingActionButtonLocation.endFloat
            );

        static readonly _ChoiceValue<FloatingActionButtonLocation> kFabCenterFloat =
            new _ChoiceValue<FloatingActionButtonLocation>(
                title: "Free - Center",
                label: "floating action button is floats above the center of the bottom app bar",
                value: FloatingActionButtonLocation.centerFloat
            );

        static void _showSnackbar() {
            const string text =
                "When the Scaffold's floating action button location changes, " +
                "the floating action button animates to its new position." +
                "The BottomAppBar adapts its shape appropriately.";
            _scaffoldKey.currentState.showSnackBar(
                new SnackBar(content: new Text(text))
            );
        }


        static readonly List<_NamedColor> kBabColors = new List<_NamedColor> {
            new _NamedColor(null, "Clear"),
            new _NamedColor(new Color(0xFFFFC100), "Orange"),
            new _NamedColor(new Color(0xFF91FAFF), "Light Blue"),
            new _NamedColor(new Color(0xFF00D1FF), "Cyan"),
            new _NamedColor(new Color(0xFF00BCFF), "Cerulean"),
            new _NamedColor(new Color(0xFF009BEE), "Blue")
        };

        _ChoiceValue<Widget> _fabShape = kCircularFab;
        _ChoiceValue<bool> _showNotch = kShowNotchTrue;
        _ChoiceValue<FloatingActionButtonLocation> _fabLocation = kFabEndDocked;
        Color _babColor = kBabColors.First().color;

        void _onShowNotchChanged(_ChoiceValue<bool> value) {
            this.setState(() => { this._showNotch = value; });
        }

        void _onFabShapeChanged(_ChoiceValue<Widget> value) {
            this.setState(() => { this._fabShape = value; });
        }

        void _onFabLocationChanged(_ChoiceValue<FloatingActionButtonLocation> value) {
            this.setState(() => { this._fabLocation = value; });
        }

        void _onBabColorChanged(Color value) {
            this.setState(() => { this._babColor = value; });
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                key: _scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Bottom app bar"),
                    elevation: 0.0f,
                    actions: new List<Widget> {
                        new MaterialDemoDocumentationButton(BottomAppBarDemo.routeName),
                        new IconButton(
                            icon: new Icon(Icons.sentiment_very_satisfied),
                            onPressed: () => {
                                this.setState(() => {
                                    this._fabShape = this._fabShape == kCircularFab ? kDiamondFab : kCircularFab;
                                });
                            }
                        )
                    }
                ),
                body: new ListView(
                    padding: EdgeInsets.only(bottom: 88.0f),
                    children: new List<Widget> {
                        new _AppBarDemoHeading("FAB Shape"),

                        new _RadioItem<Widget>(kCircularFab, this._fabShape, this._onFabShapeChanged),
                        new _RadioItem<Widget>(kDiamondFab, this._fabShape, this._onFabShapeChanged),
                        new _RadioItem<Widget>(kNoFab, this._fabShape, this._onFabShapeChanged),

                        new Divider(),
                        new _AppBarDemoHeading("Notch"),

                        new _RadioItem<bool>(kShowNotchTrue, this._showNotch, this._onShowNotchChanged),
                        new _RadioItem<bool>(kShowNotchFalse, this._showNotch, this._onShowNotchChanged),

                        new Divider(),
                        new _AppBarDemoHeading("FAB Position"),

                        new _RadioItem<FloatingActionButtonLocation>(kFabEndDocked, this._fabLocation,
                            this._onFabLocationChanged),
                        new _RadioItem<FloatingActionButtonLocation>(kFabCenterDocked, this._fabLocation,
                            this._onFabLocationChanged),
                        new _RadioItem<FloatingActionButtonLocation>(kFabEndFloat, this._fabLocation,
                            this._onFabLocationChanged),
                        new _RadioItem<FloatingActionButtonLocation>(kFabCenterFloat, this._fabLocation,
                            this._onFabLocationChanged),

                        new Divider(),
                        new _AppBarDemoHeading("App bar color"),

                        new _ColorsItem(kBabColors, this._babColor, this._onBabColorChanged)
                    }
                ),
                floatingActionButton:
                this._fabShape.value,
                floatingActionButtonLocation:
                this._fabLocation.value,
                bottomNavigationBar: new _DemoBottomAppBar(
                    color: this._babColor,
                    fabLocation: this._fabLocation.value,
                    shape: this._selectNotch()
                )
            );
        }

        NotchedShape _selectNotch() {
            if (!this._showNotch.value) {
                return null;
            }

            if (this._fabShape == kCircularFab) {
                return new CircularNotchedRectangle();
            }

            if (this._fabShape == kDiamondFab) {
                return new _DiamondNotchedRectangle();
            }

            return null;
        }
    }

    class _ChoiceValue<T> {
        public _ChoiceValue(T value, string title, string label) {
            this.value = value;
            this.title = title;
            this.label = label;
        }

        public readonly T value;
        public readonly string title;
        string label; // For the Semantics widget that contains title

        public override string ToString() {
            return $"{this.GetType()}('{this.title}')";
        }
    }

    class _RadioItem<T> : StatelessWidget {
        public _RadioItem(_ChoiceValue<T> value, _ChoiceValue<T> groupValue, ValueChanged<_ChoiceValue<T>> onChanged) {
            this.value = value;
            this.groupValue = groupValue;
            this.onChanged = onChanged;
        }

        _ChoiceValue<T> value;
        _ChoiceValue<T> groupValue;
        ValueChanged<_ChoiceValue<T>> onChanged;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            return new Container(
                height: 56.0f,
                padding: EdgeInsets.only(left: 16.0f),
                alignment: Alignment.centerLeft,
                child: new Row(
                    children: new List<Widget> {
                        new Radio<_ChoiceValue<T>>(
                            value: this.value,
                            groupValue: this.groupValue,
                            onChanged: this.onChanged
                        ),
                        new Expanded(
                            child: new GestureDetector(
                                behavior: HitTestBehavior.opaque,
                                onTap: () => { this.onChanged(this.value); },
                                child: new Text(this.value.title,
                                    style: theme.textTheme.subhead
                                )
                            )
                        )
                    }
                )
            );
        }
    }

    class _NamedColor {
        public _NamedColor(Color color, string name) {
            this.color = color;
            this.name = name;
        }

        public readonly Color color;
        public readonly string name;
    }

    class _ColorsItem : StatelessWidget {
        public _ColorsItem(List<_NamedColor> colors, Color selectedColor, ValueChanged<Color> onChanged) {
            this.colors = colors;
            this.selectedColor = selectedColor;
            this.onChanged = onChanged;
        }

        List<_NamedColor> colors;
        public readonly Color selectedColor;
        ValueChanged<Color> onChanged;

        public override Widget build(BuildContext context) {
            return new Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: this.colors.Select<_NamedColor, Widget>((_NamedColor namedColor) => {
                    return new RawMaterialButton(
                        onPressed: () => { this.onChanged(namedColor.color); },
                        constraints: BoxConstraints.tightFor(
                            width: 32.0f,
                            height: 32.0f
                        ),
                        fillColor: namedColor.color,
                        shape: new CircleBorder(
                            side: new BorderSide(
                                color: namedColor.color == this.selectedColor ? Colors.black : new Color(0xFFD5D7DA),
                                width: 2.0f
                            )
                        )
                    );
                }).ToList()
            );
        }
    }

    class _AppBarDemoHeading : StatelessWidget {
        public _AppBarDemoHeading(string text) {
            this.text = text;
        }

        public readonly string text;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            return new Container(
                height: 48.0f,
                padding: EdgeInsets.only(left: 56.0f),
                alignment: Alignment.centerLeft,
                child: new Text(this.text,
                    style: theme.textTheme.body1.copyWith(
                        color: theme.primaryColor
                    )
                )
            );
        }
    }

    class _DemoBottomAppBar : StatelessWidget {
        public _DemoBottomAppBar(
            Color color = null,
            FloatingActionButtonLocation fabLocation = null,
            NotchedShape shape = null
        ) {
            this.color = color;
            this.fabLocation = fabLocation;
            this.shape = shape;
        }

        public readonly Color color;
        public readonly FloatingActionButtonLocation fabLocation;
        public readonly NotchedShape shape;

        static readonly List<FloatingActionButtonLocation> kCenterLocations = new List<FloatingActionButtonLocation> {
            FloatingActionButtonLocation.centerDocked,
            FloatingActionButtonLocation.centerFloat
        };

        public override Widget build(BuildContext context) {
            List<Widget> rowContents = new List<Widget> {
                new IconButton(
                    icon: new Icon(Icons.menu),
                    onPressed: () => {
                        BottomSheetUtils.showModalBottomSheet<object>(
                            context: context,
                            builder: (BuildContext _context) => new _DemoDrawer()
                        );
                    }
                )
            };

            if (kCenterLocations.Contains(this.fabLocation)) {
                rowContents.Add(
                    new Expanded(child: new SizedBox())
                );
            }

            rowContents.AddRange(new List<Widget> {
                new IconButton(
                    icon: new Icon(Icons.search),
                    onPressed: () => {
                        Scaffold.of(context).showSnackBar(
                            new SnackBar(content: new Text("This is a dummy search action."))
                        );
                    }
                ),
                new IconButton(
                    icon: new Icon(
                        Theme.of(context).platform == RuntimePlatform.Android
                            ? Icons.more_vert
                            : Icons.more_horiz
                    ),
                    onPressed: () => {
                        Scaffold.of(context).showSnackBar(
                            new SnackBar(content: new Text("This is a dummy menu action."))
                        );
                    }
                )
            });

            return new BottomAppBar(
                color: this.color,
                child: new Row(children: rowContents),
                shape: this.shape
            );
        }
    }

    class _DemoDrawer : StatelessWidget {
        public _DemoDrawer() {
        }

        public override Widget build(BuildContext context) {
            return new Drawer(
                child: new Column(
                    children: new List<Widget> {
                        new ListTile(
                            leading: new Icon(Icons.search),
                            title: new Text("Search")
                        ),
                        new ListTile(
                            leading: new Icon(Icons.threed_rotation),
                            title: new Text("3D")
                        )
                    }
                )
            );
        }
    }

    class _DiamondFab : StatelessWidget {
        public _DiamondFab(
            Widget child,
            VoidCallback onPressed
        ) {
            this.child = child;
            this.onPressed = onPressed;
        }

        public readonly Widget child;
        public readonly VoidCallback onPressed;

        public override Widget build(BuildContext context) {
            return new Material(
                shape: new _DiamondBorder(),
                color: Colors.orange,
                child: new InkWell(
                    onTap: this.onPressed == null ? (GestureTapCallback) null : () => { this.onPressed(); },
                    child: new Container(
                        width: 56.0f,
                        height: 56.0f,
                        child: IconTheme.merge(
                            data: new IconThemeData(color: Theme.of(context).accentIconTheme.color),
                            child: this.child
                        )
                    )
                ),
                elevation: 6.0f
            );
        }
    }

    class _DiamondNotchedRectangle : NotchedShape {
        public _DiamondNotchedRectangle() {
        }

        public override Path getOuterPath(Rect host, Rect guest) {
            if (guest == null || !host.overlaps(guest)) {
                Path path = new Path();
                path.addRect(host);
                return path;
            }

            D.assert(guest.width > 0.0f);

            Rect intersection = guest.intersect(host);
            float notchToCenter =
                intersection.height * (guest.height / 2.0f)
                / (guest.width / 2.0f);

            Path ret = new Path();
            ret.moveTo(host.left, host.top);
            ret.lineTo(guest.center.dx - notchToCenter, host.top);
            ret.lineTo(guest.left + guest.width / 2.0f, guest.bottom);
            ret.lineTo(guest.center.dx + notchToCenter, host.top);
            ret.lineTo(host.right, host.top);
            ret.lineTo(host.right, host.bottom);
            ret.lineTo(host.left, host.bottom);
            ret.close();
            return ret;
        }
    }

    class _DiamondBorder : ShapeBorder {
        public _DiamondBorder() {
        }

        public override EdgeInsets dimensions {
            get { return EdgeInsets.only(); }
        }

        public override Path getInnerPath(Rect rect) {
            return this.getOuterPath(rect);
        }

        public override Path getOuterPath(Rect rect) {
            Path path = new Path();
            path.moveTo(rect.left + rect.width / 2.0f, rect.top);
            path.lineTo(rect.right, rect.top + rect.height / 2.0f);
            path.lineTo(rect.left + rect.width / 2.0f, rect.bottom);
            path.lineTo(rect.left, rect.top + rect.height / 2.0f);
            path.close();
            return path;
        }

        public override void paint(Canvas canvas, Rect rect) {
        }

        public override ShapeBorder scale(float t) {
            return null;
        }
    }
}