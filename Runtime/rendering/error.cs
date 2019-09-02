using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public class RenderErrorBox : RenderBox {
        const string _kLine = "\n\n────────────────────\n\n";

        public RenderErrorBox(string message = "") {
            this.message = message;
            if (message == "") {
                return;
            }

            ParagraphBuilder builder = new ParagraphBuilder(paragraphStyle);
            builder.pushStyle(textStyle);
            builder.addText(
                $"{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}{message}{_kLine}"
            );
            this._paragraph = builder.build();
        }

        string message;
        Paragraph _paragraph;

        static TextStyle textStyle = new TextStyle(
            color: new Color(0xFFFFFF66),
            fontFamily: "monospace",
            fontSize: 14.0f,
            fontWeight: FontWeight.w700
        );

        static ParagraphStyle paragraphStyle = new ParagraphStyle(
            height: 1.0f
        );
    }
}