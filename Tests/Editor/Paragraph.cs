using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgets.Tests {
    public class Paragraph : EditorWindow {
        readonly Func<RenderBox>[] _options;

        readonly string[] _optionStrings;

        int _selected;

        Paragraph() {
            this._options = new Func<RenderBox>[] {
                this.text,
                this.textHeight,
                this.textOverflow,
                this.textAlign,
                this.textDecoration,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("RenderParagraph");
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

        RenderBox none() {
            return null;
        }

        RenderBox box(RenderParagraph p, int width = 200, int height = 600) {
            return new RenderConstrainedOverflowBox(
                    minWidth: width,
                    maxWidth: width,
                    minHeight: height,
                    maxHeight: height,
                    alignment: Alignment.center,
                    child: p
                );
        }

        RenderBox flexItemBox(RenderParagraph p, int width = 200, int height = 150) {
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

        RenderBox text() {
            return this.box(
                new RenderParagraph(new TextSpan("", children:
                    new List<TextSpan>() {
                        new TextSpan("Real-time 3D revolutioni淡粉色的方式地方zes the animation pipeline ", null),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0)),
                            text: "for Disney Television Animation's “Baymax Dreams"),
                        new TextSpan(" Unity Widgets"),
                        new TextSpan(" Text"),
                        new TextSpan("Real-time 3D revolutionizes the animation pipeline "),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(125, 255, 0, 0)),
                            text: "Transparent Red Text\n\n"),
                        new TextSpan(style: new TextStyle(fontWeight: FontWeight.w700),
                            text: "Bold Text Test Bold Textfs Test: FontWeight.w70\n\n"),
                        new TextSpan(style: new TextStyle(fontStyle: FontStyle.italic),
                            text: "This is FontStyle.italic Text This is FontStyle.italic Text\n\n"),
                        new TextSpan(
                            style: new TextStyle(fontStyle: FontStyle.italic, fontWeight: FontWeight.w700),
                            text:
                            "This is FontStyle.italic And 发撒放豆腐sad 发生的 Bold Text This is FontStyle.italic  And Bold  Text\n\n"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "FontSize 18: Get a named matrix value from the shader.\n\n"),
                        new TextSpan(style: new TextStyle(fontSize: 24),
                            text: "Emoji \ud83d\ude0a\ud83d\ude0b\ud83d\ude0d\ud83d\ude0e\ud83d\ude00"),
                        new TextSpan(style: new TextStyle(fontSize: 14),
                            text: "Emoji \ud83d\ude0a\ud83d\ude0b\ud83d\ude0d\ud83d\ude0e\ud83d\ude00 Emoji"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "Emoji \ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "\ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "\ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "\ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 18),
                            text: "\ud83d\ude01\ud83d\ude02\ud83d\ude03\ud83d\ude04\ud83d\ude05"),
                        new TextSpan(style: new TextStyle(fontSize: 24),
                            text: "Emoji \ud83d\ude06\ud83d\ude1C\ud83d\ude18\ud83d\ude2D\ud83d\ude0C\ud83d\ude1E\n\n"),
                        new TextSpan(style: new TextStyle(fontSize: 14),
                            text: "FontSize 14"),
                    })));
        }

        RenderBox textDecoration() {
            return this.box(
                new RenderParagraph(new TextSpan(style: new TextStyle(height: 1.2f), text: "", children:
                    new List<TextSpan>() {
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0),
                                decoration: TextDecoration.underline),
                            text: "Real-time 3D revolution\n"),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0),
                                decoration: TextDecoration.underline, decorationStyle: TextDecorationStyle.doubleLine),
                            text: "Double line Real-time 3D revolution\n"),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0),
                                decoration: TextDecoration.underline, fontSize: 24),
                            text: "Real-time 3D revolution\n"),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0),
                                decoration: TextDecoration.overline),
                            text: "Over line Real-time 3D revolution\n"),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0),
                                decoration: TextDecoration.overline, decorationStyle: TextDecorationStyle.doubleLine),
                            text: "Over line Real-time 3D revolution\n"),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0),
                                decoration: TextDecoration.lineThrough),
                            text: "Line through Real-time 3D revolution\n"),
                        new TextSpan(style: new TextStyle(color: Color.fromARGB(255, 255, 0, 0),
                                decoration: TextDecoration.lineThrough,
                                decorationColor: Color.fromARGB(255, 0, 255, 0)),
                            text: "Color Line through Real-time 3D revolution\n"),
                    })), width: 400);
        }

        RenderBox textAlign() {
            var flexbox = new RenderFlex(
                direction: Axis.vertical,
                mainAxisAlignment: MainAxisAlignment.spaceAround,
                crossAxisAlignment: CrossAxisAlignment.center);
            var height = 120;

            flexbox.add(this.flexItemBox(
                new RenderParagraph(new TextSpan(EditorGUIUtility.pixelsPerPoint.ToString() +
                                                 "Align To Left\nMaterials define how light reacts with the " +
                                                 "surface of a model, and are an essential ingredient in making " +
                                                 "believable visuals. When you’ve created a "),
                    textAlign: TextAlign.left),
                height: height
            ));
            flexbox.add(this.flexItemBox(
                new RenderParagraph(new TextSpan(EditorGUIUtility.pixelsPerPoint.ToString() +
                                                 "Align To Rgit\nMaterials define how light reacts with the " +
                                                 "surface of a model, and are an essential ingredient in making " +
                                                 "believable visuals. When you’ve created a "),
                    textAlign: TextAlign.right),
                height: height
            ));
            flexbox.add(this.flexItemBox(
                new RenderParagraph(new TextSpan(EditorGUIUtility.pixelsPerPoint.ToString() +
                                                 "Align To Center\nMaterials define how light reacts with the " +
                                                 "surface of a model, and are an essential ingredient in making " +
                                                 "believable visuals. When you’ve created a "),
                    textAlign: TextAlign.center),
                height: height
            ));
            flexbox.add(this.flexItemBox(
                new RenderParagraph(new TextSpan("Align To Justify\nMaterials define how light reacts with the " +
                                                 "surface of a model, and are an essential ingredient in making " +
                                                 "believable visuals. When you’ve created a "),
                    textAlign: TextAlign.justify),
                height: height
            ));
            return flexbox;
        }

        RenderBox textOverflow() {
            return this.box(
                new RenderParagraph(new TextSpan("", children:
                    new List<TextSpan>() {
                        new TextSpan(
                            "Real-time 3D revolutionizes:\n the animation pipeline.\n\n\nrevolutionizesn\n\nReal-time 3D revolutionizes the animation pipeline ",
                            null),
                    }), maxLines: 3), 200, 80);
        }

        RenderBox textHeight() {
            var text =
                "Hello UIWidgets. Real-time 3D revolutionize \nReal-time 3D revolutionize\nReal-time 3D revolutionize\n\n";
            return this.box(
                new RenderParagraph(new TextSpan(text: "", children:
                    new List<TextSpan>() {
                        new TextSpan(style: new TextStyle(height: 1),
                            text: "Height 1.0 Text:" + text),
                        new TextSpan(style: new TextStyle(height: 1.2f),
                            text: "Height 1.2 Text:" + text),
                        new TextSpan(style: new TextStyle(height: 1.5f),
                            text: "Height 1.5 Text:" + text),
                    })), width: 300, height: 300);
        }
    }
}