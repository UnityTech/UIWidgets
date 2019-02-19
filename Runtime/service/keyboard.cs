using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.service {
    public class KeyboadManager {
        int _client;
        string _lastCompositionString;
        TextInputConfiguration _configuration;
        TextEditingValue _value;
        TouchScreenKeyboard _keyboard;
        RangeInt? _pendingSelection;
        bool _screenKeyboardDone;
        readonly TextInput _textInput;

        public KeyboadManager(TextInput textInput) {
            this._textInput = textInput;
        }

        public void Update() {
            if (!TouchScreenKeyboard.isSupported) {
                return;
            }

            if (this._client == 0 || this._keyboard == null) {
                return;
            }


            if (this._keyboard.canSetSelection && this._pendingSelection != null) {
                this._keyboard.selection = this._pendingSelection.Value;
                this._pendingSelection = null;
            }

            if (this._keyboard.status == TouchScreenKeyboard.Status.Done) {
                if (!this._screenKeyboardDone) {
                    this._screenKeyboardDone = true;
                    Window.instance.run(() => {
                        this._textInput._performAction(this._client,
                            TextInputAction.done);
                    });
                }
            }
            else if (this._keyboard.status == TouchScreenKeyboard.Status.Visible) {
                var keyboardSelection = this._keyboard.selection;
                var newValue = new TextEditingValue(
                    this._keyboard.text,
                    this._keyboard.canGetSelection
                        ? new TextSelection(keyboardSelection.start, keyboardSelection.end)
                        : this._value.selection
                );
                var changed = this._value != newValue;
                this._value = newValue;
                if (changed) {
                    Window.instance.run(() => {
                        this._textInput._updateEditingState(this._client,
                            this._value);
                    });
                }
            }
        }

        public void OnGUI() {
            if (TouchScreenKeyboard.isSupported) {
                return;
            }

            if (this._client == 0) {
                return;
            }

            var currentEvent = Event.current;
            if (currentEvent != null && currentEvent.type == EventType.KeyDown) {
                var action = TextInputUtils.getInputAction(currentEvent);
                if (action != null) {
                    Window.instance.run(() => { this._textInput._performAction(this._client, action.Value); });
                }

                if (action == null || action == TextInputAction.newline) {
                    if (currentEvent.keyCode == KeyCode.None) {
                        this._value = this._value.clearCompose().insert(new string(currentEvent.character, 1));
                        Window.instance.run(() => { this._textInput._updateEditingState(this._client, this._value); });
                    }
                }

                currentEvent.Use();
            }

            if (!string.IsNullOrEmpty(Input.compositionString) &&
                this._lastCompositionString != Input.compositionString) {
                this._value = this._value.compose(Input.compositionString);
                Window.instance.run(() => { this._textInput._updateEditingState(this._client, this._value); });
            }

            this._lastCompositionString = Input.compositionString;
        }

        public void show() {
            if (!TouchScreenKeyboard.isSupported) {
                return;
            }

            var secure = this._configuration.obscureText;
            var multiline = this._configuration.inputType == TextInputType.multiline;
            var autocorrection = this._configuration.autocorrect;
            this._keyboard = TouchScreenKeyboard.Open(this._value.text,
                getKeyboardTypeForConfiguration(this._configuration),
                autocorrection, multiline, secure, false, "", 0);
            this._pendingSelection = null;
            this._screenKeyboardDone = false;
            if (this._value.selection != null && this._value.selection.isValid) {
                int start = this._value.selection.start;
                int end = this._value.selection.end;
                this._pendingSelection = new RangeInt(start, end - start);
            }
        }

        public void clearClient() {
            this._client = 0;
        }

        public void setClient(int client, TextInputConfiguration configuration) {
            this._client = client;
            this._configuration = configuration;
        }

        public void hide() {
            if (this._keyboard != null) {
                this._keyboard.active = false;
                this._keyboard = null;
            }
        }

        public void setEditingState(TextEditingValue state) {
            this._value = state;
            if (this._keyboard != null && this._keyboard.active) {
                this._keyboard.text = state.text;
                if (this._value.selection != null && this._value.selection.isValid) {
                    int start = this._value.selection.start;
                    int end = this._value.selection.end;
                    this._pendingSelection = new RangeInt(start, end - start);
                    RangeInt selection = new RangeInt(state.selection.start, end - start);

                    if (this._keyboard.canGetSelection) {
                        this._pendingSelection = null;
                        this._keyboard.selection = selection;
                    }
                    else {
                        this._pendingSelection = selection;
                    }
                }
            }
        }

        public bool textInputOnKeyboard() {
            return TouchScreenKeyboard.isSupported;
        }

        static TouchScreenKeyboardType getKeyboardTypeForConfiguration(TextInputConfiguration config) {
            var inputType = config.inputType;

            if (inputType.index == TextInputType.url.index) {
                return TouchScreenKeyboardType.URL;
            }

            if (inputType.index == TextInputType.emailAddress.index) {
                return TouchScreenKeyboardType.EmailAddress;
            }

            if (inputType.index == TextInputType.phone.index) {
                return TouchScreenKeyboardType.PhonePad;
            }

            if (inputType.index == TextInputType.number.index) {
                return TouchScreenKeyboardType.NumberPad;
            }

            return TouchScreenKeyboardType.Default;
        }
    }
}