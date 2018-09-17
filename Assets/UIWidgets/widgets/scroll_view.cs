using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.rendering;

namespace UIWidgets.widgets {
    public abstract class ScrollView : StatelessWidget {
        protected ScrollView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            double? cacheExtent = null
        ) : base(key: key) {
            D.assert(!(controller != null && primary == true),
                "Primary ScrollViews obtain their ScrollController via inheritance from a PrimaryScrollController widget. " +
                "You cannot both set primary to true and pass an explicit controller.");

            primary = primary ?? controller == null && scrollDirection == Axis.vertical;
            physics = physics ?? (primary.Value ? new AlwaysScrollableScrollPhysics() : null);

            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.controller = controller;
            this.primary = primary.Value;
            this.physics = physics;
            this.shrinkWrap = shrinkWrap;
            this.cacheExtent = cacheExtent;
        }

        public readonly Axis scrollDirection;
        public readonly bool reverse;
        public readonly ScrollController controller;
        public readonly bool primary;
        public readonly ScrollPhysics physics;
        public readonly bool shrinkWrap;
        public readonly double? cacheExtent;

        protected AxisDirection getDirection(BuildContext context) {
            return AxisUtils.getAxisDirectionFromAxisReverseAndDirectionality(context, this.scrollDirection,
                this.reverse);
        }

        protected abstract List<Widget> buildSlivers(BuildContext context);

        protected Widget buildViewport(
            BuildContext context,
            ViewportOffset offset,
            AxisDirection axisDirection,
            List<Widget> slivers
        ) {
            if (this.shrinkWrap) {
                return new ShrinkWrappingViewport(
                    axisDirection: axisDirection,
                    offset: offset,
                    slivers: slivers
                );
            }

            return new Viewport(
                axisDirection: axisDirection,
                offset: offset,
                slivers: slivers,
                cacheExtent: this.cacheExtent
            );
        }

        public override Widget build(BuildContext context) {
            List<Widget> slivers = this.buildSlivers(context);
            AxisDirection axisDirection = this.getDirection(context);

            ScrollController scrollController = this.primary ? PrimaryScrollController.of(context) : this.controller;

            Scrollable scrollable = new Scrollable(
                axisDirection: axisDirection,
                controller: scrollController,
                physics: this.physics,
                viewportBuilder: (viewportContext, offset) =>
                    this.buildViewport(viewportContext, offset, axisDirection, slivers)
            );
            return this.primary && scrollController != null
                ? (Widget) PrimaryScrollController.none(child: scrollable)
                : scrollable;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<Axis>("scrollDirection", this.scrollDirection));
            properties.add(new FlagProperty("reverse", value: this.reverse, ifTrue: "reversed", showName: true));
            properties.add(new DiagnosticsProperty<ScrollController>("controller", this.controller, showName: false,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new FlagProperty("primary", value: this.primary, ifTrue: "using primary controller",
                showName: true));
            properties.add(new DiagnosticsProperty<ScrollPhysics>("physics", this.physics, showName: false,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new FlagProperty("shrinkWrap", value: this.shrinkWrap, ifTrue: "shrink-wrapping",
                showName: true));
        }
    }

    public abstract class BoxScrollView : ScrollView {
        public BoxScrollView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            double? cacheExtent = null
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            cacheExtent: cacheExtent
        ) {
            this.padding = padding;
        }

        public readonly EdgeInsets padding;

        protected override List<Widget> buildSlivers(BuildContext context) {
            Widget sliver = this.buildChildLayout(context);

            EdgeInsets effectivePadding = this.padding;
            if (this.padding == null) {
//      final MediaQueryData mediaQuery = MediaQuery.of(context, nullOk: true);
//      if (mediaQuery != null) {
//        // Automatically pad sliver with padding from MediaQuery.
//        final EdgeInsets mediaQueryHorizontalPadding =
//            mediaQuery.padding.copyWith(top: 0.0, bottom: 0.0);
//        final EdgeInsets mediaQueryVerticalPadding =
//            mediaQuery.padding.copyWith(left: 0.0, right: 0.0);
//        // Consume the main axis padding with SliverPadding.
//        effectivePadding = scrollDirection == Axis.vertical
//            ? mediaQueryVerticalPadding
//            : mediaQueryHorizontalPadding;
//        // Leave behind the cross axis padding.
//        sliver = new MediaQuery(
//          data: mediaQuery.copyWith(
//            padding: scrollDirection == Axis.vertical
//                ? mediaQueryHorizontalPadding
//                : mediaQueryVerticalPadding,
//          ),
//          child: sliver,
//        );
//      }
            }

            if (effectivePadding != null) {
//          sliver = new SliverPadding(padding: effectivePadding, sliver: sliver);
            }

            return new List<Widget> {sliver};
        }

        protected abstract Widget buildChildLayout(BuildContext context);

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }
}