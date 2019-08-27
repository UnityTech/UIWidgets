using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class BottomAppBarUtils {
        public const float _kTabBarHeight = 50.0f;
        public static readonly Color _kDefaultTabBarBorderColor = new Color(0x4C000000);
    }


    public class CupertinoTabBar : StatelessWidget {
        public CupertinoTabBar(
            Key key = null,
            List<BottomNavigationBarItem> items = null,
            ValueChanged<int> onTap = null,
            int currentIndex = 0,
            Color backgroundColor = null,
            Color activeColor = null,
            Color inactiveColor = null,
            float iconSize = 30.0f,
            Border border = null
        ) : base(key: key) {
            D.assert(items != null);
            D.assert(items.Count >= 2,
                () => "Tabs need at least 2 items to conform to Apple's HIG"
            );
            D.assert(0 <= currentIndex && currentIndex < items.Count);
            

            this.items = items;
            this.onTap = onTap;
            this.currentIndex = currentIndex;

            this.backgroundColor = backgroundColor;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor ?? CupertinoColors.inactiveGray;
            this.iconSize = iconSize;
            this.border = border ?? new Border(
                              top: new BorderSide(
                                  color: BottomAppBarUtils._kDefaultTabBarBorderColor,
                                  width: 0.0f, // One physical pixel.
                                  style: BorderStyle.solid
                              )
                          );
        }

        public readonly List<BottomNavigationBarItem> items;

        public readonly ValueChanged<int> onTap;

        public readonly int currentIndex;

        public readonly Color backgroundColor;

        public readonly Color activeColor;

        public readonly Color inactiveColor;

        public readonly float iconSize;

        public readonly Border border;

        public Size preferredSize {
            get { return Size.fromHeight(BottomAppBarUtils._kTabBarHeight); }
        }

        public bool opaque(BuildContext context) {
            Color backgroundColor =
                this.backgroundColor ?? CupertinoTheme.of(context).barBackgroundColor;
            return backgroundColor.alpha == 0xFF;
        }

        public override Widget build(BuildContext context) {
            float bottomPadding = MediaQuery.of(context).padding.bottom;

            Widget result = new DecoratedBox(
                decoration: new BoxDecoration(
                    border: this.border,
                    color: this.backgroundColor ?? CupertinoTheme.of(context).barBackgroundColor
                ),
                child: new SizedBox(
                    height: BottomAppBarUtils._kTabBarHeight + bottomPadding,
                    child: IconTheme.merge( // Default with the inactive state.
                        data: new IconThemeData(
                            color: this.inactiveColor,
                            size: this.iconSize
                        ),
                        child: new DefaultTextStyle( // Default with the inactive state.
                            style: CupertinoTheme.of(context).textTheme.tabLabelTextStyle
                                .copyWith(color: this.inactiveColor),
                            child: new Padding(
                                padding: EdgeInsets.only(bottom: bottomPadding),
                                child: new Row(
                                    crossAxisAlignment: CrossAxisAlignment.end,
                                    children: this._buildTabItems(context)
                                )
                            )
                        )
                    )
                )
            );

            if (!this.opaque(context)) {
                result = new ClipRect(
                    child: new BackdropFilter(
                        filter: ImageFilter.blur(sigmaX: 10.0f, sigmaY: 10.0f),
                        child: result
                    )
                );
            }

            return result;
        }

        List<Widget> _buildTabItems(BuildContext context) {
            List<Widget> result = new List<Widget> { };

            for (int index = 0; index < this.items.Count; index += 1) {
                bool active = index == this.currentIndex;
                var tabIndex = index;
                result.Add(
                    this._wrapActiveItem(
                        context,
                        new Expanded(
                            child: new GestureDetector(
                                behavior: HitTestBehavior.opaque,
                                onTap: this.onTap == null ? null : (GestureTapCallback) (() => { this.onTap(tabIndex); }),
                                child: new Padding(
                                    padding: EdgeInsets.only(bottom: 4.0f),
                                    child: new Column(
                                        mainAxisAlignment: MainAxisAlignment.end,
                                        children: this._buildSingleTabItem(this.items[index], active)
                                    )
                                )
                            )
                        ),
                        active: active
                    )
                );
            }

            return result;
        }

        List<Widget> _buildSingleTabItem(BottomNavigationBarItem item, bool active) {
            List<Widget> components = new List<Widget> {
                new Expanded(
                    child: new Center(child: active ? item.activeIcon : item.icon)
                )
            };

            if (item.title != null) {
                components.Add(item.title);
            }

            return components;
        }

        Widget _wrapActiveItem(BuildContext context, Widget item, bool active) {
            if (!active) {
                return item;
            }

            Color activeColor = this.activeColor ?? CupertinoTheme.of(context).primaryColor;
            return IconTheme.merge(
                data: new IconThemeData(color: activeColor),
                child: DefaultTextStyle.merge(
                    style: new TextStyle(color: activeColor),
                    child: item
                )
            );
        }

        public CupertinoTabBar copyWith(
            Key key = null,
            List<BottomNavigationBarItem> items = null,
            Color backgroundColor = null,
            Color activeColor = null,
            Color inactiveColor = null,
            float? iconSize = null,
            Border border = null,
            int? currentIndex = null,
            ValueChanged<int> onTap = null
        ) {
            return new CupertinoTabBar(
                key: key ?? this.key,
                items: items ?? this.items,
                backgroundColor: backgroundColor ?? this.backgroundColor,
                activeColor: activeColor ?? this.activeColor,
                inactiveColor: inactiveColor ?? this.inactiveColor,
                iconSize: iconSize ?? this.iconSize,
                border: border ?? this.border,
                currentIndex: currentIndex ?? this.currentIndex,
                onTap: onTap ?? this.onTap
            );
        }
    }
}