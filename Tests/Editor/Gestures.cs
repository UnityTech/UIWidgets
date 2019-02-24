using System;
using System.Linq;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgets.Tests {
    public class Gestures : EditorWindow {
        readonly Func<RenderBox>[] _options;

        readonly string[] _optionStrings;

        int _selected;

        Gestures() {
            this._options = new Func<RenderBox>[] {
                this.tap,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("Gestures");
        }

        WindowAdapter windowAdapter;

        [NonSerialized] bool hasInvoked = false;

        void OnGUI() {
            var selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);
            if (selected != this._selected || !this.hasInvoked) {
                this._selected = selected;
                this.hasInvoked = true;

                var renderBox = this._options[this._selected]();
                if (this.windowAdapter != null) {
                    this.windowAdapter.attachRootRenderBox(renderBox);
                }
            }

            this.windowAdapter.OnGUI();
        }

        void Update() {
            this.windowAdapter.Update();
        }

        void OnEnable() {
            this.windowAdapter = new EditorWindowAdapter(this);
            this.windowAdapter.OnEnable();

            this._tapRecognizer = new TapGestureRecognizer();
            this._tapRecognizer.onTap = () => { Debug.Log("tap"); };

            this._panRecognizer = new PanGestureRecognizer();
            this._panRecognizer.onUpdate = (details) => { Debug.Log("onUpdate " + details); };

            this._doubleTapGesture = new DoubleTapGestureRecognizer();
            this._doubleTapGesture.onDoubleTap = (detail) => { Debug.Log("onDoubleTap"); };
        }

        void OnDisable() {
            this.windowAdapter.OnDisable();
            this.windowAdapter = null;
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