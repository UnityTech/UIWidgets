using UIWidgets.foundation;

namespace UIWidgets.rendering {
    public enum ScrollDirection {
        idle,
        forward,
        reverse,
    }
    
    public abstract class ViewportOffset : ChangeNotifier {
        protected ViewportOffset() {
        }

        public static ViewportOffset @fixed(double value) {
            return null;
        }

        public static ViewportOffset zero() {
            return null;
        }

        public abstract double pixels { get; }
        public abstract bool applyViewportDimension(double viewportDimension);
        public abstract bool applyContentDimensions(double minScrollExtent, double maxScrollExtent);

        public abstract void correctBy(double correction);
        public abstract void jumpTo(double pixels);
    }
}