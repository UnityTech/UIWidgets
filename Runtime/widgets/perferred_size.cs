using System.Drawing;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    
    public interface PreferredSizeWidget {
        Size preferredSize { get; }
    }


    public class PreferredSize : StatelessWidget, PreferredSizeWidget {
        public PreferredSize(
            Key key = null,
            Widget child = null,
            Size? preferredSize = null) : base(key: key) {
            D.assert(child != null);
            D.assert(preferredSize != null);
            this.child = child;
            this.preferredSize = preferredSize ?? Size.Empty;
        }

        public readonly Widget child;

        public Size preferredSize { get; }

        public override Widget build(BuildContext context) {
            return this.child;
        }
    }
}