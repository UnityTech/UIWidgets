using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using UnityEngine;

namespace Unity.UIWidgets.material {

    public enum MaterialTapTargetSize {
        padded,
        
        shrinkWrap
    }

    public class ThemeData : Diagnosticable {
        public ThemeData(
            Brightness brightness,
            MaterialColor primarySwatch,
            Color primaryColor,
            Brightness primaryColorBrightness,
            Color primaryColorLight,
            Color primaryColorDark,
            Color accentColor,
            Brightness accentColorBrightness,
            Color canvasColor,
            Color scaffoldBackgroundColor,
            Color bottomAppBarColor,
            Color cardColor,
            Color dividerColor,
            Color highlightColor,
            Color splashColor
        ) {
            
        }


        readonly Brightness brightness;

        readonly MaterialColor primarySwatch;

        readonly Color primaryColor;

        readonly Brightness primaryColorBrightness;

        readonly Color primaryColorLight;

        readonly Color primaryColorDark;

        readonly Color accentColor;

        readonly Brightness accentColorBrightness;

        readonly Color canvasColor;

        readonly Color scaffoldBackgroundColor;

        readonly Color bottomAppBarColor;

        readonly Color cardColor;

        readonly Color dividerColor;

        readonly Color highlightColor;

        readonly Color splashColor;
    }
}