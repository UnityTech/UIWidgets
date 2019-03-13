using System.Collections.Generic;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.service {
    public enum DeviceOrientation {
        potraitUp,
        landscapeLeft,
        portraitDown,
        landscapeRight
    }


    public class ApplicationSwitcherDescription {
        public ApplicationSwitcherDescription(
            string label = null,
            int? primaryColor = null
        ) {
            this.label = label;
            this.primaryColor = primaryColor;
        }

        public readonly string label;

        public readonly int? primaryColor;
    }

    public enum SystemUiOverlay {
        top,
        bottom
    }

    public enum Brightness {
        dark,
        light
    }

    public class SystemUiOverlayStyle {
        public SystemUiOverlayStyle(
            Color systemNavigationBarColor = null,
            Color systemNavigationBarDividerColor = null,
            Brightness? systemNavigationBarIconBrightness = null,
            Color statusBarColor = null,
            Brightness? statusBarBrightness = null,
            Brightness? statusBarIconBrightness = null
        ) {
            this.systemNavigationBarColor = systemNavigationBarColor;
            this.systemNavigationBarDividerColor = systemNavigationBarDividerColor;
            this.systemNavigationBarIconBrightness = systemNavigationBarIconBrightness;
            this.statusBarColor = statusBarColor;
            this.statusBarBrightness = statusBarBrightness;
            this.statusBarIconBrightness = statusBarIconBrightness;
        }

        public readonly Color systemNavigationBarColor;

        public readonly Color systemNavigationBarDividerColor;

        public readonly Brightness? systemNavigationBarIconBrightness;

        public readonly Color statusBarColor;

        public readonly Brightness? statusBarBrightness;

        public readonly Brightness? statusBarIconBrightness;

        public static readonly SystemUiOverlayStyle light = new SystemUiOverlayStyle(
            systemNavigationBarColor: new Color(0xFF000000),
            systemNavigationBarDividerColor: null,
            statusBarColor: null,
            systemNavigationBarIconBrightness: Brightness.light,
            statusBarIconBrightness: Brightness.light,
            statusBarBrightness: Brightness.dark
        );

        public static readonly SystemUiOverlayStyle dark = new SystemUiOverlayStyle(
            systemNavigationBarColor: new Color(0xFF000000),
            systemNavigationBarDividerColor: null,
            statusBarColor: null,
            systemNavigationBarIconBrightness: Brightness.light,
            statusBarIconBrightness: Brightness.dark,
            statusBarBrightness: Brightness.light
        );

        public Dictionary<string, object> _toMap() {
            return new Dictionary<string, object> {
                {"systemNavigationBarColor", this.systemNavigationBarColor?.value},
                {"systemNavigationBarDividerColor", this.systemNavigationBarDividerColor?.value},
                {"statusBarColor", this.statusBarColor?.value},
                {"statusBarBrightness", this.statusBarBrightness?.ToString()},
                {"statusBarIconBrightness", this.statusBarIconBrightness?.ToString()},
                {"systemNavigationBarIconBrightness", this.systemNavigationBarIconBrightness?.ToString()}
            };
        }

        public override string ToString() {
            return this._toMap().ToString();
        }

        public SystemUiOverlayStyle copyWith(
            Color systemNavigationBarColor = null,
            Color systemNavigationBarDividerColor = null,
            Color statusBarColor = null,
            Brightness? statusBarBrightness = null,
            Brightness? statusBarIconBrightness = null,
            Brightness? systemNavigationBarIconBrightness = null
        ) {
            return new SystemUiOverlayStyle(
                systemNavigationBarColor: systemNavigationBarColor ?? this.systemNavigationBarColor,
                systemNavigationBarDividerColor:
                systemNavigationBarDividerColor ?? this.systemNavigationBarDividerColor,
                statusBarColor: statusBarColor ?? this.statusBarColor,
                statusBarIconBrightness: statusBarIconBrightness ?? this.statusBarIconBrightness,
                statusBarBrightness: statusBarBrightness ?? this.statusBarBrightness,
                systemNavigationBarIconBrightness: systemNavigationBarIconBrightness ??
                                                   this.systemNavigationBarIconBrightness
            );
        }

        public override int GetHashCode() {
            var hashCode = this.systemNavigationBarColor.GetHashCode();
            hashCode = (hashCode * 397) ^ (this.systemNavigationBarDividerColor != null
                           ? this.systemNavigationBarDividerColor.GetHashCode()
                           : 0);
            hashCode = (hashCode * 397) ^ (this.statusBarColor != null ? this.statusBarColor.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^
                       (this.statusBarBrightness != null ? this.statusBarBrightness.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^
                       (this.statusBarIconBrightness != null ? this.statusBarIconBrightness.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.systemNavigationBarIconBrightness != null
                           ? this.systemNavigationBarIconBrightness.GetHashCode()
                           : 0);
            return hashCode;
        }


        public bool Equals(SystemUiOverlayStyle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.systemNavigationBarColor == this.systemNavigationBarColor &&
                   other.systemNavigationBarDividerColor == this.systemNavigationBarDividerColor &&
                   other.statusBarColor == this.statusBarColor &&
                   other.statusBarIconBrightness == this.statusBarIconBrightness &&
                   other.statusBarBrightness == this.statusBarIconBrightness &&
                   other.systemNavigationBarIconBrightness == this.systemNavigationBarIconBrightness;
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

            return this.Equals((SystemUiOverlayStyle) obj);
        }

        public static bool operator ==(SystemUiOverlayStyle left, SystemUiOverlayStyle right) {
            return Equals(left, right);
        }

        public static bool operator !=(SystemUiOverlayStyle left, SystemUiOverlayStyle right) {
            return !Equals(left, right);
        }
    }
}