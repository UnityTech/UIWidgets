using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsSample {
    public class ScaleGestureSample : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new ScaleGesturePanel()
            );
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }

    class ScaleGesturePanel : StatefulWidget {
        public override State createState() {
            return new ScaleGesturePanelState();
        }
    }

    class ScaleGesturePanelState : State<ScaleGesturePanel> {
        float scaleValue = 1.0f;

        public override Widget build(BuildContext context) {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Center(
                        child: new Text("Test Scale Gesture Widget")
                    )
                ),
                body: new GestureDetector(
                    onScaleStart: scaleDetails => { Debug.Log("Scale Start !"); },
                    onScaleUpdate: scaleDetails => {
                        Debug.Log("Scale value = " + scaleDetails.scale);
                        this.setState(() => { this.scaleValue = scaleDetails.scale; });
                    },
                    onScaleEnd: scaleDetails => { Debug.Log("Scale End"); },
                    child: new Card(
                        color: Colors.white,
                        child: new Center(
                            child: new Column(
                                mainAxisSize: MainAxisSize.min,
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: new List<Widget> {
                                    new Icon(Unity.UIWidgets.material.Icons.ac_unit, size: 128.0f, color: Colors.black),
                                    new RaisedButton(
                                        child: new Text("Scale: " + this.scaleValue),
                                        onPressed: () => { Debug.Log("Button Pressed"); })
                                }
                            )
                        ))
                )
            );
        }
    }
}