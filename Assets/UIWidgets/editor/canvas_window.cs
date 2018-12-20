using UnityEngine;
namespace UIWidgets.editor
{
    public class CanvasWindowAdapter : WindowAdapter
    {
        private Rect _position;
        private double __devicePixelRatio;
        public CanvasWindowAdapter(Rect position, double devicePixelRatio): base(position, devicePixelRatio)
        {
            this._position = position;
            this.__devicePixelRatio = devicePixelRatio;
        }

        public override void scheduleFrame()
        {
        }

        protected override void getWindowMetrics(out double devicePixelRatio, out Rect position)
        {
            devicePixelRatio = this.__devicePixelRatio;
            position = this._position;
        }
    }

}