using System;
using UIWidgets.ui;

namespace UIWidgets.painting {
    public class Alignment : IEquatable<Alignment> {
        public Alignment(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public readonly double x;

        public readonly double y;

        public static readonly Alignment topLeft = new Alignment(-1.0, -1.0);
        public static readonly Alignment topCenter = new Alignment(0, -1.0);
        public static readonly Alignment topRight = new Alignment(1.0, -1.0);
        public static readonly Alignment centerLeft = new Alignment(-1.0, 0.0);
        public static readonly Alignment center = new Alignment(0.0, 0.0);
        public static readonly Alignment centerRight = new Alignment(1.0, 0.0);
        public static readonly Alignment bottomLeft = new Alignment(-1.0, 1.0);
        public static readonly Alignment bottomCenter = new Alignment(0.0, 1.0);
        public static readonly Alignment bottomRight = new Alignment(1.0, 1.0);

        public Alignment add(Alignment other) {
            return this + other;
        }

        public static Alignment operator -(Alignment a, Alignment b) {
            return new Alignment(a.x - b.x, a.y - b.y);
        }

        public static Alignment operator +(Alignment a, Alignment b) {
            return new Alignment(a.x + b.x, a.y + b.y);
        }

        public static Alignment operator -(Alignment a) {
            return new Alignment(-a.x, -a.y);
        }

        public static Alignment operator *(Alignment a, double b) {
            return new Alignment(a.x * b, a.y * b);
        }

        public static Alignment operator /(Alignment a, double b) {
            return new Alignment(a.x / b, a.y / b);
        }

        public static Alignment operator %(Alignment a, double b) {
            return new Alignment(a.x % b, a.y % b);
        }

        public Offset alongOffset(Offset other) {
            double centerX = other.dx / 2.0;
            double centerY = other.dy / 2.0;
            return new Offset(centerX + this.x * centerX, centerY + this.y * centerY);
        }

        public Offset alongSize(Size other) {
            double centerX = other.width / 2.0;
            double centerY = other.height / 2.0;
            return new Offset(centerX + this.x * centerX, centerY + this.y * centerY);
        }

        public Offset withinRect(Rect rect) {
            double halfWidth = rect.width / 2.0;
            double halfHeight = rect.height / 2.0;
            return new Offset(
                rect.left + halfWidth + this.x * halfWidth,
                rect.top + halfHeight + this.y * halfHeight
            );
        }

        public Rect inscribe(Size size, Rect rect) {
            double halfWidthDelta = (rect.width - size.width) / 2.0;
            double halfHeightDelta = (rect.height - size.height) / 2.0;
            return Rect.fromLTWH(
                rect.left + halfWidthDelta + this.x * halfWidthDelta,
                rect.top + halfHeightDelta + this.y * halfHeightDelta,
                size.width,
                size.height
            );
        }

        public bool Equals(Alignment other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.x.Equals(other.x) && this.y.Equals(other.y);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Alignment) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.x.GetHashCode() * 397) ^ this.y.GetHashCode();
            }
        }

        public static bool operator ==(Alignment a, Alignment b) {
            return object.Equals(a, b);
        }

        public static bool operator !=(Alignment a, Alignment b) {
            return !(a == b);
        }
    }

    public class AlignmentDirectional : IEquatable<AlignmentDirectional>  {
        public AlignmentDirectional(double start, double y) {
            this.start = start;
            this.y = y;
        }

        public double start;
        public double y;
        
        public static readonly AlignmentDirectional topStart = new AlignmentDirectional(-1.0, -1.0);
        public static readonly AlignmentDirectional topCenter = new AlignmentDirectional(0.0, -1.0);
        public static readonly AlignmentDirectional topEnd = new AlignmentDirectional(1.0, -1.0);
        public static readonly AlignmentDirectional centerStart = new AlignmentDirectional(-1.0, 0.0);
        public static readonly AlignmentDirectional center = new AlignmentDirectional(0.0, 0.0);
        public static readonly AlignmentDirectional centerEnd = new AlignmentDirectional(1.0, 0.0);
        public static readonly AlignmentDirectional bottomStart = new AlignmentDirectional(-1.0, 1.0);
        public static readonly AlignmentDirectional bottomCenter = new AlignmentDirectional(0.0, 1.0);
        public static readonly AlignmentDirectional bottomEnd = new AlignmentDirectional(1.0, 1.0);
        
        public AlignmentDirectional add(AlignmentDirectional other) {
            return this + other;
        }

        public static AlignmentDirectional operator -(AlignmentDirectional a, AlignmentDirectional b) {
            return new AlignmentDirectional(a.start - b.start, a.y - b.y);
        }

        public static AlignmentDirectional operator +(AlignmentDirectional a, AlignmentDirectional b) {
            return new AlignmentDirectional(a.start + b.start, a.y + b.y);
        }

        public static AlignmentDirectional operator -(AlignmentDirectional a) {
            return new AlignmentDirectional(-a.start, -a.y);
        }

        public static AlignmentDirectional operator *(AlignmentDirectional a, double b) {
            return new AlignmentDirectional(a.start * b, a.y * b);
        }

        public static AlignmentDirectional operator /(AlignmentDirectional a, double b) {
            return new AlignmentDirectional(a.start / b, a.y / b);
        }

        public static AlignmentDirectional operator %(AlignmentDirectional a, double b) {
            return new AlignmentDirectional(a.start % b, a.y % b);
        }

        public bool Equals(AlignmentDirectional other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.start.Equals(other.start) && this.y.Equals(other.y);
        }
        
        public Alignment resolve(TextDirection direction) {
            switch (direction) {
                case TextDirection.rtl:
                    return new Alignment(-start, y);
                case TextDirection.ltr:
                    return new Alignment(start, y);
            }
            return null;
        }
    }
}