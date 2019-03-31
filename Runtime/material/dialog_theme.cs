using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class DialogTheme : Diagnosticable, IEquatable<DialogTheme> {
        public DialogTheme(ShapeBorder shape = null) {
            this.shape = shape;
        }

        public readonly ShapeBorder shape;

        public DialogTheme copyWith(ShapeBorder shape = null) {
            return new DialogTheme(shape: shape ?? this.shape);
        }

        public static DialogTheme of(BuildContext context) {
            return Theme.of(context).dialogTheme;
        }

        public static DialogTheme lerp(DialogTheme a, DialogTheme b, float t) {
            return new DialogTheme(
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t)
            );
        }

        public bool Equals(DialogTheme other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return Equals(this.shape, other.shape);
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
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }
}
