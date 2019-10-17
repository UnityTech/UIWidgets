using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    static class TextSelectionUtils {
        public static TimeSpan _kDragSelectionUpdateThrottle = new TimeSpan(0, 0, 0, 0, 50);
    }

    public enum TextSelectionHandleType {
        left,
        right,
        collapsed,
    }

    enum _TextSelectionHandlePosition {
        start,
        end
    }

    public delegate void TextSelectionOverlayChanged(TextEditingValue value, Rect caretRect);

    public delegate void DragSelectionUpdateCallback(DragStartDetails startDetails, DragUpdateDetails updateDetails);

    public abstract class TextSelectionControls {
        public abstract Widget buildHandle(BuildContext context, TextSelectionHandleType type, float textLineHeight);

        public abstract Widget buildToolbar(BuildContext context, Rect globalEditableRegion, Offset position,
            TextSelectionDelegate selectionDelegate);

        public abstract Size handleSize { get; }

        public virtual bool canCut(TextSelectionDelegate selectionDelegate) {
            return !selectionDelegate.textEditingValue.selection.isCollapsed;
        }

        public virtual bool canCopy(TextSelectionDelegate selectionDelegate) {
            return !selectionDelegate.textEditingValue.selection.isCollapsed;
        }

        public virtual bool canPaste(TextSelectionDelegate selectionDelegate) {
            // TODO in flutter: return false when clipboard is empty
            return true;
        }

        public virtual bool canSelectAll(TextSelectionDelegate selectionDelegate) {
            return selectionDelegate.textEditingValue.text.isNotEmpty() &&
                   selectionDelegate.textEditingValue.selection.isCollapsed;
        }

        public void handleCut(TextSelectionDelegate selectionDelegate) {
            TextEditingValue value = selectionDelegate.textEditingValue;
            Clipboard.setData(new ClipboardData(
                text: value.selection.textInside(value.text)
            ));
            selectionDelegate.textEditingValue = new TextEditingValue(
                text: value.selection.textBefore(value.text)
                      + value.selection.textAfter(value.text),
                selection: TextSelection.collapsed(
                    offset: value.selection.start
                )
            );
            selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
            selectionDelegate.hideToolbar();
        }

        public void handleCopy(TextSelectionDelegate selectionDelegate) {
            TextEditingValue value = selectionDelegate.textEditingValue;
            Clipboard.setData(new ClipboardData(
                text: value.selection.textInside(value.text)
            ));
            selectionDelegate.textEditingValue = new TextEditingValue(
                text: value.text,
                selection: TextSelection.collapsed(offset: value.selection.end)
            );
            selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
            selectionDelegate.hideToolbar();
        }

        public void handlePaste(TextSelectionDelegate selectionDelegate) {
            TextEditingValue value = selectionDelegate.textEditingValue; // Snapshot the input before using `await`.
            Clipboard.getData(Clipboard.kTextPlain).Then((data) => {
                if (data != null) {
                    selectionDelegate.textEditingValue = new TextEditingValue(
                        text: value.selection.textBefore(value.text)
                              + data.text
                              + value.selection.textAfter(value.text),
                        selection: TextSelection.collapsed(
                            offset: value.selection.start + data.text.Length
                        )
                    );
                    
                    selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
                    selectionDelegate.hideToolbar();
                }
            });
        }

        public void handleSelectAll(TextSelectionDelegate selectionDelegate) {
            selectionDelegate.textEditingValue = new TextEditingValue(
                text: selectionDelegate.textEditingValue.text,
                selection: new TextSelection(
                    baseOffset: 0,
                    extentOffset: selectionDelegate.textEditingValue.text.Length
                )
            );
            selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
        }
    }

    public class TextSelectionOverlay {
        public TextSelectionOverlay(TextEditingValue value = null,
            BuildContext context = null, Widget debugRequiredFor = null,
            LayerLink layerLink = null,
            RenderEditable renderObject = null,
            TextSelectionControls selectionControls = null,
            TextSelectionDelegate selectionDelegate = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start) {
            D.assert(value != null);
            D.assert(context != null);
            this.context = context;
            this.debugRequiredFor = debugRequiredFor;
            this.layerLink = layerLink;
            this.renderObject = renderObject;
            this.selectionControls = selectionControls;
            this.selectionDelegate = selectionDelegate;
            this._value = value;
            OverlayState overlay = Overlay.of(context);
            D.assert(overlay != null, () => $"No Overlay widget exists above {context}.\n" +
                                            "Usually the Navigator created by WidgetsApp provides the overlay. Perhaps your " +
                                            "app content was created above the Navigator with the WidgetsApp builder parameter.");
            this._toolbarController = new AnimationController(duration: fadeDuration, vsync: overlay);
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly BuildContext context;
        public readonly Widget debugRequiredFor;
        public readonly LayerLink layerLink;
        public readonly RenderEditable renderObject;
        public readonly TextSelectionControls selectionControls;
        public readonly TextSelectionDelegate selectionDelegate;
        public readonly DragStartBehavior dragStartBehavior;

        public static readonly TimeSpan fadeDuration = TimeSpan.FromMilliseconds(150);
        AnimationController _toolbarController;

        Animation<float> _toolbarOpacity {
            get { return this._toolbarController.view; }
        }

        TextEditingValue _value;

        List<OverlayEntry> _handles;

        OverlayEntry _toolbar;

        TextSelection _selection {
            get { return this._value.selection; }
        }

        public void showHandles() {
            D.assert(this._handles == null);
            this._handles = new List<OverlayEntry> {
                new OverlayEntry(builder: (BuildContext context) =>
                    this._buildHandle(context, _TextSelectionHandlePosition.start)),
                new OverlayEntry(builder: (BuildContext context) =>
                    this._buildHandle(context, _TextSelectionHandlePosition.end)),
            };
            Overlay.of(this.context, debugRequiredFor: this.debugRequiredFor).insertAll(this._handles);
        }

        public void showToolbar() {
            D.assert(this._toolbar == null);
            this._toolbar = new OverlayEntry(builder: this._buildToolbar);
            Overlay.of(this.context, debugRequiredFor: this.debugRequiredFor).insert(this._toolbar);
            this._toolbarController.forward(from: 0.0f);
        }

        public void update(TextEditingValue newValue) {
            if (this._value == newValue) {
                return;
            }

            this._value = newValue;
            if (SchedulerBinding.instance.schedulerPhase == SchedulerPhase.persistentCallbacks) {
                SchedulerBinding.instance.addPostFrameCallback((duration) => this._markNeedsBuild());
            }
            else {
                this._markNeedsBuild();
            }
        }

        public void updateForScroll() {
            this._markNeedsBuild();
        }

        void _markNeedsBuild() {
            if (this._handles != null) {
                this._handles[0].markNeedsBuild();
                this._handles[1].markNeedsBuild();
            }

            this._toolbar?.markNeedsBuild();
        }

        public bool handlesAreVisible {
            get { return this._handles != null; }
        }


        public bool toolbarIsVisible {
            get { return this._toolbar != null; }
        }

        public void hide() {
            if (this._handles != null) {
                this._handles[0].remove();
                this._handles[1].remove();
                this._handles = null;
            }

            this._toolbar?.remove();
            this._toolbar = null;

            this._toolbarController.stop();
        }

        public void dispose() {
            this.hide();
            this._toolbarController.dispose();
        }

        Widget _buildHandle(BuildContext context, _TextSelectionHandlePosition position) {
            if ((this._selection.isCollapsed && position == _TextSelectionHandlePosition.end) ||
                this.selectionControls == null) {
                return new Container(); // hide the second handle when collapsed
            }

            return new _TextSelectionHandleOverlay(
                onSelectionHandleChanged: (TextSelection newSelection) => {
                    this._handleSelectionHandleChanged(newSelection, position);
                },
                onSelectionHandleTapped: this._handleSelectionHandleTapped,
                layerLink: this.layerLink,
                renderObject: this.renderObject,
                selection: this._selection,
                selectionControls: this.selectionControls,
                position: position,
                dragStartBehavior: this.dragStartBehavior
            );
        }

        Widget _buildToolbar(BuildContext context) {
            if (this.selectionControls == null) {
                return new Container();
            }

            // Find the horizontal midpoint, just above the selected text.
            List<TextSelectionPoint> endpoints = this.renderObject.getEndpointsForSelection(this._selection);
            Offset midpoint = new Offset(
                (endpoints.Count == 1) ? endpoints[0].point.dx : (endpoints[0].point.dx + endpoints[1].point.dx) / 2.0f,
                endpoints[0].point.dy - this.renderObject.preferredLineHeight
            );

            Rect editingRegion = Rect.fromPoints(this.renderObject.localToGlobal(Offset.zero),
                this.renderObject.localToGlobal(this.renderObject.size.bottomRight(Offset.zero))
            );

            return new FadeTransition(
                opacity: this._toolbarOpacity,
                child: new CompositedTransformFollower(
                    link: this.layerLink,
                    showWhenUnlinked: false,
                    offset: -editingRegion.topLeft,
                    child: this.selectionControls.buildToolbar(context, editingRegion, midpoint, this.selectionDelegate)
                )
            );
        }

        void _handleSelectionHandleChanged(TextSelection newSelection, _TextSelectionHandlePosition position) {
            TextPosition textPosition = null;
            switch (position) {
                case _TextSelectionHandlePosition.start:
                    textPosition = newSelection.basePos;
                    break;
                case _TextSelectionHandlePosition.end:
                    textPosition = newSelection.extendPos;
                    break;
            }

            this.selectionDelegate.textEditingValue =
                this._value.copyWith(selection: newSelection, composing: TextRange.empty);
            this.selectionDelegate.bringIntoView(textPosition);
        }

        void _handleSelectionHandleTapped() {
            if (this._value.selection.isCollapsed) {
                if (this._toolbar != null) {
                    this._toolbar?.remove();
                    this._toolbar = null;
                }
                else {
                    this.showToolbar();
                }
            }
        }
    }

    class _TextSelectionHandleOverlay : StatefulWidget {
        internal _TextSelectionHandleOverlay(
            Key key = null,
            TextSelection selection = null,
            _TextSelectionHandlePosition position = _TextSelectionHandlePosition.start,
            LayerLink layerLink = null,
            RenderEditable renderObject = null,
            ValueChanged<TextSelection> onSelectionHandleChanged = null,
            VoidCallback onSelectionHandleTapped = null,
            TextSelectionControls selectionControls = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            this.selection = selection;
            this.position = position;
            this.layerLink = layerLink;
            this.renderObject = renderObject;
            this.onSelectionHandleChanged = onSelectionHandleChanged;
            this.onSelectionHandleTapped = onSelectionHandleTapped;
            this.selectionControls = selectionControls;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly TextSelection selection;
        public readonly _TextSelectionHandlePosition position;
        public readonly LayerLink layerLink;
        public readonly RenderEditable renderObject;
        public readonly ValueChanged<TextSelection> onSelectionHandleChanged;
        public readonly VoidCallback onSelectionHandleTapped;
        public readonly TextSelectionControls selectionControls;
        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _TextSelectionHandleOverlayState();
        }

        internal ValueListenable<bool> _visibility {
            get {
                switch (this.position) {
                    case _TextSelectionHandlePosition.start:
                        return this.renderObject.selectionStartInViewport;
                    case _TextSelectionHandlePosition.end:
                        return this.renderObject.selectionEndInViewport;
                }

                return null;
            }
        }
    }

    class _TextSelectionHandleOverlayState : SingleTickerProviderStateMixin<_TextSelectionHandleOverlay> {
        Offset _dragPosition;

        AnimationController _controller;

        Animation<float> _opacity {
            get { return this._controller.view; }
        }

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(duration: TextSelectionOverlay.fadeDuration, vsync: this);
            this._handleVisibilityChanged();
            this.widget._visibility.addListener(this._handleVisibilityChanged);
        }

        void _handleVisibilityChanged() {
            if (this.widget._visibility.value) {
                this._controller.forward();
            }
            else {
                this._controller.reverse();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            (oldWidget as _TextSelectionHandleOverlay)._visibility.removeListener(this._handleVisibilityChanged);
            this._handleVisibilityChanged();
            this.widget._visibility.addListener(this._handleVisibilityChanged);
        }

        public override void dispose() {
            this.widget._visibility.removeListener(this._handleVisibilityChanged);
            this._controller.dispose();
            base.dispose();
        }

        void _handleDragStart(DragStartDetails details) {
            this._dragPosition = details.globalPosition +
                                 new Offset(0.0f, -this.widget.selectionControls.handleSize.height);
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            this._dragPosition += details.delta;
            TextPosition position = this.widget.renderObject.getPositionForPoint(this._dragPosition);

            if (this.widget.selection.isCollapsed) {
                this.widget.onSelectionHandleChanged(TextSelection.fromPosition(position));
                return;
            }

            TextSelection newSelection = null;
            switch (this.widget.position) {
                case _TextSelectionHandlePosition.start:
                    newSelection = new TextSelection(
                        baseOffset: position.offset,
                        extentOffset: this.widget.selection.extentOffset
                    );
                    break;
                case _TextSelectionHandlePosition.end:
                    newSelection = new TextSelection(
                        baseOffset: this.widget.selection.baseOffset,
                        extentOffset: position.offset
                    );
                    break;
            }

            if (newSelection.baseOffset >= newSelection.extentOffset) {
                return; // don't allow order swapping.
            }

            this.widget.onSelectionHandleChanged(newSelection);
        }

        void _handleTap() {
            this.widget.onSelectionHandleTapped();
        }

        public override Widget build(BuildContext context) {
            List<TextSelectionPoint> endpoints =
                this.widget.renderObject.getEndpointsForSelection(this.widget.selection);
            Offset point = null;
            TextSelectionHandleType type = TextSelectionHandleType.left;

            switch (this.widget.position) {
                case _TextSelectionHandlePosition.start:
                    point = endpoints[0].point;
                    type = this._chooseType(endpoints[0], TextSelectionHandleType.left, TextSelectionHandleType.right);
                    break;
                case _TextSelectionHandlePosition.end:
                    D.assert(endpoints.Count == 2);
                    point = endpoints[1].point;
                    type = this._chooseType(endpoints[1], TextSelectionHandleType.right, TextSelectionHandleType.left);
                    break;
            }

            Size viewport = this.widget.renderObject.size;
            point = new Offset(
                point.dx.clamp(0.0f, viewport.width),
                point.dy.clamp(0.0f, viewport.height)
            );

            return new CompositedTransformFollower(
                link: this.widget.layerLink,
                showWhenUnlinked: false,
                child: new FadeTransition(
                    opacity: this._opacity,
                    child: new GestureDetector(
                        dragStartBehavior: this.widget.dragStartBehavior,
                        onPanStart: this._handleDragStart,
                        onPanUpdate: this._handleDragUpdate,
                        onTap: this._handleTap,
                        child: new Stack(
                            overflow: Overflow.visible,
                            children: new List<Widget>() {
                                new Positioned(
                                    left: point.dx,
                                    top: point.dy,
                                    child: this.widget.selectionControls.buildHandle(context, type,
                                        this.widget.renderObject.preferredLineHeight)
                                )
                            }
                        )
                    )
                )
            );
        }

        TextSelectionHandleType _chooseType(
            TextSelectionPoint endpoint,
            TextSelectionHandleType ltrType,
            TextSelectionHandleType rtlType
        ) {
            if (this.widget.selection.isCollapsed) {
                return TextSelectionHandleType.collapsed;
            }

            D.assert(endpoint.direction != null);
            switch (endpoint.direction) {
                case TextDirection.ltr:
                    return ltrType;
                case TextDirection.rtl:
                    return rtlType;
            }

            D.assert(() => throw new UIWidgetsError($"invalid endpoint.direction {endpoint.direction}"));
            return ltrType;
        }
    }


    public class TextSelectionGestureDetector : StatefulWidget {
        public TextSelectionGestureDetector(
            Key key = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onSingleTapUp = null,
            GestureTapCancelCallback onSingleTapCancel = null,
            GestureLongPressStartCallback onSingleLongTapStart = null,
            GestureLongPressMoveUpdateCallback onSingleLongTapMoveUpdate = null,
            GestureLongPressEndCallback onSingleLongTapEnd = null,
            GestureTapDownCallback onDoubleTapDown = null,
            GestureDragStartCallback onDragSelectionStart = null,
            DragSelectionUpdateCallback onDragSelectionUpdate = null,
            GestureDragEndCallback onDragSelectionEnd = null,
            HitTestBehavior? behavior = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.onTapDown = onTapDown;
            this.onSingleTapUp = onSingleTapUp;
            this.onSingleTapCancel = onSingleTapCancel;
            this.onSingleLongTapStart = onSingleLongTapStart;
            this.onDoubleTapDown = onDoubleTapDown;
            this.onDragSelectionStart = onDragSelectionStart;
            this.onDragSelectionUpdate = onDragSelectionUpdate;
            this.onDragSelectionEnd = onDragSelectionEnd;
            this.behavior = behavior;
            this.child = child;
        }

        public readonly GestureTapDownCallback onTapDown;

        public readonly GestureTapUpCallback onSingleTapUp;

        public readonly GestureTapCancelCallback onSingleTapCancel;

        public readonly GestureLongPressStartCallback onSingleLongTapStart;

        public readonly GestureLongPressMoveUpdateCallback onSingleLongTapMoveUpdate;

        public readonly GestureLongPressEndCallback onSingleLongTapEnd;

        public readonly GestureTapDownCallback onDoubleTapDown;

        public readonly GestureDragStartCallback onDragSelectionStart;

        public readonly DragSelectionUpdateCallback onDragSelectionUpdate;

        public readonly GestureDragEndCallback onDragSelectionEnd;

        public HitTestBehavior? behavior;

        public readonly Widget child;

        public override State createState() {
            return new _TextSelectionGestureDetectorState();
        }
    }

    class _TextSelectionGestureDetectorState : State<TextSelectionGestureDetector> {
        Timer _doubleTapTimer;
        Offset _lastTapOffset;

        bool _isDoubleTap = false;

        public override void dispose() {
            this._doubleTapTimer?.cancel();
            this._dragUpdateThrottleTimer?.cancel();
            base.dispose();
        }

        void _handleTapDown(TapDownDetails details) {
            if (this.widget.onTapDown != null) {
                this.widget.onTapDown(details);
            }

            if (this._doubleTapTimer != null &&
                this._isWithinDoubleTapTolerance(details.globalPosition)) {
                if (this.widget.onDoubleTapDown != null) {
                    this.widget.onDoubleTapDown(details);
                }

                this._doubleTapTimer.cancel();
                this._doubleTapTimeout();
                this._isDoubleTap = true;
            }
        }

        void _handleTapUp(TapUpDetails details) {
            if (!this._isDoubleTap) {
                if (this.widget.onSingleTapUp != null) {
                    this.widget.onSingleTapUp(details);
                }

                this._lastTapOffset = details.globalPosition;
                this._doubleTapTimer = Window.instance.run(Constants.kDoubleTapTimeout, this._doubleTapTimeout);
            }

            this._isDoubleTap = false;
        }

        void _handleTapCancel() {
            if (this.widget.onSingleTapCancel != null) {
                this.widget.onSingleTapCancel();
            }
        }

        DragStartDetails _lastDragStartDetails;
        DragUpdateDetails _lastDragUpdateDetails;
        Timer _dragUpdateThrottleTimer;

        void _handleDragStart(DragStartDetails details) {
            D.assert(this._lastDragStartDetails == null);
            this._lastDragStartDetails = details;
            if (this.widget.onDragSelectionStart != null) {
                this.widget.onDragSelectionStart(details);
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            this._lastDragUpdateDetails = details;
            this._dragUpdateThrottleTimer = this._dragUpdateThrottleTimer ??
                                            Window.instance.run(TextSelectionUtils._kDragSelectionUpdateThrottle,
                                                this._handleDragUpdateThrottled);
        }

        void _handleDragUpdateThrottled() {
            D.assert(this._lastDragStartDetails != null);
            D.assert(this._lastDragUpdateDetails != null);
            if (this.widget.onDragSelectionUpdate != null) {
                this.widget.onDragSelectionUpdate(this._lastDragStartDetails, this._lastDragUpdateDetails);
            }

            this._dragUpdateThrottleTimer = null;
            this._lastDragUpdateDetails = null;
        }

        void _handleDragEnd(DragEndDetails details) {
            D.assert(this._lastDragStartDetails != null);
            if (this._lastDragUpdateDetails != null) {
                this._dragUpdateThrottleTimer.cancel();
                this._handleDragUpdateThrottled();
            }

            if (this.widget.onDragSelectionEnd != null) {
                this.widget.onDragSelectionEnd(details);
            }

            this._dragUpdateThrottleTimer = null;
            this._lastDragStartDetails = null;
            this._lastDragUpdateDetails = null;
        }

        void _handleLongPressStart(LongPressStartDetails details) {
            if (!this._isDoubleTap && this.widget.onSingleLongTapStart != null) {
                this.widget.onSingleLongTapStart(details);
            }
        }

        void _handleLongPressMoveUpdate(LongPressMoveUpdateDetails details) {
            if (!this._isDoubleTap && this.widget.onSingleLongTapMoveUpdate != null) {
                this.widget.onSingleLongTapMoveUpdate(details);
            }
        }

        void _handleLongPressEnd(LongPressEndDetails details) {
            if (!this._isDoubleTap && this.widget.onSingleLongTapEnd != null) {
                this.widget.onSingleLongTapEnd(details);
            }

            this._isDoubleTap = false;
        }

        void _doubleTapTimeout() {
            this._doubleTapTimer = null;
            this._lastTapOffset = null;
        }

        bool _isWithinDoubleTapTolerance(Offset secondTapOffset) {
            D.assert(secondTapOffset != null);
            if (this._lastTapOffset == null) {
                return false;
            }

            Offset difference = secondTapOffset - this._lastTapOffset;
            return difference.distance <= Constants.kDoubleTapSlop;
        }

        public override Widget build(BuildContext context) {
            Dictionary<Type, GestureRecognizerFactory> gestures = new Dictionary<Type, GestureRecognizerFactory>();

            gestures.Add(typeof(TapGestureRecognizer), new GestureRecognizerFactoryWithHandlers<TapGestureRecognizer>(
                    () => new TapGestureRecognizer(debugOwner: this),
                    instance => {
                        instance.onTapDown = this._handleTapDown;
                        instance.onTapUp = this._handleTapUp;
                        instance.onTapCancel = this._handleTapCancel;
                    }
                )
            );

            if (this.widget.onSingleLongTapStart != null ||
                this.widget.onSingleLongTapMoveUpdate != null ||
                this.widget.onSingleLongTapEnd != null
            ) {
                gestures[typeof(LongPressGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<LongPressGestureRecognizer>(
                        () => new LongPressGestureRecognizer(debugOwner: this, kind: PointerDeviceKind.touch),
                        instance => {
                            instance.onLongPressStart = this._handleLongPressStart;
                            instance.onLongPressMoveUpdate = this._handleLongPressMoveUpdate;
                            instance.onLongPressEnd = this._handleLongPressEnd;
                        });
            }

            if (this.widget.onDragSelectionStart != null ||
                this.widget.onDragSelectionUpdate != null ||
                this.widget.onDragSelectionEnd != null) {
                gestures.Add(typeof(HorizontalDragGestureRecognizer),
                    new GestureRecognizerFactoryWithHandlers<HorizontalDragGestureRecognizer>(
                        () => new HorizontalDragGestureRecognizer(debugOwner: this, kind: PointerDeviceKind.mouse),
                        instance => {
                            instance.dragStartBehavior = DragStartBehavior.down;
                            instance.onStart = this._handleDragStart;
                            instance.onUpdate = this._handleDragUpdate;
                            instance.onEnd = this._handleDragEnd;
                        }
                    )
                );
            }

            // TODO: if (this.widget.onForcePressStart != null || this.widget.onForcePressEnd != null) {
            // }

            return new RawGestureDetector(
                gestures: gestures,
                behavior: this.widget.behavior,
                child: this.widget.child
            );
        }
    }
}