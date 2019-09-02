using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class Form : StatefulWidget {
        public Form(
            Key key = null,
            Widget child = null,
            bool autovalidate = false,
            WillPopCallback onWillPop = null,
            VoidCallback onChanged = null
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
            this.autovalidate = autovalidate;
            this.onWillPop = onWillPop;
            this.onChanged = onChanged;
        }

        public static FormState of(BuildContext context) {
            _FormScope scope = (_FormScope) context.inheritFromWidgetOfExactType(typeof(_FormScope));
            return scope?._formState;
        }

        public readonly Widget child;

        public readonly bool autovalidate;

        public readonly WillPopCallback onWillPop;

        public readonly VoidCallback onChanged;

        public override State createState() {
            return new FormState();
        }
    }

    public class FormState : State<Form> {
        int _generation = 0;
        readonly HashSet<FormFieldState> _fields = new HashSet<FormFieldState>();

        public FormState() {
        }

        public void _fieldDidChange() {
            if (this.widget.onChanged != null) {
                this.widget.onChanged();
            }

            this._forceRebuild();
        }

        void _forceRebuild() {
            this.setState(() => { ++this._generation; });
        }

        public void _register(FormFieldState field) {
            this._fields.Add(field);
        }

        public void _unregister(FormFieldState field) {
            this._fields.Remove(field);
        }

        public override Widget build(BuildContext context) {
            if (this.widget.autovalidate) {
                this._validate();
            }

            return new WillPopScope(
                onWillPop: this.widget.onWillPop,
                child: new _FormScope(
                    formState: this,
                    generation: this._generation,
                    child: this.widget.child
                )
            );
        }

        public void save() {
            foreach (FormFieldState field in this._fields) {
                field.save();
            }
        }

        public void reset() {
            foreach (FormFieldState field in this._fields) {
                field.reset();
            }

            this._fieldDidChange();
        }

        public bool validate() {
            this._forceRebuild();
            return this._validate();
        }

        bool _validate() {
            bool hasError = false;
            foreach (FormFieldState field in this._fields) {
                hasError = !field.validate() || hasError;
            }

            return !hasError;
        }
    }

    class _FormScope : InheritedWidget {
        public _FormScope(
            Key key = null,
            Widget child = null,
            FormState formState = null,
            int? generation = null
        ) :
            base(key: key, child: child) {
            this._formState = formState;
            this._generation = generation;
        }

        public readonly FormState _formState;

        public readonly int? _generation;

        public Form form {
            get { return this._formState.widget; }
        }

        public override bool updateShouldNotify(InheritedWidget _old) {
            _FormScope old = _old as _FormScope;
            return this._generation != old._generation;
        }
    }

    public delegate string FormFieldValidator<T>(T value);

    public delegate void FormFieldSetter<T>(T newValue);

    public delegate Widget FormFieldBuilder<T>(FormFieldState<T> field) where T : class;

    public class FormField<T> : StatefulWidget where T : class {
        public FormField(
            Key key = null,
            FormFieldBuilder<T> builder = null,
            FormFieldSetter<T> onSaved = null,
            FormFieldValidator<T> validator = null,
            T initialValue = null,
            bool autovalidate = false,
            bool enabled = true
        ) : base(key: key) {
            D.assert(builder != null);
            this.onSaved = onSaved;
            this.validator = validator;
            this.builder = builder;
            this.initialValue = initialValue;
            this.autovalidate = autovalidate;
            this.enabled = enabled;
        }

        public readonly FormFieldSetter<T> onSaved;

        public readonly FormFieldValidator<T> validator;

        public readonly FormFieldBuilder<T> builder;

        public readonly T initialValue;

        public readonly bool autovalidate;

        public readonly bool enabled;

        public override State createState() {
            return new FormFieldState<T>();
        }
    }

    public interface FormFieldState {
        void save();

        bool validate();

        void reset();
    }

    public class FormFieldState<T> : State<FormField<T>>, FormFieldState where T : class {
        T _value;
        string _errorText;

        public T value {
            get { return this._value; }
        }

        public string errorText {
            get { return this._errorText; }
        }

        public bool hasError {
            get { return this._errorText != null; }
        }

        public void save() {
            if (this.widget.onSaved != null) {
                this.widget.onSaved(this.value);
            }
        }

        public virtual void reset() {
            this.setState(() => {
                this._value = this.widget.initialValue;
                this._errorText = null;
            });
        }

        public bool validate() {
            this.setState(() => { this._validate(); });
            return !this.hasError;
        }

        void _validate() {
            if (this.widget.validator != null) {
                this._errorText = this.widget.validator(this._value);
            }
        }

        public virtual void didChange(T value) {
            this.setState(() => { this._value = value; });
            Form.of(this.context)?._fieldDidChange();
        }

        protected void setValue(T value) {
            this._value = value;
        }

        public override void initState() {
            base.initState();
            this._value = this.widget.initialValue;
        }

        public override void deactivate() {
            Form.of(this.context)?._unregister(this);
            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            if (this.widget.autovalidate && this.widget.enabled) {
                this._validate();
            }

            Form.of(context)?._register(this);
            return this.widget.builder(this);
        }
    }
}