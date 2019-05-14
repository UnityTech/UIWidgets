using System;
using System.Collections.Generic;
using System.Linq;
using RSG.Promises;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.widgets {
    class _ChildEntry {
        public _ChildEntry(
            AnimationController controller,
            Animation<float> animation,
            Widget transition,
            Widget widgetChild
        ) {
            D.assert(animation != null);
            D.assert(transition != null);
            D.assert(controller != null);
            this.controller = controller;
            this.animation = animation;
            this.transition = transition;
            this.widgetChild = widgetChild;
        }

        public readonly AnimationController controller;

        public readonly Animation<float> animation;

        public Widget transition;

        public Widget widgetChild;

        public override string ToString() {
            return "Entry#${shortHash(this)}($widgetChild)";
        }
    }

    public delegate Widget AnimatedSwitcherTransitionBuilder(Widget child, Animation<float> animation);

    public delegate Widget AnimatedSwitcherLayoutBuilder(Widget currentChild, List<Widget> previousChildren);

    public class AnimatedSwitcher : StatefulWidget {
        public AnimatedSwitcher(
            Key key = null,
            Widget child = null,
            TimeSpan? duration = null,
            Curve switchInCurve = null,
            Curve switchOutCurve = null,
            AnimatedSwitcherTransitionBuilder transitionBuilder = null,
            AnimatedSwitcherLayoutBuilder layoutBuilder = null
        ) : base(key: key) {
            D.assert(duration != null);
            this.switchInCurve = switchInCurve ?? Curves.linear;
            this.switchOutCurve = switchOutCurve ?? Curves.linear;
            this.transitionBuilder = transitionBuilder ?? defaultTransitionBuilder;
            this.layoutBuilder = layoutBuilder ?? defaultLayoutBuilder;
            this.child = child;
            this.duration = duration;
        }

        public readonly Widget child;

        public readonly TimeSpan? duration;

        public readonly Curve switchInCurve;

        public readonly Curve switchOutCurve;

        public readonly AnimatedSwitcherTransitionBuilder transitionBuilder;

        public readonly AnimatedSwitcherLayoutBuilder layoutBuilder;

        public override State createState() {
            return new _AnimatedSwitcherState();
        }

        public static Widget defaultTransitionBuilder(Widget child, Animation<float> animation) {
            return new FadeTransition(
                opacity: animation,
                child: child
            );
        }

        public static Widget defaultLayoutBuilder(Widget currentChild, List<Widget> previousChildren) {
            List<Widget> children = previousChildren;
            if (currentChild != null) {
                children = children.ToList();
                children.Add(currentChild);
            }

            return new Stack(
                children: children,
                alignment: Alignment.center
            );
        }
    }

    class _AnimatedSwitcherState : TickerProviderStateMixin<AnimatedSwitcher> {
        _ChildEntry _currentEntry;
        HashSet<_ChildEntry> _outgoingEntries = new HashSet<_ChildEntry>();
        List<Widget> _outgoingWidgets = new List<Widget>();
        int _childNumber = 0;

        public override void initState() {
            base.initState();
            this._addEntryForNewChild(animate: false);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            AnimatedSwitcher oldWidget = _oldWidget as AnimatedSwitcher;

            if (this.widget.transitionBuilder != oldWidget.transitionBuilder) {
                this._outgoingEntries.Each(this._updateTransitionForEntry);
                if (this._currentEntry != null) {
                    this._updateTransitionForEntry(this._currentEntry);
                }

                this._markChildWidgetCacheAsDirty();
            }

            bool hasNewChild = this.widget.child != null;
            bool hasOldChild = this._currentEntry != null;
            if (hasNewChild != hasOldChild ||
                hasNewChild && !Widget.canUpdate(this.widget.child, this._currentEntry.widgetChild)) {
                this._childNumber += 1;
                this._addEntryForNewChild(animate: true);
            }
            else if (this._currentEntry != null) {
                D.assert(hasOldChild && hasNewChild);
                D.assert(Widget.canUpdate(this.widget.child, this._currentEntry.widgetChild));
                this._currentEntry.widgetChild = this.widget.child;
                this._updateTransitionForEntry(this._currentEntry);
                this._markChildWidgetCacheAsDirty();
            }
        }

        void _addEntryForNewChild(bool animate) {
            D.assert(animate || this._currentEntry == null);
            if (this._currentEntry != null) {
                D.assert(animate);
                D.assert(!this._outgoingEntries.Contains(this._currentEntry));
                this._outgoingEntries.Add(this._currentEntry);
                this._currentEntry.controller.reverse();
                this._markChildWidgetCacheAsDirty();
                this._currentEntry = null;
            }

            if (this.widget.child == null) {
                return;
            }

            AnimationController controller = new AnimationController(
                duration: this.widget.duration,
                vsync: this
            );
            Animation<float> animation = new CurvedAnimation(
                parent: controller,
                curve: this.widget.switchInCurve,
                reverseCurve: this.widget.switchOutCurve
            );
            this._currentEntry = this._newEntry(
                child: this.widget.child,
                controller: controller,
                animation: animation,
                builder: this.widget.transitionBuilder
            );
            if (animate) {
                controller.forward();
            }
            else {
                D.assert(this._outgoingEntries.isEmpty);
                controller.setValue(1.0f);
            }
        }

        _ChildEntry _newEntry(
            Widget child,
            AnimatedSwitcherTransitionBuilder builder,
            AnimationController controller,
            Animation<float> animation
        ) {
            _ChildEntry entry = new _ChildEntry(
                widgetChild: child,
                transition: KeyedSubtree.wrap(builder(child, animation), this._childNumber),
                animation: animation,
                controller: controller
            );
            animation.addStatusListener((AnimationStatus status) => {
                if (status == AnimationStatus.dismissed) {
                    this.setState(() => {
                        D.assert(this.mounted);
                        D.assert(this._outgoingEntries.Contains(entry));
                        this._outgoingEntries.Remove(entry);
                        this._markChildWidgetCacheAsDirty();
                    });
                    controller.dispose();
                }
            });
            return entry;
        }

        void _markChildWidgetCacheAsDirty() {
            this._outgoingWidgets = null;
        }

        void _updateTransitionForEntry(_ChildEntry entry) {
            entry.transition = new KeyedSubtree(
                key: entry.transition.key,
                child: this.widget.transitionBuilder(entry.widgetChild, entry.animation)
            );
        }

        void _rebuildOutgoingWidgetsIfNeeded() {
            if (this._outgoingWidgets == null) {
                this._outgoingWidgets = new List<Widget>(this._outgoingEntries.Count);
                foreach (_ChildEntry entry in this._outgoingEntries) {
                    this._outgoingWidgets.Add(entry.transition);
                }
            }
            
            D.assert(this._outgoingEntries.Count == this._outgoingWidgets.Count);
            D.assert(this._outgoingEntries.isEmpty() ||
                     this._outgoingEntries.Last().transition == this._outgoingWidgets.Last());
        }

        public override void dispose() {
            if (this._currentEntry != null) {
                this._currentEntry.controller.dispose();
            }

            foreach (_ChildEntry entry in this._outgoingEntries) {
                entry.controller.dispose();
            }

            base.dispose();
        }

        public override Widget build(BuildContext context) {
            this._rebuildOutgoingWidgetsIfNeeded();
            return this.widget.layoutBuilder(this._currentEntry?.transition, this._outgoingWidgets);
        }
    }
}