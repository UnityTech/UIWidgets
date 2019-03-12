using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class DrawerHeaderUtils {
        public const float _kDrawerHeaderHeight = 160.0f + 1.0f;
    }


    public class DrawerHeader : StatelessWidget {
        public DrawerHeader(
            Key key = null,
            Decoration decoration = null,
            EdgeInsets margin = null,
            EdgeInsets padding = null,
            TimeSpan? duration = null,
            Curve curve = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.decoration = decoration;
            this.margin = margin ?? EdgeInsets.only(bottom: 8.0f);
            this.padding = padding ?? EdgeInsets.fromLTRB(16.0f, 16.0f, 16.0f, 8.0f);
            this.duration = duration ?? new TimeSpan(0, 0, 0, 0, 250);
            this.curve = curve ?? Curves.fastOutSlowIn;
            this.child = child;
        }


        public readonly Decoration decoration;

        public readonly EdgeInsets padding;

        public readonly EdgeInsets margin;

        public readonly TimeSpan duration;

        public readonly Curve curve;

        public readonly Widget child;


        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            ThemeData theme = Theme.of(context);
            float statusBarHeight = MediaQuery.of(context).padding.top;
            return new Container(
                height: statusBarHeight + DrawerHeaderUtils._kDrawerHeaderHeight,
                margin: this.margin,
                decoration: new BoxDecoration(
                    border: new Border(
                        bottom: Divider.createBorderSide(context)
                    )
                ),
                child: new AnimatedContainer(
                    padding: this.padding.add(EdgeInsets.only(top: statusBarHeight)),
                    decoration: this.decoration,
                    duration: this.duration,
                    curve: this.curve,
                    child: this.child == null
                        ? null
                        : new DefaultTextStyle(
                            style: theme.textTheme.body2,
                            child: MediaQuery.removePadding(
                                context: context,
                                removeTop: true,
                                child: this.child)
                        )
                )
            );
        }
    }
}