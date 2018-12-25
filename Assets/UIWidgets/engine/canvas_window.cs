using System;
using UIWidgets.editor;
using UIWidgets.ui;
using UnityEngine;
using UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace UIWidgets.engine
{

    public class CanvasWindowAdapter : WindowAdapter
    {
        private Rect _position;
        private double __devicePixelRatio;
        private WidgetCanvas canvas;
        public CanvasWindowAdapter(WidgetCanvas canvas)
        {
            this.canvas = canvas;
        }

        public override GUIContent titleContent
        {
            get
            {
                return new GUIContent(canvas.gameObject.name);
            }
        }

        protected override double queryDevicePixelRatio()
        {
            return canvas.devicePixelRatio;
        }

        protected override Vector2 queryWindowSize()
        {
            return canvas.size;
        }
    }

}