using System.Collections.Generic;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class TextSpanGesture : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new WidgetsApp(
                home: new BuzzingText(),
                pageRouteBuilder: this.pageRouteBuilder);
        }
    }

    class BuzzingText : StatefulWidget {
        public override State createState() {
            return new _BuzzingTextState();
        }
    }

    class _BuzzingTextState : State<BuzzingText> {
        TapGestureRecognizer _tapRecognizer;
        HoverRecognizer _hoverRecognizer;

        public override void initState() {
            base.initState();
            this._tapRecognizer = new TapGestureRecognizer();
            this._tapRecognizer.onTap = () => {
                Debug.Log("Tap");
            };
            this._hoverRecognizer = new HoverRecognizer();
            this._hoverRecognizer.OnPointerEnter = (evt) => { Debug.Log("Pointer Enter"); };
            this._hoverRecognizer.OnPointerLeave = () => { Debug.Log("Pointer Leave"); };
        }

        public override void dispose() {
            this._tapRecognizer.dispose();
            base.dispose();
        }

        void _handleEnter() {
            Debug.Log("Enter");
        }

        void _handleLeave() {
            Debug.Log("Leave");
        }
        /*
        
        Any professional looking app you have seen probably has multiple screens in it. It can contain a welcome screen, a login screen and then further screens. 
        */

        public override Widget build(BuildContext context) {
            return new RichText(
                text: new TextSpan(
                    text: "Can you ",
                    style: new TextStyle(color: Colors.black),
                    children: new List<TextSpan>() {
                        new TextSpan(
                            text: "find the",
                            style: new TextStyle(
                                color: Colors.green,
                                decoration: TextDecoration.underline
                            ),
                            recognizer: this._tapRecognizer,
                            hoverRecognizer: this._hoverRecognizer
                        ),
                        new TextSpan(
                            text: " secret?"
                        )
                    }
                ));
        }
    }
}