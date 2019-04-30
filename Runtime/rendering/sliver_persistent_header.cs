using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public abstract class RenderSliverPersistentHeader : RenderObjectWithChildMixinRenderSliver<RenderBox> {
        public RenderSliverPersistentHeader(RenderBox child = null) {
            this.child = child;
        }

        public virtual float? maxExtent { get; }

        public virtual float? minExtent { get; }

        public float childExtent {
            get {
                if (this.child == null) {
                    return 0.0f;
                }

                D.assert(this.child.hasSize);
                switch (this.constraints.axis) {
                    case Axis.vertical:
                        return this.child.size.height;
                    case Axis.horizontal:
                        return this.child.size.width;
                    default:
                        throw new Exception("Unknown axis: " + this.constraints.axis);
                }
            }
        }

        bool _needsUpdateChild = true;
        float _lastShrinkOffset = 0.0f;
        bool _lastOverlapsContent = false;

        protected virtual void updateChild(float shrinkOffset, bool overlapsContent) {
        }

        public override void markNeedsLayout() {
            this._needsUpdateChild = true;
            base.markNeedsLayout();
        }

        protected void layoutChild(float scrollOffset, float maxExtent, bool overlapsContent = false) {
            float shrinkOffset = Mathf.Min(scrollOffset, maxExtent);
            if (this._needsUpdateChild || this._lastShrinkOffset != shrinkOffset ||
                this._lastOverlapsContent != overlapsContent) {
                this.invokeLayoutCallback<SliverConstraints>((SliverConstraints constraints) => {
                    D.assert(constraints == this.constraints);
                    this.updateChild(shrinkOffset, overlapsContent);
                });
                this._lastShrinkOffset = shrinkOffset;
                this._lastOverlapsContent = overlapsContent;
                this._needsUpdateChild = false;
            }

            D.assert(this.minExtent != null);
            D.assert(() => {
                if (this.minExtent <= maxExtent) {
                    return true;
                }

                throw new UIWidgetsError(
                    "The maxExtent for this $runtimeType is less than its minExtent.\n" +
                    "The specified maxExtent was: ${maxExtent.toStringAsFixed(1)}\n" +
                    "The specified minExtent was: ${minExtent.toStringAsFixed(1)}\n"
                );
            });
            this.child?.layout(
                this.constraints.asBoxConstraints(
                    maxExtent: Mathf.Max(this.minExtent ?? 0.0f, maxExtent - shrinkOffset)),
                parentUsesSize: true
            );
        }

        public override float childMainAxisPosition(RenderObject child) {
            return base.childMainAxisPosition(this.child);
        }

        protected override bool hitTestChildren(HitTestResult result, float mainAxisPosition, float crossAxisPosition) {
            D.assert(this.geometry.hitTestExtent > 0.0f);
            if (this.child != null) {
                return RenderSliverHelpers.hitTestBoxChild(this, result, this.child, mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition);
            }

            return false;
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            D.assert(child != null);
            D.assert(child == this.child);
            RenderSliverHelpers.applyPaintTransformForBoxChild(this, this.child, transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null && this.geometry.visible) {
                switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(this.constraints.axisDirection,
                    this.constraints.growthDirection)) {
                    case AxisDirection.up:
                        offset += new Offset(0.0f,
                            this.geometry.paintExtent - this.childMainAxisPosition(this.child) - this.childExtent);
                        break;
                    case AxisDirection.down:
                        offset += new Offset(0.0f, this.childMainAxisPosition(this.child));
                        break;
                    case AxisDirection.left:
                        offset += new Offset(
                            this.geometry.paintExtent - this.childMainAxisPosition(this.child) - this.childExtent,
                            0.0f);
                        break;
                    case AxisDirection.right:
                        offset += new Offset(this.childMainAxisPosition(this.child), 0.0f);
                        break;
                }

                context.paintChild(this.child, offset);
            }
        }

        protected bool excludeFromSemanticsScrolling {
            get { return this._excludeFromSemanticsScrolling; }
            set {
                if (this._excludeFromSemanticsScrolling == value) {
                    return;
                }

                this._excludeFromSemanticsScrolling = value;
            }
        }

        bool _excludeFromSemanticsScrolling = false;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(FloatProperty.lazy("maxExtent", () => this.maxExtent));
            properties.add(FloatProperty.lazy("child position", () => this.childMainAxisPosition(this.child)));
        }
    }

    public abstract class RenderSliverScrollingPersistentHeader : RenderSliverPersistentHeader {
        public RenderSliverScrollingPersistentHeader(
            RenderBox child = null
        ) : base(child: child) {
        }

        float _childPosition;

        protected override void performLayout() {
            float? maxExtent = this.maxExtent;
            this.layoutChild(this.constraints.scrollOffset, maxExtent ?? 0.0f);
            float? paintExtent = maxExtent - this.constraints.scrollOffset;
            this.geometry = new SliverGeometry(
                scrollExtent: maxExtent ?? 0.0f,
                paintOrigin: Mathf.Min(this.constraints.overlap, 0.0f),
                paintExtent: paintExtent?.clamp(0.0f, this.constraints.remainingPaintExtent) ?? 0.0f,
                maxPaintExtent: maxExtent ?? 0.0f,
                hasVisualOverflow: true
            );
            this._childPosition = Mathf.Min(0.0f, paintExtent ?? 0.0f - this.childExtent);
        }

        public override float childMainAxisPosition(RenderObject child) {
            D.assert(child == this.child);
            return this._childPosition;
        }
    }

    public abstract class RenderSliverPinnedPersistentHeader : RenderSliverPersistentHeader {
        public RenderSliverPinnedPersistentHeader(
            RenderBox child = null
        ) : base(child: child) {
        }

        protected override void performLayout() {
            float? maxExtent = this.maxExtent;
            bool overlapsContent = this.constraints.overlap > 0.0f;
            this.excludeFromSemanticsScrolling =
                overlapsContent || (this.constraints.scrollOffset > maxExtent - this.minExtent);
            this.layoutChild(this.constraints.scrollOffset, maxExtent ?? 0.0f, overlapsContent: overlapsContent);
            float? layoutExtent =
                (maxExtent - this.constraints.scrollOffset)?.clamp(0.0f, this.constraints.remainingPaintExtent);
            this.geometry = new SliverGeometry(
                scrollExtent: maxExtent ?? 0.0f,
                paintOrigin: this.constraints.overlap,
                paintExtent: Mathf.Min(this.childExtent, this.constraints.remainingPaintExtent),
                layoutExtent: layoutExtent,
                maxPaintExtent: maxExtent ?? 0.0f,
                maxScrollObstructionExtent: this.minExtent ?? 0.0f,
                cacheExtent: layoutExtent > 0.0f ? -this.constraints.cacheOrigin + layoutExtent : layoutExtent,
                hasVisualOverflow: true
            );
        }

        public override float childMainAxisPosition(RenderObject child) {
            return 0.0f;
        }
    }

    public class FloatingHeaderSnapConfiguration {
        public FloatingHeaderSnapConfiguration(
            TickerProvider vsync,
            Curve curve = null,
            TimeSpan? duration = null
        ) {
            D.assert(vsync != null);
            this.vsync = vsync;
            this.curve = curve ?? Curves.ease;
            this.duration = duration ?? new TimeSpan(0, 0, 0, 0, 300);
        }

        public readonly TickerProvider vsync;

        public readonly Curve curve;

        public readonly TimeSpan duration;
    }


    public abstract class RenderSliverFloatingPersistentHeader : RenderSliverPersistentHeader {
        public RenderSliverFloatingPersistentHeader(
            RenderBox child = null,
            FloatingHeaderSnapConfiguration snapConfiguration = null
        ) : base(child: child) {
            this._snapConfiguration = snapConfiguration;
        }

        AnimationController _controller;
        Animation<float> _animation;
        protected float _lastActualScrollOffset;
        protected float _effectiveScrollOffset;

        float _childPosition;

        public override void detach() {
            this._controller?.dispose();
            this._controller = null;
            base.detach();
        }

        public FloatingHeaderSnapConfiguration snapConfiguration {
            get { return this._snapConfiguration; }
            set {
                if (value == this._snapConfiguration) {
                    return;
                }

                if (value == null) {
                    this._controller?.dispose();
                    this._controller = null;
                }
                else {
                    if (this._snapConfiguration != null && value.vsync != this._snapConfiguration.vsync) {
                        this._controller?.resync(value.vsync);
                    }
                }

                this._snapConfiguration = value;
            }
        }

        FloatingHeaderSnapConfiguration _snapConfiguration;

        protected virtual float updateGeometry() {
            float? maxExtent = this.maxExtent;
            float? paintExtent = maxExtent - this._effectiveScrollOffset;
            float? layoutExtent = maxExtent - this.constraints.scrollOffset;
            this.geometry = new SliverGeometry(
                scrollExtent: maxExtent ?? 0.0f,
                paintOrigin: Mathf.Min(this.constraints.overlap, 0.0f),
                paintExtent: paintExtent?.clamp(0.0f, this.constraints.remainingPaintExtent) ?? 0.0f,
                layoutExtent: layoutExtent?.clamp(0.0f, this.constraints.remainingPaintExtent),
                maxPaintExtent: maxExtent ?? 0.0f,
                maxScrollObstructionExtent: maxExtent ?? 0.0f,
                hasVisualOverflow: true
            );
            return Mathf.Min(0.0f, paintExtent ?? 0.0f - this.childExtent);
        }

        public void maybeStartSnapAnimation(ScrollDirection direction) {
            if (this.snapConfiguration == null) {
                return;
            }

            if (direction == ScrollDirection.forward && this._effectiveScrollOffset <= 0.0f) {
                return;
            }

            if (direction == ScrollDirection.reverse && this._effectiveScrollOffset >= this.maxExtent) {
                return;
            }

            TickerProvider vsync = this.snapConfiguration.vsync;
            TimeSpan duration = this.snapConfiguration.duration;
            this._controller = this._controller ?? new AnimationController(vsync: vsync, duration: duration);
            this._controller.addListener(() => {
                if (this._effectiveScrollOffset == this._animation.value) {
                    return;
                }

                this._effectiveScrollOffset = this._animation.value;
                this.markNeedsLayout();
            });

            this._animation = this._controller.drive(
                new FloatTween(
                    begin: this._effectiveScrollOffset,
                    end: direction == ScrollDirection.forward ? 0.0f : this.maxExtent ?? 0.0f
                ).chain(new CurveTween(
                    curve: this.snapConfiguration.curve
                ))
            );

            this._controller.forward(from: 0.0f);
        }

        public void maybeStopSnapAnimation(ScrollDirection direction) {
            this._controller?.stop();
        }

        protected override void performLayout() {
            float? maxExtent = this.maxExtent;
            if (((this.constraints.scrollOffset < this._lastActualScrollOffset) ||
                 (this._effectiveScrollOffset < maxExtent))) {
                float delta = this._lastActualScrollOffset - this.constraints.scrollOffset;
                bool allowFloatingExpansion = this.constraints.userScrollDirection == ScrollDirection.forward;
                if (allowFloatingExpansion) {
                    if (this._effectiveScrollOffset > maxExtent) {
                        this._effectiveScrollOffset = maxExtent ?? 0.0f;
                    }
                }
                else {
                    if (delta > 0.0f) {
                        delta = 0.0f;
                    }
                }

                this._effectiveScrollOffset =
                    (this._effectiveScrollOffset - delta).clamp(0.0f, this.constraints.scrollOffset);
            }
            else {
                this._effectiveScrollOffset = this.constraints.scrollOffset;
            }

            bool overlapsContent = this._effectiveScrollOffset < this.constraints.scrollOffset;
            this.excludeFromSemanticsScrolling = overlapsContent;
            this.layoutChild(this._effectiveScrollOffset, maxExtent ?? 0.0f, overlapsContent: overlapsContent);
            this._childPosition = this.updateGeometry();
            this._lastActualScrollOffset = this.constraints.scrollOffset;
        }

        public override float childMainAxisPosition(RenderObject child) {
            D.assert(child == this.child);
            return this._childPosition;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("effective scroll offset", this._effectiveScrollOffset));
        }
    }

    public abstract class RenderSliverFloatingPinnedPersistentHeader : RenderSliverFloatingPersistentHeader {
        public RenderSliverFloatingPinnedPersistentHeader(
            RenderBox child = null,
            FloatingHeaderSnapConfiguration snapConfiguration = null
        ) : base(child: child, snapConfiguration: snapConfiguration) {
        }

        protected override float updateGeometry() {
            float? minExtent = this.minExtent;
            float? minAllowedExtent = this.constraints.remainingPaintExtent > minExtent
                ? minExtent
                : this.constraints.remainingPaintExtent;
            float? maxExtent = this.maxExtent;
            float? paintExtent = maxExtent - this._effectiveScrollOffset;
            float? clampedPaintExtent =
                paintExtent?.clamp(minAllowedExtent ?? 0.0f, this.constraints.remainingPaintExtent);
            float? layoutExtent = maxExtent - this.constraints.scrollOffset;
            this.geometry = new SliverGeometry(
                scrollExtent: maxExtent ?? 0.0f,
                paintExtent: clampedPaintExtent ?? 0.0f,
                layoutExtent: layoutExtent?.clamp(0.0f, clampedPaintExtent ?? 0.0f),
                maxPaintExtent: maxExtent ?? 0.0f,
                maxScrollObstructionExtent: maxExtent ?? 0.0f,
                hasVisualOverflow: true
            );
            return 0.0f;
        }
    }
}