using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public class RenderErrorBox : RenderBox {
        const double _kMaxWidth = 100000.0;
        const double _kMaxHeight = 100000.0;
        const string _kLine = "\n\n────────────────────\n\n";

        public RenderErrorBox(string message = "") {
            this.message = message;
            if (message == "") return;
            ui.ParagraphBuilder builder = new ui.ParagraphBuilder(paragraphStyle);
            builder.pushStyle(textStyle);
            builder.addText(
                string.Format(
                    "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}{20}{21}{22}{23}",
                    message, _kLine, message, _kLine, message, _kLine, message, _kLine, message, _kLine, message,
                    _kLine, message, _kLine, message, _kLine, message, _kLine, message, _kLine, message, _kLine,
                    message, _kLine)
            );
            _paragraph = builder.build();
        }

        private string message;
        ui.Paragraph _paragraph;

        static ui.TextStyle textStyle = new ui.TextStyle(
            color: new ui.Color(0xFFFFFF66),
            fontFamily: "monospace",
            fontSize: 14.0,
            fontWeight: FontWeight.w700
        );

        static ui.ParagraphStyle paragraphStyle = new ui.ParagraphStyle(
            lineHeight: 1.0
        );
    }
}