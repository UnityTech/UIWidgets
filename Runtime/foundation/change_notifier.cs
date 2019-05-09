using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.foundation {
    public interface Listenable {
        void addListener(VoidCallback listener);

        void removeListener(VoidCallback listener);
    }

    public static class ListenableUtils {
        public static Listenable merge(this List<Listenable> listenables) {
            return new _MergingListenable(listenables);
        }
    }

    public interface ValueListenable<T> : Listenable {
        T value { get; }
    }

    public class ChangeNotifier : Listenable {
        ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        bool _debugAssertNotDisposed() {
            D.assert(() => {
                if (this._listeners == null) {
                    throw new UIWidgetsError(
                        string.Format("A {0} was used after being disposed.\n" +
                                      "Once you have called dispose() on a {0}, it can no longer be used.",
                            this.GetType()));
                }

                return true;
            });

            return true;
        }

        protected bool hasListeners {
            get {
                D.assert(this._debugAssertNotDisposed());
                return this._listeners.isNotEmpty();
            }
        }

        public void addListener(VoidCallback listener) {
            D.assert(this._debugAssertNotDisposed());
            this._listeners.Add(listener);
        }

        public void removeListener(VoidCallback listener) {
            D.assert(this._debugAssertNotDisposed());
            this._listeners.Remove(listener);
        }

        public virtual void dispose() {
            D.assert(this._debugAssertNotDisposed());
            this._listeners = null;
        }

        protected virtual void notifyListeners() {
            D.assert(this._debugAssertNotDisposed());
            if (this._listeners != null) {
                var localListeners = new List<VoidCallback>(this._listeners);
                foreach (VoidCallback listener in localListeners) {
                    try {
                        if (this._listeners.Contains(listener)) {
                            listener();
                        }
                    }
                    catch (Exception ex) {
                        UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                            exception: ex,
                            library: "foundation library",
                            context: "while dispatching notifications for " + this.GetType(),
                            informationCollector: information => {
                                information.AppendLine("The " + this.GetType() + " sending notification was:");
                                information.Append("  " + this);
                            }
                        ));
                    }
                }
            }
        }
    }

    class _MergingListenable : Listenable {
        internal _MergingListenable(List<Listenable> _children) {
            this._children = _children;
        }

        readonly List<Listenable> _children;

        public void addListener(VoidCallback listener) {
            foreach (Listenable child in this._children) {
                child?.addListener(listener);
            }
        }

        public void removeListener(VoidCallback listener) {
            foreach (Listenable child in this._children) {
                child?.removeListener(listener);
            }
        }

        public override string ToString() {
            return "Listenable.merge([" + string.Join(", ", this._children.Select(c => c.ToString()).ToArray()) + "])";
        }
    }

    public class ValueNotifier<T> : ChangeNotifier, ValueListenable<T> {
        public ValueNotifier(T value) {
            this._value = value;
        }

        public virtual T value {
            get { return this._value; }
            set {
                if (Equals(value, this._value)) {
                    return;
                }

                this._value = value;
                this.notifyListeners();
            }
        }

        T _value;

        public override string ToString() {
            return Diagnostics.describeIdentity(this) + "(" + this._value + ")";
        }
    }
}