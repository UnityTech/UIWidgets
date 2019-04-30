using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class DialogTheme : Diagnosticable, IEquatable<DialogTheme> {
        public DialogTheme(
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            TextStyle titleTextStyle = null,
            TextStyle contentTextStyle = null
        ) {
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.shape = shape;
            this.titleTextStyle = titleTextStyle;
            this.contentTextStyle = contentTextStyle;
        }

        public readonly Color backgroundColor;

        public readonly float? elevation;

        public readonly ShapeBorder shape;

        public readonly TextStyle titleTextStyle;

        public readonly TextStyle contentTextStyle;

        DialogTheme copyWith(
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            TextStyle titleTextStyle = null,
            TextStyle contentTextStyle = null
        ) {
            return new DialogTheme(
                backgroundColor: backgroundColor ?? this.backgroundColor,
                elevation: elevation ?? this.elevation,
                shape: shape ?? this.shape,
                titleTextStyle: titleTextStyle ?? this.titleTextStyle,
                contentTextStyle: contentTextStyle ?? this.contentTextStyle
            );
        }

        public static DialogTheme of(BuildContext context) {
            return Theme.of(context).dialogTheme;
        }

        public static DialogTheme lerp(DialogTheme a, DialogTheme b, float t) {
            return new DialogTheme(
                backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
                elevation: MathUtils.lerpFloat(a?.elevation ?? 0.0f, b?.elevation ?? 0.0f, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t),
                titleTextStyle: TextStyle.lerp(a?.titleTextStyle, b?.titleTextStyle, t),
                contentTextStyle: TextStyle.lerp(a?.contentTextStyle, b?.contentTextStyle, t)
            );
        }

        public bool Equals(DialogTheme other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.backgroundColor, other.backgroundColor)
                   && Equals(this.elevation, other.elevation)
                   && Equals(this.shape, other.shape)
                   && Equals(this.titleTextStyle, other.titleTextStyle)
                   && Equals(this.contentTextStyle, other.contentTextStyle);
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

            return this.Equals((DialogTheme) obj);
        }

        public override int GetHashCode() {
            return (this.shape != null ? this.shape.GetHashCode() : 0);
        }

        public static bool operator ==(DialogTheme left, DialogTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(DialogTheme left, DialogTheme right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Color>("backgroundColor", this.backgroundColor));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape));
            properties.add(new DiagnosticsProperty<float?>("elevation", this.elevation));
            properties.add(new DiagnosticsProperty<TextStyle>("titleTextStyle", this.titleTextStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("contentTextStyle", this.contentTextStyle));
        }
    }
}