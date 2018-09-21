using UIWidgets.editor;
using UIWidgets.painting;
using UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = UIWidgets.ui.Color;
namespace UIWidgets.Tests
{
    public class EditableTextWiget: EditorWindow
    {
        private WindowAdapter windowAdapter;

        private PaintingBinding paintingBinding;

        private Widget root;

        private Widget image;

        [MenuItem("UIWidgetsTests/EditableTextWiget")]
        public static void renderWidgets() {
            EditorWindow.GetWindow(typeof(EditableTextWiget));
        }
        
        EditableTextWiget() {
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
            this.root = new widgets.Container(
                width: 200,
                height: 200,
                margin: EdgeInsets.all(30.0),
                padding: EdgeInsets.all(15.0),
                color: ui.Color.fromARGB(255, 244, 190, 85),
                child: new EditableText(
                    maxLines: 100, 
                    controller: new TextEditingController("click to edit"),
                    focusNode: new FocusNode(), 
                    style: new TextStyle(),
                    selectionColor: Color.fromARGB(255, 255, 0, 0),
                    cursorColor: Color.fromARGB(255, 0, 0, 0)
                )
            );
            this.windowAdapter.attachRootWidget(root);
            this.titleContent = new GUIContent("EditableTextWiget");
        }
        
        void OnDestroy() {
            this.windowAdapter = null;
        }
    }
}