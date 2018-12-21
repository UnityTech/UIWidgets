using System;
using UIWidgets.editor;
using UIWidgets.ui;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace UIWidgets.engine
{
    public class CanvasWindowAdapter : WindowAdapter
    {
        private Rect _position;
        private double __devicePixelRatio;
        public CanvasWindowAdapter(Rect position, double devicePixelRatio, Transform tranform): base(position, devicePixelRatio)
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

        protected override Vector2d convertPointerPosition(Vector2 postion)
        {
            throw new NotImplementedException("pointer event should not be handled by this class");
        }
    }

}