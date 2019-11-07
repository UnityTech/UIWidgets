using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.plugins.raycast;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;

namespace Unity.UIWidgets.Sample {
    public class RaycastSimpleTestbedPanel : UIWidgetsPanel {
        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>("fonts/MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }

        protected override Widget createWidget() {
            return new MaterialApp(
                home: new RaycastSimpleTestbedWidget()
            );
        }
    }

    public class RaycastSimpleTestbedWidget : StatefulWidget {
        public RaycastSimpleTestbedWidget(Key key = null) : base(key) { }

        public override State createState() {
            return new RaycastSimpleTestbedWidgetState();
        }
    }

    public class RaycastSimpleTestbedWidgetState : State<RaycastSimpleTestbedWidget> {
        public override Widget build(BuildContext context) {
            return new Material(
                color: new Color(0x44FFFF00),
                child: new Center(
                    child: new RaycastableContainer(
                        new MaterialButton(
                            child: new Text("Material Button"),
                            onPressed: () => { },
                            color: Colors.lightBlue
                        )
                    )
                )
            );
        }
    }
}