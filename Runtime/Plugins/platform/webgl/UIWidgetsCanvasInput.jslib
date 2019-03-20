mergeInto(LibraryManager.library, {

    $UIWidgetsCanvasInputModule__postset: 'UIWidgetsCanvasInputModule.init();',
    $UIWidgetsCanvasInputModule: { init: function() {
        
    // create a buffer that stores all inputs so that tabbing
    // between them is made possible.
    var inputs = [];

    // initialize the Canvas Input
    var UIWidgetsCanvasInput = window.UIWidgetsCanvasInput = function(o) {
        var self = this;

        o = o ? o : {};

        // setup the defaults
        self._canvas = o.canvas || null;
        self._x = o.x || 0;
        self._y = o.y || 0;
        self._type = o.type || 'text';
        self._onchange = o.onchange || function() {};
        self._onsubmit = o.onsubmit || function() {};
        self._onkeydown = o.onkeydown || function() {};
        self._onkeyup = o.onkeyup || function() {};
        self._onfocus = o.onfocus || function() {};
        self._onblur = o.onblur || function() {};
        self._multiline = o.multiline || false;
        self._hasFocus = false;

        self._createHiddenInput();
        self.value(o.value  || '');
        self._inputsIndex = inputs.length - 1;

    };

    // setup the prototype
    UIWidgetsCanvasInput.prototype = {

        x: function(data) {
            var self = this;

            if (typeof data !== 'undefined') {
                self._x = data;
                self._updateHiddenInput();

                return;
            } else {
                return self._x;
            }
        },

        y: function(data) {
            var self = this;

            if (typeof data !== 'undefined') {
                self._y = data;
                self._updateHiddenInput();

                return;
            } else {
                return self._y;
            }
        },

        multiline: function(data) {
            var self = this;
            if (typeof data !== 'undefined') {
                if (data === self._multiline) {
                    return;
                }
                self._multiline = !!data;
                self._createHiddenInput();
                return;
            } else {
                return self._multiline;
            }
        },

        type: function(data) {
            var self = this;
            if (typeof data !== 'undefined') {
                self._type = data;
                self._hiddenInput.type = data;
                return;
            } else {
                return self._type;
            }
        },

        value: function(data) {
            var self = this;
            if (typeof data !== 'undefined') {
                self._hiddenInput.value = data;
                return;
            } else {
                return self._hiddenInput.value;
            }
        },

        selection: function() {
            var self = this;
            return [self._hiddenInput.selectionStart, self._hiddenInput.selectionEnd];
        },

        onsubmit: function(fn) {
            var self = this;

            if (typeof fn !== 'undefined') {
                self._onsubmit = fn;

                return self;
            } else {
                self._onsubmit();
            }
        },

        onkeydown: function(fn) {
            var self = this;

            if (typeof fn !== 'undefined') {
                self._onkeydown = fn;

                return self;
            } else {
                self._onkeydown();
            }
        },

        onkeyup: function(fn) {
            var self = this;

            if (typeof fn !== 'undefined') {
                self._onkeyup = fn;

                return self;
            } else {
                self._onkeyup();
            }
        },

        focus: function() {
            var self = this;

            // only fire the focus event when going from unfocussed
            if (!self._hasFocus) {
                self._onfocus(self);

                // remove focus from all other inputs
                for (var i=0; i<inputs.length; i++) {
                    if (inputs[i]._hasFocus) {
                        inputs[i].blur();
                    }
                }
            }

            self._hasFocus = true;
            self._hiddenInput.focus();
            return;
        },

        blur: function(_this) {
            var self = _this || this;

            self._onblur(self);

            self._hasFocus = false;
            self._hiddenInput.blur();
        },

        keydown: function(e, self) {
            var keyCode = e.which,
                isShift = e.shiftKey,
                key = null,
                startText, endText;

            // make sure the correct text field is being updated
            if (!self._hasFocus) {
                return;
            }

            // fire custom user event
            self._onkeydown(e, self);

            // add support for Ctrl/Cmd+A selection
            if (keyCode === 65 && (e.ctrlKey || e.metaKey)) {
                self.selectText();
                e.preventDefault();
                return;
            }

            // block keys that shouldn't be processed
            if (keyCode === 17 || e.metaKey || e.ctrlKey) {
                return;
            }

            if (keyCode === 13 && !self._multiline) { // enter key
                e.preventDefault();
                self._onsubmit(e, self);
            } else {
                self._onchange();
            }
            return;
        },

        selectText: function(range) {
            var self = this,
                range = range || [0, self.value().length];

            // select the range of text specified (or all if none specified)
            setTimeout(function() {
                self._hiddenInput.selectionStart = range[0];
                self._hiddenInput.selectionEnd = range[1];
                self._onchange();
            }, 1);

            return self;
        },

        destroy: function() {
            var self = this;

            // pull from the inputs array
            var index = inputs.indexOf(self);
            if (index !== -1) {
                inputs.splice(index, 1);
            }

            // remove focus
            if (self._hasFocus) {
                self.blur();
            }

            // remove the hidden input box
            self._hiddenInput.parentNode.removeChild(self._hiddenInput);

            self._renderCtx = null;
        },

        _updateHiddenInput: function() {
            var self = this;
            self._hiddenInput.style.left = (self._x + (self._canvas ? self._canvas.offsetLeft : 0)) + 'px';
            self._hiddenInput.style.top = (self._y + (self._canvas ? self._canvas.offsetTop : 0)) + 'px';
        },

        _createHiddenInput: function () {
            var self = this;
            if (self._hiddenInput) {
                self._hiddenInput.parentNode.removeChild(self._hiddenInput);
                self._hiddenInput = null;
            }
            self._hiddenInput = document.createElement(self._multiline ? 'textarea' : 'input');
            self._hiddenInput.type = self._type;
            self._hiddenInput.style.position = 'absolute';
            self._hiddenInput.style.opacity = 0;
            self._hiddenInput.style.pointerEvents = 'none';
            self._hiddenInput.style.zIndex = 0;
            // hide native blue text cursor on iOS
            self._hiddenInput.style.transform = 'scale(0)';

            self._updateHiddenInput();
            self._canvas.parentNode.appendChild(self._hiddenInput);

            // setup the keydown listener
            self._hiddenInput.addEventListener('keydown', function(e) {
                e = e || window.event;

                if (self._hasFocus) {
                    // hack to fix touch event bug in iOS Safari
                    window.focus();
                    self._hiddenInput.focus();

                    // continue with the keydown event
                    self.keydown(e, self);
                }
            });

            // setup the keyup listener
            self._hiddenInput.addEventListener('keyup', function(e) {
                e = e || window.event;
                self._onchange();

                if (self._hasFocus) {
                    self._onkeyup(e, self);
                }
            });
        }
    };
    
}}});
