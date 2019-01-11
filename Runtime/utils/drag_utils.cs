using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.utils {
    public class DragUtils {
        public static List<T> _mapAvatarsToData<T>(List<_DragAvatar<T>> avatars) {
            return avatars.Select(avatar => avatar.data).ToList();
        }
    }
}