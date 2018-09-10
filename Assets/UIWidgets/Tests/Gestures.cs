using System;
using System.Linq;
using UIWidgets.editor;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Color = UIWidgets.ui.Color;

namespace UIWidgets.Tests {
    public class Gestures : EditorWindow {
        private readonly Func<RenderBox>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        Gestures() {
            this._options = new Func<RenderBox>[] {
                this.tap,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("Gestures");
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

            this._tapRecognizer = new TapGestureRecognizer(this.rendererBindings.rendererBinding);
            this._tapRecognizer.onTap = () => { Debug.Log("tap"); };

            this._panRecognizer = new PanGestureRecognizer(this.rendererBindings.rendererBinding);
            this._panRecognizer.onUpdate = (details) => { Debug.Log("onUpdate " + details); };

            this._doubleTapGesture = new DoubleTapGestureRecognizer(this.rendererBindings.rendererBinding);
            this._doubleTapGesture.onDoubleTap = () => { Debug.Log("onDoubleTap"); };
        }

        void OnDestroy() {
            this.windowAdapter = null;
            this.rendererBindings = null;
        }

        TapGestureRecognizer _tapRecognizer;

        PanGestureRecognizer _panRecognizer;

        DoubleTapGestureRecognizer _doubleTapGesture;

        void _handlePointerDown(PointerDownEvent evt) {
            this._tapRecognizer.addPointer(evt);
            this._panRecognizer.addPointer(evt);
            this._doubleTapGesture.addPointer(evt);
        }

        RenderBox tap() {
            return new RenderPointerListener(
                onPointerDown: this._handlePointerDown,
                behavior: HitTestBehavior.opaque,
                child: new RenderConstrainedBox(
                    additionalConstraints: BoxConstraints.tight(Size.square(100)),
                    child: new RenderDecoratedBox(
                        decoration: new BoxDecoration(
                            color: new Color(0xFF00FF00)
                        )
                    ))
            );
        }
    }
}