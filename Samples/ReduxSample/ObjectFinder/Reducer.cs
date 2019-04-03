using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.Redux;

namespace Unity.UIWidgets.Sample.Redux.ObjectFinder {
    [Serializable]
    public class GameObjectInfo : IEquatable<GameObjectInfo> {
        public int id;
        public string name;

        public bool Equals(GameObjectInfo other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return this.id == other.id && string.Equals(this.name, other.name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return this.Equals((GameObjectInfo) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.id * 397) ^ (this.name != null ? this.name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(GameObjectInfo left, GameObjectInfo right) {
            return Equals(left, right);
        }

        public static bool operator !=(GameObjectInfo left, GameObjectInfo right) {
            return !Equals(left, right);
        }
    }

    public class FinderAppState : IEquatable<FinderAppState> {
        public int selected;
        public List<GameObjectInfo> objects;

        public FinderAppState() {
            this.selected = 0;
            this.objects = new List<GameObjectInfo>();
        }

        public bool Equals(FinderAppState other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return this.selected == other.selected && Equals(this.objects, other.objects);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return this.Equals((FinderAppState) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.selected * 397) ^ (this.objects != null ? this.objects.GetHashCode() : 0);
            }
        }

        public static bool operator ==(FinderAppState left, FinderAppState right) {
            return Equals(left, right);
        }

        public static bool operator !=(FinderAppState left, FinderAppState right) {
            return !Equals(left, right);
        }
    }

    public static class SearchAction {
        public static ThunkAction<FinderAppState> create(string keyword) {
            return new ThunkAction<FinderAppState>(
                displayName: "SearchAction",
                action: (dispatcher, getState) => {
                var objects = UnityEngine.Object.FindObjectsOfType(typeof(FinderGameObject)).Where(
                    obj => keyword == "" || obj.name.ToUpper().Contains(keyword.ToUpper())).Select(
                    obj => new GameObjectInfo {id = obj.GetInstanceID(), name = obj.name}).ToList();

                dispatcher.dispatch(new SearchResultAction {keyword = keyword, results = objects});
                return null;
            });
        }
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