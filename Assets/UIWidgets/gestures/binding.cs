using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.scheduler;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.gestures {
    public class GestureBinding : SchedulerBinding, HitTestable, HitTestDispatcher, HitTestTarget {
        public GestureBinding(Window window) : base(window) {
            this.window.onPointerEvent += this._handlePointerDataPacket;

            this.gestureArena = new GestureArenaManager(window);
        }

        readonly Queue<PointerEvent> _pendingPointerEvents = new Queue<PointerEvent>();
        
        void _handlePointerDataPacket(PointerDataPacket packet) {
            foreach (var pointerEvent in PointerEventConverter.expand(packet.data, this.window.devicePixelRatio)) {
                this._pendingPointerEvents.Enqueue(pointerEvent);
            }

            this._flushPointerEventQueue();
        }

        public void cancelPointer(int pointer) {
            if (this._pendingPointerEvents.isEmpty()) {
                this.window.scheduleMicrotask(this._flushPointerEventQueue);
            }

            this._pendingPointerEvents.Enqueue(
                new PointerCancelEvent(timeStamp: DateTime.Now, pointer: pointer));
        }

        void _flushPointerEventQueue() {
            while (this._pendingPointerEvents.Count != 0) {
                this._handlePointerEvent(this._pendingPointerEvents.Dequeue());
            }
        }

        public readonly PointerRouter pointerRouter = new PointerRouter();

        public readonly GestureArenaManager gestureArena;

        public readonly Dictionary<int, HitTestResult> _hitTests = new Dictionary<int, HitTestResult>();

        void _handlePointerEvent(PointerEvent evt) {
            HitTestResult result;
            if (evt is PointerDownEvent) {
                D.assert(!this._hitTests.ContainsKey(evt.pointer));
                result = new HitTestResult();
                this.hitTest(result, evt.position);
                
                this._hitTests[evt.pointer] = result;
                D.assert(() => {
                    if (D.debugPrintHitTestResults) {
                        Debug.LogFormat("{0}: {1}", evt, result);
                    }

                    return true;
                });
            } else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                result = this._hitTests[evt.pointer];
                this._hitTests.Remove(evt.pointer);
            } else if (evt.down) {
                result = this._hitTests[evt.pointer];
            } else {
                return;
            }

            if (result != null) {
                this.dispatchEvent(evt, result);
            }
        }

        public virtual void hitTest(HitTestResult result, Offset position) {
            result.add(new HitTestEntry(this));
        }

        public void dispatchEvent(PointerEvent evt, HitTestResult result) {
            foreach (HitTestEntry entry in result.path) {
                try {
                    entry.target.handleEvent(evt, entry);
                }
                catch (Exception ex) {
                    Debug.LogError("Error while dispatching a pointer event: " + ex);
                }
            }
        }

        public void handleEvent(PointerEvent evt, HitTestEntry entry) {
            this.pointerRouter.route(evt);
            if (evt is PointerDownEvent) {
                this.gestureArena.close(evt.pointer);
            } else if (evt is PointerUpEvent) {
                this.gestureArena.sweep(evt.pointer);
            }
        }
    }
}