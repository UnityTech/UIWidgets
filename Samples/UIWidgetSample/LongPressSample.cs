using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsSample {
    public class LongPressSample: UIWidgetsSamplePanel {
        
        protected override Widget createWidget()  {
            return new WidgetsApp(
                home: new LongPressSampleWidget(),
                pageRouteBuilder: this.pageRouteBuilder);
        }
    }

    public class LongPressSampleWidget: StatefulWidget {
        public override State createState() {
            return new _LongPressSampleWidgetState();
        }
    }

    class _LongPressSampleWidgetState : State<LongPressSampleWidget> {
        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onLongPressDragStart: (value) => {
                    Debug.Log($"Long Press Drag Start: {value}");
                },
                onLongPressDragUpdate: (value) => {
                    Debug.Log($"Long Press Drag Update: {value}");
                },
                onLongPressDragUp: (value) => {
                    Debug.Log($"Long Press Drag Up: {value}");
                },
                child: new Center(
                    child: new Container(
                        width: 200,
                        height: 200,
                        color: Colors.blue
                    )
                )
            );
        }
    }
}