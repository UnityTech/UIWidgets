using System.Collections.Generic;

namespace Unity.UIWidgets.utils {
    public static partial class GcCacheHelper {
        public static List<T> RepeatList<T>(T value, int length) {
            List<T> newList = new List<T>(length);
            for (int i = 0; i < length; i++) {
                newList.Add(value);
            }
            return newList;
        }
    }
}