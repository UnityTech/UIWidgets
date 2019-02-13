using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsSample {
    public class MaterialCanvas : WidgetCanvas {
        int testCaseId = 0;

        List<Widget> testCases = new List<Widget> {
            new MaterialButtonWidget(),
            new MaterialInkWellWidget()
        };

        protected override Widget getWidget() {
            return this.testCases[this.testCaseId];
        }
    }

    class MaterialInkWellWidget : StatefulWidget {
        public MaterialInkWellWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialInkWidgetState();
        }
    }

    class MaterialInkWidgetState : State<MaterialInkWellWidget> {
        public override Widget build(BuildContext context) {
            return new Material(
                child: new Center(
                    child: new Container(
                        width: 30,
                        height: 30,
                        child: new InkWell(
                            borderRadius: BorderRadius.circular(2.0),
                            highlightColor: new Color(0xAAFF0000),
                            splashColor: new Color(0xAA0000FF),
                            onTap: () => { Debug.Log("on tap"); }
                        )
                    )
                )
            );
        }
    }

    class MaterialButtonWidget : StatefulWidget {
        public MaterialButtonWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialButtonWidgetState();
        }
    }

    class MaterialButtonWidgetState : State<MaterialButtonWidget> {
        public override Widget build(BuildContext context) {
            return new Material(
                child: new Center(
                    child: new MaterialButton(
                        splashColor: new Color(0xFFFF0011),
                        highlightColor: new Color(0x88FF0011),
                        onPressed: () => { Debug.Log("pressed here"); }
                    )
                )
            );
        }
    }
}