using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class BottomNavigationBarItem {
        public BottomNavigationBarItem(
            Widget icon = null,
            Widget title = null,
            Widget activeIcon = null,
            Color backgroundColor = null
        ) {
            D.assert(icon != null);
            this.icon = icon;
            this.activeIcon = activeIcon ?? icon;
            this.title = title;
            this.backgroundColor = backgroundColor;
        }

        public readonly Widget icon;

        public readonly Widget activeIcon;

        public readonly Widget title;

        public readonly Color backgroundColor;
    }
}