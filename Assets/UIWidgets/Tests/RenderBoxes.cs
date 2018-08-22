using System;
using System.Linq;
using UIWidgets.editor;
using UIWidgets.rendering;
using UnityEditor;
using UnityEngine;

namespace UIWidgets.Tests {
    public class RenderBoxes : EditorWindow {
        private readonly Func<RenderBox>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        RenderBoxes() {
            this._options = new Func<RenderBox>[] {
                this.test1,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("RenderBoxes");
        }

        private WindowAdapter windowAdapter;

        private RendererBindings rendererBindings;

        [NonSerialized] private bool hasInvoked = false;

        void OnGUI() {
            if (this.windowAdapter != null) {
                this.windowAdapter.OnGUI();
            }

            var selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);
            if (selected != this._selected || !this.hasInvoked) {
                this._selected = selected;
                this.hasInvoked = true;

                var renderBox = this._options[this._selected]();
                this.rendererBindings.setRoot(renderBox);
            }
        }

        void Update() {
            if (this.windowAdapter != null) {
                this.windowAdapter.Update();
            }
        }

        private void OnEnable() {
            this.windowAdapter = new WindowAdapter(this);
            this.rendererBindings = new RendererBindings(this.windowAdapter);
        }

        void OnDestroy() {
            this.windowAdapter = null;
            this.rendererBindings = null;
        }

        RenderBox test1() {
            return null;
        }
    }
}