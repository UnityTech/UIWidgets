using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class ImageInfo : IEquatable<ImageInfo> {
        public ImageInfo(Image image, float scale = 1.0f) {
            D.assert(image != null);

            this.image = image;
            this.scale = scale;
        }

        public readonly Image image;
        public readonly float scale;

        public bool Equals(ImageInfo other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.image, other.image) && this.scale.Equals(other.scale);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((ImageInfo) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.image != null ? this.image.GetHashCode() : 0) * 397) ^ this.scale.GetHashCode();
            }
        }

        public static bool operator ==(ImageInfo left, ImageInfo right) {
            return Equals(left, right);
        }

        public static bool operator !=(ImageInfo left, ImageInfo right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.image} @ {this.scale}x";
        }
    }

    public delegate void ImageListener(ImageInfo image, bool synchronousCall);

    public delegate void ImageErrorListener(Exception exception);

    class _ImageListenerPair {
        public ImageListener listener;
        public ImageErrorListener errorListener;
    }

    public class ImageStream : Diagnosticable {
        public ImageStream() {
        }

        ImageStreamCompleter _completer;

        public ImageStreamCompleter completer {
            get { return this._completer; }
        }

        List<_ImageListenerPair> _listeners;

        public void setCompleter(ImageStreamCompleter value) {
            D.assert(this._completer == null);

            this._completer = value;
            if (this._listeners != null) {
                var initialListeners = this._listeners;
                this._listeners = null;
                foreach (_ImageListenerPair listenerPair in initialListeners) {
                    this._completer.addListener(
                        listenerPair.listener,
                        listenerPair.errorListener
                    );
                }
            }
        }

        public void addListener(ImageListener listener, ImageErrorListener onError = null) {
            if (this._completer != null) {
                this._completer.addListener(listener, onError);
                return;
            }

            if (this._listeners == null) {
                this._listeners = new List<_ImageListenerPair>();
            }

            this._listeners.Add(new _ImageListenerPair {listener = listener, errorListener = onError});
        }

        public void removeListener(ImageListener listener) {
            if (this._completer != null) {
                this._completer.removeListener(listener);
                return;
            }

            D.assert(this._listeners != null);
            for (int i = 0; i < this._listeners.Count; i++) {
                if (this._listeners[i].listener == listener) {
                    this._listeners.RemoveAt(i);
                    break;
                }
            }
        }

        public object key {
            get { return this._completer != null ? (object) this._completer : this; }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ObjectFlagProperty<ImageStreamCompleter>(
                "completer",
                this._completer,
                ifPresent: this._completer?.toStringShort(),
                ifNull: "unresolved"
            ));
            properties.add(new ObjectFlagProperty<List<_ImageListenerPair>>(
                "listeners",
                this._listeners,
                ifPresent: $"{this._listeners?.Count} listener{(this._listeners?.Count == 1 ? "" : "s")}",
                ifNull: "no listeners",
                level: this._completer != null ? DiagnosticLevel.hidden : DiagnosticLevel.info
            ));
            this._completer?.debugFillProperties(properties);
        }
    }

    public abstract class ImageStreamCompleter : Diagnosticable {
        internal readonly List<_ImageListenerPair> _listeners = new List<_ImageListenerPair>();
        public ImageInfo currentImage;
        public UIWidgetsErrorDetails currentError;

        protected bool hasListeners {
            get { return this._listeners.isNotEmpty(); }
        }

        public virtual void addListener(ImageListener listener, ImageErrorListener onError = null) {
            this._listeners.Add(new _ImageListenerPair {listener = listener, errorListener = onError});

            if (this.currentImage != null) {
                try {
                    listener(this.currentImage, true);
                }
                catch (Exception ex) {
                    this.reportError(
                        context: "by a synchronously-called image listener",
                        exception: ex
                    );
                }
            }

            if (this.currentError != null && onError != null) {
                try {
                    onError(this.currentError.exception);
                }
                catch (Exception ex) {
                    UIWidgetsError.reportError(
                        new UIWidgetsErrorDetails(
                            exception: ex,
                            library: "image resource service",
                            context: "when reporting an error to an image listener"
                        )
                    );
                }
            }
        }

        public virtual void removeListener(ImageListener listener) {
            for (int i = 0; i < this._listeners.Count; i++) {
                if (this._listeners[i].listener == listener) {
                    this._listeners.RemoveAt(i);
                    break;
                }
            }
        }

        protected void setImage(ImageInfo image) {
            this.currentImage = image;
            if (this._listeners.isEmpty()) {
                return;
            }

            var localListeners = this._listeners.Select(l => l.listener).ToList();
            foreach (var listener in localListeners) {
                try {
                    listener(image, false);
                }
                catch (Exception ex) {
                    this.reportError(
                        context: "by an image listener",
                        exception: ex
                    );
                }
            }
        }

        protected void reportError(
            string context = null,
            Exception exception = null,
            InformationCollector informationCollector = null,
            bool silent = false) {
            this.currentError = new UIWidgetsErrorDetails(
                exception: exception,
                library: "image resource service",
                context: context,
                informationCollector: informationCollector,
                silent: silent
            );

            var localErrorListeners = this._listeners.Select(l => l.errorListener).Where(l => l != null).ToList();

            if (localErrorListeners.isEmpty()) {
                UIWidgetsError.reportError(this.currentError);
            }
            else {
                foreach (var errorListener in localErrorListeners) {
                    try {
                        errorListener(exception);
                    }
                    catch (Exception ex) {
                        UIWidgetsError.reportError(
                            new UIWidgetsErrorDetails(
                                context: "when reporting an error to an image listener",
                                library: "image resource service",
                                exception: ex
                            )
                        );
                    }
                }
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<ImageInfo>(
                "current", this.currentImage, ifNull: "unresolved", showName: false));
            description.add(new ObjectFlagProperty<List<_ImageListenerPair>>(
                "listeners",
                this._listeners,
                ifPresent: $"{this._listeners.Count} listener{(this._listeners.Count == 1 ? "" : "s")}"
            ));
        }
    }

    public class OneFrameImageStreamCompleter : ImageStreamCompleter {
        public OneFrameImageStreamCompleter(IPromise<ImageInfo> image,
            InformationCollector informationCollector = null) {
            D.assert(image != null);

            image.Then(result => { this.setImage(result); }).Catch(err => {
                this.reportError(
                    context: "resolving a single-frame image stream",
                    exception: err,
                    informationCollector: informationCollector,
                    silent: true
                );
            });
        }
    }

    public class MultiFrameImageStreamCompleter : ImageStreamCompleter {
        public MultiFrameImageStreamCompleter(
            IPromise<Codec> codec,
            float scale,
            InformationCollector informationCollector = null
        ) {
            D.assert(codec != null);

            this._scale = scale;
            this._informationCollector = informationCollector;

            codec.Then((Action<Codec>) this._handleCodecReady, ex => {
                this.reportError(
                    context: "resolving an image codec",
                    exception: ex,
                    informationCollector: informationCollector,
                    silent: true
                );
            });
        }

        Codec _codec;
        readonly float _scale;
        readonly InformationCollector _informationCollector;

        FrameInfo _nextFrame;
        TimeSpan? _shownTimestamp;
        TimeSpan? _frameDuration;
        int _framesEmitted = 0;
        Timer _timer;

        bool _frameCallbackScheduled = false;

        void _handleCodecReady(Codec codec) {
            this._codec = codec;
            D.assert(this._codec != null);

            if (this.hasListeners) {
                this._decodeNextFrameAndSchedule();
            }
        }

        void _handleAppFrame(TimeSpan timestamp) {
            this._frameCallbackScheduled = false;
            if (!this.hasListeners) {
                return;
            }

            if (this._isFirstFrame() || this._hasFrameDurationPassed(timestamp)) {
                this._emitFrame(new ImageInfo(image: this._nextFrame.image, scale: this._scale));
                this._shownTimestamp = timestamp;
                this._frameDuration = this._nextFrame.duration;
                this._nextFrame = null;
                int completedCycles = this._codec.frameCount == 0 ? 0 : this._framesEmitted / this._codec.frameCount;

                if (this._codec.repetitionCount == -1 || completedCycles <= this._codec.repetitionCount) {
                    this._decodeNextFrameAndSchedule();
                }

                return;
            }

            TimeSpan delay = this._frameDuration.Value - (timestamp - this._shownTimestamp.Value);
            delay = new TimeSpan((long) (delay.Ticks * SchedulerBinding.instance.timeDilation));
            this._timer = Window.instance.run(delay, this._scheduleAppFrame);
        }

        bool _isFirstFrame() {
            return this._frameDuration == null;
        }

        bool _hasFrameDurationPassed(TimeSpan timestamp) {
            D.assert(this._shownTimestamp != null);
            return timestamp - this._shownTimestamp >= this._frameDuration;
        }

        void _decodeNextFrameAndSchedule() {
            var frame = this._codec.getNextFrame();
            this._nextFrame = frame;

            if (this._codec.frameCount == 1) {
                this._emitFrame(new ImageInfo(image: this._nextFrame.image, scale: this._scale));
                return;
            }

            this._scheduleAppFrame();
        }

        void _scheduleAppFrame() {
            if (this._frameCallbackScheduled) {
                return;
            }

            this._frameCallbackScheduled = true;
            SchedulerBinding.instance.scheduleFrameCallback(this._handleAppFrame);
        }

        void _emitFrame(ImageInfo imageInfo) {
            this.setImage(imageInfo);
            this._framesEmitted += 1;
        }

        public override void addListener(ImageListener listener, ImageErrorListener onError = null) {
            if (!this.hasListeners && this._codec != null) {
                this._decodeNextFrameAndSchedule();
            }

            base.addListener(listener, onError: onError);
        }

        public override void removeListener(ImageListener listener) {
            base.removeListener(listener);
            if (!this.hasListeners) {
                this._timer?.cancel();
                this._timer = null;
            }
        }
    }
}