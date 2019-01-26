using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class PageController : ScrollController {
        public PageController(
            int initialPage = 0,
            bool keepPage = true,
            double viewportFraction = 1.0
        ) {
            this.initialPage = initialPage;
            this.keepPage = keepPage;
            this.viewportFraction = viewportFraction;
            D.assert(viewportFraction > 0.0);
        }

        public readonly int initialPage;

        public readonly bool keepPage;

        public readonly double viewportFraction;


        public double page {
            get {
                D.assert(this.positions.isNotEmpty(),
                    "PageController.page cannot be accessed before a PageView is built with it."
                );
                D.assert(this.positions.Count == 1,
                    "The page property cannot be read when multiple PageViews are attached to " +
                    "the same PageController."
                );
                _PagePosition position = (_PagePosition) this.position;
                return position.page;
            }
        }

        public IPromise animateToPage(int page, TimeSpan duration, Curve curve) {
            _PagePosition position = (_PagePosition) this.position;
            return position.animateTo(
                position.getPixelsFromPage(page),
                duration,
                curve
            );
        }

        public void jumpToPage(int page) {
            _PagePosition position = (_PagePosition) this.position;
            position.jumpTo(position.getPixelsFromPage(page));
        }

        public IPromise nextPage(TimeSpan duration, Curve curve) {
            return this.animateToPage(this.page.round() + 1, duration: duration, curve: curve);
        }

        public IPromise previousPage(TimeSpan duration, Curve curve) {
            return this.animateToPage(this.page.round() - 1, duration: duration, curve: curve);
        }

        public override ScrollPosition createScrollPosition(ScrollPhysics physics, ScrollContext context,
            ScrollPosition oldPosition) {
            return new _PagePosition(
                physics: physics,
                context: context,
                initialPage: this.initialPage,
                keepPage: this.keepPage,
                viewportFraction: this.viewportFraction,
                oldPosition: oldPosition
            );
        }

        public override void attach(ScrollPosition position) {
            base.attach(position);
            _PagePosition pagePosition = (_PagePosition) position;
            pagePosition.viewportFraction = this.viewportFraction;
        }
    }

    public interface IPageMetrics : ScrollMetrics {
        double page { get; }
        double viewportFraction { get; }
    }

    public class PageMetrics : FixedScrollMetrics, IPageMetrics {
        public PageMetrics(
            double minScrollExtent = 0.0,
            double maxScrollExtent = 0.0,
            double pixels = 0.0,
            double viewportDimension = 0.0,
            AxisDirection axisDirection = AxisDirection.down,
            double viewportFraction = 0.0
        ) : base(
            minScrollExtent: minScrollExtent,
            maxScrollExtent: maxScrollExtent,
            pixels: pixels,
            viewportDimension: viewportDimension,
            axisDirection: axisDirection
        ) {
            this._viewportFraction = viewportFraction;
        }

        public readonly double _viewportFraction;

        public double page {
            get {
                return Math.Max(0.0, this.pixels.clamp(this.minScrollExtent, this.maxScrollExtent)) /
                       Math.Max(1.0, this.viewportDimension * this.viewportFraction);
            }
        }

        public double viewportFraction {
            get { return this._viewportFraction; }
        }
    }

    class _PagePosition : ScrollPositionWithSingleContext, IPageMetrics {
        internal _PagePosition(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            int initialPage = 0,
            bool keepPage = true,
            double viewportFraction = 1.0,
            ScrollPosition oldPosition = null
        ) :
            base(
                physics: physics,
                context: context,
                initialPixels: null,
                keepScrollOffset: keepPage,
                oldPosition: oldPosition
            ) {
            D.assert(viewportFraction > 0.0);
            this.initialPage = initialPage;
            this._viewportFraction = viewportFraction;
            this._pageToUseOnStartup = initialPage;
        }

        public readonly int initialPage;
        double _pageToUseOnStartup;

        public double viewportFraction {
            get { return this._viewportFraction; }
            set {
                if (this._viewportFraction == value) {
                    return;
                }

                double oldPage = this.page;
                this._viewportFraction = value;
                this.forcePixels(this.getPixelsFromPage(oldPage));
            }
        }

        double _viewportFraction;

        public double getPageFromPixels(double pixels, double viewportDimension) {
            return Math.Max(0.0, pixels) / Math.Max(1.0, viewportDimension * this.viewportFraction);
        }

        public double getPixelsFromPage(double page) {
            return page * this.viewportDimension * this.viewportFraction;
        }

        public double page {
            get {
                return this.getPageFromPixels(this.pixels.clamp(this.minScrollExtent, this.maxScrollExtent),
                    this.viewportDimension);
            }
        }

        protected override void saveScrollOffset() {
            PageStorage.of(this.context.storageContext)?.writeState(this.context.storageContext,
                this.getPageFromPixels(this.pixels, this.viewportDimension));
        }

        protected override void restoreScrollOffset() {
            object value = PageStorage.of(this.context.storageContext)?.readState(this.context.storageContext);
            if (value != null) {
                this._pageToUseOnStartup = (double) value;
            }
        }

        public override bool applyViewportDimension(double viewportDimension) {
            double oldViewportDimensions = 0.0;
            if (this.haveDimensions) {
                oldViewportDimensions = this.viewportDimension;
            }

            bool result = base.applyViewportDimension(viewportDimension);
            double? oldPixels = null;
            if (this.hasPixles) {
                oldPixels = this.pixels;
            }

            double page = (oldPixels == null || oldViewportDimensions == 0.0)
                ? this._pageToUseOnStartup
                : this.getPageFromPixels(oldPixels.Value, oldViewportDimensions);
            double newPixels = this.getPixelsFromPage(page);
            if (newPixels != oldPixels) {
                this.correctPixels(newPixels);
                return false;
            }

            return result;
        }
    }

    public class PageScrollPhysics : ScrollPhysics {
        public PageScrollPhysics(ScrollPhysics parent = null) : base(parent: parent) {
        }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new PageScrollPhysics(parent: this.buildParent(ancestor));
        }

        double _getPage(ScrollPosition position) {
            if (position is _PagePosition) {
                return ((_PagePosition) position).page;
            }

            return position.pixels / position.viewportDimension;
        }

        double _getPixels(ScrollPosition position, double page) {
            if (position is _PagePosition) {
                return ((_PagePosition) position).getPixelsFromPage(page);
            }

            return page * position.viewportDimension;
        }

        double _getTargetPixels(ScrollPosition position, Tolerance tolerance, double velocity) {
            double page = this._getPage(position);
            if (velocity < -tolerance.velocity) {
                page -= 0.5;
            }
            else if (velocity > tolerance.velocity) {
                page += 0.5;
            }

            return this._getPixels(position, page.round());
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, double velocity) {
            if ((velocity <= 0.0 && position.pixels <= position.minScrollExtent) ||
                (velocity >= 0.0 && position.pixels >= position.maxScrollExtent)) {
                return base.createBallisticSimulation(position, velocity);
            }

            Tolerance tolerance = this.tolerance;
            double target = this._getTargetPixels((ScrollPosition) position, tolerance, velocity);
            if (target != position.pixels) {
                return new ScrollSpringSimulation(this.spring, position.pixels, target, velocity, tolerance: tolerance);
            }

            return null;
        }

        public override bool allowImplicitScrolling {
            get { return false; }
        }
    }

    public static class PageViewUtils {
        internal static PageController _defaultPageController = new PageController();
        internal static PageScrollPhysics _kPagePhysics = new PageScrollPhysics();
    }


    public class PageView : StatefulWidget {
        public PageView(
            Key key = null,
            Axis scrollDirection = Axis.horizontal,
            bool reverse = false,
            PageController controller = null,
            ScrollPhysics physics = null,
            bool pageSnapping = true,
            ValueChanged<int> onPageChanged = null,
            List<Widget> children = null,
            IndexedWidgetBuilder itemBuilder = null,
            SliverChildDelegate childDelegate = null,
            int itemCount = 0
        ) : base(key: key) {
            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.physics = physics;
            this.pageSnapping = pageSnapping;
            this.onPageChanged = onPageChanged;
            this.controller = controller ?? PageViewUtils._defaultPageController;
            if (itemBuilder != null) {
                this.childrenDelegate = new SliverChildBuilderDelegate(itemBuilder, childCount: itemCount);
            }
            else if (childDelegate != null) {
                this.childrenDelegate = childDelegate;
            }
            else {
                this.childrenDelegate = new SliverChildListDelegate(children ?? new List<Widget>());
            }
        }

        public readonly Axis scrollDirection;

        public readonly bool reverse;

        public readonly PageController controller;

        public readonly ScrollPhysics physics;

        public readonly bool pageSnapping;

        public readonly ValueChanged<int> onPageChanged;

        public readonly SliverChildDelegate childrenDelegate;

        public override State createState() {
            return new _PageViewState();
        }
    }

    class _PageViewState : State<PageView> {
        int _lastReportedPage = 0;

        public override void initState() {
            base.initState();
            this._lastReportedPage = this.widget.controller.initialPage;
        }

        AxisDirection _getDirection(BuildContext context) {
            switch (this.widget.scrollDirection) {
                case Axis.horizontal:
                    D.assert(WidgetsD.debugCheckHasDirectionality(context));
                    TextDirection textDirection = Directionality.of(context);
                    AxisDirection axisDirection = AxisUtils.textDirectionToAxisDirection(textDirection);
                    return this.widget.reverse ? AxisUtils.flipAxisDirection(axisDirection) : axisDirection;
                case Axis.vertical:
                    return this.widget.reverse ? AxisDirection.up : AxisDirection.down;
            }

            throw new UIWidgetsError("fail to get axis direction");
        }

        public override Widget build(BuildContext context) {
            AxisDirection axisDirection = this._getDirection(context);
            ScrollPhysics physics = this.widget.pageSnapping
                ? PageViewUtils._kPagePhysics.applyTo(this.widget.physics)
                : this.widget.physics;

            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                    if (notification.depth == 0 && this.widget.onPageChanged != null &&
                        notification is ScrollUpdateNotification) {
                        IPageMetrics metrics = (IPageMetrics) notification.metrics;
                        int currentPage = metrics.page.round();
                        if (currentPage != this._lastReportedPage) {
                            this._lastReportedPage = currentPage;
                            this.widget.onPageChanged(currentPage);
                        }
                    }

                    return false;
                },
                child: new Scrollable(
                    axisDirection: axisDirection,
                    controller: this.widget.controller,
                    physics: physics,
                    viewportBuilder: (BuildContext _context, ViewportOffset position) => {
                        return new Viewport(
                            cacheExtent: 0.0,
                            axisDirection: axisDirection,
                            offset: position,
                            slivers: new List<Widget> {
                                new SliverFillViewport(
                                    viewportFraction: this.widget.controller.viewportFraction,
                                    del: this.widget.childrenDelegate
                                )
                            }
                        );
                    }
                )
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new EnumProperty<Axis>("scrollDirection", this.widget.scrollDirection));
            description.add(new FlagProperty("reverse", value: this.widget.reverse, ifTrue: "reversed"));
            description.add(
                new DiagnosticsProperty<PageController>("controller", this.widget.controller, showName: false));
            description.add(new DiagnosticsProperty<ScrollPhysics>("physics", this.widget.physics, showName: false));
            description.add(new FlagProperty("pageSnapping", value: this.widget.pageSnapping,
                ifFalse: "snapping disabled"));
        }
    }
}