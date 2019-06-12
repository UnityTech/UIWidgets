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

        void setIMEPos(Offset imeGlobalPos);

        void setClient(int client, TextInputConfiguration configuration);
        void clearClient();

        bool imeRequired();
    }

    public interface TextInputUpdateListener {
        void Update();
    }
    
    public interface TextInputOnGUIListener {
        void OnGUI();
    }

    class DefaultKeyboardDelegate : KeyboardDelegate, TextInputOnGUIListener {

        int _client;
        
        TextEditingValue _value;

        public void show() {
        }

        public void hide() {
        }

        public void setEditingState(TextEditingValue value) {
            this._value = value;
        }

        public void setIMEPos(Offset imeGlobalPos) {
            var uiWidgetWindowAdapter = Window.instance as UIWidgetWindowAdapter;
            if (uiWidgetWindowAdapter != null) {
                var screenPos = uiWidgetWindowAdapter.windowPosToScreenPos(imeGlobalPos);
                Input.compositionCursorPos = new Vector2(screenPos.dx, screenPos.dy);
            }
            else { // editor window case
                Input.compositionCursorPos = new Vector2(imeGlobalPos.dx, imeGlobalPos.dy);
            }
        }

        public void setClient(int client, TextInputConfiguration configuration) {
            this._client = client;
        }

        public void clearClient() {
            this._client = 0;
        }

        public bool imeRequired() {
            return true;
        }

        public void OnGUI() {
            if (TouchScreenKeyboard.isSupported) {
                return;
            }

            if (this._client == 0) {
                return;
            }
            
            
            var currentEvent = Event.current;
            var oldValue = this._value;
            
            if (currentEvent != null && currentEvent.type == EventType.KeyDown) {
                if (currentEvent.keyCode == KeyCode.Backspace) {
                    if (this._value.selection.isValid) {
                        this._value = this._value.deleteSelection(true);
                    }
                } else if (currentEvent.character != '\0') {
                    this._value = this._value.clearCompose();
                    char ch = currentEvent.character;
                    if (ch == '\r' || ch == 3) {
                        ch = '\n';
                    }

                    if (ch == '\n') {
                        Window.instance.run(() => { TextInput._performAction(this._client, TextInputAction.newline); });
                    }
                    
                    if (_validateCharacter(ch)) {
                        this._value = this._value.insert(new string(ch, 1));
                    }
                } else if (!string.IsNullOrEmpty(Input.compositionString)) {
                    this._value = this._value.compose(Input.compositionString);
                }
                
                currentEvent.Use();
            }

            if (this._value != oldValue) {
                Window.instance.run(() => { TextInput._updateEditingState(this._client, this._value); });
            }
        }

        public void Dispose() {
        }

        static bool _validateCharacter(char ch) {
            return ch >= ' ' || ch == '\t' || ch == '\r' || ch == 10 || ch == '\n';
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

        public bool imeRequired() {
            return false;
        }

        public void setIMEPos(Offset imeGlobalPos) {
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

    abstract class AbstractUIWidgetsKeyboardDelegate : KeyboardDelegate {
        
        protected AbstractUIWidgetsKeyboardDelegate() {
            UIWidgetsMessageManager.instance.
                AddChannelMessageDelegate("TextInput", this._handleMethodCall);
        }
        
        public void Dispose() {
            UIWidgetsMessageManager.instance.
                RemoveChannelMessageDelegate("TextInput", this._handleMethodCall);
        }

        public abstract void show();

        public abstract void hide();

        public abstract void setEditingState(TextEditingValue value);
        public abstract void setIMEPos(Offset imeGlobalPos);
        public abstract void setClient(int client, TextInputConfiguration configuration);

        public abstract void clearClient();
        public virtual bool imeRequired() {
            return false;
        }

        void _handleMethodCall(string method, List<JSONNode> args) {
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
    }
    
#if UNITY_WEBGL
    class UIWidgetsWebGLKeyboardDelegate : AbstractUIWidgetsKeyboardDelegate {
        
        public override void show() {
            Input.imeCompositionMode = IMECompositionMode.On;
        }

        public override void hide() {
        }

        public override void setEditingState(TextEditingValue value) {
            UIWidgetsTextInputSetTextInputEditingState(value.toJson().ToString());
        }

        public override void setIMEPos(Offset imeGlobalPos) {
            var window = Window.instance as UIWidgetWindowAdapter;
            var canvasPos = window.windowPosToScreenPos(imeGlobalPos);
            UIWidgetsTextInputSetIMEPos(canvasPos.dx, canvasPos.dy);
        }

        public override void setClient(int client, TextInputConfiguration configuration) {
            WebGLInput.captureAllKeyboardInput = false;
            Input.imeCompositionMode = IMECompositionMode.On;
            UIWidgetsTextInputSetClient(client, configuration.toJson().ToString());
        }

        public override void clearClient() {
            UIWidgetsTextInputClearTextInputClient();
        }

        public override bool imeRequired() {
            return true;
        }

        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetClient(int client, string configuration);
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetTextInputEditingState(string jsonText);
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputClearTextInputClient();
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetIMEPos(float x, float y);


    }
#endif
    
#if UNITY_IOS || UNITY_ANDROID
    class UIWidgetsTouchScreenKeyboardDelegate : AbstractUIWidgetsKeyboardDelegate {

        public override void show() {
            UIWidgetsTextInputShow();
        }

        public override void hide() {
            UIWidgetsTextInputHide();
        }

        public override void setEditingState(TextEditingValue value) {
            UIWidgetsTextInputSetTextInputEditingState(value.toJson().ToString());
        }

        public override void setIMEPos(Offset imeGlobalPos) {
        }

        public override void setClient(int client, TextInputConfiguration configuration) {
            UIWidgetsTextInputSetClient(client, configuration.toJson().ToString());
        }

        public override void clearClient() {
            UIWidgetsTextInputClearTextInputClient();
        }
        
#if UNITY_IOS
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputShow();
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputHide();
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetClient(int client, string configuration);
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetTextInputEditingState(string jsonText);
        
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
    }
#endif  
}