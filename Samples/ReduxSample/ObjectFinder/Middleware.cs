using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.Sample.Redux.ObjectFinder {

    public class GameFinderMiddleware {
        public static Middleware<FinderAppState> Create() {

            return (store) => (next) => (action) => {
                if (action is SearchAction) {
                    var searchAction = (SearchAction)action;
                    var objects = GameObject.FindObjectsOfType(typeof(FinderGameObject)).Where((obj) => {
                        return searchAction.keyword == "" || obj.name.ToUpper().Contains(searchAction.keyword.ToUpper());
                    }).Select(obj => new GameObjectInfo{id = obj.GetInstanceID(), name=obj.name}).ToList();

                    var result = next(action);
                    store.Dispatch(new SearchResultAction() {keyword= searchAction.keyword, results= objects});
                    return result;
                }
                
                return next(action);
            };
        }
    }

}