using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.Tests.demo_charts;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace UIWidgets.Tests {
    public class Widgets : EditorWindow {
        WindowAdapter windowAdapter;

        readonly Func<Widget>[] _options;

        readonly string[] _optionStrings;

        int _selected;

        string localImagePath;

        [NonSerialized] bool hasInvoked = false;

        public Widgets() {
            this.wantsMouseEnterLeaveWindow = true;
            this.wantsMouseMove = true;
            this._options = new Func<Widget>[] {
                this.localImage,
                this.container,
                this.flexRow,
                this.flexColumn,
                this.containerSimple,
                this.eventsPage,
                this.asPage,
                this.stack,
                this.mouseHover,
                this.charts
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("Widgets Test");
        }

        void OnGUI() {
            var selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);

            // if local image test
            if (selected == 0) {
                this.localImagePath = EditorGUILayout.TextField(this.localImagePath);

                if (this._selected != selected) {
                    this._selected = selected;
                    this._attachRootWidget(new Container());
                }

                if (GUILayout.Button("loadLocal")) {
                    var rootWidget = this._options[this._selected]();
                    this._attachRootWidget(rootWidget);
                }

                if (GUILayout.Button("loadAsset")) {
                    var rootWidget = this.loadAsset();
                    this._attachRootWidget(rootWidget);
                }

                if (GUILayout.Button("UnloadUnusedAssets")) {
                    Resources.UnloadUnusedAssets();
                }
            }
            else if (selected != this._selected || !this.hasInvoked) {
                this._selected = selected;
                this.hasInvoked = true;

                var rootWidget = this._options[this._selected]();

                this._attachRootWidget(rootWidget);
            }

            this.windowAdapter.OnGUI();
        }

        void _attachRootWidget(Widget widget) {
            this.windowAdapter.attachRootWidget(() => new WidgetsApp(
                home: widget,
                navigatorKey: GlobalKey<NavigatorState>.key(),
                pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (BuildContext context, Animation<float> animation,
                            Animation<float> secondaryAnimation) => builder(context)
                    )));
        }

        void Update() {
            this.windowAdapter.Update();
        }

        void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>("fonts/MaterialIcons-Regular"), "Material Icons");

            this.windowAdapter = new EditorWindowAdapter(this);
            this.windowAdapter.OnEnable();
        }

        void OnDisable() {
            this.windowAdapter.OnDisable();
            this.windowAdapter = null;
        }

        Widget stack() {
            var image = new Container(
                width: 150,
                height: 150,
                child: Image.network(
                    "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                    width: 100,
                    height: 100
                )
            );
            var text = new Container(
                width: 150,
                height: 150,
                child: new Text("TTTTTTTTTTTTTTTTEST")
            );
            List<Widget> rowImages = new List<Widget>();
            rowImages.Add(image);
            rowImages.Add(text);
            return new Stack(
                children: rowImages,
                alignment: Alignment.center
            );
        }

        Widget localImage() {
            var image = Image.file(this.localImagePath,
                filterMode: FilterMode.Bilinear
            );

            return image;
        }

        Widget loadAsset() {
            var image = Image.asset(this.localImagePath,
                filterMode: FilterMode.Bilinear
            );

            return image;
        }

        Widget flexRow() {
            var image = Image.network(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100
            );
            List<Widget> rowImages = new List<Widget>();
            rowImages.Add(image);
            rowImages.Add(image);
            rowImages.Add(image);
            rowImages.Add(image);

            var row = new Row(
                textDirection: null,
                textBaseline: null,
                key: null,
                mainAxisAlignment: MainAxisAlignment.start,
                mainAxisSize: MainAxisSize.max,
                crossAxisAlignment: CrossAxisAlignment.center,
                verticalDirection: VerticalDirection.down,
                children: rowImages
            );

            return row;
        }

        Widget flexColumn() {
            var image = Image.network(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100
            );
            List<Widget> columnImages = new List<Widget>();
            columnImages.Add(image);
            columnImages.Add(image);
            columnImages.Add(image);

            var column = new Column(
                textDirection: null,
                textBaseline: null,
                key: null,
                mainAxisAlignment: MainAxisAlignment.start,
                mainAxisSize: MainAxisSize.max,
                crossAxisAlignment: CrossAxisAlignment.center,
                verticalDirection: VerticalDirection.down,
                children: columnImages
            );

            return column;
        }

        Widget container() {
            var image = Image.network(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100,
                repeat: ImageRepeat.repeatX
            );
            var container = new Container(
                width: 200,
                height: 200,
                margin: EdgeInsets.all(30.0f),
                padding: EdgeInsets.all(15.0f),
                child: image,
                decoration: new BoxDecoration(
                    color: CLColors.white,
                    borderRadius: BorderRadius.all(30),
                    gradient: new LinearGradient(colors: new List<Color> {CLColors.blue, CLColors.red, CLColors.green})
                )
            );

            return container;
        }

        Widget containerSimple() {
            var container = new Container(
                alignment: Alignment.centerRight,
                color: Color.fromARGB(255, 244, 190, 85),
                child: new Container(
                    width: 120,
                    height: 120,
                    color: Color.fromARGB(255, 255, 0, 85)
                )
            );

            return container;
        }

        Widget eventsPage() {
            return new EventsWaterfallScreen();
        }

        Widget asPage() {
            return new AsScreen();
        }

        Widget mouseHover() {
            return new MouseHoverWidget();
        }
        
        Widget charts() {
            return new ChartPage();
        }
    }


    public class AsScreen : StatefulWidget {
        public AsScreen(Key key = null) : base(key) {
        }

        public override State createState() {
            return new _AsScreenState();
        }
    }

    class _AsScreenState : State<AsScreen> {
        const float headerHeight = 50.0f;

        Widget _buildHeader(BuildContext context) {
            var container = new Container(
                padding: EdgeInsets.only(left: 16.0f, right: 8.0f),
                height: headerHeight,
                color: CLColors.header,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: new List<Widget> {
                        new Container(
                            child: new Text(
                                "All Assets",
                                style: new TextStyle(
                                    fontSize: 16,
                                    color: Color.fromARGB(100, 255, 255, 0)
                                )
                            )
                        ),
                        new CustomButton(
                            padding: EdgeInsets.only(0.0f, 0.0f, 16.0f, 0.0f),
                            child: new Icon(
                                Icons.keyboard_arrow_down,
                                size: 18.0f,
                                color: CLColors.icon2
                            )
                        ),
                        new Container(
                            decoration: new BoxDecoration(
                                color: CLColors.white,
                                borderRadius: BorderRadius.all(3)
                            ),
                            width: 320,
                            height: 36,
                            padding: EdgeInsets.all(10.0f),
                            margin: EdgeInsets.only(right: 4),
                            child: new EditableText(
                                maxLines: 1,
                                selectionControls: MaterialUtils.materialTextSelectionControls,
                                controller: new TextEditingController("Type here to search assets"),
                                focusNode: new FocusNode(),
                                style: new TextStyle(
                                    fontSize: 16
                                ),
                                selectionColor: Color.fromARGB(255, 255, 0, 0),
                                cursorColor: Color.fromARGB(255, 0, 0, 0)
                            )
                        ),
                        new Container(
                            decoration: new BoxDecoration(
                                color: CLColors.background4,
                                borderRadius: BorderRadius.all(2)
                            ),
                            width: 36,
                            height: 36,
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: new List<Widget> {
                                    new CustomButton(
                                        padding: EdgeInsets.only(8.0f, 0.0f, 8.0f, 0.0f),
                                        child: new Icon(
                                            Icons.search,
                                            size: 18.0f,
                                            color: CLColors.white
                                        )
                                    )
                                }
                            )
                        ),
                        new Container(
                            margin: EdgeInsets.only(left: 16, right: 16),
                            child: new Text(
                                "Learn Game Development",
                                style: new TextStyle(
                                    fontSize: 12,
                                    color: CLColors.white
                                )
                            )
                        ),
                        new Container(
                            decoration: new BoxDecoration(
                                border: Border.all(
                                    color: CLColors.white
                                )
                            ),
                            margin: EdgeInsets.only(right: 16),
                            padding: EdgeInsets.all(4),
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: new List<Widget> {
                                    new Text(
                                        "Plus/Pro",
                                        style: new TextStyle(
                                            fontSize: 11,
                                            color: CLColors.white
                                        )
                                    )
                                }
                            )
                        ),
                        new Container(
                            margin: EdgeInsets.only(right: 16),
                            child: new Text(
                                "Impressive New Assets",
                                style: new TextStyle(
                                    fontSize: 12,
                                    color: CLColors.white
                                )
                            )
                        ),
                        new Container(
                            child: new Text(
                                "Shop On Old Store",
                                style: new TextStyle(
                                    fontSize: 12,
                                    color: CLColors.white
                                )
                            )
                        ),
                    }
                )
            );

            return container;
        }

        Widget _buildFooter(BuildContext context) {
            return new Container(
                color: CLColors.header,
                margin: EdgeInsets.only(top: 50),
                height: 90,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: new List<Widget> {
                        new Container(
                            margin: EdgeInsets.only(right: 10),
                            child: new Text(
                                "Copyright © 2018 Unity Technologies",
                                style: new TextStyle(
                                    fontSize: 12,
                                    color: CLColors.text9
                                )
                            )
                        ),
                        new Container(
                            margin: EdgeInsets.only(right: 10),
                            child: new Text(
                                "All prices are exclusive of tax",
                                style: new TextStyle(
                                    fontSize: 12,
                                    color: CLColors.text9
                                )
                            )
                        ),
                        new Container(
                            margin: EdgeInsets.only(right: 10),
                            child: new Text(
                                "Terms of Service and EULA",
                                style: new TextStyle(
                                    fontSize: 12,
                                    color: CLColors.text10
                                )
                            )
                        ),
                        new Container(
                            child: new Text(
                                "Cookies",
                                style: new TextStyle(
                                    fontSize: 12,
                                    color: CLColors.text10
                                )
                            )
                        ),
                    }
                )
            );
        }

        Widget _buildBanner(BuildContext context) {
            return new Container(
                height: 450,
                color: CLColors.white,
                child: Image.network(
                    "https://assetstorev1-prd-cdn.unity3d.com/banner/9716cc07-748c-43cc-8809-10113119c97a.jpg",
                    fit: BoxFit.cover,
                    filterMode: FilterMode.Bilinear
                )
            );
        }

        Widget _buildTopAssetsRow(BuildContext context, string title) {
            var testCard = new AssetCard(
                "AI Template",
                "INVECTOR",
                45.0f,
                36.0f,
                true,
                "https://assetstorev1-prd-cdn.unity3d.com/key-image/76a549ae-de17-4536-bd96-4231ed20dece.jpg"
            );
            return new Container(
                margin: EdgeInsets.only(left: 98),
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            child: new Container(
                                margin: EdgeInsets.only(top: 50, bottom: 20),
                                child: new Row(
                                    crossAxisAlignment: CrossAxisAlignment.baseline,
                                    children: new List<Widget> {
                                        new Container(
                                            child: new Text(
                                                title,
                                                style: new TextStyle(
                                                    fontSize: 24,
                                                    color: CLColors.black
                                                )
                                            )
                                        ),
                                        new Container(
                                            margin: EdgeInsets.only(left: 15),
                                            child:
                                            new Text(
                                                "See More",
                                                style: new TextStyle(
                                                    fontSize: 16,
                                                    color: CLColors.text4
                                                )
                                            )
                                        )
                                    })
                            )
                        ),
                        new Row(
                            children: new List<Widget> {
                                testCard,
                                testCard,
                                testCard,
                                testCard,
                                testCard,
                                testCard
                            }
                        )
                    }
                ));
        }

        bool _onNotification(ScrollNotification notification, BuildContext context) {
            return true;
        }

        Widget _buildContentList(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                    this._onNotification(notification, context);
                    return true;
                },
                child: new Flexible(
                    child: new ListView(
                        physics: new AlwaysScrollableScrollPhysics(),
                        children: new List<Widget> {
                            this._buildBanner(context),
                            this._buildTopAssetsRow(context, "Recommanded For You"),
                            this._buildTopAssetsRow(context, "Beach Day"),
                            this._buildTopAssetsRow(context, "Top Free Packages"),
                            this._buildTopAssetsRow(context, "Top Paid Packages"),
                            this._buildFooter(context)
                        }
                    )
                )
            );
        }

        public override Widget build(BuildContext context) {
            var mediaQueryData = MediaQuery.of(context);
            var px = mediaQueryData.size.width / 2;
            var py = mediaQueryData.size.width / 2;

            var container = new Container(
                color: CLColors.background3,
                child: new Container(
                    color: CLColors.background3,
                    child: new Transform(
                        transform: Matrix3.makeRotate(Mathf.PI / 180 * 5, px, py),
                        child:
                        new Column(
                            children: new List<Widget> {
                                this._buildHeader(context),
                                this._buildContentList(context),
                            }
                        )
                    )
                )
            );

            var stack = new Stack(
                children: new List<Widget> {
                    container,
                    new Positioned(
                        top: 50,
                        right: 50,
                        child: new BackdropFilter(
                            filter: ImageFilter.blur(10, 10),
                            child: new Container(
                                width: 300, height: 300,
                                decoration: new BoxDecoration(
                                    color: Colors.transparent
                                )
                            )
                        )
                    )
                }
            );

            return stack;
        }
    }

    public class AssetCard : StatelessWidget {
        public AssetCard(
            string name,
            string category,
            float price,
            float priceDiscount,
            bool showBadge,
            string imageSrc
        ) {
            this.name = name;
            this.category = category;
            this.price = price;
            this.priceDiscount = priceDiscount;
            this.showBadge = showBadge;
            this.imageSrc = imageSrc;
        }

        public readonly string name;
        public readonly string category;
        public readonly float price;
        public readonly float priceDiscount;
        public readonly bool showBadge;
        public readonly string imageSrc;

        public override Widget build(BuildContext context) {
            var card = new Container(
                margin: EdgeInsets.only(right: 45),
                child: new Container(
                    child: new Column(
                        children: new List<Widget> {
                            new Container(
                                decoration: new BoxDecoration(
                                    color: CLColors.white,
                                    borderRadius: BorderRadius.only(topLeft: 3, topRight: 3)
                                ),
                                width: 200,
                                height: 124,
                                child: Image.network(
                                    this.imageSrc,
                                    fit: BoxFit.fill
                                )
                            ),
                            new Container(
                                color: CLColors.white,
                                width: 200,
                                height: 86,
                                padding: EdgeInsets.fromLTRB(14, 12, 14, 8),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.baseline,
                                    children: new List<Widget> {
                                        new Container(
                                            height: 18,
                                            padding: EdgeInsets.only(top: 3),
                                            child:
                                            new Text(this.category,
                                                style: new TextStyle(
                                                    fontSize: 11,
                                                    color: CLColors.text5
                                                )
                                            )
                                        ),
                                        new Container(
                                            height: 20,
                                            padding: EdgeInsets.only(top: 2),
                                            child:
                                            new Text(this.name,
                                                style: new TextStyle(
                                                    fontSize: 14,
                                                    color: CLColors.text6
                                                )
                                            )
                                        ),
                                        new Container(
                                            height: 22,
                                            padding: EdgeInsets.only(top: 4),
                                            child: new Row(
                                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                children: new List<Widget> {
                                                    new Container(
                                                        child: new Row(
                                                            children: new List<Widget> {
                                                                new Container(
                                                                    margin: EdgeInsets.only(right: 10),
                                                                    child: new Text(
                                                                        "$" + this.price,
                                                                        style: new TextStyle(
                                                                            fontSize: 14,
                                                                            color: CLColors.text7,
                                                                            decoration: TextDecoration.lineThrough
                                                                        )
                                                                    )
                                                                ),
                                                                new Container(
                                                                    child: new Text(
                                                                        "$" + this.priceDiscount,
                                                                        style: new TextStyle(
                                                                            fontSize: 14,
                                                                            color: CLColors.text8
                                                                        )
                                                                    )
                                                                )
                                                            })
                                                    ),
                                                    this.showBadge
                                                        ? new Container(
                                                            width: 80,
                                                            height: 18,
                                                            color: CLColors.black,
                                                            child: new Row(
                                                                mainAxisAlignment: MainAxisAlignment.center,
                                                                crossAxisAlignment: CrossAxisAlignment.center,
                                                                children: new List<Widget> {
                                                                    new Text(
                                                                        "Plus/Pro",
                                                                        style: new TextStyle(
                                                                            fontSize: 11,
                                                                            color: CLColors.white
                                                                        )
                                                                    )
                                                                }
                                                            )
                                                        )
                                                        : new Container()
                                                }
                                            )
                                        )
                                    }
                                )
                            )
                        }
                    )
                )
            );
            return card;
        }
    }

    public class EventsWaterfallScreen : StatefulWidget {
        public EventsWaterfallScreen(Key key = null) : base(key: key) {
        }

        public override State createState() {
            return new _EventsWaterfallScreenState();
        }
    }

    class _EventsWaterfallScreenState : State<EventsWaterfallScreen> {
        const float headerHeight = 80.0f;

        float _offsetY = 0.0f;
#pragma warning disable 0414
        int _index = -1;
#pragma warning restore 0414

        Widget _buildHeader(BuildContext context) {
            return new Container(
                padding: EdgeInsets.only(left: 16.0f, right: 8.0f),
                //  color: CLColors.blue,
                height: headerHeight - this._offsetY,
                child: new Row(
                    children: new List<Widget> {
                        new Flexible(
                            flex: 1,
                            fit: FlexFit.tight,
                            child: new Text(
                                "Today",
                                style: new TextStyle(
                                    fontSize: (34.0f / headerHeight) *
                                              (headerHeight - this._offsetY),
                                    color: CLColors.white
                                )
                            )),
                        new CustomButton(
                            padding: EdgeInsets.only(8.0f, 0.0f, 8.0f, 0.0f),
                            child: new Icon(
                                Icons.notifications,
                                size: 18.0f,
                                color: CLColors.icon2
                            )
                        ),
                        new CustomButton(
                            padding: EdgeInsets.only(8.0f, 0.0f, 16.0f, 0.0f),
                            child: new Icon(
                                Icons.account_circle,
                                size: 18.0f,
                                color: CLColors.icon2
                            )
                        )
                    }
                )
            );
        }

        bool _onNotification(ScrollNotification notification, BuildContext context) {
            float pixels = notification.metrics.pixels;
            if (pixels >= 0.0) {
                if (pixels <= headerHeight) {
                    this.setState(() => { this._offsetY = pixels / 2.0f; });
                }
            }
            else {
                if (this._offsetY != 0.0) {
                    this.setState(() => { this._offsetY = 0.0f; });
                }
            }

            return true;
        }


        Widget _buildContentList(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                    this._onNotification(notification, context);
                    return true;
                },
                child: new Flexible(
                    child: new Container(
                        //  color: CLColors.green,
                        child: ListView.builder(
                            itemCount: 20,
                            itemExtent: 100,
                            physics: new AlwaysScrollableScrollPhysics(),
                            itemBuilder: (BuildContext context1, int index) => {
                                return new Container(
                                    color: Color.fromARGB(255, (index * 10) % 256, (index * 20) % 256,
                                        (index * 30) % 256)
                                );
                            }
                        )
                    )
                )
            );
        }

        public override Widget build(BuildContext context) {
            var container = new Container(
                //  color: CLColors.background1,
                child: new Container(
                    //  color: CLColors.background1,
                    child: new Column(
                        children: new List<Widget> {
                            this._buildHeader(context),
                            this._buildContentList(context)
                        }
                    )
                )
            );
            return container;
        }
    }

    public class CustomButton : StatelessWidget {
        public CustomButton(
            Key key = null,
            GestureTapCallback onPressed = null,
            EdgeInsets padding = null,
            Color backgroundColor = null,
            Widget child = null
        ) : base(key: key) {
            this.onPressed = onPressed;
            this.padding = padding ?? EdgeInsets.all(8.0f);
            this.backgroundColor = backgroundColor ?? CLColors.transparent;
            this.child = child;
        }

        public readonly GestureTapCallback onPressed;
        public readonly EdgeInsets padding;
        public readonly Widget child;
        public readonly Color backgroundColor;

        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onTap: this.onPressed,
                child: new Container(
                    padding: this.padding,
                    color: this.backgroundColor,
                    child: this.child
                )
            );
        }
    }

    public static class Icons {
        public static readonly IconData notifications = new IconData(0xe7f4, fontFamily: "Material Icons");
        public static readonly IconData account_circle = new IconData(0xe853, fontFamily: "Material Icons");
        public static readonly IconData search = new IconData(0xe8b6, fontFamily: "Material Icons");
        public static readonly IconData keyboard_arrow_down = new IconData(0xe313, fontFamily: "Material Icons");
    }

    public static class CLColors {
        public static readonly Color primary = new Color(0xFFE91E63);
        public static readonly Color secondary1 = new Color(0xFF00BCD4);
        public static readonly Color secondary2 = new Color(0xFFF0513C);
        public static readonly Color background1 = new Color(0xFF292929);
        public static readonly Color background2 = new Color(0xFF383838);
        public static readonly Color background3 = new Color(0xFFF5F5F5);
        public static readonly Color background4 = new Color(0xFF00BCD4);
        public static readonly Color icon1 = new Color(0xFFFFFFFF);
        public static readonly Color icon2 = new Color(0xFFA4A4A4);
        public static readonly Color text1 = new Color(0xFFFFFFFF);
        public static readonly Color text2 = new Color(0xFFD8D8D8);
        public static readonly Color text3 = new Color(0xFF959595);
        public static readonly Color text4 = new Color(0xFF002835);
        public static readonly Color text5 = new Color(0xFF9E9E9E);
        public static readonly Color text6 = new Color(0xFF002835);
        public static readonly Color text7 = new Color(0xFF5A5A5B);
        public static readonly Color text8 = new Color(0xFF239988);
        public static readonly Color text9 = new Color(0xFFB3B5B6);
        public static readonly Color text10 = new Color(0xFF00BCD4);
        public static readonly Color dividingLine1 = new Color(0xFF666666);
        public static readonly Color dividingLine2 = new Color(0xFF404040);

        public static readonly Color transparent = new Color(0x00000000);
        public static readonly Color white = new Color(0xFFFFFFFF);
        public static readonly Color black = new Color(0xFF000000);
        public static readonly Color red = new Color(0xFFFF0000);
        public static readonly Color green = new Color(0xFF00FF00);
        public static readonly Color blue = new Color(0xFF0000FF);

        public static readonly Color header = new Color(0xFF060B0C);
    }
}