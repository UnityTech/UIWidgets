using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgets.Tests {
    public class MouseHoverWidget : StatefulWidget {
        public MouseHoverWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new _MouseHoverWidgetState();
        }
    }

    class _MouseHoverWidgetState : State<MouseHoverWidget> {
        public static Widget createRow(bool canHover = true, bool nest = false) {
            Widget result = new Container(width: 200, height: 60, color: Color.fromARGB(255, 255, 0, 255));
            if (canHover) {
                result = new HoverTrackWidget(null,
                    result, "inner");
            }

            //WARNING: nested MouseTracker is not supported by the current implementation that ported from flutter
            //refer to this issue https://github.com/flutter/flutter/issues/28407 and wait Google guys fixing it
            /*
            if (nest) {
                result = new Container(child: result, padding: EdgeInsets.all(40),
                    color: Color.fromARGB(255, 255, 0, 0));
                result = new HoverTrackWidget(null,
                    result, "outer");
            }
            */

            return result;
        }

        public override Widget build(BuildContext context) {
            //1 131231
            return new Container(
                alignment: Alignment.center, color: Color.fromARGB(255, 0, 255, 0),
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.spaceAround,
                    children: new List<Widget> {
                        createRow(),
                        createRow(false),
                        createRow(),
                        createRow(true, true),
                    }));
        }
    }

    public class HoverTrackWidget : StatefulWidget {
        public readonly Widget child;
        public readonly string name;

        public HoverTrackWidget(Key key, Widget child, string name) : base(key) {
            this.child = child;
            this.name = name;
        }

        public override State createState() {
            return new _HoverTrackWidgetState();
        }
    }

    class _HoverTrackWidgetState : State<HoverTrackWidget> {
        bool hover;

        public override Widget build(BuildContext context) {
            return new Listener(child:
                new Container(
                    forgroundDecoration: this.hover
                        ? new BoxDecoration(color: Color.fromARGB(80, 255, 255, 255))
                        : null,
                    child: this.widget.child
                ),
                onPointerEnter: (evt) => {
                    if (this.mounted) {
                        Debug.Log(this.widget.name + " pointer enter");
                        this.setState(() => { this.hover = true; });
                    }
                },
                onPointerExit: (evt) => {
                    if (this.mounted) {
                        Debug.Log(this.widget.name + " pointer exit");
                        this.setState(() => { this.hover = false; });
                    }
                }
            );
        }
    }
}