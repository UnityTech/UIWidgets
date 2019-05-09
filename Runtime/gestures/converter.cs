using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    class _PointerState {
        public _PointerState(Offset lastPosition) {
            this.lastPosition = lastPosition ?? Offset.zero;
        }

        public int pointer {
            get { return this._pointer; }
        }

        int _pointer;

        // pointers 0 ~ 9 are preserved for special unique inputs
        static int _pointerCount = 10;

        // special pointer id:
        // mouse scroll
        const int scrollPointer = 5;

        public void initScrollPointer() {
            this._pointer = scrollPointer;
        }

        public void startNewPointer() {
            _pointerCount += 1;
            this._pointer = _pointerCount;
        }

        public bool down {
            get { return this._down; }
        }

        bool _down = false;

        public void setDown() {
            D.assert(!this._down);
            this._down = true;
        }

        public void setUp() {
            D.assert(this._down);
            this._down = false;
        }

        public Offset lastPosition;

        public override string ToString() {
            return $"_PointerState(pointer: {this.pointer}, down: {this.down}, lastPosition: {this.lastPosition})";
        }
    }

    public static class PointerEventConverter {
        static readonly Dictionary<int, _PointerState> _pointers = new Dictionary<int, _PointerState>();

        static void clearPointers() {
            _pointers.Clear();
        }

        static _PointerState _ensureStateForPointer(PointerData datum, Offset position) {
            return _pointers.putIfAbsent(
                datum.device,
                () => new _PointerState(position));
        }

        public static IEnumerable<PointerEvent> expand(IEnumerable<PointerData> data, float devicePixelRatio) {
            foreach (PointerData datum in data) {
                var position = new Offset(datum.physicalX, datum.physicalY) / devicePixelRatio;
                var timeStamp = datum.timeStamp;
                var kind = datum.kind;

                switch (datum.change) {
                    case PointerChange.down: {
                        _PointerState state = _ensureStateForPointer(datum, position);
                        if (state.down) {
                            break;
                        }
                        if (state.lastPosition != position) {
                            // a hover event to be here.
                            state.lastPosition = position;
                        }

                        state.startNewPointer();
                        state.setDown();
                        yield return new PointerDownEvent(
                            timeStamp: timeStamp,
                            pointer: state.pointer,
                            kind: kind,
                            device: datum.device,
                            position: position
                        );
                    }
                        break;
                    case PointerChange.move: {
                        bool alreadyAdded = _pointers.ContainsKey(datum.device);
                        if (!alreadyAdded) {
                            break;
                        }

                        D.assert(_pointers.ContainsKey(datum.device));

                        _PointerState state = _pointers[datum.device];
                        if (!state.down) {
                            break;
                        }

                        D.assert(state.down);

                        Offset offset = position - state.lastPosition;
                        state.lastPosition = position;
                        yield return new PointerMoveEvent(
                            timeStamp: timeStamp,
                            pointer: state.pointer,
                            kind: kind,
                            device: datum.device,
                            position: position,
                            delta: offset
                        );
                    }
                        break;

                    case PointerChange.hover: {
                        yield return new PointerHoverEvent(
                            timeStamp: timeStamp,
                            kind: kind,
                            device: datum.device,
                            position: position
                        );
                        break;
                    }

                    case PointerChange.scroll: {
                        var _scrollData = (ScrollData) datum;
                        _PointerState state = _ensureStateForPointer(datum, position);
                        state.initScrollPointer();

                        if (state.lastPosition != position) {
                            state.lastPosition = position;
                        }

                        yield return new PointerScrollEvent(
                            timeStamp: timeStamp,
                            pointer: state.pointer,
                            kind: kind,
                            device: _scrollData.device,
                            position: position,
                            delta: new Offset(_scrollData.scrollX, _scrollData.scrollY) / devicePixelRatio
                        );
                        break;
                    }

                    case PointerChange.up:
                    case PointerChange.cancel: {
                        _PointerState state = _pointers.getOrDefault(datum.device);
                        if (state == null || !state.down) {
                            break;
                        }

                        D.assert(state.down);
                        if (position != state.lastPosition) {
                            Offset offset = position - state.lastPosition;
                            state.lastPosition = position;
                            yield return new PointerMoveEvent(
                                timeStamp: timeStamp,
                                pointer: state.pointer,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                delta: offset,
                                synthesized: true
                            );
                        }

                        D.assert(position == state.lastPosition);
                        state.setUp();
                        if (datum.change == PointerChange.up) {
                            yield return new PointerUpEvent(
                                timeStamp: timeStamp,
                                pointer: state.pointer,
                                kind: kind,
                                device: datum.device,
                                position: position
                            );
                        }
                        else {
                            yield return new PointerCancelEvent(
                                timeStamp: timeStamp,
                                pointer: state.pointer,
                                kind: kind,
                                device: datum.device,
                                position: position
                            );
                        }
                    }
                        break;
                }
            }
        }
    }
}