namespace Unity.UIWidgets.ui {
    public struct uiOffset {
        public uiOffset(float dx, float dy) {
            this.dx = dx;
            this.dy = dy;
        }

        public readonly float dx;
        public readonly float dy;

        public static uiOffset? fromOffset(Offset offset) {
            if (offset == null) {
                return null;
            }

            var newOffset = new uiOffset(offset.dx, offset.dy);
            return newOffset;
        }


        public static uiOffset operator -(uiOffset a) {
            return new uiOffset(-a.dx, -a.dy);
        }
    }
}