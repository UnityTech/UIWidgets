using System.Collections.Generic;
//using System.Linq;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.utils {
    public class DragUtils {
        public static List<T> _mapAvatarsToData<T>(List<_DragAvatar<T>> avatars) {
            List<T> ret = new List<T>(avatars.Count);
            foreach (var avatar in avatars) {
                ret.Add(avatar.data);
            }

            return ret;
        }
    }
}