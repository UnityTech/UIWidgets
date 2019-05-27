using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate bool DragTargetWillAccept<T>(T data);

    public delegate void DragTargetAccept<T>(T data);

    public delegate Widget DragTargetBuilder<T>(BuildContext context, List<T> candidateData, List<T> rejectedData);

    public delegate void DraggableCanceledCallback(Velocity velocity, Offset offset);

    public delegate void DragEndCallback(DraggableDetails details);

    public delegate void DragTargetLeave<T>(T data);

    public enum DragAnchor {
        child,
        pointer
    }
    
    static class _DragUtils {
        public static List<T> _mapAvatarsToData<T>(List<_DragAvatar<T>> avatars) {
            List<T> ret = new List<T>(avatars.Count);
            foreach (var avatar in avatars) {
                ret.Add(avatar.data);
            }

            return ret;
        }
    }

    public class Draggable<T> : StatefulWidget {
        public Draggable(
            T data,
            Key key = null,
            Widget child = null,
            Widget feedback = null,
            Axis? axis = null,
            Widget childWhenDragging = null,
            Offset feedbackOffset = null,
            DragAnchor dragAnchor = DragAnchor.child,
            Axis? affinity = null,
            int? maxSimultaneousDrags = null,
            VoidCallback onDragStarted = null,
            DraggableCanceledCallback onDraggableCanceled = null,
            DragEndCallback onDragEnd = null,
            VoidCallback onDragCompleted = null) : base(key) {
            D.assert(child != null);
            D.assert(feedback != null);
            D.assert(maxSimultaneousDrags == null || maxSimultaneousDrags >= 0);

            this.child = child;
            this.feedback = feedback;
            this.data = data;
            this.axis = axis;
            this.childWhenDragging = childWhenDragging;
            if (feedbackOffset == null) {
                feedbackOffset = Offset.zero;
            }

            this.feedbackOffset = feedbackOffset;
            this.dragAnchor = dragAnchor;
            this.affinity = affinity;
            this.maxSimultaneousDrags = maxSimultaneousDrags;
            this.onDragStarted = onDragStarted;
            this.onDraggableCanceled = onDraggableCanceled;
            this.onDragEnd = onDragEnd;
            this.onDragCompleted = onDragCompleted;
        }

        public readonly T data;

        public readonly Axis? axis;

        public readonly Widget child;

        public readonly Widget childWhenDragging;

        public readonly Widget feedback;

        public readonly Offset feedbackOffset;

        public readonly DragAnchor dragAnchor;

        readonly Axis? affinity;

        public readonly int? maxSimultaneousDrags;

        public readonly VoidCallback onDragStarted;

        public readonly DraggableCanceledCallback onDraggableCanceled;

        public readonly VoidCallback onDragCompleted;

        public readonly DragEndCallback onDragEnd;


        public virtual GestureRecognizer createRecognizer(GestureMultiDragStartCallback onStart) {
            switch (this.affinity) {
                case Axis.horizontal: {
                    return new HorizontalMultiDragGestureRecognizer(this) {onStart = onStart};
                }
                case Axis.vertical: {
                    return new VerticalMultiDragGestureRecognizer(this) {onStart = onStart};
                }
            }

            return new ImmediateMultiDragGestureRecognizer(this) {onStart = onStart};
        }

        public override State createState() {
            return new _DraggableState<T>();
        }
    }


    public class LongPressDraggable<T> : Draggable<T> {
        public LongPressDraggable(
            T data,
            Key key = null,
            Widget child = null,
            Widget feedback = null,
            Axis? axis = null,
            Widget childWhenDragging = null,
            Offset feedbackOffset = null,
            DragAnchor dragAnchor = DragAnchor.child,
            int? maxSimultaneousDrags = null,
            VoidCallback onDragStarted = null,
            DraggableCanceledCallback onDraggableCanceled = null,
            DragEndCallback onDragEnd = null,
            VoidCallback onDragCompleted = null
        ) : base(
            key: key,
            child: child,
            feedback: feedback,
            data: data,
            axis: axis,
            childWhenDragging: childWhenDragging,
            feedbackOffset: feedbackOffset,
            dragAnchor: dragAnchor,
            maxSimultaneousDrags: maxSimultaneousDrags,
            onDragStarted: onDragStarted,
            onDraggableCanceled: onDraggableCanceled,
            onDragEnd: onDragEnd,
            onDragCompleted: onDragCompleted
        ) {
        }

        public override GestureRecognizer createRecognizer(GestureMultiDragStartCallback onStart) {
            return new DelayedMultiDragGestureRecognizer(Constants.kLongPressTimeout) {
                onStart = (Offset position) => {
                    Drag result = onStart(position);
                    return result;
                }
            };
        }
    }

    public class _DraggableState<T> : State<Draggable<T>> {
        public override void initState() {
            base.initState();
            this._recognizer = this.widget.createRecognizer(this._startDrag);
        }

        public override void dispose() {
            this._disposeRecognizerIfInactive();
            base.dispose();
        }

        GestureRecognizer _recognizer;
        int _activeCount;

        void _disposeRecognizerIfInactive() {
            if (this._activeCount > 0) {
                return;
            }

            this._recognizer.dispose();
            this._recognizer = null;
        }


        void _routePointer(PointerEvent pEvent) {
            if (this.widget.maxSimultaneousDrags != null &&
                this._activeCount >= this.widget.maxSimultaneousDrags) {
                return;
            }

            if (pEvent is PointerDownEvent) {
                this._recognizer.addPointer((PointerDownEvent) pEvent);
            }
        }

        _DragAvatar<T> _startDrag(Offset position) {
            if (this.widget.maxSimultaneousDrags != null &&
                this._activeCount >= this.widget.maxSimultaneousDrags) {
                return null;
            }

            var dragStartPoint = Offset.zero;
            switch (this.widget.dragAnchor) {
                case DragAnchor.child:
                    RenderBox renderObject = this.context.findRenderObject() as RenderBox;
                    dragStartPoint = renderObject.globalToLocal(position);
                    break;
                case DragAnchor.pointer:
                    dragStartPoint = Offset.zero;
                    break;
            }

            this.setState(() => { this._activeCount += 1; });

            _DragAvatar<T> avatar = new _DragAvatar<T>(
                overlayState: Overlay.of(this.context, debugRequiredFor: this.widget),
                data: this.widget.data,
                axis: this.widget.axis,
                initialPosition: position,
                dragStartPoint: dragStartPoint,
                feedback: this.widget.feedback,
                feedbackOffset: this.widget.feedbackOffset,
                onDragEnd: (Velocity velocity, Offset offset, bool wasAccepted) => {
                    if (this.mounted) {
                        this.setState(() => { this._activeCount -= 1; });
                    }
                    else {
                        this._activeCount -= 1;
                        this._disposeRecognizerIfInactive();
                    }

                    if (this.mounted && this.widget.onDragEnd != null) {
                        this.widget.onDragEnd(new DraggableDetails(
                            wasAccepted: wasAccepted,
                            velocity: velocity,
                            offset: offset
                        ));
                    }

                    if (wasAccepted && this.widget.onDragCompleted != null) {
                        this.widget.onDragCompleted();
                    }

                    if (!wasAccepted && this.widget.onDraggableCanceled != null) {
                        this.widget.onDraggableCanceled(velocity, offset);
                    }
                }
            );
            if (this.widget.onDragStarted != null) {
                this.widget.onDragStarted();
            }

            return avatar;
        }

        public override Widget build(BuildContext context) {
            D.assert(Overlay.of(context, debugRequiredFor: this.widget) != null);
            bool canDrag = this.widget.maxSimultaneousDrags == null ||
                           this._activeCount < this.widget.maxSimultaneousDrags;

            bool showChild = this._activeCount == 0 || this.widget.childWhenDragging == null;
            if (canDrag) {
                return new Listener(
                    onPointerDown: this._routePointer,
                    child: showChild ? this.widget.child : this.widget.childWhenDragging
                );
            }

            return new Listener(
                child: showChild ? this.widget.child : this.widget.childWhenDragging);
        }
    }


    public class DraggableDetails {
        public DraggableDetails(
            bool wasAccepted = false,
            Velocity velocity = null,
            Offset offset = null
        ) {
            D.assert(velocity != null);
            D.assert(offset != null);
            this.wasAccepted = wasAccepted;
            this.velocity = velocity;
            this.offset = offset;
        }

        public readonly bool wasAccepted;

        public readonly Velocity velocity;

        public readonly Offset offset;
    }


    public class DragTarget<T> : StatefulWidget {
        public DragTarget(
            Key key = null,
            DragTargetBuilder<T> builder = null,
            DragTargetWillAccept<T> onWillAccept = null,
            DragTargetAccept<T> onAccept = null,
            DragTargetLeave<T> onLeave = null
        ) : base(key) {
            D.assert(builder != null);
            this.builder = builder;
            this.onWillAccept = onWillAccept;
            this.onAccept = onAccept;
            this.onLeave = onLeave;
        }

        public readonly DragTargetBuilder<T> builder;

        public readonly DragTargetWillAccept<T> onWillAccept;

        public readonly DragTargetAccept<T> onAccept;

        public readonly DragTargetLeave<T> onLeave;

        public override State createState() {
            return new _DragTargetState<T>();
        }
    }

    public class _DragTargetState<T> : State<DragTarget<T>> {
        readonly List<_DragAvatar<T>> _candidateAvatars = new List<_DragAvatar<T>>();
        readonly List<_DragAvatar<T>> _rejectedAvatars = new List<_DragAvatar<T>>();

        public bool didEnter(_DragAvatar<T> avatar) {
            D.assert(!this._candidateAvatars.Contains(avatar));
            D.assert(!this._rejectedAvatars.Contains(avatar));

            if (avatar.data is T && (this.widget.onWillAccept == null || this.widget.onWillAccept(avatar.data))) {
                this.setState(() => { this._candidateAvatars.Add(avatar); });
                return true;
            }

            this._rejectedAvatars.Add(avatar);
            return false;
        }

        public void didLeave(_DragAvatar<T> avatar) {
            D.assert(this._candidateAvatars.Contains(avatar) || this._rejectedAvatars.Contains(avatar));
            if (!this.mounted) {
                return;
            }

            this.setState(() => {
                this._candidateAvatars.Remove(avatar);
                this._rejectedAvatars.Remove(avatar);
            });
            if (this.widget.onLeave != null) {
                this.widget.onLeave(avatar.data);
            }
        }

        public void didDrop(_DragAvatar<T> avatar) {
            D.assert(this._candidateAvatars.Contains(avatar));
            if (!this.mounted) {
                return;
            }

            this.setState(() => { this._candidateAvatars.Remove(avatar); });
            if (this.widget.onAccept != null) {
                this.widget.onAccept(avatar.data);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(this.widget.builder != null);
            return new MetaData(
                metaData: this,
                behavior: HitTestBehavior.translucent,
                child: this.widget.builder(context, _DragUtils._mapAvatarsToData(this._candidateAvatars),
                    _DragUtils._mapAvatarsToData(this._rejectedAvatars)));
        }
    }


    public enum _DragEndKind {
        dropped,
        canceled
    }

    public delegate void _OnDragEnd(Velocity velocity, Offset offset, bool wasAccepted);


    public class _DragAvatar<T> : Drag {
        public _DragAvatar(
            T data,
            OverlayState overlayState,
            Axis? axis = null,
            Offset initialPosition = null,
            Offset dragStartPoint = null,
            Widget feedback = null,
            Offset feedbackOffset = null,
            _OnDragEnd onDragEnd = null
        ) {
            if (initialPosition == null) {
                initialPosition = Offset.zero;
            }

            if (dragStartPoint == null) {
                dragStartPoint = Offset.zero;
            }

            if (feedbackOffset == null) {
                feedbackOffset = Offset.zero;
            }

            D.assert(overlayState != null);
            this.overlayState = overlayState;
            this.data = data;
            this.axis = axis;
            this.dragStartPoint = dragStartPoint;
            this.feedback = feedback;
            this.feedbackOffset = feedbackOffset;
            this.onDragEnd = onDragEnd;

            this._entry = new OverlayEntry(this._build);

            this.overlayState.insert(this._entry);
            this._position = initialPosition;
            this.updateDrag(initialPosition);
        }

        public readonly T data;

        readonly Axis? axis;

        readonly Offset dragStartPoint;

        readonly Widget feedback;

        readonly Offset feedbackOffset;

        readonly _OnDragEnd onDragEnd;

        readonly OverlayState overlayState;

        _DragTargetState<T> _activeTarget;

        readonly List<_DragTargetState<T>> _enteredTargets = new List<_DragTargetState<T>>();

        Offset _position;

        Offset _lastOffset;

        OverlayEntry _entry;

        public void update(DragUpdateDetails details) {
            this._position += this._restrictAxis(details.delta);
            this.updateDrag(this._position);
        }

        public void end(DragEndDetails details) {
            this.finishDrag(_DragEndKind.dropped, this._restrictVelocityAxis(details.velocity));
        }

        public void cancel() {
            this.finishDrag(_DragEndKind.canceled);
        }

        void updateDrag(Offset globalPosition) {
            this._lastOffset = globalPosition - this.dragStartPoint;
            this._entry.markNeedsBuild();

            HitTestResult result = new HitTestResult();
            WidgetsBinding.instance.hitTest(result, globalPosition + this.feedbackOffset);

            List<_DragTargetState<T>> targets = this._getDragTargets(result.path);

            bool listsMatch = false;
            if (targets.Count >= this._enteredTargets.Count && this._enteredTargets.isNotEmpty()) {
                listsMatch = true;
                List<_DragTargetState<T>>.Enumerator iterator = targets.GetEnumerator();
                for (int i = 0; i < this._enteredTargets.Count; i++) {
                    iterator.MoveNext();
                    if (iterator.Current != this._enteredTargets[i]) {
                        listsMatch = false;
                        break;
                    }
                }
            }

            if (listsMatch) {
                return;
            }

            this._leaveAllEntered();

            _DragTargetState<T> newTarget = null;
            foreach (var target in targets) {
                this._enteredTargets.Add(target);
                if (target.didEnter(this)) {
                    newTarget = target;
                    break;
                }
            }

            this._activeTarget = newTarget;
        }

        List<_DragTargetState<T>> _getDragTargets(IList<HitTestEntry> path) {
            List<_DragTargetState<T>> ret = new List<_DragTargetState<T>>();

            foreach (HitTestEntry entry in path) {
                if (entry.target is RenderMetaData) {
                    RenderMetaData renderMetaData = (RenderMetaData) entry.target;
                    if (renderMetaData.metaData is _DragTargetState<T>) {
                        ret.Add((_DragTargetState<T>) renderMetaData.metaData);
                    }
                }
            }

            return ret;
        }

        void _leaveAllEntered() {
            for (int i = 0; i < this._enteredTargets.Count; i++) {
                this._enteredTargets[i].didLeave(this);
            }

            this._enteredTargets.Clear();
        }

        void finishDrag(_DragEndKind endKind, Velocity velocity = null) {
            bool wasAccepted = false;
            if (endKind == _DragEndKind.dropped && this._activeTarget != null) {
                this._activeTarget.didDrop(this);
                wasAccepted = true;
                this._enteredTargets.Remove(this._activeTarget);
            }

            this._leaveAllEntered();
            this._activeTarget = null;
            this._entry.remove();
            this._entry = null;

            if (this.onDragEnd != null) {
                this.onDragEnd(velocity == null ? Velocity.zero : velocity, this._lastOffset, wasAccepted);
            }
        }

        public Widget _build(BuildContext context) {
            RenderBox box = (RenderBox) this.overlayState.context.findRenderObject();
            Offset overlayTopLeft = box.localToGlobal(Offset.zero);
            return new Positioned(
                left: this._lastOffset.dx - overlayTopLeft.dx,
                top: this._lastOffset.dy - overlayTopLeft.dy,
                child: new IgnorePointer(
                    child: this.feedback
                )
            );
        }

        Velocity _restrictVelocityAxis(Velocity velocity) {
            if (this.axis == null) {
                return velocity;
            }

            return new Velocity(
                this._restrictAxis(velocity.pixelsPerSecond));
        }

        Offset _restrictAxis(Offset offset) {
            if (this.axis == null) {
                return offset;
            }

            if (this.axis == Axis.horizontal) {
                return new Offset(offset.dx, 0.0f);
            }

            return new Offset(0.0f, offset.dy);
        }
    }
}