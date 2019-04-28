using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    public class ModalBottomSheetDemo : StatelessWidget {
        public const string routeName = "/material/modal-bottom-sheet";

        public override Widget build(BuildContext context) {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Modal bottom sheet"),
                    actions: new List<Widget> {new MaterialDemoDocumentationButton(routeName)}
                ),
                body: new Center(
                    child: new RaisedButton(
                        child: new Text("SHOW BOTTOM SHEET"),
                        onPressed: () => {
                            BottomSheetUtils.showModalBottomSheet<object>(context: context,
                                builder: (BuildContext _context) => {
                                    return new Container(
                                        child: new Padding(
                                            padding: EdgeInsets.all(32.0f),
                                            child: new Text("This is the modal bottom sheet. Tap anywhere to dismiss.",
                                                textAlign: TextAlign.center,
                                                style: new TextStyle(
                                                    color: Theme.of(_context).accentColor,
                                                    fontSize: 24.0f
                                                )
                                            )
                                        )
                                    );
                                });
                        }
                    )
                )
            );
        }
    }
}