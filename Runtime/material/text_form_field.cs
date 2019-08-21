using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgets.Runtime.material {
    public class TextFormField : FormField<string> {
        public TextFormField(
            Key key = null,
            TextEditingController controller = null,
            string initialValue = null,
            FocusNode focusNode = null,
            InputDecoration decoration = null,
            TextInputType keyboardType = null,
            TextCapitalization textCapitalization = TextCapitalization.none,
            TextInputAction? textInputAction = null,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextDirection? textDirection = null,
            TextAlign textAlign = TextAlign.left,
            bool autofocus = false,
            bool obscureText = false,
            bool autocorrect = true,
            bool autovalidate = false,
            bool maxLengthEnforced = true,
            int? maxLines = 1,
            int? minLines = null,
            bool expands = false,
            int? maxLength = null,
            VoidCallback onEditingComplete = null,
            ValueChanged<string> onFieldSubmitted = null,
            FormFieldSetter<string> onSaved = null,
            FormFieldValidator<string> validator = null,
            List<TextInputFormatter> inputFormatters = null,
            bool enabled = true,
            float cursorWidth = 2.0f,
            Radius cursorRadius = null,
            Color cursorColor = null,
            Brightness? keyboardAppearance = null,
            EdgeInsets scrollPadding = null,
            bool enableInteractiveSelection = true,
            InputCounterWidgetBuilder buildCounter = null
        ) : base(
            key: key,
            initialValue: controller != null ? controller.text : (initialValue ?? ""),
            onSaved: onSaved,
            validator: validator,
            autovalidate: autovalidate,
            enabled: enabled,
            builder: (FormFieldState<string> field) => {
                _TextFormFieldState state = (_TextFormFieldState) field;
                InputDecoration effectiveDecoration = (decoration ?? new InputDecoration())
                    .applyDefaults(Theme.of(field.context).inputDecorationTheme);
                return new TextField(
                    controller: state._effectiveController,
                    focusNode: focusNode,
                    decoration: effectiveDecoration.copyWith(errorText: field.errorText),
                    keyboardType: keyboardType,
                    textInputAction: textInputAction,
                    style: style,
                    strutStyle: strutStyle,
                    textAlign: textAlign,
                    textDirection: textDirection ?? TextDirection.ltr,
                    textCapitalization: textCapitalization,
                    autofocus: autofocus,
                    obscureText: obscureText,
                    autocorrect: autocorrect,
                    maxLengthEnforced: maxLengthEnforced,
                    maxLines: maxLines,
                    minLines: minLines,
                    expands: expands,
                    maxLength: maxLength,
                    onChanged: field.didChange,
                    onEditingComplete: onEditingComplete,
                    onSubmitted: onFieldSubmitted,
                    inputFormatters: inputFormatters,
                    enabled: enabled,
                    cursorWidth: cursorWidth,
                    cursorRadius: cursorRadius,
                    cursorColor: cursorColor,
                    scrollPadding: scrollPadding ?? EdgeInsets.all(20.0f),
                    keyboardAppearance: keyboardAppearance,
                    enableInteractiveSelection: enableInteractiveSelection,
                    buildCounter: buildCounter
                );
            }
        ) {
            D.assert(initialValue == null || controller == null);
            D.assert(maxLines > 0);
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert((maxLines == null) || (minLines == null) || (maxLines >= minLines),
                () => "minLines can't be greater than maxLines");
            D.assert(!expands || (maxLines == null && minLines == null),
                () => "minLines and maxLines must be null when expands is true.");
            D.assert(maxLength == null || maxLength > 0);
            this.controller = controller;
        }

        public readonly TextEditingController controller;

        public override State createState() {
            return new _TextFormFieldState();
        }
    }

    class _TextFormFieldState : FormFieldState<string> {
        TextEditingController _controller;

        public TextEditingController _effectiveController {
            get { return this.widget.controller ?? this._controller; }
        }

        public new TextFormField widget {
            get { return (TextFormField) base.widget; }
        }

        public override void initState() {
            base.initState();
            if (this.widget.controller == null) {
                this._controller = new TextEditingController(text: this.widget.initialValue);
            }
            else {
                this.widget.controller.addListener(this._handleControllerChanged);
            }
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            TextFormField oldWidget = _oldWidget as TextFormField;
            base.didUpdateWidget(oldWidget);
            if (this.widget.controller != oldWidget.controller) {
                oldWidget.controller?.removeListener(this._handleControllerChanged);
                this.widget.controller?.addListener(this._handleControllerChanged);

                if (oldWidget.controller != null && this.widget.controller == null) {
                    this._controller = TextEditingController.fromValue(oldWidget.controller.value);
                }

                if (this.widget.controller != null) {
                    this.setValue(this.widget.controller.text);
                    if (oldWidget.controller == null) {
                        this._controller = null;
                    }
                }
            }
        }

        public override void dispose() {
            this.widget.controller?.removeListener(this._handleControllerChanged);
            base.dispose();
        }

        public override void reset() {
            base.reset();
            this.setState(() => { this._effectiveController.text = (string) this.widget.initialValue; });
        }

        void _handleControllerChanged() {
            if (this._effectiveController.text != this.value) {
                this.didChange(this._effectiveController.text);
            }
        }
    }
}