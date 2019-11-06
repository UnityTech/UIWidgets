using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine.EventSystems;

namespace Unity.UIWidgets.engine {
    public static class InputUtils {
        const int mouseScrollId = 1;
        const int preservedKeyNum = 10;
        const int preservedMouseKeyNum = 100;
        const int fingerKeyStart = preservedKeyNum + preservedMouseKeyNum;

        public const int MouseLeftKeyDevice = preservedKeyNum;
        public const int MouseRightKeyDevice = preservedKeyNum + 1;

        public static PointerDeviceKind getPointerDeviceKind(PointerEventData eventData) {
            return isTouchEvent(eventData) ? PointerDeviceKind.touch : PointerDeviceKind.mouse;
        }

        public static int getPointerDeviceKey(PointerEventData eventData) {
            return isTouchEvent(eventData)
                ? getTouchFingerKey(eventData.pointerId)
                : getMouseButtonKey((int) eventData.button);
        }

        public static int getScrollButtonKey() {
            return mouseScrollId;
        }

        public static int getMouseButtonKey(int buttonId) {
            D.assert(buttonId < preservedMouseKeyNum);
            return buttonId + preservedKeyNum;
        }

        static int getTouchFingerKey(int fingerId) {
            return fingerId + fingerKeyStart;
        }

        static bool isTouchEvent(PointerEventData eventData) {
            //pointerId >= 0 : touches
            //ref: https://docs.unity3d.com/ScriptReference/EventSystems.PointerEventData-pointerId.html
            return eventData.pointerId >= 0;
        }

        static bool isMouseEvent(PointerEventData eventData) {
            //pointerId = -1, -2, -3 : mouse buttons
            //ref: https://docs.unity3d.com/ScriptReference/EventSystems.PointerEventData-pointerId.html
            return eventData.pointerId < 0;
        }
    }
}