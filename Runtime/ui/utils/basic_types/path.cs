namespace Unity.UIWidgets.ui {
    
    public class uiPath : PoolItem {
        Path path;
        
        public void Setup() {
            this.path = null;
        }

        public void CopyFrom(Path path) {
            this.path = path;
        }

        public override void Dispose() {
            this.path = null;
            base.Dispose();
        }
    }
}