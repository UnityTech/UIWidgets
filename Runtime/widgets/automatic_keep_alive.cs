using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class AutomaticKeepAlive : StatefulWidget {
        public AutomaticKeepAlive(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _AutomaticKeepAliveState();
        }
    }

    class _AutomaticKeepAliveState : State<AutomaticKeepAlive> {
        Dictionary<Listenable, VoidCallback> _handles;
        Widget _child;
        bool _keepingAlive = false;

        public override void initState() {
            base.initState();
            this._updateChild();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            this._updateChild();
        }

        void _updateChild() {
            this._child = new NotificationListener<KeepAliveNotification>(
                onNotification: this._addClient,
                child: this.widget.child
            );
        }

        public override void dispose() {
            if (this._handles != null) {
                foreach (Listenable handle in this._handles.Keys) {
                    handle.removeListener(this._handles[handle]);
                }
            }

            base.dispose();
        }

        bool _addClient(KeepAliveNotification notification) {
            Listenable handle = notification.handle;
            this._handles = this._handles ?? new Dictionary<Listenable, VoidCallback>();

            D.assert(!this._handles.ContainsKey(handle));
            this._handles[handle] = this._createCallback(handle);
            handle.addListener(this._handles[handle]);
            if (!this._keepingAlive) {
                this._keepingAlive = true;
                ParentDataElement childElement = this._getChildElement();
                if (childElement != null) {
                    this._updateParentDataOfChild(childElement);
                }
                else {
                    SchedulerBinding.instance.addPostFrameCallback(timeStamp => {
                        ParentDataElement childElement1 = this._getChildElement();
                        D.assert(childElement1 != null);
                        this._updateParentDataOfChild(childElement1);
                    });
                }
            }

            return false;
        }

        ParentDataElement _getChildElement() {
            Element element = (Element) this.context;
            Element childElement = null;
            element.visitChildren((Element child) => { childElement = child; });

            D.assert(childElement == null || childElement is ParentDataElement);
            return (ParentDataElement) childElement;
        }

        void _updateParentDataOfChild(ParentDataElement childElement) {
            childElement.applyWidgetOutOfTurn((ParentDataWidget) this.build(this.context));
        }

        VoidCallback _createCallback(Listenable handle) {
            return () => {
                D.assert(() => {
                    if (!this.mounted) {
                        throw new UIWidgetsError(
                            "AutomaticKeepAlive handle triggered after AutomaticKeepAlive was disposed." +
                            "Widgets should always trigger their KeepAliveNotification handle when they are " +
                            "deactivated, so that they (or their handle) do not send spurious events later " +
                            "when they are no longer in the tree."
                        );
                    }

                    return true;
                });
                this._handles.Remove(handle);
                if (this._handles.isEmpty()) {
                    if (SchedulerBinding.instance.schedulerPhase < SchedulerPhase.persistentCallbacks) {
                        this.setState(() => { this._keepingAlive = false; });
                    }
                    else {
                        this._keepingAlive = false;
                        Window.instance.scheduleMicrotask(() => {
                            if (this.mounted && this._handles.isEmpty()) {
                                this.setState(() => { D.assert(!this._keepingAlive); });
                            }
                        });
                    }
                }
            };
        }

        public override Widget build(BuildContext context) {
            D.assert(this._child != null);
            return new KeepAlive(
                keepAlive: this._keepingAlive,
                child: this._child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new FlagProperty("_keepingAlive", value: this._keepingAlive,
                ifTrue: "keeping subtree alive"));
            description.add(new DiagnosticsProperty<Dictionary<Listenable, VoidCallback>>(
                "handles",
                this._handles,
                description: this._handles != null ? this._handles.Count + " active clients" : null,
                ifNull: "no notifications ever received"
            ));
        }
    }

    public class KeepAliveNotification : Notification {
        public KeepAliveNotification(Listenable handle) {
            D.assert(handle != null);
            this.handle = handle;
        }

        public readonly Listenable handle;
    }

    public class KeepAliveHandle : ChangeNotifier {
        public void release() {
            this.notifyListeners();
        }
    }

    public abstract class AutomaticKeepAliveClientMixin<T> : State<T> where T : StatefulWidget {
        KeepAliveHandle _keepAliveHandle;

        void _ensureKeepAlive() {
            D.assert(this._keepAliveHandle == null);
            this._keepAliveHandle = new KeepAliveHandle();
            new KeepAliveNotification(this._keepAliveHandle).dispatch(this.context);
        }

        void _releaseKeepAlive() {
            this._keepAliveHandle.release();
            this._keepAliveHandle = null;
        }

        protected abstract bool wantKeepAlive { get; }

        protected void updateKeepAlive() {
            if (this.wantKeepAlive) {
                if (this._keepAliveHandle == null) {
                    this._ensureKeepAlive();
                }
            }
            else {
                if (this._keepAliveHandle != null) {
                    this._releaseKeepAlive();
                }
            }
        }

        public override void initState() {
            base.initState();
            if (this.wantKeepAlive) {
                this._ensureKeepAlive();
            }
        }

        public override void deactivate() {
            if (this._keepAliveHandle != null) {
                this._releaseKeepAlive();
            }

            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            if (this.wantKeepAlive && this._keepAliveHandle == null) {
                this._ensureKeepAlive();
            }

            return null;
        }
    }
}