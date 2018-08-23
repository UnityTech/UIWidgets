using System;
using System.Linq;
using UIWidgets.editor;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Color = UIWidgets.ui.Color;

namespace UIWidgets.Tests {
    public class RenderBoxes : EditorWindow {
        private readonly Func<RenderBox>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        RenderBoxes() {
            this._options = new Func<RenderBox>[] {
                this.none,
                this.decoratedBox,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("RenderBoxes");
        }

        private WindowAdapter windowAdapter;

        private RendererBindings rendererBindings;

        [NonSerialized] private bool hasInvoked = false;

        void OnGUI() {
            var selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);
            if (selected != this._selected || !this.hasInvoked) {
                this._selected = selected;
                this.hasInvoked = true;

                var renderBox = this._options[this._selected]();
                this.rendererBindings.setRoot(renderBox);
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
            this.rendererBindings = new RendererBindings(this.windowAdapter);
        }

        void OnDestroy() {
            this.windowAdapter = null;
            this.rendererBindings = null;
        }

        RenderBox none() {
            return null;
        }

        RenderBox decoratedBox() {
            return new RenderConstrainedOverflowBox(
                minWidth: 100,
                maxWidth: 100,
                minHeight: 100,
                maxHeight: 100,
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFFFFFFFF),
                        borderRadius: BorderRadius.all(3)
                    )
                )
            );
        }
    }
}