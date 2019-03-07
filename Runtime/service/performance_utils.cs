using System.Diagnostics;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.service {
    public class PerformanceUtils {
        public static PerformanceUtils instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new PerformanceUtils();
                _instance._setup();

                return _instance;
            }
        }

        static PerformanceUtils _instance;

        const int FrameBufferSize = 200;

        float[] _frames;
        int _curFrameId;
        Stopwatch _stopwatch;

        float deltaTime = 0.0f;

        bool _enabled;


        void _setup() {
            this._frames = new float[FrameBufferSize];
            this._curFrameId = -1;
            this._enabled = false;
        }

        void _ensureStopWatch() {
            if (this._stopwatch == null) {
                this._stopwatch = new Stopwatch();
            }
        }

        public void updateDeltaTime(float unscaledDeltaTime) {
            this.deltaTime += (unscaledDeltaTime - this.deltaTime) * 0.1f;
        }

        public float getFPS() {
            return 1.0f / this.deltaTime;
        }

        public void startProfile() {
            if (!this._enabled) {
                return;
            }

            this._ensureStopWatch();
            if (this._stopwatch.IsRunning) {
                D.assert(false, "Try to start the stopwatch when it is already running");
                return;
            }

            this._stopwatch.Start();
        }

        public void endProfile() {
            if (!this._enabled || this._stopwatch == null) {
                return;
            }

            if (!this._stopwatch.IsRunning) {
                D.assert(false, "Try to record the stopwatch when it is already stopped");
            }

            this._stopwatch.Stop();
            float frameCost = this._stopwatch.ElapsedMilliseconds;
            this._stopwatch.Reset();
            if (frameCost == 0) {
                return;
            }

            this._curFrameId = (this._curFrameId + 1) % FrameBufferSize;
            this._frames[this._curFrameId] = frameCost;
        }

        public float[] getFrames() {
            if (!this._enabled) {
                this._enabled = true;
            }

            return this._frames;
        }

        public int getCurFrame() {
            return this._curFrameId;
        }
    }
}