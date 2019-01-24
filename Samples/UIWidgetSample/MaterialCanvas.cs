using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsSample {
    public class MaterialCanvas : WidgetCanvas {
        protected override Widget getWidget() {
            return new MaterialApp();
        }

        class MaterialApp : StatefulWidget {
            public MaterialApp(Key key = null) : base(key) {
            }

            public override State createState() {
                return new MaterialWidgetState();
            }
        }

        class MaterialWidget : StatefulWidget {
            public MaterialWidget(Key key = null) : base(key) {
            }

            public override State createState() {
                return new MaterialWidgetState();
            }
        }

        // test-case: material button 
        class MaterialWidgetState : State<MaterialWidget> {
            public override Widget build(BuildContext context) {
                return new Material(
                    child: new Center(
                        child: new MaterialButton(
                            color: Colors.blue,
                            splashColor: new Color(0xFFFF0011),
                            highlightColor: new Color(0x88FF0011),
                            onPressed: () => { Debug.Log("pressed here");}
                        )
                    )
                );
            }
        }

//        // test-case: ink well         
//        class MaterialWidgetState : State<MaterialWidget> {
//            public override Widget build(BuildContext context) {
//                return new Material(
//                    child: new Center(
//                        child: new Container(
//                            width: 30,
//                            height: 30,
//                               child : new InkWell(
//                                borderRadius: BorderRadius.circular(2.0),
//                                highlightColor: new Color(0xAAFF0000),
//                                splashColor: new Color(0xAA0000FF),
//                                //radius : 20,
//                                onTap: () => { }
//                            ))
//                        )
//                );
//            }
//        }
    }
}