using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.gestures {
    class _PointerState {
        public _PointerState(Offset lastPosition) {
            this.lastPosition = lastPosition ?? Offset.zero;
        }

        public int pointer {
            get { return this._pointer; }
        }

        int _pointer;

        static int _pointerCount = 0;

        public void startNewPointer() {
            _PointerState._pointerCount += 1;
            this._pointer = _PointerState._pointerCount;
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
            return string.Format("_PointerState(pointer: {0}, down: {1}, lastPosition: {2})",
                this.pointer, this.down, this.lastPosition);
        }
    }

    public static class PointerEventConverter {
        static readonly Dictionary<int, _PointerState> _pointers = new Dictionary<int, _PointerState>();

        static _PointerState _ensureStateForPointer(PointerData datum, Offset position) {
            return _pointers.putIfAbsent(
                datum.device,
                () => new _PointerState(position));
        }

        public static IEnumerable<PointerEvent> expand(IEnumerable<PointerData> data, double devicePixelRatio) {
            foreach (PointerData datum in data) {
                var position = new Offset(datum.physicalX, datum.physicalY) / devicePixelRatio;
                var timeStamp = datum.timeStamp;
                var kind = datum.kind;

                switch (datum.change) {
                    case PointerChange.down: {
                        _PointerState state = _ensureStateForPointer(datum, position);
                        D.assert(!state.down);
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
                    case PointerChange.up:
                    case PointerChange.cancel: {
                        D.assert(_pointers.ContainsKey(datum.device));
                        _PointerState state = _pointers[datum.device];
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
                        } else {
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