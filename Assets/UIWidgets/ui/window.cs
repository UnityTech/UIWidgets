using System;
using UIWidgets.async;

namespace UIWidgets.ui {
    public delegate void VoidCallback();

    public delegate void FrameCallback(TimeSpan duration);

    public delegate void PointerDataPacketCallback(PointerDataPacket packet);

    public abstract class Window {
        public double devicePixelRatio {
            get { return this._devicePixelRatio; }
        }

        protected double _devicePixelRatio = 1.0;

        public Size physicalSize {
            get { return this._physicalSize; }
        }

        protected Size _physicalSize = Size.zero;

        public VoidCallback onMetricsChanged {
            get { return this._onMetricsChanged; }
            set { this._onMetricsChanged = value; }
        }

        VoidCallback _onMetricsChanged;

        public FrameCallback onBeginFrame {
            get { return this._onBeginFrame; }
            set { this._onBeginFrame = value; }
        }

        FrameCallback _onBeginFrame;

        public VoidCallback onDrawFrame {
            get { return this._onDrawFrame; }
            set { this._onDrawFrame = value; }
        }

        VoidCallback _onDrawFrame;

        public PointerDataPacketCallback onPointerEvent {
            get { return this._onPointerEvent; }
            set { this._onPointerEvent = value; }
        }

        PointerDataPacketCallback _onPointerEvent;

        public abstract void scheduleFrame();

        public abstract void render(Scene scene);

        public abstract void scheduleMicrotask(Action callback);

        public abstract void flushMicrotasks();

        public abstract Timer run(TimeSpan duration, Action callback);

        public Timer run(Action callback) {
            return this.run(TimeSpan.Zero, callback);
        }
    }
}