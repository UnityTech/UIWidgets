using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.animation {

 
    public abstract class AnimationLazyListenerMixinAnimationDouble : AnimationDouble {
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



 
    public abstract class AnimationEagerListenerMixinAnimationDouble : AnimationDouble {
        protected void didRegisterListener() {
        }

        protected void didUnregisterListener() {
        }

        public virtual void dispose() {
        }
    }



 
    public abstract class AnimationLocalListenersMixinAnimationLazyListenerMixinAnimationDouble : AnimationLazyListenerMixinAnimationDouble {
        readonly ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        public override void addListener(VoidCallback listener) {
            this.didRegisterListener();
            this._listeners.Add(listener);
        }

        public override void removeListener(VoidCallback listener) {
            this._listeners.Remove(listener);
            this.didUnregisterListener();
        }

        public void notifyListeners() {
            var localListeners = new List<VoidCallback>(this._listeners);
            foreach (VoidCallback listener in localListeners) {
                try {
                    if (this._listeners.Contains(listener)) {
                        listener();
                    }
                }
                catch (Exception exception) {
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


 
    public abstract class AnimationLocalListenersMixinAnimationEagerListenerMixinAnimationDouble : AnimationEagerListenerMixinAnimationDouble {
        readonly ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        public override void addListener(VoidCallback listener) {
            this.didRegisterListener();
            this._listeners.Add(listener);
        }

        public override void removeListener(VoidCallback listener) {
            this._listeners.Remove(listener);
            this.didUnregisterListener();
        }

        public void notifyListeners() {
            var localListeners = new List<VoidCallback>(this._listeners);
            foreach (VoidCallback listener in localListeners) {
                try {
                    if (this._listeners.Contains(listener)) {
                        listener();
                    }
                }
                catch (Exception exception) {
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



 
    public abstract class AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimationDouble : AnimationLocalListenersMixinAnimationLazyListenerMixinAnimationDouble {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            this.didRegisterListener();
            this._statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            this._statusListeners.Remove(listener);
            this.didUnregisterListener();
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(this._statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (this._statusListeners.Contains(listener)) {
                        listener(status);
                    }
                }
                catch (Exception exception) {
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


 
    public abstract class AnimationLocalStatusListenersMixinAnimationLazyListenerMixinAnimationDouble : AnimationLazyListenerMixinAnimationDouble {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            this.didRegisterListener();
            this._statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            this._statusListeners.Remove(listener);
            this.didUnregisterListener();
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(this._statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (this._statusListeners.Contains(listener)) {
                        listener(status);
                    }
                }
                catch (Exception exception) {
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


 
    public abstract class AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationEagerListenerMixinAnimationDouble : AnimationLocalListenersMixinAnimationEagerListenerMixinAnimationDouble {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            this.didRegisterListener();
            this._statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            this._statusListeners.Remove(listener);
            this.didUnregisterListener();
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(this._statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (this._statusListeners.Contains(listener)) {
                        listener(status);
                    }
                }
                catch (Exception exception) {
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