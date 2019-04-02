using System;
using System.Runtime.InteropServices;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.engine {

    [StructLayout(LayoutKind.Sequential)]
    public struct ViewPadding {
        public float top;
        public float bottom;
        public float left;
        public float right;
    }

    public static class DisplayMetricsProvider {
        public static Func<DisplayMetrics> provider = () => new PlayerDisplayMetrics();
    }

    public interface DisplayMetrics {
        void OnEnable();
        void OnGUI();
        void Update();

        float devicePixelRatio { get; } 
        
        WindowPadding viewPadding { get; }
    }

    public class PlayerDisplayMetrics: DisplayMetrics {
        
        float _devicePixelRatio = 0;
        WindowPadding _viewPadding = null;

        public void OnEnable() {
        }

        public void OnGUI() {
            
        }

        public void Update() {
        }


        public float devicePixelRatio {
            get {
                if (this._devicePixelRatio > 0) {
                    return this._devicePixelRatio;
                }

#if UNITY_ANDROID 
                this._devicePixelRatio = AndroidDevicePixelRatio();
#endif
                
#if UNITY_WEBGL 
                this._devicePixelRatio = UIWidgetsWebGLDevicePixelRatio();
#endif
                
#if UNITY_IOS 
                this._devicePixelRatio = IOSDeviceScaleFactor();
#endif

                if (this._devicePixelRatio <= 0) {
                    this._devicePixelRatio = 1;
                }

                return this._devicePixelRatio;
            }
            
        }

        public WindowPadding viewPadding {
            get {
                if (this._viewPadding != null) {
                    return this._viewPadding;
                }
#if UNITY_ANDROID
                this._viewPadding = WindowPadding.zero;
#elif UNITY_WEBGL
                this._viewPadding = WindowPadding.zero;
#elif UNITY_IOS
                
                ViewPadding padding = IOSGetViewportPadding();
                this._viewPadding = new WindowPadding(left: padding.left, top: padding.top, right: padding.right,
                    bottom: padding.bottom);
#else
                this._viewPadding = WindowPadding.zero;
#endif
                return this._viewPadding;
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
                    return metricsInstance.Get<float>("density");
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
        static extern int IOSDeviceScaleFactor();

		[DllImport("__Internal")]
		static extern ViewPadding IOSGetViewportPadding();
#endif
        
    }
}