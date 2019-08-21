using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    static class ThemeDataUtils {
        public static readonly Color _kLightThemeHighlightColor = new Color(0x66BCBCBC);

        public static readonly Color _kLightThemeSplashColor = new Color(0x66C8C8C8);

        public static readonly Color _kDarkThemeHighlightColor = new Color(0x40CCCCCC);

        public static readonly Color _kDarkThemeSplashColor = new Color(0x40CCCCCC);
    }

    public enum MaterialTapTargetSize {
        padded,

        shrinkWrap
    }

    public class ThemeData : Diagnosticable, IEquatable<ThemeData> {
        public ThemeData(
            Brightness? brightness = null,
            MaterialColor primarySwatch = null,
            Color primaryColor = null,
            Brightness? primaryColorBrightness = null,
            Color primaryColorLight = null,
            Color primaryColorDark = null,
            Color accentColor = null,
            Brightness? accentColorBrightness = null,
            Color canvasColor = null,
            Color scaffoldBackgroundColor = null,
            Color bottomAppBarColor = null,
            Color cardColor = null,
            Color dividerColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            Color selectedRowColor = null,
            Color unselectedWidgetColor = null,
            Color disabledColor = null,
            Color buttonColor = null,
            ButtonThemeData buttonTheme = null,
            Color secondaryHeaderColor = null,
            Color textSelectionColor = null,
            Color cursorColor = null,
            Color textSelectionHandleColor = null,
            Color backgroundColor = null,
            Color dialogBackgroundColor = null,
            Color indicatorColor = null,
            Color hintColor = null,
            Color errorColor = null,
            Color toggleableActiveColor = null,
            string fontFamily = null,
            TextTheme textTheme = null,
            TextTheme primaryTextTheme = null,
            TextTheme accentTextTheme = null,
            InputDecorationTheme inputDecorationTheme = null,
            IconThemeData iconTheme = null,
            IconThemeData primaryIconTheme = null,
            IconThemeData accentIconTheme = null,
            SliderThemeData sliderTheme = null,
            TabBarTheme tabBarTheme = null,
            CardTheme cardTheme = null,
            ChipThemeData chipTheme = null,
            RuntimePlatform? platform = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            PageTransitionsTheme pageTransitionsTheme = null,
            AppBarTheme appBarTheme = null,
            BottomAppBarTheme bottomAppBarTheme = null,
            ColorScheme colorScheme = null,
            DialogTheme dialogTheme = null,
            FloatingActionButtonThemeData floatingActionButtonTheme = null,
            Typography typography = null
        ) {
            brightness = brightness ?? Brightness.light;
            bool isDark = brightness == Brightness.dark;

            primarySwatch = primarySwatch ?? Colors.blue;
            primaryColor = primaryColor ?? (isDark ? Colors.grey[900] : primarySwatch);
            primaryColorBrightness = primaryColorBrightness ?? estimateBrightnessForColor(primaryColor);
            primaryColorLight = primaryColorLight ?? (isDark ? Colors.grey[500] : primarySwatch[100]);
            primaryColorDark = primaryColorDark ?? (isDark ? Colors.black : primarySwatch[700]);
            bool primaryIsDark = primaryColorBrightness == Brightness.dark;
            toggleableActiveColor = toggleableActiveColor ??
                                    (isDark ? Colors.tealAccent[200] : (accentColor ?? primarySwatch[600]));

            accentColor = accentColor ?? (isDark ? Colors.tealAccent[200] : primarySwatch[500]);
            accentColorBrightness = accentColorBrightness ?? estimateBrightnessForColor(accentColor);
            bool accentIsDark = accentColorBrightness == Brightness.dark;

            canvasColor = canvasColor ?? (isDark ? Colors.grey[850] : Colors.grey[50]);
            scaffoldBackgroundColor = scaffoldBackgroundColor ?? canvasColor;
            bottomAppBarColor = bottomAppBarColor ?? (isDark ? Colors.grey[800] : Colors.white);
            cardColor = cardColor ?? (isDark ? Colors.grey[800] : Colors.white);
            dividerColor = dividerColor ?? (isDark ? new Color(0x1FFFFFFF) : new Color(0x1F000000));

            colorScheme = colorScheme ?? ColorScheme.fromSwatch(
                              primarySwatch: primarySwatch,
                              primaryColorDark: primaryColorDark,
                              accentColor: accentColor,
                              cardColor: cardColor,
                              backgroundColor: backgroundColor,
                              errorColor: errorColor,
                              brightness: brightness);

            splashFactory = splashFactory ?? InkSplash.splashFactory;
            selectedRowColor = selectedRowColor ?? Colors.grey[100];
            unselectedWidgetColor = unselectedWidgetColor ?? (isDark ? Colors.white70 : Colors.black54);
            secondaryHeaderColor = secondaryHeaderColor ?? (isDark ? Colors.grey[700] : primarySwatch[50]);
            textSelectionColor = textSelectionColor ?? (isDark ? accentColor : primarySwatch[200]);
            cursorColor = cursorColor ?? Color.fromRGBO(66, 133, 244, 1.0f);
            textSelectionHandleColor =
                textSelectionHandleColor ?? (isDark ? Colors.tealAccent[400] : primarySwatch[300]);

            backgroundColor = backgroundColor ?? (isDark ? Colors.grey[700] : primarySwatch[200]);
            dialogBackgroundColor = dialogBackgroundColor ?? (isDark ? Colors.grey[800] : Colors.white);
            indicatorColor = indicatorColor ?? (accentColor == primaryColor ? Colors.white : accentColor);
            hintColor = hintColor ?? (isDark ? new Color(0x80FFFFFF) : new Color(0x8A000000));
            errorColor = errorColor ?? Colors.red[700];
            inputDecorationTheme = inputDecorationTheme ?? new InputDecorationTheme();
            pageTransitionsTheme = pageTransitionsTheme ?? new PageTransitionsTheme();
            appBarTheme = appBarTheme ?? new AppBarTheme();
            bottomAppBarTheme = bottomAppBarTheme ?? new BottomAppBarTheme();
            primaryIconTheme = primaryIconTheme ??
                               (primaryIsDark
                                   ? new IconThemeData(color: Colors.white)
                                   : new IconThemeData(color: Colors.black));
            accentIconTheme = accentIconTheme ??
                              (accentIsDark
                                  ? new IconThemeData(color: Colors.white)
                                  : new IconThemeData(color: Colors.black));
            iconTheme = iconTheme ??
                        (isDark ? new IconThemeData(color: Colors.white) : new IconThemeData(color: Colors.black87));
            platform = platform ?? Application.platform;
            typography = typography ?? new Typography();
            TextTheme defaultTextTheme = isDark ? typography.white : typography.black;
            textTheme = defaultTextTheme.merge(textTheme);
            TextTheme defaultPrimaryTextTheme = primaryIsDark ? typography.white : typography.black;
            primaryTextTheme = defaultPrimaryTextTheme.merge(primaryTextTheme);
            TextTheme defaultAccentTextTheme = accentIsDark ? typography.white : typography.black;
            accentTextTheme = defaultAccentTextTheme.merge(accentTextTheme);
            materialTapTargetSize = materialTapTargetSize ?? MaterialTapTargetSize.padded;
            if (fontFamily != null) {
                textTheme = textTheme.apply(fontFamily: fontFamily);
                primaryTextTheme = primaryTextTheme.apply(fontFamily: fontFamily);
                accentTextTheme = accentTextTheme.apply(fontFamily: fontFamily);
            }

            buttonColor = buttonColor ?? (isDark ? primarySwatch[600] : Colors.grey[300]);
            buttonTheme = buttonTheme ?? new ButtonThemeData(
                              colorScheme: colorScheme,
                              buttonColor: buttonColor,
                              disabledColor: disabledColor,
                              highlightColor: highlightColor,
                              splashColor: splashColor,
                              materialTapTargetSize: materialTapTargetSize);
            disabledColor = disabledColor ?? (isDark ? Colors.white30 : Colors.black38);
            highlightColor = highlightColor ??
                             (isDark
                                 ? ThemeDataUtils._kDarkThemeHighlightColor
                                 : ThemeDataUtils._kLightThemeHighlightColor);
            splashColor = splashColor ??
                          (isDark
                              ? ThemeDataUtils._kDarkThemeSplashColor
                              : ThemeDataUtils._kLightThemeSplashColor);

            sliderTheme = sliderTheme ?? SliderThemeData.fromPrimaryColors(
                              primaryColor: primaryColor,
                              primaryColorLight: primaryColorLight,
                              primaryColorDark: primaryColorDark,
                              valueIndicatorTextStyle: accentTextTheme.body2);

            tabBarTheme = tabBarTheme ?? new TabBarTheme();
            cardTheme = cardTheme ?? new CardTheme();
            chipTheme = chipTheme ?? ChipThemeData.fromDefaults(
                secondaryColor: primaryColor,
                brightness: brightness,
                labelStyle: textTheme.body2
            );
            dialogTheme = dialogTheme ?? new DialogTheme();
            floatingActionButtonTheme = floatingActionButtonTheme ?? new FloatingActionButtonThemeData();

            D.assert(brightness != null);
            D.assert(primaryColor != null);
            D.assert(primaryColorBrightness != null);
            D.assert(primaryColorLight != null);
            D.assert(primaryColorDark != null);
            D.assert(accentColor != null);
            D.assert(accentColorBrightness != null);
            D.assert(canvasColor != null);
            D.assert(scaffoldBackgroundColor != null);
            D.assert(bottomAppBarColor != null);
            D.assert(cardColor != null);
            D.assert(dividerColor != null);
            D.assert(highlightColor != null);
            D.assert(splashColor != null);
            D.assert(splashFactory != null);
            D.assert(selectedRowColor != null);
            D.assert(unselectedWidgetColor != null);
            D.assert(disabledColor != null);
            D.assert(toggleableActiveColor != null);
            D.assert(buttonTheme != null);
            D.assert(secondaryHeaderColor != null);
            D.assert(textSelectionColor != null);
            D.assert(cursorColor != null);
            D.assert(textSelectionHandleColor != null);
            D.assert(backgroundColor != null);
            D.assert(dialogBackgroundColor != null);
            D.assert(indicatorColor != null);
            D.assert(hintColor != null);
            D.assert(errorColor != null);
            D.assert(textTheme != null);
            D.assert(primaryTextTheme != null);
            D.assert(accentTextTheme != null);
            D.assert(sliderTheme != null);
            D.assert(iconTheme != null);
            D.assert(primaryIconTheme != null);
            D.assert(accentIconTheme != null);
            D.assert(materialTapTargetSize != null);
            D.assert(pageTransitionsTheme != null);
            D.assert(appBarTheme != null);
            D.assert(bottomAppBarTheme != null);
            D.assert(colorScheme != null);
            D.assert(typography != null);
            D.assert(buttonColor != null);
            D.assert(tabBarTheme != null);
            D.assert(cardTheme != null);
            D.assert(chipTheme != null);
            D.assert(dialogTheme != null);
            D.assert(floatingActionButtonTheme != null);

            this.brightness = brightness ?? Brightness.light;
            this.primaryColor = primaryColor;
            this.primaryColorBrightness = primaryColorBrightness ?? Brightness.light;
            this.primaryColorLight = primaryColorLight;
            this.primaryColorDark = primaryColorDark;
            this.canvasColor = canvasColor;
            this.accentColor = accentColor;
            this.accentColorBrightness = accentColorBrightness ?? Brightness.light;
            this.scaffoldBackgroundColor = scaffoldBackgroundColor;
            this.bottomAppBarColor = bottomAppBarColor;
            this.cardColor = cardColor;
            this.dividerColor = dividerColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.splashFactory = splashFactory;
            this.selectedRowColor = selectedRowColor;
            this.unselectedWidgetColor = unselectedWidgetColor;
            this.disabledColor = disabledColor;
            this.buttonTheme = buttonTheme;
            this.buttonColor = buttonColor;
            this.secondaryHeaderColor = secondaryHeaderColor;
            this.textSelectionColor = textSelectionColor;
            this.cursorColor = cursorColor;
            this.textSelectionHandleColor = textSelectionHandleColor;
            this.backgroundColor = backgroundColor;
            this.dialogBackgroundColor = dialogBackgroundColor;
            this.indicatorColor = indicatorColor;
            this.hintColor = hintColor;
            this.errorColor = errorColor;
            this.toggleableActiveColor = toggleableActiveColor;
            this.textTheme = textTheme;
            this.primaryTextTheme = primaryTextTheme;
            this.accentTextTheme = accentTextTheme;
            this.inputDecorationTheme = inputDecorationTheme;
            this.iconTheme = iconTheme;
            this.primaryIconTheme = primaryIconTheme;
            this.accentIconTheme = accentIconTheme;
            this.sliderTheme = sliderTheme;
            this.tabBarTheme = tabBarTheme;
            this.cardTheme = cardTheme;
            this.chipTheme = chipTheme;
            this.platform = platform.Value;
            this.materialTapTargetSize = materialTapTargetSize ?? MaterialTapTargetSize.padded;
            this.pageTransitionsTheme = pageTransitionsTheme;
            this.appBarTheme = appBarTheme;
            this.bottomAppBarTheme = bottomAppBarTheme;
            this.colorScheme = colorScheme;
            this.dialogTheme = dialogTheme;
            this.floatingActionButtonTheme = floatingActionButtonTheme;
            this.typography = typography;
        }

        public static ThemeData raw(
            Brightness? brightness = null,
            Color primaryColor = null,
            Brightness? primaryColorBrightness = null,
            Color primaryColorLight = null,
            Color primaryColorDark = null,
            Color canvasColor = null,
            Color accentColor = null,
            Brightness? accentColorBrightness = null,
            Color scaffoldBackgroundColor = null,
            Color bottomAppBarColor = null,
            Color cardColor = null,
            Color dividerColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            Color selectedRowColor = null,
            Color unselectedWidgetColor = null,
            Color disabledColor = null,
            ButtonThemeData buttonTheme = null,
            Color buttonColor = null,
            Color secondaryHeaderColor = null,
            Color textSelectionColor = null,
            Color cursorColor = null,
            Color textSelectionHandleColor = null,
            Color backgroundColor = null,
            Color dialogBackgroundColor = null,
            Color indicatorColor = null,
            Color hintColor = null,
            Color errorColor = null,
            Color toggleableActiveColor = null,
            TextTheme textTheme = null,
            TextTheme primaryTextTheme = null,
            TextTheme accentTextTheme = null,
            SliderThemeData sliderTheme = null,
            InputDecorationTheme inputDecorationTheme = null,
            IconThemeData iconTheme = null,
            IconThemeData primaryIconTheme = null,
            IconThemeData accentIconTheme = null,
            TabBarTheme tabBarTheme = null,
            CardTheme cardTheme = null,
            ChipThemeData chipTheme = null,
            RuntimePlatform? platform = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            PageTransitionsTheme pageTransitionsTheme = null,
            AppBarTheme appBarTheme = null,
            BottomAppBarTheme bottomAppBarTheme = null,
            ColorScheme colorScheme = null,
            DialogTheme dialogTheme = null,
            FloatingActionButtonThemeData floatingActionButtonTheme = null,
            Typography typography = null
        ) {
            D.assert(brightness != null);
            D.assert(primaryColor != null);
            D.assert(primaryColorBrightness != null);
            D.assert(primaryColorLight != null);
            D.assert(primaryColorDark != null);
            D.assert(accentColor != null);
            D.assert(accentColorBrightness != null);
            D.assert(canvasColor != null);
            D.assert(scaffoldBackgroundColor != null);
            D.assert(bottomAppBarColor != null);
            D.assert(cardColor != null);
            D.assert(dividerColor != null);
            D.assert(highlightColor != null);
            D.assert(splashColor != null);
            D.assert(splashFactory != null);
            D.assert(selectedRowColor != null);
            D.assert(unselectedWidgetColor != null);
            D.assert(disabledColor != null);
            D.assert(toggleableActiveColor != null);
            D.assert(buttonTheme != null);
            D.assert(secondaryHeaderColor != null);
            D.assert(textSelectionColor != null);
            D.assert(cursorColor != null);
            D.assert(textSelectionHandleColor != null);
            D.assert(backgroundColor != null);
            D.assert(dialogBackgroundColor != null);
            D.assert(indicatorColor != null);
            D.assert(hintColor != null);
            D.assert(errorColor != null);
            D.assert(textTheme != null);
            D.assert(primaryTextTheme != null);
            D.assert(accentTextTheme != null);
            D.assert(sliderTheme != null);
            D.assert(inputDecorationTheme != null);
            D.assert(iconTheme != null);
            D.assert(primaryIconTheme != null);
            D.assert(accentIconTheme != null);
            D.assert(platform != null);
            D.assert(materialTapTargetSize != null);
            D.assert(pageTransitionsTheme != null);
            D.assert(appBarTheme != null);
            D.assert(bottomAppBarTheme != null);
            D.assert(colorScheme != null);
            D.assert(typography != null);
            D.assert(buttonColor != null);
            D.assert(tabBarTheme != null);
            D.assert(cardTheme != null);
            D.assert(chipTheme != null);
            D.assert(dialogTheme != null);
            D.assert(floatingActionButtonTheme != null);

            return new ThemeData(
                brightness: brightness,
                primaryColor: primaryColor,
                primaryColorBrightness: primaryColorBrightness,
                primaryColorLight: primaryColorLight,
                primaryColorDark: primaryColorDark,
                accentColor: accentColor,
                accentColorBrightness: accentColorBrightness,
                canvasColor: canvasColor,
                scaffoldBackgroundColor: scaffoldBackgroundColor,
                bottomAppBarColor: bottomAppBarColor,
                cardColor: cardColor,
                dividerColor: dividerColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                splashFactory: splashFactory,
                selectedRowColor: selectedRowColor,
                unselectedWidgetColor: unselectedWidgetColor,
                disabledColor: disabledColor,
                buttonTheme: buttonTheme,
                buttonColor: buttonColor,
                toggleableActiveColor: toggleableActiveColor,
                secondaryHeaderColor: secondaryHeaderColor,
                textSelectionColor: textSelectionColor,
                cursorColor: cursorColor,
                textSelectionHandleColor: textSelectionHandleColor,
                backgroundColor: backgroundColor,
                dialogBackgroundColor: dialogBackgroundColor,
                indicatorColor: indicatorColor,
                hintColor: hintColor,
                errorColor: errorColor,
                textTheme: textTheme,
                primaryTextTheme: primaryTextTheme,
                accentTextTheme: accentTextTheme,
                inputDecorationTheme: inputDecorationTheme,
                iconTheme: iconTheme,
                primaryIconTheme: primaryIconTheme,
                accentIconTheme: accentIconTheme,
                sliderTheme: sliderTheme,
                tabBarTheme: tabBarTheme,
                cardTheme: cardTheme,
                chipTheme: chipTheme,
                platform: platform,
                materialTapTargetSize: materialTapTargetSize,
                pageTransitionsTheme: pageTransitionsTheme,
                appBarTheme: appBarTheme,
                bottomAppBarTheme: bottomAppBarTheme,
                colorScheme: colorScheme,
                dialogTheme: dialogTheme,
                floatingActionButtonTheme: floatingActionButtonTheme,
                typography: typography);
        }

        public static ThemeData light() {
            return new ThemeData(brightness: Brightness.light);
        }

        public static ThemeData dark() {
            return new ThemeData(brightness: Brightness.dark);
        }

        public static ThemeData fallback() {
            return light();
        }


        public readonly Brightness brightness;

        public readonly Color primaryColor;

        public readonly Brightness primaryColorBrightness;

        public readonly Color primaryColorLight;

        public readonly Color primaryColorDark;

        public readonly Color canvasColor;

        public readonly Color accentColor;

        public readonly Brightness accentColorBrightness;

        public readonly Color scaffoldBackgroundColor;

        public readonly Color bottomAppBarColor;

        public readonly Color cardColor;

        public readonly Color dividerColor;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly InteractiveInkFeatureFactory splashFactory;

        public readonly Color selectedRowColor;

        public readonly Color unselectedWidgetColor;

        public readonly Color disabledColor;

        public readonly ButtonThemeData buttonTheme;

        public readonly Color buttonColor;

        public readonly Color secondaryHeaderColor;

        public readonly Color textSelectionColor;

        public readonly Color cursorColor;

        public readonly Color textSelectionHandleColor;

        public readonly Color backgroundColor;

        public readonly Color dialogBackgroundColor;

        public readonly Color indicatorColor;

        public readonly Color hintColor;

        public readonly Color errorColor;

        public readonly Color toggleableActiveColor;

        public readonly TextTheme textTheme;

        public readonly TextTheme primaryTextTheme;

        public readonly TextTheme accentTextTheme;

        public readonly SliderThemeData sliderTheme;

        public readonly InputDecorationTheme inputDecorationTheme;

        public readonly IconThemeData iconTheme;

        public readonly IconThemeData primaryIconTheme;

        public readonly IconThemeData accentIconTheme;

        public readonly TabBarTheme tabBarTheme;
        
        public readonly CardTheme cardTheme;
        public readonly ChipThemeData chipTheme;

        public readonly RuntimePlatform platform;

        public readonly MaterialTapTargetSize materialTapTargetSize;

        public readonly PageTransitionsTheme pageTransitionsTheme;

        public readonly AppBarTheme appBarTheme;
        
        public readonly BottomAppBarTheme bottomAppBarTheme;

        public readonly ColorScheme colorScheme;

        public readonly DialogTheme dialogTheme;
        
        public readonly FloatingActionButtonThemeData floatingActionButtonTheme;

        public readonly Typography typography;

        public ThemeData copyWith(
            Brightness? brightness = null,
            Color primaryColor = null,
            Brightness? primaryColorBrightness = null,
            Color primaryColorLight = null,
            Color primaryColorDark = null,
            Color accentColor = null,
            Brightness? accentColorBrightness = null,
            Color canvasColor = null,
            Color scaffoldBackgroundColor = null,
            Color bottomAppBarColor = null,
            Color cardColor = null,
            Color dividerColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            Color selectedRowColor = null,
            Color unselectedWidgetColor = null,
            Color disabledColor = null,
            ButtonThemeData buttonTheme = null,
            Color buttonColor = null,
            Color secondaryHeaderColor = null,
            Color textSelectionColor = null,
            Color cursorColor = null,
            Color textSelectionHandleColor = null,
            Color backgroundColor = null,
            Color dialogBackgroundColor = null,
            Color indicatorColor = null,
            Color hintColor = null,
            Color errorColor = null,
            Color toggleableActiveColor = null,
            TextTheme textTheme = null,
            TextTheme primaryTextTheme = null,
            TextTheme accentTextTheme = null,
            SliderThemeData sliderTheme = null,
            InputDecorationTheme inputDecorationTheme = null,
            IconThemeData iconTheme = null,
            IconThemeData primaryIconTheme = null,
            IconThemeData accentIconTheme = null,
            TabBarTheme tabBarTheme = null,
            CardTheme cardTheme = null,
            ChipThemeData chipTheme = null,
            RuntimePlatform? platform = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            PageTransitionsTheme pageTransitionsTheme = null,
            AppBarTheme appBarTheme = null,
            BottomAppBarTheme bottomAppBarTheme = null,
            ColorScheme colorScheme = null,
            DialogTheme dialogTheme = null,
            FloatingActionButtonThemeData floatingActionButtonTheme = null,
            Typography typography = null
        ) {
            return raw(
                brightness: brightness ?? this.brightness,
                primaryColor: primaryColor ?? this.primaryColor,
                primaryColorBrightness: primaryColorBrightness ?? this.primaryColorBrightness,
                primaryColorLight: primaryColorLight ?? this.primaryColorLight,
                primaryColorDark: primaryColorDark ?? this.primaryColorDark,
                accentColor: accentColor ?? this.accentColor,
                accentColorBrightness: accentColorBrightness ?? this.accentColorBrightness,
                canvasColor: canvasColor ?? this.canvasColor,
                scaffoldBackgroundColor: scaffoldBackgroundColor ?? this.scaffoldBackgroundColor,
                bottomAppBarColor: bottomAppBarColor ?? this.bottomAppBarColor,
                cardColor: cardColor ?? this.cardColor,
                dividerColor: dividerColor ?? this.dividerColor,
                highlightColor: highlightColor ?? this.highlightColor,
                splashColor: splashColor ?? this.splashColor,
                splashFactory: splashFactory ?? this.splashFactory,
                selectedRowColor: selectedRowColor ?? this.selectedRowColor,
                unselectedWidgetColor: unselectedWidgetColor ?? this.unselectedWidgetColor,
                disabledColor: disabledColor ?? this.disabledColor,
                buttonTheme: buttonTheme ?? this.buttonTheme,
                buttonColor: buttonColor ?? this.buttonColor,
                secondaryHeaderColor: secondaryHeaderColor ?? this.secondaryHeaderColor,
                textSelectionColor: textSelectionColor ?? this.textSelectionColor,
                cursorColor: cursorColor ?? this.cursorColor,
                textSelectionHandleColor: textSelectionHandleColor ?? this.textSelectionHandleColor,
                backgroundColor: backgroundColor ?? this.backgroundColor,
                dialogBackgroundColor: dialogBackgroundColor ?? this.dialogBackgroundColor,
                indicatorColor: indicatorColor ?? this.indicatorColor,
                hintColor: hintColor ?? this.hintColor,
                errorColor: errorColor ?? this.errorColor,
                toggleableActiveColor: toggleableActiveColor ?? this.toggleableActiveColor,
                textTheme: textTheme ?? this.textTheme,
                primaryTextTheme: primaryTextTheme ?? this.primaryTextTheme,
                accentTextTheme: accentTextTheme ?? this.accentTextTheme,
                sliderTheme: sliderTheme ?? this.sliderTheme,
                inputDecorationTheme: this.inputDecorationTheme ?? this.inputDecorationTheme,
                iconTheme: iconTheme ?? this.iconTheme,
                primaryIconTheme: primaryIconTheme ?? this.primaryIconTheme,
                accentIconTheme: accentIconTheme ?? this.accentIconTheme,
                tabBarTheme: tabBarTheme ?? this.tabBarTheme,
                cardTheme: cardTheme ?? this.cardTheme,
                chipTheme: chipTheme ?? this.chipTheme,
                platform: platform ?? this.platform,
                materialTapTargetSize: materialTapTargetSize ?? this.materialTapTargetSize,
                pageTransitionsTheme: pageTransitionsTheme ?? this.pageTransitionsTheme,
                appBarTheme: appBarTheme ?? this.appBarTheme,
                bottomAppBarTheme: bottomAppBarTheme ?? this.bottomAppBarTheme,
                colorScheme: colorScheme ?? this.colorScheme,
                dialogTheme: dialogTheme ?? this.dialogTheme,
                floatingActionButtonTheme: floatingActionButtonTheme ?? this.floatingActionButtonTheme,
                typography: typography ?? this.typography
            );
        }

        const int _localizedThemeDataCacheSize = 5;

        static readonly _FifoCache<_IdentityThemeDataCacheKey, ThemeData> _localizedThemeDataCache =
            new _FifoCache<_IdentityThemeDataCacheKey, ThemeData>(_localizedThemeDataCacheSize);


        public static ThemeData localize(ThemeData baseTheme, TextTheme localTextGeometry) {
            D.assert(baseTheme != null);
            D.assert(localTextGeometry != null);

            return _localizedThemeDataCache.putIfAbsent(
                new _IdentityThemeDataCacheKey(baseTheme, localTextGeometry),
                () => {
                    return baseTheme.copyWith(
                        primaryTextTheme: localTextGeometry.merge(baseTheme.primaryTextTheme),
                        accentTextTheme: localTextGeometry.merge(baseTheme.accentTextTheme),
                        textTheme: localTextGeometry.merge(baseTheme.textTheme)
                    );
                });
        }

        public static Brightness estimateBrightnessForColor(Color color) {
            float relativeLuminance = color.computeLuminance();
            float kThreshold = 0.15f;
            if ((relativeLuminance + 0.05f) * (relativeLuminance + 0.05f) > kThreshold) {
                return Brightness.light;
            }

            return Brightness.dark;
        }

        public static ThemeData lerp(ThemeData a, ThemeData b, float t) {
            D.assert(a != null);
            D.assert(b != null);
            return raw(
                brightness: t < 0.5 ? a.brightness : b.brightness,
                primaryColor: Color.lerp(a.primaryColor, b.primaryColor, t),
                primaryColorBrightness: t < 0.5 ? a.primaryColorBrightness : b.primaryColorBrightness,
                primaryColorLight: Color.lerp(a.primaryColorLight, b.primaryColorLight, t),
                primaryColorDark: Color.lerp(a.primaryColorDark, b.primaryColorDark, t),
                canvasColor: Color.lerp(a.canvasColor, b.canvasColor, t),
                accentColor: Color.lerp(a.accentColor, b.accentColor, t),
                accentColorBrightness: t < 0.5 ? a.accentColorBrightness : b.accentColorBrightness,
                scaffoldBackgroundColor: Color.lerp(a.scaffoldBackgroundColor, b.scaffoldBackgroundColor, t),
                bottomAppBarColor: Color.lerp(a.bottomAppBarColor, b.bottomAppBarColor, t),
                cardColor: Color.lerp(a.cardColor, b.cardColor, t),
                dividerColor: Color.lerp(a.dividerColor, b.dividerColor, t),
                highlightColor: Color.lerp(a.highlightColor, b.highlightColor, t),
                splashColor: Color.lerp(a.splashColor, b.splashColor, t),
                splashFactory: t < 0.5 ? a.splashFactory : b.splashFactory,
                selectedRowColor: Color.lerp(a.selectedRowColor, b.selectedRowColor, t),
                unselectedWidgetColor: Color.lerp(a.unselectedWidgetColor, b.unselectedWidgetColor, t),
                disabledColor: Color.lerp(a.disabledColor, b.disabledColor, t),
                buttonTheme: t < 0.5 ? a.buttonTheme : b.buttonTheme,
                buttonColor: Color.lerp(a.buttonColor, b.buttonColor, t),
                secondaryHeaderColor: Color.lerp(a.secondaryHeaderColor, b.secondaryHeaderColor, t),
                textSelectionColor: Color.lerp(a.textSelectionColor, b.textSelectionColor, t),
                cursorColor: Color.lerp(a.cursorColor, b.cursorColor, t),
                textSelectionHandleColor: Color.lerp(a.textSelectionHandleColor, b.textSelectionHandleColor, t),
                backgroundColor: Color.lerp(a.backgroundColor, b.backgroundColor, t),
                dialogBackgroundColor: Color.lerp(a.dialogBackgroundColor, b.dialogBackgroundColor, t),
                indicatorColor: Color.lerp(a.indicatorColor, b.indicatorColor, t),
                hintColor: Color.lerp(a.hintColor, b.hintColor, t),
                errorColor: Color.lerp(a.errorColor, b.errorColor, t),
                toggleableActiveColor: Color.lerp(a.toggleableActiveColor, b.toggleableActiveColor, t),
                textTheme: TextTheme.lerp(a.textTheme, b.textTheme, t),
                primaryTextTheme: TextTheme.lerp(a.primaryTextTheme, b.primaryTextTheme, t),
                accentTextTheme: TextTheme.lerp(a.accentTextTheme, b.accentTextTheme, t),
                inputDecorationTheme: t < 0.5f ? a.inputDecorationTheme : b.inputDecorationTheme,
                iconTheme: IconThemeData.lerp(a.iconTheme, b.iconTheme, t),
                primaryIconTheme: IconThemeData.lerp(a.primaryIconTheme, b.primaryIconTheme, t),
                accentIconTheme: IconThemeData.lerp(a.accentIconTheme, b.accentIconTheme, t),
                sliderTheme: SliderThemeData.lerp(a.sliderTheme, b.sliderTheme, t),
                tabBarTheme: TabBarTheme.lerp(a.tabBarTheme, b.tabBarTheme, t),
                cardTheme: CardTheme.lerp(a.cardTheme, b.cardTheme, t),
                chipTheme: ChipThemeData.lerp(a.chipTheme, b.chipTheme, t),
                platform: t < 0.5 ? a.platform : b.platform,
                materialTapTargetSize: t < 0.5 ? a.materialTapTargetSize : b.materialTapTargetSize,
                pageTransitionsTheme: t < 0.5 ? a.pageTransitionsTheme : b.pageTransitionsTheme,
                appBarTheme: AppBarTheme.lerp(a.appBarTheme, b.appBarTheme, t),
                bottomAppBarTheme: BottomAppBarTheme.lerp(a.bottomAppBarTheme, b.bottomAppBarTheme, t),
                colorScheme: ColorScheme.lerp(a.colorScheme, b.colorScheme, t),
                dialogTheme: DialogTheme.lerp(a.dialogTheme, b.dialogTheme, t),
                floatingActionButtonTheme: FloatingActionButtonThemeData.lerp(a.floatingActionButtonTheme, b.floatingActionButtonTheme, t),
                typography: Typography.lerp(a.typography, b.typography, t)
            );
        }

        public bool Equals(ThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.brightness == this.brightness &&
                   other.primaryColor == this.primaryColor &&
                   other.primaryColorBrightness == this.primaryColorBrightness &&
                   other.primaryColorLight == this.primaryColorLight &&
                   other.primaryColorDark == this.primaryColorDark &&
                   other.accentColor == this.accentColor &&
                   other.accentColorBrightness == this.accentColorBrightness &&
                   other.canvasColor == this.canvasColor &&
                   other.scaffoldBackgroundColor == this.scaffoldBackgroundColor &&
                   other.bottomAppBarColor == this.bottomAppBarColor &&
                   other.cardColor == this.cardColor &&
                   other.dividerColor == this.dividerColor &&
                   other.highlightColor == this.highlightColor &&
                   other.splashColor == this.splashColor &&
                   other.splashFactory == this.splashFactory &&
                   other.selectedRowColor == this.selectedRowColor &&
                   other.unselectedWidgetColor == this.unselectedWidgetColor &&
                   other.disabledColor == this.disabledColor &&
                   other.buttonTheme == this.buttonTheme &&
                   other.buttonColor == this.buttonColor &&
                   other.secondaryHeaderColor == this.secondaryHeaderColor &&
                   other.textSelectionColor == this.textSelectionColor &&
                   other.cursorColor == this.cursorColor &&
                   other.textSelectionHandleColor == this.textSelectionHandleColor &&
                   other.backgroundColor == this.backgroundColor &&
                   other.dialogBackgroundColor == this.dialogBackgroundColor &&
                   other.indicatorColor == this.indicatorColor &&
                   other.hintColor == this.hintColor &&
                   other.errorColor == this.errorColor &&
                   other.textTheme == this.textTheme &&
                   other.primaryTextTheme == this.primaryTextTheme &&
                   other.accentTextTheme == this.accentTextTheme &&
                   other.sliderTheme == this.sliderTheme &&
                   other.inputDecorationTheme == this.inputDecorationTheme &&
                   other.toggleableActiveColor == this.toggleableActiveColor &&
                   other.iconTheme == this.iconTheme &&
                   other.primaryIconTheme == this.primaryIconTheme &&
                   other.accentIconTheme == this.accentIconTheme &&
                   other.tabBarTheme == this.tabBarTheme &&
                   other.cardTheme == this.cardTheme &&
                   other.chipTheme == this.chipTheme &&
                   other.platform == this.platform &&
                   other.materialTapTargetSize == this.materialTapTargetSize &&
                   other.pageTransitionsTheme == this.pageTransitionsTheme &&
                   other.appBarTheme == this.appBarTheme &&
                   other.bottomAppBarTheme == this.bottomAppBarTheme &&
                   other.colorScheme == this.colorScheme &&
                   other.dialogTheme == this.dialogTheme &&
                   other.floatingActionButtonTheme == this.floatingActionButtonTheme &&
                   other.typography == this.typography;
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

            return this.Equals((ThemeData) obj);
        }

        public static bool operator ==(ThemeData left, ThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(ThemeData left, ThemeData right) {
            return !Equals(left, right);
        }

        int? _cachedHashCode = null;

        public override int GetHashCode() {
            if (this._cachedHashCode != null) {
                return this._cachedHashCode.Value;
            }

            unchecked {
                var hashCode = this.brightness.GetHashCode();
                hashCode = (hashCode * 397) ^ this.primaryColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.primaryColorBrightness.GetHashCode();
                hashCode = (hashCode * 397) ^ this.primaryColorLight.GetHashCode();
                hashCode = (hashCode * 397) ^ this.primaryColorDark.GetHashCode();
                hashCode = (hashCode * 397) ^ this.canvasColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.accentColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.accentColorBrightness.GetHashCode();
                hashCode = (hashCode * 397) ^ this.scaffoldBackgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottomAppBarColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.cardColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.dividerColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.highlightColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.splashColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.splashFactory.GetHashCode();
                hashCode = (hashCode * 397) ^ this.selectedRowColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.unselectedWidgetColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.disabledColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.buttonTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.buttonColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.secondaryHeaderColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.textSelectionColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.cursorColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.textSelectionHandleColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.backgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.dialogBackgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.indicatorColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.hintColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.errorColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.toggleableActiveColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.textTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.primaryTextTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.accentTextTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.inputDecorationTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.iconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.primaryIconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.accentIconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.sliderTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.tabBarTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.cardTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.chipTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.platform.GetHashCode();
                hashCode = (hashCode * 397) ^ this.materialTapTargetSize.GetHashCode();
                hashCode = (hashCode * 397) ^ this.pageTransitionsTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.appBarTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottomAppBarTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.colorScheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.dialogTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.floatingActionButtonTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.typography.GetHashCode();

                this._cachedHashCode = hashCode;
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ThemeData defaultData = fallback();
            properties.add(new EnumProperty<RuntimePlatform>("platform", this.platform,
                defaultValue: Application.platform));
            properties.add(new EnumProperty<Brightness>("brightness", this.brightness,
                defaultValue: defaultData.brightness));
            properties.add(new DiagnosticsProperty<Color>("primaryColor", this.primaryColor,
                defaultValue: defaultData.primaryColor));
            properties.add(new EnumProperty<Brightness>("primaryColorBrightness", this.primaryColorBrightness,
                defaultValue: defaultData.primaryColorBrightness));
            properties.add(new DiagnosticsProperty<Color>("accentColor", this.accentColor,
                defaultValue: defaultData.accentColor));
            properties.add(new EnumProperty<Brightness>("accentColorBrightness", this.accentColorBrightness,
                defaultValue: defaultData.accentColorBrightness));
            properties.add(new DiagnosticsProperty<Color>("canvasColor", this.canvasColor,
                defaultValue: defaultData.canvasColor));
            properties.add(new DiagnosticsProperty<Color>("scaffoldBackgroundColor", this.scaffoldBackgroundColor,
                defaultValue: defaultData.scaffoldBackgroundColor));
            properties.add(new DiagnosticsProperty<Color>("bottomAppBarColor", this.bottomAppBarColor,
                defaultValue: defaultData.bottomAppBarColor));
            properties.add(new DiagnosticsProperty<Color>("cardColor", this.cardColor,
                defaultValue: defaultData.cardColor));
            properties.add(new DiagnosticsProperty<Color>("dividerColor", this.dividerColor,
                defaultValue: defaultData.dividerColor));
            properties.add(new DiagnosticsProperty<Color>("highlightColor", this.highlightColor,
                defaultValue: defaultData.highlightColor));
            properties.add(new DiagnosticsProperty<Color>("splashColor", this.splashColor,
                defaultValue: defaultData.splashColor));
            properties.add(new DiagnosticsProperty<Color>("selectedRowColor", this.selectedRowColor,
                defaultValue: defaultData.selectedRowColor));
            properties.add(new DiagnosticsProperty<Color>("unselectedWidgetColor", this.unselectedWidgetColor,
                defaultValue: defaultData.unselectedWidgetColor));
            properties.add(new DiagnosticsProperty<Color>("disabledColor", this.disabledColor,
                defaultValue: defaultData.disabledColor));
            properties.add(new DiagnosticsProperty<ButtonThemeData>("buttonTheme", this.buttonTheme));
            properties.add(new DiagnosticsProperty<Color>("buttonColor", this.buttonColor,
                defaultValue: defaultData.buttonColor));
            properties.add(new DiagnosticsProperty<Color>("secondaryHeaderColor", this.secondaryHeaderColor,
                defaultValue: defaultData.secondaryHeaderColor));
            properties.add(new DiagnosticsProperty<Color>("textSelectionColor", this.textSelectionColor,
                defaultValue: defaultData.textSelectionColor));
            properties.add(new DiagnosticsProperty<Color>("cursorColor", this.cursorColor,
                defaultValue: defaultData.cursorColor));
            properties.add(new DiagnosticsProperty<Color>("textSelectionHandleColor", this.textSelectionHandleColor,
                defaultValue: defaultData.textSelectionHandleColor));
            properties.add(new DiagnosticsProperty<Color>("backgroundColor", this.backgroundColor,
                defaultValue: defaultData.backgroundColor));
            properties.add(new DiagnosticsProperty<Color>("dialogBackgroundColor", this.dialogBackgroundColor,
                defaultValue: defaultData.dialogBackgroundColor));
            properties.add(new DiagnosticsProperty<Color>("indicatorColor", this.indicatorColor,
                defaultValue: defaultData.indicatorColor));
            properties.add(new DiagnosticsProperty<Color>("hintColor", this.hintColor,
                defaultValue: defaultData.hintColor));
            properties.add(new DiagnosticsProperty<Color>("errorColor", this.errorColor,
                defaultValue: defaultData.errorColor));
            properties.add(new DiagnosticsProperty<TextTheme>("textTheme", this.textTheme));
            properties.add(new DiagnosticsProperty<TextTheme>("primaryTextTheme", this.primaryTextTheme));
            properties.add(new DiagnosticsProperty<TextTheme>("accentTextTheme", this.accentTextTheme));
            properties.add(
                new DiagnosticsProperty<InputDecorationTheme>("inputDecorationTheme", this.inputDecorationTheme));
            properties.add(new DiagnosticsProperty<Color>("toggleableActiveColor", this.toggleableActiveColor,
                defaultValue: defaultData.toggleableActiveColor));
            properties.add(new DiagnosticsProperty<IconThemeData>("iconTheme", this.iconTheme));
            properties.add(new DiagnosticsProperty<IconThemeData>("primaryIconTheme", this.primaryIconTheme));
            properties.add(new DiagnosticsProperty<IconThemeData>("accentIconTheme", this.accentIconTheme));
            properties.add(new DiagnosticsProperty<SliderThemeData>("sliderTheme", this.sliderTheme));
            properties.add(new DiagnosticsProperty<TabBarTheme>("tabBarTheme", this.tabBarTheme));
            properties.add(new DiagnosticsProperty<CardTheme>("cardTheme", this.cardTheme));
            properties.add(new DiagnosticsProperty<ChipThemeData>("chipTheme", this.chipTheme));
            properties.add(
                new DiagnosticsProperty<MaterialTapTargetSize>("materialTapTargetSize", this.materialTapTargetSize));
            properties.add(
                new DiagnosticsProperty<PageTransitionsTheme>("pageTransitionsTheme", this.pageTransitionsTheme));
            properties.add(new DiagnosticsProperty<AppBarTheme>("appBarTheme", this.appBarTheme));
            properties.add(new DiagnosticsProperty<BottomAppBarTheme>("bottomAppBarTheme", this.bottomAppBarTheme));
            properties.add(new DiagnosticsProperty<ColorScheme>("colorScheme", this.colorScheme,
                defaultValue: defaultData.colorScheme));
            properties.add(new DiagnosticsProperty<DialogTheme>("dialogTheme", this.dialogTheme,
                defaultValue: defaultData.dialogTheme));
            properties.add(new DiagnosticsProperty<FloatingActionButtonThemeData>("floatingActionButtonTheme",
                this.floatingActionButtonTheme, defaultValue: defaultData.floatingActionButtonTheme));
            properties.add(new DiagnosticsProperty<Typography>("typography", this.typography,
                defaultValue: defaultData.typography));
        }
    }


    class _IdentityThemeDataCacheKey : IEquatable<_IdentityThemeDataCacheKey> {
        public _IdentityThemeDataCacheKey(
            ThemeData baseTheme,
            TextTheme localTextGeometry) {
            this.baseTheme = baseTheme;
            this.localTextGeometry = localTextGeometry;
        }

        public readonly ThemeData baseTheme;

        public readonly TextTheme localTextGeometry;

        public bool Equals(_IdentityThemeDataCacheKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return ReferenceEquals(this.baseTheme, other.baseTheme) &&
                   ReferenceEquals(this.localTextGeometry, other.localTextGeometry);
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

            return this.Equals((_IdentityThemeDataCacheKey) obj);
        }

        public static bool operator ==(_IdentityThemeDataCacheKey left, _IdentityThemeDataCacheKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(_IdentityThemeDataCacheKey left, _IdentityThemeDataCacheKey right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            var hashCode = this.baseTheme.GetHashCode();
            hashCode = (hashCode * 397) ^ this.localTextGeometry.GetHashCode();
            return hashCode;
        }
    }

    class _FifoCache<K, V> {
        public _FifoCache(int maximumSize) {
            D.assert(maximumSize > 0);
            this._maximumSize = maximumSize;
        }

        readonly Dictionary<K, V> _cache = new Dictionary<K, V>();

        readonly int _maximumSize;

        public V putIfAbsent(K key, Func<V> value) {
            D.assert(key != null);
            D.assert(value != null);

            V get_value;
            if (this._cache.TryGetValue(key, out get_value)) {
                return get_value;
            }

            if (this._cache.Count == this._maximumSize) {
                this._cache.Remove(this._cache.Keys.First());
            }

            this._cache[key] = value();
            return this._cache[key];
        }
    }
}