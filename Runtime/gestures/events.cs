using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Object = UnityEngine.Object;

namespace Unity.UIWidgets.gestures {
    public abstract class PointerEvent : Diagnosticable {
        public PointerEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            int buttons = 0,
            bool down = false,
            bool obscured = false,
            float pressure = 1.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            int platformData = 0,
            bool synthesized = false
        ) {
            this.timeStamp = timeStamp;
            this.pointer = pointer;
            this.kind = kind;
            this.device = device;
            this.position = position ?? Offset.zero;
            this.delta = delta ?? Offset.zero;
            this.buttons = buttons;
            this.down = down;
            this.obscured = obscured;
            this.pressure = pressure;
            this.pressureMin = pressureMin;
            this.pressureMax = pressureMax;
            this.distance = distance;
            this.distanceMax = distanceMax;
            this.size = size;
            this.radiusMajor = radiusMajor;
            this.radiusMinor = radiusMinor;
            this.radiusMin = radiusMin;
            this.radiusMax = radiusMax;
            this.orientation = orientation;
            this.tilt = tilt;
            this.platformData = platformData;
            this.synthesized = synthesized;
        }

        public readonly TimeSpan timeStamp;

        public readonly int pointer;

        public PointerDeviceKind kind;

        public int device;

        public readonly Offset position;

        public readonly Offset delta;

        public readonly int buttons;

        public readonly bool down;

        public readonly bool obscured;

        public readonly float pressure;

        public readonly float pressureMin;

        public readonly float pressureMax;

        public readonly float distance;

        public float distanceMin {
            get { return 0.0f; }
        }

        public readonly float distanceMax;

        public readonly float size;

        public readonly float radiusMajor;

        public readonly float radiusMinor;

        public readonly float radiusMin;

        public readonly float radiusMax;

        public readonly float orientation;

        public readonly float tilt;

        public readonly int platformData;

