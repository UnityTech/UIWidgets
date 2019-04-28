using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using DialogUtils = Unity.UIWidgets.material.DialogUtils;

namespace UIWidgetsGallery.gallery {
    public class PersistentBottomSheetDemo : StatefulWidget {
        public const string routeName = "/material/persistent-bottom-sheet";

        public override State createState() {
            return new _PersistentBottomSheetDemoState();
        }
    }

    class _PersistentBottomSheetDemoState : State<PersistentBottomSheetDemo> {
        GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();

        VoidCallback _showBottomSheetCallback;

        public override void initState() {
            base.initState();
            this._showBottomSheetCallback = this._showBottomSheet;
        }

        void _showBottomSheet() {
            this.setState(() => {
                // disable the button
                this._showBottomSheetCallback = null;
            });
            this._scaffoldKey.currentState.showBottomSheet((BuildContext context) => {
                    ThemeData themeData = Theme.of(this.context);
                    return new Container(
                        decoration: new BoxDecoration(
                            border: new Border(top: new BorderSide(color: themeData.disabledColor))
                        ),
                        child: new Padding(
                            padding: EdgeInsets.all(32.0f),
                            child: new Text("This is a Material persistent bottom sheet. Drag downwards to dismiss it.",
                                textAlign: TextAlign.center,
                                style: new TextStyle(
                                    color: themeData.accentColor,
                                    fontSize: 24.0f
                                )
                            )
                        )
                    );
                })
                .closed.Then((value) => {
                    if (this.mounted) {
                        this.setState(() => {
                            // re-enable the button
                            this._showBottomSheetCallback = this._showBottomSheet;
                        });
                    }
                });
        }

        void _showMessage() {
            DialogUtils.showDialog(
                context: this.context,
                builder: (BuildContext context) => {
                    return new AlertDialog(
                        content: new Text("You tapped the floating action button."),
                        actions: new List<Widget> {
                            new FlatButton(
                                onPressed: () => { Navigator.pop(context); },
                                child: new Text("OK")
                            )
                        }
                    );
                }
            );
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                key: this._scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Persistent bottom sheet"),
                    actions: new List<Widget> {
                        new MaterialDemoDocumentationButton(PersistentBottomSheetDemo.routeName)
                    }
                ),
                floatingActionButton: new FloatingActionButton(
                    onPressed: this._showMessage,
                    backgroundColor: Colors.redAccent,
                    child: new Icon(
                        Icons.add
                    )
                ),
                body: new Center(
                    child: new RaisedButton(
                        onPressed: this._showBottomSheetCallback,
                        child: new Text("SHOW BOTTOM SHEET")
                    )
                )
            );
        }
    }
}
