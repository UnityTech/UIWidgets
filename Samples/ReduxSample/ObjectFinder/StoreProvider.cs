using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.Sample.Redux.ObjectFinder {

    public static class StoreProvider {

        private static Store<FinderAppState> _store;

        public static Store<FinderAppState> store {
            get {
                if (_store != null) {
                    return _store;
                }
                var middlewares = new Middleware<FinderAppState>[]  {
                    ReduxLogging.Create<FinderAppState>(),
                    GameFinderMiddleware.Create(),
                };
                _store = new Store<FinderAppState>(ObjectFinderReducer.Reduce,
                    new FinderAppState(),
                    middlewares
                );
                return _store;
            }
        }
    }
}