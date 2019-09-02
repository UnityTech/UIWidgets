using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.ui {
    public enum PointerChange {
        cancel,
        add,
        remove,
        hover,
        down,
        move,
        up,
        scroll,
        dragFromEditorMove,
        dragFromEditorRelease
    }

    public enum PointerDeviceKind {
        touch,
        mouse,
    }

    public enum PointerSignalKind {
        none,
        scroll,
        unknown,
    }

    public class PointerData {
        public PointerData(
            TimeSpan timeStamp,
            PointerChange change,
            PointerDeviceKind kind,
            PointerSignalKind signalKind = PointerSignalKind.none,
            int device = 0,
            float physicalX = 0.0f,
            float physicalY = 0.0f,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 0.0f,
            float pressureMax = 0.0f,
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
            float scrollDeltaX = 0.0f,
            float scrollDeltaY = 0.0f) {
            this.timeStamp = timeStamp;
            this.change = change;
            this.kind = kind;
            this.signalKind = signalKind;
            this.device = device;
            this.physicalX = physicalX;
            this.physicalY = physicalY;
            this.buttons = buttons;
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
            this.scrollDeltaX = scrollDeltaX;
            this.scrollDeltaY = scrollDeltaY;
        }

        public readonly TimeSpan timeStamp;
        public readonly PointerChange change;
        public readonly PointerDeviceKind kind;
        public readonly PointerSignalKind signalKind;
        public readonly int device;
        public readonly float physicalX;
        public readonly float physicalY;
        public readonly int buttons;
        public readonly bool obscured;
        public readonly float pressure;
        public readonly float pressureMin;
        public readonly float pressureMax;
        public readonly float distance;
        public readonly float distanceMax;
        public readonly float size;
        public readonly float radiusMajor;
        public readonly float radiusMinor;
        public readonly float radiusMin;
        public readonly float radiusMax;
        public readonly float orientation;
        public readonly float tilt;
        public readonly int platformData;
        public readonly float scrollDeltaX;
        public readonly float scrollDeltaY;
    }

    public class ScrollData : PointerData {
        public ScrollData(
            TimeSpan timeStamp,
            PointerChange change,
            PointerDeviceKind kind,
            PointerSignalKind signalKind = PointerSignalKind.none,
            int device = 0,
            float physicalX = 0.0f,
            float physicalY = 0.0f,
            float scrollX = 0.0f,
            float scrollY = 0.0f) : base(timeStamp, change, kind, signalKind, device, physicalX, physicalY) {
            this.scrollX = scrollX;
            this.scrollY = scrollY;
        }

        public float scrollX;
        public float scrollY;
    }

    public class PointerDataPacket {
        public PointerDataPacket(List<PointerData> data) {
            this.data = data;
        }

        public readonly List<PointerData> data;
    }
}