using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class DividerButton : UIWidgetsSamplePanel {
        
        protected override Widget createWidget() {
            return new WidgetsApp(
                home: new DemoApp(),
                pageRouteBuilder: this.pageRouteBuilder);
        }
        
        public class DemoApp : StatefulWidget {
            public DemoApp(Key key = null) : base(key) {
            }

            public override State createState() {
                return new _DemoAppState();
            }
        }

        public class _DemoAppState : State<DemoApp> {
            string title = "Hello";
            string subtitle = "World";
            TextEditingController controller = new TextEditingController("");

            public override Widget build(BuildContext context) {
                return new Container(
                    height: 200,
                    padding: EdgeInsets.all(10),
                    decoration: new BoxDecoration(
                        color: new Color(0xFFEF1F7F),
                        border: Border.all(color: Color.fromARGB(255, 0xDF, 0x10, 0x70), width: 5),
                        borderRadius: BorderRadius.all(20)
                    ),
                    child: new Center(
                        child: new Column(
                            children: new List<Widget>() {
                                new Text(this.title),
                                new Divider(),
                                new Text(this.subtitle),
                                new Divider(),
                                new Container(
                                    width: 500,
                                    decoration: new BoxDecoration(border: Border.all(new Color(0xFF00FF00), 1)),
                                    child: new EditableText(
                                        controller: this.controller,
                                        focusNode: new FocusNode(),
                                        style: new TextStyle(
                                            fontSize: 18,
                                            height: 1.5f,
                                            color: new Color(0xFFFF89FD)),
                                        cursorColor: Color.fromARGB(255, 0, 0, 0)
                                    )
                                ),
                                new Divider(),
                                new ButtonBar(
                                    children: new List<Widget> {
                                        new FlatButton(
                                            onPressed: () => {
                                                this.setState(() => { this.title = this.controller.text; });
                                            },
                                            padding: EdgeInsets.all(5.0f),
                                            child: new Center(
                                                child: new Text("Set Title")
                                            )
                                        ),
                                        new RaisedButton(
                                            onPressed: () => {
                                                this.setState(() => { this.subtitle = this.controller.text; });
                                            },
                                            padding: EdgeInsets.all(5.0f),
                                            child: new Center(
                                                child: new Text("Set Subtitle")
                                            )
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
}