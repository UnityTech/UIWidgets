using System;
using System.Collections.Generic;
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

        public static double timeSinceStartup {
            get {
#if UNITY_EDITOR
                return EditorApplication.timeSinceStartup;
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

        internal static void runInMainFromFinalizer(Action callback) {
            lock (_syncObj) {
                _callbacks.AddLast(callback);
            }
        }

        public static void runInMain(Action callback) {
            lock (_syncObj) {
                _callbacks.AddLast(callback);
            }
        }

        internal static void update() {
            LinkedList<Action> callbacks;

            lock (_syncObj) {
                callbacks = _callbacks;
                _callbacks = new LinkedList<Action>();
            }

            foreach (var callback in callbacks) {
                try {
                    callback();
                } catch (Exception ex) {
                    Debug.LogError("Error to execute runInMain callback: " + ex);
                }
            }
        }
    }

    public class TimerProvider {
        readonly PriorityQueue<TimerImpl> _queue;

        public TimerProvider() {
            this._queue = new PriorityQueue<TimerImpl>();
        }

        public Timer run(TimeSpan duration, Action callback) {
            var timer = new TimerImpl(duration, callback);

            this._queue.enqueue(timer);
            return timer;
        }

        public Timer periodic(TimeSpan duration, Action callback) {
            var timer = new TimerImpl(duration, callback, periodic: true);

            this._queue.enqueue(timer);
            return timer;
        }

        public void update(Action flushMicroTasks = null) {
            var now = Timer.timeSinceStartup;

            List<TimerImpl> timers = null;
            List<TimerImpl> appendList = null;

            while (this._queue.count > 0 && this._queue.peek().deadline <= now) {
                var timer = this._queue.dequeue();
                if (timers == null) {
                    timers = new List<TimerImpl>();
                }

                timers.Add(timer);
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
                foreach (var timer in appendList) {
                    this._queue.enqueue(timer);
                }
            }
        }

        class TimerImpl : Timer, IComparable<TimerImpl> {
            public readonly bool periodic;
            public readonly TimeSpan internval;
            double _deadline;
            readonly Action _callback;
            bool _done;

            public TimerImpl(TimeSpan duration, Action callback, bool periodic = false) {
                this._deadline = timeSinceStartup + duration.TotalSeconds;
                this._callback = callback;
                this.periodic = periodic;
                this._done = false;
                if (periodic) {
                    this.internval = duration;
                }
            }

            public double deadline {
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
                } catch (Exception ex) {
                    Debug.LogError("Error to execute timer callback: " + ex);
                }

                if (this.periodic) {
                    this._deadline = now + this.internval.TotalSeconds;
                }
            }

            public int CompareTo(TimerImpl other) {
                return this.deadline.CompareTo(other.deadline);
            }
        }
    }
}
