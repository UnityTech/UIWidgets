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
        LongPressGestureRecognizer _longPressRecognizer;

        public override void initState() {
            base.initState();
            this._longPressRecognizer = new LongPressGestureRecognizer();
            this._longPressRecognizer.onLongPress = this._handlePress;
        }

        public override void dispose() {
            this._longPressRecognizer.dispose();
            base.dispose();
        }

        void _handlePress() {
            Debug.Log("Long Pressed Text");
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
                            )
                            //recognizer: this._longPressRecognizer
                        ),
                        new TextSpan(
                            text: " secret?"
                        )
                    }
                ));
        }
    }
}