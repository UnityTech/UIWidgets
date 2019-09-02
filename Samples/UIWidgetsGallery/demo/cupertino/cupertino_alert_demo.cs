using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class CupertinoAlertDemo : StatefulWidget {
        public static string routeName = "/cupertino/alert";

        public override State createState() {
            return new _CupertinoAlertDemoState();
        }
    }

    class _CupertinoAlertDemoState : State<CupertinoAlertDemo> {
        string lastSelectedValue;

        void showDemoDialog(
            BuildContext context = null,
            Widget child = null
        ) {
            CupertinoRouteUtils.showCupertinoDialog(
                context: context,
                builder: (BuildContext _context) => child
            ).Then((object value) => {
                if (value != null) {
                    this.setState(() => { this.lastSelectedValue = value as string; });
                }
            });
        }

        void showDemoActionSheet(
            BuildContext context = null,
            Widget child = null
        ) {
            CupertinoRouteUtils.showCupertinoModalPopup(
                context: context,
                builder: (BuildContext _context) => child
            ).Then((object value) => {
                if (value != null) {
                    this.setState(() => { this.lastSelectedValue = value as string; });
                }
            });
        }

        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    middle: new Text("Alerts"),
                    previousPageTitle: "Cupertino",
                    trailing: new CupertinoDemoDocumentationButton(CupertinoAlertDemo.routeName)
                ),
                child:
                new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new Builder(
                        builder: (BuildContext _context) => {
                            List<Widget> stackChildren = new List<Widget> {
                                new ListView(
                                    padding: EdgeInsets.symmetric(vertical: 24.0f, horizontal: 72.0f)
                                             + MediaQuery.of(_context).padding,
                                    children: new List<Widget> {
                                        CupertinoButton.filled(
                                            child: new Text("Alert"),
                                            onPressed: () => {
                                                this.showDemoDialog(
                                                    context: _context,
                                                    child:
                                                    new CupertinoAlertDialog(
                                                        title: new Text("Discard draft?"),
                                                        actions: new List<Widget> {
                                                            new CupertinoDialogAction(
                                                                child: new Text("Discard"),
                                                                isDestructiveAction: true,
                                                                onPressed: () => { Navigator.pop(_context, "Discard"); }
                                                            ),
                                                            new CupertinoDialogAction(
                                                                child: new Text("Cancel"),
                                                                isDefaultAction: true,
                                                                onPressed: () => { Navigator.pop(_context, "Cancel"); }
                                                            ),
                                                        }
                                                    )
                                                );
                                            }
                                        ),
                                        new Padding(padding: EdgeInsets.all(8.0f)),
                                        CupertinoButton.filled(
                                            child: new Text("Alert with Title"),
                                            padding: EdgeInsets.symmetric(vertical: 16.0f, horizontal: 36.0f),
                                            onPressed: () => {
                                                this.showDemoDialog(
                                                    context: _context,
                                                    child: new CupertinoAlertDialog(
                                                        title: new Text(
                                                            "Allow \"Maps\" to access your location while you are using the app?")
                                                        ,
                                                        content: new Text(
                                                            "Your current location will be displayed on the map and used \n" +
                                                            "for directions, nearby search results, and estimated travel times.")
                                                        ,
                                                        actions: new List<Widget> {
                                                            new CupertinoDialogAction(
                                                                child: new Text("Don\"t Allow"),
                                                                onPressed: () => {
                                                                    Navigator.pop(_context, "Disallow");
                                                                }
                                                            ),
                                                            new CupertinoDialogAction(
                                                                child: new Text("Allow"),
                                                                onPressed: () => { Navigator.pop(_context, "Allow"); }
                                                            ),
                                                        }
                                                    )
                                                );
                                            }
                                        ),
                                        new Padding(padding: EdgeInsets.all(8.0f)),
                                        CupertinoButton.filled(
                                            child: new Text("Alert with Buttons"),
                                            padding: EdgeInsets.symmetric(vertical: 16.0f, horizontal: 36.0f),
                                            onPressed: () => {
                                                this.showDemoDialog(
                                                    context: _context,
                                                    child: new CupertinoDessertDialog(
                                                        title: new Text("Select Favorite Dessert"),
                                                        content: new Text(
                                                            "Please select your favorite type of dessert from the \n" +
                                                            "list below. Your selection will be used to customize the suggested \n" +
                                                            "list of eateries in your area.")
                                                    )
                                                );
                                            }
                                        ),
                                        new Padding(padding: EdgeInsets.all(8.0f)),
                                        CupertinoButton.filled(
                                            child: new Text("Alert Buttons Only"),
                                            padding: EdgeInsets.symmetric(vertical: 16.0f, horizontal: 36.0f),
                                            onPressed: () => {
                                                this.showDemoDialog(
                                                    context: _context,
                                                    child: new CupertinoDessertDialog()
                                                );
                                            }
                                        ),
                                        // TODO: FIX BUG
                                        new Padding(padding: EdgeInsets.all(8.0f)),
                                        CupertinoButton.filled(
                                            child: new Text("Action Sheet"),
                                            padding: EdgeInsets.symmetric(vertical: 16.0f, horizontal: 36.0f),
                                            onPressed: () => {
                                                this.showDemoActionSheet(
                                                    context: _context,
                                                    child: new CupertinoActionSheet(
                                                        title: new Text("Favorite Dessert"),
                                                        message: new Text(
                                                            "Please select the best dessert from the options below."),
                                                        actions: new List<Widget> {
                                                            new CupertinoActionSheetAction(
                                                                child: new Text("Profiteroles"),
                                                                onPressed: () => {
                                                                    Navigator.pop(_context, "Profiteroles");
                                                                }
                                                            ),
                                                            new CupertinoActionSheetAction(
                                                                child: new Text("Cannolis"),
                                                                onPressed: () => {
                                                                    Navigator.pop(_context, "Cannolis");
                                                                }
                                                            ),
                                                            new CupertinoActionSheetAction(
                                                                child: new Text("Trifle"),
                                                                onPressed: () => { Navigator.pop(_context, "Trifle"); }
                                                            )
                                                        },
                                                        cancelButton: new CupertinoActionSheetAction(
                                                            child: new Text("Cancel"),
                                                            isDefaultAction: true,
                                                            onPressed: () => { Navigator.pop(_context, "Cancel"); }
                                                        )
                                                    )
                                                );
                                            }
                                        )
                                    }
                                )
                            };

                            if (this.lastSelectedValue != null) {
                                stackChildren.Add(
                                    new Positioned(
                                        bottom: 32.0f,
                                        child: new Text($"You selected: {this.lastSelectedValue}")
                                    )
                                );
                            }

                            return new Stack(
                                alignment: Alignment.center,
                                children: stackChildren
                            );
                        }
                    )
                )
            );
        }
    }

    class CupertinoDessertDialog : StatelessWidget {
        public CupertinoDessertDialog(
            Key key = null,
            Widget title = null,
            Widget content = null
        ) : base(key: key) {
            this.title = title;
            this.content = content;
        }

        public readonly Widget title;
        public readonly Widget content;

        public override Widget build(BuildContext context) {
            return new CupertinoAlertDialog(
                title: this.title,
                content: this.content,
                actions: new List<Widget> {
                    new CupertinoDialogAction(
                        child: new Text("Cheesecake"),
                        onPressed: () => { Navigator.pop(context, "Cheesecake"); }
                    ),
                    new CupertinoDialogAction(
                        child: new Text("Tiramisu"),
                        onPressed: () => { Navigator.pop(context, "Tiramisu"); }
                    ),
                    new CupertinoDialogAction(
                        child: new Text("Apple Pie"),
                        onPressed:
                        () => { Navigator.pop(context, "Apple Pie"); }
                    ),
                    new CupertinoDialogAction(
                        child: new Text("Devil\"s food cake"),
                        onPressed:
                        () => { Navigator.pop(context, "Devil\"s food cake"); }
                    ),
                    new CupertinoDialogAction(
                        child: new Text("Banana Split"),
                        onPressed:
                        () => { Navigator.pop(context, "Banana Split"); }
                    ),
                    new CupertinoDialogAction(
                        child: new Text("Oatmeal Cookie"),
                        onPressed:
                        () => { Navigator.pop(context, "Oatmeal Cookies"); }
                    ),
                    new CupertinoDialogAction(
                        child: new Text("Chocolate Brownie"),
                        onPressed:
                        () => { Navigator.pop(context, "Chocolate Brownies"); }
                    ),
                    new CupertinoDialogAction(
                        child: new Text("Cancel"),
                        isDestructiveAction:
                        true,
                        onPressed:
                        () => { Navigator.pop(context, "Cancel"); }
                    ),
                }
            );
        }
    }
}