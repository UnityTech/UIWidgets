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
        scroll
    }

    public enum PointerDeviceKind {
        touch,
        mouse,
    }

    public class PointerData {
        public PointerData(
            TimeSpan timeStamp,
            PointerChange change,
            PointerDeviceKind kind,
            int device,
            float physicalX,
            float physicalY) {
            this.timeStamp = timeStamp;
            this.change = change;
            this.kind = kind;
            this.device = device;
            this.physicalX = physicalX;
            this.physicalY = physicalY;
        }

        public readonly TimeSpan timeStamp;
        public PointerChange change;
        public PointerDeviceKind kind;
        public int device;
        public float physicalX;
        public float physicalY;
    }

    public class ScrollData : PointerData {
        public ScrollData(
            TimeSpan timeStamp,
            PointerChange change,
            PointerDeviceKind kind,
            int device,
            float physicalX,
            float physicalY,
            float scrollX,
            float scrollY) : base(timeStamp, change, kind, device, physicalX, physicalY) {
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