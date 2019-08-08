using System.Collections.Generic;
using System.Text;

namespace Unity.UIWidgets.ui {
    public class ParagraphBuilder {
        StringBuilder _text = new StringBuilder();
        ParagraphStyle _paragraphStyle;
        StyledRuns _runs = new StyledRuns();
        List<int> _styleStack = new List<int>();
        int _paragraph_style_index;

        public ParagraphBuilder(ParagraphStyle style) {
            this.setParagraphStyle(style);
        }

        public Paragraph build() {
            this._runs.endRunIfNeeded(this._text.Length);
            var paragraph = Paragraph.create();
            paragraph.setText(this._text.ToString(), this._runs);
            paragraph.setParagraphStyle(this._paragraphStyle);
            return paragraph;
        }

        public void pushStyle(painting.TextStyle style, float textScaleFactor) {
            var newStyle = TextStyle.applyStyle(this.peekStyle(), style, textScaleFactor: textScaleFactor);
            var styleIndex = this._runs.addStyle(newStyle);
            this._styleStack.Add(styleIndex);
            this._runs.startRun(styleIndex, this._text.Length);
        }

        internal void pushStyle(TextStyle style) {
            var styleIndex = this._runs.addStyle(style);
            this._styleStack.Add(styleIndex);
            this._runs.startRun(styleIndex, this._text.Length);
        }

        public void pop() {
            var lastIndex = this._styleStack.Count - 1;
            if (lastIndex < 0) {
                return;
            }

            this._styleStack.RemoveAt(lastIndex);
            this._runs.startRun(this.peekStyleIndex(), this._text.Length);
        }

        public void addText(string text) {
            this._text.Append(text);
        }

        internal TextStyle peekStyle() {
            return this._runs.getStyle(this.peekStyleIndex());
        }


        public int peekStyleIndex() {
            int count = this._styleStack.Count;
            if (count > 0) {
                return this._styleStack[count - 1];
            }

            return this._paragraph_style_index;
        }

        void setParagraphStyle(ParagraphStyle style) {
            this._paragraphStyle = style;
            this._paragraph_style_index = this._runs.addStyle(style.getTextStyle());
            this._runs.startRun(this._paragraph_style_index, this._text.Length);
        }
    }
}