using System;
using System.Runtime.InteropServices;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    [StructLayout(LayoutKind.Sequential)]
    public struct viewMetrics {
        public float insets_top;
        public float insets_bottom;
        public float insets_left;
        public float insets_right;

        public float padding_top;
        public float padding_bottom;
        public float padding_left;
        public float padding_right;
    }

    public static class DisplayMetricsProvider {
        public static Func<DisplayMetrics> provider = () => new PlayerDisplayMetrics();
    }

    public interface DisplayMetrics {
        void OnEnable();
        void OnGUI();
        void Update();
        void onViewMetricsChanged();

        float devicePixelRatio { get; }

        viewMetrics viewMetrics { get; }

        WindowPadding viewPadding { get; }

        WindowPadding viewInsets { get; }
    }

    public class PlayerDisplayMetrics : DisplayMetrics {
        float _devicePixelRatio = 0;
        viewMetrics? _viewMetrics = null;

        public void OnEnable() {
        }

        public void OnGUI() {
        }

        public void Update() {
            
        }

        public void onViewMetricsChanged() {
            //view metrics marks dirty
            this._viewMetrics = null;
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
                return new WindowPadding(this.viewMetrics.padding_left,
                    this.viewMetrics.padding_top,
                    this.viewMetrics.padding_right,
                    this.viewMetrics.padding_bottom);
            }
        }

        public WindowPadding viewInsets {
            get {
                return new WindowPadding(this.viewMetrics.insets_left,
                    this.viewMetrics.insets_top,
                    this.viewMetrics.insets_right,
                    this.viewMetrics.insets_bottom);
            }
        }

        public viewMetrics viewMetrics {
            get {
                if (this._viewMetrics != null) {
                    return this._viewMetrics.Value;
                }

#if UNITY_ANDROID

                using (
                    AndroidJavaClass viewController =
                        new AndroidJavaClass("com.unity.uiwidgets.plugin.UIWidgetsViewController")
                ) {
                    AndroidJavaObject metrics = viewController.CallStatic<AndroidJavaObject>("getMetrics");
                    float insets_bottom = metrics.Get<float>("insets_bottom");
                    float insets_top = metrics.Get<float>("insets_top");
                    float insets_left = metrics.Get<float>("insets_left");
                    float insets_right = metrics.Get<float>("insets_right");
                    float padding_bottom = metrics.Get<float>("padding_bottom");
                    float padding_top = metrics.Get<float>("padding_top");
                    float padding_left = metrics.Get<float>("padding_left");
                    float padding_right = metrics.Get<float>("padding_right");

                    this._viewMetrics = new viewMetrics {
                        insets_bottom = insets_bottom,
                        insets_left = insets_left,
                        insets_right = insets_right,
                        insets_top = insets_top,
                        padding_left = padding_left,
                        padding_top = padding_top,
                        padding_right = padding_right,
                        padding_bottom = padding_bottom
                    };
                }
#elif UNITY_WEBGL
                this._viewMetrics = new viewMetrics {
                    insets_bottom = 0,
                    insets_left = 0,
                    insets_right = 0,
                    insets_top = 0,
                    padding_left = 0,
                    padding_top = 0,
                    padding_right = 0,
                    padding_bottom = 0
                };
#elif UNITY_IOS
                viewMetrics metrics = IOSGetViewportPadding();
                this._viewMetrics = metrics;
#else
                this._viewMetrics = new viewMetrics {
                    insets_bottom = 0,
                    insets_left = 0,
                    insets_right = 0,
                    insets_top = 0,
                    padding_left = 0,
                    padding_top = 0,
                    padding_right = 0,
                    padding_bottom = 0
                };
#endif
                return this._viewMetrics.Value;
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
        static extern float IOSDeviceScaleFactor();

		[DllImport("__Internal")]
		static extern viewMetrics IOSGetViewportPadding();
#endif
    }
}