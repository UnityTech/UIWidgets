mergeInto(LibraryManager.library, {
    JS_SystemInfo_GetWidth: function () {
        //////////// Modification Start ////////////////////
        return UnityLoader.SystemInfo.width * window.devicePixelRatio;
        //////////// Modification End ////////////////////
    },

    JS_SystemInfo_GetHeight: function () {
        //////////// Modification Start ////////////////////
        return UnityLoader.SystemInfo.height * window.devicePixelRatio;
        //////////// Modification End ////////////////////
    },

    JS_SystemInfo_GetCurrentCanvasWidth: function () {
        //////////// Modification Start ////////////////////
        return Module['canvas'].clientWidth * window.devicePixelRatio;
        //////////// Modification End ////////////////////
    },

    JS_SystemInfo_GetCurrentCanvasHeight: function () {
        //////////// Modification Start ////////////////////
        return Module['canvas'].clientHeight * window.devicePixelRatio;
        //////////// Modification End ////////////////////
    },
    $Browser : {
        mainLoop: {
            scheduler: null,
            method: "",
            currentlyRunningMainloop: 0,
            func: null,
            arg: 0,
            timingMode: 0,
            timingValue: 0,
            currentFrameNumber: 0,
            queue: [],
            pause: (function() {
                Browser.mainLoop.scheduler = null;
                Browser.mainLoop.currentlyRunningMainloop++;
            }),
            resume: (function() {
                Browser.mainLoop.currentlyRunningMainloop++;
                var timingMode = Browser.mainLoop.timingMode;
                var timingValue = Browser.mainLoop.timingValue;
                var func = Browser.mainLoop.func;
                Browser.mainLoop.func = null;
                _emscripten_set_main_loop(func, 0, false, Browser.mainLoop.arg, true);
                _emscripten_set_main_loop_timing(timingMode, timingValue);
                Browser.mainLoop.scheduler();
            }),
            updateStatus: (function() {
                if (Module["setStatus"]) {
                    var message = Module["statusMessage"] || "Please wait...";
                    var remaining = Browser.mainLoop.remainingBlockers;
                    var expected = Browser.mainLoop.expectedBlockers;
                    if (remaining) {
                        if (remaining < expected) {
                            Module["setStatus"](message + " (" + (expected - remaining) + "/" + expected + ")");
                        } else {
                            Module["setStatus"](message);
                        }
                    } else {
                        Module["setStatus"]("");
                    }
                }
            }),
            runIter: (function(func) {
                if (ABORT) return;
                if (Module["preMainLoop"]) {
                    var preRet = Module["preMainLoop"]();
                    if (preRet === false) {
                        return;
                    }
                }
                try {
                    func();
                } catch (e) {
                    if (e instanceof ExitStatus) {
                        return;
                    } else {
                        if (e && typeof e === "object" && e.stack) err("exception thrown: " + [ e, e.stack ]);
                        throw e;
                    }
                }
                if (Module["postMainLoop"]) Module["postMainLoop"]();
            })
        },
        isFullscreen: false,
        pointerLock: false,
        moduleContextCreatedCallbacks: [],
        workers: [],
        init: (function() {
            if (!Module["preloadPlugins"]) Module["preloadPlugins"] = [];
            if (Browser.initted) return;
            Browser.initted = true;
            try {
                new Blob;
                Browser.hasBlobConstructor = true;
            } catch (e) {
                Browser.hasBlobConstructor = false;
                console.log("warning: no blob constructor, cannot create blobs with mimetypes");
            }
            Browser.BlobBuilder = typeof MozBlobBuilder != "undefined" ? MozBlobBuilder : typeof WebKitBlobBuilder != "undefined" ? WebKitBlobBuilder : !Browser.hasBlobConstructor ? console.log("warning: no BlobBuilder") : null;
            Browser.URLObject = typeof window != "undefined" ? window.URL ? window.URL : window.webkitURL : undefined;
            if (!Module.noImageDecoding && typeof Browser.URLObject === "undefined") {
                console.log("warning: Browser does not support creating object URLs. Built-in browser image decoding will not be available.");
                Module.noImageDecoding = true;
            }
            var imagePlugin = {};
            imagePlugin["canHandle"] = function imagePlugin_canHandle(name) {
                return !Module.noImageDecoding && /\.(jpg|jpeg|png|bmp)$/i.test(name);
            };
            imagePlugin["handle"] = function imagePlugin_handle(byteArray, name, onload, onerror) {
                var b = null;
                if (Browser.hasBlobConstructor) {
                    try {
                        b = new Blob([ byteArray ], {
                            type: Browser.getMimetype(name)
                        });
                        if (b.size !== byteArray.length) {
                            b = new Blob([ (new Uint8Array(byteArray)).buffer ], {
                                type: Browser.getMimetype(name)
                            });
                        }
                    } catch (e) {
                        warnOnce("Blob constructor present but fails: " + e + "; falling back to blob builder");
                    }
                }
                if (!b) {
                    var bb = new Browser.BlobBuilder;
                    bb.append((new Uint8Array(byteArray)).buffer);
                    b = bb.getBlob();
                }
                var url = Browser.URLObject.createObjectURL(b);
                assert(typeof url == "string", "createObjectURL must return a url as a string");
                var img = new Image;
                img.onload = function img_onload() {
                    assert(img.complete, "Image " + name + " could not be decoded");
                    var canvas = document.createElement("canvas");
                    canvas.width = img.width;
                    canvas.height = img.height;
                    var ctx = canvas.getContext("2d");
                    ctx.drawImage(img, 0, 0);
                    Module["preloadedImages"][name] = canvas;
                    Browser.URLObject.revokeObjectURL(url);
                    if (onload) onload(byteArray);
                };
                img.onerror = function img_onerror(event) {
                    console.log("Image " + url + " could not be decoded");
                    if (onerror) onerror();
                };
                img.src = url;
            };
            Module["preloadPlugins"].push(imagePlugin);
            var audioPlugin = {};
            audioPlugin["canHandle"] = function audioPlugin_canHandle(name) {
                return !Module.noAudioDecoding && name.substr(-4) in {
                    ".ogg": 1,
                    ".wav": 1,
                    ".mp3": 1
                };
            };
            audioPlugin["handle"] = function audioPlugin_handle(byteArray, name, onload, onerror) {
                var done = false;
                function finish(audio) {
                    if (done) return;
                    done = true;
                    Module["preloadedAudios"][name] = audio;
                    if (onload) onload(byteArray);
                }
                function fail() {
                    if (done) return;
                    done = true;
                    Module["preloadedAudios"][name] = new Audio;
                    if (onerror) onerror();
                }
                if (Browser.hasBlobConstructor) {
                    try {
                        var b = new Blob([ byteArray ], {
                            type: Browser.getMimetype(name)
                        });
                    } catch (e) {
                        return fail();
                    }
                    var url = Browser.URLObject.createObjectURL(b);
                    assert(typeof url == "string", "createObjectURL must return a url as a string");
                    var audio = new Audio;
                    audio.addEventListener("canplaythrough", (function() {
                        finish(audio);
                    }), false);
                    audio.onerror = function audio_onerror(event) {
                        if (done) return;
                        console.log("warning: browser could not fully decode audio " + name + ", trying slower base64 approach");
                        function encode64(data) {
                            var BASE = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
                            var PAD = "=";
                            var ret = "";
                            var leftchar = 0;
                            var leftbits = 0;
                            for (var i = 0; i < data.length; i++) {
                                leftchar = leftchar << 8 | data[i];
                                leftbits += 8;
                                while (leftbits >= 6) {
                                    var curr = leftchar >> leftbits - 6 & 63;
                                    leftbits -= 6;
                                    ret += BASE[curr];
                                }
                            }
                            if (leftbits == 2) {
                                ret += BASE[(leftchar & 3) << 4];
                                ret += PAD + PAD;
                            } else if (leftbits == 4) {
                                ret += BASE[(leftchar & 15) << 2];
                                ret += PAD;
                            }
                            return ret;
                        }
                        audio.src = "data:audio/x-" + name.substr(-3) + ";base64," + encode64(byteArray);
                        finish(audio);
                    };
                    audio.src = url;
                    Browser.safeSetTimeout((function() {
                        finish(audio);
                    }), 1e4);
                } else {
                    return fail();
                }
            };
            Module["preloadPlugins"].push(audioPlugin);
            function pointerLockChange() {
                Browser.pointerLock = document["pointerLockElement"] === Module["canvas"] || document["mozPointerLockElement"] === Module["canvas"] || document["webkitPointerLockElement"] === Module["canvas"] || document["msPointerLockElement"] === Module["canvas"];
            }
            var canvas = Module["canvas"];
            if (canvas) {
                canvas.requestPointerLock = canvas["requestPointerLock"] || canvas["mozRequestPointerLock"] || canvas["webkitRequestPointerLock"] || canvas["msRequestPointerLock"] || (function() {});
                canvas.exitPointerLock = document["exitPointerLock"] || document["mozExitPointerLock"] || document["webkitExitPointerLock"] || document["msExitPointerLock"] || (function() {});
                canvas.exitPointerLock = canvas.exitPointerLock.bind(document);
                document.addEventListener("pointerlockchange", pointerLockChange, false);
                document.addEventListener("mozpointerlockchange", pointerLockChange, false);
                document.addEventListener("webkitpointerlockchange", pointerLockChange, false);
                document.addEventListener("mspointerlockchange", pointerLockChange, false);
                if (Module["elementPointerLock"]) {
                    canvas.addEventListener("click", (function(ev) {
                        if (!Browser.pointerLock && Module["canvas"].requestPointerLock) {
                            Module["canvas"].requestPointerLock();
                            ev.preventDefault();
                        }
                    }), false);
                }
            }
        }),
        createContext: (function(canvas, useWebGL, setInModule, webGLContextAttributes) {
            if (useWebGL && Module.ctx && canvas == Module.canvas) return Module.ctx;
            var ctx;
            var contextHandle;
            if (useWebGL) {
                var contextAttributes = {
                    antialias: false,
                    alpha: false
                };
                if (webGLContextAttributes) {
                    for (var attribute in webGLContextAttributes) {
                        contextAttributes[attribute] = webGLContextAttributes[attribute];
                    }
                }
                contextHandle = GL.createContext(canvas, contextAttributes);
                if (contextHandle) {
                    ctx = GL.getContext(contextHandle).GLctx;
                }
            } else {
                ctx = canvas.getContext("2d");
            }
            if (!ctx) return null;
            if (setInModule) {
                if (!useWebGL) assert(typeof GLctx === "undefined", "cannot set in module if GLctx is used, but we are a non-GL context that would replace it");
                Module.ctx = ctx;
                if (useWebGL) GL.makeContextCurrent(contextHandle);
                Module.useWebGL = useWebGL;
                Browser.moduleContextCreatedCallbacks.forEach((function(callback) {
                    callback();
                }));
                Browser.init();
            }
            return ctx;
        }),
        destroyContext: (function(canvas, useWebGL, setInModule) {}),
        fullscreenHandlersInstalled: false,
        lockPointer: undefined,
        resizeCanvas: undefined,
        requestFullscreen: (function(lockPointer, resizeCanvas, vrDevice) {
            Browser.lockPointer = lockPointer;
            Browser.resizeCanvas = resizeCanvas;
            Browser.vrDevice = vrDevice;
            if (typeof Browser.lockPointer === "undefined") Browser.lockPointer = true;
            if (typeof Browser.resizeCanvas === "undefined") Browser.resizeCanvas = false;
            if (typeof Browser.vrDevice === "undefined") Browser.vrDevice = null;
            var canvas = Module["canvas"];
            function fullscreenChange() {
                Browser.isFullscreen = false;
                var canvasContainer = canvas.parentNode;
                if ((document["fullscreenElement"] || document["mozFullScreenElement"] || document["msFullscreenElement"] || document["webkitFullscreenElement"] || document["webkitCurrentFullScreenElement"]) === canvasContainer) {
                    canvas.exitFullscreen = document["exitFullscreen"] || document["cancelFullScreen"] || document["mozCancelFullScreen"] || document["msExitFullscreen"] || document["webkitCancelFullScreen"] || (function() {});
                    canvas.exitFullscreen = canvas.exitFullscreen.bind(document);
                    if (Browser.lockPointer) canvas.requestPointerLock();
                    Browser.isFullscreen = true;
                    if (Browser.resizeCanvas) {
                        Browser.setFullscreenCanvasSize();
                    } else {
                        Browser.updateCanvasDimensions(canvas);
                    }
                } else {
                    canvasContainer.parentNode.insertBefore(canvas, canvasContainer);
                    canvasContainer.parentNode.removeChild(canvasContainer);
                    if (Browser.resizeCanvas) {
                        Browser.setWindowedCanvasSize();
                    } else {
                        Browser.updateCanvasDimensions(canvas);
                    }
                }
                if (Module["onFullScreen"]) Module["onFullScreen"](Browser.isFullscreen);
                if (Module["onFullscreen"]) Module["onFullscreen"](Browser.isFullscreen);
                Browser.updateCanvasDimensions(canvas);
            }
            if (!Browser.fullscreenHandlersInstalled) {
                Browser.fullscreenHandlersInstalled = true;
                document.addEventListener("fullscreenchange", fullscreenChange, false);
                document.addEventListener("mozfullscreenchange", fullscreenChange, false);
                document.addEventListener("webkitfullscreenchange", fullscreenChange, false);
                document.addEventListener("MSFullscreenChange", fullscreenChange, false);
            }
            var canvasContainer = document.createElement("div");
            canvas.parentNode.insertBefore(canvasContainer, canvas);
            canvasContainer.appendChild(canvas);
            canvasContainer.requestFullscreen = canvasContainer["requestFullscreen"] || canvasContainer["mozRequestFullScreen"] || canvasContainer["msRequestFullscreen"] || (canvasContainer["webkitRequestFullscreen"] ? (function() {
                canvasContainer["webkitRequestFullscreen"](Element["ALLOW_KEYBOARD_INPUT"]);
            }) : null) || (canvasContainer["webkitRequestFullScreen"] ? (function() {
                canvasContainer["webkitRequestFullScreen"](Element["ALLOW_KEYBOARD_INPUT"]);
            }) : null);
            if (vrDevice) {
                canvasContainer.requestFullscreen({
                    vrDisplay: vrDevice
                });
            } else {
                canvasContainer.requestFullscreen();
            }
        }),
        requestFullScreen: (function(lockPointer, resizeCanvas, vrDevice) {
            err("Browser.requestFullScreen() is deprecated. Please call Browser.requestFullscreen instead.");
            Browser.requestFullScreen = (function(lockPointer, resizeCanvas, vrDevice) {
                return Browser.requestFullscreen(lockPointer, resizeCanvas, vrDevice);
            });
            return Browser.requestFullscreen(lockPointer, resizeCanvas, vrDevice);
        }),
        nextRAF: 0,
        fakeRequestAnimationFrame: (function(func) {
            var now = Date.now();
            if (Browser.nextRAF === 0) {
                Browser.nextRAF = now + 1e3 / 60;
            } else {
                while (now + 2 >= Browser.nextRAF) {
                    Browser.nextRAF += 1e3 / 60;
                }
            }
            var delay = Math.max(Browser.nextRAF - now, 0);
            setTimeout(func, delay);
        }),
        requestAnimationFrame: function requestAnimationFrame(func) {
            if (typeof window === "undefined") {
                Browser.fakeRequestAnimationFrame(func);
            } else {
                if (!window.requestAnimationFrame) {
                    window.requestAnimationFrame = window["requestAnimationFrame"] || window["mozRequestAnimationFrame"] || window["webkitRequestAnimationFrame"] || window["msRequestAnimationFrame"] || window["oRequestAnimationFrame"] || Browser.fakeRequestAnimationFrame;
                }
                window.requestAnimationFrame(func);
            }
        },
        safeCallback: (function(func) {
            return (function() {
                if (!ABORT) return func.apply(null, arguments);
            });
        }),
        allowAsyncCallbacks: true,
        queuedAsyncCallbacks: [],
        pauseAsyncCallbacks: (function() {
            Browser.allowAsyncCallbacks = false;
        }),
        resumeAsyncCallbacks: (function() {
            Browser.allowAsyncCallbacks = true;
            if (Browser.queuedAsyncCallbacks.length > 0) {
                var callbacks = Browser.queuedAsyncCallbacks;
                Browser.queuedAsyncCallbacks = [];
                callbacks.forEach((function(func) {
                    func();
                }));
            }
        }),
        safeRequestAnimationFrame: (function(func) {
            return Browser.requestAnimationFrame((function() {
                if (ABORT) return;
                if (Browser.allowAsyncCallbacks) {
                    func();
                } else {
                    Browser.queuedAsyncCallbacks.push(func);
                }
            }));
        }),
        safeSetTimeout: (function(func, timeout) {
            Module["noExitRuntime"] = true;
            return setTimeout((function() {
                if (ABORT) return;
                if (Browser.allowAsyncCallbacks) {
                    func();
                } else {
                    Browser.queuedAsyncCallbacks.push(func);
                }
            }), timeout);
        }),
        safeSetInterval: (function(func, timeout) {
            Module["noExitRuntime"] = true;
            return setInterval((function() {
                if (ABORT) return;
                if (Browser.allowAsyncCallbacks) {
                    func();
                }
            }), timeout);
        }),
        getMimetype: (function(name) {
            return {
                "jpg": "image/jpeg",
                "jpeg": "image/jpeg",
                "png": "image/png",
                "bmp": "image/bmp",
                "ogg": "audio/ogg",
                "wav": "audio/wav",
                "mp3": "audio/mpeg"
            }[name.substr(name.lastIndexOf(".") + 1)];
        }),
        getUserMedia: (function(func) {
            if (!window.getUserMedia) {
                window.getUserMedia = navigator["getUserMedia"] || navigator["mozGetUserMedia"];
            }
            window.getUserMedia(func);
        }),
        getMovementX: (function(event) {
            return event["movementX"] || event["mozMovementX"] || event["webkitMovementX"] || 0;
        }),
        getMovementY: (function(event) {
            return event["movementY"] || event["mozMovementY"] || event["webkitMovementY"] || 0;
        }),
        getMouseWheelDelta: (function(event) {
            var delta = 0;
            switch (event.type) {
                case "DOMMouseScroll":
                    delta = event.detail;
                    break;
                case "mousewheel":
                    delta = event.wheelDelta;
                    break;
                case "wheel":
                    delta = event["deltaY"];
                    break;
                default:
                    throw "unrecognized mouse wheel event: " + event.type;
            }
            return delta;
        }),
        mouseX: 0,
        mouseY: 0,
        mouseMovementX: 0,
        mouseMovementY: 0,
        touches: {},
        lastTouches: {},
        calculateMouseEvent: (function(event) {
            if (Browser.pointerLock) {
                if (event.type != "mousemove" && "mozMovementX" in event) {
                    Browser.mouseMovementX = Browser.mouseMovementY = 0;
                } else {
                    Browser.mouseMovementX = Browser.getMovementX(event);
                    Browser.mouseMovementY = Browser.getMovementY(event);
                }
                if (typeof SDL != "undefined") {
                    Browser.mouseX = SDL.mouseX + Browser.mouseMovementX;
                    Browser.mouseY = SDL.mouseY + Browser.mouseMovementY;
                } else {
                    Browser.mouseX += Browser.mouseMovementX;
                    Browser.mouseY += Browser.mouseMovementY;
                }
            } else {
                var rect = Module["canvas"].getBoundingClientRect();
                var cw = Module["canvas"].width;
                var ch = Module["canvas"].height;
                var scrollX = typeof window.scrollX !== "undefined" ? window.scrollX : window.pageXOffset;
                var scrollY = typeof window.scrollY !== "undefined" ? window.scrollY : window.pageYOffset;
                assert(typeof scrollX !== "undefined" && typeof scrollY !== "undefined", "Unable to retrieve scroll position, mouse positions likely broken.");
                if (event.type === "touchstart" || event.type === "touchend" || event.type === "touchmove") {
                    var touch = event.touch;
                    if (touch === undefined) {
                        return;
                    }
                    var adjustedX = touch.pageX - (scrollX + rect.left);
                    var adjustedY = touch.pageY - (scrollY + rect.top);
                    adjustedX = adjustedX * (cw / rect.width);
                    adjustedY = adjustedY * (ch / rect.height);
                    var coords = {
                        x: adjustedX,
                        y: adjustedY
                    };
                    if (event.type === "touchstart") {
                        Browser.lastTouches[touch.identifier] = coords;
                        Browser.touches[touch.identifier] = coords;
                    } else if (event.type === "touchend" || event.type === "touchmove") {
                        var last = Browser.touches[touch.identifier];
                        if (!last) last = coords;
                        Browser.lastTouches[touch.identifier] = last;
                        Browser.touches[touch.identifier] = coords;
                    }
                    return;
                }
                var x = event.pageX - (scrollX + rect.left);
                var y = event.pageY - (scrollY + rect.top);
                x = x * (cw / rect.width);
                y = y * (ch / rect.height);
                Browser.mouseMovementX = x - Browser.mouseX;
                Browser.mouseMovementY = y - Browser.mouseY;
                Browser.mouseX = x;
                Browser.mouseY = y;
            }
        }),
        asyncLoad: (function(url, onload, onerror, noRunDep) {
            var dep = !noRunDep ? getUniqueRunDependency("al " + url) : "";
            Module["readAsync"](url, (function(arrayBuffer) {
                assert(arrayBuffer, 'Loading data file "' + url + '" failed (no arrayBuffer).');
                onload(new Uint8Array(arrayBuffer));
                if (dep) removeRunDependency(dep);
            }), (function(event) {
                if (onerror) {
                    onerror();
                } else {
                    throw 'Loading data file "' + url + '" failed.';
                }
            }));
            if (dep) addRunDependency(dep);
        }),
        resizeListeners: [],
        updateResizeListeners: (function() {
            var canvas = Module["canvas"];
            Browser.resizeListeners.forEach((function(listener) {
                listener(canvas.width, canvas.height);
            }));
        }),
        setCanvasSize: (function(width, height, noUpdates) {
            var canvas = Module["canvas"];
            Browser.updateCanvasDimensions(canvas, width, height);
            if (!noUpdates) Browser.updateResizeListeners();
        }),
        windowedWidth: 0,
        windowedHeight: 0,
        setFullscreenCanvasSize: (function() {
            if (typeof SDL != "undefined") {
                var flags = HEAPU32[SDL.screen >> 2];
                flags = flags | 8388608;
                HEAP32[SDL.screen >> 2] = flags;
            }
            Browser.updateCanvasDimensions(Module['canvas']);
            Browser.updateResizeListeners();
        }),
        setWindowedCanvasSize: (function() {
            if (typeof SDL != "undefined") {
                var flags = HEAPU32[SDL.screen >> 2];
                flags = flags & ~8388608;
                HEAP32[SDL.screen >> 2] = flags;
            }
            Browser.updateCanvasDimensions(Module['canvas']);
            Browser.updateResizeListeners();
        }),
        updateCanvasDimensions: (function (canvas, wNative, hNative) {
            if (wNative && hNative) {
                canvas.widthNative = wNative;
                canvas.heightNative = hNative;
            } else {
                wNative = canvas.widthNative;
                hNative = canvas.heightNative;
            }
            var w = wNative;
            var h = hNative;
            if (Module["forcedAspectRatio"] && Module["forcedAspectRatio"] > 0) {
                if (w / h < Module["forcedAspectRatio"]) {
                    w = Math.round(h * Module["forcedAspectRatio"]);
                } else {
                    h = Math.round(w / Module["forcedAspectRatio"]);
                }
            }
            //////////// Modification Start ////////////////////
            var dpr = window.devicePixelRatio;
            if ((document["fullscreenElement"] || document["mozFullScreenElement"] || document["msFullscreenElement"] || document["webkitFullscreenElement"] || document["webkitCurrentFullScreenElement"]) === canvas.parentNode && typeof screen != "undefined") {
                var factor = Math.min((screen.width * dpr) / w, (screen.height * dpr) / h);
                w = Math.round(w * factor);
                h = Math.round(h * factor);
            }
            if (Browser.resizeCanvas) {
                if (canvas.width != w) canvas.width = w;
                if (canvas.height != h) canvas.height = h;
                if (typeof canvas.style != "undefined") {
                    canvas.style.removeProperty("width");
                    canvas.style.removeProperty("height");
                }
            } else {
                if (canvas.width != wNative) canvas.width = wNative;
                if (canvas.height != hNative) canvas.height = hNative;
                if (typeof canvas.style != "undefined") {
                    if (!canvas.style.getPropertyValue("width").includes("%")) canvas.style.setProperty("width", (w / dpr) + "px", "important");
                    if (!canvas.style.getPropertyValue("height").includes("%")) canvas.style.setProperty("height", (h / dpr) + "px", "important");
                }
            }
            ///////////////// Modification End ///////////////////
        }),
        wgetRequests: {},
        nextWgetRequestHandle: 0,
        getNextWgetRequestHandle: (function() {
            var handle = Browser.nextWgetRequestHandle;
            Browser.nextWgetRequestHandle++;
            return handle;
        })
    },
    $JSEvents : {
        keyEvent: 0,
        mouseEvent: 0,
        wheelEvent: 0,
        uiEvent: 0,
        focusEvent: 0,
        deviceOrientationEvent: 0,
        deviceMotionEvent: 0,
        fullscreenChangeEvent: 0,
        pointerlockChangeEvent: 0,
        visibilityChangeEvent: 0,
        touchEvent: 0,
        lastGamepadState: null,
        lastGamepadStateFrame: null,
        numGamepadsConnected: 0,
        previousFullscreenElement: null,
        previousScreenX: null,
        previousScreenY: null,
        removeEventListenersRegistered: false,

        _onGamepadConnected: function() { ++JSEvents.numGamepadsConnected; },
        _onGamepadDisconnected: function() { --JSEvents.numGamepadsConnected; },
        
        staticInit: (function() {
            if (typeof window !== "undefined") {
                window.addEventListener("gamepadconnected", JSEvents._onGamepadConnected);
                window.addEventListener("gamepaddisconnected", JSEvents._onGamepadDisconnected);
                var firstState = navigator.getGamepads ? navigator.getGamepads() : navigator.webkitGetGamepads ? navigator.webkitGetGamepads() : null;
                if (firstState) {
                    JSEvents.numGamepadsConnected = firstState.length;
                }
            }
        }),

        removeAllEventListeners: function() {
            for(var i = JSEvents.eventHandlers.length-1; i >= 0; --i) {
                JSEvents._removeHandler(i);
            }
            JSEvents.eventHandlers = [];
            JSEvents.deferredCalls = [];
            window.removeEventListener("gamepadconnected", JSEvents._onGamepadConnected);
            window.removeEventListener("gamepaddisconnected", JSEvents._onGamepadDisconnected);
        },
        
        registerRemoveEventListeners: (function() {
            if (!JSEvents.removeEventListenersRegistered) {
                __ATEXIT__.push(JSEvents.removeAllEventListeners);
                JSEvents.removeEventListenersRegistered = true;
            }
        }),
        findEventTarget: function(target) {
            try {
                if (!target) return window;
                if (typeof target === "number") target = Pointer_stringify(target);
                if (target === '#window') return window;
                else if (target === '#document') return document;
                else if (target === '#screen') return window.screen;
                else if (target === '#canvas') return Module['canvas'];
                return (typeof target === 'string') ? document.getElementById(target) : target;
            } catch(e) {
                return null;
            }
        },

        findCanvasEventTarget : function(target) {
            if (typeof target === 'number') target = Pointer_stringify(target);
            if (!target || target === '#canvas') {
                if (typeof GL !== 'undefined' && GL.offscreenCanvases['canvas']) return GL.offscreenCanvases['canvas']; // TODO: Remove this line, target '#canvas' should refer only to Module['canvas'], not to GL.offscreenCanvases['canvas'] - but need stricter tests to be able to remove this line.
                return Module['canvas'];
            }
            if (typeof GL !== 'undefined' && GL.offscreenCanvases[target]) return GL.offscreenCanvases[target];
            return JSEvents.findEventTarget(target);
        },

        deferredCalls: [],
        deferCall: (function(targetFunction, precedence, argsList) {
            function arraysHaveEqualContent(arrA, arrB) {
                if (arrA.length != arrB.length) return false;
                for (var i in arrA) {
                    if (arrA[i] != arrB[i]) return false;
                }
                return true;
            }
            for (var i in JSEvents.deferredCalls) {
                var call = JSEvents.deferredCalls[i];
                if (call.targetFunction == targetFunction && arraysHaveEqualContent(call.argsList, argsList)) {
                    return;
                }
            }
            JSEvents.deferredCalls.push({
                targetFunction: targetFunction,
                precedence: precedence,
                argsList: argsList
            });
            JSEvents.deferredCalls.sort((function(x, y) {
                return x.precedence < y.precedence;
            }));
        }),
        removeDeferredCalls: (function(targetFunction) {
            for (var i = 0; i < JSEvents.deferredCalls.length; ++i) {
                if (JSEvents.deferredCalls[i].targetFunction == targetFunction) {
                    JSEvents.deferredCalls.splice(i, 1);
                    --i;
                }
            }
        }),
        canPerformEventHandlerRequests: (function() {
            return JSEvents.inEventHandler && JSEvents.currentEventHandler.allowsDeferredCalls;
        }),
        runDeferredCalls: (function() {
            if (!JSEvents.canPerformEventHandlerRequests()) {
                return;
            }
            for (var i = 0; i < JSEvents.deferredCalls.length; ++i) {
                var call = JSEvents.deferredCalls[i];
                JSEvents.deferredCalls.splice(i, 1);
                --i;
                call.targetFunction.apply(this, call.argsList);
            }
        }),
        inEventHandler: 0,
        currentEventHandler: null,
        eventHandlers: [],
        isInternetExplorer: (function() {
            return navigator.userAgent.indexOf("MSIE") !== -1 || navigator.appVersion.indexOf("Trident/") > 0;
        }),
        removeAllHandlersOnTarget: (function(target, eventTypeString) {
            for (var i = 0; i < JSEvents.eventHandlers.length; ++i) {
                if (JSEvents.eventHandlers[i].target == target && (!eventTypeString || eventTypeString == JSEvents.eventHandlers[i].eventTypeString)) {
                    JSEvents._removeHandler(i--);
                }
            }
        }),
        _removeHandler: (function(i) {
            var h = JSEvents.eventHandlers[i];
            h.target.removeEventListener(h.eventTypeString, h.eventListenerFunc, h.useCapture);
            JSEvents.eventHandlers.splice(i, 1);
        }),
        registerOrRemoveHandler: (function(eventHandler) {
            var jsEventHandler = function jsEventHandler(event) {
                ++JSEvents.inEventHandler;
                JSEvents.currentEventHandler = eventHandler;
                JSEvents.runDeferredCalls();
                eventHandler.handlerFunc(event);
                JSEvents.runDeferredCalls();
                --JSEvents.inEventHandler;
            };
            if (eventHandler.callbackfunc) {
                eventHandler.eventListenerFunc = jsEventHandler;
                eventHandler.target.addEventListener(eventHandler.eventTypeString, jsEventHandler, eventHandler.useCapture);
                JSEvents.eventHandlers.push(eventHandler);
                JSEvents.registerRemoveEventListeners();
            } else {
                for (var i = 0; i < JSEvents.eventHandlers.length; ++i) {
                    if (JSEvents.eventHandlers[i].target == eventHandler.target && JSEvents.eventHandlers[i].eventTypeString == eventHandler.eventTypeString) {
                        JSEvents._removeHandler(i--);
                    }
                }
            }
        }),
        registerKeyEventCallback: function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.keyEvent) {
                JSEvents.keyEvent = _malloc(164);
            }
            var keyEventHandlerFunc = function(event) {
                var e = event || window.event;
                var keyEventData = JSEvents.keyEvent;
                stringToUTF8(e.key ? e.key : "", keyEventData + 0, 32);
                stringToUTF8(e.code ? e.code : "", keyEventData + 32, 32);
                HEAP32[keyEventData + 64 >> 2] = e.location;
                HEAP32[keyEventData + 68 >> 2] = e.ctrlKey;
                HEAP32[keyEventData + 72 >> 2] = e.shiftKey;
                HEAP32[keyEventData + 76 >> 2] = e.altKey;
                HEAP32[keyEventData + 80 >> 2] = e.metaKey;
                HEAP32[keyEventData + 84 >> 2] = e.repeat;
                stringToUTF8(e.locale ? e.locale : "", keyEventData + 88, 32);
                stringToUTF8(e.char ? e.char : "", keyEventData + 120, 32);
                HEAP32[keyEventData + 152 >> 2] = e.charCode;
                HEAP32[keyEventData + 156 >> 2] = e.keyCode;
                HEAP32[keyEventData + 160 >> 2] = e.which;
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, keyEventData, userData)) e.preventDefault();
            };
            var eventHandler = {
                target: JSEvents.findEventTarget(target),
                allowsDeferredCalls: JSEvents.isInternetExplorer() ? false : true,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: keyEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        },
        
        getBoundingClientRectOrZeros: (function(target) {
            return target.getBoundingClientRect ? target.getBoundingClientRect() : {
                left: 0,
                top: 0
            };
        }),
        fillMouseEventData: (function (eventStruct, e, target) {
            ///////////////// Modification Start ///////////////////////
            var devicePixelRatio = window.devicePixelRatio;
            HEAPF64[eventStruct >> 3] = JSEvents.tick();
            HEAP32[eventStruct + 8 >> 2] = e.screenX * devicePixelRatio;
            HEAP32[eventStruct + 12 >> 2] = e.screenY * devicePixelRatio;
            HEAP32[eventStruct + 16 >> 2] = e.clientX * devicePixelRatio;
            HEAP32[eventStruct + 20 >> 2] = e.clientY * devicePixelRatio;
            HEAP32[eventStruct + 24 >> 2] = e.ctrlKey;
            HEAP32[eventStruct + 28 >> 2] = e.shiftKey;
            HEAP32[eventStruct + 32 >> 2] = e.altKey;
            HEAP32[eventStruct + 36 >> 2] = e.metaKey;
            HEAP16[eventStruct + 40 >> 1] = e.button;
            HEAP16[eventStruct + 42 >> 1] = e.buttons;
            HEAP32[eventStruct + 44 >> 2] = e["movementX"] || e["mozMovementX"] || e["webkitMovementX"] || (e.screenX * devicePixelRatio) - JSEvents.previousScreenX;
            HEAP32[eventStruct + 48 >> 2] = e["movementY"] || e["mozMovementY"] || e["webkitMovementY"] || (e.screenY * devicePixelRatio) - JSEvents.previousScreenY;
            if (Module["canvas"]) {
                var rect = Module["canvas"].getBoundingClientRect();
                HEAP32[eventStruct + 60 >> 2] = (e.clientX - rect.left) * devicePixelRatio;
                HEAP32[eventStruct + 64 >> 2] = (e.clientY - rect.top) * devicePixelRatio;
            } else {
                HEAP32[eventStruct + 60 >> 2] = 0;
                HEAP32[eventStruct + 64 >> 2] = 0;
            }
            if (target) {
                var rect = JSEvents.getBoundingClientRectOrZeros(target);
                HEAP32[eventStruct + 52 >> 2] = (e.clientX - rect.left) * devicePixelRatio;
                HEAP32[eventStruct + 56 >> 2] = (e.clientY - rect.top) * devicePixelRatio;
            } else {
                HEAP32[eventStruct + 52 >> 2] = 0;
                HEAP32[eventStruct + 56 >> 2] = 0;
            }
            if (e.type !== "wheel" && e.type !== "mousewheel") {
                JSEvents.previousScreenX = e.screenX * devicePixelRatio;
                JSEvents.previousScreenY = e.screenY * devicePixelRatio;
            }
            ////////////// Modification End //////////////////////////////
        }),
        registerMouseEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString) {
            if (!JSEvents.mouseEvent) {
                JSEvents.mouseEvent = _malloc(72);
            }
            target = JSEvents.findEventTarget(target);
            var mouseEventHandlerFunc = (function(event) {
                var e = event || window.event;
                JSEvents.fillMouseEventData(JSEvents.mouseEvent, e, target);
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, JSEvents.mouseEvent, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: target,
                allowsDeferredCalls: eventTypeString != "mousemove" && eventTypeString != "mouseenter" && eventTypeString != "mouseleave",
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: mouseEventHandlerFunc,
                useCapture: useCapture
            };
            if (JSEvents.isInternetExplorer() && eventTypeString == "mousedown") eventHandler.allowsDeferredCalls = false;
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        registerWheelEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.wheelEvent) {
                JSEvents.wheelEvent = _malloc(104);
            }
            target = JSEvents.findEventTarget(target);
            var wheelHandlerFunc = (function(event) {
                var e = event || window.event;
                var wheelEvent = JSEvents.wheelEvent;
                JSEvents.fillMouseEventData(JSEvents.wheelEvent, e, target);
                HEAPF64[wheelEvent + 72 >> 3] = e["deltaX"];
                HEAPF64[wheelEvent + 80 >> 3] = e["deltaY"];
                HEAPF64[wheelEvent + 88 >> 3] = e["deltaZ"];
                HEAP32[wheelEvent + 96 >> 2] = e["deltaMode"];
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, wheelEvent, userData)) e.preventDefault();
            });
            var mouseWheelHandlerFunc = (function(event) {
                var e = event || window.event;
                JSEvents.fillMouseEventData(JSEvents.wheelEvent, e, target);
                HEAPF64[JSEvents.wheelEvent + 72 >> 3] = e["wheelDeltaX"] || 0;
                HEAPF64[JSEvents.wheelEvent + 80 >> 3] = -(e["wheelDeltaY"] ? e["wheelDeltaY"] : e["wheelDelta"]);
                HEAPF64[JSEvents.wheelEvent + 88 >> 3] = 0;
                HEAP32[JSEvents.wheelEvent + 96 >> 2] = 0;
                var shouldCancel = Module["dynCall_iiii"](callbackfunc, eventTypeId, JSEvents.wheelEvent, userData);
                if (shouldCancel) {
                    e.preventDefault();
                }
            });
            var eventHandler = {
                target: target,
                allowsDeferredCalls: true,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: eventTypeString == "wheel" ? wheelHandlerFunc : mouseWheelHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        pageScrollPos: (function() {
            if (window.pageXOffset > 0 || window.pageYOffset > 0) {
                return [ window.pageXOffset, window.pageYOffset ];
            }
            if (typeof document.documentElement.scrollLeft !== "undefined" || typeof document.documentElement.scrollTop !== "undefined") {
                return [ document.documentElement.scrollLeft, document.documentElement.scrollTop ];
            }
            return [ document.body.scrollLeft | 0, document.body.scrollTop | 0 ];
        }),
        registerUiEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.uiEvent) {
                JSEvents.uiEvent = _malloc(36);
            }
            if (eventTypeString == "scroll" && !target) {
                target = document;
            } else {
                target = JSEvents.findEventTarget(target);
            }
            var uiEventHandlerFunc = (function(event) {
                var e = event || window.event;
                if (e.target != target) {
                    return;
                }
                var scrollPos = JSEvents.pageScrollPos();
                var uiEvent = JSEvents.uiEvent;
                HEAP32[uiEvent >> 2] = e.detail;
                HEAP32[uiEvent + 4 >> 2] = document.body.clientWidth;
                HEAP32[uiEvent + 8 >> 2] = document.body.clientHeight;
                HEAP32[uiEvent + 12 >> 2] = window.innerWidth;
                HEAP32[uiEvent + 16 >> 2] = window.innerHeight;
                HEAP32[uiEvent + 20 >> 2] = window.outerWidth;
                HEAP32[uiEvent + 24 >> 2] = window.outerHeight;
                HEAP32[uiEvent + 28 >> 2] = scrollPos[0];
                HEAP32[uiEvent + 32 >> 2] = scrollPos[1];
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, uiEvent, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: target,
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: uiEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        getNodeNameForTarget: (function(target) {
            if (!target) return "";
            if (target == window) return "#window";
            if (target == window.screen) return "#screen";
            return target && target.nodeName ? target.nodeName : "";
        }),
        registerFocusEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.focusEvent) {
                JSEvents.focusEvent = _malloc(256);
            }
            var focusEventHandlerFunc = (function(event) {
                var e = event || window.event;
                var nodeName = JSEvents.getNodeNameForTarget(e.target);
                var id = e.target.id ? e.target.id : "";
                var focusEvent = JSEvents.focusEvent;
                stringToUTF8(nodeName, focusEvent + 0, 128);
                stringToUTF8(id, focusEvent + 128, 128);
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, focusEvent, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: JSEvents.findEventTarget(target),
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: focusEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        tick: (function() {
            if (window["performance"] && window["performance"]["now"]) return window["performance"]["now"](); else return Date.now();
        }),

        fillDeviceOrientationEventData: function(eventStruct, e, target) {
            HEAPF64[JSEvents.deviceOrientationEvent >> 3] = JSEvents.tick();
            HEAPF64[JSEvents.deviceOrientationEvent + 8 >> 3] = e.alpha;
            HEAPF64[JSEvents.deviceOrientationEvent + 16 >> 3] = e.beta;
            HEAPF64[JSEvents.deviceOrientationEvent + 24 >> 3] = e.gamma;
            HEAP32[JSEvents.deviceOrientationEvent + 32 >> 2] = e.absolute;
        },
        
        registerDeviceOrientationEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.deviceOrientationEvent) {
                JSEvents.deviceOrientationEvent = _malloc(40);
            }
            var deviceOrientationEventHandlerFunc = (function(event) {
                var e = event || window.event;
                JSEvents.fillDeviceOrientationEventData(JSEvents.deviceOrientationEvent, e, target);
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, JSEvents.deviceOrientationEvent, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: JSEvents.findEventTarget(target),
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: deviceOrientationEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),

        fillDeviceMotionEventData: function(eventStruct, e, target) {
            HEAPF64[JSEvents.deviceMotionEvent >> 3] = JSEvents.tick();
            HEAPF64[JSEvents.deviceMotionEvent + 8 >> 3] = e.acceleration.x;
            HEAPF64[JSEvents.deviceMotionEvent + 16 >> 3] = e.acceleration.y;
            HEAPF64[JSEvents.deviceMotionEvent + 24 >> 3] = e.acceleration.z;
            HEAPF64[JSEvents.deviceMotionEvent + 32 >> 3] = e.accelerationIncludingGravity.x;
            HEAPF64[JSEvents.deviceMotionEvent + 40 >> 3] = e.accelerationIncludingGravity.y;
            HEAPF64[JSEvents.deviceMotionEvent + 48 >> 3] = e.accelerationIncludingGravity.z;
            HEAPF64[JSEvents.deviceMotionEvent + 56 >> 3] = e.rotationRate.alpha;
            HEAPF64[JSEvents.deviceMotionEvent + 64 >> 3] = e.rotationRate.beta;
            HEAPF64[JSEvents.deviceMotionEvent + 72 >> 3] = e.rotationRate.gamma;
        },
        
        registerDeviceMotionEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.deviceMotionEvent) {
                JSEvents.deviceMotionEvent = _malloc(80);
            }
            var deviceMotionEventHandlerFunc = (function(event) {
                var e = event || window.event;
                JSEvents.fillDeviceMotionEventData(JSEvents.deviceMotionEvent, e, target);
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, JSEvents.deviceMotionEvent, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: JSEvents.findEventTarget(target),
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: deviceMotionEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        screenOrientation: (function() {
            if (!window.screen) return undefined;
            return window.screen.orientation || window.screen.mozOrientation || window.screen.webkitOrientation || window.screen.msOrientation;
        }),
        fillOrientationChangeEventData: (function(eventStruct, e) {
            var orientations = [ "portrait-primary", "portrait-secondary", "landscape-primary", "landscape-secondary" ];
            var orientations2 = [ "portrait", "portrait", "landscape", "landscape" ];
            var orientationString = JSEvents.screenOrientation();
            var orientation = orientations.indexOf(orientationString);
            if (orientation == -1) {
                orientation = orientations2.indexOf(orientationString);
            }
            HEAP32[eventStruct >> 2] = 1 << orientation;
            HEAP32[eventStruct + 4 >> 2] = window.orientation;
        }),
        registerOrientationChangeEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.orientationChangeEvent) {
                JSEvents.orientationChangeEvent = _malloc(8);
            }
            if (!target) {
                target = window.screen;
            } else {
                target = JSEvents.findEventTarget(target);
            }
            var orientationChangeEventHandlerFunc = (function(event) {
                var e = event || window.event;
                var orientationChangeEvent = JSEvents.orientationChangeEvent;
                JSEvents.fillOrientationChangeEventData(orientationChangeEvent, e);
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, orientationChangeEvent, userData)) e.preventDefault();
            });
            if (eventTypeString == "orientationchange" && window.screen.mozOrientation !== undefined) {
                eventTypeString = "mozorientationchange";
            }
            var eventHandler = {
                target: target,
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: orientationChangeEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        fullscreenEnabled: (function() {
            return document.fullscreenEnabled || document.mozFullScreenEnabled || document.webkitFullscreenEnabled || document.msFullscreenEnabled;
        }),
        fillFullscreenChangeEventData: (function (eventStruct, e) {
            var fullscreenElement = document.fullscreenElement || document.mozFullScreenElement || document.webkitFullscreenElement || document.msFullscreenElement;
            var isFullscreen = !!fullscreenElement;
            HEAP32[eventStruct >> 2] = isFullscreen;
            HEAP32[eventStruct + 4 >> 2] = JSEvents.fullscreenEnabled();
            var reportedElement = isFullscreen ? fullscreenElement : JSEvents.previousFullscreenElement;
            var nodeName = JSEvents.getNodeNameForTarget(reportedElement);
            var id = reportedElement && reportedElement.id ? reportedElement.id : "";
            stringToUTF8(nodeName, eventStruct + 8, 128);
            stringToUTF8(id, eventStruct + 136, 128);
            HEAP32[eventStruct + 264 >> 2] = reportedElement ? reportedElement.clientWidth : 0;
            HEAP32[eventStruct + 268 >> 2] = reportedElement ? reportedElement.clientHeight : 0;
            //////////////////// Modification Start ///////////////////////////////
            HEAP32[eventStruct + 272 >> 2] = screen.width * window.devicePixelRatio;
            HEAP32[eventStruct + 276 >> 2] = screen.height * window.devicePixelRatio;
            //////////////////// Modification End ///////////////////////////////
            if (isFullscreen) {
                JSEvents.previousFullscreenElement = fullscreenElement;
            }
        }),
        registerFullscreenChangeEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.fullscreenChangeEvent) {
                JSEvents.fullscreenChangeEvent = _malloc(280);
            }
            if (!target) {
                target = document;
            } else {
                target = JSEvents.findEventTarget(target);
            }
            var fullscreenChangeEventhandlerFunc = (function(event) {
                var e = event || window.event;
                var fullscreenChangeEvent = JSEvents.fullscreenChangeEvent;
                JSEvents.fillFullscreenChangeEventData(fullscreenChangeEvent, e);
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, fullscreenChangeEvent, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: target,
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: fullscreenChangeEventhandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        resizeCanvasForFullscreen: (function(target, strategy) {
            var restoreOldStyle = __registerRestoreOldStyle(target);
            var cssWidth = strategy.softFullscreen ? window.innerWidth : screen.width;
            var cssHeight = strategy.softFullscreen ? window.innerHeight : screen.height;
            var rect = target.getBoundingClientRect();
            var windowedCssWidth = rect.right - rect.left;
            var windowedCssHeight = rect.bottom - rect.top;
            var canvasSize = emscripten_get_canvas_element_size_js(target.id);
            var windowedRttWidth = canvasSize[0]; // target.width
            var windowedRttHeight = canvasSize[1]; // target.height
            
            if (strategy.scaleMode == 3) {
                __setLetterbox(target, (cssHeight - windowedCssHeight) / 2, (cssWidth - windowedCssWidth) / 2);
                cssWidth = windowedCssWidth;
                cssHeight = windowedCssHeight;
            } else if (strategy.scaleMode == 2) {
                if (cssWidth * windowedRttHeight < windowedRttWidth * cssHeight) {
                    var desiredCssHeight = windowedRttHeight * cssWidth / windowedRttWidth;
                    __setLetterbox(target, (cssHeight - desiredCssHeight) / 2, 0);
                    cssHeight = desiredCssHeight;
                } else {
                    var desiredCssWidth = windowedRttWidth * cssHeight / windowedRttHeight;
                    __setLetterbox(target, 0, (cssWidth - desiredCssWidth) / 2);
                    cssWidth = desiredCssWidth;
                }
            }
            if (!target.style.backgroundColor) target.style.backgroundColor = "black";
            if (!document.body.style.backgroundColor) document.body.style.backgroundColor = "black";
            target.style.width = cssWidth + "px";
            target.style.height = cssHeight + "px";
            if (strategy.filteringMode == 1) {
                target.style.imageRendering = "optimizeSpeed";
                target.style.imageRendering = "-moz-crisp-edges";
                target.style.imageRendering = "-o-crisp-edges";
                target.style.imageRendering = "-webkit-optimize-contrast";
                target.style.imageRendering = "optimize-contrast";
                target.style.imageRendering = "crisp-edges";
                target.style.imageRendering = "pixelated";
            }
            var dpiScale = strategy.canvasResolutionScaleMode == 2 ? window.devicePixelRatio : 1;
            if (strategy.canvasResolutionScaleMode != 0) {
                var newWidth = (cssWidth * dpiScale)|0;
                var newHeight = (cssHeight * dpiScale)|0;

                if (!target.controlTransferredOffscreen) {
                    target.width = newWidth;
                    target.height = newHeight;
                } else {
                    emscripten_set_canvas_element_size_js(target.id, newWidth, newHeight);
                }
                if (target.GLctxObject) target.GLctxObject.GLctx.viewport(0, 0, newWidth, newHeight);
            }
            return restoreOldStyle;
        }),
        requestFullscreen: (function(target, strategy) {
            if (strategy.scaleMode != 0 || strategy.canvasResolutionScaleMode != 0) {
                JSEvents.resizeCanvasForFullscreen(target, strategy);
            }
            if (target.requestFullscreen) {
                target.requestFullscreen();
            } else if (target.msRequestFullscreen) {
                target.msRequestFullscreen();
            } else if (target.mozRequestFullScreen) {
                target.mozRequestFullScreen();
            } else if (target.mozRequestFullscreen) {
                target.mozRequestFullscreen();
            } else if (target.webkitRequestFullscreen) {
                target.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
            } else {
                if (typeof JSEvents.fullscreenEnabled() === "undefined") {
                    return -1;
                } else {
                    return -3;
                }
            }
            if (strategy.canvasResizedCallback) {
                Module["dynCall_iiii"](strategy.canvasResizedCallback, 37, 0, strategy.canvasResizedCallbackUserData);
            }
            return 0;
        }),
        fillPointerlockChangeEventData: (function(eventStruct, e) {
            var pointerLockElement = document.pointerLockElement || document.mozPointerLockElement || document.webkitPointerLockElement || document.msPointerLockElement;
            var isPointerlocked = !!pointerLockElement;
            HEAP32[eventStruct >> 2] = isPointerlocked;
            var nodeName = JSEvents.getNodeNameForTarget(pointerLockElement);
            var id = pointerLockElement && pointerLockElement.id ? pointerLockElement.id : "";
            stringToUTF8(nodeName, eventStruct + 4, 128);
            stringToUTF8(id, eventStruct + 132, 128);
        }),
        registerPointerlockChangeEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.pointerlockChangeEvent) {
                JSEvents.pointerlockChangeEvent = _malloc(260);
            }
            if (!target) {
                target = document;
            } else {
                target = JSEvents.findEventTarget(target);
            }
            var pointerlockChangeEventHandlerFunc = (function(event) {
                var e = event || window.event;
                var pointerlockChangeEvent = JSEvents.pointerlockChangeEvent;
                JSEvents.fillPointerlockChangeEventData(pointerlockChangeEvent, e);
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, pointerlockChangeEvent, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: target,
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: pointerlockChangeEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        registerPointerlockErrorEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString) {
            if (!target) {
                target = document;
            } else {
                target = JSEvents.findEventTarget(target);
            }
            var pointerlockErrorEventHandlerFunc = (function(event) {
                var e = event || window.event;
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, 0, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: target,
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: pointerlockErrorEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        requestPointerLock: (function(target) {
            if (target.requestPointerLock) {
                target.requestPointerLock();
            } else if (target.mozRequestPointerLock) {
                target.mozRequestPointerLock();
            } else if (target.webkitRequestPointerLock) {
                target.webkitRequestPointerLock();
            } else if (target.msRequestPointerLock) {
                target.msRequestPointerLock();
            } else {
                if (document.body.requestPointerLock || document.body.mozRequestPointerLock || document.body.webkitRequestPointerLock || document.body.msRequestPointerLock) {
                    return -3;
                } else {
                    return -1;
                }
            }
            return 0;
        }),
        fillVisibilityChangeEventData: (function(eventStruct, e) {
            var visibilityStates = [ "hidden", "visible", "prerender", "unloaded" ];
            var visibilityState = visibilityStates.indexOf(document.visibilityState);
            HEAP32[eventStruct >> 2] = document.hidden;
            HEAP32[eventStruct + 4 >> 2] = visibilityState;
        }),
        registerVisibilityChangeEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString, targetThread) {
            if (!JSEvents.visibilityChangeEvent) {
                JSEvents.visibilityChangeEvent = _malloc(8);
            }
            if (!target) {
                target = document;
            } else {
                target = JSEvents.findEventTarget(target);
            }
            var visibilityChangeEventHandlerFunc = (function(event) {
                var e = event || window.event;
                var visibilityChangeEvent = JSEvents.visibilityChangeEvent;
                JSEvents.fillVisibilityChangeEventData(visibilityChangeEvent, e);
                if (Module['dynCall_iiii'](callbackfunc, eventTypeId, visibilityChangeEvent, userData)) e.preventDefault();
            });
            var eventHandler = {
                target: target,
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: visibilityChangeEventHandlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        registerTouchEventCallback: (function (target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString) {
            if (!JSEvents.touchEvent) {
                JSEvents.touchEvent = _malloc(1684);
            }
            target = JSEvents.findEventTarget(target);
            var handlerFunc = (function (event) {
                var e = event || window.event;
                var touches = {};
                for (var i = 0; i < e.touches.length; ++i) {
                    var touch = e.touches[i];
                    touches[touch.identifier] = touch;
                }
                for (var i = 0; i < e.changedTouches.length; ++i) {
                    var touch = e.changedTouches[i];
                    touches[touch.identifier] = touch;
                    touch.changed = true;
                }
                for (var i = 0; i < e.targetTouches.length; ++i) {
                    var touch = e.targetTouches[i];
                    touches[touch.identifier].onTarget = true;
                }
                var ptr = JSEvents.touchEvent;
                HEAP32[ptr + 4 >> 2] = e.ctrlKey;
                HEAP32[ptr + 8 >> 2] = e.shiftKey;
                HEAP32[ptr + 12 >> 2] = e.altKey;
                HEAP32[ptr + 16 >> 2] = e.metaKey;
                ptr += 20;
                var canvasRect = Module["canvas"] ? Module["canvas"].getBoundingClientRect() : undefined;
                var targetRect = JSEvents.getBoundingClientRectOrZeros(target);
                var numTouches = 0;
                //////////////////// Modification Start ////////////////////////////
                var devicePixelRatio = window.devicePixelRatio;
                for (var i in touches) {
                    var t = touches[i];
                    HEAP32[ptr >> 2] = t.identifier;
                    HEAP32[ptr + 4 >> 2] = t.screenX * devicePixelRatio;
                    HEAP32[ptr + 8 >> 2] = t.screenY * devicePixelRatio;
                    HEAP32[ptr + 12 >> 2] = t.clientX * devicePixelRatio;
                    HEAP32[ptr + 16 >> 2] = t.clientY * devicePixelRatio;
                    HEAP32[ptr + 20 >> 2] = t.pageX * devicePixelRatio;
                    HEAP32[ptr + 24 >> 2] = t.pageY * devicePixelRatio;
                    HEAP32[ptr + 28 >> 2] = t.changed;
                    HEAP32[ptr + 32 >> 2] = t.onTarget;
                    if (canvasRect) {
                        HEAP32[ptr + 44 >> 2] = (t.clientX - canvasRect.left) * devicePixelRatio;
                        HEAP32[ptr + 48 >> 2] = (t.clientY - canvasRect.top) * devicePixelRatio;
                    } else {
                        HEAP32[ptr + 44 >> 2] = 0;
                        HEAP32[ptr + 48 >> 2] = 0;
                    }
                    HEAP32[ptr + 36 >> 2] = (t.clientX - targetRect.left) * devicePixelRatio;
                    HEAP32[ptr + 40 >> 2] = (t.clientY - targetRect.top) * devicePixelRatio;
                    //////////////////// Modification End ////////////////////////////
                    ptr += 52;
                    if (++numTouches >= 32) {
                        break;
                    }
                }
                HEAP32[JSEvents.touchEvent >> 2] = numTouches;
                var shouldCancel = Module["dynCall_iiii"](callbackfunc, eventTypeId, JSEvents.touchEvent, userData);
                if (shouldCancel) {
                    e.preventDefault();
                }
            });
            var eventHandler = {
                target: target,
                allowsDeferredCalls: eventTypeString == "touchstart" || eventTypeString == "touchend",
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: handlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        fillGamepadEventData: (function(eventStruct, e) {
            HEAPF64[eventStruct >> 3] = e.timestamp;
            for (var i = 0; i < e.axes.length; ++i) {
                HEAPF64[eventStruct + i * 8 + 16 >> 3] = e.axes[i];
            }
            for (var i = 0; i < e.buttons.length; ++i) {
                if (typeof e.buttons[i] === "object") {
                    HEAPF64[eventStruct + i * 8 + 528 >> 3] = e.buttons[i].value;
                } else {
                    HEAPF64[eventStruct + i * 8 + 528 >> 3] = e.buttons[i];
                }
            }
            for (var i = 0; i < e.buttons.length; ++i) {
                if (typeof e.buttons[i] === "object") {
                    HEAP32[eventStruct + i * 4 + 1040 >> 2] = e.buttons[i].pressed;
                } else {
                    HEAP32[eventStruct + i * 4 + 1040 >> 2] = e.buttons[i] == 1;
                }
            }
            HEAP32[eventStruct + 1296 >> 2] = e.connected;
            HEAP32[eventStruct + 1300 >> 2] = e.index;
            HEAP32[eventStruct + 8 >> 2] = e.axes.length;
            HEAP32[eventStruct + 12 >> 2] = e.buttons.length;
            stringToUTF8(e.id, eventStruct + 1304, 64);
            stringToUTF8(e.mapping, eventStruct + 1368, 64);
        }),
        registerGamepadEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString) {
            if (!JSEvents.gamepadEvent) {
                JSEvents.gamepadEvent = _malloc(1432);
            }
            var handlerFunc = (function(event) {
                var e = event || window.event;
                JSEvents.fillGamepadEventData(JSEvents.gamepadEvent, e.gamepad);
                var shouldCancel = Module["dynCall_iiii"](callbackfunc, eventTypeId, JSEvents.gamepadEvent, userData);
                if (shouldCancel) {
                    e.preventDefault();
                }
            });
            var eventHandler = {
                target: JSEvents.findEventTarget(target),
                allowsDeferredCalls: true,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: handlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        registerBeforeUnloadEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString) {
            var handlerFunc = (function(event) {
                var e = event || window.event;
                var confirmationMessage = Module["dynCall_iiii"](callbackfunc, eventTypeId, 0, userData);
                if (confirmationMessage) {
                    confirmationMessage = Pointer_stringify(confirmationMessage);
                }
                if (confirmationMessage) {
                    e.preventDefault();
                    e.returnValue = confirmationMessage;
                    return confirmationMessage;
                }
            });
            var eventHandler = {
                target: JSEvents.findEventTarget(target),
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: handlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        battery: (function() {
            return navigator.battery || navigator.mozBattery || navigator.webkitBattery;
        }),
        fillBatteryEventData: (function(eventStruct, e) {
            HEAPF64[eventStruct >> 3] = e.chargingTime;
            HEAPF64[eventStruct + 8 >> 3] = e.dischargingTime;
            HEAPF64[eventStruct + 16 >> 3] = e.level;
            HEAP32[eventStruct + 24 >> 2] = e.charging;
        }),
        registerBatteryEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString) {
            if (!JSEvents.batteryEvent) {
                JSEvents.batteryEvent = _malloc(32);
            }
            var handlerFunc = (function(event) {
                var e = event || window.event;
                JSEvents.fillBatteryEventData(JSEvents.batteryEvent, JSEvents.battery());
                var shouldCancel = Module["dynCall_iiii"](callbackfunc, eventTypeId, JSEvents.batteryEvent, userData);
                if (shouldCancel) {
                    e.preventDefault();
                }
            });
            var eventHandler = {
                target: JSEvents.findEventTarget(target),
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: handlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        }),
        registerWebGlEventCallback: (function(target, userData, useCapture, callbackfunc, eventTypeId, eventTypeString) {
            if (!target) {
                target = Module["canvas"];
            }
            var handlerFunc = (function(event) {
                var e = event || window.event;
                var shouldCancel = Module["dynCall_iiii"](callbackfunc, eventTypeId, 0, userData);
                if (shouldCancel) {
                    e.preventDefault();
                }
            });
            var eventHandler = {
                target: JSEvents.findEventTarget(target),
                allowsDeferredCalls: false,
                eventTypeString: eventTypeString,
                callbackfunc: callbackfunc,
                handlerFunc: handlerFunc,
                useCapture: useCapture
            };
            JSEvents.registerOrRemoveHandler(eventHandler);
        })
    },
});
