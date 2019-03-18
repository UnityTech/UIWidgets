using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class WillPopScope : StatefulWidget {
        public WillPopScope(
            Key key = null,
            Widget child = null,
            WillPopCallback onWillPop = null
        ) : base(key: key) {
            D.assert(child != null);
            this.onWillPop = onWillPop;
            this.child = child;
        }

        public readonly Widget child;

        public readonly WillPopCallback onWillPop;

        public override State createState() {
            return new _WillPopScopeState();
        }
    }

    class _WillPopScopeState : State<WillPopScope> {
        ModalRoute _route;

        public _WillPopScopeState() {
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (this.widget.onWillPop != null) {
                this._route?.removeScopedWillPopCallback(this.widget.onWillPop);
            }

            this._route = ModalRoute.of(this.context);
            if (this.widget.onWillPop != null) {
                this._route?.addScopedWillPopCallback(this.widget.onWillPop);
            }
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            D.assert(this._route == ModalRoute.of(this.context));
            WillPopScope oldWidget = _oldWidget as WillPopScope;
            if (this.widget.onWillPop != oldWidget.onWillPop && this._route != null) {
                if (oldWidget.onWillPop != null) {
                    this._route.removeScopedWillPopCallback(oldWidget.onWillPop);
                }

                if (this.widget.onWillPop != null) {
                    this._route.addScopedWillPopCallback(this.widget.onWillPop);
                }
            }
        }

        public override void dispose() {
            if (this.widget.onWillPop != null) {
                this._route?.removeScopedWillPopCallback(this.widget.onWillPop);
            }

            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return this.widget.child;
        }
    }
}