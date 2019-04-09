using RSG;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.material {
    public class Feedback {
        Feedback() {
        }

        public static IPromise forTap(BuildContext context) {
            switch (_platform(context)) {
                case RuntimePlatform.Android:
                    return
                        Promise.Resolved(); // SystemSound.play(SystemSoundType.click); TODO: replace with unity equivalent
                default:
                    return Promise.Resolved();
            }
        }

        public static GestureTapCallback wrapForTap(GestureTapCallback callback, BuildContext context) {
            if (callback == null) {
                return null;
            }

            return () => {
                forTap(context);
                callback();
            };
        }

        public static IPromise forLongPress(BuildContext context) {
            switch (_platform(context)) {
                case RuntimePlatform.Android:
                    return Promise.Resolved(); // HapticFeedback.vibrate(); TODO
                default:
                    return Promise.Resolved();
            }
        }

        public static GestureLongPressCallback
            wrapForLongPress(GestureLongPressCallback callback, BuildContext context) {
            if (callback == null) {
                return null;
            }

            return () => {
                forLongPress(context);
                callback();
            };
        }

        static RuntimePlatform _platform(BuildContext context) {
            return Theme.of(context).platform;
        }
    }
}