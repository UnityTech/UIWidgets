using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class SafeArea : StatelessWidget {
        public SafeArea(
            Key key = null,
            bool left = true,
            bool top = true,
            bool right = true,
            bool bottom = true,
            EdgeInsets mininum = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.minimum = mininum ?? EdgeInsets.zero;
            this.child = child;
        }

        public readonly bool left;

        public readonly bool top;

        public readonly bool right;

        public readonly bool bottom;

        public readonly EdgeInsets minimum;

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            EdgeInsets padding = MediaQuery.of(context).padding;
            return new Padding(
                padding: EdgeInsets.only(
                    left: Mathf.Max(this.left ? padding.left : 0.0f, this.minimum.left),
                    top: Mathf.Max(this.top ? padding.top : 0.0f, this.minimum.top),
                    right: Mathf.Max(this.right ? padding.right : 0.0f, this.minimum.right),
                    bottom: Mathf.Max(this.bottom ? padding.bottom : 0.0f, this.minimum.bottom)
                ),
                child: MediaQuery.removePadding(
                    context: context,
                    removeLeft: this.left,
                    removeTop: this.top,
                    removeRight: this.right,
                    removeBottom: this.bottom,
                    child: this.child));
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("left", value: this.left, ifTrue: "avoid left padding"));
            properties.add(new FlagProperty("top", value: this.top, ifTrue: "avoid top padding"));
            properties.add(new FlagProperty("right", value: this.right, ifTrue: "avoid right padding"));
            properties.add(new FlagProperty("bottom", value: this.bottom, ifTrue: "avoid bottom padding"));
        }
    }


    public class SliverSafeArea : StatelessWidget {
        public SliverSafeArea(
            Key key = null,
            bool left = true,
            bool top = true,
            bool right = true,
            bool bottom = true,
            EdgeInsets minimum = null,
            Widget sliver = null) : base(key: key) {
            D.assert(sliver != null);
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.minimum = minimum ?? EdgeInsets.zero;
            this.sliver = sliver;
        }

        public readonly bool left;

        public readonly bool top;

        public readonly bool right;

        public readonly bool bottom;

        public readonly EdgeInsets minimum;

        public readonly Widget sliver;

        public override Widget build(BuildContext context) {
            EdgeInsets padding = MediaQuery.of(context).padding;
            return new SliverPadding(
                padding: EdgeInsets.only(
                    left: Mathf.Max(this.left ? padding.left : 0.0f, this.minimum.left),
                    top: Mathf.Max(this.top ? padding.top : 0.0f, this.minimum.top),
                    right: Mathf.Max(this.right ? padding.right : 0.0f, this.minimum.right),
                    bottom: Mathf.Max(this.bottom ? padding.bottom : 0.0f, this.minimum.bottom)
                ),
                sliver: MediaQuery.removePadding(
                    context: context,
                    removeLeft: this.left,
                    removeTop: this.top,
                    removeRight: this.right,
                    removeBottom: this.bottom,
                    child: this.sliver));
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("left", value: this.left, ifTrue: "avoid left padding"));
            properties.add(new FlagProperty("top", value: this.top, ifTrue: "avoid top padding"));
            properties.add(new FlagProperty("right", value: this.right, ifTrue: "avoid right padding"));
            properties.add(new FlagProperty("bottom", value: this.bottom, ifTrue: "avoid bottom padding"));
        }
    }
}