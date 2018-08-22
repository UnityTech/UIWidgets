using UnityEditor;

namespace Editor.Tests {
    public static class Menu {
        [MenuItem("UIWidgetsTests/CanvasAndLayers")]
        public static void canvasAndLayers() {
            EditorWindow.GetWindow(typeof(CanvasAndLayers));
        }
    }
}