using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class ExpandIcon : StatefulWidget {
        public ExpandIcon(
            Key key = null,
            bool isExpanded = false,
            double size = 24.0,
            ValueChanged<bool> onPressed = null,
            EdgeInsets padding = null) : base(key: key) {
            this.isExpanded = isExpanded;
            this.size = size;
            this.onPressed = onPressed;
            this.padding = padding ?? EdgeInsets.all(8.0);
        }

        public readonly bool isExpanded;

        public readonly double size;

        public readonly ValueChanged<bool> onPressed;

        public readonly EdgeInsets padding;

        public override State createState() {
            return new _ExpandIconState();
        }
    }


    public class _ExpandIconState : SingleTickerProviderStateMixin<ExpandIcon> {
        AnimationController _controller;
        Animation<double> _iconTurns;

        static readonly Animatable<double> _iconTurnTween =
            new DoubleTween(begin: 0.0, end: 0.5).chain(new CurveTween(curve: Curves.fastOutSlowIn));

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(duration: ThemeUtils.kThemeAnimationDuration, vsync: this);
            this._iconTurns = this._controller.drive(_iconTurnTween);
            if (this.widget.isExpanded) {
                this._controller.setValue(Math.PI);
            }
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }


        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            ExpandIcon _oldWidget = (ExpandIcon) oldWidget;
            if (this.widget.isExpanded != _oldWidget.isExpanded) {
                if (this.widget.isExpanded) {
                    this._controller.forward();
                }
                else {
                    this._controller.reverse();
                }
            }
        }


        void _handlePressed() {
            if (this.widget.onPressed != null) {
                this.widget.onPressed(this.widget.isExpanded);
            }
        }


        public override Widget build(BuildContext context) {
            D.assert(MaterialDebug.debugCheckHasMaterial(context));
            ThemeData theme = Theme.of(context);
            return new IconButton(
                padding: this.widget.padding,
                color: theme.brightness == Brightness.dark ? Colors.white54 : Colors.black54,
                onPressed: this.widget.onPressed == null ? (VoidCallback) null : this._handlePressed,
                icon: new RotationTransition(
                    turns: this._iconTurns,
                    child: new Icon(Icons.expand_more))
            );
        }
    }
}