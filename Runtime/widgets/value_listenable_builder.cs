using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public delegate Widget ValueWidgetBuilder<T>(BuildContext context, T value, Widget child);

    public class ValueListenableBuilder<T> : StatefulWidget {
        public ValueListenableBuilder(
            ValueListenable<T> valueListenable,
            ValueWidgetBuilder<T> builder,
            Widget child = null
        ) {
            D.assert(valueListenable != null);
            D.assert(builder != null);
            this.valueListenable = valueListenable;
            this.builder = builder;
            this.child = child;
        }

        public readonly ValueListenable<T> valueListenable;

        public readonly ValueWidgetBuilder<T> builder;

        public readonly Widget child;

        public override State createState() {
            return new _ValueListenableBuilderState<T>();
        }
    }

    class _ValueListenableBuilderState<T> : State<ValueListenableBuilder<T>> {
        T value;

        public override void initState() {
            base.initState();
            this.value = this.widget.valueListenable.value;
            this.widget.valueListenable.addListener(this._valueChanged);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            ValueListenableBuilder<T> oldWidget = _oldWidget as ValueListenableBuilder<T>;
            if (oldWidget.valueListenable != this.widget.valueListenable) {
                oldWidget.valueListenable.removeListener(this._valueChanged);
                this.value = this.widget.valueListenable.value;
                this.widget.valueListenable.addListener(this._valueChanged);
            }

            base.didUpdateWidget(oldWidget);
        }

        public override void dispose() {
            this.widget.valueListenable.removeListener(this._valueChanged);
            base.dispose();
        }

        void _valueChanged() {
            this.setState(() => { this.value = this.widget.valueListenable.value; });
        }

        public override Widget build(BuildContext context) {
            return this.widget.builder(context, this.value, this.widget.child);
        }
    }
}