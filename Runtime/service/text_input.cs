using System;
using System.Collections.Generic;
using Unity.UIWidgets.external.simplejson;
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

        public JSONNode toJson() {
            JSONObject jsonObject = new JSONObject();
            jsonObject["name"] = this._name;
            jsonObject["signed"] = this.signed;
            jsonObject["decimal"] = this.decimalNum;
            return jsonObject;
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

    static partial class TextInputUtils {
        internal static TextAffinity? _toTextAffinity(string affinity) {
            switch (affinity) {
                case "TextAffinity.downstream":
                    return TextAffinity.downstream;
                case "TextAffinity.upstream":
                    return TextAffinity.upstream;
            }
            return null;
        }

        internal static TextInputAction _toTextInputAction(string action) {
            switch (action) {
                case "TextInputAction.none":
                    return TextInputAction.none;
                case "TextInputAction.unspecified":
                    return TextInputAction.unspecified;
                case "TextInputAction.go":
                    return TextInputAction.go;
                case "TextInputAction.search":
                    return TextInputAction.search;
                case "TextInputAction.send":
                    return TextInputAction.send;
                case "TextInputAction.next":
                    return TextInputAction.next;
                case "TextInputAction.previuos":
                    return TextInputAction.previous;
                case "TextInputAction.continue_action":
                    return TextInputAction.continueAction;
                case "TextInputAction.join":
                    return TextInputAction.join;
                case "TextInputAction.route":
                    return TextInputAction.route;
                case "TextInputAction.emergencyCall":
                    return TextInputAction.emergencyCall;
                case "TextInputAction.done":
                    return TextInputAction.done;
                case "TextInputAction.newline":
                    return TextInputAction.newline;
            }
            throw new UIWidgetsError("Unknown text input action: $action");
        }
    }
    
    public class TextEditingValue : IEquatable<TextEditingValue> {
        public readonly string text;
        public readonly TextSelection selection;
        public readonly TextRange composing;
        static JSONNode defaultIndexNode = new JSONNumber(-1);
        public TextEditingValue(string text = "", TextSelection selection = null, TextRange composing = null) {
            this.text = text;
            this.selection = selection ?? TextSelection.collapsed(-1);
            this.composing = composing ?? TextRange.empty;
        }

        public static TextEditingValue fromJson(JSONObject json) {
            TextAffinity? affinity =
                TextInputUtils._toTextAffinity(json["selectionAffinity"].Value);
            return new TextEditingValue(
                text: json["text"].Value,
                selection: new TextSelection(
                    baseOffset: json.GetValueOrDefault("selectionBase", defaultIndexNode).AsInt,
                    extentOffset: json.GetValueOrDefault("selectionExtent", defaultIndexNode).AsInt,
                    affinity: affinity != null ? affinity.Value : TextAffinity.downstream,
                    isDirectional: json["selectionIsDirectional"].AsBool
                ),
                composing: new TextRange(
                    start: json.GetValueOrDefault("composingBase", defaultIndexNode).AsInt,
                    end: json.GetValueOrDefault("composingExtent", defaultIndexNode).AsInt
                )
            );
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

        public JSONNode toJson() {
            var json = new JSONObject();
            json["text"] = this.text;
            json["selectionBase"] = this.selection.baseOffset;
            json["selectionExtent"] = this.selection.extentOffset;
            json["selectionAffinity"] = this.selection.affinity.ToString();
            json["selectionIsDirectional"] = this.selection.isDirectional;
            json["composingBase"] = this.composing.start;
            json["composingExtent"] = this.composing.end;
            return json;
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

    public enum TextCapitalization {
        words,
        sentences,
        characters,
        none
    }

    class TextInputConfiguration {
        public TextInputConfiguration(TextInputType inputType = null,
            bool obscureText = false, bool autocorrect = true, TextInputAction inputAction = TextInputAction.done,
            Brightness keyboardAppearance = Brightness.light, TextCapitalization textCapitalization = TextCapitalization.none,
            bool unityTouchKeyboard = false) {
            this.inputType = inputType ?? TextInputType.text;
            this.inputAction = inputAction;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.textCapitalization = textCapitalization;
            this.keyboardAppearance = keyboardAppearance;
            this.unityTouchKeyboard = unityTouchKeyboard;
        }

        public readonly TextInputType inputType;
        public readonly bool obscureText;
        public readonly bool autocorrect;
        public readonly TextInputAction inputAction;
        public readonly TextCapitalization textCapitalization;
        public readonly Brightness keyboardAppearance;
        public readonly bool unityTouchKeyboard;

        public JSONNode toJson() {
            var json = new JSONObject();
            json["inputType"] = this.inputType.toJson();
            json["obscureText"] = this.obscureText;
            json["autocorrect"] = this.autocorrect;
            json["inputAction"] = $"TextInputAction.{this.inputAction.ToString()}";
            json["unityTouchKeyboard"] = this.unityTouchKeyboard;
            json["textCapitalization"] = $"TextCapitalization.{this.textCapitalization.ToString()}";
            json["keyboardAppearance"] = $"Brightness.{this.keyboardAppearance.ToString()}";
            return json;
        }
    }

    public class TextInputConnection {
        internal TextInputConnection(TextInputClient client) {
            D.assert(client != null);
            this._window = Window.instance;
            this._client = client;
            this._id = _nextId++;
        }

        public bool attached {
            get { return TextInput._currentConnection == this; }
        }

        public void setEditingState(TextEditingValue value) {
            D.assert(this.attached);
            TextInput.keyboardDelegate.setEditingState(value);
        }

        public void setCompositionCursorPos(float x, float y) {
            D.assert(this.attached);
            TextInput.setCompositionCursorPos(x, y);
        }

        public void close() {
            if (this.attached) {
                TextInput.keyboardDelegate.clearClient();
                TextInput._currentConnection = null;
                Input.imeCompositionMode = IMECompositionMode.Auto;
                TextInput._scheduleHide();
            }

            D.assert(!this.attached);
        }

        public void show() {
            D.assert(this.attached);
            Input.imeCompositionMode = IMECompositionMode.On;
            TextInput.keyboardDelegate.show();
        }

        static int _nextId = 1;
        internal readonly int _id;
        internal readonly TextInputClient _client;
        internal readonly Window _window;
        TouchScreenKeyboard _keyboard;
    }

    class TextInput {
        static internal TextInputConnection _currentConnection;

        static internal KeyboardDelegate keyboardDelegate;

        public TextInput() {
        }

        public static TextInputConnection attach(TextInputClient client, TextInputConfiguration configuration) {
            D.assert(client != null);
            var connection = new TextInputConnection(client);
            _currentConnection = connection;
            string x = configuration.toJson();
            if (keyboardDelegate != null) {
                keyboardDelegate.Dispose();
            }

            if (TouchScreenKeyboard.isSupported) {
#if UNITY_IOS || UNITY_ANDROID
                if (configuration.unityTouchKeyboard) {
                    keyboardDelegate = new UnityTouchScreenKeyboardDelegate();
                }
                else {
                    keyboardDelegate = new UIWidgetsTouchScreenKeyboardDelegate();
                }
#else
                keyboardDelegate = new UnityTouchScreenKeyboardDelegate();
#endif
            }
            else {
                keyboardDelegate = new DefaultKeyboardDelegate();
            }
            keyboardDelegate.setClient(connection._id, configuration);
            return connection;
        }

        internal static void Update() {
            if (_currentConnection != null && _currentConnection._window == Window.instance) {
                (keyboardDelegate as TextInputUpdateListener)?.Update();
            }
        }
        
        internal static void OnGUI() {
            if (_currentConnection != null && _currentConnection._window == Window.instance) {
                (keyboardDelegate as TextInputOnGUIListener)?.OnGUI();
            }
        }
        
        public static void setCompositionCursorPos(float x, float y) {
            Input.compositionCursorPos = new Vector2(x, y);
        }
        
        internal static void _updateEditingState(int client, TextEditingValue value) {
            if (_currentConnection == null) {
                return;
            }

            if (client != _currentConnection._id) {
                return;
            }

            _currentConnection._client.updateEditingValue(value);
        }

        internal static void  _performAction(int client, TextInputAction action) {
            if (_currentConnection == null) {
                return;
            }

            if (client != _currentConnection._id) {
                return;
            }

            _currentConnection._client.performAction(action);
        }

        static bool _hidePending = false;

        static internal void _scheduleHide() {
            if (_hidePending) {
                return;
            }

            _hidePending = true;

            Window.instance.scheduleMicrotask(() => {
                _hidePending = false;
                if (_currentConnection == null) {
                    keyboardDelegate.hide();
                }
            });
        }     
    }
}