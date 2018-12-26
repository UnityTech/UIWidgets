using System;
using System.Collections.Generic;

namespace UIWidgets.ui {
    public enum PointerChange {
        cancel,
        add,
        remove,
        hover,
        down,
        move,
        up,
    }

    public enum PointerDeviceKind {
        touch,
        mouse,
    }

    public class PointerData {
        public PointerData(
            DateTime timeStamp,
            PointerChange change,
            PointerDeviceKind kind,
            int device,
            double physicalX,
            double physicalY) {
            this.timeStamp = timeStamp;
            this.change = change;
            this.kind = kind;
            this.device = device;
            this.physicalX = physicalX;
            this.physicalY = physicalY;
        }

        public readonly DateTime timeStamp;
        public PointerChange change;
        public PointerDeviceKind kind;
        public int device;
        public double physicalX;
        public double physicalY;
    }

    public class PointerDataPacket {
        public PointerDataPacket(List<PointerData> data) {
            this.data = data;
        }

        public readonly List<PointerData> data;
    }
}