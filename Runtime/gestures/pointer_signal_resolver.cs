using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.gestures {
    public delegate void PointerSignalResolvedCallback(PointerSignalEvent evt);

    public class PointerSignalResolver {
        PointerSignalResolvedCallback _firstRegisteredCallback;

        PointerSignalEvent _currentEvent;

        public void register(PointerSignalEvent evt, PointerSignalResolvedCallback callback) {
            D.assert(evt != null);
            D.assert(callback != null);
            D.assert(this._currentEvent == null || this._currentEvent == evt);
            if (this._firstRegisteredCallback != null) {
                return;
            }

            this._currentEvent = evt;
            this._firstRegisteredCallback = callback;
        }

        public void resolve(PointerSignalEvent evt) {
            if (this._firstRegisteredCallback == null) {
                D.assert(this._currentEvent == null);
                return;
            }

            D.assert(this._currentEvent == evt);
            try {
                this._firstRegisteredCallback(evt);
            }
            catch (Exception exception) {
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "gesture library",
                        context: "while resolving a PointerSignalEvent",
                        informationCollector: information => {
                            information.AppendLine("Event: ");
                            information.AppendFormat(" {0}", evt);
                        }
                    )
                );
            }

            this._firstRegisteredCallback = null;
            this._currentEvent = null;
        }
    }
}