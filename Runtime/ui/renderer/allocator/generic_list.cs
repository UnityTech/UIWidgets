using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.ui {
    public class uiList<T> : PoolObject {
        List<T> list;

        public override void setup() {
            base.setup();
            this.list = this.list ?? new List<T>(128);
        }

        public uiList() {
        }

        public List<T> data {
            get { return this.list; }
        }

        public void Add(T item) {
            this.list.Add(item);
        }

        public void AddRange(IList<T> items) {
            this.list.AddRange(items);
        }

        public void Clear() {
            this.list.Clear();
        }

        public override void clear() {
            //clear the list immediately to avoid potential memory leak
            //otherwise, we may clear it in Setup() for lazy update
            this.list.Clear();
        }

        public int Count {
            get { return this.list.Count; }
        }

        public void SetCapacity(int capacity) {
            this.list.Capacity = Math.Max(capacity, this.list.Capacity);
        }

        public T this[int index] {
            get { return this.list[index]; }
            set { this.list[index] = value; }
        }
    }
}