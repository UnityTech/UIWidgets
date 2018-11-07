using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIWidgets.async {
    public abstract class Timer {
        public abstract void cancel();
    }

    public class TimerProvider {
        private readonly PriorityQueue<TimerImpl> _queue;

        public TimerProvider() {
            this._queue = new PriorityQueue<TimerImpl>();
        }

        public Timer run(TimeSpan duration, Action callback) {
            var timer = new TimerImpl(duration, callback);
            this._queue.enqueue(timer);

            return timer;
        }

        public Timer periodic(TimeSpan duration, Action callback)
        {
            var timer = new TimerImpl(duration, callback, periodic: true);
            this._queue.enqueue(timer);

            return timer;
        }
        
        public void update() {
            var now = DateTime.Now;
            List<TimerImpl> appendList = null;
            while (this._queue.count > 0 && this._queue.peek().deadline <= now) {
                var timer = this._queue.dequeue();
                timer.invoke();
                if (timer.periodic && !timer.done)
                {
                    if (appendList == null)
                    {
                        appendList = new List<TimerImpl>();
                    }
                    appendList.Add(timer);
                }
            }

            if (appendList != null)
            {
                foreach (var timer in appendList)
                {
                    this._queue.enqueue(timer);
                }
            }
        }
        

        private class TimerImpl : Timer, IComparable<TimerImpl> {
            public readonly bool periodic;
            public readonly TimeSpan internval;
            private DateTime _deadline;
            private readonly Action _callback;
            private bool _done;
           
            public TimerImpl(TimeSpan duration, Action callback, bool periodic = false) {
                this._deadline = DateTime.Now + duration;
                this._callback = callback;
                this.periodic = periodic;
                this._done = false;
                if (periodic)
                {
                    internval = duration;
                }
            }

            public DateTime deadline
            {
                get { return _deadline; }
            }
            
            public override void cancel() {
                this._done = true;
            }

            public bool done
            {
                get { return _done; }
            }
            
            public void invoke() {
                if (this._done) {
                    return;
                }

                DateTime now = DateTime.Now;
                if (!periodic)
                {
                    this._done = true;    
                }

                try {
                    this._callback();
                }
                catch (Exception ex) {
                    Debug.LogError("Error to execute timer callback: " + ex);
                }

                if (periodic)
                {
                    this._deadline = now + internval;
                }
            }

            public int CompareTo(TimerImpl other) {
                return this.deadline.CompareTo(other.deadline);
            }
        }
    }
}