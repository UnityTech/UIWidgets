using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.Sample.Redux.ObjectFinder {
    [Serializable]
    public class GameObjectInfo {
        public int id;
        public string name;
    }

    public class FinderAppState {
        public int selected;
        public List<GameObjectInfo> objects;

        public FinderAppState() {
            this.selected = 0;
            this.objects = new List<GameObjectInfo>();
        }
    }

    public class SearchAction {
        public string keyword;
    }

    [Serializable]
    public class SearchResultAction {
        public string keyword;
        public List<GameObjectInfo> results;
    }

    [Serializable]
    public class SelectObjectAction {
        public int id;
    }

    public class ObjectFinderReducer {
        public static FinderAppState Reduce(FinderAppState state, object action) {
            if (action is SearchResultAction) {
                var resultAction = (SearchResultAction) action;
                var selected = state.selected;
                if (selected != 0) {
                    var obj = resultAction.results.Find(o => o.id == selected);
                    if (obj == null) {
                        selected = 0;
                    }
                }

                return new FinderAppState() {
                    objects = resultAction.results,
                    selected = state.selected,
                };
            }

            if (action is SelectObjectAction) {
                return new FinderAppState() {
                    objects = state.objects,
                    selected = ((SelectObjectAction) action).id,
                };
            }

            return state;
        }
    }
}