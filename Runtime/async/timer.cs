using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Unity.UIWidgets.async {
    public abstract class Timer : IDisposable {
        public abstract void cancel();

        public void Dispose() {
            this.cancel();
        }

        public static float timeSinceStartup {
            get {
#if UNITY_EDITOR
                return (float) EditorApplication.timeSinceStartup;
#else
                return Time.realtimeSinceStartup;
#endif
            }
        }

        public static TimeSpan timespanSinceStartup {
            get { return TimeSpan.FromSeconds(timeSinceStartup); }
        }

        static readonly object _syncObj = new object();

        static LinkedList<Action> _callbacks = new LinkedList<Action>();

        public static void runInMainFromFinalizer(Action callback) {
            lock (_syncObj) {
                _callbacks.AddLast(callback);
            }
        }

        internal static void update() {
            LinkedList<Action> callbacks;

            lock (_syncObj) {
                if (_callbacks.isEmpty()) {
                    return;
                }

                callbacks = _callbacks;
                _callbacks = new LinkedList<Action>();
            }

            foreach (var callback in callbacks) {
                try {
                    callback();
                }
                catch (Exception ex) {
                    D.logError("Error to execute runInMain callback: ", ex);
                }
            }
        }
    }

    public class TimerProvider {
        readonly PriorityQueue<TimerImpl> _queue;

        public TimerProvider() {
            this._queue = new PriorityQueue<TimerImpl>();
        }

        public Timer runInMain(Action callback) {
            var timer = new TimerImpl(callback);

            lock (this._queue) {
                this._queue.enqueue(timer);
            }

            return timer;
        }

        public Timer run(TimeSpan duration, Action callback) {
            var timer = new TimerImpl(duration, callback);

            lock (this._queue) {
                this._queue.enqueue(timer);
            }

            return timer;
        }

        public Timer periodic(TimeSpan duration, Action callback) {
            var timer = new TimerImpl(duration, callback, periodic: true);

            lock (this._queue) {
                this._queue.enqueue(timer);
            }

            return timer;
        }

        public void update(Action flushMicroTasks = null) {
            var now = Timer.timeSinceStartup;

            List<TimerImpl> timers = null;
            List<TimerImpl> appendList = null;

            lock (this._queue) {
                while (this._queue.count > 0 && this._queue.peek().deadline <= now) {
                    var timer = this._queue.dequeue();
                    if (timers == null) {
                        timers = new List<TimerImpl>();
                    }

                    timers.Add(timer);
                }
            }

            if (timers != null) {
                foreach (var timer in timers) {
                    if (flushMicroTasks != null) {
                        flushMicroTasks();
                    }

                    timer.invoke();
                    if (timer.periodic && !timer.done) {
                        if (appendList == null) {
                            appendList = new List<TimerImpl>();
                        }

                        appendList.Add(timer);
                    }
                }
            }

            if (appendList != null) {
                lock (this._queue) {
                    foreach (var timer in appendList) {
                        this._queue.enqueue(timer);
                    }
                }
            }
        }

        class TimerImpl : Timer, IComparable<TimerImpl> {
            float _deadline;
            readonly Action _callback;
            bool _done;

            public readonly bool periodic;
            readonly TimeSpan _interval;

            public TimerImpl(TimeSpan duration, Action callback, bool periodic = false) {
                this._deadline = timeSinceStartup + (float) duration.TotalSeconds;
                this._callback = callback;
                this._done = false;

                this.periodic = periodic;
                if (periodic) {
                    this._interval = duration;
                }
            }

            public TimerImpl(Action callback) {
                this._deadline = 0;
                this._callback = callback;
                this._done = false;
            }

            public float deadline {
                get { return this._deadline; }
            }

            public override void cancel() {
                this._done = true;
            }

            public bool done {
                get { return this._done; }
            }

            public void invoke() {
                if (this._done) {
                    return;
                }

                var now = timeSinceStartup;
                if (!this.periodic) {
                    this._done = true;
                }

                try {
                    this._callback();
                }
                catch (Exception ex) {
                    D.logError("Error to execute timer callback: ", ex);
                }

                if (this.periodic) {
                    this._deadline = now + (float) this._interval.TotalSeconds;
                }
            }

            public int CompareTo(TimerImpl other) {
                return this.deadline.CompareTo(other.deadline);
            }
        }
    }
}