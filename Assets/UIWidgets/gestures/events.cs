using System;
using UIWidgets.ui;

namespace UIWidgets.gestures {
    public abstract class PointerEvent {
        public PointerEvent(
            DateTime timeStamp,
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

        public readonly DateTime timeStamp;

        public readonly int pointer;

        public PointerDeviceKind kind;

        public int device;

        public readonly Offset position;

        public readonly Offset delta;

        public readonly bool down;

        public readonly bool synthesized;
    }

    public class PointerDownEvent : PointerEvent {
        public PointerDownEvent(
            DateTime timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            bool down = false,
            bool synthesized = false)
            : base(
                timeStamp,
                pointer,
                kind,
                device,
                position,
                delta,
                down,
                synthesized) {
        }
    }

    public class PointerUpEvent : PointerEvent {
        public PointerUpEvent(
            DateTime timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            bool down = false,
            bool synthesized = false)
            : base(
                timeStamp,
                pointer,
                kind,
                device,
                position,
                delta,
                down,
                synthesized) {
        }
    }

    public class PointerCancelEvent : PointerEvent {
        public PointerCancelEvent(
            DateTime timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            bool down = false,
            bool synthesized = false)
            : base(
                timeStamp,
                pointer,
                kind,
                device,
                position,
                delta,
                down,
                synthesized) {
        }
    }

    public class PointerMoveEvent : PointerEvent {
        public PointerMoveEvent(
            DateTime timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            bool down = false,
            bool synthesized = false)
            : base(
                timeStamp,
                pointer,
                kind,
                device,
                position,
                delta,
                down,
                synthesized) {
        }
    }
}