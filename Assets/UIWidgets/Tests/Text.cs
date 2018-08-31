using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.editor;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Color = UIWidgets.ui.Color;
using FontStyle = UIWidgets.ui.FontStyle;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.Tests
{
    public class Text : EditorWindow
    {
        private readonly Func<RenderBox>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        Text() {
            this._options = new Func<RenderBox>[] {
                this.text,
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

        RenderBox text()
        {
            /*
             *bool inherit, Color color, double? fontSize, FontWeight fontWeight, 
            FontStyle fontStyle, double letterSpacing, double wordSpacing, 
            TextBaseline textBaseline, double height, TextDecoration decoration
             * 
             */
            var style1 = new painting.TextStyle(true, Color.fromARGB(255, 0, 0, 0), null, FontWeight.w400, FontStyle.normal, 1,
                1, TextBaseline.alphabetic, 1, TextDecoration.none);
            var style2 = new painting.TextStyle(true, Color.fromARGB(255, 255, 0, 0), null, FontWeight.w400, FontStyle.normal, 1,
                1, TextBaseline.alphabetic, 1, TextDecoration.none, fontFamily:"Helvetica");
            return new RenderConstrainedOverflowBox(
                minWidth: 200,
                maxWidth: 200,
                minHeight: 100,
                maxHeight: 100,
                alignment: Alignment.center,
//                child: new RenderParagraph(new TextSpan(style1, "is is FontStyle fontStyle, double letterSpacing, double wordSpacing, ",
//                    null))
                child: new RenderParagraph(new TextSpan(style1, "",
                    new List<TextSpan>()
                    {
                        new TextSpan(null, "Hello  sda fdf asdf asd fadsfdfs fdsffsdf d sdfsfsf sd fsfsdf", null),
                        new TextSpan(null, "TextBaseline textBaseline, double height, TextDecoration decoration", null),
                        new TextSpan(style2, " Unity Widgets", null),
                        new TextSpan(null, " Text", null)
                    }))
            );
        }
    }
}