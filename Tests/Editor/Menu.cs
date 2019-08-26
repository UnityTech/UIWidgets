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

        [MenuItem("UIWidgetsTests/RenderParagraph")]
        public static void renderRenderParagraph() {
            EditorWindow.GetWindow(typeof(Paragraph));
        }

        [MenuItem("UIWidgetsTests/Gestures")]
        public static void gestures() {
            EditorWindow.GetWindow(typeof(Gestures));
        }

        [MenuItem("UIWidgetsTests/RenderEditable")]
        public static void renderEditable() {
            EditorWindow.GetWindow(typeof(RenderEditable));
        }

        [MenuItem("UIWidgetsTests/Widgets")]
        public static void renderWidgets() {
            EditorWindow.GetWindow(typeof(Widgets));
        }

        //These samples are not available after Unity2019.1
        /*
        [MenuItem("UIWidgetsTests/Show SceneViewTests")]
        public static void showSceneView() {
            SceneViewTests.show();
        }

        [MenuItem("UIWidgetsTests/Hide SceneViewTests")]
        public static void hideSceneView() {
            SceneViewTests.hide();
        }*/
    }
}