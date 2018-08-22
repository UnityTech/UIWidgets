using UnityEditor;

namespace UIWidgets.Tests {
    public static class Menu {
        [MenuItem("UIWidgetsTests/CanvasAndLayers")]
        public static void canvasAndLayers() {
            EditorWindow.GetWindow(typeof(CanvasAndLayers));
        }
        
        [MenuItem("UIWidgetsTests/RenderBoxes")]
        public static void renderBoxes() {
            EditorWindow.GetWindow(typeof(RenderBoxes));
        }
    }
}