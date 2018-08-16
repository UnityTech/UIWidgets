using System;
using System.Collections.Generic;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.scheduler {
    public enum SchedulerPhase {
        idle,
        transientCallbacks,
        midFrameMicrotasks,
        persistentCallbacks,
        postFrameCallbacks,
    }

    public class _FrameCallbackEntry {
        public _FrameCallbackEntry(FrameCallback callback, bool rescheduling = false) {
            this.callback = callback;
        }

        public readonly FrameCallback callback;
    }

    public abstract class SchedulerBinding {
        public SchedulerBinding(Window window) {
            this._window = window;

            window.onBeginFrame = this._handleBeginFrame;
            window.onDrawFrame = this._handleDrawFrame;
        }

        public readonly Window _window;

        public double timeDilation {
            get { return this._timeDilation; }
            set {
                if (this._timeDilation == value) {
                    return;
                }

                this.resetEpoch();
                this._timeDilation = value;
            }
        }

        public double _timeDilation = 1.0;


        public int _nextFrameCallbackId = 0;
        public Dictionary<int, _FrameCallbackEntry> _transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();
        public HashSet<int> _removedIds = new HashSet<int>();

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
            this._transientCallbacks.Remove(id);
            this._removedIds.Add(id);
        }

        public readonly List<FrameCallback> _persistentCallbacks = new List<FrameCallback>();

        public void addPersistentFrameCallback(FrameCallback callback) {
            this._persistentCallbacks.Add(callback);
        }

        public readonly List<FrameCallback> _postFrameCallbacks = new List<FrameCallback>();

        public void addPostFrameCallback(FrameCallback callback) {
            this._postFrameCallbacks.Add(callback);
        }

        public bool hasScheduledFrame {
            get { return this._hasScheduledFrame; }
        }

        public bool _hasScheduledFrame = false;

        public SchedulerPhase schedulerPhase {
            get { return this._schedulerPhase; }
        }

        public SchedulerPhase _schedulerPhase = SchedulerPhase.idle;

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
            if (this._hasScheduledFrame) {
                return;
            }

            this._window.scheduleFrame();
            this._hasScheduledFrame = true;
        }

        public void scheduleForcedFrame() {
            this.scheduleFrame();
        }

        public TimeSpan? _firstRawTimeStampInEpoch;
        public TimeSpan _epochStart = TimeSpan.Zero;
        public TimeSpan _lastRawTimeStamp = TimeSpan.Zero;

        public void resetEpoch() {
            this._epochStart = this._adjustForEpoch(this._lastRawTimeStamp);
            this._firstRawTimeStampInEpoch = null;
        }

        public TimeSpan _adjustForEpoch(TimeSpan rawTimeStamp) {
            var rawDurationSinceEpoch = this._firstRawTimeStampInEpoch == null
                ? TimeSpan.Zero
                : rawTimeStamp - this._firstRawTimeStampInEpoch.Value;
            return new TimeSpan((long) (rawDurationSinceEpoch.Ticks / this.timeDilation) + this._epochStart.Ticks);
        }

        public TimeSpan currentFrameTimeStamp {
            get { return this._currentFrameTimeStamp.Value; }
        }

        public TimeSpan? _currentFrameTimeStamp;

        public void _handleBeginFrame(TimeSpan rawTimeStamp) {
            this.handleBeginFrame(rawTimeStamp);
        }

        public void _handleDrawFrame() {
            this.handleDrawFrame();
        }

        public void handleBeginFrame(TimeSpan? rawTimeStamp) {
            if (this._firstRawTimeStampInEpoch == null) {
                this._firstRawTimeStampInEpoch = rawTimeStamp;
            }

            this._currentFrameTimeStamp = this._adjustForEpoch(rawTimeStamp ?? this._lastRawTimeStamp);

            if (rawTimeStamp != null) {
                this._lastRawTimeStamp = rawTimeStamp.Value;
            }

            this._hasScheduledFrame = false;

            try {
                this._schedulerPhase = SchedulerPhase.transientCallbacks;
                var callbacks = this._transientCallbacks;
                this._transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();
                foreach (var entry in callbacks) {
                    if (!this._removedIds.Contains(entry.Key)) {
                        this._invokeFrameCallback(entry.Value.callback, this._currentFrameTimeStamp.Value);
                    }
                }

                this._removedIds.Clear();
            }
            finally {
                this._schedulerPhase = SchedulerPhase.midFrameMicrotasks;
            }
        }

        public void _invokeFrameCallback(FrameCallback callback, TimeSpan timeStamp) {
            try {
                callback(timeStamp);
            }
            catch (Exception ex) {
                Debug.LogError("error in frame callback: " + ex);
            }
        }

        public void handleDrawFrame() {
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
            }
            finally {
                this._schedulerPhase = SchedulerPhase.idle;
                this._currentFrameTimeStamp = null;
            }
        }
    }
}