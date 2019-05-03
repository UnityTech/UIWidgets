using System;
using System.Collections.Generic;
using System.Text;
using RSG.Promises;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Debug = UnityEngine.Debug;

namespace Unity.UIWidgets.scheduler {
    class _FrameCallbackEntry {
        internal _FrameCallbackEntry(FrameCallback callback, bool rescheduling = false) {
            this.callback = callback;

            D.assert(() => {
                if (rescheduling) {
                    D.assert(() => {
                        if (debugCurrentCallbackStack == null) {
                            throw new UIWidgetsError(
                                "scheduleFrameCallback called with rescheduling true, but no callback is in scope.\n" +
                                "The \"rescheduling\" argument should only be set to true if the " +
                                "callback is being reregistered from within the callback itself, " +
                                "and only then if the callback itself is entirely synchronous. " +
                                "If this is the initial registration of the callback, or if the " +
                                "callback is asynchronous, then do not use the \"rescheduling\" " +
                                "argument.");
                        }

                        return true;
                    });
                    this.debugStack = debugCurrentCallbackStack;
                } else {
                    this.debugStack = "skipped, use StackTraceUtility.ExtractStackTrace() if you need it"; // StackTraceUtility.ExtractStackTrace();
                }

                return true;
            });
        }

        public readonly FrameCallback callback;

        internal static string debugCurrentCallbackStack;
        internal string debugStack;
    }

    public enum SchedulerPhase {
        idle,
        transientCallbacks,
        midFrameMicrotasks,
        persistentCallbacks,
        postFrameCallbacks,
    }

    public class SchedulerBinding {
        public static SchedulerBinding instance {
            get {
                D.assert(_instance != null,
                    () => "Binding.instance is null. " +
                    "This usually happens when there is a callback from outside of UIWidgets. " +
                    "Try to use \"using (WindowProvider.of(BuildContext).getScope()) { ... }\" to wrap your code.");
                return _instance;
            }

            set {
                if (value == null) {
                    D.assert(_instance != null, () => "Binding.instance is already cleared.");
                    _instance = null;
                } else {
                    D.assert(_instance == null, () => "Binding.instance is already assigned.");
                    _instance = value;
                }
            }
        }

        internal static SchedulerBinding _instance;

        public SchedulerBinding() {
            Window.instance.onBeginFrame += this._handleBeginFrame;
            Window.instance.onDrawFrame += this._handleDrawFrame;
        }

        public float timeDilation {
            get { return this._timeDilation; }
            set {
                if (this._timeDilation == value) {
                    return;
                }

                this.resetEpoch();
                this._timeDilation = value;
            }
        }

        float _timeDilation = 1.0f;

        int _nextFrameCallbackId = 0;
        Dictionary<int, _FrameCallbackEntry> _transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();
        readonly HashSet<int> _removedIds = new HashSet<int>();

        public int transientCallbackCount {
            get { return this._transientCallbacks.Count; }
        }

        public int scheduleFrameCallback(FrameCallback callback, bool rescheduling = false) {
            this.scheduleFrame();
            this._nextFrameCallbackId += 1;
            this._transientCallbacks[this._nextFrameCallbackId] =
                new _FrameCallbackEntry(callback, rescheduling: rescheduling);
            return this._nextFrameCallbackId;
        }

        public void cancelFrameCallbackWithId(int id) {
            D.assert(id > 0);
            this._transientCallbacks.Remove(id);
            this._removedIds.Add(id);
        }

        readonly List<FrameCallback> _persistentCallbacks = new List<FrameCallback>();

        public void addPersistentFrameCallback(FrameCallback callback) {
            this._persistentCallbacks.Add(callback);
        }

        readonly List<FrameCallback> _postFrameCallbacks = new List<FrameCallback>();

        public void addPostFrameCallback(FrameCallback callback) {
            this._postFrameCallbacks.Add(callback);
        }

        public bool hasScheduledFrame {
            get { return this._hasScheduledFrame; }
        }

        bool _hasScheduledFrame = false;

        public SchedulerPhase schedulerPhase {
            get { return this._schedulerPhase; }
        }

