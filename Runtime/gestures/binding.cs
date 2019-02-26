using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public class GestureBinding : SchedulerBinding, HitTestable, HitTestDispatcher, HitTestTarget {
        public new static GestureBinding instance {
            get { return (GestureBinding) SchedulerBinding.instance; }
            set { SchedulerBinding.instance = value; }
        }

        public GestureBinding() {
            Window.instance.onPointerEvent += this._handlePointerDataPacket;

            this.gestureArena = new GestureArenaManager();
        }

        readonly Queue<PointerEvent> _pendingPointerEvents = new Queue<PointerEvent>();

        void _handlePointerDataPacket(PointerDataPacket packet) {
            foreach (var pointerEvent in PointerEventConverter.expand(packet.data, Window.instance.devicePixelRatio)) {
                this._pendingPointerEvents.Enqueue(pointerEvent);
            }

            this._flushPointerEventQueue();
        }

        public void cancelPointer(int pointer) {
            if (this._pendingPointerEvents.isEmpty()) {
                Window.instance.scheduleMicrotask(this._flushPointerEventQueue);
            }

            this._pendingPointerEvents.Enqueue(
                new PointerCancelEvent(timeStamp: Timer.timespanSinceStartup, pointer: pointer));
        }

        void _flushPointerEventQueue() {
            while (this._pendingPointerEvents.Count != 0) {
                this._handlePointerEvent(this._pendingPointerEvents.Dequeue());
            }
        }

        public readonly PointerRouter pointerRouter = new PointerRouter();

        public readonly GestureArenaManager gestureArena;

        public readonly Dictionary<int, HitTestResult> _hitTests = new Dictionary<int, HitTestResult>();

        public readonly HashSet<HitTestTarget> lastMoveTargets = new HashSet<HitTestTarget>();

        void _handlePointerEvent(PointerEvent evt) {
            if (evt is PointerScrollEvent) {
                this._handlePointerScrollEvent(evt);
                return;
            }

            if (evt is PointerHoverEvent) {
                this._handlePointerHoverEvent(evt);
            }

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
            }
            else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                result = this._hitTests.getOrDefault(evt.pointer);
                this._hitTests.Remove(evt.pointer);
            }
            else if (evt.down) {
                result = this._hitTests.getOrDefault(evt.pointer);
            }
            else {
                return;
            }

            if (result != null) {
                this.dispatchEvent(evt, result);
            }
        }

        void _handlePointerScrollEvent(PointerEvent evt) {
            this.pointerRouter.clearScrollRoute(evt.pointer);

            HitTestResult result = new HitTestResult();
            this.hitTest(result, evt.position);

            this.dispatchEvent(evt, result);
        }

        void _handlePointerHoverEvent(PointerEvent evt) {
            HitTestResult result = new HitTestResult();
            this.hitTest(result, evt.position);

            // enter event
            foreach (var hitTestEntry in result.path) {
                if (this.lastMoveTargets.Contains(hitTestEntry.target)) {
                    hitTestEntry.target.handleEvent(evt, hitTestEntry);
                    this.lastMoveTargets.Remove(hitTestEntry.target);
                }
                else {
                    hitTestEntry.target.handleEvent(new PointerEnterEvent(
                        timeStamp: evt.timeStamp,
                        pointer: evt.pointer,
                        device: evt.device,
                        kind: evt.kind
                    ), hitTestEntry);
                }
            }

            foreach (var lastMoveTarget in this.lastMoveTargets) {
                lastMoveTarget.handleEvent(new PointerLeaveEvent(
                    timeStamp: evt.timeStamp,
                    pointer: evt.pointer,
                    device: evt.device,
                    kind: evt.kind
                ), null);
            }

            this.lastMoveTargets.Clear();
            foreach (var hitTestEntry in result.path) {
                this.lastMoveTargets.Add(hitTestEntry.target);
            }

            this.dispatchEvent(evt, result);
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
                    D.logError("Error while dispatching a pointer event: ", ex);
                }
            }
        }

        public void handleEvent(PointerEvent evt, HitTestEntry entry) {
            this.pointerRouter.route(evt);
            if (evt is PointerDownEvent) {
                this.gestureArena.close(evt.pointer);
            }
            else if (evt is PointerUpEvent) {
                this.gestureArena.sweep(evt.pointer);
            }
        }
    }
}