using System;

namespace Unity.UIWidgets.Redux {
    public static class ReduxThunk {
        public static Middleware<State> create<State>() {
            return (store) => (next) => new DispatcherImpl((action) => {
                var thunkAction = action as ThunkAction<State>;
                if (thunkAction != null && thunkAction.action != null) {
                    return thunkAction.action(store.dispatcher, store.getState);
                }
                
                return next.dispatch(action);          
            });
        }
    }

    public sealed class ThunkAction<State> {
        public readonly Func<Dispatcher, Func<State>, object> action;
        public readonly string displayName;

        public ThunkAction(
            Func<Dispatcher, Func<State>, object> action = null,
            string displayName = null) {
            this.action = action;
            this.displayName = displayName ?? "";
        }

        public override string ToString() {
            return "ThunkAction(" + this.displayName + ")";
        }
    }
}