        SchedulerPhase _schedulerPhase = SchedulerPhase.idle;

        public bool framesEnabled {
            get { return this._framesEnabled; }
            set {
                if (this._framesEnabled == value) {
                    return;
                }

                this._framesEnabled = value;
                if (value) {
                    this.scheduleFrame();
                }
            }
        }

        bool _framesEnabled = true; // todo: set it to false when app switched to background

        public void ensureVisualUpdate() {
            switch (this.schedulerPhase) {
                case SchedulerPhase.idle:
                case SchedulerPhase.postFrameCallbacks:
                    this.scheduleFrame();
                    return;
                case SchedulerPhase.transientCallbacks:
                case SchedulerPhase.midFrameMicrotasks:
                case SchedulerPhase.persistentCallbacks:
                    return;
            }
        }

        public void scheduleFrame() {
            if (this._hasScheduledFrame || !this._framesEnabled) {
                return;
            }

            D.assert(() => {
                if (D.debugPrintScheduleFrameStacks) {
                    Debug.LogFormat("scheduleFrame() called. Current phase is {0}.", this.schedulerPhase);
                }

                return true;
            });

            Window.instance.scheduleFrame();
            this._hasScheduledFrame = true;
        }

        public void scheduleForcedFrame() {
            if (this._hasScheduledFrame) {
                return;
            }

            D.assert(() => {
                if (D.debugPrintScheduleFrameStacks) {
                    Debug.LogFormat("scheduleForcedFrame() called. Current phase is {0}.", this.schedulerPhase);
                }

                return true;
            });

            Window.instance.scheduleFrame();
            this._hasScheduledFrame = true;
        }

        TimeSpan? _firstRawTimeStampInEpoch;
        TimeSpan _epochStart = TimeSpan.Zero;
        TimeSpan _lastRawTimeStamp = TimeSpan.Zero;

        public void resetEpoch() {
            this._epochStart = this._adjustForEpoch(this._lastRawTimeStamp);
            this._firstRawTimeStampInEpoch = null;
        }

        TimeSpan _adjustForEpoch(TimeSpan rawTimeStamp) {
            var rawDurationSinceEpoch = this._firstRawTimeStampInEpoch == null
                ? TimeSpan.Zero
                : rawTimeStamp - this._firstRawTimeStampInEpoch.Value;
            return new TimeSpan((long) (rawDurationSinceEpoch.Ticks / this.timeDilation) + this._epochStart.Ticks);
        }

        public TimeSpan currentFrameTimeStamp {
            get { return this._currentFrameTimeStamp.Value; }
        }

        TimeSpan? _currentFrameTimeStamp;

        int _profileFrameNumber = 0;
        string _debugBanner;

        void _handleBeginFrame(TimeSpan rawTimeStamp) {
            this.handleBeginFrame(rawTimeStamp);
        }

        void _handleDrawFrame() {
            this.handleDrawFrame();
        }

        public void handleBeginFrame(TimeSpan? rawTimeStamp) {
            this._firstRawTimeStampInEpoch = this._firstRawTimeStampInEpoch ?? rawTimeStamp;
            this._currentFrameTimeStamp = this._adjustForEpoch(rawTimeStamp ?? this._lastRawTimeStamp);

            if (rawTimeStamp != null) {
                this._lastRawTimeStamp = rawTimeStamp.Value;
            }

            D.assert(() => {
                this._profileFrameNumber += 1;
                return true;
            });

            D.assert(() => {
                if (D.debugPrintBeginFrameBanner || D.debugPrintEndFrameBanner) {
                    var frameTimeStampDescription = new StringBuilder();
                    if (rawTimeStamp != null) {
                        _debugDescribeTimeStamp(
                            this._currentFrameTimeStamp.Value, frameTimeStampDescription);
                    } else {
                        frameTimeStampDescription.Append("(warm-up frame)");
                    }

                    this._debugBanner =
                        $"▄▄▄▄▄▄▄▄ Frame {this._profileFrameNumber.ToString().PadRight(7)}   {frameTimeStampDescription.ToString().PadLeft(18)} ▄▄▄▄▄▄▄▄";
                    if (D.debugPrintBeginFrameBanner) {
                        Debug.Log(this._debugBanner);
                    }
                }

                return true;
            });

            D.assert(this._schedulerPhase == SchedulerPhase.idle);
            this._hasScheduledFrame = false;

            try {
                this._schedulerPhase = SchedulerPhase.transientCallbacks;
                var callbacks = this._transientCallbacks;
                this._transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();
                foreach (var entry in callbacks) {
                    if (!this._removedIds.Contains(entry.Key)) {
                        this._invokeFrameCallback(
                            entry.Value.callback, this._currentFrameTimeStamp.Value, entry.Value.debugStack);
                    }
                }

                this._removedIds.Clear();
            } finally {
                this._schedulerPhase = SchedulerPhase.midFrameMicrotasks;
            }
        }


