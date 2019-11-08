using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Color = Unity.UIWidgets.ui.Color;
using Constants = Unity.UIWidgets.gestures.Constants;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public class SelectableText : StatefulWidget {
        public SelectableText(string data,
            Key key = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null,
            FocusNode focusNode = null,
            Color selectionColor = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onTapUp = null,
            GestureTapCancelCallback onTapCancel = null) : base(key) {
            D.assert(data != null);
            this.textSpan = null;
            this.data = data;
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
            this.focusNode = focusNode ?? new FocusNode();
            this.selectionColor = selectionColor;
            this.onTapDown = onTapDown;
            this.onTapUp = onTapUp;
            this.onTapCancel = onTapCancel;
        }

        public SelectableText(TextSpan textSpan,
            Key key = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null,
            FocusNode focusNode = null,
            Color selectionColor = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onTapUp = null,
            GestureTapCancelCallback onTapCancel = null) : base(key) {
            D.assert(textSpan != null);
            this.textSpan = textSpan;
            this.data = null;
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
            this.focusNode = focusNode ?? new FocusNode();
            this.selectionColor = selectionColor;
            this.onTapDown = onTapDown;
            this.onTapUp = onTapUp;
            this.onTapCancel = onTapCancel;
        }

        public static SelectableText rich(TextSpan textSpan,
            Key key = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null,
            FocusNode focusNode = null,
            Color selectionColor = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onTapUp = null,
            GestureTapCancelCallback onTapCancel = null) {
            return new SelectableText(
                textSpan, key,
                style,
                textAlign,
                softWrap,
                overflow,
                textScaleFactor,
                maxLines,
                focusNode,
                selectionColor,
                onTapDown,
                onTapUp,
                onTapCancel);
        }

        public readonly string data;

        public readonly FocusNode focusNode;

        public readonly TextSpan textSpan;

        public readonly TextStyle style;

        public readonly TextAlign? textAlign;

        public readonly bool? softWrap;

        public readonly TextOverflow? overflow;

        public readonly float? textScaleFactor;

        public readonly int? maxLines;

        public readonly Color selectionColor;

        public readonly GestureTapDownCallback onTapDown;

        public readonly GestureTapUpCallback onTapUp;

        public readonly GestureTapCancelCallback onTapCancel;

        public override State createState() {
            return new _SelectableTextState();
        }
    }


    class _SelectableTextState : State<SelectableText>, WidgetsBindingObserver {
        readonly GlobalKey _richTextKey = GlobalKey.key();

        RenderParagraph _renderParagragh {
            get { return (RenderParagraph) this._richTextKey.currentContext.findRenderObject(); }
        }

        public override void initState() {
            base.initState();
            this.widget.focusNode.addListener(this._handleFocusChanged);
        }


        public override void didUpdateWidget(StatefulWidget old) {
            SelectableText oldWidget = (SelectableText) old;
            base.didUpdateWidget(oldWidget);

            if (oldWidget.focusNode != this.widget.focusNode) {
                oldWidget.focusNode.removeListener(this._handleFocusChanged);
                this.widget.focusNode.addListener(this._handleFocusChanged);
            }
        }

        public override void dispose() {
            this.widget.focusNode.removeListener(this._handleFocusChanged);
            base.dispose();
        }

        bool _hasFocus {
            get { return this.widget.focusNode.hasFocus; }
        }

        void _handleFocusChanged() {
            if (this._hasFocus) {
                WidgetsBinding.instance.addObserver(this);
                this._renderParagragh.hasFocus = true;
            }
            else {
                WidgetsBinding.instance.removeObserver(this);
                this._renderParagragh.hasFocus = false;
            }
        }


        public void didChangeMetrics() {
        }

        public void didChangeTextScaleFactor() {
        }
        
        public void didChangePlatformBrightness() {
        }

        public void didChangeLocales(List<Locale> locale) {
        }

        public IPromise<bool> didPopRoute() {
            return Promise<bool>.Resolved(false);
        }

        public IPromise<bool> didPushRoute(string route) {
            return Promise<bool>.Resolved(false);
        }

        void _handleTapDown(TapDownDetails details) {
            this.widget.onTapDown?.Invoke(details);
        }

        void _handleSingleTapUp(TapUpDetails details) {
            this.widget.onTapUp?.Invoke(details);
        }

        void _handleSingleTapCancel() {
            this.widget.onTapCancel?.Invoke();
        }

        void _handleLongPress() {
        }

        void _handleDragSelectionStart(DragStartDetails details) {
            this._renderParagragh.selectPositionAt(
                from: details.globalPosition,
                cause: SelectionChangedCause.drag);
        }

        void _handleDragSelectionUpdate(DragStartDetails startDetails,
            DragUpdateDetails updateDetails) {
            this._renderParagragh.selectPositionAt(
                from: startDetails.globalPosition,
                to: updateDetails.globalPosition,
                cause: SelectionChangedCause.drag);
        }

        public override Widget build(BuildContext context) {
            FocusScope.of(context).reparentIfNeeded(this.widget.focusNode);

            DefaultTextStyle defaultTextStyle = DefaultTextStyle.of(context);
            TextStyle effectiveTextStyle = this.widget.style;
            if (this.widget.style == null || this.widget.style.inherit) {
                effectiveTextStyle = defaultTextStyle.style.merge(this.widget.style);
            }

            Widget child = new RichText(
                key: this._richTextKey,
                textAlign: this.widget.textAlign ?? defaultTextStyle.textAlign ?? TextAlign.left,
                softWrap: this.widget.softWrap ?? defaultTextStyle.softWrap,
                overflow: this.widget.overflow ?? defaultTextStyle.overflow,
                textScaleFactor: this.widget.textScaleFactor ?? MediaQuery.textScaleFactorOf(context),
                maxLines: this.widget.maxLines ?? defaultTextStyle.maxLines,
                text: new TextSpan(
                    style: effectiveTextStyle,
                    text: this.widget.data,
                    children: this.widget.textSpan != null ? new List<TextSpan> {this.widget.textSpan} : null
                ),
                onSelectionChanged: () => {
                    if (this._hasFocus) {
                        return;
                    }

                    FocusScope.of(this.context).requestFocus(this.widget.focusNode);
                },
                selectionColor: this.widget.selectionColor ?? Colors.blue);

            return new IgnorePointer(
                ignoring: false,
                child: new RichTextSelectionGestureDetector(
                    onTapDown: this._handleTapDown,
                    onSingleTapUp: this._handleSingleTapUp,
                    onSingleTapCancel: this._handleSingleTapCancel,
                    onSingleLongTapStart: this._handleLongPress,
                    onDragSelectionStart: this._handleDragSelectionStart,
                    onDragSelectionUpdate: this._handleDragSelectionUpdate,
                    behavior: HitTestBehavior.translucent,
                    child: child
                )
            );
        }
    }

    public class RichTextSelectionGestureDetector : StatefulWidget {
        public RichTextSelectionGestureDetector(
            Key key = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onSingleTapUp = null,
            GestureTapCancelCallback onSingleTapCancel = null,
            GestureLongPressCallback onSingleLongTapStart = null,
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

        public readonly GestureLongPressCallback onSingleLongTapStart;

        public readonly GestureTapDownCallback onDoubleTapDown;

        public readonly GestureDragStartCallback onDragSelectionStart;

        public readonly DragSelectionUpdateCallback onDragSelectionUpdate;

        public readonly GestureDragEndCallback onDragSelectionEnd;

        public HitTestBehavior? behavior;

        public readonly Widget child;

        public override State createState() {
            return new _RichTextSelectionGestureDetectorState();
        }
    }

    class _RichTextSelectionGestureDetectorState : State<RichTextSelectionGestureDetector> {
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

        void _handleLongPressStart() {
            if (!this._isDoubleTap && this.widget.onSingleLongTapStart != null) {
                this.widget.onSingleLongTapStart();
            }
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

            if (this.widget.onSingleLongTapStart != null) {
                gestures[typeof(LongPressGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<LongPressGestureRecognizer>(
                        () => new LongPressGestureRecognizer(debugOwner: this, kind: PointerDeviceKind.touch),
                        instance => { instance.onLongPress = this._handleLongPressStart; });
            }

            if (this.widget.onDragSelectionStart != null ||
                this.widget.onDragSelectionUpdate != null ||
                this.widget.onDragSelectionEnd != null) {
                gestures.Add(typeof(PanGestureRecognizer),
                    new GestureRecognizerFactoryWithHandlers<PanGestureRecognizer>(
                        () => new PanGestureRecognizer(debugOwner: this, kind: PointerDeviceKind.mouse),
                        instance => {
                            instance.dragStartBehavior = DragStartBehavior.down;
                            instance.onStart = this._handleDragStart;
                            instance.onUpdate = this._handleDragUpdate;
                            instance.onEnd = this._handleDragEnd;
                        }
                    )
                );
            }

            return new RawGestureDetector(
                gestures: gestures,
                behavior: this.widget.behavior,
                child: this.widget.child
            );
        }
    }
}