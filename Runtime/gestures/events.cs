using System;
using Unity.UIWidgets.ui;
using Object = UnityEngine.Object;

namespace Unity.UIWidgets.gestures {
    public abstract class PointerEvent {
        public PointerEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            bool down = false,
            bool synthesized = false
        ) {
            this.timeStamp = timeStamp;
            this.pointer = pointer;
            this.kind = kind;
            this.device = device;
            this.position = position ?? Offset.zero;
            this.delta = delta ?? Offset.zero;
            this.down = down;
            this.synthesized = synthesized;
        }

        public readonly TimeSpan timeStamp;

        public readonly int pointer;

        public PointerDeviceKind kind;

        public int device;

        public readonly Offset position;

        public readonly Offset delta;

        public readonly bool down;

        public readonly bool synthesized;
    }

    public class PointerAddedEvent : PointerEvent {
        public PointerAddedEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null
        ) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position
        ) { }
    }

    public class PointerRemovedEvent : PointerEvent {
        public PointerRemovedEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0
        ) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device
        ) { }
    }

    public class PointerScrollEvent : PointerEvent {
        public PointerScrollEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset delta = null)
            : base(
                timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                down: true,
                delta: delta) { }
    }

    public class PointerDragFromEditorEnterEvent : PointerEvent {
        public PointerDragFromEditorEnterEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position
        ) { }

        public static PointerDragFromEditorEnterEvent fromDragFromEditorEvent(PointerEvent evt) {
            return new PointerDragFromEditorEnterEvent(
                timeStamp: evt.timeStamp,
                pointer: evt.pointer,
                kind: evt.kind,
                device: evt.device,
                position: evt.position
            );
        }
    }

    public class PointerDragFromEditorExitEvent : PointerEvent {
        public PointerDragFromEditorExitEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position
        ) { }

        public static PointerDragFromEditorExitEvent fromDragFromEditorEvent(PointerEvent evt) {
            return new PointerDragFromEditorExitEvent(
                timeStamp: evt.timeStamp,
                pointer: evt.pointer,
                kind: evt.kind,
                device: evt.device,
                position: evt.position
            );
        }
    }

    public class PointerDragFromEditorHoverEvent : PointerEvent {
        public PointerDragFromEditorHoverEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position
        ) { }

        public static PointerDragFromEditorHoverEvent fromDragFromEditorEvent(PointerEvent evt) {
            return new PointerDragFromEditorHoverEvent(
                timeStamp: evt.timeStamp,
                pointer: evt.pointer,
                kind: evt.kind,
                device: evt.device,
                position: evt.position
            );
        }
    }

    public class PointerDragFromEditorReleaseEvent : PointerEvent {
        public PointerDragFromEditorReleaseEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Object[] objectReferences = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position
        ) {
            this.objectReferences = objectReferences;
        }

        public Object[] objectReferences;

        public static PointerDragFromEditorReleaseEvent fromDragFromEditorEvent(PointerEvent evt,
            Object[] objectReferences) {
            return new PointerDragFromEditorReleaseEvent(
                timeStamp: evt.timeStamp,
                pointer: evt.pointer,
                kind: evt.kind,
                device: evt.device,
                position: evt.position,
                objectReferences: objectReferences
            );
        }
    }

    public class PointerHoverEvent : PointerEvent {
        public PointerHoverEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null)
            : base(
                timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                down: false) { }

        public static PointerHoverEvent fromHoverEvent(PointerEvent hover) {
            return new PointerHoverEvent(
                timeStamp: hover.timeStamp,
                pointer: hover.pointer,
                kind: hover.kind,
                device: hover.device,
                position: hover.position
            );
        }
    }

    public class PointerEnterEvent : PointerEvent {
        public PointerEnterEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null)
            : base(
                timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                down: false) { }

        public static PointerEnterEvent fromHoverEvent(PointerEvent hover) {
            return new PointerEnterEvent(
                timeStamp: hover.timeStamp,
                pointer: hover.pointer,
                kind: hover.kind,
                device: hover.device,
                position: hover.position
            );
        }
    }

    public class PointerExitEvent : PointerEvent {
        public PointerExitEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null)
            : base(
                timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                down: false) { }

        public static PointerExitEvent fromHoverEvent(PointerEvent hover) {
            return new PointerExitEvent(
                timeStamp: hover.timeStamp,
                pointer: hover.pointer,
                kind: hover.kind,
                device: hover.device,
                position: hover.position
            );
        }
    }

    public class PointerDownEvent : PointerEvent {
        public PointerDownEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null)
            : base(
                timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                down: true) { }
    }

    public class PointerMoveEvent : PointerEvent {
        public PointerMoveEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            bool synthesized = false)
            : base(
                timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                delta: delta,
                down: true,
                synthesized: synthesized) { }
    }

    public class PointerUpEvent : PointerEvent {
        public PointerUpEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null)
            : base(
                timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                down: false) { }
    }

    public class PointerCancelEvent : PointerEvent {
        public PointerCancelEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null)
            : base(
                timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                down: false) { }
    }

    public class PointerSignalEvent : PointerEvent {
        public PointerSignalEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position
        ) { }
    }
}