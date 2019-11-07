using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.plugins.raycast {
    public class RaycastableRect {
        public bool isDirty;
        public Rect rect;

        public RaycastableRect(bool isDirty) {
            this.isDirty = isDirty;
        }
    }

    public class RaycastManager {
        static RaycastManager _instance;

        public static RaycastManager instance {
            get {
                if (_instance == null) {
                    _instance = new RaycastManager();
                }

                return _instance;
            }
        }

        public Dictionary<int, Dictionary<int, RaycastableRect>> hashCodeList =
            new Dictionary<int, Dictionary<int, RaycastableRect>>();

        public static void VerifyWindow(int windowHashCode) {
            if (!instance.hashCodeList.ContainsKey(windowHashCode)) {
                // Debug.Log($"New Window: @[{windowHashCode}] ({instance.hashCodeList.Count})");
                instance.hashCodeList.Add(windowHashCode, new Dictionary<int, RaycastableRect>());
            }
        }

        public static void AddToList(int key, int windowHashCode) {
            VerifyWindow(windowHashCode);
            // Debug.Log($"Add To List: [{key}]@[{windowHashCode}]");
            if (!instance.hashCodeList[windowHashCode].ContainsKey(key)) {
                instance.hashCodeList[windowHashCode][key] = new RaycastableRect(true);
            }
        }

        public static void MarkDirty(int key, int windowHashCode) {
            // Debug.Log($"Mark Dirty: [{key}]@[{windowHashCode}]");
            if (instance.hashCodeList[windowHashCode].ContainsKey(key)) {
                instance.hashCodeList[windowHashCode][key].isDirty = true;
            }
        }

        public static void UpdateSizeOffset(int key, int windowHashCode, Size size, Offset offset) {
            // Debug.Log($"Update Size Offset: [{key}]@[{windowHashCode}]");
            if (instance.hashCodeList[windowHashCode].ContainsKey(key)) {
                if (instance.hashCodeList[windowHashCode][key].isDirty) {
                    instance.hashCodeList[windowHashCode][key].rect =
                        Rect.fromLTWH(offset.dx, offset.dy, size.width, size.height);
                    instance.hashCodeList[windowHashCode][key].isDirty = false;
                }
            }
        }

        public static void RemoveFromList(int key, int windowHashCode) {
            // Debug.Log($"Remove From List: [{key}]@[{windowHashCode}]");
            if (instance.hashCodeList[windowHashCode].ContainsKey(key)) {
                instance.hashCodeList[windowHashCode].Remove(key);
            }
        }

        public static bool CheckCastThrough(int windowHashCode, Vector2 pos) {
            foreach (var item in instance.hashCodeList[windowHashCode]) {
                if (item.Value.rect.contains(new Offset(pos.x, pos.y))) {
                    return false;
                }
            }

            return true;
        }
    }
}