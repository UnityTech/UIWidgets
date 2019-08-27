using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class AppBarTheme : Diagnosticable {
        public AppBarTheme(
            Brightness? brightness = null,
            Color color = null,
            float? elevation = null,
            IconThemeData iconTheme = null,
            IconThemeData actionsIconTheme = null,
            TextTheme textTheme = null
        ) {
            this.brightness = brightness;
            this.color = color;
            this.elevation = elevation;
            this.iconTheme = iconTheme;
            this.actionsIconTheme = actionsIconTheme;
            this.textTheme = textTheme;
        }

        public readonly Brightness? brightness;

        public readonly Color color;

        public readonly float? elevation;

        public readonly IconThemeData iconTheme;

        public readonly IconThemeData actionsIconTheme;

        public readonly TextTheme textTheme;

        AppBarTheme copyWith(
            Brightness? brightness = null,
            Color color = null,
            float? elevation = null,
            IconThemeData iconTheme = null,
            IconThemeData actionsIconTheme = null,
            TextTheme textTheme = null
        ) {
            return new AppBarTheme(
                brightness: brightness ?? this.brightness,
                color: color ?? this.color,
                elevation: elevation ?? this.elevation,
                iconTheme: iconTheme ?? this.iconTheme,
                actionsIconTheme: actionsIconTheme ?? this.actionsIconTheme,
                textTheme: textTheme ?? this.textTheme
            );
        }

        public static AppBarTheme of(BuildContext context) {
            return Theme.of(context).appBarTheme;
        }

        public static AppBarTheme lerp(AppBarTheme a, AppBarTheme b, float t) {
            return new AppBarTheme(
                brightness: t < 0.5f ? a?.brightness : b?.brightness,
                color: Color.lerp(a?.color, b?.color, t),
                elevation: MathUtils.lerpFloat(a?.elevation ?? 0.0f, b?.elevation ?? 0.0f, t),
                iconTheme: IconThemeData.lerp(a?.iconTheme, b?.iconTheme, t),
                actionsIconTheme: IconThemeData.lerp(a?.actionsIconTheme, b?.actionsIconTheme, t),
                textTheme: TextTheme.lerp(a?.textTheme, b?.textTheme, t)
            );
        }

        public override int GetHashCode() {
            var hashCode = this.brightness?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.color?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.elevation?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.iconTheme?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.actionsIconTheme?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ this.textTheme?.GetHashCode() ?? 0;
            return hashCode;
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

            return this.Equals((AppBarTheme) obj);
        }

        public static bool operator ==(AppBarTheme left, AppBarTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(AppBarTheme left, AppBarTheme right) {
            return !Equals(left, right);
        }

        public bool Equals(AppBarTheme other) {
            return other.brightness == this.brightness
                   && other.color == this.color
                   && other.elevation == this.elevation
                   && other.iconTheme == this.iconTheme
                   && other.actionsIconTheme == this.actionsIconTheme
                   && other.textTheme == this.textTheme;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Brightness?>("brightness", this.brightness, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("color", this.color, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("elevation", this.elevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<IconThemeData>("iconTheme", this.iconTheme, defaultValue: null));
            properties.add(new DiagnosticsProperty<IconThemeData>("actionsIconTheme", this.actionsIconTheme, defaultValue: null));
            properties.add(new DiagnosticsProperty<TextTheme>("textTheme", this.textTheme, defaultValue: null));
        }
    }
}