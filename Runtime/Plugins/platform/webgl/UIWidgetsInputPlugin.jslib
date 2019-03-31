mergeInto(LibraryManager.library, {

    $UIWidgetsInputPluginModule__postset: 'UIWidgetsInputPluginModule.init();',
    $UIWidgetsInputPluginModule: { init: function() {

    var UIWidgetsTextInputPlugin = window.UIWidgetsTextInputPlugin = function(o) {
        this._canvas = o.canvas;
        this._messageManager = o.messageManager;
    };

    // setup the prototype
    UIWidgetsTextInputPlugin.prototype = {

        setClient: function (client, configuration) {
            var self = this;
            if (!self._canvasInput) {
                self._canvasInput = new UIWidgetsCanvasInput({
                    canvas: self._canvas,
                    onchange: self._onchange.bind(self),
                    onsubmit: self._onsubmit.bind(self),
                });
                self._canvas.addEventListener('mouseup', function(e) {
                    if (self._client) {
                        self._canvasInput.focus();

                    }
                }, false);
            }

            var configObj = JSON.parse(configuration);
            var multiline = configObj.inputType.name === 'TextInputType.multiline';
            self._canvasInput.type(configObj.obscureText ? 'password' : 'text');
            self._canvasInput.multiline(multiline);

            self._canvasInput.focus();
            self._client = client;
        },

        setTextInputEditingState: function (jsonText) {
            var self = this;
            var state = JSON.parse(jsonText);
            self._canvasInput.value(state.text);
            self._canvasInput.selectText([state.selectionBase, state.selectionExtent]);
        },

        setTextInputIMEPos: function(x, y) {
            var self = this;
            self._canvasInput.x(x);
            self._canvasInput.y(y);
        },

        clearTextInputClient: function () {
            var self = this;
            self._canvasInput.blur();
            self._client = null;
        },

        _onsubmit: function() {
            var self = this;
            if (!self._client) {
                return;
            }
            self._messageManager.sendMethodInvokeMessage('TextInput', 'TextInputClient.performAction',
                [self._client, 'TextInputAction.done']);
        },

        _onchange: function() {
            var self = this;
            if (!self._client) {
                return;
            }

            var value = self._canvasInput.value();
            var selection = self._canvasInput.selection();

            var state = {
                selectionBase: selection[0],
                selectionExtent: selection[1],
                selectionIsDirectional: false,
                text: value
            };

            self._messageManager.sendMethodInvokeMessage('TextInput', 'TextInputClient.updateEditingState',
                [self._client, state]);
        }
    };


    var UIWidgetsMessageManager = window.UIWidgetsMessageManager = function(sendMessage) {
        var self = this;
        self._sendMesssage = sendMessage;

    };

    UIWidgetsMessageManager.prototype = {

        setObjectName: function (name) {
            var self = this;
            self._gameObjectName = name;
        },

        sendMethodInvokeMessage: function(channel, method, args) {
            var self = this;
            var body = {
                channel: channel,
                method: method,
                args: args
            };
            self._sendMesssage(self._gameObjectName, 'OnUIWidgetsMethodMessage', JSON.stringify(body));
        }
    };
    
 }}});