        public readonly bool synthesized;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("position", this.position));
            properties.add(new DiagnosticsProperty<Offset>("delta", this.delta, defaultValue: Offset.zero,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<TimeSpan>("timeStamp", this.timeStamp, defaultValue: TimeSpan.Zero,
                level: DiagnosticLevel.debug));
            properties.add(new IntProperty("pointer", this.pointer, level: DiagnosticLevel.debug));
            properties.add(new EnumProperty<PointerDeviceKind>("kind", this.kind, level: DiagnosticLevel.debug));
            properties.add(new IntProperty("device", this.device, defaultValue: 0, level: DiagnosticLevel.debug));
            properties.add(new IntProperty("buttons", this.buttons, defaultValue: 0, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<bool>("down", this.down, level: DiagnosticLevel.debug));
            properties.add(
                new FloatProperty("pressure", this.pressure, defaultValue: 1.0, level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("pressureMin", this.pressureMin, defaultValue: 1.0,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("pressureMax", this.pressureMax, defaultValue: 1.0,
                level: DiagnosticLevel.debug));
            properties.add(
                new FloatProperty("distance", this.distance, defaultValue: 0.0, level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("distanceMin", this.distanceMin, defaultValue: 0.0,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("distanceMax", this.distanceMax, defaultValue: 0.0,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("size", this.size, defaultValue: 0.0, level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("radiusMajor", this.radiusMajor, defaultValue: 0.0,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("radiusMinor", this.radiusMinor, defaultValue: 0.0,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("radiusMin", this.radiusMin, defaultValue: 0.0,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("radiusMax", this.radiusMax, defaultValue: 0.0,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("orientation", this.orientation, defaultValue: 0.0,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("tilt", this.tilt, defaultValue: 0.0, level: DiagnosticLevel.debug));
            properties.add(new IntProperty("platformData", this.platformData, defaultValue: 0,
                level: DiagnosticLevel.debug));
            properties.add(new FlagProperty("obscured", value: this.obscured, ifTrue: "obscured",
                level: DiagnosticLevel.debug));
            properties.add(new FlagProperty("synthesized", value: this.synthesized, ifTrue: "synthesized",
                level: DiagnosticLevel.debug));
        }
    }

    public class PointerAddedEvent : PointerEvent {
        public PointerAddedEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f
        ) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt
        ) {
        }
    }

    public class PointerRemovedEvent : PointerEvent {
        public PointerRemovedEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distanceMax = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f
        ) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax
        ) {
        }
    }

    public class PointerHoverEvent : PointerEvent {
        public PointerHoverEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            bool synthesized = false) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position,
            delta: delta,
            buttons: buttons,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            synthesized: synthesized) {
        }
    }

    public class PointerEnterEvent : PointerEvent {
        public PointerEnterEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            bool synthesized = false,
            bool down = false) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position,
            delta: delta,
            buttons: buttons,
            down: down,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            synthesized: synthesized) {
        }

        public static PointerEnterEvent fromHoverEvent(PointerHoverEvent e) {
            return fromMouseEvent(e);
        }

        public static PointerEnterEvent fromMouseEvent(PointerEvent hover) {
            return new PointerEnterEvent(
                timeStamp: hover?.timeStamp ?? TimeSpan.Zero,
                kind: hover?.kind ?? PointerDeviceKind.touch,
                device: hover?.device ?? 0,
                position: hover?.position,
                delta: hover?.delta,
                buttons: hover?.buttons ?? 0,
                down: hover?.down ?? false,
                obscured: hover?.obscured ?? false,
                pressure: hover?.pressure ?? 0.0f,
                pressureMin: hover?.pressureMin ?? 1.0f,
                pressureMax: hover?.pressureMax ?? 1.0f,
                distance: hover?.distance ?? 0.0f,
                distanceMax: hover?.distanceMax ?? 0.0f,
                size: hover?.size ?? 0.0f,
                radiusMajor: hover?.radiusMajor ?? 0.0f,
                radiusMinor: hover?.radiusMinor ?? 0.0f,
                radiusMin: hover?.radiusMin ?? 0.0f,
                radiusMax: hover?.radiusMax ?? 0.0f,
                orientation: hover?.orientation ?? 0.0f,
                tilt: hover?.tilt ?? 0.0f,
                synthesized: hover?.synthesized ?? false
            );
        }
    }

    public class PointerExitEvent : PointerEvent {
        public PointerExitEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            bool synthesized = false,
            bool down = false) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position,
            delta: delta,
            buttons: buttons,
            down: down,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            synthesized: synthesized) {
        }

        public static PointerExitEvent fromHoverEvent(PointerHoverEvent e) {
            return fromMouseEvent(e);
        }

        public static PointerExitEvent fromMouseEvent(PointerEvent hover) {
            return new PointerExitEvent(
                timeStamp: hover?.timeStamp ?? TimeSpan.Zero,
                kind: hover?.kind ?? PointerDeviceKind.touch,
                device: hover?.device ?? 0,
                position: hover?.position,
                delta: hover?.delta,
                buttons: hover?.buttons ?? 0,
                down: hover?.down ?? false,
                obscured: hover?.obscured ?? false,
                pressure: hover?.pressure ?? 0.0f,
                pressureMin: hover?.pressureMin ?? 1.0f,
                pressureMax: hover?.pressureMax ?? 1.0f,
                distance: hover?.distance ?? 0.0f,
                distanceMax: hover?.distanceMax ?? 0.0f,
                size: hover?.size ?? 0.0f,
                radiusMajor: hover?.radiusMajor ?? 0.0f,
                radiusMinor: hover?.radiusMinor ?? 0.0f,
                radiusMin: hover?.radiusMin ?? 0.0f,
                radiusMax: hover?.radiusMax ?? 0.0f,
                orientation: hover?.orientation ?? 0.0f,
                tilt: hover?.tilt ?? 0.0f,
                synthesized: hover?.synthesized ?? false
            );
        }
    }

    public class PointerDownEvent : PointerEvent {
        public PointerDownEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f
        ) : base(
            timeStamp: timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            buttons: buttons,
            down: true,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt) {
        }
    }

    public class PointerMoveEvent : PointerEvent {
        public PointerMoveEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset delta = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            int platformdData = 0,
            bool synthesized = false
        ) : base(
            timeStamp: timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            delta: delta,
            buttons: buttons,
            down: true,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            platformData: platformdData,
            synthesized: synthesized) {
        }
    }

    public class PointerUpEvent : PointerEvent {
        public PointerUpEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f
        ) : base(
            timeStamp: timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            buttons: buttons,
            down: false,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt) {
        }
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
        ) {
        }
    }

    public class PointerScrollEvent : PointerSignalEvent {
        public PointerScrollEvent(
            TimeSpan timeStamp,
            int pointer,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset scrollDelta = null)
            : base(
                timeStamp,
                kind: kind,
                pointer: pointer,
                device: device,
                position: position) {
            D.assert(position != null);
            D.assert(scrollDelta != null);
            this.scrollDelta = scrollDelta;
        }

        public readonly Offset scrollDelta;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("scrollDelta", this.scrollDelta));
        }
    }

    public class PointerCancelEvent : PointerEvent {
        public PointerCancelEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f
        ) : base(
            timeStamp: timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            buttons: buttons,
            down: false,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt) {
        }
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
        ) {
        }

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
        ) {
        }

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
        ) {
        }

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
}