using System;
using System.Collections.Generic;
using System.Linq;
using RSG.Exceptions;
using RSG.Promises;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace RSG {
    /// <summary>
    /// Implements a non-generic C# promise, this is a promise that simply resolves without delivering a value.
    /// https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise
    /// </summary>
    public interface IPromise {
        /// <summary>
        /// ID of the promise, useful for debugging.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Set the name of the promise, useful for debugging.
        /// </summary>
        IPromise WithName(string name);

        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// onRejected is called on error.
        /// </summary>
        void Done(Action onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// Adds a default error handler.
        /// </summary>
        void Done(Action onResolved);

        /// <summary>
        /// Complete the promise. Adds a default error handler.
        /// </summary>
        void Done();

        /// <summary>
        /// Handle errors for the promise. 
        /// </summary>
        IPromise Catch(Action<Exception> onRejected);

        /// <summary>
        /// Add a resolved callback that chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved);

        /// <summary>
        /// Add a resolved callback that chains a non-value promise.
        /// </summary>
        IPromise Then(Func<IPromise> onResolved);

        /// <summary>
        /// Add a resolved callback.
        /// </summary>
        IPromise Then(Action onResolved);

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected);

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// </summary>
        IPromise Then(Action onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected, Action<float> onProgress);

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected, Action<float> onProgress);

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// </summary>
        IPromise Then(Action onResolved, Action<Exception> onRejected, Action<float> onProgress);

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise ThenAll(Func<IEnumerable<IPromise>> chain);

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Converts to a non-value promise.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary>
        /// Chain a sequence of operations using promises.
        /// Reutrn a collection of functions each of which starts an async operation and yields a promise.
        /// Each function will be called and each promise resolved in turn.
        /// The resulting promise is resolved after each promise is resolved in sequence.
        /// </summary>
        IPromise ThenSequence(Func<IEnumerable<Func<IPromise>>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// </summary>
        IPromise ThenRace(Func<IEnumerable<IPromise>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Converts to a value promise.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// </summary>
        IPromise<ConvertedT> ThenRace<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary> 
        /// Add a finally callback. 
        /// Finally callbacks will always be called, even if any preceding promise is rejected, or encounters an error.
        /// The returned promise will be resolved or rejected, as per the preceding promise.
        /// </summary> 
        IPromise Finally(Action onComplete);

        /// <summary>
        /// Add a callback that chains a non-value promise.
        /// ContinueWith callbacks will always be called, even if any preceding promise is rejected, or encounters an error.
        /// The state of the returning promise will be based on the new non-value promise, not the preceding (rejected or resolved) promise.
        /// </summary>
        IPromise ContinueWith(Func<IPromise> onResolved);

        /// <summary> 
        /// Add a callback that chains a value promise (optionally converting to a different value type).
        /// ContinueWith callbacks will always be called, even if any preceding promise is rejected, or encounters an error.
        /// The state of the returning promise will be based on the new value promise, not the preceding (rejected or resolved) promise.
        /// </summary> 
        IPromise<ConvertedT> ContinueWith<ConvertedT>(Func<IPromise<ConvertedT>> onComplete);

        /// <summary>
        /// Add a progress callback.
        /// Progress callbacks will be called whenever the promise owner reports progress towards the resolution
        /// of the promise.
        /// </summary>
        IPromise Progress(Action<float> onProgress);
    }

    /// <summary>
    /// Interface for a promise that can be rejected or resolved.
    /// </summary>
    public interface IPendingPromise : IRejectable {
        /// <summary>
        /// ID of the promise, useful for debugging.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Resolve the promise with a particular value.
        /// </summary>
        void Resolve();

        /// <summary>
        /// Report progress in a promise.
        /// </summary>
        void ReportProgress(float progress);
    }

    /// <summary>
    /// Used to list information of pending promises.
    /// </summary>
    public interface IPromiseInfo {
        /// <summary>
        /// Id of the promise.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Human-readable name for the promise.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// Arguments to the UnhandledError event.
    /// </summary>
    public class ExceptionEventArgs : EventArgs {
        internal ExceptionEventArgs(Exception exception) {
//            Argument.NotNull(() => exception);

            this.Exception = exception;
        }

        public Exception Exception { get; private set; }
    }

    /// <summary>
    /// Represents a handler invoked when the promise is rejected.
    /// </summary>
    public struct RejectHandler {
        /// <summary>
        /// Callback fn.
        /// </summary>
        public Action<Exception> callback;

        /// <summary>
        /// The promise that is rejected when there is an error while invoking the handler.
        /// </summary>
        public IRejectable rejectable;
    }

    public struct ProgressHandler {
        /// <summary>
        /// Callback fn.
        /// </summary>
        public Action<float> callback;

        /// <summary>
        /// The promise that is rejected when there is an error while invoking the handler.
        /// </summary>
        public IRejectable rejectable;
    }

    /// <summary>
    /// Implements a non-generic C# promise, this is a promise that simply resolves without delivering a value.
    /// https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise
    /// </summary>
    public class Promise : IPromise, IPendingPromise, IPromiseInfo {
        /// <summary>
        /// Set to true to enable tracking of promises.
        /// </summary>
        public static bool EnablePromiseTracking = false;

        /// <summary>
        /// Event raised for unhandled errors.
        /// For this to work you have to complete your promises with a call to Done().
        /// </summary>
        public static event EventHandler<ExceptionEventArgs> UnhandledException {
            add { unhandlerException += value; }
            remove { unhandlerException -= value; }
        }

        static EventHandler<ExceptionEventArgs> unhandlerException;

        /// <summary>
        /// Id for the next promise that is created.
        /// </summary>
        static int nextPromiseId;

        /// <summary>
        /// Information about pending promises.
        /// </summary>
        internal static readonly HashSet<IPromiseInfo> PendingPromises =
            new HashSet<IPromiseInfo>();

        /// <summary>
        /// Information about pending promises, useful for debugging.
        /// This is only populated when 'EnablePromiseTracking' is set to true.
        /// </summary>
        public static IEnumerable<IPromiseInfo> GetPendingPromises() {
            return PendingPromises;
        }

        /// <summary>
        /// The exception when the promise is rejected.
        /// </summary>
        Exception rejectionException;

        /// <summary>
        /// Error handlers.
        /// </summary>
        List<RejectHandler> rejectHandlers;

        /// <summary>
        /// Represents a handler invoked when the promise is resolved.
        /// </summary>
        public struct ResolveHandler {
            /// <summary>
            /// Callback fn.
            /// </summary>
            public Action callback;

            /// <summary>
            /// The promise that is rejected when there is an error while invoking the handler.
            /// </summary>
            public IRejectable rejectable;
        }

        /// <summary>
        /// Completed handlers that accept no value.
        /// </summary>
        List<ResolveHandler> resolveHandlers;

        /// <summary>
        /// Progress handlers.
        /// </summary>
        List<ProgressHandler> progressHandlers;

        /// <summary>
        /// ID of the promise, useful for debugging.
        /// </summary>
        public int Id {
            get { return this.id; }
        }

        readonly int id;

        /// <summary>
        /// Name of the promise, when set, useful for debugging.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Tracks the current state of the promise.
        /// </summary>
        public PromiseState CurState { get; private set; }

        public bool IsSync { get; }

        public Promise(bool isSync = false) {
            this.IsSync = isSync;
            this.CurState = PromiseState.Pending;
            this.id = NextId();
            if (EnablePromiseTracking) {
                PendingPromises.Add(this);
            }
        }

        public Promise(Action<Action, Action<Exception>> resolver, bool isSync = false) {
            this.IsSync = isSync;
            this.CurState = PromiseState.Pending;
            this.id = NextId();
            if (EnablePromiseTracking) {
                PendingPromises.Add(this);
            }

            try {
                resolver(this.Resolve, this.Reject);
            }
            catch (Exception ex) {
                this.Reject(ex);
            }
        }

        /// <summary>
        /// Increments the ID counter and gives us the ID for the next promise.
        /// </summary>
        internal static int NextId() {
            return ++nextPromiseId;
        }

        /// <summary>
        /// Add a rejection handler for this promise.
        /// </summary>
        void AddRejectHandler(Action<Exception> onRejected, IRejectable rejectable) {
            if (this.rejectHandlers == null) {
                this.rejectHandlers = new List<RejectHandler>();
            }

            this.rejectHandlers.Add(new RejectHandler {
                callback = onRejected,
                rejectable = rejectable
            });
        }

        /// <summary>
        /// Add a resolve handler for this promise.
        /// </summary>
        void AddResolveHandler(Action onResolved, IRejectable rejectable) {
            if (this.resolveHandlers == null) {
                this.resolveHandlers = new List<ResolveHandler>();
            }

            this.resolveHandlers.Add(new ResolveHandler {
                callback = onResolved,
                rejectable = rejectable
            });
        }

        /// <summary>
        /// Add a progress handler for this promise.
        /// </summary>
        void AddProgressHandler(Action<float> onProgress, IRejectable rejectable) {
            if (this.progressHandlers == null) {
                this.progressHandlers = new List<ProgressHandler>();
            }

            this.progressHandlers.Add(new ProgressHandler {callback = onProgress, rejectable = rejectable});
        }

        /// <summary>
        /// Invoke a single error handler.
        /// </summary>
        void InvokeRejectHandler(Action<Exception> callback, IRejectable rejectable, Exception value) {
//            Argument.NotNull(() => callback);
//            Argument.NotNull(() => rejectable);

            try {
                callback(value);
            }
            catch (Exception ex) {
                rejectable.Reject(ex);
            }
        }

        /// <summary>
        /// Invoke a single resolve handler.
        /// </summary>
        void InvokeResolveHandler(Action callback, IRejectable rejectable) {
//            Argument.NotNull(() => callback);
//            Argument.NotNull(() => rejectable);

            try {
                callback();
            }
            catch (Exception ex) {
                rejectable.Reject(ex);
            }
        }

        /// <summary>
        /// Invoke a single progress handler.
        /// </summary>
        void InvokeProgressHandler(Action<float> callback, IRejectable rejectable, float progress) {
//            Argument.NotNull(() => callback);
//            Argument.NotNull(() => rejectable);

            try {
                callback(progress);
            }
            catch (Exception ex) {
                rejectable.Reject(ex);
            }
        }

        /// <summary>
        /// Helper function clear out all handlers after resolution or rejection.
        /// </summary>
        void ClearHandlers() {
            this.rejectHandlers = null;
            this.resolveHandlers = null;
            this.progressHandlers = null;
        }

        /// <summary>
        /// Invoke all reject handlers.
        /// </summary>
        void InvokeRejectHandlers(Exception ex) {
//            Argument.NotNull(() => ex);

            if (this.rejectHandlers != null) {
                this.rejectHandlers.Each(handler => this.InvokeRejectHandler(handler.callback, handler.rejectable, ex));
            }
            else {
                PropagateUnhandledException(this, ex);
            }

            this.ClearHandlers();
        }

        /// <summary>
        /// Invoke all resolve handlers.
        /// </summary>
        void InvokeResolveHandlers() {
            if (this.resolveHandlers != null) {
                this.resolveHandlers.Each(handler => this.InvokeResolveHandler(handler.callback, handler.rejectable));
            }

            this.ClearHandlers();
        }

        /// <summary>
        /// Invoke all progress handlers.
        /// </summary>
        void InvokeProgressHandlers(float progress) {
            if (this.progressHandlers != null) {
                this.progressHandlers.Each(handler =>
                    this.InvokeProgressHandler(handler.callback, handler.rejectable, progress));
            }
        }

        /// <summary>
        /// Reject the promise with an exception.
        /// </summary>
        public void Reject(Exception ex) {
            if (this.IsSync) {
                this.RejectSync(ex);
            }
            else {
                Window.instance.run(() => this.RejectSync(ex));
            }
        }

        public void RejectSync(Exception ex) {
//            Argument.NotNull(() => ex);

            if (this.CurState != PromiseState.Pending) {
                throw new PromiseStateException(
                    "Attempt to reject a promise that is already in state: "
                    + this.CurState
                    + ", a promise can only be rejected when it is still in state: "
                    + PromiseState.Pending
                );
            }

            this.rejectionException = ex;
            this.CurState = PromiseState.Rejected;

            if (EnablePromiseTracking) {
                PendingPromises.Remove(this);
            }

            this.InvokeRejectHandlers(ex);
        }


        /// <summary>
        /// Resolve the promise with a particular value.
        /// </summary>
        public void Resolve() {
            if (this.IsSync) {
                this.ResolveSync();
            }
            else {
                Window.instance.run(() => this.ResolveSync());
            }
        }

        public void ResolveSync() {
            if (this.CurState != PromiseState.Pending) {
                throw new PromiseStateException(
                    "Attempt to resolve a promise that is already in state: "
                    + this.CurState
                    + ", a promise can only be resolved when it is still in state: "
                    + PromiseState.Pending
                );
            }

            this.CurState = PromiseState.Resolved;

            if (EnablePromiseTracking) {
                PendingPromises.Remove(this);
            }

            this.InvokeResolveHandlers();
        }


        /// <summary>
        /// Report progress on the promise.
        /// </summary>
        public void ReportProgress(float progress) {
            if (this.CurState != PromiseState.Pending) {
                throw new PromiseStateException(
                    "Attempt to report progress on a promise that is already in state: "
                    + this.CurState + ", a promise can only report progress when it is still in state: "
                    + PromiseState.Pending
                );
            }

            this.InvokeProgressHandlers(progress);
        }


        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// onRejected is called on error.
        /// </summary>
        public void Done(Action onResolved, Action<Exception> onRejected) {
            this.Then(onResolved, onRejected)
                .Catch(ex =>
                    PropagateUnhandledException(this, ex)
                );
        }

        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// Adds a default error handler.
        /// </summary>
        public void Done(Action onResolved) {
            this.Then(onResolved)
                .Catch(ex =>
                    PropagateUnhandledException(this, ex)
                );
        }

        /// <summary>
        /// Complete the promise. Adds a defualt error handler.
        /// </summary>
        public void Done() {
            this.Catch(ex => PropagateUnhandledException(this, ex));
        }

        /// <summary>
        /// Set the name of the promise, useful for debugging.
        /// </summary>
        public IPromise WithName(string name) {
            this.Name = name;
            return this;
        }

        /// <summary>
        /// Handle errors for the promise. 
        /// </summary>
        public IPromise Catch(Action<Exception> onRejected) {
//            Argument.NotNull(() => onRejected);

            var resultPromise = new Promise(isSync: true);
            resultPromise.WithName(this.Name);

            Action resolveHandler = () => resultPromise.Resolve();

            Action<Exception> rejectHandler = ex => {
                try {
                    onRejected(ex);
                    resultPromise.Resolve();
                }
                catch (Exception callbackException) {
                    resultPromise.Reject(callbackException);
                }
            };

            this.ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            this.ProgressHandlers(resultPromise, v => resultPromise.ReportProgress(v));

            return resultPromise;
        }

        /// <summary>
        /// Add a resolved callback that chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved) {
            return this.Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback that chains a non-value promise.
        /// </summary>
        public IPromise Then(Func<IPromise> onResolved) {
            return this.Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback.
        /// </summary>
        public IPromise Then(Action onResolved) {
            return this.Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected) {
            return this.Then(onResolved, onRejected, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        public IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected) {
            return this.Then(onResolved, onRejected, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// </summary>
        public IPromise Then(Action onResolved, Action<Exception> onRejected) {
            return this.Then(onResolved, onRejected, null);
        }

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(
            Func<IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected,
            Action<float> onProgress) {
            // This version of the function must supply an onResolved.
            // Otherwise there is now way to get the converted value to pass to the resulting promise.
//            Argument.NotNull(() => onResolved);

            var resultPromise = new Promise<ConvertedT>(isSync: true);
            resultPromise.WithName(this.Name);

            Action resolveHandler = () => {
                onResolved()
                    .Progress(progress => resultPromise.ReportProgress(progress))
                    .Then(
                        // Should not be necessary to specify the arg type on the next line, but Unity (mono) has an internal compiler error otherwise.
                        chainedValue => resultPromise.Resolve(chainedValue),
                        ex => resultPromise.Reject(ex)
                    );
            };

            Action<Exception> rejectHandler = ex => {
                if (onRejected == null) {
                    resultPromise.Reject(ex);
                    return;
                }

                try {
                    onRejected(ex)
                        .Then(
                            chainedValue => resultPromise.Resolve(chainedValue),
                            callbackEx => resultPromise.Reject(callbackEx)
                        );
                }
                catch (Exception callbackEx) {
                    resultPromise.Reject(callbackEx);
                }
            };

            this.ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            if (onProgress != null) {
                this.ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        public IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected, Action<float> onProgress) {
            var resultPromise = new Promise(isSync: true);
            resultPromise.WithName(this.Name);

            Action resolveHandler = () => {
                if (onResolved != null) {
                    onResolved()
                        .Progress(progress => resultPromise.ReportProgress(progress))
                        .Then(
                            () => resultPromise.Resolve(),
                            ex => resultPromise.Reject(ex)
                        );
                }
                else {
                    resultPromise.Resolve();
                }
            };

            Action<Exception> rejectHandler = ex => {
                if (onRejected != null) {
                    onRejected(ex);
                }

                resultPromise.Reject(ex);
            };

            this.ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            if (onProgress != null) {
                this.ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// </summary>
        public IPromise Then(Action onResolved, Action<Exception> onRejected, Action<float> onProgress) {
            var resultPromise = new Promise(isSync: true);
            resultPromise.WithName(this.Name);

            Action resolveHandler = () => {
                if (onResolved != null) {
                    onResolved();
                }

                resultPromise.Resolve();
            };

            Action<Exception> rejectHandler = ex => {
                if (onRejected != null) {
                    onRejected(ex);
                    resultPromise.Resolve();
                    return;
                }

                resultPromise.Reject(ex);
            };

            this.ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            if (onProgress != null) {
                this.ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        /// <summary>
        /// Helper function to invoke or register resolve/reject handlers.
        /// </summary>
        void ActionHandlers(IRejectable resultPromise, Action resolveHandler, Action<Exception> rejectHandler) {
            if (this.CurState == PromiseState.Resolved) {
                this.InvokeResolveHandler(resolveHandler, resultPromise);
            }
            else if (this.CurState == PromiseState.Rejected) {
                this.InvokeRejectHandler(rejectHandler, resultPromise, this.rejectionException);
            }
            else {
                this.AddResolveHandler(resolveHandler, resultPromise);
                this.AddRejectHandler(rejectHandler, resultPromise);
            }
        }

        /// <summary>
        /// Helper function to invoke or register progress handlers.
        /// </summary>
        void ProgressHandlers(IRejectable resultPromise, Action<float> progressHandler) {
            if (this.CurState == PromiseState.Pending) {
                this.AddProgressHandler(progressHandler, resultPromise);
            }
        }

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        public IPromise ThenAll(Func<IEnumerable<IPromise>> chain) {
            return this.Then(() => All(chain()));
        }

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Converts to a non-value promise.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        public IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain) {
            return this.Then(() => Promise<ConvertedT>.All(chain()));
        }

        /// <summary>
        /// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        /// Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise All(params IPromise[] promises) {
            return
                All(
                    (IEnumerable<IPromise>) promises); // Cast is required to force use of the other All function.
        }

        /// <summary>
        /// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        /// Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise All(IEnumerable<IPromise> promises) {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0) {
                return Resolved();
            }

            var remainingCount = promisesArray.Length;
            var resultPromise = new Promise(isSync: true);
            resultPromise.WithName("All");
            var progress = new float[remainingCount];

            promisesArray.Each((promise, index) => {
                promise
                    .Progress(v => {
                        progress[index] = v;
                        if (resultPromise.CurState == PromiseState.Pending) {
                            resultPromise.ReportProgress(progress.Average());
                        }
                    })
                    .Then(() => {
                        progress[index] = 1f;

                        --remainingCount;
                        if (remainingCount <= 0 && resultPromise.CurState == PromiseState.Pending) {
                            // This will never happen if any of the promises errorred.
                            resultPromise.Resolve();
                        }
                    })
                    .Catch(ex => {
                        if (resultPromise.CurState == PromiseState.Pending) {
                            // If a promise errorred and the result promise is still pending, reject it.
                            resultPromise.Reject(ex);
                        }
                    })
                    .Done();
            });

            return resultPromise;
        }

        /// <summary>
        /// Chain a sequence of operations using promises.
        /// Reutrn a collection of functions each of which starts an async operation and yields a promise.
        /// Each function will be called and each promise resolved in turn.
        /// The resulting promise is resolved after each promise is resolved in sequence.
        /// </summary>
        public IPromise ThenSequence(Func<IEnumerable<Func<IPromise>>> chain) {
            return this.Then(() => Sequence(chain()));
        }

        /// <summary>
        /// Chain a number of operations using promises.
        /// Takes a number of functions each of which starts an async operation and yields a promise.
        /// </summary>
        public static IPromise Sequence(params Func<IPromise>[] fns) {
            return Sequence((IEnumerable<Func<IPromise>>) fns);
        }

        /// <summary>
        /// Chain a sequence of operations using promises.
        /// Takes a collection of functions each of which starts an async operation and yields a promise.
        /// </summary>
        public static IPromise Sequence(IEnumerable<Func<IPromise>> fns) {
            var promise = new Promise(isSync: true);

            int count = 0;

            fns.Aggregate(
                    Resolved(),
                    (prevPromise, fn) => {
                        int itemSequence = count;
                        ++count;

                        return prevPromise
                                .Then(() => {
                                    var sliceLength = 1f / count;
                                    promise.ReportProgress(sliceLength * itemSequence);
                                    return fn();
                                })
                                .Progress(v => {
                                    var sliceLength = 1f / count;
                                    promise.ReportProgress(sliceLength * (v + itemSequence));
                                })
                            ;
                    }
                )
                .Then(() => promise.Resolve())
                .Catch(promise.Reject);

            return promise;
        }

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// </summary>
        public IPromise ThenRace(Func<IEnumerable<IPromise>> chain) {
            return this.Then(() => Race(chain()));
        }

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Converts to a value promise.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// </summary>
        public IPromise<ConvertedT> ThenRace<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain) {
            return this.Then(() => Promise<ConvertedT>.Race(chain()));
        }

        /// <summary>
        /// Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        /// Returns the value from the first promise that has resolved.
        /// </summary>
        public static IPromise Race(params IPromise[] promises) {
            return
                Race((IEnumerable<IPromise>) promises); // Cast is required to force use of the other function.
        }

        /// <summary>
        /// Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        /// Returns the value from the first promise that has resolved.
        /// </summary>
        public static IPromise Race(IEnumerable<IPromise> promises) {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0) {
                throw new InvalidOperationException("At least 1 input promise must be provided for Race");
            }

            var resultPromise = new Promise(isSync: true);
            resultPromise.WithName("Race");

            var progress = new float[promisesArray.Length];

            promisesArray.Each((promise, index) => {
                promise
                    .Progress(v => {
                        progress[index] = v;
                        resultPromise.ReportProgress(progress.Max());
                    })
                    .Catch(ex => {
                        if (resultPromise.CurState == PromiseState.Pending) {
                            // If a promise errorred and the result promise is still pending, reject it.
                            resultPromise.Reject(ex);
                        }
                    })
                    .Then(() => {
                        if (resultPromise.CurState == PromiseState.Pending) {
                            resultPromise.Resolve();
                        }
                    })
                    .Done();
            });

            return resultPromise;
        }

        /// <summary>
        /// Convert a simple value directly into a resolved promise.
        /// </summary>
        public static IPromise Resolved() {
            var promise = new Promise(isSync: true);
            promise.Resolve();
            return promise;
        }

        /// <summary>
        /// Convert an exception directly into a rejected promise.
        /// </summary>
        public static IPromise Rejected(Exception ex) {
//            Argument.NotNull(() => ex);

            var promise = new Promise(isSync: true);
            promise.Reject(ex);
            return promise;
        }
        
        public static IPromise Delayed(TimeSpan duration) {
            var promise = new Promise(isSync: true);
            Window.instance.run(duration, () => { promise.Resolve(); });
            
            return promise;
        }

        public IPromise Finally(Action onComplete) {
            var promise = new Promise(isSync: true);
            promise.WithName(this.Name);

            this.Then(() => promise.Resolve());
            this.Catch(e => {
                try {
                    onComplete();
                    promise.Reject(e);
                }
                catch (Exception ne) {
                    promise.Reject(ne);
                }
            });

            return promise.Then(onComplete);
        }

        public IPromise ContinueWith(Func<IPromise> onComplete) {
            var promise = new Promise(isSync: true);
            promise.WithName(this.Name);

            this.Then(() => promise.Resolve());
            this.Catch(e => promise.Resolve());

            return promise.Then(onComplete);
        }

        public IPromise<ConvertedT> ContinueWith<ConvertedT>(Func<IPromise<ConvertedT>> onComplete) {
            var promise = new Promise(isSync: true);
            promise.WithName(this.Name);

            this.Then(() => promise.Resolve());
            this.Catch(e => promise.Resolve());

            return promise.Then(onComplete);
        }

        public IPromise Progress(Action<float> onProgress) {
            if (onProgress != null) {
                this.ProgressHandlers(this, onProgress);
            }

            return this;
        }

        /// <summary>
        /// Raises the UnhandledException event.
        /// </summary>
        internal static void PropagateUnhandledException(object sender, Exception ex) {
            if (unhandlerException != null) {
                unhandlerException(sender, new ExceptionEventArgs(ex));
            }
            else {
                Debug.LogWarning("Unhandled Exception from " + sender + ": " + ex);
            }
        }
    }
}