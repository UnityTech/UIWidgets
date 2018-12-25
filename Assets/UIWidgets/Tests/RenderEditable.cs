using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using UIWidgets.animation;
using UIWidgets.editor;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.service;
using UIWidgets.ui;
using UnityEngine;
using Color = UIWidgets.ui.Color;

namespace UIWidgets.Tests
{
    public class RenderEditable: EditorWindow
    {
        private readonly Func<RenderBox>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        class _FixedViewportOffset : ViewportOffset {
            internal _FixedViewportOffset(double _pixels) {
                this._pixels = _pixels;
            }

            internal new static _FixedViewportOffset zero() {
                return new _FixedViewportOffset(0.0);
            }

            double _pixels;

            public override double pixels {
                get { return this._pixels; }
            }

            public override bool applyViewportDimension(double viewportDimension) {
                return true;
            }

            public override bool applyContentDimensions(double minScrollExtent, double maxScrollExtent) {
                return true;
            }

            public override void correctBy(double correction) {
                this._pixels += correction;
            }

            public override void jumpTo(double pixels) {
            }

            public override IPromise animateTo(double to, TimeSpan duration, Curve curve) {
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

        private WindowAdapter windowAdapter;

        [NonSerialized] private bool hasInvoked = false;

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

        private void OnEnable() {
            this.windowAdapter = new EditorWindowAdapter(this);
            this.windowAdapter.OnEnable();
        }

        void OnDisable() {
            this.windowAdapter.OnDisable();
            this.windowAdapter = null;
        }

        private RenderBox box(RenderBox p, int width = 400, int height = 400)
        {
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
        
        private RenderBox flexItemBox(RenderBox p, int width = 200, int height = 100)
        {
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
        
        RenderBox textEditable()
        {
            var span = new TextSpan("", children:
                new List<TextSpan>
                {
                    new TextSpan("Word Wrap:The ascent of the font is the distance from the baseline to the top line of the font, as defined in the font's original data file.", null),
                }, style:new painting.TextStyle(height:1.0));
            
            var flexbox = new RenderFlex(
                direction: Axis.vertical,
                mainAxisAlignment: MainAxisAlignment.spaceAround,
                crossAxisAlignment: CrossAxisAlignment.center);

            flexbox.add(flexItemBox(
                new rendering.RenderEditable(span, TextDirection.ltr, 
                    new _FixedViewportOffset(0.0), new ValueNotifier<bool>(true),
                    onSelectionChanged: selectionChanged, cursorColor: Color.fromARGB(255, 0, 0, 0), 
                    maxLines: 100,
                    selectionColor: Color.fromARGB(255, 255, 0, 0))
            ));
            
            span = new TextSpan("", children:
                new List<TextSpan>
                {
                    new TextSpan("Hard Break:The ascent of the font is the distance\nfrom the baseline to the top \nline of the font,\nas defined in", null),
                }, style:new painting.TextStyle(height:1.0));
            flexbox.add(flexItemBox(
                new rendering.RenderEditable(span, TextDirection.ltr, 
                    new _FixedViewportOffset(0.0), new ValueNotifier<bool>(true),
                    onSelectionChanged: selectionChanged, cursorColor: Color.fromARGB(255, 0, 0, 0), 
                    maxLines: 100,
                    selectionColor: Color.fromARGB(255, 255, 0, 0))
            ));
            
            span = new TextSpan("", children:
                new List<TextSpan>
                {
                    new TextSpan("Single Line:How to create mixin", null),
                }, style:new painting.TextStyle(height:1.0));
            flexbox.add(flexItemBox(
                new rendering.RenderEditable(span, TextDirection.ltr, 
                    new _FixedViewportOffset(0.0), new ValueNotifier<bool>(true),
                    onSelectionChanged: selectionChanged, cursorColor: Color.fromARGB(255, 0, 0, 0), 
                    selectionColor: Color.fromARGB(255, 255, 0, 0))
            , width:300));
            return flexbox;
        }
        

        private void selectionChanged(TextSelection selection, rendering.RenderEditable renderObject,
            SelectionChangedCause cause)
        {
            Debug.Log(string.Format("selection {0}", selection));
            renderObject.selection = selection;
        }
        
    }
}