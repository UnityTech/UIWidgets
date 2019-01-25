using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.service {
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
            } else {
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
            offset = Math.Max(0, offset);
            offset = Math.Min(offset, this.text.Length);
            return this.copyWith(selection: this.selection.copyWith(extentOffset: offset));
        }

        public TextEditingValue moveSelection(int move) {
            int offset = this.selection.baseOffset + move;
            offset = Math.Max(0, offset);
            offset = Math.Min(offset, this.text.Length);
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
    // text client

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
            this._textInput._value = value;
        }

        public void setCompositionCursorPos(double x, double y) {
            D.assert(this.attached);
            this._textInput.setCompositionCursorPos(x, y);
        }

        public void close() {
            if (this.attached) {
                this._textInput._currentConnection = null;
                this._textInput._value = null;
                Input.imeCompositionMode = IMECompositionMode.Auto;
            }
            D.assert(!this.attached);
        }

        static int _nextId = 1;
        internal readonly int _id;
        internal readonly TextInputClient _client;
        internal readonly TextInput _textInput;
    }

    public class TextInput {
        internal TextInputConnection _currentConnection;
        internal TextEditingValue _value;
        string _lastCompositionString;

        public TextInputConnection attach(TextInputClient client) {
            D.assert(client != null);
            var connection = new TextInputConnection(client, this);
            this._currentConnection = connection;
            Input.imeCompositionMode = IMECompositionMode.On;
            return connection;
        }

        public void OnGUI() {
            if (this._currentConnection == null) {
                return;
            }

            var currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown) {
                var action = TextInputUtils.getInputAction(currentEvent);
                if (action != null) {
                    this._performAction(this._currentConnection._id, action.Value);
                }

                if (action == null || action == TextInputAction.newline) {
                    if (currentEvent.keyCode == KeyCode.None) {
                        this._value = this._value.clearCompose().insert(new string(currentEvent.character, 1));
                        this._updateEditingState(this._currentConnection._id, this._value);
                    }
                }
                currentEvent.Use();
            }

            if (!string.IsNullOrEmpty(Input.compositionString) &&
                this._lastCompositionString != Input.compositionString) {
                this._value = this._value.compose(Input.compositionString);
                this._updateEditingState(this._currentConnection._id, this._value);
            }

            this._lastCompositionString = Input.compositionString;
        }

        public void setCompositionCursorPos(double x, double y) {
            Input.compositionCursorPos = new Vector2((float) x, (float) y);
        }

        void _updateEditingState(int client, TextEditingValue value) {        
            Window.instance.run(() => {
                if (this._currentConnection == null) {
                    return;
                }

                if (client != this._currentConnection._id) {
                    return;
                }
                
                this._currentConnection._client.updateEditingValue(value);
            });
        }

        void _performAction(int client, TextInputAction action) {
            Window.instance.run(() => {
                if (this._currentConnection == null) {
                    return;
                }

                if (client != this._currentConnection._id) {
                    return;
                }
                
                this._currentConnection._client.performAction(action);
            }); 
        }

    }

   
}
