using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.painting {
    public abstract class Decoration {
        protected Decoration() {
        }

        public virtual EdgeInsets padding {
            get { return EdgeInsets.zero; }
        }

        public abstract BoxPainter createBoxPainter(VoidCallback onChanged = null);
    }
    
    public abstract class BoxPainter {
        protected BoxPainter(VoidCallback onChanged = null) {
            this.onChanged = onChanged;
        }

        public readonly VoidCallback onChanged;

        public abstract void paint(Canvas canvas, Offset offset, ImageConfiguration configuration);

        public void dispose() {
        }
    }
}