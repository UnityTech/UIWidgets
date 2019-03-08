using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class BackButtonIcon : StatelessWidget {
        public BackButtonIcon(
            Key key = null) : base(key: key) {
        }

        static IconData _getIconData() {
            return Icons.arrow_back;
        }

        public override Widget build(BuildContext context) {
            return new Icon(_getIconData());
        }
    }

    public class BackButton : StatelessWidget {
        public BackButton(
            Key key = null,
            Color color = null) : base(key: key) {
            this.color = color;
        }

        public readonly Color color;

        public override Widget build(BuildContext context) {
            return new IconButton(
                icon: new BackButtonIcon(),
                color: this.color,
                onPressed: () => { Navigator.maybePop(context); });
        }
    }

    public class CloseButton : StatelessWidget {
        public CloseButton(
            Key key = null) : base(key: key) {
        }
        
        public override Widget build(BuildContext context) {
            return new IconButton(
                icon: new Icon(Icons.close),
                onPressed: () => { Navigator.maybePop(context); });
        }
    }
}