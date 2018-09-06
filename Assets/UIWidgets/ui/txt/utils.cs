using System;
using UnityEditor;

namespace UIWidgets.ui
{
    internal class Utils
    {
        public static double PixelCorrectRound(double v)
        {
            return Math.Round(v * EditorGUIUtility.pixelsPerPoint) / EditorGUIUtility.pixelsPerPoint;
        }
    }
}