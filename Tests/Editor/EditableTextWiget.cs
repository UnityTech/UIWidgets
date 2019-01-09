using Unity.UIWidgets.editor;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgets.Tests {
    public class EditableTextWiget : EditorWindow {
        WindowAdapter windowAdapter;

        Widget root;

        Widget image;

        [MenuItem("UIWidgetsTests/EditableTextWidget")]
        public static void renderWidgets() {
            GetWindow(typeof(EditableTextWiget));
        }

        string txt = "Hello\n" +
                     "This is useful when you need to check if a certain key has been pressed - possibly with modifiers. The syntax for the key string\n" +
                     "asfsd \n" +
                     "P1:\n" +
                     "This is useful when you need to check if a certain key has been pressed - possibly with modifiers.The syntax for the key st\n" +
                     "\n" +
                     "\n" +
                     "\n" +
                     "\n" +
                     " sfsafd";

        EditableTextWiget() {
        }

        void OnGUI() {
            this.windowAdapter.OnGUI();
        }

        void Update() {
            this.windowAdapter.Update();
        }

        void OnEnable() {
            this.windowAdapter = new EditorWindowAdapter(this);
            this.windowAdapter.OnEnable();
            this.root = new Container(
                width: 200,
                height: 200,
                margin: EdgeInsets.all(30.0),
                padding: EdgeInsets.all(15.0),
                color: Color.fromARGB(255, 244, 190, 85),
                child: new EditableText(
                    maxLines: 100,
                    controller: new TextEditingController(this.txt),
                    focusNode: new FocusNode(),
                    style: new TextStyle(),
                    selectionColor: Color.fromARGB(255, 255, 0, 0),
                    cursorColor: Color.fromARGB(255, 0, 0, 0)
                )
            );
            this.windowAdapter.attachRootWidget(this.root);
            this.titleContent = new GUIContent("EditableTextWidget");
        }

        void OnDisable() {
            this.windowAdapter.OnDisable();
            this.windowAdapter = null;
        }
    }
}
