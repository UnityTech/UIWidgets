using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public abstract class ScrollPosition : ViewportOffset, ScrollMetrics {
        protected ScrollPosition(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            bool keepScrollOffset = true,
            ScrollPosition oldPosition = null,
            string debugLabel = null
        ) {
            D.assert(physics != null);
            D.assert(context != null);
            D.assert(context.vsync != null);

            this.physics = physics;
            this.context = context;
            this.keepScrollOffset = keepScrollOffset;
            this.debugLabel = debugLabel;

            if (oldPosition != null) {
                this.absorb(oldPosition);
            }

            if (keepScrollOffset) {
                this.restoreScrollOffset();
            }
        }

        public readonly ScrollPhysics physics;

        public readonly ScrollContext context;

        public readonly bool keepScrollOffset;

        public readonly string debugLabel;

        public double minScrollExtent {
            get { return this._minScrollExtent.Value; }
        }

        double? _minScrollExtent;

        public double maxScrollExtent {
            get { return this._maxScrollExtent.Value; }
        }

        double? _maxScrollExtent;

        public override double pixels {
            get {
                D.assert(this._pixels != null);
                return this._pixels ?? 0.0;
            }
        }

        public bool hasPixles {
            get { return this._pixels != null; }
        }

        internal double? _pixels;

        public double viewportDimension {
            get { return this._viewportDimension.Value; }
        }

        double? _viewportDimension;

        public bool haveDimensions {
            get { return this._haveDimensions; }
        }

        bool _haveDimensions = false;

        public abstract AxisDirection axisDirection { get; }

        protected virtual void absorb(ScrollPosition other) {
            D.assert(other != null);
            D.assert(other.context == this.context);
            D.assert(this._pixels == null);
            this._minScrollExtent = other.minScrollExtent;
            this._maxScrollExtent = other.maxScrollExtent;
            this._pixels = other._pixels;
            this._viewportDimension = other.viewportDimension;

            D.assert(this.activity == null);
            D.assert(other.activity != null);
            this._activity = other.activity;
            other._activity = null;
            if (other.GetType() != this.GetType()) {
                this.activity.resetActivity();
            }

            this.context.setIgnorePointer(this.activity.shouldIgnorePointer);
            this.isScrollingNotifier.value = this.activity.isScrolling;
        }

        public virtual double setPixels(double newPixels) {
            D.assert(this._pixels != null);
            D.assert(SchedulerBinding.instance.schedulerPhase <= SchedulerPhase.transientCallbacks);
            if (newPixels != this.pixels) {
                double overscroll = this.applyBoundaryConditions(newPixels);
                D.assert(() => {
                    double delta = newPixels - this.pixels;
                    if (overscroll.abs() > delta.abs()) {
                        throw new UIWidgetsError(
                            string.Format(
                                "{0}.applyBoundaryConditions returned invalid overscroll value.\n" +
                                "setPixels() was called to change the scroll offset from {1} to {2}.\n" +
                                "That is a delta of {3} units.\n" +
                                "{0}.applyBoundaryConditions reported an overscroll of {4} units."
                                , this.GetType(), this.pixels, newPixels, delta, overscroll));
                    }

                    return true;
                });

                double oldPixels = this.pixels;
                this._pixels = newPixels - overscroll;
                if (this.pixels != oldPixels) {
                    this.notifyListeners();
                    this.didUpdateScrollPositionBy(this.pixels - oldPixels);
                }

                if (overscroll != 0.0) {
                    this.didOverscrollBy(overscroll);
                    return overscroll;
                }
            }

            return 0.0;
        }

        public void correctPixels(double value) {
            this._pixels = value;
        }

        public override void correctBy(double correction) {
            D.assert(
                this._pixels != null,
                "An initial pixels value must exist by caling correctPixels on the ScrollPosition"
            );

            this._pixels += correction;
            this._didChangeViewportDimensionOrReceiveCorrection = true;
        }

        protected void forcePixels(double value) {
            D.assert(this._pixels != null);
            this._pixels = value;
            this.notifyListeners();
        }

        protected virtual void saveScrollOffset() {
            var pageStorage = PageStorage.of(this.context.storageContext);
            if (pageStorage != null) {
                pageStorage.writeState(this.context.storageContext, this.pixels);
            }
        }

        protected virtual void restoreScrollOffset() {
            if (this._pixels == null) {
                var pageStorage = PageStorage.of(this.context.storageContext);
                if (pageStorage != null) {
                    object valueRaw = pageStorage.readState(this.context.storageContext);
                    if (valueRaw != null) {
                        this.correctPixels((double) valueRaw);
                    }
                }
            }
        }

        protected double applyBoundaryConditions(double value) {
            double result = this.physics.applyBoundaryConditions(this, value);
            D.assert(() => {
                double delta = value - this.pixels;
                if (result.abs() > delta.abs()) {
                    throw new UIWidgetsError(
                        $"{this.physics.GetType()}.applyBoundaryConditions returned invalid overscroll value.\n" +
                        $"The method was called to consider a change from {this.pixels} to {value}, which is a " +
                        $"delta of {delta:F1} units. However, it returned an overscroll of " +
                        $"${result:F1} units, which has a greater magnitude than the delta. " +
                        "The applyBoundaryConditions method is only supposed to reduce the possible range " +
                        "of movement, not increase it.\n" +
                        $"The scroll extents are {this.minScrollExtent} .. {this.maxScrollExtent}, and the " +
                        $"viewport dimension is {this.viewportDimension}.");
                }

                return true;
            });

            return result;
        }

        bool _didChangeViewportDimensionOrReceiveCorrection = true;

        public override bool applyViewportDimension(double viewportDimension) {
            if (this._viewportDimension != viewportDimension) {
                this._viewportDimension = viewportDimension;
                this._didChangeViewportDimensionOrReceiveCorrection = true;
            }

            return true;
        }

        public override bool applyContentDimensions(double minScrollExtent, double maxScrollExtent) {
            if (!PhysicsUtils.nearEqual(this._minScrollExtent, minScrollExtent, Tolerance.defaultTolerance.distance) ||
                !PhysicsUtils.nearEqual(this._maxScrollExtent, maxScrollExtent, Tolerance.defaultTolerance.distance) ||
                this._didChangeViewportDimensionOrReceiveCorrection) {
                this._minScrollExtent = minScrollExtent;
                this._maxScrollExtent = maxScrollExtent;
                this._haveDimensions = true;
                this.applyNewDimensions();
                this._didChangeViewportDimensionOrReceiveCorrection = false;
            }

            return true;
        }

        protected virtual void applyNewDimensions() {
            D.assert(this._pixels != null);
            this.activity.applyNewDimensions();
        }

        public IPromise ensureVisible(RenderObject renderObject,
            double alignment = 0.0,
            TimeSpan? duration = null,
            Curve curve = null
        ) {
            D.assert(renderObject.attached);
            RenderAbstractViewport viewport = RenderViewportUtils.of(renderObject);
            D.assert(viewport != null);

            double target = viewport.getOffsetToReveal(renderObject, alignment).offset.clamp(
                this.minScrollExtent, this.maxScrollExtent);

            if (target == this.pixels) {
                return Promise.Resolved();
            }

            duration = duration ?? TimeSpan.Zero;
            if (duration == TimeSpan.Zero) {
                this.jumpTo(target);
                return Promise.Resolved();
            }

            curve = curve ?? Curves.ease;
            return this.animateTo(target, duration: duration.Value, curve: curve);
        }

        public readonly ValueNotifier<bool> isScrollingNotifier = new ValueNotifier<bool>(false);

        public override bool allowImplicitScrolling {
            get { return this.physics.allowImplicitScrolling; }
        }

        public abstract ScrollHoldController hold(VoidCallback holdCancelCallback);

        public abstract Drag drag(DragStartDetails details, VoidCallback dragCancelCallback);

        protected ScrollActivity activity {
            get { return this._activity; }
        }

        ScrollActivity _activity;

        public virtual void beginActivity(ScrollActivity newActivity) {
            if (newActivity == null) {
                return;
            }

            bool wasScrolling, oldIgnorePointer;
            if (this._activity != null) {
                oldIgnorePointer = this._activity.shouldIgnorePointer;
                wasScrolling = this._activity.isScrolling;
                if (wasScrolling && !newActivity.isScrolling) {
                    this.didEndScroll();
                }

                this._activity.dispose();
            }
            else {
                oldIgnorePointer = false;
                wasScrolling = false;
            }

            this._activity = newActivity;
            if (oldIgnorePointer != this.activity.shouldIgnorePointer) {
                this.context.setIgnorePointer(this.activity.shouldIgnorePointer);
            }

            this.isScrollingNotifier.value = this.activity.isScrolling;
            if (!wasScrolling && this._activity.isScrolling) {
                this.didStartScroll();
            }
        }

        public void didStartScroll() {
            this.activity.dispatchScrollStartNotification(
                ScrollMetricsUtils.copyWith(this), this.context.notificationContext);
        }

        public void didUpdateScrollPositionBy(double delta) {
            this.activity.dispatchScrollUpdateNotification(
                ScrollMetricsUtils.copyWith(this), this.context.notificationContext, delta);
        }

        public void didEndScroll() {
            this.activity.dispatchScrollEndNotification(
                ScrollMetricsUtils.copyWith(this), this.context.notificationContext);
            if (this.keepScrollOffset) {
                this.saveScrollOffset();
            }
        }

        public void didOverscrollBy(double value) {
            D.assert(this.activity.isScrolling);
            this.activity.dispatchOverscrollNotification(
                ScrollMetricsUtils.copyWith(this), this.context.notificationContext, value);
        }

        public void didUpdateScrollDirection(ScrollDirection direction) {
            new UserScrollNotification(metrics:
                ScrollMetricsUtils.copyWith(this), context: this.context.notificationContext, direction: direction
            ).dispatch(this.context.notificationContext);
        }

        public override void dispose() {
            D.assert(this._pixels != null);
            if (this.activity != null) {
                this.activity.dispose();
                this._activity = null;
            }

            base.dispose();
        }

        protected override void debugFillDescription(List<string> description) {
            if (this.debugLabel != null) {
                description.Add(this.debugLabel);
            }

            base.debugFillDescription(description);
            description.Add($"range: {this._minScrollExtent:F1}..{this._maxScrollExtent:F1}");
            description.Add($"viewport: {this._viewportDimension:F1}");
        }
    }
}