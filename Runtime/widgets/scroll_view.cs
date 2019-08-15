using System.Collections.Generic;
using com.unity.uiwidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public abstract class ScrollView : StatelessWidget {
        protected ScrollView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            Key center = null,
            float anchor = 0.0f,
            float? cacheExtent = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(!(controller != null && primary == true),
                () => "Primary ScrollViews obtain their ScrollController via inheritance from a PrimaryScrollController widget. " +
                "You cannot both set primary to true and pass an explicit controller.");
            D.assert(!shrinkWrap || center == null);
            D.assert(anchor >= 0.0f && anchor <= 1.0f);

            primary = primary ?? controller == null && scrollDirection == Axis.vertical;
            physics = physics ?? (primary.Value ? new AlwaysScrollableScrollPhysics() : null);

            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.controller = controller;
            this.primary = primary.Value;
            this.physics = physics;
            this.shrinkWrap = shrinkWrap;
            this.center = center;
            this.anchor = anchor;
            this.cacheExtent = cacheExtent;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly Axis scrollDirection;
        public readonly bool reverse;
        public readonly ScrollController controller;
        public readonly bool primary;
        public readonly ScrollPhysics physics;
        public readonly bool shrinkWrap;
        public readonly Key center;
        public readonly float anchor;
        public readonly float? cacheExtent;
        public readonly DragStartBehavior dragStartBehavior;

        protected AxisDirection getDirection(BuildContext context) {
            return LayoutUtils.getAxisDirectionFromAxisReverseAndDirectionality(
                context, this.scrollDirection, this.reverse);
        }

        protected abstract List<Widget> buildSlivers(BuildContext context);

        protected virtual Widget buildViewport(
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
                cacheExtent: this.cacheExtent,
                center: this.center,
                anchor: this.anchor
            );
        }

        public override Widget build(BuildContext context) {
            List<Widget> slivers = this.buildSlivers(context);
            AxisDirection axisDirection = this.getDirection(context);

            ScrollController scrollController = this.primary ? PrimaryScrollController.of(context) : this.controller;

            Scrollable scrollable = new Scrollable(
                dragStartBehavior: this.dragStartBehavior,
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

    public class CustomScrollView : ScrollView {
        public CustomScrollView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            Key center = null,
            float anchor = 0.0f,
            float? cacheExtent = null,
            List<Widget> slivers = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            center: center,
            anchor: anchor,
            cacheExtent: cacheExtent,
            dragStartBehavior: dragStartBehavior
        ) {
            this.slivers = slivers ?? new List<Widget>();
        }

        public readonly List<Widget> slivers;

        protected override List<Widget> buildSlivers(BuildContext context) {
            return this.slivers;
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
            float? cacheExtent = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            cacheExtent: cacheExtent,
            dragStartBehavior: dragStartBehavior
        ) {
            this.padding = padding;
        }

        public readonly EdgeInsets padding;

        protected override List<Widget> buildSlivers(BuildContext context) {
            Widget sliver = this.buildChildLayout(context);

            EdgeInsets effectivePadding = this.padding; // no need to check MediaQuery for now.
            if (effectivePadding != null) {
                sliver = new SliverPadding(padding: effectivePadding, sliver: sliver);
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

    public class ListView : BoxScrollView {
        public ListView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            float? itemExtent = null,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null,
            List<Widget> children = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            cacheExtent: cacheExtent,
            dragStartBehavior: dragStartBehavior
        ) {
            this.itemExtent = itemExtent;
            this.childrenDelegate = new SliverChildListDelegate(
                children,
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries
            );
        }

        ListView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            float? itemExtent = null,
            IndexedWidgetBuilder itemBuilder = null,
            int? itemCount = null,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            cacheExtent: cacheExtent,
            dragStartBehavior: dragStartBehavior
        ) {
            this.itemExtent = itemExtent;
            this.childrenDelegate = new SliverChildBuilderDelegate(
                itemBuilder,
                childCount: itemCount,
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries
            );
        }

        public static ListView builder(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            float? itemExtent = null,
            IndexedWidgetBuilder itemBuilder = null,
            int? itemCount = null,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) {
            return new ListView(
                key: key,
                scrollDirection: scrollDirection,
                reverse: reverse,
                controller: controller,
                primary: primary,
                physics: physics,
                shrinkWrap: shrinkWrap,
                padding: padding,
                cacheExtent: cacheExtent,
                itemExtent: itemExtent,
                itemBuilder: itemBuilder,
                itemCount: itemCount,
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries,
                dragStartBehavior: dragStartBehavior
            );
        }


        ListView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            IndexedWidgetBuilder itemBuilder = null,
            IndexedWidgetBuilder separatorBuilder = null,
            int itemCount = 0,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            cacheExtent: cacheExtent,
            dragStartBehavior: dragStartBehavior
        ) {
            D.assert(itemBuilder != null);
            D.assert(separatorBuilder != null);
            D.assert(itemCount >= 0);
            this.itemExtent = null;
            this.childrenDelegate = new SliverChildBuilderDelegate(
                (context, index) => {
                    int itemIndex = index / 2;
                    return index % 2 == 0
                        ? itemBuilder(context, itemIndex)
                        : separatorBuilder(context, itemIndex);
                },
                childCount: Mathf.Max(0, itemCount * 2 - 1),
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries
            );
        }

        public static ListView seperated(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            IndexedWidgetBuilder itemBuilder = null,
            IndexedWidgetBuilder separatorBuilder = null,
            int itemCount = 0,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null
        ) {
            return new ListView(
                key,
                scrollDirection,
                reverse,
                controller,
                primary,
                physics,
                shrinkWrap,
                padding,
                itemBuilder,
                separatorBuilder,
                itemCount,
                addAutomaticKeepAlives,
                addRepaintBoundaries,
                cacheExtent
            );
        }

        ListView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            float? itemExtent = null,
            SliverChildDelegate childrenDelegate = null,
            float? cacheExtent = null
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            cacheExtent: cacheExtent
        ) {
            D.assert(childrenDelegate != null);
            this.itemExtent = itemExtent;
            this.childrenDelegate = childrenDelegate;
        }

        public static ListView custom(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            float? itemExtent = null,
            SliverChildDelegate childrenDelegate = null,
            float? cacheExtent = null
        ) {
            return new ListView(
                key,
                scrollDirection,
                reverse,
                controller,
                primary,
                physics,
                shrinkWrap,
                padding,
                itemExtent,
                childrenDelegate,
                cacheExtent);
        }

        public readonly float? itemExtent;

        public readonly SliverChildDelegate childrenDelegate;

        protected override Widget buildChildLayout(BuildContext context) {
            if (this.itemExtent != null) {
                return new SliverFixedExtentList(
                    del: this.childrenDelegate,
                    itemExtent: this.itemExtent.Value
                );
            }

            return new SliverList(del: this.childrenDelegate);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("itemExtent", this.itemExtent,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }

    public class GridView : BoxScrollView {
        public GridView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            SliverGridDelegate gridDelegate = null,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null,
            List<Widget> children = null
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            cacheExtent: cacheExtent
        ) {
            D.assert(gridDelegate != null);
            this.childrenDelegate = new SliverChildListDelegate(
                children ?? new List<Widget>(),
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries
            );
        }

        public GridView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            SliverGridDelegate gridDelegate = null,
            IndexedWidgetBuilder itemBuilder = null,
            int? itemCount = null,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            cacheExtent: cacheExtent
        ) {
            this.gridDelegate = gridDelegate;
            this.childrenDelegate = new SliverChildBuilderDelegate(
                itemBuilder,
                childCount: itemCount,
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries
            );
        }

        public static GridView builder(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            SliverGridDelegate gridDelegate = null,
            IndexedWidgetBuilder itemBuilder = null,
            int? itemCount = null,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null
        ) {
            return new GridView(
                key: key,
                scrollDirection: scrollDirection,
                reverse: reverse,
                controller: controller,
                primary: primary,
                physics: physics,
                shrinkWrap: shrinkWrap,
                padding: padding,
                gridDelegate: gridDelegate,
                itemBuilder: itemBuilder,
                itemCount: itemCount,
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries,
                cacheExtent: cacheExtent
            );
        }

        public GridView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            SliverGridDelegate gridDelegate = null,
            SliverChildDelegate childrenDelegate = null,
            float? cacheExtent = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            cacheExtent: cacheExtent,
            dragStartBehavior: dragStartBehavior
        ) {
            D.assert(gridDelegate != null);
            D.assert(childrenDelegate != null);
            this.gridDelegate = gridDelegate;
            this.childrenDelegate = childrenDelegate;
        }

        public static GridView custom(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            SliverGridDelegate gridDelegate = null,
            SliverChildDelegate childrenDelegate = null,
            float? cacheExtent = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) {
            return new GridView(
                key: key,
                scrollDirection: scrollDirection,
                reverse: reverse,
                controller: controller,
                primary: primary,
                physics: physics,
                shrinkWrap: shrinkWrap,
                padding: padding,
                gridDelegate: gridDelegate,
                childrenDelegate: childrenDelegate,
                cacheExtent: cacheExtent,
                dragStartBehavior: dragStartBehavior
            );
        }

        public GridView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            int? crossAxisCount = null,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null,
            List<Widget> children = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            cacheExtent: cacheExtent,
            dragStartBehavior: dragStartBehavior
        ) {
            this.gridDelegate = new SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: crossAxisCount ?? 0,
                mainAxisSpacing: mainAxisSpacing,
                crossAxisSpacing: crossAxisSpacing,
                childAspectRatio: childAspectRatio
            );
            this.childrenDelegate = new SliverChildListDelegate(
                children ?? new List<Widget>(),
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries
            );
        }

        public static GridView count(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            int? crossAxisCount = null,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            float? cacheExtent = null,
            List<Widget> children = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) {
            return new GridView(
                key: key,
                scrollDirection: scrollDirection,
                reverse: reverse,
                controller: controller,
                primary: primary,
                physics: physics,
                shrinkWrap: shrinkWrap,
                padding: padding,
                crossAxisCount: crossAxisCount,
                mainAxisSpacing: mainAxisSpacing,
                crossAxisSpacing: crossAxisSpacing,
                childAspectRatio: childAspectRatio,
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries,
                cacheExtent: cacheExtent,
                children: children,
                dragStartBehavior: dragStartBehavior
            );
        }

        public GridView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            float? maxCrossAxisExtent = null,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            bool addSemanticIndexes = true,
            List<Widget> children = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            key: key,
            scrollDirection: scrollDirection,
            reverse: reverse,
            controller: controller,
            primary: primary,
            physics: physics,
            shrinkWrap: shrinkWrap,
            padding: padding,
            dragStartBehavior: dragStartBehavior
        ) {
            this.gridDelegate = new SliverGridDelegateWithMaxCrossAxisExtent(
                maxCrossAxisExtent: maxCrossAxisExtent ?? 0,
                mainAxisSpacing: mainAxisSpacing,
                crossAxisSpacing: crossAxisSpacing,
                childAspectRatio: childAspectRatio
            );
            this.childrenDelegate = new SliverChildListDelegate(
                children ?? new List<Widget> { },
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries
            );
        }

        public static GridView extent(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsets padding = null,
            float? maxCrossAxisExtent = null,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true,
            List<Widget> children = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) {
            return new GridView(
                key: key,
                scrollDirection: scrollDirection,
                reverse: reverse,
                controller: controller,
                primary: primary,
                physics: physics,
                shrinkWrap: shrinkWrap,
                padding: padding,
                maxCrossAxisExtent: maxCrossAxisExtent,
                mainAxisSpacing: mainAxisSpacing,
                crossAxisSpacing: crossAxisSpacing,
                childAspectRatio: childAspectRatio,
                addAutomaticKeepAlives: addAutomaticKeepAlives,
                addRepaintBoundaries: addRepaintBoundaries,
                children: children,
                dragStartBehavior: dragStartBehavior
            );
        }

        public readonly SliverGridDelegate gridDelegate;

        public readonly SliverChildDelegate childrenDelegate;

        protected override Widget buildChildLayout(BuildContext context) {
            return new SliverGrid(
                layoutDelegate: this.childrenDelegate,
                gridDelegate: this.gridDelegate
            );
        }
    }
}