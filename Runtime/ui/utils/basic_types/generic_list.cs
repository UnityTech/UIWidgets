using System.Collections.Generic;

namespace Unity.UIWidgets.ui {
    
    public class uiList<T> : PoolItem {
        List<T> list;

        public override void setup() {
            base.setup();
            this.list = this.list ?? new List<T>(128);
        }

        public void Add(T item) {
            this.list.Add(item);
        }

        public override void dispose() {
            //clear the list immediately to avoid potential memory leak
            //otherwise, we may clear it in Setup() for lazy update
            this.list.Clear();
            base.dispose();
        }
    }
}