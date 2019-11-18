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

        public readonly PointerSignalResolver pointerSignalResolver = new PointerSignalResolver();

        public readonly Dictionary<int, HitTestResult> _hitTests = new Dictionary<int, HitTestResult>();

        public readonly HashSet<HitTestTarget> lastMoveTargets = new HashSet<HitTestTarget>();

        readonly HashSet<HitTestEntry> _enteredTargets = new HashSet<HitTestEntry>();

        void _handlePointerEvent(PointerEvent evt) {
            if (evt is PointerScrollEvent) {
                this._handlePointerScrollEvent(evt);
                return;
            }

            HitTestResult hitTestResult = null;
            if (evt is PointerDownEvent || evt is PointerSignalEvent) {
                D.assert(!this._hitTests.ContainsKey(evt.pointer));
                hitTestResult = new HitTestResult();
                this.hitTest(hitTestResult, evt.position);
                if (evt is PointerDownEvent) {
                    this._hitTests[evt.pointer] = hitTestResult;
                }

                this._hitTests[evt.pointer] = hitTestResult;
                D.assert(() => {
                    if (D.debugPrintHitTestResults) {
                        Debug.LogFormat("{0}: {1}", evt, hitTestResult);
                    }

                    return true;
                });
            }
            else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                hitTestResult = this._hitTests.getOrDefault(evt.pointer);
                this._hitTests.Remove(evt.pointer);
            }
            else if (evt.down) {
                hitTestResult = this._hitTests.getOrDefault(evt.pointer);
            }

            D.assert(() => {
                if (D.debugPrintMouseHoverEvents && evt is PointerHoverEvent) {
                    Debug.LogFormat("{0}", evt);
                }

                return true;
            });

            if (hitTestResult != null ||
                evt is PointerHoverEvent ||
                evt is PointerAddedEvent ||
                evt is PointerRemovedEvent ||
                evt is PointerDragFromEditorHoverEvent ||
                evt is PointerDragFromEditorReleaseEvent
            ) {
                this.dispatchEvent(evt, hitTestResult);
            }
        }

        void _handlePointerScrollEvent(PointerEvent evt) {
            this.pointerRouter.clearScrollRoute(evt.pointer);
            if (!this.pointerRouter.acceptScroll()) {
                return;
            }

            HitTestResult result = new HitTestResult();
            this.hitTest(result, evt.position);

            this.dispatchEvent(evt, result);
        }

        public virtual void hitTest(HitTestResult result, Offset position) {
            result.add(new HitTestEntry(this));
        }

        public void dispatchEvent(PointerEvent evt, HitTestResult hitTestResult) {
            if (hitTestResult == null) {
                D.assert(evt is PointerHoverEvent ||
                         evt is PointerAddedEvent ||
                         evt is PointerRemovedEvent ||
                         evt is PointerDragFromEditorHoverEvent ||
                         evt is PointerDragFromEditorReleaseEvent
                );
                try {
                    this.pointerRouter.route(evt);
                }
                catch (Exception ex) {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                            exception: ex,
                            library: "gesture library",
                            context: "while dispatching a non-hit-tested pointer event",
                            informationCollector: information => {
                                information.AppendLine("Event: ");
                                information.AppendFormat(" {0}", evt);
                            }
                        )
                    );
                }

                return;
            }

            foreach (HitTestEntry entry in hitTestResult.path) {
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
            else if (evt is PointerSignalEvent) {
                this.pointerSignalResolver.resolve((PointerSignalEvent) evt);
            }
        }
    }
}