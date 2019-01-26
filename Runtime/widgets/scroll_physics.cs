using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.ui;

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

        public virtual double applyPhysicsToUserOffset(ScrollMetrics position, double offset) {
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

        public virtual double applyBoundaryConditions(ScrollMetrics position, double value) {
            if (this.parent == null) {
                return 0.0;
            }

            return this.parent.applyBoundaryConditions(position, value);
        }

        public virtual Simulation createBallisticSimulation(ScrollMetrics position, double velocity) {
            if (this.parent == null) {
                return null;
            }

            return this.parent.createBallisticSimulation(position, velocity);
        }

        static readonly SpringDescription _kDefaultSpring = SpringDescription.withDampingRatio(
            mass: 0.5,
            stiffness: 100.0,
            ratio: 1.1
        );

        public virtual SpringDescription spring {
            get {
                if (this.parent == null) {
                    return _kDefaultSpring;
                }

                return this.parent.spring ?? _kDefaultSpring;
            }
        }

        // todo: Handle the case of the device pixel ratio changing. use 1 as devicePixelRatio for now.
        static readonly Tolerance _kDefaultTolerance = new Tolerance(
            velocity: 1.0 / (0.050 * 1),
            distance: 1.0 / 1
        );

        public virtual Tolerance tolerance {
            get {
                if (this.parent == null) {
                    return _kDefaultTolerance;
                }

                return this.parent.tolerance ?? _kDefaultTolerance;
            }
        }

        public virtual double minFlingDistance {
            get {
                if (this.parent == null) {
                    return Constants.kTouchSlop;
                }

                return this.parent.minFlingDistance;
            }
        }

        public virtual double carriedMomentum(double existingVelocity) {
            if (this.parent == null) {
                return 0.0;
            }

            return this.parent.carriedMomentum(existingVelocity);
        }

        public virtual double minFlingVelocity {
            get {
                if (this.parent == null) {
                    return Constants.kMinFlingVelocity;
                }

                return this.parent.minFlingVelocity;
            }
        }

        public virtual double maxFlingVelocity {
            get {
                if (this.parent == null) {
                    return Constants.kMaxFlingVelocity;
                }

                return this.parent.maxFlingVelocity;
            }
        }

        public virtual double dragStartDistanceMotionThreshold {
            get {
                if (this.parent == null) {
                    return 0.0;
                }

                return this.parent.dragStartDistanceMotionThreshold;
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

        public double frictionFactor(double overscrollFraction) {
            return 0.52 * Math.Pow(1 - overscrollFraction, 2);
        }

        public override double applyPhysicsToUserOffset(ScrollMetrics position, double offset) {
            D.assert(position.minScrollExtent <= position.maxScrollExtent);

            if (!position.outOfRange()) {
                return offset;
            }

            double overscrollPastStart = Math.Max(position.minScrollExtent - position.pixels, 0.0);
            double overscrollPastEnd = Math.Max(position.pixels - position.maxScrollExtent, 0.0);
            double overscrollPast = Math.Max(overscrollPastStart, overscrollPastEnd);
            bool easing = (overscrollPastStart > 0.0 && offset < 0.0) || (overscrollPastEnd > 0.0 && offset > 0.0);

            double friction = easing
                ? this.frictionFactor((overscrollPast - offset.abs()) / position.viewportDimension)
                : this.frictionFactor(overscrollPast / position.viewportDimension);
            double direction = offset.sign();

            return direction * _applyFriction(overscrollPast, offset.abs(), friction);
        }

        static double _applyFriction(double extentOutside, double absDelta, double gamma) {
            D.assert(absDelta > 0);
            double total = 0.0;
            if (extentOutside > 0) {
                double deltaToLimit = extentOutside / gamma;
                if (absDelta < deltaToLimit) {
                    return absDelta * gamma;
                }

                total += extentOutside;
                absDelta -= deltaToLimit;
            }

            return total + absDelta;
        }

        public override double applyBoundaryConditions(ScrollMetrics position, double value) {
            return 0.0;
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, double velocity) {
            Tolerance tolerance = this.tolerance;
            if (velocity.abs() >= tolerance.velocity || position.outOfRange()) {
                return new BouncingScrollSimulation(
                    spring: this.spring,
                    position: position.pixels,
                    velocity: velocity * 0.91,
                    leadingExtent: position.minScrollExtent,
                    trailingExtent: position.maxScrollExtent,
                    tolerance: tolerance
                );
            }

            return null;
        }

        public override double minFlingVelocity {
            get { return Constants.kMinFlingVelocity * 2.0; }
        }

        public override double carriedMomentum(double existingVelocity) {
            return existingVelocity.sign() * Math.Min(0.000816 * Math.Pow(existingVelocity.abs(), 1.967), 40000.0);
        }

        public override double dragStartDistanceMotionThreshold {
            get { return 3.5; }
        }
    }


    public class ClampingScrollPhysics : ScrollPhysics {
        public ClampingScrollPhysics(ScrollPhysics parent = null) : base(parent: parent) {
        }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new ClampingScrollPhysics(parent: this.buildParent(ancestor));
        }

        public override double applyBoundaryConditions(ScrollMetrics position, double value) {
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

            return 0.0;
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, double velocity) {
            Tolerance tolerance = this.tolerance;
            if (position.outOfRange()) {
                double? end = null;
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
                    Math.Min(0.0, velocity),
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