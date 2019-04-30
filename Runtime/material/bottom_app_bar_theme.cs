using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class BottomAppBarTheme : Diagnosticable {
        public BottomAppBarTheme(
            Color color = null,
            float? elevation = null,
            NotchedShape shape = null
        ) {
            this.color = color;
            this.elevation = elevation;
            this.shape = shape;
        }

        public readonly Color color;

        public readonly float? elevation;

        public readonly NotchedShape shape;

        BottomAppBarTheme copyWith(
            Color color = null,
            float? elevation = null,
            NotchedShape shape = null
        ) {
            return new BottomAppBarTheme(
                color: color ?? this.color,
                elevation: elevation ?? this.elevation,
                shape: shape ?? this.shape
            );
        }

        public static BottomAppBarTheme of(BuildContext context) {
            return Theme.of(context).bottomAppBarTheme;
        }

        public static BottomAppBarTheme lerp(BottomAppBarTheme a, BottomAppBarTheme b, float t) {
            return new BottomAppBarTheme(
                color: Color.lerp(a?.color, b?.color, t),
                elevation: MathUtils.lerpFloat(a?.elevation ?? 0.0f, b?.elevation ?? 0.0f, t),
                shape: t < 0.5f ? a?.shape : b?.shape
            );
        }

        public override int GetHashCode() {
            var hashCode = this.color?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.elevation?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.shape?.GetHashCode() ?? 0;
            return hashCode;
        }

        public bool Equals(BottomAppBarTheme other) {
            return other.color == this.color
                   && other.elevation == this.elevation
                   && other.shape == this.shape;
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

            return this.Equals((BottomAppBarTheme) obj);
        }

        public static bool operator ==(BottomAppBarTheme left, BottomAppBarTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(BottomAppBarTheme left, BottomAppBarTheme right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Color>("color", this.color, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("elevation", this.elevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<NotchedShape>("shape", this.shape, defaultValue: null));
        }
    }
}