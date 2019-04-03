using UnityEngine;

namespace Unity.UIWidgets.Redux {
    public static class ReduxLogging {
        public static Middleware<State> create<State>() {
            return (store) => (next) => new DispatcherImpl((action) => {
                var previousState = store.getState();
                var previousStateDump = JsonUtility.ToJson(previousState);
                var result = next.dispatch(action);
                var afterState = store.getState();
                var afterStateDump = JsonUtility.ToJson(afterState);
                Debug.LogFormat("Action name={0} data={1}", action.ToString(), JsonUtility.ToJson(action));
                Debug.LogFormat("previousState=\n{0}", previousStateDump);
                Debug.LogFormat("afterState=\n{0}", afterStateDump);
                return result;
            });
        }
    }
}