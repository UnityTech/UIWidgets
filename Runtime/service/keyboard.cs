using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.service {

    interface KeyboardDelegate: IDisposable {
        void show();
        void hide();
        void setEditingState(TextEditingValue value);

        void setClient(int client, TextInputConfiguration configuration);
        void clearClient();

        bool hashPreview();
    }

    public interface TextInputUpdateListener {
        void Update();
    }
    
    public interface TextInputOnGUIListener {
        void OnGUI();
    }

    class DefaultKeyboardDelegate : KeyboardDelegate, TextInputOnGUIListener {

        int _client;
        string _lastCompositionString;
        TextEditingValue _value;

        public void show() {
        }

        public void hide() {
        }

        public void setEditingState(TextEditingValue value) {
            this._value = value;
        }

        public void setClient(int client, TextInputConfiguration configuration) {
            this._client = client;
        }

        public void clearClient() {
            this._client = 0;
        }

        public bool hashPreview() {
            return false;
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
                    Window.instance.run(() => { TextInput._performAction(this._client, action.Value); });
                }

                if (action == null || action == TextInputAction.newline) {
                    if (currentEvent.keyCode == KeyCode.None) {
                        this._value = this._value.clearCompose().insert(new string(currentEvent.character, 1));
                        Window.instance.run(() => { TextInput._updateEditingState(this._client, this._value); });
                    }
                }

                currentEvent.Use();
            }

            if (!string.IsNullOrEmpty(Input.compositionString) &&
                this._lastCompositionString != Input.compositionString) {
                this._value = this._value.compose(Input.compositionString);
                Window.instance.run(() => { TextInput._updateEditingState(this._client, this._value); });
            }

            this._lastCompositionString = Input.compositionString;
        }

        public void Dispose() {
        }
    }
    
    class UnityTouchScreenKeyboardDelegate : KeyboardDelegate, TextInputUpdateListener {
        int _client;
        string _lastCompositionString;
        TextInputConfiguration _configuration;
        TextEditingValue _value;
        TouchScreenKeyboard _keyboard;
        RangeInt? _pendingSelection;
        bool _screenKeyboardDone;
        readonly TextInput _textInput;

        public void Update() {
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
                        TextInput._performAction(this._client,
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
                        : TextSelection.collapsed(0)
                );
                var changed = this._value != newValue;
                
                this._value = newValue;
                if (changed) {
                    Window.instance.run(() => {
                        TextInput._updateEditingState(this._client,
                            this._value);
                    });
                }
            }
        }

        public void show() {
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

        public bool hashPreview() {
            return this._keyboard != null;
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

        public void Dispose() {
        }
    }

#if UNITY_IOS || UNITY_ANDROID
    class UIWidgetsTouchScreenKeyboardDelegate : KeyboardDelegate {

        public UIWidgetsTouchScreenKeyboardDelegate() {
            UIWidgetsMessageManager.instance.AddChannelMessageDelegate("TextInput", this.handleMethodCall);
        }
        
        public void Dispose() {
            UIWidgetsMessageManager.instance.RemoveChannelMessageDelegate("TextInput", this.handleMethodCall);
        }

        public void show() {
            UIWidgetsTextInputShow();
        }

        public void hide() {
            UIWidgetsTextInputHide();
        }

        public void setEditingState(TextEditingValue value) {
            UIWidgetsTextInputSetTextInputEditingState(value.toJson().ToString());
        }

        public void setClient(int client, TextInputConfiguration configuration) {
            UIWidgetsTextInputSetClient(client, configuration.toJson().ToString());
        }

        public void clearClient() {
            UIWidgetsTextInputClearTextInputClient();
        }

        public bool hashPreview() {
            return false;
        }

        void handleMethodCall(string method, List<JSONNode> args) {
            if (TextInput._currentConnection == null) {
                return;
            }
            int client = args[0].AsInt;
            if (client != TextInput._currentConnection._id) {
                return;
            }

            using (TextInput._currentConnection._window.getScope()) {
                switch (method) {
                    case "TextInputClient.updateEditingState":
                        TextInput._updateEditingState(client, TextEditingValue.fromJson(args[1].AsObject));
                        break;
                    case "TextInputClient.performAction":
                        TextInput._performAction(client, TextInputUtils._toTextInputAction(args[1].Value));
                        break;
                    default:
                        throw new UIWidgetsError($"unknown method ${method}");
                }
            }
        }
        
#if UNITY_IOS
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputShow();
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputHide();
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetClient(int client, string configuration);
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetTextInputEditingState(string jsonText); // also send to client ?
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputClearTextInputClient();
#elif UNITY_ANDROID
        internal static void UIWidgetsTextInputShow() {
            using (
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
                pluginClass.CallStatic("show");
            }
        }
        
        internal static void UIWidgetsTextInputHide() {
            using (
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
                pluginClass.CallStatic("hide");
            }
        }
        
        internal static void UIWidgetsTextInputSetClient(int client, string configuration) {
            using (
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
               
                pluginClass.CallStatic("setClient", client, configuration);
            }
        }
        
        internal static void UIWidgetsTextInputSetTextInputEditingState(string jsonText) {
            using (
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
                pluginClass.CallStatic("setEditingState", jsonText);
            }
        }
        
        internal static void UIWidgetsTextInputClearTextInputClient() {
            using (
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
                pluginClass.CallStatic("clearClient");
            }
        }
#endif
#endif    
    }

}