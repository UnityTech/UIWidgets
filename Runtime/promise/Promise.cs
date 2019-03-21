using System;
using System.Collections.Generic;
using System.Linq;
using RSG.Exceptions;
using RSG.Promises;
using Unity.UIWidgets.ui;

namespace RSG {
    /// <summary>
    /// Implements a C# promise.
    /// https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise
    /// </summary>
    public interface IPromise<PromisedT> {
        /// <summary>
        /// Gets the id of the promise, useful for referencing the promise during runtime.
        /// </summary>
        int Id { get; }

        bool isCompleted { get; }

        /// <summary>
        /// Set the name of the promise, useful for debugging.
        /// </summary>
        IPromise<PromisedT> WithName(string name);

        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// onRejected is called on error.
        /// </summary>
        void Done(Action<PromisedT> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// Adds a default error handler.
        /// </summary>
        void Done(Action<PromisedT> onResolved);

        /// <summary>
        /// Complete the promise. Adds a default error handler.
        /// </summary>
        void Done();

        /// <summary>
        /// Handle errors for the promise. 
        /// </summary>
        IPromise Catch(Action<Exception> onRejected);

        /// <summary>
        /// Handle errors for the promise. 
        /// </summary>
        IPromise<PromisedT> Catch(Func<Exception, PromisedT> onRejected);

        /// <summary>
        /// Add a resolved callback that chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, IPromise<ConvertedT>> onResolved);

        /// <summary>
        /// Add a resolved callback that chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved);

        /// <summary>
        /// Add a resolved callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved);

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected
        );

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected,
            Action<float> onProgress
        );

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected, Action<float> onProgress);

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected, Action<float> onProgress);

        /// <summary>
        /// Return a new promise with a different value.
        /// May also change the type of the value.
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, ConvertedT> transform);

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Returns a promise for a collection of the resolved results.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Converts to a non-value promise.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise ThenAll(Func<PromisedT, IEnumerable<IPromise>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// </summary>
        IPromise<ConvertedT> ThenRace<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Converts to a non-value promise.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// </summary>
        IPromise ThenRace(Func<PromisedT, IEnumerable<IPromise>> chain);

        /// <summary> 
        /// Add a finally callback. 
        /// Finally callbacks will always be called, even if any preceding promise is rejected, or encounters an error.
        /// The returned promise will be resolved or rejected, as per the preceding promise.
        /// </summary> 
        IPromise<PromisedT> Finally(Action onComplete);

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
        IPromise<PromisedT> Progress(Action<float> onProgress);
    }

    /// <summary>
    /// Interface for a promise that can be rejected.
    /// </summary>
    public interface IRejectable {
        /// <summary>
        /// Reject the promise with an exception.
        /// </summary>
        void Reject(Exception ex);
    }

    /// <summary>
    /// Interface for a promise that can be rejected or resolved.
    /// </summary>
    public interface IPendingPromise<PromisedT> : IRejectable {
        /// <summary>
        /// ID of the promise, useful for debugging.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Resolve the promise with a particular value.
        /// </summary>
        void Resolve(PromisedT value);

        /// <summary>
        /// Report progress in a promise.
        /// </summary>
        void ReportProgress(float progress);
    }

    /// <summary>
    /// Specifies the state of a promise.
    /// </summary>
    public enum PromiseState {
        Pending, // The promise is in-flight.
        Rejected, // The promise has been rejected.
        Resolved // The promise has been resolved.
    };

    /// <summary>
    /// Implements a C# promise.
    /// https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise
    /// </summary>
    public class Promise<PromisedT> : IPromise<PromisedT>, IPendingPromise<PromisedT>, IPromiseInfo {
        /// <summary>
        /// The exception when the promise is rejected.
        /// </summary>
        Exception rejectionException;

        /// <summary>
        /// The value when the promises is resolved.
        /// </summary>
        PromisedT resolveValue;

        /// <summary>
        /// Error handler.
        /// </summary>
        List<RejectHandler> rejectHandlers;

        /// <summary>
        /// Progress handlers.
        /// </summary>
        List<ProgressHandler> progressHandlers;

        /// <summary>
        /// Completed handlers that accept a value.
        /// </summary>
        List<Action<PromisedT>> resolveCallbacks;

        List<IRejectable> resolveRejectables;

        /// <summary>
        /// ID of the promise, useful for debugging.
        /// </summary>
        public int Id {
            get { return this.id; }
        }

        public bool isCompleted {
            get { return this.CurState != PromiseState.Pending; }
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
            this.id = Promise.NextId();

            if (Promise.EnablePromiseTracking) {
                Promise.PendingPromises.Add(this);
            }
        }

        public Promise(Action<Action<PromisedT>, Action<Exception>> resolver, bool isSync = false) {
            this.IsSync = isSync;
            this.CurState = PromiseState.Pending;
            this.id = Promise.NextId();

            if (Promise.EnablePromiseTracking) {
                Promise.PendingPromises.Add(this);
            }

            try {
                resolver(this.Resolve, this.Reject);
            }
            catch (Exception ex) {
                this.Reject(ex);
            }
        }

        /// <summary>
        /// Add a rejection handler for this promise.
        /// </summary>
        void AddRejectHandler(Action<Exception> onRejected, IRejectable rejectable) {
            if (this.rejectHandlers == null) {
                this.rejectHandlers = new List<RejectHandler>();
            }

            this.rejectHandlers.Add(new RejectHandler {callback = onRejected, rejectable = rejectable});
        }

        /// <summary>
        /// Add a resolve handler for this promise.
        /// </summary>
        void AddResolveHandler(Action<PromisedT> onResolved, IRejectable rejectable) {
            if (this.resolveCallbacks == null) {
                this.resolveCallbacks = new List<Action<PromisedT>>();
            }

            if (this.resolveRejectables == null) {
                this.resolveRejectables = new List<IRejectable>();
            }

            this.resolveCallbacks.Add(onResolved);
            this.resolveRejectables.Add(rejectable);
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
        /// Invoke a single handler.
        /// </summary>
        void InvokeHandler<T>(Action<T> callback, IRejectable rejectable, T value) {
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
        /// Helper function clear out all handlers after resolution or rejection.
        /// </summary>
        void ClearHandlers() {
            this.rejectHandlers = null;
            this.resolveCallbacks = null;
            this.resolveRejectables = null;
            this.progressHandlers = null;
        }

        /// <summary>
        /// Invoke all reject handlers.
        /// </summary>
        void InvokeRejectHandlers(Exception ex) {
//            Argument.NotNull(() => ex);

            if (this.rejectHandlers != null) {
                this.rejectHandlers.Each(handler => this.InvokeHandler(handler.callback, handler.rejectable, ex));
            }
            else {
                Promise.PropagateUnhandledException(this, ex);
            }

            this.ClearHandlers();
        }

        /// <summary>
        /// Invoke all resolve handlers.
        /// </summary>
        void InvokeResolveHandlers(PromisedT value) {
            if (this.resolveCallbacks != null) {
                for (int i = 0, maxI = this.resolveCallbacks.Count; i < maxI; i++) {
                    this.InvokeHandler(this.resolveCallbacks[i], this.resolveRejectables[i], value);
                }
            }

            this.ClearHandlers();
        }

        /// <summary>
        /// Invoke all progress handlers.
        /// </summary>
        void InvokeProgressHandlers(float progress) {
            if (this.progressHandlers != null) {
                this.progressHandlers.Each(
                    handler => this.InvokeHandler(handler.callback, handler.rejectable, progress));
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

            if (Promise.EnablePromiseTracking) {
                Promise.PendingPromises.Remove(this);
            }

            this.InvokeRejectHandlers(ex);
        }

        /// <summary>
        /// Resolve the promise with a particular value.
        /// </summary>
        public void Resolve(PromisedT value) {
            if (this.IsSync) {
                this.ResolveSync(value);
            }
            else {
                Window.instance.run(() => this.ResolveSync(value));
            }
        }

        public void ResolveSync(PromisedT value) {
            if (this.CurState != PromiseState.Pending) {
                throw new PromiseStateException(
                    "Attempt to resolve a promise that is already in state: "
                    + this.CurState
                    + ", a promise can only be resolved when it is still in state: "
                    + PromiseState.Pending
                );
            }

            this.resolveValue = value;
            this.CurState = PromiseState.Resolved;

            if (Promise.EnablePromiseTracking) {
                Promise.PendingPromises.Remove(this);
            }

            this.InvokeResolveHandlers(value);
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
        public void Done(Action<PromisedT> onResolved, Action<Exception> onRejected) {
            this.Then(onResolved, onRejected)
                .Catch(ex =>
                    Promise.PropagateUnhandledException(this, ex)
                );
        }

        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// Adds a default error handler.
        /// </summary>
        public void Done(Action<PromisedT> onResolved) {
            this.Then(onResolved)
                .Catch(ex =>
                    Promise.PropagateUnhandledException(this, ex)
                );
        }

        /// <summary>
        /// Complete the promise. Adds a default error handler.
        /// </summary>
        public void Done() {
            this.Catch(ex =>
                Promise.PropagateUnhandledException(this, ex)
            );
        }

        /// <summary>
        /// Set the name of the promise, useful for debugging.
        /// </summary>
        public IPromise<PromisedT> WithName(string name) {
            this.Name = name;
            return this;
        }

        /// <summary>
        /// Handle errors for the promise. 
        /// </summary>
        public IPromise Catch(Action<Exception> onRejected) {
            var resultPromise = new Promise(isSync: true);
            resultPromise.WithName(this.Name);

            Action<PromisedT> resolveHandler = _ => resultPromise.Resolve();

            Action<Exception> rejectHandler = ex => {
                try {
                    onRejected(ex);
                    resultPromise.Resolve();
                }
                catch (Exception cbEx) {
                    resultPromise.Reject(cbEx);
                }
            };

            this.ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            this.ProgressHandlers(resultPromise, v => resultPromise.ReportProgress(v));

            return resultPromise;
        }

        /// <summary>
        /// Handle errors for the promise.
        /// </summary>
        public IPromise<PromisedT> Catch(Func<Exception, PromisedT> onRejected) {
            var resultPromise = new Promise<PromisedT>(isSync: true);
            resultPromise.WithName(this.Name);

            Action<PromisedT> resolveHandler = v => resultPromise.Resolve(v);

            Action<Exception> rejectHandler = ex => {
                try {
                    resultPromise.Resolve(onRejected(ex));
                }
                catch (Exception cbEx) {
                    resultPromise.Reject(cbEx);
                }
            };

            this.ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            this.ProgressHandlers(resultPromise, v => resultPromise.ReportProgress(v));

            return resultPromise;
        }

        /// <summary>
        /// Add a resolved callback that chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, IPromise<ConvertedT>> onResolved) {
            return this.Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback that chains a non-value promise.
        /// </summary>
        public IPromise Then(Func<PromisedT, IPromise> onResolved) {
            return this.Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback.
        /// </summary>
        public IPromise Then(Action<PromisedT> onResolved) {
            return this.Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected
        ) {
            return this.Then(onResolved, onRejected, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        public IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected) {
            return this.Then(onResolved, onRejected, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// </summary>
        public IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected) {
            return this.Then(onResolved, onRejected, null);
        }


        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected,
            Action<float> onProgress
        ) {
            // This version of the function must supply an onResolved.
            // Otherwise there is now way to get the converted value to pass to the resulting promise.
//            Argument.NotNull(() => onResolved); 

            var resultPromise = new Promise<ConvertedT>(isSync: true);
            resultPromise.WithName(this.Name);

            Action<PromisedT> resolveHandler = v => {
                onResolved(v)
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
        public IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected,
            Action<float> onProgress) {
            var resultPromise = new Promise(isSync: true);
            resultPromise.WithName(this.Name);

            Action<PromisedT> resolveHandler = v => {
                if (onResolved != null) {
                    onResolved(v)
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
        public IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected, Action<float> onProgress) {
            var resultPromise = new Promise(isSync: true);
            resultPromise.WithName(this.Name);

            Action<PromisedT> resolveHandler = v => {
                if (onResolved != null) {
                    onResolved(v);
                }

                resultPromise.Resolve();
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
        /// Return a new promise with a different value.
        /// May also change the type of the value.
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, ConvertedT> transform) {
//            Argument.NotNull(() => transform);
            return this.Then(value => Promise<ConvertedT>.Resolved(transform(value)));
        }

        /// <summary>
        /// Helper function to invoke or register resolve/reject handlers.
        /// </summary>
        void ActionHandlers(IRejectable resultPromise, Action<PromisedT> resolveHandler,
            Action<Exception> rejectHandler) {
            if (this.CurState == PromiseState.Resolved) {
                this.InvokeHandler(resolveHandler, resultPromise, this.resolveValue);
            }
            else if (this.CurState == PromiseState.Rejected) {
                this.InvokeHandler(rejectHandler, resultPromise, this.rejectionException);
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
        /// Returns a promise for a collection of the resolved results.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        public IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(
            Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain) {
            return this.Then(value => Promise<ConvertedT>.All(chain(value)));
        }

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Converts to a non-value promise.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        public IPromise ThenAll(Func<PromisedT, IEnumerable<IPromise>> chain) {
            return this.Then(value => Promise.All(chain(value)));
        }

        /// <summary>
        /// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        /// Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise<IEnumerable<PromisedT>> All(params IPromise<PromisedT>[] promises) {
            return
                All(
                    (IEnumerable<IPromise<PromisedT>>)
                    promises); // Cast is required to force use of the other All function.
        }

        /// <summary>
        /// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        /// Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise<IEnumerable<PromisedT>> All(IEnumerable<IPromise<PromisedT>> promises) {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0) {
                return Promise<IEnumerable<PromisedT>>.Resolved(Enumerable.Empty<PromisedT>());
            }

            var remainingCount = promisesArray.Length;
            var results = new PromisedT[remainingCount];
            var progress = new float[remainingCount];
            var resultPromise = new Promise<IEnumerable<PromisedT>>(isSync: true);
            resultPromise.WithName("All");

            promisesArray.Each((promise, index) => {
                promise
                    .Progress(v => {
                        progress[index] = v;
                        if (resultPromise.CurState == PromiseState.Pending) {
                            resultPromise.ReportProgress(progress.Average());
                        }
                    })
                    .Then(result => {
                        progress[index] = 1f;
                        results[index] = result;

                        --remainingCount;
                        if (remainingCount <= 0 && resultPromise.CurState == PromiseState.Pending) {
                            // This will never happen if any of the promises errorred.
                            resultPromise.Resolve(results);
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
        /// Takes a function that yields an enumerable of promises.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// </summary>
        public IPromise<ConvertedT> ThenRace<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain) {
            return this.Then(value => Promise<ConvertedT>.Race(chain(value)));
        }

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Converts to a non-value promise.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// </summary>
        public IPromise ThenRace(Func<PromisedT, IEnumerable<IPromise>> chain) {
            return this.Then(value => Promise.Race(chain(value)));
        }

        /// <summary>
        /// Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        /// Returns the value from the first promise that has resolved.
        /// </summary>
        public static IPromise<PromisedT> Race(params IPromise<PromisedT>[] promises) {
            return
                Race(
                    (IEnumerable<IPromise<PromisedT>>)
                    promises); // Cast is required to force use of the other function.
        }

        /// <summary>
        /// Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        /// Returns the value from the first promise that has resolved.
        /// </summary>
        public static IPromise<PromisedT> Race(IEnumerable<IPromise<PromisedT>> promises) {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0) {
                throw new InvalidOperationException(
                    "At least 1 input promise must be provided for Race"
                );
            }

            var resultPromise = new Promise<PromisedT>(isSync: true);
            resultPromise.WithName("Race");

            var progress = new float[promisesArray.Length];

            promisesArray.Each((promise, index) => {
                promise
                    .Progress(v => {
                        if (resultPromise.CurState == PromiseState.Pending) {
                            progress[index] = v;
                            resultPromise.ReportProgress(progress.Max());
                        }
                    })
                    .Then(result => {
                        if (resultPromise.CurState == PromiseState.Pending) {
                            resultPromise.Resolve(result);
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
        /// Convert a simple value directly into a resolved promise.
        /// </summary>
        public static IPromise<PromisedT> Resolved(PromisedT promisedValue) {
            var promise = new Promise<PromisedT>(isSync: true);
            promise.Resolve(promisedValue);
            return promise;
        }

        /// <summary>
        /// Convert an exception directly into a rejected promise.
        /// </summary>
        public static IPromise<PromisedT> Rejected(Exception ex) {
//            Argument.NotNull(() => ex);

            var promise = new Promise<PromisedT>(isSync: true);
            promise.Reject(ex);
            return promise;
        }

        public IPromise<PromisedT> Finally(Action onComplete) {
            var promise = new Promise<PromisedT>(isSync: true);
            promise.WithName(this.Name);

            this.Then(x => promise.Resolve(x));
            this.Catch(e => {
                try {
                    onComplete();
                    promise.Reject(e);
                }
                catch (Exception ne) {
                    promise.Reject(ne);
                }
            });

            return promise.Then(v => {
                onComplete();
                return v;
            });
        }

        public IPromise ContinueWith(Func<IPromise> onComplete) {
            var promise = new Promise(isSync: true);
            promise.WithName(this.Name);

            this.Then(x => promise.Resolve());
            this.Catch(e => promise.Resolve());

            return promise.Then(onComplete);
        }

        public IPromise<ConvertedT> ContinueWith<ConvertedT>(Func<IPromise<ConvertedT>> onComplete) {
            var promise = new Promise(isSync: true);
            promise.WithName(this.Name);

            this.Then(x => promise.Resolve());
            this.Catch(e => promise.Resolve());

            return promise.Then(onComplete);
        }

        public IPromise<PromisedT> Progress(Action<float> onProgress) {
            if (onProgress != null) {
                this.ProgressHandlers(this, onProgress);
            }

            return this;
        }
    }
}