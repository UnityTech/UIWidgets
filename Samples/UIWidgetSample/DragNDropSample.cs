using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Transform = UnityEngine.Transform;

namespace UIWidgetsSample {
    public class DragNDropSample : UIWidgetsEditorWindow {
        [MenuItem("UIWidgetsTests/Drag&Drop/DragNDropSample")]
        public static void ShowEditorWindow() {
            var window = GetWindow<DragNDropSample>();
            window.titleContent.text = "DragNDropSample";
        }

        protected override Widget createWidget() {
            Debug.Log("[ WIDGET CREATED ]");
            return new WidgetsApp(
                home: new DragNDropSampleWidget(),
                pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (BuildContext context, Animation<float> animation,
                            Animation<float> secondaryAnimation) => builder(context)
                    )
            );
        }
    }

    public class DragNDropSampleWidget : StatefulWidget {
        public DragNDropSampleWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new DragNDropSampleWidgetState();
        }
    }

    public class DragNDropSampleWidgetState : State<DragNDropSampleWidget> {
        readonly Color highlightColor = Color.fromARGB(255, 88, 127, 219);
        readonly Color defaultColor = Color.fromARGB(255, 211, 211, 211);
        readonly List<bool> isHighlighted = new List<bool> { };
        readonly List<Object[]> objects = new List<Object[]>();

        List<Widget> getUnityObjectDetectorList(int count) {
            if (this.isHighlighted.isEmpty()) {
                for (int i = 0; i < count; i++) {
                    this.isHighlighted.Add(false);
                }
            }

            if (this.objects.isEmpty()) {
                for (int i = 0; i < count; i++) {
                    this.objects.Add(null);
                }
            }

            List<Widget> widgetList = new List<Widget>();
            widgetList.Add(this.getGapBox("Use UnityObjectDetector"));

            for (int i = 0; i < count; i++) {
                var _i = i;

                Widget widget = new Container(
                    decoration: this.isHighlighted[_i]
                        ? new BoxDecoration(color: this.highlightColor)
                        : new BoxDecoration(color: this.defaultColor),
                    height: 100f,
                    child: new UnityObjectDetector(
                        onEnter: () => {
                            Debug.Log("Widget " + _i + " onEnter");
                            this.setState(() => { this.isHighlighted[_i] = true; });
                        },
                        onRelease: (details) => {
                            Debug.Log("Widget " + _i + " onRelease");
                            this.setState(() => {
                                this.isHighlighted[_i] = false;
                                this.objects[_i] = details.objectReferences;
                            });
                        },
                        onExit: () => {
                            Debug.Log("Widget " + _i + " onExit");
                            this.setState(() => { this.isHighlighted[_i] = false; });
                        },
                        child: new Center(
                            child: new Text(this.objects[_i] != null
                                ? this.getNameString(this.objects[_i])
                                : "[Drop/Multi-Drop Here]")
                        )
                    )
                );

                widgetList.Add(widget);
                if (_i != count - 1) {
                    widgetList.Add(this.getGapBox());
                }
            }

            return widgetList;
        }

        string getNameString(Object[] objs) {
            var str = "";
            for (int i = 0; i < objs.Length; i++) {
                str += "[" + objs[i].name + "]";
                if (i != objs.Length - 1) {
                    str += "\n";
                }
            }

            return str;
        }

        Widget getGapBox(string str = "") {
            return new Container(
                height: 25,
                child: str == ""
                    ? null
                    : new Center(
                        child: new Text(str)
                    )
            );
        }

        Widget getSizedTableCell(string text) {
            return new Container(
                height: 20f,
                child: new Center(
                    child: new Text(text)
                )
            );
        }

        bool highlight = false;
        Object[] objRef;

        bool isTransformDisplayHighlighted = false;
        Transform transformRef;

        public override Widget build(BuildContext context) {
            var columnList = new List<Widget>();

            columnList.AddRange(this.getUnityObjectDetectorList(2));
            columnList.AddRange(
                new List<Widget> {
                    this.getGapBox("Use Listener"),
                    new Container(
                        decoration: this.highlight
                            ? new BoxDecoration(color: this.highlightColor)
                            : new BoxDecoration(color: this.defaultColor),
                        height: 100f,
                        child: new Listener(
                            onPointerDragFromEditorEnter: (evt) => {
                                this.setState(() => { this.highlight = true; });
                            },
                            onPointerDragFromEditorExit: (evt) => {
                                this.setState(() => { this.highlight = false; });
                            },
                            // onPointerDragFromEditorHover: (evt) => {  },
                            onPointerDragFromEditorRelease: (evt) => {
                                this.objRef = evt.objectReferences;
                                this.setState(() => { this.highlight = false; });
                            },
                            child: new Center(
                                child: new Text(this.objRef != null
                                    ? this.getNameString(this.objRef)
                                    : "[Drop/Multi-Drop Here]")
                            )
                        )
                    )
                }
            );
            columnList.AddRange(
                new List<Widget>() {
                    this.getGapBox("Sample: Display Transform"),
                    new Container(
                        decoration: this.isTransformDisplayHighlighted
                            ? new BoxDecoration(color: this.highlightColor)
                            : new BoxDecoration(color: this.defaultColor),
                        height: 100f,
                        child: new UnityObjectDetector(
                            onEnter: () => { this.setState(() => { this.isTransformDisplayHighlighted = true; }); },
                            onRelease: (details) => {
                                this.setState(() => {
                                    this.isTransformDisplayHighlighted = false;
                                    var gameObj = details.objectReferences[0] as GameObject;
                                    if (gameObj) {
                                        this.transformRef = gameObj.GetComponent<Transform>();
                                    }
                                    else {
                                        Debug.Log("Not a GameObject");
                                    }
                                });
                            },
                            onExit: () => {
                                Debug.Log("onExit");
                                this.setState(() => { this.isTransformDisplayHighlighted = false; });
                            },
                            child: new Column(
                                children: new List<Widget> {
                                    new Container(
                                        height: 20,
                                        child: new Center(
                                            child: new Text(this.transformRef == null
                                                ? "[Drop a GameObject from scene here]"
                                                : this.transformRef.name
                                            )
                                        )
                                    ),
                                    new Table(
                                        border: TableBorder.all(),
                                        children: new List<TableRow> {
                                            new TableRow(
                                                children: new List<Widget> {
                                                    this.getSizedTableCell(""),
                                                    this.getSizedTableCell("X"),
                                                    this.getSizedTableCell("Y"),
                                                    this.getSizedTableCell("Z")
                                                }
                                            ),
                                            new TableRow(
                                                children: new List<Widget> {
                                                    this.getSizedTableCell("Position"),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.position.x.ToString()),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.position.y.ToString()),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.position.z.ToString())
                                                }
                                            ),
                                            new TableRow(
                                                children: new List<Widget> {
                                                    this.getSizedTableCell("Rotation"),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.eulerAngles.x.ToString()),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.eulerAngles.y.ToString()),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.eulerAngles.z.ToString())
                                                }
                                            ),
                                            new TableRow(
                                                children: new List<Widget> {
                                                    this.getSizedTableCell("Scale"),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.localScale.x.ToString()),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.localScale.y.ToString()),
                                                    this.getSizedTableCell(this.transformRef == null
                                                        ? ""
                                                        : this.transformRef.localScale.z.ToString())
                                                }
                                            )
                                        }
                                    )
                                }
                            )
                        ))
                }
            );

            return new Container(
                padding: EdgeInsets.symmetric(horizontal: 25f),
                color: Colors.grey,
                child: new ListView(
                    children: columnList
                ));
        }
    }
}