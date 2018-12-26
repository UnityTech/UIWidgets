using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public abstract class Decoration : Diagnosticable {
        protected Decoration() {
        }

        public override string toStringShort() {
            return this.GetType().ToString();
        }

        public virtual bool debugAssertIsValid() {
            return true;
        }

        public virtual EdgeInsets padding {
            get { return EdgeInsets.zero; }
        }

        public virtual bool isComplex {
            get { return false; }
        }

        public virtual bool hitTest(Size size, Offset position) {
            return true;
        }

        public abstract BoxPainter createBoxPainter(VoidCallback onChanged = null);
    }

    public abstract class BoxPainter {
        protected BoxPainter(VoidCallback onChanged = null) {
            this.onChanged = onChanged;
        }

        public readonly VoidCallback onChanged;

        public abstract void paint(Canvas canvas, Offset offset, ImageConfiguration configuration);

        public virtual void dispose() {
        }
    }
}