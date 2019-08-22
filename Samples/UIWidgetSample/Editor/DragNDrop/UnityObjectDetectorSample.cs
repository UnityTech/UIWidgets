using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsSample.DragNDrop {
    public class UnityObjectDetectorSample : UIWidgetsEditorWindow {
        [MenuItem("UIWidgetsTests/Drag&Drop/UnityObject Detector")]
        public static void ShowEditorWindow() {
            var window = GetWindow<UnityObjectDetectorSample>();
            window.titleContent.text = "UnityObject Detector Sample";
        }

        protected override Widget createWidget() {
            Debug.Log("[ WIDGET RECREATED ]");
            return new WidgetsApp(
                home: new UnityObjectDetectorSampleWidget(),
                pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (BuildContext context, Animation<float> animation,
                            Animation<float> secondaryAnimation) => builder(context)
                    )
            );
        }
    }

    public class UnityObjectDetectorSampleWidget : StatefulWidget {
        public UnityObjectDetectorSampleWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new UnityObjectDetectorSampleWidgetState();
        }
    }

    public class UnityObjectDetectorSampleWidgetState : State<UnityObjectDetectorSampleWidget> {
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
            widgetList.Add(this.getGapBox("Generated List with UnityObjectDetector"));

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

        bool highlight;
        Object[] objRef;

        public override Widget build(BuildContext context) {
            var columnList = new List<Widget>();

            columnList.Add(this.getGapBox());
            columnList.AddRange(this.getUnityObjectDetectorList(3));
            columnList.AddRange(
                new List<Widget> {
                    this.getGapBox("With Listener"),
                    new Container(
                        decoration: this.highlight
                            ? new BoxDecoration(color: this.highlightColor)
                            : new BoxDecoration(color: this.defaultColor),
                        height: 100f,
                        child: new Listener(
                            onPointerDragFromEditorEnter: (evt) => { this.setState(() => { this.highlight = true; }); },
                            onPointerDragFromEditorExit: (evt) => { this.setState(() => { this.highlight = false; }); },
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

            return new Container(
                padding: EdgeInsets.symmetric(horizontal: 25f),
                color: Colors.grey,
                child: new ListView(
                    children: columnList
                ));
        }
    }
}