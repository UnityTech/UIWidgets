using System.Collections.Generic;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Transform = UnityEngine.Transform;

namespace UIWidgetsSample.DragNDrop {
    public class CustomInspectorSample : UIWidgetsEditorWindow {
        [MenuItem("Window/UIWidgets/Tests/Drag&Drop/Custom Inspector")]
        public static void ShowEditorWindow() {
            var window = GetWindow<CustomInspectorSample>();
            window.titleContent.text = "Custom Inspector Sample";
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>("MaterialIcons-Regular"), "Material Icons");
            FontManager.instance.addFont(Resources.Load<Font>("GalleryIcons"), "GalleryIcons");

            base.OnEnable();
        }

        protected override Widget createWidget() {
            Debug.Log("[ WIDGET RECREATED ]");
            return new MaterialApp(
                home: new CustomInspectorSampleWidget(),
                darkTheme: new ThemeData(primaryColor: Colors.black26)
            );
        }
    }

    public class CustomInspectorSampleWidget : StatefulWidget {
        public CustomInspectorSampleWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new CustomInspectorSampleWidgetState();
        }
    }

    public class CustomInspectorSampleWidgetState : State<CustomInspectorSampleWidget> {
        GameObject objectRef;
        Transform transformRef;

        TextEditingController textController = new TextEditingController();

        public override void initState() {
            this.textController.addListener(() => {
                var text = this.textController.text.ToLower();
                this.textController.value = this.textController.value.copyWith(
                    text: text,
                    selection: new TextSelection(baseOffset: text.Length, extentOffset: text.Length),
                    composing: TextRange.empty
                );
            });
            base.initState();
        }

        enum ETransfrom {
            Position,
            Rotation,
            Scale
        }

        // make custom control of cursor position in TextField.
        int oldCursorPosition = 0;

        // The decimal point input-and-parse exists problem.
        Widget getCardRow(ETransfrom type, bool hasRef) {
            var xValue = hasRef
                ? type == ETransfrom.Position
                    ? this.transformRef.position.x.ToString()
                    : type == ETransfrom.Rotation
                        ? this.transformRef.localEulerAngles.x.ToString()
                        : this.transformRef.localScale.x.ToString()
                : "";
            // Using individual TextEditingController to control TextField cursor position.
            var xValueController = TextEditingController.fromValue(
                new TextEditingValue(xValue, TextSelection.collapsed(this.oldCursorPosition))
            );

            var yValue = hasRef
                ? type == ETransfrom.Position
                    ? this.transformRef.position.y.ToString()
                    : type == ETransfrom.Rotation
                        ? this.transformRef.localEulerAngles.y.ToString()
                        : this.transformRef.localScale.y.ToString()
                : "";

            var yValueController = TextEditingController.fromValue(
                new TextEditingValue(yValue, TextSelection.collapsed(this.oldCursorPosition))
            );

            var zValue = hasRef
                ? type == ETransfrom.Position
                    ? this.transformRef.position.z.ToString()
                    : type == ETransfrom.Rotation
                        ? this.transformRef.localEulerAngles.z.ToString()
                        : this.transformRef.localScale.z.ToString()
                : "";

            var zValueController = TextEditingController.fromValue(
                new TextEditingValue(zValue, TextSelection.collapsed(this.oldCursorPosition))
            );

            return new Column(
                children: new List<Widget> {
                    new Container(
                        padding: EdgeInsets.symmetric(vertical: 8f),
                        child: new Align(
                            alignment: Alignment.centerLeft,
                            child: new Text(
                                type == ETransfrom.Position ? "Position" :
                                type == ETransfrom.Rotation ? "Rotation" : "Scale",
                                style: new TextStyle(fontSize: 16.0f)
                            )
                        )
                    ),
                    new Row(
                        children: new List<Widget> {
                            new Flexible(
                                flex: 8,
                                child: new Container(
                                    decoration: new BoxDecoration(
                                        color: new Color(0xfff5f5f5)),
                                    child: new TextField(
                                        decoration: new InputDecoration(
                                            border: new UnderlineInputBorder(),
                                            contentPadding:
                                            EdgeInsets.symmetric(
                                                horizontal: 10f, vertical: 5f),
                                            labelText: "X"
                                        ),
                                        controller: xValueController,
                                        onChanged: hasRef
                                            ? (str) => {
                                                // While the TextField value changed, try to parse and assign to transformRef.
                                                this.setState(() => {
                                                    float result = 0;
                                                    float.TryParse(str, out result);
                                                    if (str == "" || str[0] == '0') {
                                                        this.oldCursorPosition = 1;
                                                    }
                                                    else {
                                                        this.oldCursorPosition =
                                                            xValueController.selection.startPos.offset;
                                                    }

                                                    switch (type) {
                                                        case ETransfrom.Position:
                                                            var newPos = this.transformRef.position;
                                                            newPos.x = result;
                                                            this.transformRef.position = newPos;
                                                            break;
                                                        case ETransfrom.Rotation:
                                                            var newRot = this.transformRef.localEulerAngles;
                                                            newRot.x = result;
                                                            this.transformRef.localEulerAngles = newRot;
                                                            break;
                                                        case ETransfrom.Scale:
                                                            var newScale = this.transformRef.localScale;
                                                            newScale.x = result;
                                                            this.transformRef.localScale = newScale;
                                                            break;
                                                    }
                                                });
                                            }
                                            : (ValueChanged<string>) null
                                    )
                                )),
                            new Flexible(
                                child: new Container()
                            ),
                            new Flexible(
                                flex: 8,
                                child: new Container(
                                    decoration: new BoxDecoration(
                                        color: new Color(0xfff5f5f5)),
                                    child: new TextField(
                                        decoration: new InputDecoration(
                                            border: new UnderlineInputBorder(),
                                            contentPadding:
                                            EdgeInsets.symmetric(
                                                horizontal: 10f, vertical: 5f),
                                            labelText: "Y"
                                        ),
                                        controller: yValueController,
                                        onChanged: hasRef
                                            ? (str) => {
                                                this.setState(() => {
                                                    float result = 0;
                                                    float.TryParse(str, out result);
                                                    if (str == "" || str[0] == '0') {
                                                        this.oldCursorPosition = 1;
                                                    }
                                                    else {
                                                        this.oldCursorPosition =
                                                            yValueController.selection.startPos.offset;
                                                    }

                                                    switch (type) {
                                                        case ETransfrom.Position:
                                                            var newPos = this.transformRef.position;
                                                            newPos.y = result;
                                                            this.transformRef.position = newPos;
                                                            break;
                                                        case ETransfrom.Rotation:
                                                            var newRot = this.transformRef.localEulerAngles;
                                                            newRot.y = result;
                                                            this.transformRef.localEulerAngles = newRot;
                                                            break;
                                                        case ETransfrom.Scale:
                                                            var newScale = this.transformRef.localScale;
                                                            newScale.y = result;
                                                            this.transformRef.localScale = newScale;
                                                            break;
                                                    }
                                                });
                                            }
                                            : (ValueChanged<string>) null
                                    )
                                )),
                            new Flexible(
                                child: new Container()
                            ),
                            new Flexible(
                                flex: 8,
                                child: new Container(
                                    decoration: new BoxDecoration(
                                        color: new Color(0xfff5f5f5)),
                                    child: new TextField(
                                        decoration: new InputDecoration(
                                            border: new UnderlineInputBorder(),
                                            contentPadding:
                                            EdgeInsets.symmetric(
                                                horizontal: 10f, vertical: 5f),
                                            labelText: "Z"
                                        ),
                                        controller: zValueController,
                                        onChanged: hasRef
                                            ? (str) => {
                                                this.setState(() => {
                                                    float result = 0;
                                                    float.TryParse(str, out result);
                                                    if (str == "" || str[0] == '0') {
                                                        this.oldCursorPosition = 1;
                                                    }
                                                    else {
                                                        this.oldCursorPosition =
                                                            zValueController.selection.startPos.offset;
                                                    }

                                                    switch (type) {
                                                        case ETransfrom.Position:
                                                            var newPos = this.transformRef.position;
                                                            newPos.z = result;
                                                            this.transformRef.position = newPos;
                                                            break;
                                                        case ETransfrom.Rotation:
                                                            var newRot = this.transformRef.localEulerAngles;
                                                            newRot.z = result;
                                                            this.transformRef.localEulerAngles = newRot;
                                                            break;
                                                        case ETransfrom.Scale:
                                                            var newScale = this.transformRef.localScale;
                                                            newScale.z = result;
                                                            this.transformRef.localScale = newScale;
                                                            break;
                                                    }
                                                });
                                            }
                                            : (ValueChanged<string>) null
                                    )
                                ))
                        }
                    )
                }
            );
        }

        public override Widget build(BuildContext context) {
            return new Theme(
                data: new ThemeData(
                    appBarTheme: new AppBarTheme(
                        color: Colors.purple
                    ),
                    cardTheme: new CardTheme(
                        color: Colors.white,
                        elevation: 2.0f
                    )
                ),
                child: new Scaffold(
                    appBar: new AppBar(title: new Text("Custom Inspector")),
                    body: new ListView(
                        children: new List<Widget> {
                            new Card(
                                clipBehavior: Clip.antiAlias,
                                margin: EdgeInsets.all(20.0f),
                                shape: new RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(20.0f)
                                ),
                                child: new Container(
                                    padding: EdgeInsets.symmetric(vertical: 20f, horizontal: 10f),
                                    child: new Column(
                                        mainAxisSize: MainAxisSize.min,
                                        children: new List<Widget> {
                                            new UnityObjectDetector(
                                                // When receiving a GameObject, get its transfrom.
                                                onRelease: (details) => {
                                                    this.setState(() => {
                                                        var gameObj = details.objectReferences[0] as GameObject;
                                                        if (gameObj) {
                                                            this.objectRef = gameObj;
                                                            if (this.objectRef) {
                                                                this.transformRef = this.objectRef.transform;
                                                            }
                                                        }
                                                    });
                                                },
                                                child: new ListTile(
                                                    title: new Text(
                                                        this.objectRef == null ? "Object Name" : this.objectRef.name,
                                                        style: new TextStyle(fontSize: 28.0f)),
                                                    subtitle: new Text("Drag an object here",
                                                        style: new TextStyle(fontSize: 16.0f)),
                                                    contentPadding: EdgeInsets.symmetric(horizontal: 10f)
                                                )
                                            ),
                                            new Card(
                                                clipBehavior: Clip.antiAlias,
                                                shape: new RoundedRectangleBorder(
                                                    borderRadius: BorderRadius.circular(20.0f)
                                                ),
                                                child: new Container(
                                                    padding: EdgeInsets.symmetric(horizontal: 10.0f),
                                                    child: new Column(
                                                        mainAxisSize: MainAxisSize.min,
                                                        children: new List<Widget> {
                                                            new Container(
                                                                padding: EdgeInsets.only(top: 20f),
                                                                child: new Align(
                                                                    alignment: Alignment.centerLeft,
                                                                    child: new Text("Transform",
                                                                        style: new TextStyle(fontSize: 20.0f))
                                                                )
                                                            ),
                                                            this.getCardRow(ETransfrom.Position,
                                                                this.objectRef != null),
                                                            this.getCardRow(ETransfrom.Rotation,
                                                                this.objectRef != null),
                                                            this.getCardRow(ETransfrom.Scale, this.objectRef != null),
                                                            new Container(padding: EdgeInsets.only(bottom: 20f))
                                                        }
                                                    )
                                                )
                                            ),
                                        }
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }
    }
}
