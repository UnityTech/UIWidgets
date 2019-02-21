using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.UIWidgets.engine {

    public class DisplayMetrics {

        static float _devicePixelRatio = 0;
        
        public static float devicePixelRatio {
            get {
                if (_devicePixelRatio > 0) {
                    return _devicePixelRatio;
                }

                if (Application.platform == RuntimePlatform.Android) {
                    _devicePixelRatio = DevicePixelRatioAndroid();
                } else if (Application.platform == RuntimePlatform.WebGLPlayer) {
                    _devicePixelRatio = UIWidgetsWebGLDevicePixelRatio();
                } else if (Application.platform == RuntimePlatform.IPhonePlayer ||
                           Application.platform == RuntimePlatform.tvOS) {
                    _devicePixelRatio = IOSDeviceSaleFactor();
                }

                if (_devicePixelRatio <= 0) {
                    _devicePixelRatio = 1;
                }

                return _devicePixelRatio;
            }
            
        }

        static float DevicePixelRatioAndroid() {
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

        [DllImport("__Internal")]
        static extern float UIWidgetsWebGLDevicePixelRatio();

        [DllImport("__Internal")]
        static extern int IOSDeviceSaleFactor();

    }
    
}