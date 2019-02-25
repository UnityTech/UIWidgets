using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.async {
    public class MicrotaskQueue {
        readonly Queue<Action> _queue = new Queue<Action>();

        public void scheduleMicrotask(Action action) {
            this._queue.Enqueue(action);
        }

        public void flushMicrotasks() {
            while (this._queue.isNotEmpty()) {
                var action = this._queue.Dequeue();
                try {
                    action();
                }
                catch (Exception ex) {
                    D.logError("Error to execute microtask: ", ex);
                }
            }
        }
    }
}