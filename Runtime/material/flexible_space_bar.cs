using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.material {
    public enum CollapseMode {
        parallax,
        pin,
        none
    }

    public class FlexibleSpaceBar : StatefulWidget {
        public FlexibleSpaceBar(
            Key key = null,
            Widget title = null,
            Widget background = null,
            bool? centerTitle = null,
            EdgeInsets titlePadding = null,
            CollapseMode collapseMode = CollapseMode.parallax
        ) : base(key: key) {
            this.title = title;
            this.background = background;
            this.centerTitle = centerTitle;
            this.titlePadding = titlePadding;
            this.collapseMode = collapseMode;
        }

        public readonly Widget title;

        public readonly Widget background;

        public readonly bool? centerTitle;
        
        public readonly CollapseMode collapseMode;

        public readonly EdgeInsets titlePadding;

        public static Widget createSettings(
            float? toolbarOpacity = null,
            float? minExtent = null,
            float? maxExtent = null,
            float? currentExtent = null,
            Widget child = null) {
            D.assert(currentExtent != null);
            D.assert(child != null);
            return new FlexibleSpaceBarSettings(
                toolbarOpacity: toolbarOpacity ?? 1.0f,
                minExtent: minExtent ?? currentExtent,
                maxExtent: maxExtent ?? currentExtent,
                currentExtent: currentExtent,
                child: child
            );
        }

        public override State createState() {
            return new _FlexibleSpaceBarState();
        }
    }


    class _FlexibleSpaceBarState : State<FlexibleSpaceBar> {
        bool? _getEffectiveCenterTitle(ThemeData themeData) {
            if (this.widget.centerTitle != null) {
                return this.widget.centerTitle;
            }

            switch (themeData.platform) {
                case RuntimePlatform.IPhonePlayer:
                    return true;
                default:
                    return false;
            }
        }


        Alignment _getTitleAlignment(bool effectiveCenterTitle) {
            if (effectiveCenterTitle) {
                return Alignment.bottomCenter;
            }

            return Alignment.bottomLeft;
        }

        float? _getCollapsePadding(float t, FlexibleSpaceBarSettings settings) {
            switch (this.widget.collapseMode) {
                case CollapseMode.pin:
                    return -(settings.maxExtent.Value - settings.currentExtent.Value);
                case CollapseMode.none:
                    return 0.0f;
                case CollapseMode.parallax:
                    float deltaExtent = settings.maxExtent.Value - settings.minExtent.Value;
                    return -new FloatTween(begin: 0.0f, end: deltaExtent / 4.0f).lerp(t);
            }

            return null;
        }

        public override Widget build(BuildContext context) {
            FlexibleSpaceBarSettings settings =
                (FlexibleSpaceBarSettings) context.inheritFromWidgetOfExactType(typeof(FlexibleSpaceBarSettings));
            D.assert(settings != null,
                () => "A FlexibleSpaceBar must be wrapped in the widget returned by FlexibleSpaceBar.createSettings().");

            List<Widget> children = new List<Widget>();
            float deltaExtent = settings.maxExtent.Value - settings.minExtent.Value;

            float t = (1.0f - (settings.currentExtent.Value - settings.minExtent.Value) / deltaExtent)
                .clamp(0.0f, 1.0f);

            if (this.widget.background != null) {
                float fadeStart = Mathf.Max(0.0f, 1.0f - Constants.kToolbarHeight / deltaExtent);
                float fadeEnd = 1.0f;
                D.assert(fadeStart <= fadeEnd);

                float opacity = 1.0f - new Interval(fadeStart, fadeEnd).transform(t);
                if (opacity > 0.0f) {
                    children.Add(new Positioned(
                            top: this._getCollapsePadding(t, settings),
                            left: 0.0f,
                            right: 0.0f,
                            height: settings.maxExtent,
                            child: new Opacity(
                                opacity: opacity,
                                child: this.widget.background)
                        )
                    );
                }
            }

            Widget title = null;
            if (this.widget.title != null) {
                switch (Application.platform) {
                    case RuntimePlatform.IPhonePlayer:
                        title = this.widget.title;
                        break;
                    default:
                        title = this.widget.title;
                        break;
                }
            }

            ThemeData theme = Theme.of(context);
            float toolbarOpacity = settings.toolbarOpacity.Value;
            if (toolbarOpacity > 0.0f) {
                TextStyle titleStyle = theme.primaryTextTheme.title;
                titleStyle = titleStyle.copyWith(
                    color: titleStyle.color.withOpacity(toolbarOpacity));

                bool effectiveCenterTitle = this._getEffectiveCenterTitle(theme).Value;
                EdgeInsets padding = this.widget.titlePadding ??
                    EdgeInsets.only(
                        left: effectiveCenterTitle ? 0.0f : 72.0f,
                        bottom: 16.0f
                    );
                float scaleValue = new FloatTween(begin: 1.5f, end: 1.0f).lerp(t);
                Matrix3 scaleTransform = Matrix3.makeScale(scaleValue, scaleValue);
                Alignment titleAlignment = this._getTitleAlignment(effectiveCenterTitle);

                children.Add(new Container(
                        padding: padding,
                        child: new Transform(
                            alignment: titleAlignment,
                            transform: scaleTransform,
                            child: new Align(
                                alignment: titleAlignment,
                                child: new DefaultTextStyle(
                                    style: titleStyle,
                                    child: title)
                            )
                        )
                    )
                );
            }

            return new ClipRect(
                child: new Stack(
                    children: children)
            );
        }
    }


    public class FlexibleSpaceBarSettings : InheritedWidget {
        public FlexibleSpaceBarSettings(
            Key key = null,
            float? toolbarOpacity = null,
            float? minExtent = null,
            float? maxExtent = null,
            float? currentExtent = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(currentExtent != null);
            D.assert(child != null);
            this.toolbarOpacity = toolbarOpacity;
            this.minExtent = minExtent;
            this.maxExtent = maxExtent;
            this.currentExtent = currentExtent;
        }

        public readonly float? toolbarOpacity;

        public readonly float? minExtent;

        public readonly float? maxExtent;

        public readonly float? currentExtent;


        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            FlexibleSpaceBarSettings _oldWidget = (FlexibleSpaceBarSettings) oldWidget;
            return this.toolbarOpacity != _oldWidget.toolbarOpacity
                   || this.minExtent != _oldWidget.minExtent
                   || this.maxExtent != _oldWidget.maxExtent
                   || this.currentExtent != _oldWidget.currentExtent;
        }
    }
}