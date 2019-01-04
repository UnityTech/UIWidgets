using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public static class MatrixUtils {
        
        public static Offset getAsTranslation(this Matrix3 matrix3)
        {
            return matrix3.isTranslate() ? new Offset(matrix3[Matrix3.kMTransX], matrix3[Matrix3.kMTransY]) : null;
        }
    }
}