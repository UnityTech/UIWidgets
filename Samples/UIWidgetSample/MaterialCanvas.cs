using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;

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
                    child : new Center(
                        child : new Container(
                            width: 30,
                            height : 30,
                        child: new MaterialButton(
                            color: Colors.blue,
                            splashColor: new Color(0xFFFF0011),
                            highlightColor: new Color(0x88000011),
                            onPressed : () => { }))));
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