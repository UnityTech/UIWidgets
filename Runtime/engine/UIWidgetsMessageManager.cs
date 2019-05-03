using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    public class UIWidgetsMessageManager: MonoBehaviour {

        public delegate void MethodChannelMessageDelegate(string method, List<JSONNode> args);
        
        static UIWidgetsMessageManager _instance;

        readonly Dictionary<string, MethodChannelMessageDelegate> _methodChannelMessageDelegates = 
            new Dictionary<string, MethodChannelMessageDelegate>();
        public static UIWidgetsMessageManager instance {
            get { return _instance; } 
        }

        internal static void ensureUIWidgetsMessageManagerIfNeeded() {
            if (!Application.isPlaying) {
                return;
            }
            if (_instance != null) {
                return;
            }
            var managerObj = new GameObject("__UIWidgetsMessageManager");
            managerObj.AddComponent<UIWidgetsMessageManager>();
        }
        
#if UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
        string _lastObjectName;
#endif
        
        void OnEnable() {
            D.assert(_instance == null, () => "Only one instance of UIWidgetsMessageManager should exists");
            _instance = this;
            this.UpdateNameIfNeed();
        }

        void OnDisable() {
            D.assert(_instance != null, () => "_instance should not be null");
            _instance = null;
        }

        void Update() {
            this.UpdateNameIfNeed();
        }

        void UpdateNameIfNeed() {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
            var name = this.gameObject.name;
            if (name != this._lastObjectName) {

                if (!Application.isEditor) {
                    UIWidgetsMessageSetObjectName(name);
                }
                this._lastObjectName = name;
            }
#endif
        }

        public void AddChannelMessageDelegate(string channel, MethodChannelMessageDelegate del) {
            MethodChannelMessageDelegate exists;
            this._methodChannelMessageDelegates.TryGetValue(channel, out exists);
            this._methodChannelMessageDelegates[channel] = exists + del;
        }
        
        public void RemoveChannelMessageDelegate(string channel, MethodChannelMessageDelegate del) {
            MethodChannelMessageDelegate exists;
            this._methodChannelMessageDelegates.TryGetValue(channel, out exists);
            if (exists != null) {
                this._methodChannelMessageDelegates[channel] = exists - del;
            }  
        }

        void OnUIWidgetsMethodMessage(string message) {
            JSONObject jsonObject = (JSONObject)JSON.Parse(message);
            string channel = jsonObject["channel"].Value;
            string method = jsonObject["method"].Value;
            var args = new List<JSONNode>(jsonObject["args"].AsArray.Children);
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(method)) {
                Debug.LogError("invalid uiwidgets method message");
            }
            else {
                MethodChannelMessageDelegate exists;
                this._methodChannelMessageDelegates.TryGetValue(channel, out exists);
                exists?.Invoke(method, args);
            }
        }

#if UNITY_IOS || UNITY_WEBGL        
        [DllImport("__Internal")]
        static extern void UIWidgetsMessageSetObjectName(string objectName);
#elif UNITY_ANDROID
        
        static void UIWidgetsMessageSetObjectName(string objectName) {
            using (
                AndroidJavaClass managerClass = new AndroidJavaClass("com.unity.uiwidgets.plugin.UIWidgetsMessageManager")
            ) {
                using (
                    AndroidJavaObject managerInstance = managerClass.CallStatic<AndroidJavaObject>("getInstance")
                ) {
                    managerInstance.Call("SetObjectName", objectName);
                }
            }
        }
#endif
        
    }
}