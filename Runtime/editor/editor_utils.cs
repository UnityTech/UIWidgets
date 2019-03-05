#if UNITY_EDITOR

using System;
using System.Collections;
using System.Reflection;
using Unity.UIWidgets.engine;
using UnityEngine;


using UnityEditor;
namespace Unity.UIWidgets.Editor {
    [InitializeOnLoad]
    public class EditorUtils {
        static EditorUtils() {
            DisplayMetricsProvider.provider = () => new EditorPlayerDisplayMetrics();
        }
    }

    public class EditorPlayerDisplayMetrics : DisplayMetrics {

        float _lastDevicePixelRatio = 0;

        public void OnGUI() {
        }

        public void Update() {
            this._lastDevicePixelRatio = GameViewUtil.getGameViewDevicePixelRatio();
        }

        public float DevicePixelRatio {
            get { return this._lastDevicePixelRatio; }
        }
    }

    static class GameViewUtil {

        static Type _gameViewType;

        static string _gameViewClassName = "UnityEditor.GameView";

        public static float getGameViewDevicePixelRatio(float fallback = 1) {
            loadTypeIfNeed();

            EditorWindow gameview = getMainGameView();
            if (gameview == null) {
                return fallback;
            }

            bool lowResolutionForAspectRatios = false;
            if (!getPropertyValue(gameview, "lowResolutionForAspectRatios",
                ref lowResolutionForAspectRatios)) {
                return fallback;
            }
            if (lowResolutionForAspectRatios) {
                return 1;
            }

            Vector2 sizeValue = new Vector2();
            if (!getFieldValue(gameview, "m_LastWindowPixelSize", ref sizeValue)) {
                return fallback;
            }
            if (gameview.position.width > 0) {
                return sizeValue.x / gameview.position.width;
            }
            if (gameview.position.height > 0) {
                return sizeValue.y / gameview.position.height;
            }

            return fallback;
        }

        static EditorWindow getMainGameView() {
            IEnumerable enumerable = null;
            if (!getFieldValue(null, "s_GameViews", ref enumerable)) {
                return null;
            }
            IEnumerator enumerator = enumerable != null ? enumerable.GetEnumerator() : null;
            if (enumerator != null && enumerator.MoveNext()) {
                return enumerator.Current as EditorWindow;
            }
            return null;
        }

        static bool getFieldValue<T>(object ins, string name, ref T result) {
            var fieldInfo = _gameViewType.GetField(name, BindingFlags.Public
                                                         | BindingFlags.NonPublic
                                                         | BindingFlags.Static | BindingFlags.Instance);
            if (fieldInfo == null) {
                return false;
            }
            result = (T) fieldInfo.GetValue(ins);
            return true;
        }

        static void loadTypeIfNeed() {
            if (_gameViewType == null) {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    var type = assembly.GetType(_gameViewClassName);
                    if (type != null) {
                        _gameViewType = type;
                    }
                }
            }
        }

        static bool getPropertyValue<T>(object ins, string name, ref T result) {
            var property = _gameViewType.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Static | BindingFlags.Instance);
            if (property == null) {
                return false;
            }

            result = (T) property.GetValue(ins);
            return true;
        }
    }
}

#endif