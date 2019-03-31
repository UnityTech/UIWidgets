using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public interface SizePreferred {
        Size preferredSize { get; }
    }

    public abstract class PreferredSizeWidget : StatefulWidget, SizePreferred {
        protected PreferredSizeWidget(Key key = null) : base(key: key) {
        }

        public virtual Size preferredSize { get; }
    }


    public class PreferredSize : PreferredSizeWidget {
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

        public override Size preferredSize { get; }

        public override State createState() {
            return new _PreferredSizeState();
        }
    }

    class _PreferredSizeState : State<PreferredSize> {
        public override Widget build(BuildContext context) {
            return this.widget.child;
        }
    }
}