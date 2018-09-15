using UIWidgets.painting;
using UIWidgets.editor;
using UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace UIWidgets.Tests {
    public class Widgets : EditorWindow {
        private WindowAdapter windowAdapter;

        private WidgetsBindings widgetsBindings;

        private PaintingBinding paintingBinding;

        private Widget root;

        private Widget image;

        Widgets() {
            this.titleContent = new GUIContent("Widgets Test");
            this.image = new widgets.Image(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100
            );
            this.root = new widgets.Container(
                width: 200,
                height: 200,
                margin: EdgeInsets.all(30.0),
                child: image
            );
        }

        void OnGUI() {
            if (this.windowAdapter != null) {
                this.windowAdapter.OnGUI();
            }
        }

        private void Update() {
            if (this.windowAdapter != null) {
                this.windowAdapter.Update();
            }
        }

        private void OnEnable() {
            this.paintingBinding = new PaintingBinding(null);
            paintingBinding.initInstances();
            this.windowAdapter = new WindowAdapter(this);
            this.widgetsBindings = new WidgetsBindings(windowAdapter);
            if (widgetsBindings != null) {
                widgetsBindings.attachRootWidget(root);
            }
        }

        void OnDestroy() {
            this.windowAdapter = null;
            this.widgetsBindings = null;
        }
    }
}