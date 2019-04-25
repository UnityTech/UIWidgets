using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.animation {

 
    public abstract class AnimationLazyListenerMixinAnimation<T> : Animation<T> {
        int _listenerCounter = 0;

        protected void didRegisterListener() {
            D.assert(this._listenerCounter >= 0);
            if (this._listenerCounter == 0) {
                this.didStartListening();
            }

            this._listenerCounter += 1;
        }

        protected void didUnregisterListener() {
            D.assert(this._listenerCounter >= 1);
            this._listenerCounter -= 1;
            if (this._listenerCounter == 0) {
                this.didStopListening();
            }
        }

        protected abstract void didStartListening();

        protected abstract void didStopListening();

        public bool isListening {
            get { return this._listenerCounter > 0; }
        }
    }



 
    public abstract class AnimationEagerListenerMixinAnimation<T> : Animation<T> {
        protected void didRegisterListener() {
        }

        protected void didUnregisterListener() {
        }

        public virtual void dispose() {
        }
    }



 
    public abstract class AnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T> : AnimationLazyListenerMixinAnimation<T> {
        readonly ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        public override void addListener(VoidCallback listener) {
            this.didRegisterListener();
            this._listeners.Add(listener);
        }

        public override void removeListener(VoidCallback listener) {
            bool removed = this._listeners.Remove(listener);
            if (removed) {
                this.didUnregisterListener();
            }
        }

        public void notifyListeners() {
            var localListeners = new List<VoidCallback>(this._listeners);
            foreach (VoidCallback listener in localListeners) {
                try {
                    if (this._listeners.Contains(listener)) {
                        listener();
                    }
                } catch (Exception exception) {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: "while notifying listeners for " + this.GetType(),
                        informationCollector: information => {
                            information.AppendLine("The " + this.GetType() + " notifying listeners was:");
                            information.Append("  " + this);
                        }
                    ));
                }
            }
        }
    }


 
    public abstract class AnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<T> : AnimationEagerListenerMixinAnimation<T> {
        readonly ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        public override void addListener(VoidCallback listener) {
            this.didRegisterListener();
            this._listeners.Add(listener);
        }

        public override void removeListener(VoidCallback listener) {
            bool removed = this._listeners.Remove(listener);
            if (removed) {
                this.didUnregisterListener();
            }
        }

        public void notifyListeners() {
            var localListeners = new List<VoidCallback>(this._listeners);
            foreach (VoidCallback listener in localListeners) {
                try {
                    if (this._listeners.Contains(listener)) {
                        listener();
                    }
                } catch (Exception exception) {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: "while notifying listeners for " + this.GetType(),
                        informationCollector: information => {
                            information.AppendLine("The " + this.GetType() + " notifying listeners was:");
                            information.Append("  " + this);
                        }
                    ));
                }
            }
        }
    }



 
    public abstract class AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T> : AnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T> {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            this.didRegisterListener();
            this._statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            bool removed = this._statusListeners.Remove(listener);
            if (removed) {
                this.didUnregisterListener();
            }
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(this._statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (this._statusListeners.Contains(listener)) {
                        listener(status);
                    }
                } catch (Exception exception) {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: "while notifying status listeners for " + this.GetType(),
                        informationCollector: information => {
                            information.AppendLine("The " + this.GetType() + " notifying status listeners was:");
                            information.Append("  " + this);
                        }
                    ));
                }
            }
        }
    }


 
    public abstract class AnimationLocalStatusListenersMixinAnimationLazyListenerMixinAnimation<T> : AnimationLazyListenerMixinAnimation<T> {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            this.didRegisterListener();
            this._statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            bool removed = this._statusListeners.Remove(listener);
            if (removed) {
                this.didUnregisterListener();
            }
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(this._statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (this._statusListeners.Contains(listener)) {
                        listener(status);
                    }
                } catch (Exception exception) {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: "while notifying status listeners for " + this.GetType(),
                        informationCollector: information => {
                            information.AppendLine("The " + this.GetType() + " notifying status listeners was:");
                            information.Append("  " + this);
                        }
                    ));
                }
            }
        }
    }


 
    public abstract class AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<T> : AnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<T> {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            this.didRegisterListener();
            this._statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            bool removed = this._statusListeners.Remove(listener);
            if (removed) {
                this.didUnregisterListener();
            }
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(this._statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (this._statusListeners.Contains(listener)) {
                        listener(status);
                    }
                } catch (Exception exception) {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: "while notifying status listeners for " + this.GetType(),
                        informationCollector: information => {
                            information.AppendLine("The " + this.GetType() + " notifying status listeners was:");
                            information.Append("  " + this);
                        }
                    ));
                }
            }
        }
    }


}