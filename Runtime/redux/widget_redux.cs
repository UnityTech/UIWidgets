using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.Redux {
    public class StoreProvider<State> : InheritedWidget {
        readonly Store<State> _store;

        public StoreProvider(
            Store<State> store = null,
            Widget child = null,
            Key key = null) : base(key: key, child: child) {
            D.assert(store != null);
            D.assert(child != null);
            this._store = store;
        }

        public static Store<State> of(BuildContext context) {
            var type = _typeOf<StoreProvider<State>>();
            StoreProvider<State> provider = context.inheritFromWidgetOfExactType(type) as StoreProvider<State>;
            if (provider == null) {
                throw new UIWidgetsError("StoreProvider is missing");
            }

            return provider._store;
        }

        static Type _typeOf<T>() {
            return typeof(T);
        }

        public override bool updateShouldNotify(InheritedWidget old) {
            return !Equals(this._store, ((StoreProvider<State>) old)._store);
        }
    }

    public delegate Widget ViewModelBuilder<ViewModel>(BuildContext context, ViewModel viewModel, Dispatcher dispatcher);

    public delegate ViewModel StoreConverter<State, ViewModel>(State state);

    public delegate bool ShouldRebuildCallback<ViewModel>(ViewModel previous, ViewModel current);

    public class StoreConnector<State, ViewModel> : StatelessWidget {
        public readonly ViewModelBuilder<ViewModel> builder;

        public readonly StoreConverter<State, ViewModel> converter;

        public readonly ShouldRebuildCallback<ViewModel> shouldRebuild;

        public readonly bool pure;

        public StoreConnector(
            ViewModelBuilder<ViewModel> builder = null,
            StoreConverter<State, ViewModel> converter = null,
            bool pure = false, 
            ShouldRebuildCallback<ViewModel> shouldRebuild = null,
            Key key = null) : base(key) {
            D.assert(builder != null);
            D.assert(converter != null);
            this.pure = pure;
            this.builder = builder;
            this.converter = converter;
            this.shouldRebuild = shouldRebuild;
        }

        public override Widget build(BuildContext context) {
            return new _StoreListener<State, ViewModel>(
                store: StoreProvider<State>.of(context),
                builder: this.builder,
                converter: this.converter,
                pure: this.pure,
                shouldRebuild: this.shouldRebuild
            );
        }
    }

    public class _StoreListener<State, ViewModel> : StatefulWidget {
        public readonly ViewModelBuilder<ViewModel> builder;

        public readonly StoreConverter<State, ViewModel> converter;

        public readonly Store<State> store;

        public readonly ShouldRebuildCallback<ViewModel> shouldRebuild;

        public readonly bool pure;

        public _StoreListener(
            ViewModelBuilder<ViewModel> builder = null,
            StoreConverter<State, ViewModel> converter = null,
            Store<State> store = null,
            bool pure = false,
            ShouldRebuildCallback<ViewModel> shouldRebuild = null,
            Key key = null) : base(key) {
            D.assert(builder != null);
            D.assert(converter != null);
            D.assert(store != null);
            this.store = store;
            this.builder = builder;
            this.converter = converter;
            this.pure = pure;
            this.shouldRebuild = shouldRebuild;
        }

        public override widgets.State createState() {
            return new _StoreListenerState<State, ViewModel>();
        }
    }

    class _StoreListenerState<State, ViewModel> : State<_StoreListener<State, ViewModel>> {
        ViewModel latestValue;

        public override void initState() {
            base.initState();
            this._init();
        }

        public override void dispose() {
            this.widget.store.stateChanged -= this._handleStateChanged;
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var oldStore = ((_StoreListener<State, ViewModel>) oldWidget).store;
            if (this.widget.store != oldStore) {
                oldStore.stateChanged -= this._handleStateChanged;
                this._init();
            }

            base.didUpdateWidget(oldWidget);
        }

        void _init() {
            this.widget.store.stateChanged += this._handleStateChanged;
            this.latestValue = this.widget.converter(this.widget.store.getState());
        }

        void _handleStateChanged(State state) {
            if (Window.hasInstance) {
                this._innerStateChanged(state);
            }
            else {
                using (WindowProvider.of(this.context).getScope()) {
                    this._innerStateChanged(state);
                }
            }
        }

        void _innerStateChanged(State state) {
            var preValue = this.latestValue;
            this.latestValue = this.widget.converter(this.widget.store.getState());
            if (this.widget.shouldRebuild != null) {
                if (!this.widget.shouldRebuild(preValue, this.latestValue)) {
                    return;
                }
            }
            else if (this.widget.pure) {
                if (Equals(preValue, this.latestValue)) {
                    return;
                }
            }

            this.setState();
        }

        public override Widget build(BuildContext context) {
            return this.widget.builder(context, this.latestValue, this.widget.store.dispatcher);
        }
    }
}