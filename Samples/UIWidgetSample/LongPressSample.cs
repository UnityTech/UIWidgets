using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsSample {
    public class LongPressSample : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new WidgetsApp(
                home: new LongPressSampleWidget(),
                pageRouteBuilder: this.pageRouteBuilder);
        }
    }

    public class LongPressSampleWidget : StatefulWidget {
        public override State createState() {
            return new _LongPressSampleWidgetState();
        }
    }

    class _LongPressSampleWidgetState : State<LongPressSampleWidget> {
        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onLongPressStart: (value) => { Debug.Log($"Long Press Drag Start: {value}"); },
                onLongPressMoveUpdate: (value) => { Debug.Log($"Long Press Drag Update: {value}"); },
                onLongPressEnd: (value) => { Debug.Log($"Long Press Drag Up: {value}"); },
                onLongPressUp: () => { Debug.Log($"Long Press Up"); },
                onLongPress: () => { Debug.Log($"Long Press"); },
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