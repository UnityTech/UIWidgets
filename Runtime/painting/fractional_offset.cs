using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class FractionalOffset : Alignment {
        public FractionalOffset(float dx, float dy)
            : base(dx * 2.0f - 1.0f, dy * 2.0f - 1.0f) {
        }

        public static FractionalOffset fromOffsetAndSize(Offset offset, Size size) {
            D.assert(size != null);
            D.assert(offset != null);
            return new FractionalOffset(
                offset.dx / size.width,
                offset.dy / size.height
            );
        }

        public static FractionalOffset fromOffsetAndRect(Offset offset, Rect rect) {
            return fromOffsetAndSize(
                offset - rect.topLeft,
                rect.size
            );
        }

        public float dx {
            get { return (this.x + 1.0f) / 2.0f; }
        }

        public float dy {
            get { return (this.y + 1.0f) / 2.0f; }
        }

#pragma warning disable 0108
        public static readonly FractionalOffset topLeft = new FractionalOffset(0.0f, 0.0f);

        public static readonly FractionalOffset topCenter = new FractionalOffset(0.5f, 0.0f);

        public static readonly FractionalOffset topRight = new FractionalOffset(1.0f, 0.0f);

        public static readonly FractionalOffset centerLeft = new FractionalOffset(0.0f, 0.5f);

        public static readonly FractionalOffset center = new FractionalOffset(0.5f, 0.5f);

        public static readonly FractionalOffset centerRight = new FractionalOffset(1.0f, 0.5f);

        public static readonly FractionalOffset bottomLeft = new FractionalOffset(0.0f, 1.0f);

        public static readonly FractionalOffset bottomCenter = new FractionalOffset(0.5f, 1.0f);

        public static readonly FractionalOffset bottomRight = new FractionalOffset(1.0f, 1.0f);
#pragma warning restore 0108

        public static Alignment operator -(FractionalOffset a, Alignment b) {
            if (!(b is FractionalOffset)) {
                return (a as Alignment) - b;
            }

            FractionalOffset typedOther = (FractionalOffset) b;
            return new FractionalOffset(a.dx - typedOther.dx, a.dy - typedOther.dy);
        }

        public static Alignment operator +(FractionalOffset a, Alignment b) {
            if (!(b is FractionalOffset)) {
                return (a as Alignment) + b;
            }

            FractionalOffset typedOther = (FractionalOffset) b;
            return new FractionalOffset(a.dx + typedOther.dx, a.dy + typedOther.dy);
        }

        public static FractionalOffset operator -(FractionalOffset a) {
            return new FractionalOffset(-a.dx, -a.dy);
        }

        public static FractionalOffset operator *(FractionalOffset a, float b) {
            return new FractionalOffset(a.dx * b, a.dy * b);
        }

        public static FractionalOffset operator /(FractionalOffset a, float b) {
            return new FractionalOffset(a.dx / b, a.dy / b);
        }

        public static FractionalOffset operator %(FractionalOffset a, float b) {
            return new FractionalOffset(a.dx % b, a.dy % b);
        }

        public static FractionalOffset lerp(FractionalOffset a, FractionalOffset b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return new FractionalOffset(MathUtils.lerpFloat(0.5f, b.dx, t), MathUtils.lerpFloat(0.5f, b.dy, t));
            }

            if (b == null) {
                return new FractionalOffset(MathUtils.lerpFloat(a.dx, 0.5f, t), MathUtils.lerpFloat(a.dy, 0.5f, t));
            }

            return new FractionalOffset(MathUtils.lerpFloat(a.dx, b.dx, t), MathUtils.lerpFloat(a.dy, b.dy, t));
        }

        public override string ToString() {
            return $"FractionalOffset({this.dx:0.0}, " +
                   $"{this.dy:0.0})";
        }
    }
}