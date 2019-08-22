namespace Unity.UIWidgets.ui {
    public partial class uiPath {

        public enum uiPathShapeHint {
            Rect,
            Circle,
            NaiveRRect,
            Other
        }

        public bool isRect {
            get { return this._shapeHint == uiPathShapeHint.Rect; }
        }

        public bool isCircle {
            get { return this._shapeHint == uiPathShapeHint.Circle; }
        }
    }
}