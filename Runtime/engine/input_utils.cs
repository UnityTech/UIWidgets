using Unity.UIWidgets.ui;
using UnityEngine.EventSystems;

namespace Unity.UIWidgets.engine {
    public static class InputUtils {
        const int mouseScrollId = 1;
        const int preservedPointerKeyNum = 10;

        public static PointerDeviceKind getPointerDeviceKind() {
#if UNITY_IOS || UNITY_ANDROID
            return PointerDeviceKind.touch;
#else
            return PointerDeviceKind.mouse;
#endif
        }

        public static int getPointerDeviceKey(PointerEventData eventData) {
#if UNITY_IOS || UNITY_ANDROID
            return getTouchFingerKey(eventData.pointerId);
#else
            return getMouseButtonKey((int) eventData.button);
#endif
        }

        public static int getScrollButtonKey() {
            return mouseScrollId;
        }

        public static int getMouseButtonKey(int buttonId) {
            return buttonId + preservedPointerKeyNum;
        }

        public static int getTouchFingerKey(int fingerId) {
            return fingerId + preservedPointerKeyNum;
        }
    }
}