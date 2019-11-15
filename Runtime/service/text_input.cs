using System;
using System.Collections.Generic;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.service {
    public enum FloatingCursorDragState {
        Start,

        Update,

        End
    }

    public class RawFloatingCursorPoint {
        public RawFloatingCursorPoint(
            Offset offset = null,
            FloatingCursorDragState? state = null
        ) {
            D.assert(state != null);
            D.assert(state == FloatingCursorDragState.Update ? offset != null : true);
            this.offset = offset;
            this.state = state;
        }

        public readonly Offset offset;

        public readonly FloatingCursorDragState? state;
    }

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

        public static FloatingCursorDragState _toTextCursorAction(string state) {
            switch (state) {
                case "FloatingCursorDragState.start":
                    return FloatingCursorDragState.Start;
                case "FloatingCursorDragState.update":
                    return FloatingCursorDragState.Update;
                case "FloatingCursorDragState.end":
                    return FloatingCursorDragState.End;
            }

            throw new UIWidgetsError("Unknown text cursor action: $state");
        }

        public static RawFloatingCursorPoint _toTextPoint(FloatingCursorDragState state,
            Dictionary<string, float?> encoded) {
            D.assert(encoded.getOrDefault("X") != null,
                () => "You must provide a value for the horizontal location of the floating cursor.");
            D.assert(encoded.getOrDefault("Y") != null,
                () => "You must provide a value for the vertical location of the floating cursor.");
            Offset offset = state == FloatingCursorDragState.Update
                ? new Offset(encoded["X"] ?? 0.0f, encoded["Y"] ?? 0.0f)
                : new Offset(0, 0);
            return new RawFloatingCursorPoint(offset: offset, state: state);
        }
    }

    public class TextEditingValue : IEquatable<TextEditingValue> {
        public readonly string text;
        public readonly TextSelection selection;
        public readonly TextRange composing;
        static JSONNode defaultIndexNode = new JSONNumber(-1);

        public TextEditingValue(string text = "", TextSelection selection = null, TextRange composing = null) {
            this.text = text;
            this.composing = composing ?? TextRange.empty;

            if (selection != null && selection.start >= 0 && selection.end >= 0) {
                // handle surrogate pair emoji, which takes 2 utf16 chars
                // if selection cuts in the middle of the emoji, move it to the end
                int start = selection.start, end = selection.end;
                if (start < text.Length && char.IsLowSurrogate(text[start])) {
                    start++;
                }
                if (end < text.Length && char.IsLowSurrogate(text[end])) {
                    end++;
                }
                this.selection = selection.copyWith(start, end);
            }
            else {
                this.selection = TextSelection.collapsed(-1);
            }
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

            var selection = this.selection;
            if (selection.start < 0) {
                selection = TextSelection.collapsed(0, this.selection.affinity);
            }

            newText = selection.textBefore(this.text) + text + selection.textAfter(this.text);
            newSelection = TextSelection.collapsed(selection.start + text.Length);
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

                    if (char.IsHighSurrogate(this.text[this.selection.start - 1])) {
                        return this.copyWith(
                            text: this.text.Substring(0, this.selection.start - 1) +
                                  this.text.Substring(this.selection.start + 1),
                            selection: TextSelection.collapsed(this.selection.start - 1),
                            composing: TextRange.empty);
                    }

                    if (char.IsLowSurrogate(this.text[this.selection.start - 1])) {
                        D.assert(this.selection.start > 1);
                        return this.copyWith(
                            text: this.text.Substring(0, this.selection.start - 2) +
                                  this.selection.textAfter(this.text),
                            selection: TextSelection.collapsed(this.selection.start - 2),
                            composing: TextRange.empty);
                    }

                    return this.copyWith(
                        text: this.text.Substring(0, this.selection.start - 1) + this.selection.textAfter(this.text),
                        selection: TextSelection.collapsed(this.selection.start - 1),
                        composing: TextRange.empty);
                }

                if (this.selection.start >= this.text.Length) {
                    return this;
                }

                return this.copyWith(text: this.text.Substring(0, this.selection.start) +
                                           this.text.Substring(this.selection.start + 1),
                    composing: TextRange.empty);
            }
            else {
                var newText = this.selection.textBefore(this.text) + this.selection.textAfter(this.text);
                return this.copyWith(text: newText, selection: TextSelection.collapsed(this.selection.start),
                    composing: TextRange.empty);
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
            
            composeStart = Mathf.Clamp(composeStart, 0, this.text.Length);
            lastComposeEnd = Mathf.Clamp(lastComposeEnd, 0, this.text.Length);
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
        void updateEditingValue(TextEditingValue value, bool isIMEInput);

        void performAction(TextInputAction action);

        void updateFloatingCursor(RawFloatingCursorPoint point);

        RawInputKeyResponse globalInputKeyHandler(RawKeyEvent evt);
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
        newline
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
            Brightness keyboardAppearance = Brightness.light,
            TextCapitalization textCapitalization = TextCapitalization.none,
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

        public void setIMEPos(Offset imeGlobalPos) {
            D.assert(this.attached);
            D.assert(imeGlobalPos != null);
            D.assert(this.imeRequired());
            TextInput.keyboardDelegate.setIMEPos(imeGlobalPos);
        }

        public bool imeRequired() {
            return TextInput.keyboardDelegate != null && TextInput.keyboardDelegate.imeRequired();
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
            if (keyboardDelegate != null) {
                keyboardDelegate.Dispose();
            }

            if (Application.isEditor) {
                keyboardDelegate = new DefaultKeyboardDelegate();
            }
            else {
#if UNITY_IOS || UNITY_ANDROID
                if (configuration.unityTouchKeyboard) {
                    keyboardDelegate = new UnityTouchScreenKeyboardDelegate();
                }
                else {
                    keyboardDelegate = new UIWidgetsTouchScreenKeyboardDelegate();
                }
#elif UNITY_WEBGL
                keyboardDelegate = new UIWidgetsWebGLKeyboardDelegate();
#else
                keyboardDelegate = new DefaultKeyboardDelegate();
#endif
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

        internal static void _updateEditingState(int client, TextEditingValue value, bool isIMEInput = false) {
            if (_currentConnection == null) {
                return;
            }

            if (client != _currentConnection._id) {
                return;
            }

            _currentConnection._client.updateEditingValue(value, isIMEInput); 
        }

        internal static void _performAction(int client, TextInputAction action) {
            if (_currentConnection == null) {
                return;
            }

            if (client != _currentConnection._id) {
                return;
            }

            _currentConnection._client.performAction(action);
        }

        internal static RawInputKeyResponse _handleGlobalInputKey(int client, RawKeyEvent evt) {
            if (_currentConnection == null) {
                return RawInputKeyResponse.convert(evt);
            }

            if (client != _currentConnection._id) {
                return RawInputKeyResponse.convert(evt);
            }

            return _currentConnection._client.globalInputKeyHandler(evt);
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