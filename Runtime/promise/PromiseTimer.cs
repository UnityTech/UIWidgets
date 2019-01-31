using System;
using System.Collections.Generic;

namespace RSG {
    public class PromiseCancelledException : Exception {
        /// <summary>
        /// Just create the exception
        /// </summary>
        public PromiseCancelledException() {
        }

        /// <summary>
        /// Create the exception with description
        /// </summary>
        /// <param name="message">Exception description</param>
        public PromiseCancelledException(string message) : base(message) {
        }
    }

    /// <summary>
    /// A class that wraps a pending promise with it's predicate and time data
    /// </summary>
    class PredicateWait {
        /// <summary>
        /// Predicate for resolving the promise
        /// </summary>
        public Func<TimeData, bool> predicate;

        /// <summary>
        /// The time the promise was started
        /// </summary>
        public float timeStarted;

        /// <summary>
        /// The pending promise which is an interface for a promise that can be rejected or resolved.
        /// </summary>
        public IPendingPromise pendingPromise;

        /// <summary>
        /// The time data specific to this pending promise. Includes elapsed time and delta time.
        /// </summary>
        public TimeData timeData;

        /// <summary>
        /// The frame the promise was started
        /// </summary>
        public int frameStarted;
    }

    /// <summary>
    /// Time data specific to a particular pending promise.
    /// </summary>
    public struct TimeData {
        /// <summary>
        /// The amount of time that has elapsed since the pending promise started running
        /// </summary>
        public float elapsedTime;

        /// <summary>
        /// The amount of time since the last time the pending promise was updated.
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// The amount of times that update has been called since the pending promise started running
        /// </summary>
        public int elapsedUpdates;
    }

    public interface IPromiseTimer {
        /// <summary>
        /// Resolve the returned promise once the time has elapsed
        /// </summary>
        IPromise WaitFor(float seconds);

        /// <summary>
        /// Resolve the returned promise once the predicate evaluates to true
        /// </summary>
        IPromise WaitUntil(Func<TimeData, bool> predicate);

        /// <summary>
        /// Resolve the returned promise once the predicate evaluates to false
        /// </summary>
        IPromise WaitWhile(Func<TimeData, bool> predicate);

        /// <summary>
        /// Update all pending promises. Must be called for the promises to progress and resolve at all.
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Cancel a waiting promise and reject it immediately.
        /// </summary>
        bool Cancel(IPromise promise);
    }

    public class PromiseTimer : IPromiseTimer {
        /// <summary>
        /// The current running total for time that this PromiseTimer has run for
        /// </summary>
        float curTime;

        /// <summary>
        /// The current running total for the amount of frames the PromiseTimer has run for
        /// </summary>
        int curFrame;

        /// <summary>
        /// Currently pending promises
        /// </summary>
        readonly LinkedList<PredicateWait> waiting = new LinkedList<PredicateWait>();

        /// <summary>
        /// Resolve the returned promise once the time has elapsed
        /// </summary>
        public IPromise WaitFor(float seconds) {
            return this.WaitUntil(t => t.elapsedTime >= seconds);
        }

        /// <summary>
        /// Resolve the returned promise once the predicate evaluates to false
        /// </summary>
        public IPromise WaitWhile(Func<TimeData, bool> predicate) {
            return this.WaitUntil(t => !predicate(t));
        }

        /// <summary>
        /// Resolve the returned promise once the predicate evalutes to true
        /// </summary>
        public IPromise WaitUntil(Func<TimeData, bool> predicate) {
            var promise = new Promise();

            var wait = new PredicateWait() {
                timeStarted = this.curTime,
                pendingPromise = promise,
                timeData = new TimeData(),
                predicate = predicate,
                frameStarted = this.curFrame
            };

            this.waiting.AddLast(wait);

            return promise;
        }

        public bool Cancel(IPromise promise) {
            var node = this.FindInWaiting(promise);

            if (node == null) {
                return false;
            }

            node.Value.pendingPromise.Reject(new PromiseCancelledException("Promise was cancelled by user."));
            this.waiting.Remove(node);

            return true;
        }

        LinkedListNode<PredicateWait> FindInWaiting(IPromise promise) {
            for (var node = this.waiting.First; node != null; node = node.Next) {
                if (node.Value.pendingPromise.Id.Equals(promise.Id)) {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Update all pending promises. Must be called for the promises to progress and resolve at all.
        /// </summary>
        public void Update(float deltaTime) {
            this.curTime += deltaTime;
            this.curFrame += 1;

            var node = this.waiting.First;
            while (node != null) {
                var wait = node.Value;

                var newElapsedTime = this.curTime - wait.timeStarted;
                wait.timeData.deltaTime = newElapsedTime - wait.timeData.elapsedTime;
                wait.timeData.elapsedTime = newElapsedTime;
                var newElapsedUpdates = this.curFrame - wait.frameStarted;
                wait.timeData.elapsedUpdates = newElapsedUpdates;

                bool result;
                try {
                    result = wait.predicate(wait.timeData);
                }
                catch (Exception ex) {
                    wait.pendingPromise.Reject(ex);

                    node = this.RemoveNode(node);
                    continue;
                }

                if (result) {
                    wait.pendingPromise.Resolve();

                    node = this.RemoveNode(node);
                }
                else {
                    node = node.Next;
                }
            }
        }

        /// <summary>
        /// Removes the provided node and returns the next node in the list.
        /// </summary>
        LinkedListNode<PredicateWait> RemoveNode(LinkedListNode<PredicateWait> node) {
            var currentNode = node;
            node = node.Next;

            this.waiting.Remove(currentNode);

            return node;
        }
    }
}