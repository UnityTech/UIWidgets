using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class AsScreenSample : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new WidgetsApp(
                home: new AsScreenWidget(),
                pageRouteBuilder: this.pageRouteBuilder);
        }

        public class AsScreenWidget : StatefulWidget {
            public AsScreenWidget(Key key = null) : base(key) {
            }

            public override State createState() {
                return new _AsScreenState();
            }
        }

        class _AsScreenState : State<AsScreenWidget> {
            const float headerHeight = 50.0f;

            Widget _buildHeader(BuildContext context) {
                return new Container(
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
                                    cursorColor: Color.fromARGB(255, 0, 0, 0),
                                    backgroundCursorColor: Colors.blue
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
                var container = new Container(
                    color: CLColors.background3,
                    child: new Container(
                        color: CLColors.background3,
                        child: new Column(
                            children: new List<Widget> {
                                this._buildHeader(context),
                                this._buildContentList(context),
                            }
                        )
                    )
                );
                return container;
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

            public string name;
            public string category;
            public float price;
            public float priceDiscount;
            public bool showBadge;
            public string imageSrc;

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
    }
}