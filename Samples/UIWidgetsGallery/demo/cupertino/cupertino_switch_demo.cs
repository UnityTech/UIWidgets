using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class CupertinoSwitchDemo : StatefulWidget {
        public static string routeName = "/cupertino/switch";

        public override State createState() => new _CupertinoSwitchDemoState();
    }

    class _CupertinoSwitchDemoState : State<CupertinoSwitchDemo> {
        bool _switchValue = false;

        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    middle: new Text("Switch"),
                    previousPageTitle: "Cupertino",
                    trailing: new CupertinoDemoDocumentationButton(CupertinoSwitchDemo.routeName)
                ),
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new SafeArea(
                        child: new Center(
                            child: new Column(
                                mainAxisAlignment: MainAxisAlignment.spaceAround,
                                children: new List<Widget> {
                                    new Column(
                                        children: new List<Widget> {
                                            new CupertinoSwitch(
                                                value: this._switchValue,
                                                onChanged: (bool value) => {
                                                    this.setState(() => { this._switchValue = value; });
                                                }
                                            ),
                                            new Text(
                                                "Enabled - " + (this._switchValue ? "On" : "Off")
                                            ),
                                        }
                                    ),
                                    new Column(
                                        children: new List<Widget> {
                                            new CupertinoSwitch(
                                                value: true,
                                                onChanged: null
                                            ),
                                            new Text(
                                                "Disabled - On"
                                            ),
                                        }
                                    ),
                                    new Column(
                                        children: new List<Widget> {
                                            new CupertinoSwitch(
                                                value: false,
                                                onChanged: null
                                            ),
                                            new Text(
                                                "Disabled - Off"
                                            ),
                                        }
                                    )
                                }
                            )
                        )
                    )
                )
            );
        }
    }
}