using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class ScrollPhysics {
        public ScrollPhysics(ScrollPhysics parent) {
            this.parent = parent;
        }

        public readonly ScrollPhysics parent;

        protected ScrollPhysics buildParent(ScrollPhysics ancestor) {
            if (this.parent == null) {
                return ancestor;
            }

            return this.parent.applyTo(ancestor) ?? ancestor;
        }

        public virtual ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new ScrollPhysics(parent: this.buildParent(ancestor));
        }

        public virtual float applyPhysicsToUserOffset(ScrollMetrics position, float offset) {
            if (this.parent == null) {
                return offset;
            }

            return this.parent.applyPhysicsToUserOffset(position, offset);
        }

        public virtual bool shouldAcceptUserOffset(ScrollMetrics position) {
            if (this.parent == null) {
                return position.pixels != 0.0 || position.minScrollExtent != position.maxScrollExtent;
            }

            return this.parent.shouldAcceptUserOffset(position);
        }

        public virtual float applyBoundaryConditions(ScrollMetrics position, float value) {
            if (this.parent == null) {
                return 0.0f;
            }

            return this.parent.applyBoundaryConditions(position, value);
        }

        public virtual Simulation createBallisticSimulation(ScrollMetrics position, float velocity) {
            if (this.parent == null) {
                return null;
            }

            return this.parent.createBallisticSimulation(position, velocity);
        }

        static readonly SpringDescription _kDefaultSpring = SpringDescription.withDampingRatio(
            mass: 0.5f,
            stiffness: 100.0f,
            ratio: 1.1f
        );

        public virtual SpringDescription spring {
            get {
                if (this.parent == null) {
                    return _kDefaultSpring;
                }

                return this.parent.spring ?? _kDefaultSpring;
            }
        }

        static readonly Tolerance _kDefaultTolerance = new Tolerance(
            velocity: 1.0f / (0.050f * Window.instance.devicePixelRatio),
            distance: 1.0f / Window.instance.devicePixelRatio
        );

        public virtual Tolerance tolerance {
            get {
                if (this.parent == null) {
                    return _kDefaultTolerance;
                }

                return this.parent.tolerance ?? _kDefaultTolerance;
            }
        }

        public virtual float minFlingDistance {
            get {
                if (this.parent == null) {
                    return Constants.kTouchSlop;
                }

                return this.parent.minFlingDistance;
            }
        }

        public virtual float carriedMomentum(float existingVelocity) {
            if (this.parent == null) {
                return 0.0f;
            }

            return this.parent.carriedMomentum(existingVelocity);
        }

        public virtual float minFlingVelocity {
            get {
                if (this.parent == null) {
                    return Constants.kMinFlingVelocity;
                }

                return this.parent.minFlingVelocity;
            }
        }

        public virtual float maxFlingVelocity {
            get {
                if (this.parent == null) {
                    return Constants.kMaxFlingVelocity;
                }

                return this.parent.maxFlingVelocity;
            }
        }

        public virtual float? dragStartDistanceMotionThreshold {
            get {
                return this.parent?.dragStartDistanceMotionThreshold;
            }
        }

        public virtual bool allowImplicitScrolling {
            get { return true; }
        }

        public override string ToString() {
            if (this.parent == null) {
                return $"{this.GetType()}";
            }

            return $"{this.GetType()} -> {this.parent}";
        }
    }


    public class BouncingScrollPhysics : ScrollPhysics {
        public BouncingScrollPhysics(ScrollPhysics parent = null) : base(parent: parent) {
        }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new BouncingScrollPhysics(parent: this.buildParent(ancestor));
        }

        public float frictionFactor(float overscrollFraction) {
            return 0.52f * Mathf.Pow(1 - overscrollFraction, 2);
        }

        public override float applyPhysicsToUserOffset(ScrollMetrics position, float offset) {
            D.assert(position.minScrollExtent <= position.maxScrollExtent);

            if (!position.outOfRange()) {
                return offset;
            }

            float overscrollPastStart = Mathf.Max(position.minScrollExtent - position.pixels, 0.0f);
            float overscrollPastEnd = Mathf.Max(position.pixels - position.maxScrollExtent, 0.0f);
            float overscrollPast = Mathf.Max(overscrollPastStart, overscrollPastEnd);
            bool easing = (overscrollPastStart > 0.0f && offset < 0.0f) || (overscrollPastEnd > 0.0f && offset > 0.0f);

            float friction = easing
                ? this.frictionFactor((overscrollPast - offset.abs()) / position.viewportDimension)
                : this.frictionFactor(overscrollPast / position.viewportDimension);
            float direction = offset.sign();

            return direction * _applyFriction(overscrollPast, offset.abs(), friction);
        }

        static float _applyFriction(float extentOutside, float absDelta, float gamma) {
            D.assert(absDelta > 0);
            float total = 0.0f;
            if (extentOutside > 0) {
                float deltaToLimit = extentOutside / gamma;
                if (absDelta < deltaToLimit) {
                    return absDelta * gamma;
                }

                total += extentOutside;
                absDelta -= deltaToLimit;
            }

            return total + absDelta;
        }

        public override float applyBoundaryConditions(ScrollMetrics position, float value) {
            return 0.0f;
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, float velocity) {
            Tolerance tolerance = this.tolerance;
            if (velocity.abs() >= tolerance.velocity || position.outOfRange()) {
                return new BouncingScrollSimulation(
                    spring: this.spring,
                    position: position.pixels,
                    velocity: velocity * 0.91f,
                    leadingExtent: position.minScrollExtent,
                    trailingExtent: position.maxScrollExtent,
                    tolerance: tolerance
                );
            }

            return null;
        }

        public override float minFlingVelocity {
            get { return Constants.kMinFlingVelocity * 2.0f; }
        }

        public override float carriedMomentum(float existingVelocity) {
            return existingVelocity.sign() *
                   Mathf.Min(0.000816f * Mathf.Pow(existingVelocity.abs(), 1.967f), 40000.0f);
        }

        public override float? dragStartDistanceMotionThreshold {
            get { return 3.5f; }
        }
    }


    public class ClampingScrollPhysics : ScrollPhysics {
        public ClampingScrollPhysics(ScrollPhysics parent = null) : base(parent: parent) {
        }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new ClampingScrollPhysics(parent: this.buildParent(ancestor));
        }

        public override float applyBoundaryConditions(ScrollMetrics position, float value) {
            D.assert(() => {
                if (value == position.pixels) {
                    throw new UIWidgetsError(
                        $"{this.GetType()}.applyBoundaryConditions() was called redundantly.\n" +
                        $"The proposed new position, {value}, is exactly equal to the current position of the " +
                        $"given {position.GetType()}, {position.pixels}.\n" +
                        "The applyBoundaryConditions method should only be called when the value is " +
                        "going to actually change the pixels, otherwise it is redundant.\n" +
                        "The physics object in question was:\n" + $"  {this}\n" +
                        "The position object in question was:\n" + $"  {position}\n");
                }

                return true;
            });
            if (value < position.pixels && position.pixels <= position.minScrollExtent) {
                return value - position.pixels;
            }

            if (position.maxScrollExtent <= position.pixels && position.pixels < value) {
                return value - position.pixels;
            }

            if (value < position.minScrollExtent && position.minScrollExtent < position.pixels) {
                return value - position.minScrollExtent;
            }

            if (position.pixels < position.maxScrollExtent && position.maxScrollExtent < value) {
                return value - position.maxScrollExtent;
            }

            return 0.0f;
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, float velocity) {
            Tolerance tolerance = this.tolerance;
            if (position.outOfRange()) {
                float? end = null;
                if (position.pixels > position.maxScrollExtent) {
                    end = position.maxScrollExtent;
                }

                if (position.pixels < position.minScrollExtent) {
                    end = position.minScrollExtent;
                }

                D.assert(end != null);
                return new ScrollSpringSimulation(
                    this.spring,
                    position.pixels,
                    position.maxScrollExtent,
                    Mathf.Min(0.0f, velocity),
                    tolerance: tolerance
                );
            }

            if (velocity.abs() < tolerance.velocity) {
                return null;
            }

            if (velocity > 0.0 && position.pixels >= position.maxScrollExtent) {
                return null;
            }

            if (velocity < 0.0 && position.pixels <= position.minScrollExtent) {
                return null;
            }

            return new ClampingScrollSimulation(
                position: position.pixels,
                velocity: velocity,
                tolerance: tolerance
            );
        }
    }

    public class AlwaysScrollableScrollPhysics : ScrollPhysics {
        public AlwaysScrollableScrollPhysics(ScrollPhysics parent = null) : base(parent: parent) {
        }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new AlwaysScrollableScrollPhysics(parent: this.buildParent(ancestor));
        }

        public override bool shouldAcceptUserOffset(ScrollMetrics position) {
            return true;
        }
    }

    public class NeverScrollableScrollPhysics : ScrollPhysics {
        public NeverScrollableScrollPhysics(ScrollPhysics parent = null) : base(parent: parent) {
        }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new NeverScrollableScrollPhysics(parent: this.buildParent(ancestor));
        }

        public override bool shouldAcceptUserOffset(ScrollMetrics position) {
            return false;
        }

        public override bool allowImplicitScrolling {
            get { return false; }
        }
    }
}