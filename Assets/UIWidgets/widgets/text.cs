using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.ui;
using TextStyle = UIWidgets.painting.TextStyle;

namespace UIWidgets.widgets {
    public class Text : StatelessWidget {
        public Text(string data,
            Key key = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            double? textScaleFactor = null,
            int? maxLines = null) : base(key: key) {
            D.assert(data != null);

            this.data = data;
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
            this.textSpan = null;
        }

        public readonly string data;

        public readonly TextSpan textSpan;

        public readonly TextStyle style;

        public readonly TextAlign? textAlign;

        public readonly bool? softWrap;

        public readonly TextOverflow? overflow;

        public readonly double? textScaleFactor;

        public readonly int? maxLines;

        public override Widget build(BuildContext context) {
            TextStyle effectiveTextStyle = this.style;
            Widget result = new RichText(
                textAlign: this.textAlign ?? TextAlign.left,
                softWrap: this.softWrap ?? false,
                overflow: this.overflow ?? TextOverflow.clip,
                textScaleFactor: this.textScaleFactor ?? 1.0,
                maxLines: this.maxLines ?? null,
                text: new TextSpan(
                    style: effectiveTextStyle,
                    text: this.data,
                    children: this.textSpan != null ? new List<TextSpan> {this.textSpan} : null
                )
            );
            return result;
        }
    }
}