using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class ScrollController : ChangeNotifier {
        public ScrollController(
            float initialScrollOffset = 0.0f,
            bool keepScrollOffset = true,
            string debugLabel = null
        ) {
            this._initialScrollOffset = initialScrollOffset;
            this.keepScrollOffset = keepScrollOffset;
            this.debugLabel = debugLabel;
        }

        public virtual float initialScrollOffset {
            get { return this._initialScrollOffset; }
        }

        readonly float _initialScrollOffset;

        public readonly bool keepScrollOffset;

        public readonly string debugLabel;

        public ICollection<ScrollPosition> positions {
            get { return this._positions; }
        }

        readonly List<ScrollPosition> _positions = new List<ScrollPosition>();

        public bool hasClients {
            get { return this._positions.isNotEmpty(); }
        }

        public ScrollPosition position {
            get {
                D.assert(this._positions.isNotEmpty(), () => "ScrollController not attached to any scroll views.");
                D.assert(this._positions.Count == 1, () => "ScrollController attached to multiple scroll views.");
                return this._positions.Single();
            }
        }

        public float offset {
            get { return this.position.pixels; }
        }


        public IPromise animateTo(float to,
            TimeSpan duration,
            Curve curve
        ) {
            D.assert(this._positions.isNotEmpty(), () => "ScrollController not attached to any scroll views.");
            List<IPromise> animations = CollectionUtils.CreateRepeatedList<IPromise>(null, this._positions.Count);
            for (int i = 0; i < this._positions.Count; i += 1) {
                animations[i] = this._positions[i].animateTo(to, duration: duration, curve: curve);
            }

            return Promise.All(animations);
        }

        public void jumpTo(float value) {
            D.assert(this._positions.isNotEmpty(), () => "ScrollController not attached to any scroll views.");
            foreach (ScrollPosition position in new List<ScrollPosition>(this._positions)) {
                position.jumpTo(value);
            }
        }

        public virtual void attach(ScrollPosition position) {
            D.assert(!this._positions.Contains(position));
            this._positions.Add(position);
            position.addListener(this.notifyListeners);
        }

        public virtual void detach(ScrollPosition position) {
            D.assert(this._positions.Contains(position));
            position.removeListener(this.notifyListeners);
            this._positions.Remove(position);
        }

        public override void dispose() {
            foreach (ScrollPosition position in this._positions) {
                position.removeListener(this.notifyListeners);
            }

            base.dispose();
        }

        public virtual ScrollPosition createScrollPosition(
            ScrollPhysics physics,
            ScrollContext context,
            ScrollPosition oldPosition
        ) {
            return new ScrollPositionWithSingleContext(
                physics: physics,
                context: context,
                initialPixels: this.initialScrollOffset,
                keepScrollOffset: this.keepScrollOffset,
                oldPosition: oldPosition,
                debugLabel: this.debugLabel
            );
        }

        public override string ToString() {
            List<string> description = new List<string>();
            this.debugFillDescription(description);
            return $"{Diagnostics.describeIdentity(this)}({string.Join(", ", description.ToArray())})";
        }

        protected virtual void debugFillDescription(List<string> description) {
            if (this.debugLabel != null) {
                description.Add(this.debugLabel);
            }

            if (this.initialScrollOffset != 0.0) {
                description.Add($"initialScrollOffset: {this.initialScrollOffset:F1}, ");
            }

            if (this._positions.isEmpty()) {
                description.Add("no clients");
            }
            else if (this._positions.Count == 1) {
                description.Add($"one client, offset {this.offset:F1}");
            }
            else {
                description.Add(this._positions.Count + " clients");
            }
        }
    }

    public class TrackingScrollController : ScrollController {
        public TrackingScrollController(
            float initialScrollOffset = 0.0f,
            bool keepScrollOffset = true,
            string debugLabel = null
        ) : base(initialScrollOffset: initialScrollOffset,
            keepScrollOffset: keepScrollOffset,
            debugLabel: debugLabel) {
        }

        readonly Dictionary<ScrollPosition, VoidCallback> _positionToListener =
            new Dictionary<ScrollPosition, VoidCallback>();

        ScrollPosition _lastUpdated;
        float? _lastUpdatedOffset;

        public ScrollPosition mostRecentlyUpdatedPosition {
            get { return this._lastUpdated; }
        }

        public override float initialScrollOffset {
            get { return this._lastUpdatedOffset ?? base.initialScrollOffset; }
        }

        public override void attach(ScrollPosition position) {
            base.attach(position);
            D.assert(!this._positionToListener.ContainsKey(position));
            this._positionToListener[position] = () => {
                this._lastUpdated = position;
                this._lastUpdatedOffset = position.pixels;
            };
            position.addListener(this._positionToListener[position]);
        }

        public override void detach(ScrollPosition position) {
            base.detach(position);
            D.assert(this._positionToListener.ContainsKey(position));
            position.removeListener(this._positionToListener[position]);
            this._positionToListener.Remove(position);
            if (this._lastUpdated == position) {
                this._lastUpdated = null;
            }

            if (this._positionToListener.isEmpty()) {
                this._lastUpdatedOffset = null;
            }
        }

        public override void dispose() {
            foreach (ScrollPosition position in this.positions) {
                D.assert(this._positionToListener.ContainsKey(position));
                position.removeListener(this._positionToListener[position]);
            }

            base.dispose();
        }
    }
}