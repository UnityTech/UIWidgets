using System;
using System.Text;
using RSG;
using RSG.Promises;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.scheduler {
    public delegate void TickerCallback(TimeSpan elapsed);

    public interface TickerProvider {
        Ticker createTicker(TickerCallback onTick);
    }

    public class Ticker {
        public Ticker(TickerCallback onTick, Func<string> debugLabel = null) {
            D.assert(() => {
                this._debugCreationStack = "skipped, use StackTraceUtility.ExtractStackTrace() if you need it"; // StackTraceUtility.ExtractStackTrace();
                return true;
            });
            this._onTick = onTick;
            this.debugLabel = debugLabel;
        }

        TickerFutureImpl _future;

        public bool muted {
            get { return this._muted; }
            set {
                if (value == this._muted) {
                    return;
                }

                this._muted = value;
                if (value) {
                    this.unscheduleTick();
                }
                else if (this.shouldScheduleTick) {
                    this.scheduleTick();
                }
            }
        }

        bool _muted = false;

        public bool isTicking {
            get {
                if (this._future == null) {
                    return false;
                }

                if (this.muted) {
                    return false;
                }

                if (SchedulerBinding.instance.framesEnabled) {
                    return true;
                }

                if (SchedulerBinding.instance.schedulerPhase != SchedulerPhase.idle) {
                    return true;
                }

                return false;
            }
        }

        public bool isActive {
            get { return this._future != null; }
        }

        TimeSpan? _startTime;

        public TickerFuture start() {
            D.assert(() => {
                if (this.isActive) {
                    throw new UIWidgetsError(
                        "A ticker that is already active cannot be started again without first stopping it.\n" +
                        "The affected ticker was: " + this.toString(debugIncludeStack: true));
                }

                return true;
            });

            D.assert(this._startTime == null);
            this._future = new TickerFutureImpl();
            if (this.shouldScheduleTick) {
                this.scheduleTick();
            }

            if (SchedulerBinding.instance.schedulerPhase > SchedulerPhase.idle &&
                SchedulerBinding.instance.schedulerPhase < SchedulerPhase.postFrameCallbacks) {
                this._startTime = SchedulerBinding.instance.currentFrameTimeStamp;
            }

            return this._future;
        }

        public void stop(bool canceled = false) {
            if (!this.isActive) {
                return;
            }

            var localFuture = this._future;
            this._future = null;
            this._startTime = null;
            D.assert(!this.isActive);

            this.unscheduleTick();
            if (canceled) {
                localFuture._cancel(this);
            }
            else {
                localFuture._complete();
            }
        }

        readonly TickerCallback _onTick;

        int? _animationId;

        protected bool scheduled {
            get { return this._animationId != null; }
        }

        protected bool shouldScheduleTick {
            get { return !this.muted && this.isActive && !this.scheduled; }
        }

        void _tick(TimeSpan timeStamp) {
            D.assert(this.isTicking);
            D.assert(this.scheduled);
            this._animationId = null;

            this._startTime = this._startTime ?? timeStamp;

            this._onTick(timeStamp - this._startTime.Value);

            if (this.shouldScheduleTick) {
                this.scheduleTick(rescheduling: true);
            }
        }

        protected void scheduleTick(bool rescheduling = false) {
            D.assert(!this.scheduled);
            D.assert(this.shouldScheduleTick);
            this._animationId = SchedulerBinding.instance.scheduleFrameCallback(this._tick, rescheduling: rescheduling);
        }

        protected void unscheduleTick() {
            if (this.scheduled) {
                SchedulerBinding.instance.cancelFrameCallbackWithId(this._animationId.Value);
                this._animationId = null;
            }

            D.assert(!this.shouldScheduleTick);
        }

        public void absorbTicker(Ticker originalTicker) {
            D.assert(!this.isActive);
            D.assert(this._future == null);
            D.assert(this._startTime == null);
            D.assert(this._animationId == null);
            D.assert((originalTicker._future == null) == (originalTicker._startTime == null),
                () => "Cannot absorb Ticker after it has been disposed.");
            if (originalTicker._future != null) {
                this._future = originalTicker._future;
                this._startTime = originalTicker._startTime;
                if (this.shouldScheduleTick) {
                    this.scheduleTick();
                }

                originalTicker._future = null;
                originalTicker.unscheduleTick();
            }

            originalTicker.dispose();
        }

        public virtual void dispose() {
            if (this._future != null) {
                var localFuture = this._future;
                this._future = null;
                D.assert(!this.isActive);
                this.unscheduleTick();
                localFuture._cancel(this);
            }

            D.assert(() => {
                this._startTime = default(TimeSpan);
                return true;
            });
        }

        internal readonly Func<string> debugLabel;

        string _debugCreationStack;

        public override string ToString() {
            return this.toString(debugIncludeStack: false);
        }

        public string toString(bool debugIncludeStack = false) {
            var buffer = new StringBuilder();
            buffer.Append(this.GetType() + "(");
            D.assert(() => {
                if (this.debugLabel != null) {
                    buffer.Append(this.debugLabel());
                }
                return true;
            });
            buffer.Append(')');
            D.assert(() => {
                if (debugIncludeStack) {
                    buffer.AppendLine();
                    buffer.AppendLine("The stack trace when the " + this.GetType() + " was actually created was:");
                    UIWidgetsError.defaultStackFilter(this._debugCreationStack.TrimEnd().Split('\n'))
                        .Each(line => buffer.AppendLine(line));
                }

                return true;
            });
            return buffer.ToString();
        }
    }

    public interface TickerFuture : IPromise {
        void whenCompleteOrCancel(VoidCallback callback);

        IPromise orCancel { get; }
    }

    public class TickerFutureImpl : Promise, TickerFuture {
        public static TickerFuture complete() {
            var result = new TickerFutureImpl();
            result._complete();
            return result;
        }

        Promise _secondaryCompleter;
        bool? _completed;

        internal void _complete() {
            D.assert(this._completed == null);
            this._completed = true;
            this.Resolve();
            if (this._secondaryCompleter != null) {
                this._secondaryCompleter.Resolve();
            }
        }

        internal void _cancel(Ticker ticker) {
            D.assert(this._completed == null);
            this._completed = false;
            if (this._secondaryCompleter != null) {
                this._secondaryCompleter.Reject(new TickerCanceled(ticker));
            }
        }

        public void whenCompleteOrCancel(VoidCallback callback) {
            this.orCancel.Then(() => callback(), ex => callback());
        }

        public IPromise orCancel {
            get {
                if (this._secondaryCompleter == null) {
                    this._secondaryCompleter = new Promise();
                    if (this._completed != null) {
                        if (this._completed.Value) {
                            this._secondaryCompleter.Resolve();
                        }
                        else {
                            this._secondaryCompleter.Reject(new TickerCanceled());
                        }
                    }
                }

                return this._secondaryCompleter;
            }
        }
    }

    public class TickerCanceled : Exception {
        public TickerCanceled(Ticker ticker = null) {
            this.ticker = ticker;
        }

        public readonly Ticker ticker;

        public override string ToString() {
            if (this.ticker != null) {
                return "This ticker was canceled: " + this.ticker;
            }

            return "The ticker was canceled before the \"orCancel\" property was first used.";
        }
    }
}