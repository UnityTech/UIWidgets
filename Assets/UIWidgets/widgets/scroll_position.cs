using UIWidgets.foundation;
using UIWidgets.rendering;
using UIWidgets.scheduler;
using UIWidgets.ui;

namespace UIWidgets.widgets {
//    public abstract class ScrollPosition : ViewportOffset, ScrollMetrics {
//        ScrollPosition(
//            ScrollPhysics physics = null,
//            ScrollContext context = null,
//            bool keepScrollOffset = true,
//            ScrollPosition oldPosition = null,
//            string debugLabel = null
//        ) {
//            D.assert(physics != null);
//            D.assert(context != null);
//            D.assert(context.vsync != null);
//
//            this.physics = physics;
//            this.context = context;
//            this.keepScrollOffset = keepScrollOffset;
//            this.debugLabel = debugLabel;
//
//            if (oldPosition != null) {
//                this.absorb(oldPosition);
//            }
//
//            if (keepScrollOffset) {
//                this.restoreScrollOffset();
//            }
//        }
//
//        public readonly ScrollPhysics physics;
//        public readonly ScrollContext context;
//        public readonly bool keepScrollOffset;
//        public readonly string debugLabel;
//
//        public double minScrollExtent {
//            get { return this._minScrollExtent; }
//        }
//
//        double _minScrollExtent;
//
//        public double maxScrollExtent {
//            get { return this._maxScrollExtent; }
//        }
//
//        double _maxScrollExtent;
//
//        public override double? pixels {
//            get { return this._pixels; }
//        }
//
//        double? _pixels;
//
//        public double viewportDimension {
//            get { return this._viewportDimension; }
//        }
//
//        double _viewportDimension;
//
//        public bool haveDimensions {
//            get { return this._haveDimensions; }
//        }
//
//        bool _haveDimensions = false;
//
//        protected virtual void absorb(ScrollPosition other) {
//            D.assert(other != null);
//            D.assert(other.context == context);
//            D.assert(_pixels == null);
//            _minScrollExtent = other.minScrollExtent;
//            _maxScrollExtent = other.maxScrollExtent;
//            _pixels = other._pixels;
//            _viewportDimension = other.viewportDimension;
//
//            D.assert(activity == null);
//            D.assert(other.activity != null);
//            _activity = other.activity;
//            other._activity = null;
//            if (other.runtimeType != runtimeType)
//                activity.resetActivity();
//            context.setIgnorePointer(activity.shouldIgnorePointer);
//            isScrollingNotifier.value = activity.isScrolling;
//        }
//
//        public double setPixels(double newPixels) {
//            D.assert(this._pixels != null);
//            D.assert(this.context.vsync.schedulerBinding.schedulerPhase <= SchedulerPhase.transientCallbacks);
//            if (newPixels != this.pixels) {
//                double overscroll = this.applyBoundaryConditions(newPixels);
//                D.assert(() => {
//                    double delta = newPixels - this.pixels;
//                    if (overscroll.abs() > delta.abs()) {
//                        throw new UIWidgetsError(
//                            string.Format(
//                                "{0}.applyBoundaryConditions returned invalid overscroll value.\n" +
//                                "setPixels() was called to change the scroll offset from {1} to {2}.\n" +
//                                "That is a delta of {3} units.\n" +
//                                "{0}.applyBoundaryConditions reported an overscroll of {4} units."
//                                , this.GetType(), this.pixels, newPixels, delta, overscroll));
//                    }
//
//                    return true;
//                });
//                
//                double oldPixels = this.pixels;
//                this._pixels = newPixels - overscroll;
//                if (this._pixels != oldPixels) {
//                    this.notifyListeners();
//                    this.didUpdateScrollPositionBy(this._pixels - oldPixels);
//                }
//
//                if (overscroll != 0.0) {
//                    this.didOverscrollBy(overscroll);
//                    return overscroll;
//                }
//            }
//
//            return 0.0;
//        }
//        
//        public void correctPixels(double value) {
//            this._pixels = value;
//        }
//        
//        public override void correctBy(double correction) {
//            D.assert(
//                this._pixels != null,
//                "An initial pixels value must exist by caling correctPixels on the ScrollPosition"
//            );
//            
//            this._pixels += correction;
//            this._didChangeViewportDimensionOrReceiveCorrection = true;
//        }
//        
//        protected void forcePixels(double value) {
//            D.assert(this.pixels != null);
//            _pixels = value;
//            notifyListeners();
//        }
//        
//        protected void saveScrollOffset() {
//            PageStorage.of(context.storageContext)?.writeState(context.storageContext, pixels);
//        }
//        
//        protected void restoreScrollOffset() {
//            if (pixels == null) {
//                final double value = PageStorage.of(context.storageContext)?.readState(context.storageContext);
//                if (value != null)
//                    correctPixels(value);
//            }
//        }
//    }
}