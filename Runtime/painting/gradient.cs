using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    class _ColorsAndStops {
        public _ColorsAndStops(List<Color> colors, List<float> stops) {
            this.colors = colors;
            this.stops = stops;
        }

        public readonly List<Color> colors;
        public readonly List<float> stops;

        static Color _sample(List<Color> colors, List<float> stops, float t) {
            D.assert(colors != null);
            D.assert(colors.isNotEmpty);
            D.assert(stops != null);
            D.assert(stops.isNotEmpty);

            if (t < stops.first()) {
                return colors.first();
            }

            if (t > stops.last()) {
                return colors.last();
            }

            int index = stops.FindLastIndex((float s) => { return s <= t; });
            D.assert(index != -1);
            return Color.lerp(colors[index], colors[index + 1], 
                (t - stops[index]) / (stops[index + 1] - stops[index]));
        }

        internal static _ColorsAndStops _interpolateColorsAndStops(
            List<Color> aColors,
            List<float> aStops,
            List<Color> bColors,
            List<float> bStops,
            float t) {
            D.assert(aColors.Count >= 2);
            D.assert(bColors.Count >= 2);
            D.assert(aStops.Count == aColors.Count);
            D.assert(bStops.Count == bColors.Count);

            SplayTree<float, bool> stops = new SplayTree<float, bool>();
            stops.AddAll(aStops);
            stops.AddAll(bStops);

            List<float> interpolatedStops = stops.Keys.ToList();
            List<Color> interpolatedColors = interpolatedStops.Select<float, Color>((float stop) => {
                return Color.lerp(_sample(aColors, aStops, stop), _sample(bColors, bStops, stop), t);
            }).ToList();

            return new _ColorsAndStops(interpolatedColors, interpolatedStops);
        }
    }


    public abstract class Gradient {
        public Gradient(
            List<Color> colors = null,
            List<float> stops = null
        ) {
            D.assert(colors != null);
            this.colors = colors;
            this.stops = stops;
        }

        public readonly List<Color> colors;

        public readonly List<float> stops;

        protected List<float> _impliedStops() {
            if (this.stops != null) {
                return this.stops;
            }

            D.assert(this.colors.Count >= 2, () => "colors list must have at least two colors");
            float separation = 1.0f / (this.colors.Count - 1);

            return Enumerable.Range(0, this.colors.Count).Select(i => i * separation).ToList();
        }

        public abstract PaintShader createShader(Rect rect);

        public abstract Gradient scale(float factor);

        protected virtual Gradient lerpFrom(Gradient a, float t) {
            if (a == null) {
                return this.scale(t);
            }

            return null;
        }

        protected virtual Gradient lerpTo(Gradient b, float t) {
            if (b == null) {
                return this.scale(1.0f - t);
            }

            return null;
        }


        public static Gradient lerp(Gradient a, Gradient b, float t) {
            Gradient result = null;
            if (b != null) {
                result = b.lerpFrom(a, t); // if a is null, this must return non-null
            }

            if (result == null && a != null) {
                result = a.lerpTo(b, t); // if b is null, this must return non-null
            }

            if (result != null) {
                return result;
            }

            if (a == null && b == null) {
                return null;
            }

            D.assert(a != null && b != null);
            return t < 0.5 ? a.scale(1.0f - (t * 2.0f)) : b.scale((t - 0.5f) * 2.0f);
        }
    }


    public class LinearGradient : Gradient, IEquatable<LinearGradient> {
        public LinearGradient(
            Alignment begin = null,
            Alignment end = null,
            List<Color> colors = null,
            List<float> stops = null,
            TileMode tileMode = TileMode.clamp
        ) : base(colors: colors, stops: stops) {
            this.begin = begin ?? Alignment.centerLeft;
            this.end = end ?? Alignment.centerRight;
            this.tileMode = tileMode;
        }

        public readonly Alignment begin;

        public readonly Alignment end;

        public readonly TileMode tileMode;

        public override PaintShader createShader(Rect rect) {
            return ui.Gradient.linear(
                this.begin.withinRect(rect),
                this.end.withinRect(rect),
                this.colors, this._impliedStops(),
                this.tileMode
            );
        }

        public override Gradient scale(float factor) {
            return new LinearGradient(
                begin: this.begin,
                end: this.end,
                colors: this.colors.Select(color => Color.lerp(null, color, factor)).ToList(),
                stops: this.stops,
                tileMode: this.tileMode
            );
        }

        protected override Gradient lerpFrom(Gradient a, float t) {
            if (a == null || (a is LinearGradient)) {
                return lerp((LinearGradient) a, this, t);
            }

            return base.lerpFrom(a, t);
        }

        protected override Gradient lerpTo(Gradient b, float t) {
            if (b == null || (b is LinearGradient)) {
                return lerp(this, (LinearGradient) b, t);
            }

            return base.lerpTo(b, t);
        }

        public static LinearGradient lerp(LinearGradient a, LinearGradient b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (LinearGradient) b.scale(t);
            }

            if (b == null) {
                return (LinearGradient) a.scale(1.0f - t);
            }

            _ColorsAndStops interpolated = _ColorsAndStops._interpolateColorsAndStops(
                a.colors,
                a._impliedStops(),
                b.colors,
                b._impliedStops(),
                t);
            return new LinearGradient(
                begin: Alignment.lerp(a.begin, b.begin, t),
                end: Alignment.lerp(a.end, b.end, t),
                colors: interpolated.colors,
                stops: interpolated.stops,
                tileMode: t < 0.5 ? a.tileMode : b.tileMode
            );
        }

        public bool Equals(LinearGradient other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return
                this.colors.equalsList(other.colors) &&
                this.stops.equalsList(other.stops) &&
                Equals(this.begin, other.begin) &&
                Equals(this.end, other.end) &&
                this.tileMode == other.tileMode;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((LinearGradient) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.colors.hashList();
                hashCode = (hashCode * 397) ^ this.stops.hashList();
                hashCode = (hashCode * 397) ^ (this.begin != null ? this.begin.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.end != null ? this.end.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) this.tileMode;
                return hashCode;
            }
        }

        public static bool operator ==(LinearGradient left, LinearGradient right) {
            return Equals(left, right);
        }

        public static bool operator !=(LinearGradient left, LinearGradient right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.begin}, {this.end}," +
                   $"{this.colors.toStringList()}, {this.stops.toStringList()}, {this.tileMode})";
        }
    }

    public class RadialGradient : Gradient, IEquatable<RadialGradient> {
        public RadialGradient(
            Alignment center = null,
            float radius = 0.5f,
            List<Color> colors = null,
            List<float> stops = null,
            TileMode tileMode = TileMode.clamp
        ) : base(colors: colors, stops: stops) {
            this.center = center ?? Alignment.center;
            this.radius = radius;
            this.tileMode = tileMode;
        }

        public readonly Alignment center;

        public readonly float radius;

        public readonly TileMode tileMode;


        public override PaintShader createShader(Rect rect) {
            return ui.Gradient.radial(
                this.center.withinRect(rect),
                this.radius * rect.shortestSide,
                this.colors, this._impliedStops(),
                this.tileMode
            );
        }

        public override Gradient scale(float factor) {
            return new RadialGradient(
                center: this.center,
                radius: this.radius,
                colors: this.colors.Select(color => Color.lerp(null, color, factor)).ToList(),
                stops: this.stops,
                tileMode: this.tileMode
            );
        }

        protected override Gradient lerpFrom(Gradient a, float t) {
            if (a == null || (a is RadialGradient)) {
                return lerp((RadialGradient) a, this, t);
            }

            return base.lerpFrom(a, t);
        }

        protected override Gradient lerpTo(Gradient b, float t) {
            if (b == null || (b is RadialGradient)) {
                return lerp(this, (RadialGradient) b, t);
            }

            return base.lerpTo(b, t);
        }

        public static RadialGradient lerp(RadialGradient a, RadialGradient b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (RadialGradient) b.scale(t);
            }

            if (b == null) {
                return (RadialGradient) a.scale(1.0f - t);
            }

            _ColorsAndStops interpolated = _ColorsAndStops._interpolateColorsAndStops(
                a.colors,
                a._impliedStops(),
                b.colors,
                b._impliedStops(),
                t);
            return new RadialGradient(
                center: Alignment.lerp(a.center, b.center, t),
                radius: Mathf.Max(0.0f, MathUtils.lerpFloat(a.radius, b.radius, t)),
                colors: interpolated.colors,
                stops: interpolated.stops,
                tileMode: t < 0.5 ? a.tileMode : b.tileMode
            );
        }

        public bool Equals(RadialGradient other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return
                this.colors.equalsList(other.colors) &&
                this.stops.equalsList(other.stops) &&
                Equals(this.center, other.center) &&
                Equals(this.radius, other.radius) &&
                this.tileMode == other.tileMode;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((RadialGradient) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.colors.hashList();
                hashCode = (hashCode * 397) ^ this.stops.hashList();
                hashCode = (hashCode * 397) ^ (this.center != null ? this.center.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.radius.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) this.tileMode;
                return hashCode;
            }
        }

        public static bool operator ==(RadialGradient left, RadialGradient right) {
            return Equals(left, right);
        }

        public static bool operator !=(RadialGradient left, RadialGradient right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.center}, {this.radius}," +
                   $"{this.colors.toStringList()}, {this.stops.toStringList()}, {this.tileMode})";
        }
    }

    public class SweepGradient : Gradient, IEquatable<SweepGradient> {
        public SweepGradient(
            Alignment center = null,
            float startAngle = 0.0f,
            float endAngle = Mathf.PI * 2,
            List<Color> colors = null,
            List<float> stops = null,
            TileMode tileMode = TileMode.clamp
        ) : base(colors: colors, stops: stops) {
            this.center = center ?? Alignment.center;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.tileMode = tileMode;
        }

        public readonly Alignment center;

        public readonly float startAngle;

        public readonly float endAngle;

        public readonly TileMode tileMode;


        public override PaintShader createShader(Rect rect) {
            return ui.Gradient.sweep(
                this.center.withinRect(rect),
                this.colors, this._impliedStops(),
                this.tileMode,
                this.startAngle, this.endAngle
            );
        }

        public override Gradient scale(float factor) {
            return new SweepGradient(
                center: this.center,
                startAngle: this.startAngle,
                endAngle: this.endAngle,
                colors: this.colors.Select(color => Color.lerp(null, color, factor)).ToList(),
                stops: this.stops,
                tileMode: this.tileMode
            );
        }

        protected override Gradient lerpFrom(Gradient a, float t) {
            if (a == null || (a is SweepGradient && a.colors.Count == this.colors.Count)) {
                return lerp((SweepGradient) a, this, t);
            }

            return base.lerpFrom(a, t);
        }

        protected override Gradient lerpTo(Gradient b, float t) {
            if (b == null || (b is SweepGradient && b.colors.Count == this.colors.Count)) {
                return lerp(this, (SweepGradient) b, t);
            }

            return base.lerpTo(b, t);
        }

        public static SweepGradient lerp(SweepGradient a, SweepGradient b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (SweepGradient) b.scale(t);
            }

            if (b == null) {
                return (SweepGradient) a.scale(1.0f - t);
            }

            _ColorsAndStops interpolated =
                _ColorsAndStops._interpolateColorsAndStops(a.colors, a.stops, b.colors, b.stops, t);
            return new SweepGradient(
                center: Alignment.lerp(a.center, b.center, t),
                startAngle: Mathf.Max(0.0f, MathUtils.lerpFloat(a.startAngle, b.startAngle, t)),
                endAngle: Mathf.Max(0.0f, MathUtils.lerpFloat(a.endAngle, b.endAngle, t)),
                colors: interpolated.colors,
                stops: interpolated.stops,
                tileMode: t < 0.5 ? a.tileMode : b.tileMode
            );
        }

        public bool Equals(SweepGradient other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return
                this.colors.equalsList(other.colors) &&
                this.stops.equalsList(other.stops) &&
                Equals(this.center, other.center) &&
                Equals(this.startAngle, other.startAngle) &&
                Equals(this.endAngle, other.endAngle) &&
                this.tileMode == other.tileMode;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((SweepGradient) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.colors.hashList();
                hashCode = (hashCode * 397) ^ this.stops.hashList();
                hashCode = (hashCode * 397) ^ (this.center != null ? this.center.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.startAngle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.endAngle.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) this.tileMode;
                return hashCode;
            }
        }

        public static bool operator ==(SweepGradient left, SweepGradient right) {
            return Equals(left, right);
        }

        public static bool operator !=(SweepGradient left, SweepGradient right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.center}, {this.startAngle}, {this.endAngle}, " +
                   $"{this.colors.toStringList()}, {this.stops.toStringList()}, {this.tileMode})";
        }
    }
}