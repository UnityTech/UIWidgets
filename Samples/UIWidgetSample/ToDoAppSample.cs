using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Texture = Unity.UIWidgets.widgets.Texture;

namespace UIWidgetsSample {
    public class ToDoAppSample : UIWidgetsSamplePanel {
        public class ToDoListApp : StatefulWidget {
            public ToDoListApp(Key key = null) : base(key) {
            }

            public override State createState() {
                return new _ToDoListAppState();
            }
        }

        protected override Widget createWidget() {
            return new WidgetsApp(
                home: new ToDoListApp(),
                pageRouteBuilder: this.pageRouteBuilder);
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
                this.padding = padding ?? EdgeInsets.all(8.0f);
                this.backgroundColor = backgroundColor ?? CLColors.transparent;
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

        class _ToDoListAppState : State<ToDoListApp> {
            public class ToDoItem {
                public int id;
                public string content;
            }

            List<ToDoItem> items = new List<ToDoItem>();
            int nextId = 0;
            TextEditingController controller = new TextEditingController("");
            FocusNode _focusNode;

            public override void initState() {
                base.initState();
                this._focusNode = new FocusNode();
            }

            public override void dispose() {
                this._focusNode.dispose();
                base.dispose();
            }

            Widget title() {
                return new Text("ToDo App", textAlign: TextAlign.center,
                    style: new TextStyle(fontSize: 30, fontWeight: FontWeight.w700));
            }

            Widget textInput() {
                return new Container(
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget>(
                        ) {
                            new Container(
                                width: 300,
                                decoration: new BoxDecoration(border: Border.all(new Color(0xFF000000), 1)),
                                padding: EdgeInsets.fromLTRB(8, 0, 8, 0),
                                child: new EditableText(maxLines: 1,
                                    controller: this.controller,
                                    onSubmitted: (text) => {
                                        this.controller.clear();
                                        this._addItem(text);
                                    },
                                    selectionControls: MaterialUtils.materialTextSelectionControls,
                                    autofocus: true,
                                    focusNode: new FocusNode(),
                                    style: new TextStyle(
                                        fontSize: 18,
                                        height: 1.5f,
                                        color: new Color(0xFF1389FD)
                                    ),
                                    selectionColor: Color.fromARGB(255, 255, 0, 0),
                                    cursorColor: Color.fromARGB(255, 0, 0, 0))
                            ),

                            new CustomButton(backgroundColor: Color.fromARGB(255, 0, 204, 204),
                                padding: EdgeInsets.all(10),
                                child: new Text("Add", style: new TextStyle(
                                    fontSize: 20, color: Color.fromARGB(255, 255, 255, 255), fontWeight: FontWeight.w700
                                )), onPressed: () => { this._addItem(); })
                        }
                    )
                );
            }

            void _addItem(string text = null) {
                this.setState(() => {
                    text = text ?? this.controller.text;
                    if (text != "") {
                        this.items.Add(new ToDoItem()
                            {id = this.nextId++, content = text});
                    }
                });
            }

            Widget contents() {
                var children = this.items.Select((item) => {
                    return (Widget) new Text(
                        item.content, style: new TextStyle(
                            fontSize: 18,
                            height: 1.5f
                        )
                    );
                });
                return new Flexible(
                    child: new ListView(
                        physics: new AlwaysScrollableScrollPhysics(),
                        children: children.ToList()
                    )
                );
            }

            Widget videoTexture() {
                var texture = Resources.Load<RenderTexture>("VideoSampleRT");
                return new Center(
                    child: new Container(
                        width: 240, height: 135,
                        child: new Texture(texture: texture)
                    )
                );
            }

            public override Widget build(BuildContext context) {
                var container = new Container(
                    padding: EdgeInsets.all(10),
                    decoration: new BoxDecoration(color: new Color(0x7F000000),
                        border: Border.all(color: Color.fromARGB(255, 255, 0, 0), width: 5),
                        borderRadius: BorderRadius.all(2)),
                    child: new Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: new List<Widget> {
                            this.title(),
                            this.textInput(),
                            //      textInput(),
                            this.contents(),
                            this.videoTexture(),
                        }
                    )
                );
                return container;
            }
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
            this.padding = padding ?? EdgeInsets.all(8.0f);
            this.backgroundColor = backgroundColor ?? CLColors.transparent;
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