using System;
using UIWidgets.rendering;
using UIWidgets.widgets;

namespace UIWidgets.ui {
    public delegate void VoidCallback();

    public delegate void FrameCallback(TimeSpan duration);

    public abstract class Window {
        public double devicePixelRatio {
            get { return this._devicePixelRatio; }
        }

        public double _devicePixelRatio = 1.0;

        public Size physicalSize {
            get { return this._physicalSize; }
        }

        public Size _physicalSize = Size.zero;

        public VoidCallback onMetricsChanged {
            get { return this._onMetricsChanged; }
            set { this._onMetricsChanged = value; }
        }

        public VoidCallback _onMetricsChanged;

        public FrameCallback onBeginFrame {
            get { return this._onBeginFrame; }
            set { this._onBeginFrame = value; }
        }

        public FrameCallback _onBeginFrame;

        public VoidCallback onDrawFrame {
            get { return this._onDrawFrame; }
            set { this._onDrawFrame = value; }
        }

        public VoidCallback _onDrawFrame;

        public abstract void scheduleFrame();

        public abstract void render(Scene scene);
    }

   
}