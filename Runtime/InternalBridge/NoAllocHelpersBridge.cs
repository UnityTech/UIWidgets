using System.Collections.Generic;

namespace Unity.UIWidgets.InternalBridge {
    public static class NoAllocHelpersBridge<T> {
        public static T[] ExtractArrayFromListT(List<T> list) {
            return UnityEngine.NoAllocHelpers.ExtractArrayFromListT(list);
        }

        public static void ResizeList(List<T> list, int size) {
            if (size < list.Count) {
                list.RemoveRange(size, list.Count - size);
                return;
            }

            if (size == list.Count) {
                return;
            }

            if (list.Capacity < size) {
                list.Capacity = size;
            }

            UnityEngine.NoAllocHelpers.ResizeList(list, size);
        }

        public static void EnsureListElemCount(List<T> list, int size) {
            list.Clear();
            if (list.Capacity < size) {
                list.Capacity = size;
            }

            ResizeList(list, size);
        }
    }
}
