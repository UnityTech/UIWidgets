using UIWidgets.painting;
using UIWidgets.editor;
using UIWidgets.widgets;
using System.Collections.Generic;
using UIWidgets.rendering;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace UIWidgets.Tests {
    public class Widgets : EditorWindow {
        private WindowAdapter windowAdapter;

        private PaintingBinding paintingBinding;

        private readonly Func<Widget>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;
        
        [NonSerialized] private bool hasInvoked = false;

        Widgets() {
            this._options = new Func<Widget>[] {
                this.container,
                this.flexRow,
                this.flexColumn,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("Widgets Test");
        }

        void OnGUI() {
            var selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);
            if (selected != this._selected || !this.hasInvoked) {
                this._selected = selected;
                this.hasInvoked = true;

                var rootWidget = this._options[this._selected]();
                
                this.windowAdapter.attachRootWidget(rootWidget);
            }

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
        }

        void OnDestroy() {
            this.windowAdapter = null;
        }

        Widget flexRow() {
            var image = new widgets.Image(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100
            );
            List<Widget> rowImages = new List<Widget>();
            rowImages.Add(image);
            rowImages.Add(image);
            rowImages.Add(image);
            rowImages.Add(image);

            var row = new widgets.Row(
                textDirection: null,
                textBaseline: null,
                key: null,
                mainAxisAlignment: MainAxisAlignment.start,
                mainAxisSize: MainAxisSize.max,
                crossAxisAlignment: CrossAxisAlignment.center,
                verticalDirection: VerticalDirection.down,
                children: rowImages
            );

            return row;
        }
        
        Widget flexColumn() {
            var image = new widgets.Image(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100
            );
            List<Widget> columnImages = new List<Widget>();
            columnImages.Add(image);
            columnImages.Add(image);
            columnImages.Add(image);

            var column = new widgets.Column(
                textDirection: null,
                textBaseline: null,
                key: null,
                mainAxisAlignment: MainAxisAlignment.start,
                mainAxisSize: MainAxisSize.max,
                crossAxisAlignment: CrossAxisAlignment.center,
                verticalDirection: VerticalDirection.down,
                children: columnImages
            );

            return column;
        }

        Widget container() {
            var image = new widgets.Image(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100,
                repeat: ImageRepeat.repeatX
            );
            var container = new widgets.Container(
                width: 200,
                height: 200,
                margin: EdgeInsets.all(30.0),
                padding: EdgeInsets.all(15.0),
                color: ui.Color.fromARGB(255, 244, 190, 85),
                child: image
            );

            return container;
        }
    }
}