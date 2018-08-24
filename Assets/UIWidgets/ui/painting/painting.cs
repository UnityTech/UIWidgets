using System;
using UIWidgets.painting;
using UnityEngine;

namespace UIWidgets.ui {
    public class Color : IEquatable<Color> {
        public Color(long value) {
            this.value = value & 0xFFFFFFFF;
        }

        public static Color fromARGB(int a, int r, int g, int b) {
            return new Color(
                (((a & 0xff) << 24) |
                 ((r & 0xff) << 16) |
                 ((g & 0xff) << 8) |
                 ((b & 0xff) << 0)) & 0xFFFFFFFF);
        }

        public static Color fromRGBO(int r, int g, int b, double opacity) {
            return new Color(
                ((((int) (opacity * 0xff) & 0xff) << 24) |
                 ((r & 0xff) << 16) |
                 ((g & 0xff) << 8) |
                 ((b & 0xff) << 0)) & 0xFFFFFFFF);
        }

        public readonly long value;

        public int alpha {
            get { return (int) ((0xff000000 & this.value) >> 24); }
        }

        public double opacity {
            get { return this.alpha / 255.0; }
        }

        public int red {
            get { return (int) ((0x00ff0000 & this.value) >> 16); }
        }

        public int green {
            get { return (int) ((0x0000ff00 & this.value) >> 8); }
        }

        public int blue {
            get { return (int) ((0x000000ff & this.value) >> 0); }
        }

        public Color withAlpha(int a) {
            return Color.fromARGB(a, this.red, this.green, this.blue);
        }

        public Color withOpacity(double opacity) {
            return this.withAlpha((int) (opacity * 255));
        }

        public Color withRed(int r) {
            return Color.fromARGB(this.alpha, r, this.green, this.blue);
        }

        public Color withGreen(int g) {
            return Color.fromARGB(this.alpha, this.red, g, this.blue);
        }

        public Color withBlue(int b) {
            return Color.fromARGB(this.alpha, this.red, this.green, b);
        }

        public bool Equals(Color other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.value == other.value;
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Color) obj);
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator ==(Color a, Color b) {
            return object.ReferenceEquals(a, null) ? object.ReferenceEquals(b, null) : a.Equals(b);
        }

        public static bool operator !=(Color a, Color b) {
            return !(a == b);
        }
    }

    public class Paint {
        public Color color;
        public double blurSigma;
    }

    public static class Conversions {
        public static UnityEngine.Color toColor(this Color color) {
            return new UnityEngine.Color(
                color.red / 255f, color.green / 255f, color.blue / 255f, color.alpha / 255f);
        }

        public static Vector2 toVector(this Offset offset) {
            return new Vector2((float) offset.dx, (float) offset.dy);
        }

        public static UnityEngine.Rect toRect(this Rect rect) {
            return new UnityEngine.Rect((float) rect.left, (float) rect.top, (float) rect.width, (float) rect.height);
        }

        public static Vector4 toVector(this BorderWidth borderWidth) {
            return new Vector4((float) borderWidth.left, (float) borderWidth.top, (float) borderWidth.right,
                (float) borderWidth.bottom);
        }

        public static float[] toFloatArray(this BorderWidth borderWidth) {
            return new[] {
                (float) borderWidth.left, (float) borderWidth.top,
                (float) borderWidth.right, (float) borderWidth.bottom
            };
        }

        public static Vector4 toVector(this BorderRadius borderRadius) {
            return new Vector4((float) borderRadius.topLeft, (float) borderRadius.topRight,
                (float) borderRadius.bottomRight, (float) borderRadius.bottomLeft);
        }

        public static float[] toFloatArray(this BorderRadius borderRadius) {
            return new[] {
                (float) borderRadius.topLeft, (float) borderRadius.topRight,
                (float) borderRadius.bottomRight, (float) borderRadius.bottomLeft
            };
        }
    }

//    public class GUICanvas : Canvas {
//        static GUICanvas() {
//            GUICanvas.shadowMat = Resources.Load<Material>("UIWidgets_ShadowMat");
//            if (GUICanvas.shadowMat == null) {
//                throw new Exception("UIWidgets_ShadowShader not found");
//            }
//        }
//
//        public static readonly Material shadowMat;
//
//        public override void drawPloygon4(Paint paint, params Offset[] points) {
//            Vector3[] vectors = new Vector3 [points.Length];
//            for (int i = 0; i < points.Length; i++) {
//                vectors[i] = points[i].toVector();
//            }
//
//            Handles.DrawSolidRectangleWithOutline(vectors, paint.color.toColor(),
//                new UnityEngine.Color(0f, 0f, 0f, 0f));
//        }
//
//        public override void drawRect(Paint paint, Rect rect, BorderWidth borderWidth, BorderRadius borderRadius) {
//            GUI.DrawTexture(rect.toRect(), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0,
//                paint.color.toColor(), borderWidth.toVector(), borderRadius.toVector());
//        }
//
//        public override void drawRectShadow(Paint paint, Rect rect) {
//            GUICanvas.shadowMat.SetFloatArray("_Rect", new float[] {
//                (float) rect.left, (float) rect.top, (float) rect.width, (float) rect.height,
//            });
//            GUICanvas.shadowMat.SetFloat("_sigma", (float) paint.blurSigma);
//
//            Graphics.DrawTexture(rect.toRect(), EditorGUIUtility.whiteTexture,
//                new UnityEngine.Rect(0.0f, 0.0f, 1f, 1f), 0, 0, 0, 0, paint.color.toColor(), GUICanvas.shadowMat);
//        }
//    }
    
    public class ColorFilter
    {
        public ColorFilter(Color color, BlendMode blendMode)
        {
            _color = color;
            _blendMode = blendMode;
        }

        Color _color;
        BlendMode _blendMode;

//        public static bool operator ==(ColorFilter a, dynamic other) {
//            if (other is! ColorFilter)
//                return false;
//            ColorFilter typedOther = other;
//            return a._color == typedOther._color &&
//                   a._blendMode == typedOther._blendMode;
//        }
    }
    
    public enum BlendMode
    {
        None = 0, // explicitly assign zero to make it more clear
        clear,
        src,
        dst,
        dstOver,
        srcIn,
        dstIn,
        srcOut,
        dstOut,
        srcATop,
        dstATop,
        xor,
        plus,
        modulate,
        screen, // The last coeff mode.
        overlay,
        darken,
        lighten,
        colorDodge,
        colorBurn,
        hardLight,
        softLight,
        difference,
        exclusion,
        multiply, // The last separable mode.
        hue,
        saturation,
        color,
        luminosity,
    } 
}