using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class ScrollPositionWithSingleContext : ScrollPosition, ScrollActivityDelegate {
        public ScrollPositionWithSingleContext(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            float? initialPixels = 0.0f,
            bool keepScrollOffset = true,
            ScrollPosition oldPosition = null,
            string debugLabel = null
        ) : base(
            physics: physics,
            context: context,
            keepScrollOffset: keepScrollOffset,
            oldPosition: oldPosition,
            debugLabel: debugLabel
        ) {
            if (this._pixels == null && initialPixels != null) {
                this.correctPixels(initialPixels.Value);
            }

            if (this.activity == null) {
                this.goIdle();
            }

            D.assert(this.activity != null);
        }


        float _heldPreviousVelocity = 0.0f;

        public override AxisDirection axisDirection {
            get { return this.context.axisDirection; }
        }

        public override float setPixels(float newPixels) {
            D.assert(this.activity.isScrolling);
            return base.setPixels(newPixels);
        }

        protected override void absorb(ScrollPosition other) {
            base.absorb(other);
            if (!(other is ScrollPositionWithSingleContext)) {
                this.goIdle();
                return;
            }

            this.activity.updateDelegate(this);
            ScrollPositionWithSingleContext typedOther = (ScrollPositionWithSingleContext) other;
            this._userScrollDirection = typedOther._userScrollDirection;
            D.assert(this._currentDrag == null);
            if (typedOther._currentDrag != null) {
                this._currentDrag = typedOther._currentDrag;
                this._currentDrag.updateDelegate(this);
                typedOther._currentDrag = null;
            }
        }

        protected override void applyNewDimensions() {
            base.applyNewDimensions();
            this.context.setCanDrag(this.physics.shouldAcceptUserOffset(this));
        }

        public override void beginActivity(ScrollActivity newActivity) {
            this._heldPreviousVelocity = 0.0f;
            if (newActivity == null) {
                return;
            }

            D.assert(newActivity.del == this);
            base.beginActivity(newActivity);
            if (this._currentDrag != null) {
                this._currentDrag.dispose();
                this._currentDrag = null;
            }

            if (!this.activity.isScrolling) {
                this.updateUserScrollDirection(ScrollDirection.idle);
            }
        }

        public virtual void applyUserScrollOffset(float delta) {
            this.updateUserScrollDirection(delta > 0.0 ? ScrollDirection.forward : ScrollDirection.reverse);

            var pixel = this.pixels - this.physics.applyPhysicsToUserOffset(this, delta);
            if (pixel < this.minScrollExtent) {
                pixel = this.minScrollExtent;
            }

            if (pixel > this.maxScrollExtent) {
                pixel = this.maxScrollExtent;
            }

            this.setPixels(pixel);
        }

        public virtual void applyUserOffset(float delta) {
            this.updateUserScrollDirection(delta > 0.0 ? ScrollDirection.forward : ScrollDirection.reverse);
            this.setPixels(this.pixels - this.physics.applyPhysicsToUserOffset(this, delta));
        }

        public void goIdle() {
            this.beginActivity(new IdleScrollActivity(this));
        }

        public void goBallistic(float velocity) {
            D.assert(this._pixels != null);
            Simulation simulation = this.physics.createBallisticSimulation(this, velocity);
            if (simulation != null) {
                this.beginActivity(new BallisticScrollActivity(this, simulation, this.context.vsync));
            }
            else {
                this.goIdle();
            }
        }

        public override ScrollDirection userScrollDirection {
            get { return this._userScrollDirection; }
        }

        ScrollDirection _userScrollDirection = ScrollDirection.idle;

        protected void updateUserScrollDirection(ScrollDirection value) {
            if (this.userScrollDirection == value) {
                return;
            }

            this._userScrollDirection = value;
            this.didUpdateScrollDirection(value);
        }

        public override IPromise animateTo(float to,
            TimeSpan duration,
            Curve curve
        ) {
            if (PhysicsUtils.nearEqual(to, this.pixels, this.physics.tolerance.distance)) {
                this.jumpTo(to);
                return Promise.Resolved();
            }

            DrivenScrollActivity activity = new DrivenScrollActivity(
                this,
                from: this.pixels,
                to: to,
                duration: duration,
                curve: curve,
                vsync: this.context.vsync
            );
            this.beginActivity(activity);
            return activity.done;
        }

        public override void jumpTo(float value) {
            this.goIdle();
            if (this.pixels != value) {
                float oldPixels = this.pixels;
                this.forcePixels(value);
                // this.notifyListeners(); already in forcePixels, no need here.
                this.didStartScroll();
                this.didUpdateScrollPositionBy(this.pixels - oldPixels);
                this.didEndScroll();
            }

            this.goBallistic(0.0f);
        }

        public override ScrollHoldController hold(VoidCallback holdCancelCallback) {
            float previousVelocity = this.activity.velocity;
            HoldScrollActivity holdActivity = new HoldScrollActivity(
                del: this,
                onHoldCanceled: holdCancelCallback
            );
            this.beginActivity(holdActivity);
            this._heldPreviousVelocity = previousVelocity;
            return holdActivity;
        }

        ScrollDragController _currentDrag;

        public override Drag drag(DragStartDetails details, VoidCallback dragCancelCallback) {
            ScrollDragController drag = new ScrollDragController(
                del: this,
                details: details,
                onDragCanceled: dragCancelCallback,
                carriedVelocity: this.physics.carriedMomentum(this._heldPreviousVelocity),
                motionStartDistanceThreshold: this.physics.dragStartDistanceMotionThreshold
            );
            this.beginActivity(new DragScrollActivity(this, drag));
            D.assert(this._currentDrag == null);
            this._currentDrag = drag;
            return drag;
        }

        public override void dispose() {
            if (this._currentDrag != null) {
                this._currentDrag.dispose();
                this._currentDrag = null;
            }

            base.dispose();
        }

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add(this.context.GetType().ToString());
            description.Add(this.physics.ToString());
            description.Add(this.activity?.ToString());
            description.Add(this.userScrollDirection.ToString());
        }
    }
}