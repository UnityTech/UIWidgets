using System;
using System.Linq;

namespace Unity.UIWidgets.Sample.Redux {
    public delegate object Dispatcher(object action);

    public delegate State Reducer<State>(State previousState, object action);

    public delegate Func<Dispatcher, Dispatcher> Middleware<State>(Store<State> store);

    public delegate void StateChangedHandler<State>(State action);

    public class Store<State> {
        public StateChangedHandler<State> stateChanged;
        State _state;
        readonly Dispatcher _dispatcher;
        readonly Reducer<State> _reducer;

        public Store(Reducer<State> reducer, State initialState = default(State),
            params Middleware<State>[] middlewares) {
            this._reducer = reducer;
            this._dispatcher = this.ApplyMiddlewares(middlewares);
            this._state = initialState;
        }

        public object Dispatch(object action) {
            return this._dispatcher(action);
        }

        public State state {
            get { return this._state; }
        }

        Dispatcher ApplyMiddlewares(params Middleware<State>[] middlewares) {
            return middlewares.Reverse().Aggregate<Middleware<State>, Dispatcher>(this.InnerDispatch,
                (current, middleware) => middleware(this)(current));
        }

        object InnerDispatch(object action) {
            this._state = this._reducer(this._state, action);

            if (this.stateChanged != null) {
                this.stateChanged(this._state);
            }

            return action;
        }
    }
}