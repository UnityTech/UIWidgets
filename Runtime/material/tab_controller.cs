using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class TabController : ChangeNotifier {
        public TabController(
            int initialIndex = 0,
            int? length = null,
            TickerProvider vsync = null) {
            D.assert(length != null && length >= 0);
            D.assert(initialIndex >= 0 && (length == 0 || initialIndex < length));
            D.assert(vsync != null);
            this.length = length.Value;

            this._index = initialIndex;
            this._previousIndex = initialIndex;
            this._animationController = length < 2
                ? null
                : new AnimationController(
                    value: initialIndex,
                    upperBound: length.Value - 1,
                    vsync: vsync);
        }

        public Animation<float> animation {
            get { return this._animationController?.view ?? Animations.kAlwaysCompleteAnimation; }
        }

        readonly AnimationController _animationController;

        public readonly int length;

        void _changeIndex(int value, TimeSpan? duration = null, Curve curve = null) {
            D.assert(value >= 0 && (value < this.length || this.length == 0));
            D.assert(duration == null ? curve == null : true);
            D.assert(this._indexIsChangingCount >= 0);

            if (value == this._index || this.length < 2) {
                return;
            }

            this._previousIndex = this.index;
            this._index = value;
            if (duration != null) {
                this._indexIsChangingCount++;
                this.notifyListeners();
                this._animationController.animateTo(
                    this._index, duration: duration, curve: curve).whenCompleteOrCancel(() => {
                    this._indexIsChangingCount--;
                    this.notifyListeners();
                });
            }
            else {
                this._indexIsChangingCount++;
                this._animationController.setValue(this._index);
                this._indexIsChangingCount--;
                this.notifyListeners();
            }
        }

        public int index {
            get { return this._index; }
            set { this._changeIndex(value); }
        }

        int _index;

        public int previousIndex {
            get { return this._previousIndex; }
        }

        int _previousIndex;

        public bool indexIsChanging {
            get { return this._indexIsChangingCount != 0; }
        }

        int _indexIsChangingCount = 0;

        public void animateTo(int value, TimeSpan? duration = null, Curve curve = null) {
            duration = duration ?? Constants.kTabScrollDuration;
            curve = curve ?? Curves.ease;
            this._changeIndex(value, duration: duration, curve: curve);
        }

        public float offset {
            get { return this.length > 1 ? this._animationController.value - this._index : 0.0f; }
            set {
                D.assert(this.length > 1);
                D.assert(value >= -1.0f && value <= 1.0f);
                D.assert(!this.indexIsChanging);
                if (value == this.offset) {
                    return;
                }

                this._animationController.setValue(value + this._index);
            }
        }

        public override void dispose() {
            this._animationController?.dispose();
            base.dispose();
        }
    }

    class _TabControllerScope : InheritedWidget {
        public _TabControllerScope(
            Key key = null,
            TabController controller = null,
            bool? enabled = null,
            Widget child = null
        ) : base(key: key, child: child) {
            this.controller = controller;
            this.enabled = enabled;
        }

        public readonly TabController controller;

        public readonly bool? enabled;

        public override bool updateShouldNotify(InheritedWidget old) {
            _TabControllerScope _old = (_TabControllerScope) old;
            return this.enabled != _old.enabled
                   || this.controller != _old.controller;
        }
    }

    public class DefaultTabController : StatefulWidget {
        public DefaultTabController(
            Key key = null,
            int? length = null,
            int initialIndex = 0,
            Widget child = null
        ) : base(key: key) {
            D.assert(length != null);
            D.assert(child != null);
            this.length = length;
            this.initialIndex = initialIndex;
            this.child = child;
        }

        public readonly int? length;

        public readonly int initialIndex;

        public readonly Widget child;

        public static TabController of(BuildContext context) {
            _TabControllerScope scope =
                (_TabControllerScope) context.inheritFromWidgetOfExactType(typeof(_TabControllerScope));
            return scope?.controller;
        }

        public override State createState() {
            return new _DefaultTabControllerState();
        }
    }

    class _DefaultTabControllerState : SingleTickerProviderStateMixin<DefaultTabController> {
        TabController _controller;

        public override void initState() {
            base.initState();
            this._controller = new TabController(
                vsync: this,
                length: this.widget.length,
                initialIndex: this.widget.initialIndex
            );
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new _TabControllerScope(
                controller: this._controller,
                enabled: TickerMode.of(context),
                child: this.widget.child
            );
        }
    }
}