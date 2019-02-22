using Unity.UIWidgets.engine;
using UnityEditor;

namespace Unity.UIWidgets.Editor {
    
    [InitializeOnLoad]
    public class Startup {
        static Startup() {
            DisplayMetrics.SetDevicePixelRatioGetter(() => { return EditorGUIUtility.pixelsPerPoint; });
        }
    }
}