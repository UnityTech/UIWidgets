using System;
using UnityEngine;

namespace UIWidgets.ui.txt
{
    public class TextBlob
    {
        public TextBlob(string text, int start, int end, Vector2d[] positions, TextStyle style, Rect bounds)
        {
            Debug.Assert(start < end);
            this.text = text;
            this.start = start;
            this.end = end;
            this.positions = positions;
            this.style = style;
            this.bounds = bounds;
        }

        public Rect boundsInText
        {
            get { return bounds.shift(new Offset(positions[start].x, positions[start].y)); }    
        }
        
        public readonly string text;        
        public readonly int start;
        public readonly int end;
        public readonly Vector2d[] positions;
        public readonly TextStyle style;
        public readonly Rect bounds;  // bounds with positions[start] as origin       
    }
}