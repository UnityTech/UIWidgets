using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class DividerDemo : WidgetCanvas {
        protected override Widget getWidget() {
            return new DemoApp();
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
            bool setTitle = true;
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
                        new Text(title),
                        new Divider(),
                        new Text(subtitle),
                        new Divider(),
                        new Container(
                            width: 500,
                            decoration: new BoxDecoration(border: Border.all(new Color(0xFF00FF00), 1)),
                            child: new EditableText(
                                controller: controller,
                                focusNode: new FocusNode(),
                                style: new TextStyle(
                                        fontSize: 18,
                                        height: 1.5f,
                                        color: new Color(0xFFFF89FD)),
                                cursorColor: Color.fromARGB(255, 0, 0, 0)
                            )
                        ),
                        new Divider(),
                        new Container(
                            width: 100,
                            height: 50,
                            child: new CustomButton(
                                onPressed: () => {
                                setState(() => {
                                        if(setTitle) {
                                            title = controller.text;
                                        } else {
                                            subtitle = controller.text;
                                        }
                                        setTitle = !setTitle;
                                    });
                                },
                                padding: EdgeInsets.all(5.0),
                                child: new Center(
                                    child: new Text(setTitle? "Set Title": "Set Subtitle")
                                )
                            )
                        )
                            }
                        )
                    )
                );
            }
        }

        public class CustomButton : StatelessWidget {
            public CustomButton(
                Key key = null,
                GestureTapCallback onPressed = null,
                EdgeInsets padding = null,
                Color backgroundColor = null,
                Widget child = null
            ) : base(key: key) {
                this.onPressed = onPressed;
                this.padding = padding ?? EdgeInsets.all(8.0);
                this.backgroundColor = backgroundColor ?? new Color(0xFFFF00FD);
                this.child = child;
            }

            public readonly GestureTapCallback onPressed;
            public readonly EdgeInsets padding;
            public readonly Widget child;
            public readonly Color backgroundColor;

            public override Widget build(BuildContext context) {
                return new GestureDetector(
                    onTap: this.onPressed,
                    child: new Container(
                        padding: this.padding,
                        color: this.backgroundColor,
                        child: this.child
                    )
                );
            }
        }
    }
}