        public void handleDrawFrame() {
            D.assert(this._schedulerPhase == SchedulerPhase.midFrameMicrotasks);

            try {
                this._schedulerPhase = SchedulerPhase.persistentCallbacks;
                foreach (FrameCallback callback in this._persistentCallbacks) {
                    this._invokeFrameCallback(callback, this._currentFrameTimeStamp.Value);
                }

                this._schedulerPhase = SchedulerPhase.postFrameCallbacks;
                var localPostFrameCallbacks = new List<FrameCallback>(this._postFrameCallbacks);
                this._postFrameCallbacks.Clear();
                foreach (FrameCallback callback in localPostFrameCallbacks) {
                    this._invokeFrameCallback(callback, this._currentFrameTimeStamp.Value);
                }
            } finally {
                this._schedulerPhase = SchedulerPhase.idle;
                D.assert(() => {
                    if (D.debugPrintEndFrameBanner) {
                        Debug.Log(new string('▀', this._debugBanner.Length));
                    }

                    this._debugBanner = null;
                    return true;
                });
                this._currentFrameTimeStamp = null;
            }
        }

        static void _debugDescribeTimeStamp(TimeSpan timeStamp, StringBuilder buffer) {
            if (timeStamp.TotalDays > 0) {
                buffer.AppendFormat("{0}d ", timeStamp.Days);
            }

            if (timeStamp.TotalHours > 0) {
                buffer.AppendFormat("{0}h ", timeStamp.Hours);
            }

            if (timeStamp.TotalMinutes > 0) {
                buffer.AppendFormat("{0}m ", timeStamp.Minutes);
            }

            if (timeStamp.TotalSeconds > 0) {
                buffer.AppendFormat("{0}s ", timeStamp.Seconds);
            }

            buffer.AppendFormat("{0}", timeStamp.Milliseconds);

            int microseconds = (int) (timeStamp.Ticks % 10000 / 10);
            if (microseconds > 0) {
                buffer.AppendFormat(".{0}", microseconds.ToString().PadLeft(3, '0'));
            }

            buffer.Append("ms");
        }

        void _invokeFrameCallback(FrameCallback callback, TimeSpan timeStamp, string callbackStack = null) {
            D.assert(callback != null);
            D.assert(_FrameCallbackEntry.debugCurrentCallbackStack == null);
            D.assert(() => {
                _FrameCallbackEntry.debugCurrentCallbackStack = callbackStack;
                return true;
            });

            try {
                callback(timeStamp);
            } catch (Exception ex) {
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "scheduler library",
                    context: "during a scheduler callback",
                    informationCollector: callbackStack == null
                        ? (InformationCollector) null
                        : information => {
                            information.AppendLine(
                                "\nThis exception was thrown in the context of a scheduler callback. " +
                                "When the scheduler callback was _registered_ (as opposed to when the " +
                                "exception was thrown), this was the stack:"
                            );
                            UIWidgetsError.defaultStackFilter(callbackStack.TrimEnd().Split('\n'))
                                .Each((line) => information.AppendLine(line));
                        }
                ));
            }

            D.assert(() => {
                _FrameCallbackEntry.debugCurrentCallbackStack = null;
                return true;
            });
        }
    }
}
