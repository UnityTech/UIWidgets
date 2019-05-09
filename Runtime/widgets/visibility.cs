using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class Visibility : StatelessWidget {
        public Visibility(
            Key key = null,
            Widget child = null,
            Widget replacement = null,
            bool visible = true,
            bool maintainState = false,
            bool maintainAnimation = false,
            bool maintainSize = false,
            bool maintainInteractivity = false
        ) : base(key: key) {
            D.assert(child != null);
            D.assert(maintainState == true || maintainAnimation == false,
                () => "Cannot maintain animations if the state is not also maintained.");
            D.assert(maintainAnimation == true || maintainSize == false,
                () => "Cannot maintain size if animations are not maintained.");
            D.assert(maintainSize == true || maintainInteractivity == false,
                () => "Cannot maintain interactivity if size is not maintained.");
            this.replacement = replacement ?? SizedBox.shrink();
            this.child = child;
            this.visible = visible;
            this.maintainState = maintainState;
            this.maintainAnimation = maintainAnimation;
            this.maintainSize = maintainSize;
            this.maintainInteractivity = maintainInteractivity;
        }

        public readonly Widget child;

        public readonly Widget replacement;

        public readonly bool visible;

        public readonly bool maintainState;

        public readonly bool maintainAnimation;

        public readonly bool maintainSize;

        public readonly bool maintainInteractivity;

        public override Widget build(BuildContext context) {
            if (this.maintainSize) {
                Widget result = this.child;
                if (!this.maintainInteractivity) {
                    result = new IgnorePointer(
                        child: this.child,
                        ignoring: !this.visible
                    );
                }

                return new Opacity(
                    opacity: this.visible ? 1.0f : 0.0f,
                    child: result
                );
            }

            D.assert(!this.maintainInteractivity);
            D.assert(!this.maintainSize);
            if (this.maintainState) {
                Widget result = this.child;
                if (!this.maintainAnimation) {
                    result = new TickerMode(child: this.child, enabled: this.visible);
                }

                return new Offstage(
                    child: result,
                    offstage: !this.visible
                );
            }

            D.assert(!this.maintainAnimation);
            D.assert(!this.maintainState);
            return this.visible ? this.child : this.replacement;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("visible", value: this.visible, ifFalse: "hidden", ifTrue: "visible"));
            properties.add(new FlagProperty("maintainState", value: this.maintainState, ifFalse: "maintainState"));
            properties.add(new FlagProperty("maintainAnimation", value: this.maintainAnimation,
                ifFalse: "maintainAnimation"));
            properties.add(new FlagProperty("maintainSize", value: this.maintainSize, ifFalse: "maintainSize"));
            properties.add(new FlagProperty("maintainInteractivity", value: this.maintainInteractivity,
                ifFalse: "maintainInteractivity"));
        }
    }
}