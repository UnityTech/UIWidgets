using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    
    public delegate void PointerHoverEventListener(PointerHoverEvent evt);

    public delegate void PointerEnterEventListener(PointerEnterEvent evt);

    public delegate void PointerExitEventListener(PointerExitEvent evt);
    
    
    public class MouseTrackerAnnotation {
        public MouseTrackerAnnotation(
            PointerEnterEventListener onEnter = null,
            PointerHoverEventListener onHover = null,
            PointerExitEventListener onExit = null
        ) {
            this.onEnter = onEnter;
            this.onHover = onHover;
            this.onExit = onExit;
        }

        public readonly PointerEnterEventListener onEnter;

        public readonly PointerHoverEventListener onHover;

        public readonly PointerExitEventListener onExit;

        public override string ToString() {
            return $"{this.GetType()}#{this.GetHashCode()}{(this.onEnter == null ? "" : " onEnter")}{(this.onHover == null ? "" : " onHover")}{(this.onExit == null ? "" : " onExit")}";
        }
    }

    public class _TrackedAnnotation {
        public _TrackedAnnotation(
            MouseTrackerAnnotation annotation) {
            this.annotation = annotation;
        }
        
        public readonly MouseTrackerAnnotation annotation;

        public HashSet<int> activeDevices = new HashSet<int>();
    }

    public delegate MouseTrackerAnnotation MouseDetectorAnnotationFinder(Offset offset);


    public class MouseTracker {
        public MouseTracker(
            PointerRouter router,
            MouseDetectorAnnotationFinder annotationFinder) {
            router.addGlobalRoute(this._handleEvent);
            this.annotationFinder = annotationFinder;
        }
        
        readonly Dictionary<int, PointerEvent> _lastMouseEvent = new Dictionary<int, PointerEvent>();

        public bool mouseIsConnected {
            get { return this._lastMouseEvent.isNotEmpty(); }
        }

        public readonly MouseDetectorAnnotationFinder annotationFinder;

        public readonly Dictionary<MouseTrackerAnnotation, _TrackedAnnotation> _trackedAnnotations =
            new Dictionary<MouseTrackerAnnotation, _TrackedAnnotation>();


        public void attachAnnotation(MouseTrackerAnnotation annotation) {
            this._trackedAnnotations[annotation] = new _TrackedAnnotation(annotation);
            this._scheduleMousePositionCheck();
        }

        public void detachAnnotation(MouseTrackerAnnotation annotation) {
            _TrackedAnnotation trackedAnnotation = this._findAnnotation(annotation);
            foreach (int deviceId in trackedAnnotation.activeDevices) {
                annotation.onExit(PointerExitEvent.fromHoverEvent((PointerHoverEvent) this._lastMouseEvent[deviceId]));
            }

            this._trackedAnnotations.Remove(annotation);
        }

        void _scheduleMousePositionCheck() {
            SchedulerBinding.instance.addPostFrameCallback(_ => { this.collectMousePositions();});
            SchedulerBinding.instance.scheduleFrame();
        }
        
        void _handleEvent(PointerEvent evt) {
            if (evt.kind != PointerDeviceKind.mouse) {
                return;
            }

            int deviceId = evt.device;
            if (this._trackedAnnotations.isEmpty()) {
                this._lastMouseEvent.Remove(deviceId);
                return;
            }

            if (evt is PointerRemovedEvent) {
                this._lastMouseEvent.Remove(deviceId);
                this._scheduleMousePositionCheck();
            }
            else {
                if (evt is PointerMoveEvent ||
                    evt is PointerHoverEvent ||
                    evt is PointerDownEvent) {
                    if (!this._lastMouseEvent.ContainsKey(deviceId) ||
                        this._lastMouseEvent[deviceId].position != evt.position) {
                        this._scheduleMousePositionCheck();
                    }

                    this._lastMouseEvent[deviceId] = evt;
                }
            }
        }

        _TrackedAnnotation _findAnnotation(MouseTrackerAnnotation annotation) {
            if (!this._trackedAnnotations.TryGetValue(annotation, out var trackedAnnotation)) {
                D.assert(false, () => "Unable to find annotation $annotation in tracked annotations. " +
                                "Check that attachAnnotation has been called for all annotated layers.");
            }
            
            return trackedAnnotation;
        }
        
        bool isAnnotationAttached(MouseTrackerAnnotation annotation) {
            return this._trackedAnnotations.ContainsKey(annotation);
        }

        public void collectMousePositions() {
            void exitAnnotation(_TrackedAnnotation trackedAnnotation, int deviceId) {
                if (trackedAnnotation.annotation?.onExit != null &&
                    trackedAnnotation.activeDevices.Contains(deviceId)) {
                    trackedAnnotation.annotation.onExit(PointerExitEvent.fromHoverEvent(this._lastMouseEvent[deviceId]));
                    trackedAnnotation.activeDevices.Remove(deviceId);
                }
            }

            void exitAllDevices(_TrackedAnnotation trackedAnnotation) {
                if (trackedAnnotation.activeDevices.isNotEmpty()) {
                    HashSet<int> deviceIds = new HashSet<int>(trackedAnnotation.activeDevices);
                    foreach (int deviceId in deviceIds) {
                        exitAnnotation(trackedAnnotation, deviceId);
                    }
                }
            }

            if (!this.mouseIsConnected) {
                foreach (var annotation in this._trackedAnnotations.Values) {
                    exitAllDevices(annotation);
                }

                return;
            }

            foreach (int deviceId in this._lastMouseEvent.Keys) {
                PointerEvent lastEvent = this._lastMouseEvent[deviceId];
                MouseTrackerAnnotation hit = this.annotationFinder(lastEvent.position);

                if (hit == null) {
                    foreach (_TrackedAnnotation trackedAnnotation in this._trackedAnnotations.Values) {
                        exitAnnotation(trackedAnnotation, deviceId);
                    }

                    return;
                }

                _TrackedAnnotation hitAnnotation = this._findAnnotation(hit);
                //enter
                if (!hitAnnotation.activeDevices.Contains(deviceId)) {
                    hitAnnotation.activeDevices.Add(deviceId);
                    if (hitAnnotation.annotation?.onEnter != null) {
                        hitAnnotation.annotation.onEnter(PointerEnterEvent.fromHoverEvent(lastEvent));
                    }
                }

                //hover
                if (hitAnnotation.annotation?.onHover != null) {
                    hitAnnotation.annotation.onHover(PointerHoverEvent.fromHoverEvent(lastEvent));
                }
                
                //leave
                foreach (_TrackedAnnotation trackedAnnotation in this._trackedAnnotations.Values) {
                    if (hitAnnotation == trackedAnnotation) {
                        continue;
                    }

                    if (trackedAnnotation.activeDevices.Contains(deviceId)) {
                        if (trackedAnnotation.annotation?.onExit != null) {
                            trackedAnnotation.annotation.onExit(PointerExitEvent.fromHoverEvent((PointerHoverEvent)lastEvent));
                        }

                        trackedAnnotation.activeDevices.Remove(deviceId);
                    }
                }
            }
        }
    }
}