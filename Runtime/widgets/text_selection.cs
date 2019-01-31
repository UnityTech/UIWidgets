using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
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

    public abstract class TextSelectionControls {
        public abstract Widget buildHandle(BuildContext context, TextSelectionHandleType type, double textLineHeight);

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
            return selectionDelegate.textEditingValue.text.isEmpty() &&
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
                }

                selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
                selectionDelegate.hideToolbar();
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
            TextSelectionDelegate selectionDelegate = null) {
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
            D.assert(overlay != null);
            this._handleController = new AnimationController(duration: _fadeDuration, vsync: overlay);
            this._toolbarController = new AnimationController(duration: _fadeDuration, vsync: overlay);
        }

        public readonly BuildContext context;
        public readonly Widget debugRequiredFor;
        public readonly LayerLink layerLink;
        public readonly RenderEditable renderObject;
        public readonly TextSelectionControls selectionControls;
        public readonly TextSelectionDelegate selectionDelegate;

        public static TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(150);
        AnimationController _handleController;
        AnimationController _toolbarController;

        Animation<double> _handleOpacity {
            get { return this._handleController.view; }
        }

        Animation<double> _toolbarOpacity {
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
            this._handleController.forward(from: 0.0);
        }

        public void showToolbar() {
            D.assert(this._toolbar == null);
            this._toolbar = new OverlayEntry(builder: this._buildToolbar);
            Overlay.of(this.context, debugRequiredFor: this.debugRequiredFor).insert(this._toolbar);
            this._toolbarController.forward(from: 0.0);
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

            this._handleController.stop();
            this._toolbarController.stop();
        }

        public void dispose() {
            this.hide();
            this._handleController.dispose();
            this._toolbarController.dispose();
        }

        Widget _buildHandle(BuildContext context, _TextSelectionHandlePosition position) {
            if ((this._selection.isCollapsed && position == _TextSelectionHandlePosition.end) ||
                this.selectionControls == null) {
                return new Container(); // hide the second handle when collapsed
            }

            return new FadeTransition(
                opacity: this._handleOpacity,
                child: new _TextSelectionHandleOverlay(
                    onSelectionHandleChanged: (TextSelection newSelection) => {
                        this._handleSelectionHandleChanged(newSelection, position);
                    },
                    onSelectionHandleTapped: this._handleSelectionHandleTapped,
                    layerLink: this.layerLink,
                    renderObject: this.renderObject,
                    selection: this._selection,
                    selectionControls: this.selectionControls,
                    position: position
                )
            );
        }

        Widget _buildToolbar(BuildContext context) {
            if (this.selectionControls == null) {
                return new Container();
            }

            // Find the horizontal midpoint, just above the selected text.
            List<TextSelectionPoint> endpoints = this.renderObject.getEndpointsForSelection(this._selection);
            Offset midpoint = new Offset(
                (endpoints.Count == 1) ? endpoints[0].point.dx : (endpoints[0].point.dx + endpoints[1].point.dx) / 2.0,
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
            TextSelectionControls selectionControls = null
        ) : base(key: key) {
            this.selection = selection;
            this.position = position;
            this.layerLink = layerLink;
            this.renderObject = renderObject;
            this.onSelectionHandleChanged = onSelectionHandleChanged;
            this.onSelectionHandleTapped = onSelectionHandleTapped;
            this.selectionControls = selectionControls;
        }

        public readonly TextSelection selection;
        public readonly _TextSelectionHandlePosition position;
        public readonly LayerLink layerLink;
        public readonly RenderEditable renderObject;
        public readonly ValueChanged<TextSelection> onSelectionHandleChanged;
        public readonly VoidCallback onSelectionHandleTapped;
        public readonly TextSelectionControls selectionControls;


        public override State createState() {
            return new _TextSelectionHandleOverlayState();
        }
    }

    class _TextSelectionHandleOverlayState : State<_TextSelectionHandleOverlay> {
        Offset _dragPosition;

        void _handleDragStart(DragStartDetails details) {
            this._dragPosition = details.globalPosition +
                                 new Offset(0.0, -this.widget.selectionControls.handleSize.height);
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

            return new CompositedTransformFollower(
                link: this.widget.layerLink,
                showWhenUnlinked: false,
                child: new GestureDetector(
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
}