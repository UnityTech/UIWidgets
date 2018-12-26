namespace RSG {
    public static class PromiseHelpers {
        /// <summary>
        /// Returns a promise that resolves with all of the specified promises have resolved.
        /// Returns a promise of a tuple of the resolved results.
        /// </summary>
        public static IPromise<Tuple<T1, T2>> All<T1, T2>(IPromise<T1> p1, IPromise<T2> p2) {
            var val1 = default(T1);
            var val2 = default(T2);
            var numUnresolved = 2;
            var alreadyRejected = false;
            var promise = new Promise<Tuple<T1, T2>>();

            p1
                .Then(val => {
                    val1 = val;
                    numUnresolved--;
                    if (numUnresolved <= 0) {
                        promise.Resolve(Tuple.Create(val1, val2));
                    }
                })
                .Catch(e => {
                    if (!alreadyRejected) {
                        promise.Reject(e);
                    }

                    alreadyRejected = true;
                })
                .Done();

            p2
                .Then(val => {
                    val2 = val;
                    numUnresolved--;
                    if (numUnresolved <= 0) {
                        promise.Resolve(Tuple.Create(val1, val2));
                    }
                })
                .Catch(e => {
                    if (!alreadyRejected) {
                        promise.Reject(e);
                    }

                    alreadyRejected = true;
                })
                .Done();

            return promise;
        }

        /// <summary>
        /// Returns a promise that resolves with all of the specified promises have resolved.
        /// Returns a promise of a tuple of the resolved results.
        /// </summary>
        public static IPromise<Tuple<T1, T2, T3>> All<T1, T2, T3>(IPromise<T1> p1, IPromise<T2> p2, IPromise<T3> p3) {
            return All(All(p1, p2), p3)
                .Then(vals => Tuple.Create(vals.Item1.Item1, vals.Item1.Item2, vals.Item2));
        }

        /// <summary>
        /// Returns a promise that resolves with all of the specified promises have resolved.
        /// Returns a promise of a tuple of the resolved results.
        /// </summary>
        public static IPromise<Tuple<T1, T2, T3, T4>> All<T1, T2, T3, T4>(IPromise<T1> p1, IPromise<T2> p2,
            IPromise<T3> p3, IPromise<T4> p4) {
            return All(All(p1, p2), All(p3, p4))
                .Then(vals => Tuple.Create(vals.Item1.Item1, vals.Item1.Item2, vals.Item2.Item1, vals.Item2.Item2));
        }
    }
}