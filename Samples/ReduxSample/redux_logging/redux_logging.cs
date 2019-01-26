using UnityEngine;

namespace Unity.UIWidgets.Sample.Redux {
    public class ReduxLogging {
        public static Middleware<State> Create<State>() {
            return (store) => (next) => (action) => {
                var previousState = store.state;
                var previousStateDump = JsonUtility.ToJson(previousState);
                var result = next(action);
                var afterState = store.state;
                var afterStateDump = JsonUtility.ToJson(afterState);
                Debug.LogFormat("Action name={0} data={1}", action.GetType().Name, JsonUtility.ToJson(action));
                Debug.LogFormat("previousState=\n{0}", previousStateDump);
                Debug.LogFormat("afterState=\n{0}", afterStateDump);
                return result;
            };
        }
    }
}