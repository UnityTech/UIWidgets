using System.Collections.Generic;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public abstract class AnimatedIcons {
        public static readonly AnimatedIconData add_event = AnimatedIconsData._add_event;

        public static readonly AnimatedIconData arrow_menu = AnimatedIconsData._arrow_menu;

        public static readonly AnimatedIconData close_menu = AnimatedIconsData._close_menu;

        // public readonly AnimatedIconData ellipsis_search = AnimatedIconsData._ellipsis_search;

        // public readonly AnimatedIconData event_add = AnimatedIconsData._event_add;

        // public readonly AnimatedIconData home_menu = AnimatedIconsData._home_menu;

        // public readonly AnimatedIconData list_view = AnimatedIconsData._list_view;

        // public readonly AnimatedIconData menu_arrow = AnimatedIconsData._menu_arrow;

        // public readonly AnimatedIconData menu_close = AnimatedIconsData._menu_close;

        // public readonly AnimatedIconData menu_home = AnimatedIconsData._menu_home;

        // public readonly AnimatedIconData pause_play = AnimatedIconsData._pause_play;

        // public readonly AnimatedIconData play_pause = AnimatedIconsData._play_pause;

        // public readonly AnimatedIconData search_ellipsis = AnimatedIconsData._search_ellipsis;

        // public readonly AnimatedIconData view_list = AnimatedIconsData._view_list;
    }

    public abstract class AnimatedIconData {
        public AnimatedIconData() {
        }

        public abstract bool matchTextDirection { get; }
    }

    class _AnimatedIconData : AnimatedIconData {
        public _AnimatedIconData(Size size, List<_PathFrames> paths, bool matchTextDirection = false) {
            this.size = size;
            this.paths = paths;
            this.matchTextDirection = matchTextDirection;
        }

        public readonly Size size;
        public readonly List<_PathFrames> paths;

        public override bool matchTextDirection { get; }
    }
}