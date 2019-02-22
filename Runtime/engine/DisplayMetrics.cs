using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.UIWidgets.engine {

    public class DisplayMetrics {

        static float _devicePixelRatio = 0;

        static Func<float> _devicePixelRatioGetter;

        public static void SetDevicePixelRatioGetter(Func<float> f) {
            _devicePixelRatioGetter = f;
        }
        
        public static float devicePixelRatio {
            get {

                if (_devicePixelRatioGetter != null) {
                    return _devicePixelRatioGetter();
                }

                if (_devicePixelRatio > 0) {
                    return _devicePixelRatio;
                }

#if UNITY_ANDROID 
                _devicePixelRatio = AndroidDevicePixelRatio();
#endif
                
#if UNITY_WEBGL 
                _devicePixelRatio = UIWidgetsWebGLDevicePixelRatio();
#endif
                
#if UNITY_IOS 
                _devicePixelRatio = IOSDeviceSaleFactor();
#endif

                if (_devicePixelRatio <= 0) {
                    _devicePixelRatio = 1;
                }

                return _devicePixelRatio;
            }
            
        }

#if UNITY_ANDROID 
        static float AndroidDevicePixelRatio() {
            using (
                AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            ) {
                using (
                    AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
                    activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
                    windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
                    displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
                ) {
                    displayInstance.Call("getMetrics", metricsInstance);
                    return  metricsInstance.Get<float>("density");
                }
            }
        }
#endif

#if UNITY_WEBGL
        [DllImport("__Internal")]
        static extern float UIWidgetsWebGLDevicePixelRatio();
#endif

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern int IOSDeviceSaleFactor();
#endif
        
    }
    
}