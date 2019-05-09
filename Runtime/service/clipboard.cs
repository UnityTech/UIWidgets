using System.Runtime.InteropServices;
using RSG;
using UnityEngine;

namespace Unity.UIWidgets.service {
    public class ClipboardData {
        public ClipboardData(string text = null) {
            this.text = text;
        }

        public readonly string text;
    }

    public abstract class Clipboard {
        static readonly Clipboard _instance = new UnityGUIClipboard();

        public static readonly string kTextPlain = "text/plain";

        public static IPromise setData(ClipboardData data) {
            return _instance.setClipboardData(data);
        }

        public static IPromise<ClipboardData> getData(string format) {
            return _instance.getClipboardData(format);
        }

        protected abstract IPromise setClipboardData(ClipboardData data);
        protected abstract IPromise<ClipboardData> getClipboardData(string format);
    }

    public class UnityGUIClipboard : Clipboard {
        protected override IPromise setClipboardData(ClipboardData data) {
#if UNITY_WEBGL
            UIWidgetsCopyTextToClipboard(data.text);
#else
            GUIUtility.systemCopyBuffer = data.text;
#endif
            
            return Promise.Resolved();
        }

        protected override IPromise<ClipboardData> getClipboardData(string format) {
            var data = new ClipboardData(text: GUIUtility.systemCopyBuffer);
            return Promise<ClipboardData>.Resolved(data);
        }
        
#if UNITY_WEBGL
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsCopyTextToClipboard(string text);
#endif
    }
}