using System;
using System.Collections.Generic;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.foundation {
    public interface Listenable {
        void addListener(VoidCallback listener);

        void removeListener(VoidCallback listener);
    }

    public static class ListenableUtils {
        public static Listenable merge(List<Listenable> listenables) {
            return new _MergingListenable(listenables);
        }
    }

    public interface ValueListenable<T> : Listenable {
        T value { get; }
    }

    public class ChangeNotifier : Listenable {
        public ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        public bool hasListeners {
            get { return this._listeners.Count > 0; }
        }

        public void addListener(VoidCallback listener) {
            this._listeners.Add(listener);
        }

        public void removeListener(VoidCallback listener) {
            this._listeners.Remove(listener);
        }

        public virtual void dispose() {
            this._listeners = null;
        }

        public void notifyListeners() {
            if (this._listeners != null) {
                var localListeners = new List<VoidCallback>(this._listeners);
                foreach (VoidCallback listener in localListeners) {
                    try {
                        if (this._listeners.Contains(listener)) {
                            listener();
                        }
                    }
                    catch (Exception ex) {
                        Debug.LogError("error while dispatching notifications: " + ex);
                    }
                }
            }
        }
    }

    public class _MergingListenable : ChangeNotifier {
        public _MergingListenable(List<Listenable> _children) {
            this._children = _children;

            foreach (Listenable child in _children) {
                if (child != null) {
                    child.addListener(this.notifyListeners);
                }
            }
        }

        public readonly List<Listenable> _children;

        public override void dispose() {
            foreach (Listenable child in this._children) {
                if (child != null) {
                    child.removeListener(this.notifyListeners);
                }
            }

            base.dispose();
        }
    }

    public class ValueNotifier<T> : ChangeNotifier, ValueListenable<T> {
        public ValueNotifier(T value) {
            this._value = value;
        }

        public T value {
            get { return this._value; }
            set {
                if (object.Equals(value, this._value)) {
                    return;
                }

                this._value = value;
                this.notifyListeners();
            }
        }

        public T _value;
    }
}