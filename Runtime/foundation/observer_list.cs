using System.Collections;
using System.Collections.Generic;

namespace Unity.UIWidgets.foundation {
    public class ObserverList<T> : ICollection<T> {
        readonly List<T> _list = new List<T>();
        bool _isDirty = false;
        HashSet<T> _set = null;

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator() {
            return this._list.GetEnumerator();
        }

        public void Add(T item) {
            this._isDirty = true;
            this._list.Add(item);
        }

        public void Clear() {
            this._isDirty = true;
            this._list.Clear();
        }

        public bool Contains(T item) {
            if (this._list.Count < 3) {
                return this._list.Contains(item);
            }

            if (this._isDirty) {
                if (this._set == null) {
                    this._set = new HashSet<T>(this._list);
                }
                else {
                    this._set.Clear();
                    this._set.UnionWith(this._list);
                }

                this._isDirty = false;
            }

            return this._set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            this._list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            this._isDirty = true;
            return this._list.Remove(item);
        }

        public int Count {
            get { return this._list.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }
    }
}