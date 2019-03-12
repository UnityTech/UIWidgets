using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public interface IPreferredSizeWidget {
        Size preferredSize { get; }
    }

    public abstract class PreferredSizeWidget : StatefulWidget, IPreferredSizeWidget {
        protected PreferredSizeWidget(Key key = null) : base(key: key) {
        }

        public virtual Size preferredSize { get; }
    }


    public class PreferredSize : StatelessWidget, IPreferredSizeWidget {
        public PreferredSize(
            Key key = null,
            Widget child = null,
            Size preferredSize = null) : base(key: key) {
            D.assert(child != null);
            D.assert(preferredSize != null);
            this.child = child;
            this.preferredSize = preferredSize;
        }

        public readonly Widget child;

        public Size preferredSize { get; }

        public override Widget build(BuildContext context) {
            return this.child;
        }
    }
}