using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.service {
    
    public class RawKeyboard {
        public static readonly RawKeyboard instance = new RawKeyboard();

        RawKeyboard() {
            
        }
        
        readonly List<ValueChanged<RawKeyEvent>> _listeners = new List<ValueChanged<RawKeyEvent>>();
        
        public void addListener(ValueChanged<RawKeyEvent> listener) {
            this._listeners.Add(listener);
        }
        
        public void removeListener(ValueChanged<RawKeyEvent> listener) {
            this._listeners.Remove(listener);
        }

        internal void _handleKeyEvent(Event evt) {
            if (this._listeners.isEmpty()) {
                return;
            }

            var keyboardEvent = RawKeyEvent.fromEvent(evt);
            if (keyboardEvent == null) {
                return;
            }

            foreach (var listener in new List<ValueChanged<RawKeyEvent>>(this._listeners)) {
                if (this._listeners.Contains(listener)) {
                    listener(keyboardEvent);
                }    
            }
        }
    }

    public abstract class RawKeyEvent {
        
        protected RawKeyEvent(RawKeyEventData data) {
            this.data = data;
        }

        public static RawKeyEvent fromEvent(Event evt) {
            if (evt == null) {
                return null;
            }

            if (evt.type == EventType.KeyDown) {
                return new RawKeyDownEvent(new RawKeyEventData(evt));
            } else if (evt.type == EventType.KeyUp) {
                return new RawKeyUpEvent(new RawKeyEventData(evt));
            }

            return null;
        }
        
        public readonly RawKeyEventData data;
    }
    
    public class RawKeyDownEvent: RawKeyEvent {
        public RawKeyDownEvent(RawKeyEventData data) : base(data) {
        }
    }

    public class RawKeyUpEvent : RawKeyEvent {
        public RawKeyUpEvent(RawKeyEventData data) : base(data) {
        }
    }

    public class RawKeyEventData {
        public readonly Event unityEvent;

        public RawKeyEventData(Event unityEvent) {
            this.unityEvent = unityEvent;
        }
    }
}