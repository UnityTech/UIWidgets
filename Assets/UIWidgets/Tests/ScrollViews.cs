using System;
using System.Linq;
using UIWidgets.editor;
using UIWidgets.painting;
using UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = UIWidgets.ui.Color;

namespace UIWidgets.Tests {
    public class ScrollViews : EditorWindow {
        private readonly Func<Widget>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        ScrollViews() {
            this._options = new Func<Widget>[] {
                this.none,
                this.listView,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("ScrollViews");
        }

        private WindowAdapter windowAdapter;

        [NonSerialized] private bool hasInvoked = false;

        void OnGUI() {
            var selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);
            if (selected != this._selected || !this.hasInvoked) {
                this._selected = selected;
                this.hasInvoked = true;

                var widget = this._options[this._selected]();
                if (this.windowAdapter != null) {
                    this.windowAdapter.attachRootWidget(widget);
                }
            }

            if (this.windowAdapter != null) {
                this.windowAdapter.OnGUI();
            }
        }

        void Update() {
            if (this.windowAdapter != null) {
                this.windowAdapter.Update();
            }
        }

        private void OnEnable() {
            this.windowAdapter = new WindowAdapter(this);
        }

        void OnDestroy() {
            this.windowAdapter = null;
        }

        Widget none() {
            return null;
        }

        Widget listView() {
            return ListView.builder(
                itemExtent: 20.0,
                itemBuilder: (context, index) => {
                    return new Container(
                        color: Color.fromARGB(255, (index * 10) % 256, (index * 10) % 256, (index * 10) % 256)
                    );
                }
            );
        }
    }
}