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
        private GameObject gameObject;
        public CanvasWindowAdapter(Rect position, double devicePixelRatio, GameObject gameObject): base(position, devicePixelRatio)
        {
            this._position = position;
            this.__devicePixelRatio = devicePixelRatio;
            this.gameObject = gameObject;
        }

        public override void scheduleFrame()
        {
        }

        public override GUIContent titleContent
        {
            get
            {
                return new GUIContent(gameObject.name);
            }
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