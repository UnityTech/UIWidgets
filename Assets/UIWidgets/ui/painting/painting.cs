using System;
using UIWidgets.painting;
using UnityEngine;

namespace UIWidgets.ui {
    public static class PaintingUtils {
        internal static Color _scaleAlpha(this Color a, double factor) {
            return a.withAlpha((a.alpha * factor).round().clamp(0, 255));
        }
    }

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

        public static Color lerp(Color a, Color b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b._scaleAlpha(t);
            }

            if (b == null) {
                return a._scaleAlpha(1.0 - t);
            }

            return Color.fromARGB(
                ((int) MathUtils.lerpDouble(a.alpha, b.alpha, t)).clamp(0, 255),
                ((int) MathUtils.lerpDouble(a.red, b.red, t)).clamp(0, 255),
                ((int) MathUtils.lerpDouble(a.green, b.green, t)).clamp(0, 255),
                ((int) MathUtils.lerpDouble(a.blue, b.blue, t)).clamp(0, 255)
            );
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

    public class ColorFilter {
        public ColorFilter(Color color, BlendMode blendMode) {
            _color = color;
            _blendMode = blendMode;
        }

        Color _color;
        BlendMode _blendMode;
    }

    public enum BlendMode {
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