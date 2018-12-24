using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIWidgets.async {
    public abstract class Timer {
        public abstract void cancel();

        static readonly TimerProvider _globalTimerProvider = new TimerProvider();
        
        public static Timer run(TimeSpan duration, Action callback) {
            return _globalTimerProvider.run(duration, callback);
        }

        public static Timer run(Action callback) {
            return run(TimeSpan.Zero, callback);
        }

        internal static void update() {
            _globalTimerProvider.update();
        }
    }

    public class TimerProvider {
        private readonly PriorityQueue<TimerImpl> _queue;

        public TimerProvider() {
            this._queue = new PriorityQueue<TimerImpl>();
        }

        public Timer run(TimeSpan duration, Action callback) {
            var timer = new TimerImpl(DateTime.Now + duration, callback);

            lock (this._queue) {
                this._queue.enqueue(timer);
            }

            return timer;
        }

        public void update() {
            var now = DateTime.Now;

            var timers = new List<TimerImpl>();

            lock (this._queue) {
                while (this._queue.count > 0 && this._queue.peek().deadline <= now) {
                    var timer = this._queue.dequeue();
                    timers.Add(timer);
                }
            }

            foreach (var timer in timers) {
                timer.invoke();
            }
        }

        private class TimerImpl : Timer, IComparable<TimerImpl> {
            public readonly DateTime deadline;
            private readonly Action _callback;
            private bool _done;

            public TimerImpl(DateTime deadline, Action callback) {
                this.deadline = deadline;
                this._callback = callback;
                this._done = false;
            }

            public override void cancel() {
                this._done = true;
            }

            public void invoke() {
                if (this._done) {
                    return;
                }

                this._done = true;

                try {
                    this._callback();
                }
                catch (Exception ex) {
                    Debug.LogError("Error to execute timer callback: " + ex);
                }
            }

            public int CompareTo(TimerImpl other) {
                return this.deadline.CompareTo(other.deadline);
            }
        }
    }
}