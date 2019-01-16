using System.Collections.Generic;
using JetBrains.Annotations;
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
        bottom,
    }

    public enum Brightness {
        dark,
        light,
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
        
        public static SystemUiOverlayStyle light = new SystemUiOverlayStyle(
            systemNavigationBarColor: new Color(0xFF000000),
            systemNavigationBarDividerColor: null,
            statusBarColor: null,
            systemNavigationBarIconBrightness: Brightness.light,
            statusBarIconBrightness: Brightness.light,
            statusBarBrightness: Brightness.dark
            );
        
        public static SystemUiOverlayStyle dark = new SystemUiOverlayStyle(
            systemNavigationBarColor: new Color(0xFF000000),
            systemNavigationBarDividerColor: null,
            statusBarColor: null,
            systemNavigationBarIconBrightness: Brightness.light,
            statusBarIconBrightness: Brightness.dark,
            statusBarBrightness: Brightness.light
            );

        public Dictionary<string, object> _toMap() {
            return new Dictionary<string, object>() {
                {"systemNavigationBarColor", this.systemNavigationBarColor?.value},
                {"systemNavigationBarDividerColor", this.systemNavigationBarDividerColor?.value},
                {"statusBarColor", this.statusBarColor?.value},
                {"statusBarBrightness", this.statusBarBrightness?.ToString()},
                {"statusBarIconBrightness", this.statusBarIconBrightness?.ToString()},
                {"systemNavigationBarIconBrightness", this.systemNavigationBarIconBrightness?.ToString()}
            };
        }

        public string toString() => this._toMap().ToString();

        public SystemUiOverlayStyle copyWith(
            [CanBeNull] Color systemNavigationBarColor,
            [CanBeNull] Color systemNavigationBarDividerColor,
            [CanBeNull] Color statusBarColor,
            Brightness? statusBarBrightness,
            Brightness? statusBarIconBrightness,
            Brightness? systemNavigationBarIconBrightness
        ) {
            return new SystemUiOverlayStyle(
                systemNavigationBarColor: systemNavigationBarColor ?? this.systemNavigationBarColor,
                systemNavigationBarDividerColor: systemNavigationBarDividerColor ?? this.systemNavigationBarDividerColor,
                statusBarColor: statusBarColor ?? this.statusBarColor,
                statusBarIconBrightness: statusBarIconBrightness ?? this.statusBarIconBrightness,
                statusBarBrightness: statusBarBrightness ?? this.statusBarBrightness,
                systemNavigationBarIconBrightness: systemNavigationBarIconBrightness ?? this.systemNavigationBarIconBrightness
                );
        }

        public int GetHashCode() {
            var hashCode = this.systemNavigationBarColor == null ? 0 : this.systemNavigationBarColor.GetHashCode();
            hashCode = (hashCode * 397) ^ (this.systemNavigationBarDividerColor == null ? 0 : this.systemNavigationBarDividerColor.GetHashCode());
            hashCode = (hashCode * 397) ^ (this.statusBarColor == null ? 0 : this.statusBarColor.GetHashCode());
            hashCode = (hashCode * 397) ^ (this.statusBarBrightness == null ? 0 : this.statusBarBrightness.GetHashCode());
            hashCode = (hashCode * 397) ^ (this.statusBarIconBrightness == null ? 0 : this.statusBarIconBrightness.GetHashCode());
            hashCode = (hashCode * 397) ^ (this.systemNavigationBarIconBrightness == null ? 0 : this.systemNavigationBarIconBrightness.GetHashCode());
            return hashCode;
        }

        public static bool operator ==(SystemUiOverlayStyle a, SystemUiOverlayStyle b) {
            return a.systemNavigationBarColor == b.systemNavigationBarColor &&
                   a.systemNavigationBarDividerColor == b.systemNavigationBarDividerColor &&
                   a.statusBarColor == b.statusBarColor &&
                   a.statusBarIconBrightness == b.statusBarIconBrightness &&
                   a.statusBarBrightness == b.statusBarIconBrightness &&
                   a.systemNavigationBarIconBrightness == b.systemNavigationBarIconBrightness;
        }

        public static bool operator !=(SystemUiOverlayStyle a, SystemUiOverlayStyle b) {
            return !(a == b);
        }
    }
}