using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.plugins.raycast;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.Sample {
    public class RaycastTestbedPanel : UIWidgetsPanel {
        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>("fonts/MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }

        protected override Widget createWidget() {
            return new MaterialApp(
                home: new RaycastTestbedWidget()
            );
        }
    }

    public class RaycastTestbedWidget : StatefulWidget {
        public RaycastTestbedWidget(Key key = null) : base(key) { }

        public override State createState() {
            return new RaycastTestbedWidgetState();
        }
    }

    public class RaycastTestbedWidgetState : State<RaycastTestbedWidget> {
        public bool enableState = false;
        public int switchState = 0;
        public int switchPosState = 0;
        public bool enableState2 = false;
        public int switchState2 = 0;
        public int switchPosState2 = 2;

        public override Widget build(BuildContext context) {
            return new Material(
                color: Colors.transparent,
                child: new Center(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: new List<Widget> {
                            new Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: new List<Widget> {
                                    new RaycastableContainer(child: new MaterialButton(
                                            child: new Text($"Enable State: {this.enableState.ToString()}"),
                                            onPressed: () => {
                                                this.setState(
                                                    () => { this.enableState = !this.enableState; });
                                            },
                                            color: Colors.lightBlue
                                        )
                                    ),
                                    new Padding(padding: EdgeInsets.symmetric(horizontal: 5f)),
                                    new RaycastableContainer(child: new MaterialButton(
                                        child: new Text($"Switch State: {this.switchState.ToString()}"),
                                        onPressed: () => {
                                            this.setState(
                                                () => { this.switchState = (this.switchState + 1) % 3; });
                                        },
                                        color: Colors.lightBlue
                                    )),
                                    new Padding(padding: EdgeInsets.symmetric(horizontal: 5f)),
                                    new RaycastableContainer(child: new MaterialButton(
                                        child: new Text($"Switch Pos State: {this.switchPosState.ToString()}"),
                                        onPressed: () => {
                                            this.setState(
                                                () => { this.switchPosState = (this.switchPosState + 1) % 2; });
                                        },
                                        color: Colors.lightBlue
                                    ))
                                }
                            ),
                            new Padding(padding: EdgeInsets.symmetric(5f)),
                            new Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: new List<Widget> {
                                    new RaycastableContainer(child: new MaterialButton(
                                        child: new Text($"Enable State: {this.enableState2.ToString()}"),
                                        onPressed: () => {
                                            this.setState(
                                                () => { this.enableState2 = !this.enableState2; });
                                        },
                                        color: Colors.lightBlue
                                    )),
                                    new Padding(padding: EdgeInsets.symmetric(horizontal: 5f)),
                                    new RaycastableContainer(child: new MaterialButton(
                                        child: new Text($"Switch State: {this.switchState2.ToString()}"),
                                        onPressed: () => {
                                            this.setState(
                                                () => { this.switchState2 = (this.switchState2 + 1) % 3; });
                                        },
                                        color: Colors.lightBlue
                                    )),
                                    new Padding(padding: EdgeInsets.symmetric(horizontal: 5f)),
                                    new RaycastableContainer(child: new MaterialButton(
                                        child: new Text($"Switch Pos State: {this.switchPosState2.ToString()}"),
                                        onPressed: () => {
                                            this.setState(
                                                () => { this.switchPosState2 = (this.switchPosState2) % 2 + 1; });
                                        },
                                        color: Colors.lightBlue
                                    ))
                                }
                            ),
                            new Padding(padding: EdgeInsets.symmetric(5f)),
                            new Stack(
                                children: new List<Widget> {
                                    new Row(
                                        mainAxisAlignment: MainAxisAlignment.center,
                                        children: new List<Widget> {
                                            new Container(
                                                padding: EdgeInsets.only(top: 25f * this.switchPosState,
                                                    bottom: 25f * (3 - this.switchPosState)),
                                                child: this.enableState
                                                    ? (Widget) new RaycastableContainer(
                                                        new Container(
                                                            child: new Text(
                                                                data: this.switchState == 0
                                                                    ? "特殊字符串"
                                                                    : this.switchState == 1
                                                                        ? "特殊字符串串"
                                                                        : "特殊字符串串串",
                                                                style: new TextStyle(
                                                                    fontSize: 48,
                                                                    fontWeight: FontWeight.bold,
                                                                    decoration: TextDecoration.none,
                                                                    color: Colors.red
                                                                )
                                                            ),
                                                            decoration: new BoxDecoration(
                                                                color: new Color(0x44FFFF00)
                                                            )
                                                        )
                                                    )
                                                    : new Text(
                                                        data: this.switchState == 0
                                                            ? "普通字符串"
                                                            : this.switchState == 1
                                                                ? "普通字符串串"
                                                                : "普通字符串串串",
                                                        style: new TextStyle(
                                                            fontSize: 48,
                                                            fontWeight: FontWeight.bold,
                                                            decoration: TextDecoration.none,
                                                            color: Colors.red
                                                        )
                                                    )
                                            )
                                        }
                                    ),
                                    new Row(
                                        mainAxisAlignment: MainAxisAlignment.center,
                                        children: new List<Widget> {
                                            new Container(
                                                padding: EdgeInsets.only(top: 25f * this.switchPosState2,
                                                    bottom: 25f * (3 - this.switchPosState2)),
                                                child: this.enableState2
                                                    ? (Widget) new RaycastableContainer(
                                                        new Container(
                                                            child: new Text(
                                                                data: this.switchState2 == 0
                                                                    ? "特殊字符串"
                                                                    : this.switchState2 == 1
                                                                        ? "特殊字符串串"
                                                                        : "特殊字符串串串",
                                                                style: new TextStyle(
                                                                    fontSize: 48,
                                                                    fontWeight: FontWeight.bold,
                                                                    decoration: TextDecoration.none,
                                                                    color: Colors.red
                                                                )
                                                            ),
                                                            decoration: new BoxDecoration(
                                                                color: new Color(0x44FFFF00)
                                                            )
                                                        )
                                                    )
                                                    : (Widget) new Text(
                                                        data: this.switchState2 == 0
                                                            ? "普通字符串"
                                                            : this.switchState2 == 1
                                                                ? "普通字符串串"
                                                                : "普通字符串串串",
                                                        style: new TextStyle(
                                                            fontSize: 48,
                                                            fontWeight: FontWeight.bold,
                                                            decoration: TextDecoration.none,
                                                            color: Colors.red
                                                        )
                                                    )
                                            )
                                        }
                                    )
                                }
                            )
                        }
                    )
                )
            );
        }
    }
}