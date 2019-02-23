using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.service {
    public class TextInputType : IEquatable<TextInputType> {
        public readonly int index;
        public readonly bool? signed;
        public readonly bool? decimalNum;

        TextInputType(int index, bool? signed = null, bool? decimalNum = null) {
            this.index = index;
            this.signed = signed;
            this.decimalNum = decimalNum;
        }

        public static TextInputType numberWithOptions(bool signed = false, bool decimalNum = false) {
            return new TextInputType(2, signed: signed, decimalNum: decimalNum);
        }

        public static readonly TextInputType text = new TextInputType(0);
        public static readonly TextInputType multiline = new TextInputType(1);

        public static readonly TextInputType number = numberWithOptions();

        public static readonly TextInputType phone = new TextInputType(3);

        public static readonly TextInputType datetime = new TextInputType(4);

        public static readonly TextInputType emailAddress = new TextInputType(5);

        public static readonly TextInputType url = new TextInputType(6);

        public static List<string> _names = new List<string> {
            "text", "multiline", "number", "phone", "datetime", "emailAddress", "url"
        };

        public Dictionary<string, object> toJson() {
            return new Dictionary<string, object>() {
                {"name", this._name},
                {"signed", this.signed},
                {"decimal", this.decimalNum}
            };
        }

        string _name {
            get { return $"TextInputType.{_names[this.index]}"; }
        }

        public bool Equals(TextInputType other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.index == other.index && this.signed == other.signed && this.decimalNum == other.decimalNum;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((TextInputType) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.index;
                hashCode = (hashCode * 397) ^ this.signed.GetHashCode();
                hashCode = (hashCode * 397) ^ this.decimalNum.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(TextInputType left, TextInputType right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextInputType left, TextInputType right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType().FullName}(name: {this._name}, signed: {this.signed}, decimal: {this.decimalNum})";
        }
    }

    public class TextEditingValue : IEquatable<TextEditingValue> {
        public readonly string text;
        public readonly TextSelection selection;
        public readonly TextRange composing;

        public TextEditingValue(string text = "", TextSelection selection = null, TextRange composing = null) {
            this.text = text;
            this.selection = selection ?? TextSelection.collapsed(-1);
            this.composing = composing ?? TextRange.empty;
        }

        public TextEditingValue copyWith(string text = null, TextSelection selection = null,
            TextRange composing = null) {
            return new TextEditingValue(
                text ?? this.text, selection ?? this.selection, composing ?? this.composing
            );
        }

        public TextEditingValue insert(string text) {
            string newText;
            TextSelection newSelection;
            if (string.IsNullOrEmpty(text)) {
                return this;
            }

            newText = this.selection.textBefore(this.text) + text + this.selection.textAfter(this.text);
            newSelection = TextSelection.collapsed(this.selection.start + text.Length);
            return new TextEditingValue(
                text: newText, selection: newSelection, composing: TextRange.empty
            );
        }

        public TextEditingValue deleteSelection(bool backDelete = true) {
            if (this.selection.isCollapsed) {
                if (backDelete) {
                    if (this.selection.start == 0) {
                        return this;
                    }

                    return this.copyWith(
                        text: this.text.Substring(0, this.selection.start - 1) + this.selection.textAfter(this.text),
                        selection: TextSelection.collapsed(this.selection.start - 1));
                }

                if (this.selection.start >= this.text.Length) {
                    return this;
                }

                return this.copyWith(text: this.text.Substring(0, this.selection.start) +
                                           this.text.Substring(this.selection.start + 1));
            }
            else {
                var newText = this.selection.textBefore(this.text) + this.selection.textAfter(this.text);
                return this.copyWith(text: newText, selection: TextSelection.collapsed(this.selection.start));
            }
        }

        public TextEditingValue moveLeft() {
            return this.moveSelection(-1);
        }

        public TextEditingValue moveRight() {
            return this.moveSelection(1);
        }

        public TextEditingValue extendLeft() {
            return this.moveExtent(-1);
        }

        public TextEditingValue extendRight() {
            return this.moveExtent(1);
        }

        public TextEditingValue moveExtent(int move) {
            int offset = this.selection.extentOffset + move;
            offset = Mathf.Max(0, offset);
            offset = Mathf.Min(offset, this.text.Length);
            return this.copyWith(selection: this.selection.copyWith(extentOffset: offset));
        }

        public TextEditingValue moveSelection(int move) {
            int offset = this.selection.baseOffset + move;
            offset = Mathf.Max(0, offset);
            offset = Mathf.Min(offset, this.text.Length);
            return this.copyWith(selection: TextSelection.collapsed(offset, affinity: this.selection.affinity));
        }

        public TextEditingValue compose(string composeText) {
            D.assert(!string.IsNullOrEmpty(composeText));
            var composeStart = this.composing == TextRange.empty ? this.selection.start : this.composing.start;
            var lastComposeEnd = this.composing == TextRange.empty ? this.selection.end : this.composing.end;
            var newText = this.text.Substring(0, composeStart) + composeText + this.text.Substring(lastComposeEnd);
            var componseEnd = composeStart + composeText.Length;
            return new TextEditingValue(
                text: newText, selection: TextSelection.collapsed(componseEnd),
                composing: new TextRange(composeStart, componseEnd)
            );
        }

        public TextEditingValue clearCompose() {
            if (this.composing == TextRange.empty) {
                return this;
            }

            return new TextEditingValue(
                text: this.text.Substring(0, this.composing.start) + this.text.Substring(this.composing.end),
                selection: TextSelection.collapsed(this.composing.start),
                composing: TextRange.empty
            );
        }

        public static readonly TextEditingValue empty = new TextEditingValue();

        public bool Equals(TextEditingValue other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(this.text, other.text) && Equals(this.selection, other.selection) &&
                   Equals(this.composing, other.composing);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((TextEditingValue) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.text != null ? this.text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.selection != null ? this.selection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.composing != null ? this.composing.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TextEditingValue left, TextEditingValue right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextEditingValue left, TextEditingValue right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Text: {this.text}, Selection: {this.selection}, Composing: {this.composing}";
        }
    }

    public interface TextSelectionDelegate {
        TextEditingValue textEditingValue { get; set; }

        void hideToolbar();

        void bringIntoView(TextPosition textPosition);
    }

    public interface TextInputClient {
        void updateEditingValue(TextEditingValue value);

        void performAction(TextInputAction action);
    }

    public enum TextInputAction {
        none,
        unspecified,
        done,
        go,
        search,
        send,
        next,
        previous,
        continueAction,
        join,
        route,
        emergencyCall,
        newline,
        moveLeft,
        moveRight,
        moveUp,
        moveDown,
        moveLineStart,
        moveLineEnd,
        moveTextStart,
        moveTextEnd,
        movePageUp,
        movePageDown,
        moveGraphicalLineStart,
        moveGraphicalLineEnd,
        moveWordLeft,
        moveWordRight,
        moveParagraphForward,
        moveParagraphBackward,
        moveToStartOfNextWord,
        moveToEndOfPreviousWord,
        selectLeft,
        selectRight,
        selectUp,
        selectDown,
        selectTextStart,
        selectTextEnd,
        selectPageUp,
        selectPageDown,
        expandSelectGraphicalLineStart,
        expandSelectGraphicalLineEnd,
        selectGraphicalLineStart,
        selectGraphicalLineEnd,
        selectWordLeft,
        selectWordRight,
        selectToEndOfPreviousWord,
        selectToStartOfNextWord,
        selectParagraphBackward,
        selectParagraphForward,
        delete,
        backspace,
        deleteWordBack,
        deleteWordForward,
        deleteLineBack,
        cut,
        copy,
        paste,
        selectAll,
        selectNone,
        scrollStart,
        scrollEnd,
        scrollPageUp,
        scrollPageDown,
    }

    public class TextInputConfiguration {
        public TextInputConfiguration(TextInputType inputType = null,
            bool obscureText = false, bool autocorrect = true, TextInputAction inputAction = TextInputAction.done) {
            this.inputType = inputType ?? TextInputType.text;
            this.inputAction = inputAction;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
        }

        public readonly TextInputType inputType;
        public readonly bool obscureText;
        public readonly bool autocorrect;
        public readonly TextInputAction inputAction;

        public Dictionary<string, object> toJson() {
            return new Dictionary<string, object>() {
                {"inputType", this.inputType.toJson()},
                {"obscureText", this.obscureText},
                {"autocorrect", this.autocorrect},
                {"inputAction", this.inputAction.ToString()}
            };
        }
    }

    public class TextInputConnection {
        internal TextInputConnection(TextInputClient client, TextInput textInput) {
            D.assert(client != null);
            D.assert(textInput != null);
            this._client = client;
            this._textInput = textInput;
            this._id = _nextId++;
        }

        public bool attached {
            get { return this._textInput._currentConnection == this; }
        }

        public void setEditingState(TextEditingValue value) {
            D.assert(this.attached);
            this._textInput.keyboardManager.setEditingState(value);
        }

        public void setCompositionCursorPos(float x, float y) {
            D.assert(this.attached);
            this._textInput.setCompositionCursorPos(x, y);
        }

        public void close() {
            if (this.attached) {
                this._textInput.keyboardManager.clearClient();
                this._textInput._currentConnection = null;
                Input.imeCompositionMode = IMECompositionMode.Auto;
                this._textInput._scheduleHide();
            }

            D.assert(!this.attached);
        }

        public void show() {
            D.assert(this.attached);
            Input.imeCompositionMode = IMECompositionMode.On;
            this._textInput.keyboardManager.show();
        }

        static int _nextId = 1;
        internal readonly int _id;
        internal readonly TextInputClient _client;
        internal readonly TextInput _textInput;
        TouchScreenKeyboard _keyboard;
    }

    public class TextInput {
        internal TextInputConnection _currentConnection;

        public readonly KeyboadManager keyboardManager;

        public TextInput() {
            this.keyboardManager = new KeyboadManager(this);
        }

        public TextInputConnection attach(TextInputClient client, TextInputConfiguration configuration) {
            D.assert(client != null);
            var connection = new TextInputConnection(client, this);
            this.keyboardManager.setClient(connection._id, configuration);
            this._currentConnection = connection;
            return connection;
        }

        public void setCompositionCursorPos(float x, float y) {
            Input.compositionCursorPos = new Vector2(x, y);
        }

        internal void _updateEditingState(int client, TextEditingValue value) {
            if (this._currentConnection == null) {
                return;
            }

            if (client != this._currentConnection._id) {
                return;
            }

            this._currentConnection._client.updateEditingValue(value);
        }

        internal void _performAction(int client, TextInputAction action) {
            if (this._currentConnection == null) {
                return;
            }

            if (client != this._currentConnection._id) {
                return;
            }

            this._currentConnection._client.performAction(action);
        }


        bool _hidePending = false;

        internal void _scheduleHide() {
            if (this._hidePending) {
                return;
            }

            this._hidePending = true;

            Window.instance.scheduleMicrotask(() => {
                this._hidePending = false;
                if (this._currentConnection == null) {
                    this.keyboardManager.hide();
                }
            });
        }
    }
}