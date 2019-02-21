mergeInto(LibraryManager.library, {

    UIWidgetsWebGLDevicePixelRatio: function () {
        return window.devicePixelRatio || 1;
    },

});