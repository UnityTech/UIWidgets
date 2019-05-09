var UIWidgetsLibrary = {


    //$method_support__postset: 'method_support.init1();method_support.init2();',
    //$method_support: {
        
    //},
    $UIWidgetsPlugin: {
        textInput: null,
        messageManager: null,

        getTextInput: function() {
            if (UIWidgetsPlugin.textInput) {
                return UIWidgetsPlugin.textInput;
            }

            UIWidgetsPlugin.textInput = new UIWidgetsTextInputPlugin({
                messageManager: UIWidgetsPlugin.getMessageManager(),
                canvas: Module.canvas
            });
            return UIWidgetsPlugin.textInput;
        },

        getMessageManager: function() {
            if (UIWidgetsPlugin.messageManager) {
                return UIWidgetsPlugin.messageManager;
            }

            UIWidgetsPlugin.messageManager = new UIWidgetsMessageManager(SendMessage);
            return UIWidgetsPlugin.messageManager;
        },

        messageObjectName: ""
    },

    UIWidgetsWebGLDevicePixelRatio: function () {
        return window.devicePixelRatio || 1;
    },


    UIWidgetsTextInputSetClient: function (client, configuration) {
        UIWidgetsPlugin.getTextInput().setClient(client, Pointer_stringify(configuration));
    },

    UIWidgetsTextInputSetTextInputEditingState: function (jsonText) {
        UIWidgetsPlugin.getTextInput().setTextInputEditingState(Pointer_stringify(jsonText));
    },

    UIWidgetsTextInputSetIMEPos: function(x, y) {
        UIWidgetsPlugin.getTextInput().setTextInputIMEPos(x, y);
    },

    UIWidgetsTextInputClearTextInputClient: function () {
        UIWidgetsPlugin.getTextInput().clearTextInputClient();
    },

    UIWidgetsMessageSetObjectName: function (name) {
        UIWidgetsPlugin.getMessageManager().setObjectName(Pointer_stringify(name));
    },
    
    UIWidgetsCopyTextToClipboard: function (text) {
        var el = document.createElement('textarea');
        el.value = Pointer_stringify(text);
        el.setAttribute('readonly', '');
        el.style = {
            position: 'fixed',
            left: '-9999px',
        };
        document.body.appendChild(el);
        el.select();
        document.execCommand('copy');
        document.body.removeChild(el);
    }
};

autoAddDeps(UIWidgetsLibrary, '$UIWidgetsPlugin');
autoAddDeps(UIWidgetsLibrary, '$UIWidgetsCanvasInputModule');
autoAddDeps(UIWidgetsLibrary, '$UIWidgetsInputPluginModule');
mergeInto(LibraryManager.library, UIWidgetsLibrary);
