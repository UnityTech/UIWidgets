using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class BackButtonIcon : StatelessWidget {
        public BackButtonIcon(
            Key key = null) : base(key: key) {
        }

        static IconData _getIconData(RuntimePlatform platform) {
            switch (platform) {
                case RuntimePlatform.IPhonePlayer:
                    return Icons.arrow_back_ios;
                default:
                    return Icons.arrow_back;
            }
        }

        public override Widget build(BuildContext context) {
            return new Icon(_getIconData(Theme.of(context).platform));
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
                tooltip: MaterialLocalizations.of(context).backButtonTooltip,
                onPressed: () => { Navigator.maybePop(context); });
        }
    }

    public class CloseButton : StatelessWidget {
        public CloseButton(
            Key key = null) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            return new IconButton(
                icon: new Icon(Icons.close),
                tooltip: MaterialLocalizations.of(context).closeButtonTooltip,
                onPressed: () => { Navigator.maybePop(context); });
        }
    }
}