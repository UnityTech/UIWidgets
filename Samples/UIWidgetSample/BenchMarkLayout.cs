using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsSample {
    public class BenchMarkLayout : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new Material(
                    child: new BenchMarkLayoutWidget()),
                builder: (_, child) => {
                    return new Builder(builder:
                        context => {
                            return new MediaQuery(
                                data: MediaQuery.of(context).copyWith(
                                    textScaleFactor: 1.0f
                                ),
                                child: child);
                        });
                });
        }

        protected override void OnEnable() {
            base.OnEnable();
            FontManager.instance.addFont(Resources.Load<Font>(path: "MaterialIcons-Regular"), "Material Icons");
        }
    }

    class BenchMarkLayoutWidget : StatefulWidget {
        public BenchMarkLayoutWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new BenchMarkLayoutWidgetState();
        }
    }

    class BenchMarkLayoutWidgetState : State<BenchMarkLayoutWidget> {
        int width = 260;
        bool visible = true;

        Widget richtext = new Container(
            child: new RichText(
                text: new TextSpan("", children:
                    new List<TextSpan>() {
                        new TextSpan("Real-time 3D revolutioni\t淡粉色的方式地方\tzes the animation pipeline "),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0)),
                            text: "for Disney Television Animation's\t “Baymax Dreams"),
                        new TextSpan("\t", style: new TextStyle(color: Colors.black)),
                        new TextSpan(" Unity Widgets"),
                        new TextSpan(" Text"),
                        new TextSpan("Real-time 3D revolutionizes the animation pipeline "),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(125, 255, 0, 0)),
                            text: "Transparent Red Text\n\n"),
                        new TextSpan("Bold Text Test Bold Textfs Test: FontWeight.w70\n\n"),
                        new TextSpan(style: new TextStyle(fontStyle: FontStyle.italic),
                            text: "This is FontStyle.italic Text This is FontStyle.italic Text\n\n"),
                        new TextSpan(
                            style: new TextStyle(fontStyle: FontStyle.italic, fontWeight: FontWeight.w700),
                            text:
                            "This is FontStyle.italic And 发撒放豆腐sad 发生的 Bold Text This is FontStyle.italic  And Bold  Text\n\n"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "FontSize 18: Get a named matrix value from the shader.\n\n"),
                        new TextSpan(style: new TextStyle(fontSize: 24),
                            text: "Emoji \ud83d\ude0a\ud83d\ude0b\t\ud83d\ude0d\ud83d\ude0e\ud83d\ude00"),
                        new TextSpan(style: new TextStyle(fontSize: 14),
                            text: "Emoji \ud83d\ude0a\ud83d\ude0b\ud83d\ude0d\ud83d\ude0e\ud83d\ude00 Emoji"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "Emoji \ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "\ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "\ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "\ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "\ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 24),
                            text:
                            "Emoji \ud83d\ude06\ud83d\ude1C\ud83d\ude18\ud83d\ude2D\ud83d\ude0C\ud83d\ude1E\n\n"),
                        new TextSpan(style: new TextStyle(fontSize: 14),
                            text: "FontSize 14"),
                    })
            )
        );

        public override Widget build(BuildContext context) {
            Widget buttons = new Column(
                mainAxisAlignment: MainAxisAlignment.end,
                children: new List<Widget> {
                    new Text($"Width: {this.width}"),
                    new RaisedButton(
                        onPressed: () => { this.setState(() => { this.width += 10; }); },
                        child: new Text("Add Width")
                    ),
                    new Divider(),
                    new RaisedButton(
                        onPressed: () => { this.setState(() => { this.width -= 10; }); },
                        child: new Text("Dec Width")
                    ),
                    new Divider(),
                    new RaisedButton(
                        onPressed: () => { this.setState(() => { this.visible = true; }); },
                        child: new Text("Show")
                    ),
                    new Divider(),
                    new RaisedButton(
                        onPressed: () => { this.setState(() => { this.visible = false; }); },
                        child: new Text("Hide")
                    )
                }
            );
            Widget child = new Column(
                children: new List<Widget> {
                    this.visible ? this.richtext : new Text(""),
                    this.visible
                        ? new Text(
                            "Very Very Very Very Very Very Very Very Very Very Very Very Very Very\nVery Very Very Very Very Very Very Very Very Very Very Long Text",
                            maxLines: 3, overflow: TextOverflow.ellipsis, textAlign: TextAlign.justify
                        )
                        : new Text("")
                });
            child = new Stack(
                children: new List<Widget> {
                    child,
                    buttons
                }
            );
            child = new Container(
                width: this.width,
                color: Colors.black12,
                child: child
            );
            child = new Center(
                child: child
            );
            return child;
        }
    }
}