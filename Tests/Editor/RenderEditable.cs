using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgets.Tests {
    public class RenderEditable : EditorWindow, TextSelectionDelegate {
        readonly Func<RenderBox>[] _options;

        readonly string[] _optionStrings;

        int _selected;

        class _FixedViewportOffset : ViewportOffset {
            internal _FixedViewportOffset(float _pixels) {
                this._pixels = _pixels;
            }

            internal new static _FixedViewportOffset zero() {
                return new _FixedViewportOffset(0.0f);
            }

            float _pixels;

            public override float pixels {
                get { return this._pixels; }
            }

            public override bool applyViewportDimension(float viewportDimension) {
                return true;
            }

            public override bool applyContentDimensions(float minScrollExtent, float maxScrollExtent) {
                return true;
            }

            public override void correctBy(float correction) {
                this._pixels += correction;
            }

            public override void jumpTo(float pixels) { }

            public override IPromise animateTo(float to, TimeSpan duration, Curve curve) {
                return Promise.Resolved();
            }

            public override ScrollDirection userScrollDirection {
                get { return ScrollDirection.idle; }
            }

            public override bool allowImplicitScrolling {
                get { return false; }
            }
        }

        RenderEditable() {
            this._options = new Func<RenderBox>[] {
                this.textEditable,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;
            this.titleContent = new GUIContent("RenderEditable");
        }

        WindowAdapter windowAdapter;

        [NonSerialized] bool hasInvoked = false;

        void OnGUI() {
            var selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);
            if (selected != this._selected || !this.hasInvoked) {
                this._selected = selected;
                this.hasInvoked = true;

                var renderBox = this._options[this._selected]();
                this.windowAdapter.attachRootRenderBox(renderBox);
            }

            this.windowAdapter.OnGUI();
        }

        void Update() {
            this.windowAdapter.Update();
        }

        void OnEnable() {
            this.windowAdapter = new EditorWindowAdapter(this);
            this.windowAdapter.OnEnable();
        }

        void OnDisable() {
            this.windowAdapter.OnDisable();
            this.windowAdapter = null;
        }

        RenderBox box(RenderBox p, int width = 400, int height = 400) {
            return new RenderConstrainedOverflowBox(
                    minWidth: width,
                    maxWidth: width,
                    minHeight: height,
                    maxHeight: height,
                    alignment: Alignment.center,
                    child: p
                )
                ;
        }

        RenderBox flexItemBox(RenderBox p, int width = 200, int height = 100) {
            return new RenderConstrainedBox(
                additionalConstraints: new BoxConstraints(minWidth: width, maxWidth: width, minHeight: height,
                    maxHeight: height),
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFFFFFFFF),
                        borderRadius: BorderRadius.all(3),
                        border: Border.all(Color.fromARGB(255, 255, 0, 0), 1)
                    ),
                    child: new RenderPadding(EdgeInsets.all(10), p
                    )
                ));
        }

        RenderBox textEditable() {
            var span = new TextSpan("", children:
                new List<TextSpan> {
                    new TextSpan(
                        "Word Wrap:The ascent of the font is the distance from the baseline to the top line of the font, as defined in the font's original data file.",
                        null),
                }, style: new TextStyle(height: 1.0f));

            var flexbox = new RenderFlex(
                direction: Axis.vertical,
                mainAxisAlignment: MainAxisAlignment.spaceAround,
                crossAxisAlignment: CrossAxisAlignment.center);

            flexbox.add(this.flexItemBox(
                new Unity.UIWidgets.rendering.RenderEditable(span, TextDirection.ltr,
                    offset: new _FixedViewportOffset(0.0f), showCursor: new ValueNotifier<bool>(true),
                    onSelectionChanged: this.selectionChanged, cursorColor: Color.fromARGB(255, 0, 0, 0),
                    maxLines: 100,
                    selectionColor: Color.fromARGB(255, 255, 0, 0),
                    textSelectionDelegate: this)
            ));

            span = new TextSpan("", children:
                new List<TextSpan> {
                    new TextSpan(
                        "Hard Break:The ascent of the font is the distance\nfrom the baseline to the top \nline of the font,\nas defined in",
                        null),
                }, style: new TextStyle(height: 1.0f));
            flexbox.add(this.flexItemBox(
                new Unity.UIWidgets.rendering.RenderEditable(span, TextDirection.ltr,
                    offset: new _FixedViewportOffset(0.0f), showCursor: new ValueNotifier<bool>(true),
                    onSelectionChanged: this.selectionChanged, cursorColor: Color.fromARGB(255, 0, 0, 0),
                    maxLines: 100,
                    selectionColor: Color.fromARGB(255, 255, 0, 0),
                    textSelectionDelegate: this)
            ));

            span = new TextSpan("", children:
                new List<TextSpan> {
                    new TextSpan("Single Line:How to create mixin", null),
                }, style: new TextStyle(height: 1.0f));
            flexbox.add(this.flexItemBox(
                new Unity.UIWidgets.rendering.RenderEditable(span, TextDirection.ltr,
                    offset: new _FixedViewportOffset(0.0f), showCursor: new ValueNotifier<bool>(true),
                    onSelectionChanged: this.selectionChanged, cursorColor: Color.fromARGB(255, 0, 0, 0),
                    selectionColor: Color.fromARGB(255, 255, 0, 0),
                    textSelectionDelegate: this)
                , width: 300));
            return flexbox;
        }


        void selectionChanged(TextSelection selection, Unity.UIWidgets.rendering.RenderEditable renderObject,
            SelectionChangedCause cause) {
            Debug.Log($"selection {selection}");
            renderObject.selection = selection;
        }

        public TextEditingValue textEditingValue { get; set; }

        public void hideToolbar() { }

        public void bringIntoView(TextPosition textPosition) { }
    }
}