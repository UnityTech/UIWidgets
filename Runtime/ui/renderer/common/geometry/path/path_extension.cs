namespace Unity.UIWidgets.ui {
    public partial class uiPath {

        public enum uiPathShapeHint {
            Rect,
            Oval,
            RRect,
            Other
        }

        public bool isRect {
            get { return this._shapeHint == uiPathShapeHint.Rect; }
        }

        public bool isOval {
            get { return this._shapeHint == uiPathShapeHint.Oval; }
        }
    }
}