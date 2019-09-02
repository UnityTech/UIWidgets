using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public class FloatingActionButtonThemeData : Diagnosticable {
        public FloatingActionButtonThemeData(
            Color backgroundColor = null,
            Color foregroundColor = null,
            float? elevation = null,
            float? disabledElevation = null,
            float? highlightElevation = null,
            ShapeBorder shape = null
        ) {
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
            this.elevation = elevation;
            this.disabledElevation = disabledElevation;
            this.highlightElevation = highlightElevation;
            this.shape = shape;
        }

        public readonly Color backgroundColor;

        public readonly Color foregroundColor;

        public readonly float? elevation;

        public readonly float? disabledElevation;

        public readonly float? highlightElevation;

        public readonly ShapeBorder shape;

        public FloatingActionButtonThemeData copyWith(
            Color backgroundColor,
            Color foregroundColor,
            float? elevation,
            float? disabledElevation,
            float? highlightElevation,
            ShapeBorder shape
        ) {
            return new FloatingActionButtonThemeData(
                backgroundColor: backgroundColor ?? this.backgroundColor,
                foregroundColor: foregroundColor ?? this.foregroundColor,
                elevation: elevation ?? this.elevation,
                disabledElevation: disabledElevation ?? this.disabledElevation,
                highlightElevation: highlightElevation ?? this.highlightElevation,
                shape: shape ?? this.shape
            );
        }

        public static FloatingActionButtonThemeData lerp(FloatingActionButtonThemeData a, FloatingActionButtonThemeData b,
            float t) {
            if (a == null && b == null) {
                return null;
            }

            return new FloatingActionButtonThemeData(
                backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
                foregroundColor: Color.lerp(a?.foregroundColor, b?.foregroundColor, t),
                elevation: MathUtils.lerpFloat(a?.elevation ?? 0, b?.elevation ?? 0, t),
                disabledElevation: MathUtils.lerpFloat(a?.disabledElevation ?? 0, b?.disabledElevation ?? 0, t),
                highlightElevation: MathUtils.lerpFloat(a?.highlightElevation ?? 0, b?.highlightElevation ?? 0, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t)
            );
        }

        public override int GetHashCode() {
            var hashCode = this.backgroundColor?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.foregroundColor?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.elevation?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.disabledElevation?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.highlightElevation?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.shape?.GetHashCode() ?? 0;
            return hashCode;
        }

        public bool Equals(FloatingActionButtonThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.backgroundColor, other.backgroundColor)
                   && Equals(this.elevation, other.elevation)
                   && Equals(this.shape, other.shape)
                   && Equals(this.foregroundColor, other.foregroundColor)
                   && Equals(this.disabledElevation, other.disabledElevation)
                   && Equals(this.highlightElevation, other.highlightElevation);
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

            return this.Equals((FloatingActionButtonThemeData) obj);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            FloatingActionButtonThemeData defaultData = new FloatingActionButtonThemeData();

            properties.add(new DiagnosticsProperty<Color>("backgroundColor", this.backgroundColor,
                defaultValue: defaultData.backgroundColor));
            properties.add(new DiagnosticsProperty<Color>("foregroundColor", this.foregroundColor,
                defaultValue: defaultData.foregroundColor));
            properties.add(new DiagnosticsProperty<float?>("elevation", this.elevation,
                defaultValue: defaultData.elevation));
            properties.add(new DiagnosticsProperty<float?>("disabledElevation", this.disabledElevation,
                defaultValue: defaultData.disabledElevation));
            properties.add(new DiagnosticsProperty<float?>("highlightElevation", this.highlightElevation,
                defaultValue: defaultData.highlightElevation));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape, defaultValue: defaultData.shape));
        }
    }
}