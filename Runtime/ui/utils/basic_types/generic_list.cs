using System.Collections.Generic;

namespace Unity.UIWidgets.ui {
    
    public class uiList<T> : PoolItem {
        List<T> list;

        public void Setup() {
            this.list = this.list ?? new List<T>(128);
        }

        public void Add(T item) {
            this.list.Add(item);
        }

        public override void Dispose() {
            //clear the list immediately to avoid potential memory leak
            //otherwise, we may clear it in Setup() for lazy action
            this.list.Clear();
            base.Dispose();
        }
    }
}