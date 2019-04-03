using System;
using System.Linq;

namespace Unity.UIWidgets {
    public interface Dispatcher {
        T dispatch<T>(object action);

        object dispatch(object action);
    }

    public class DispatcherImpl : Dispatcher {
        readonly Func<object, object> _impl;

        public DispatcherImpl(Func<object, object> impl) {
            this._impl = impl;
        }
        
        public T dispatch<T>(object action) {
            if (this._impl == null) {
                return default;
            }

            return (T) this._impl(action);
        }

        public object dispatch(object action) {
            if (this._impl == null) {
                return default;
            }

            return this._impl(action);
        }
    }

    public delegate State Reducer<State>(State previousState, object action);

    public delegate Func<Dispatcher, Dispatcher> Middleware<State>(Store<State> store);

    public delegate void StateChangedHandler<State>(State action);

    public class Store<State> {
        public StateChangedHandler<State> stateChanged;
        
        readonly Dispatcher _dispatcher;
        readonly Reducer<State> _reducer;
        State _state;

        public Store(
            Reducer<State> reducer,
            State initialState = default,
            params Middleware<State>[] middleware) {
            this._reducer = reducer;
            this._dispatcher = this._applyMiddleware(middleware);
            this._state = initialState;
        }

        public Dispatcher dispatcher {
            get { return this._dispatcher; }
        }

        public State getState() {
            return this._state;
        }

        Dispatcher _applyMiddleware(params Middleware<State>[] middleware) {
            return middleware.Reverse().Aggregate<Middleware<State>, Dispatcher>(
                new DispatcherImpl(this._innerDispatch),
                (current, middlewareItem) => middlewareItem(this)(current));
        }

        object _innerDispatch(object action) {
            this._state = this._reducer(this._state, action);

            if (this.stateChanged != null) {
                this.stateChanged(this._state);
            }

            return action;
        }
    }
}