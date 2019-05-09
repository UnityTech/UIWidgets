using System.Collections.Generic;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsSample {
    public class HoverRecognizerSample : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new HoverMainPanel()
                );
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }

    class HoverMainPanel : StatefulWidget {
        public override State createState() {
            return new HoverMainPanelState();
        }
    }

    class HoverMainPanelState : State<HoverMainPanel> {
        bool hoverActivated = false;
        
        public override Widget build(BuildContext context) {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Center(
                        child: new SelectableText("Test Hover Widget")
                    )
                ),
                body: new Card(
                    color: Colors.white,
                    child: new Center(
                        child: new Column(
                            mainAxisSize: MainAxisSize.min,
                            crossAxisAlignment: CrossAxisAlignment.center,
                            children: new List<Widget> {
                                new Icon(this.hoverActivated ? Unity.UIWidgets.material.Icons.pool : Unity.UIWidgets.material.Icons.directions_walk, size: 128.0f),
                                new RichText(
                                    text: new TextSpan(
                                        text: "Test <",
                                        style: new TextStyle(color: Colors.black),
                                        children: new List<TextSpan>() {
                                            new TextSpan(
                                                text: "Hover Me",
                                                style: new TextStyle(
                                                    color: Colors.green,
                                                    decoration: TextDecoration.underline
                                                ),
                                                hoverRecognizer: new HoverRecognizer {
                                                    OnPointerEnter = evt => {
                                                        this.setState(() => { this.hoverActivated = true; });
                                                    },
                                                    OnPointerLeave = () => {
                                                        this.setState(() => { this.hoverActivated = false;});
                                                    }
                                                }
                                            ),
                                            new TextSpan(
                                                text: ">"
                                            )
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