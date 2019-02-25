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

namespace UIWidgets.Tests {
    public class RenderBoxes : EditorWindow {
        readonly Func<RenderBox>[] _options;

        readonly string[] _optionStrings;

        int _selected;

        RenderBoxes() {
            this._options = new Func<RenderBox>[] {
                this.decoratedBox,
                this.decoratedShape,
                this.flex,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("RenderBoxes");
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

        RenderBox decoratedBox() {
            return new RenderConstrainedOverflowBox(
                minWidth: 100,
                maxWidth: 100,
                minHeight: 100,
                maxHeight: 100,
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFFFF00FF),
                        borderRadius: BorderRadius.all(15),
                        boxShadow: new List<BoxShadow> {
                            new BoxShadow(
                                color: new Color(0xFFFF00FF),
                                offset: new Offset(0, 0),
                                blurRadius: 3.0f,
                                spreadRadius: 10
                            )
                        },
                        image: new DecorationImage(
                            image: new NetworkImage(
                                url:
                                "https://sg.fiverrcdn.com/photos/4665137/original/39322-140411095619534.jpg?1424268945"
                            ),
                            fit: BoxFit.cover)
                    )
                )
            );
        }

        RenderBox decoratedShape() {
            return new RenderConstrainedOverflowBox(
                minWidth: 100,
                maxWidth: 100,
                minHeight: 100,
                maxHeight: 100,
                child: new RenderDecoratedBox(
                    decoration: new ShapeDecoration(
                        color: new Color(0xFFFF00FF),
                        shape: new BeveledRectangleBorder(
                            new BorderSide(width: 5, color: Color.white),
                            BorderRadius.circular(5)),
                        image: new DecorationImage(
                            image: new NetworkImage(
                                url:
                                "https://sg.fiverrcdn.com/photos/4665137/original/39322-140411095619534.jpg?1424268945"
                            ),
                            fit: BoxFit.cover)
                    )
                )
            );
        }


        RenderBox flex() {
            var flexbox = new RenderFlex(
                direction: Axis.horizontal,
                crossAxisAlignment: CrossAxisAlignment.center);

            flexbox.add(new RenderConstrainedBox(
                additionalConstraints: new BoxConstraints(minWidth: 300, minHeight: 200),
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFF00FF00)
                    )
                )));

            flexbox.add(new RenderConstrainedBox(
                additionalConstraints: new BoxConstraints(minWidth: 100, minHeight: 300),
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFF00FFFF)
                    )
                )));

            flexbox.add(new RenderConstrainedBox(
                additionalConstraints: new BoxConstraints(minWidth: 50, minHeight: 100),
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFF0000FF)
                    )
                )));


            return flexbox;
        }
    }
}