namespace UIWidgets.painting {
    public abstract class EdgeInsetsGeometry {
        protected EdgeInsetsGeometry() {
        }
    }


    public class EdgeInsets : EdgeInsetsGeometry {
        private EdgeInsets() {
        }

        public static EdgeInsets only() {
            return new EdgeInsets();
        }

        public static readonly EdgeInsets zero = EdgeInsets.only();
    }
}