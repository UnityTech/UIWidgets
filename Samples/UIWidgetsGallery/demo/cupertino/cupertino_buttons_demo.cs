using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class CupertinoButtonsDemo : StatefulWidget {
        public static string routeName = "/cupertino/buttons";

        public override State createState() {
            return new _CupertinoButtonDemoState();
        }
    }

    class _CupertinoButtonDemoState : State<CupertinoButtonsDemo> {
        int _pressedCount = 0;


        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    middle: new Text("Buttons"),
                    previousPageTitle: "Cupertino",
                    trailing: new CupertinoDemoDocumentationButton(CupertinoButtonsDemo.routeName)
                ),
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new SafeArea(
                        child: new Column(
                            children: new List<Widget> {
                                new Padding(
                                    padding: EdgeInsets.all(16.0f),
                                    child: new Text(
                                        "iOS themed buttons are flat. They can have borders or backgrounds but only when necessary."
                                    )
                                ),
                                new Expanded(
                                    child: new Column(
                                        mainAxisAlignment: MainAxisAlignment.center,
                                        children: new List<Widget> {
                                            new Text(this._pressedCount > 0
                                                ? $"Button pressed {this._pressedCount} time" +
                                                  (this._pressedCount == 1 ? "" : "s")
                                                : " "),
                                            new Padding(padding: EdgeInsets.all(12.0f)),
                                            new Align(
                                                alignment: new Alignment(0.0f, -0.2f),
                                                child:
                                                new Row(
                                                    mainAxisSize: MainAxisSize.min,
                                                    children: new List<Widget> {
                                                        new CupertinoButton(
                                                            child: new Text("Cupertino Button"),
                                                            onPressed:
                                                            () => { this.setState(() => { this._pressedCount += 1; }); }
                                                        ),
                                                        new CupertinoButton(
                                                            child: new Text("Disabled"),
                                                            onPressed: null
                                                        )
                                                    }
                                                )
                                            ),
                                            new Padding(padding: EdgeInsets.all(12.0f)),
                                            CupertinoButton.filled(
                                                child: new Text("With Background"),
                                                onPressed:
                                                () => { this.setState(() => { this._pressedCount += 1; }); }
                                            ),
                                            new Padding(padding: EdgeInsets.all(12.0f)),
                                            CupertinoButton.filled(
                                                child: new Text("Disabled"),
                                                onPressed: null
                                            ),
                                        }
                                    )
                                )
                            }
                        )
                    )
                )
            );
        }
    }
}