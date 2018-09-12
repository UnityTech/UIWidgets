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

namespace UIWidgets.Tests
{
    public class Paragraph : EditorWindow
    {
        private readonly Func<RenderBox>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        Paragraph() {
            this._options = new Func<RenderBox>[] {
                this.text,
                this.textHeight,
                this.textOverflow,
                this.textAlign,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("RenderParagraph");
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

        private RenderBox box(RenderParagraph p, int width = 200, int height = 200)
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
        
        private RenderBox flexItemBox(RenderParagraph p, int width = 200, int height = 150)
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
        
        RenderBox text()
        {
            return box(
                new RenderParagraph(new TextSpan("", children:
                    new List<TextSpan>()
                    {
                        new TextSpan("Real-time 3D revolutioni淡粉色的方式地方zes the animation pipeline ", null),
                        new TextSpan(style: new painting.TextStyle(color: Color.fromARGB(255, 255, 0, 0)), 
                            text: "for Disney Television Animation's “Baymax Dreams"),
                        new TextSpan(" Unity Widgets"),
                        new TextSpan(" Text"),
                        new TextSpan("Real-time 3D revolutionizes the animation pipeline "),
                        new TextSpan(style: new painting.TextStyle(color: Color.fromARGB(125, 255, 0, 0)), 
                            text: "Transparent Red Text\n\n"),
                        new TextSpan(style: new painting.TextStyle(fontWeight: FontWeight.w700), 
                            text: "Bold Text Test Bold Textfs Test: FontWeight.w70\n\n"),
                        new TextSpan(style: new painting.TextStyle(fontStyle: FontStyle.italic), 
                            text: "This is FontStyle.italic Text This is FontStyle.italic Text\n\n"),
                        new TextSpan(style: new painting.TextStyle(fontStyle: FontStyle.italic, fontWeight: FontWeight.w700), 
                            text: "This is FontStyle.italic And 发撒放豆腐sad 发生的 Bold Text This is FontStyle.italic  And Bold  Text\n\n"),
                        new TextSpan(style: new painting.TextStyle(fontSize: 18), 
                            text: "FontSize 18: Get a named matrix value from the shader."),
                        new TextSpan(style: new painting.TextStyle(fontSize: 14), 
                            text: "FontSize 14"),
                    })));
        }
        
        
        RenderBox textAlign()
        {
            var flexbox = new RenderFlex(
                direction: Axis.vertical,
                mainAxisAlignment: MainAxisAlignment.spaceAround,
                crossAxisAlignment: CrossAxisAlignment.center);
            var height = 120;

            flexbox.add(flexItemBox(
                new RenderParagraph(new TextSpan(EditorGUIUtility.pixelsPerPoint.ToString() + "Align To Left\nMaterials define how light reacts with the " +
                                                 "surface of a model, and are an essential ingredient in making " +
                                                 "believable visuals. When you’ve created a "), textAlign: TextAlign.left),
                height: height
            ));
            flexbox.add(flexItemBox(
                new RenderParagraph(new TextSpan(EditorGUIUtility.pixelsPerPoint.ToString() + "Align To Rgit\nMaterials define how light reacts with the " +
                                                 "surface of a model, and are an essential ingredient in making " +
                                                 "believable visuals. When you’ve created a "), textAlign: TextAlign.right),
                height: height
            ));
            flexbox.add(flexItemBox(
                new RenderParagraph(new TextSpan(EditorGUIUtility.pixelsPerPoint.ToString() + "Align To Center\nMaterials define how light reacts with the " +
                                                 "surface of a model, and are an essential ingredient in making " +
                                                 "believable visuals. When you’ve created a "), textAlign: TextAlign.center),
                height: height
            ));
            flexbox.add(flexItemBox(
                new RenderParagraph(new TextSpan("Align To Justify\nMaterials define how light reacts with the " +
                                                 "surface of a model, and are an essential ingredient in making " +
                                                 "believable visuals. When you’ve created a "), textAlign: TextAlign.justify),
                 height: height
            ));
          return flexbox;
        }
        
        RenderBox textOverflow()
        {
            return box(
                new RenderParagraph(new TextSpan("", children:
                    new List<TextSpan>()
                    {
                        new TextSpan("Real-time 3D revolutionizes:\n the animation pipeline.\n\n\nrevolutionizesn\n\nReal-time 3D revolutionizes the animation pipeline ", null),
                    }), maxLines: 3), 200, 80);
        }
        
        RenderBox textHeight()
        {
            var text =
                "Hello UIWidgets. Real-time 3D revolutionize \nReal-time 3D revolutionize\nReal-time 3D revolutionize\n\n";
            return box(
                new RenderParagraph(new TextSpan(text: "", children:
                    new List<TextSpan>()
                    {
                        new TextSpan(style: new painting.TextStyle(height: 1), 
                            text: "Height 1.0 Text:" + text),
                        new TextSpan(style: new painting.TextStyle(height: 1.2), 
                            text: "Height 1.2 Text:" + text),
                        new TextSpan(style: new painting.TextStyle(height: 1.5), 
                           text: "Height 1.5 Text:" + text),
                    })), width: 300, height: 300);
        }
        
    }
}