using UnityEditor;

namespace UIWidgets.Tests {
    public static class Menu {
        [MenuItem("Window/UIWidgets/Tests/CanvasAndLayers")]
        public static void canvasAndLayers() {
            EditorWindow.GetWindow(typeof(CanvasAndLayers));
        }

        [MenuItem("Window/UIWidgets/Tests/RenderBoxes")]
        public static void renderBoxes() {
            EditorWindow.GetWindow(typeof(RenderBoxes));
        }

        [MenuItem("Window/UIWidgets/Tests/RenderParagraph")]
        public static void renderRenderParagraph() {
            EditorWindow.GetWindow(typeof(Paragraph));
        }

        [MenuItem("Window/UIWidgets/Tests/Gestures")]
        public static void gestures() {
            EditorWindow.GetWindow(typeof(Gestures));
        }

        [MenuItem("Window/UIWidgets/Tests/RenderEditable")]
        public static void renderEditable() {
            EditorWindow.GetWindow(typeof(RenderEditable));
        }

        [MenuItem("Window/UIWidgets/Tests/Widgets")]
        public static void renderWidgets() {
            EditorWindow.GetWindow(typeof(Widgets));
        }

        [MenuItem("Window/UIWidgets/Tests/Show SceneViewTests")]
        public static void showSceneView() {
            SceneViewTests.show();
        }

        [MenuItem("Window/UIWidgets/Tests/Hide SceneViewTests")]
        public static void hideSceneView() {
            SceneViewTests.hide();
        }
    }
}