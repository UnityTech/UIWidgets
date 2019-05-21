using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.async {
    public class UIWidgetsCoroutine {
        _WaitForSecondsProcessor _waitProcessor;
        _WaitForCoroutineProcessor _waitForCoroutine;
        _WaitForAsyncOPProcessor _waitForAsyncOPProcessor;

        readonly IEnumerator _routine;
        readonly Window _window;
        readonly IDisposable _unhook;
        readonly bool _isBackground;

        internal volatile object lastResult;
        internal volatile Exception lastError;
        internal bool isDone;

        readonly Promise<object> _promise = new Promise<object>(isSync: true);

        public IPromise<object> promise {
            get { return this._promise; }
        }

        internal UIWidgetsCoroutine(IEnumerator routine, Window window, bool isBackground = false) {
            D.assert(routine != null);
            D.assert(window != null);

            this._routine = routine;
            this._window = window;
            this._isBackground = isBackground;

            if (isBackground && BackgroundCallbacks.getInstance() != null) {
                this._unhook = BackgroundCallbacks.getInstance().addCallback(this._moveNext);
            }
            else {
                this._unhook = this._window.run(TimeSpan.Zero, this._moveNext, periodic: true);
                this._moveNext(true); // try to run the first enumeration in the current loop.
            }
        }

        void _moveNext() {
            this._moveNext(false);
        }

        void _moveNext(bool firstTime) {
            D.assert(!this.isDone);

            var lastError = this.lastError;
            if (lastError != null) {
                this._unhook.Dispose();

                this.isDone = true;
                this.lastResult = null;
                if (this._isBackground) {
                    this._window.runInMain(() => { this._promise.Reject(lastError); });
                }
                else {
                    this._promise.Reject(lastError);
                }

                return;
            }

            bool hasNext = true;
            try {
                if (firstTime) {
                    hasNext = this._routine.MoveNext();
                }
                if (hasNext) {
                    hasNext = this._processIEnumeratorRecursive(this._routine);
                }
            }
            catch (Exception ex) {
                this.stop(ex);
                return;
            }

            if (!hasNext && !this.isDone) {
                this._unhook.Dispose();

                this.isDone = true;
                D.assert(this.lastError == null);
                if (this._isBackground) {
                    this._window.runInMain(() => { this._promise.Resolve(this.lastResult); });
                }
                else {
                    this._promise.Resolve(this.lastResult);
                }
            }
        }

        bool _processIEnumeratorRecursive(IEnumerator child) {
            D.assert(child != null);

            if (child.Current is IEnumerator nestedEnumerator) {
                return this._processIEnumeratorRecursive(nestedEnumerator) || child.MoveNext();
            }

            if (child.Current is UIWidgetsCoroutine nestedCoroutine) {
                if (this._isBackground) {
                    throw new Exception("nestedCoroutine is not supported in Background Coroutine");
                }

                this._waitForCoroutine.set(nestedCoroutine);
                return this._waitForCoroutine.moveNext(child, this);
            }

            if (child.Current is UIWidgetsWaitForSeconds waitForSeconds) {
                if (this._isBackground) {
                    throw new Exception("waitForSeconds is not supported in Background Coroutine");
                }

                this._waitProcessor.set(waitForSeconds);
                return this._waitProcessor.moveNext(child);
            }

            if (child.Current is AsyncOperation waitForAsyncOP) {
                if (this._isBackground) {
                    throw new Exception("asyncOperation is not supported in Background Coroutine");
                }

                this._waitForAsyncOPProcessor.set(waitForAsyncOP);
                return this._waitForAsyncOPProcessor.moveNext(child);
            }

            this.lastResult = child.Current;
            return child.MoveNext();
        }

        public void stop() {
            this.stop(null);
        }

        internal void stop(Exception ex) {
            if (this.lastError == null) {
                this.lastError = ex ?? new CoroutineCanceledException();
            }
        }
    }

    struct _WaitForSecondsProcessor {
        UIWidgetsWaitForSeconds _current;
        float _targetTime;

        public void set(UIWidgetsWaitForSeconds yieldStatement) {
            if (this._current != yieldStatement) {
                this._current = yieldStatement;
                this._targetTime = Timer.timeSinceStartup + yieldStatement.waitTime;
            }
        }

        public bool moveNext(IEnumerator enumerator) {
            if (this._targetTime <= Timer.timeSinceStartup) {
                this._current = null;
                this._targetTime = 0;
                return enumerator.MoveNext();
            }

            return true;
        }
    }

    struct _WaitForCoroutineProcessor {
        UIWidgetsCoroutine _current;

        public void set(UIWidgetsCoroutine routine) {
            if (this._current != routine) {
                this._current = routine;
            }
        }

        public bool moveNext(IEnumerator enumerator, UIWidgetsCoroutine parent) {
            if (this._current.isDone) {
                var current = this._current;
                this._current = null;

                if (current.lastError != null) {
                    parent.stop(current.lastError);
                    return false;
                }

                parent.lastResult = current.lastResult;
                return enumerator.MoveNext();
            }

            return true;
        }
    }

    struct _WaitForAsyncOPProcessor {
        AsyncOperation _current;

        public void set(AsyncOperation operation) {
            if (this._current != operation) {
                this._current = operation;
            }
        }

        public bool moveNext(IEnumerator enumerator) {
            if (this._current.isDone) {
                this._current = null;
                return enumerator.MoveNext();
            }

            return true;
        }
    }

    public static class Coroutine {
        public static UIWidgetsCoroutine startCoroutine(this Window owner, IEnumerator routine) {
            return new UIWidgetsCoroutine(routine, owner);
        }

        public static UIWidgetsCoroutine startBackgroundCoroutine(this Window owner, IEnumerator routine) {
            return new UIWidgetsCoroutine(routine, owner, isBackground: true);
        }
    }

    public class CoroutineCanceledException : Exception {
    }

    public class UIWidgetsWaitForSeconds {
        public float waitTime { get; }

        public UIWidgetsWaitForSeconds(float time) {
            this.waitTime = time;
        }
    }

    class BackgroundCallbacks : IDisposable {
        static BackgroundCallbacks _instance;

        public static BackgroundCallbacks getInstance() {
#if UNITY_WEBGL
            return null;
#else
            if (_instance == null) {
                _instance = new BackgroundCallbacks(2);
            }

            return _instance;
#endif
        }

        readonly LinkedList<_CallbackNode> _callbackList;
        readonly ManualResetEvent _event;

        readonly Thread[] _threads;
        volatile bool _aborted = false;

        public BackgroundCallbacks(int threadCount = 1) {
            this._callbackList = new LinkedList<_CallbackNode>();
            this._event = new ManualResetEvent(false);

            this._threads = new Thread[threadCount];
            for (var i = 0; i < this._threads.Length; i++) {
                this._threads[i] = new Thread(this._threadLoop);
                this._threads[i].Start();
            }
        }

        public void Dispose() {
            foreach (var t in this._threads) {
                this._aborted = true;
                this._event.Set();
                t.Join();
            }

            this._callbackList.Clear();
        }

        void _threadLoop() {
            while (true) {
                if (this._aborted) {
                    break;
                }

                LinkedListNode<_CallbackNode> node;
                lock (this._callbackList) {
                    node = this._callbackList.First;
                    if (node != null) {
                        this._callbackList.Remove(node);
                    }
                }

                if (node == null) {
                    this._event.WaitOne();
                    this._event.Reset();
                    continue;
                }

                var callbackNode = node.Value;
                D.assert(!callbackNode.isDone);

                try {
                    callbackNode.callback();
                }
                catch (Exception ex) {
                    D.logError("Failed to execute callback in BackgroundCallbacks: ", ex);
                }

                if (!callbackNode.isDone) {
                    lock (this._callbackList) {
                        this._callbackList.AddLast(node);
                    }
                }
            }
        }

        public IDisposable addCallback(VoidCallback callback) {
            var node = new _CallbackNode {callback = callback};
            lock (this._callbackList) {
                this._callbackList.AddLast(node);
            }

            this._event.Set();

            return new _CallbackDisposable(node);
        }

        class _CallbackDisposable : IDisposable {
            readonly _CallbackNode _node;

            public _CallbackDisposable(_CallbackNode node) {
                this._node = node;
            }

            public void Dispose() {
                this._node.isDone = true;
            }
        }

        class _CallbackNode {
            public VoidCallback callback;
            public volatile bool isDone;
        }
    }
}