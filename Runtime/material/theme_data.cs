using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public enum MaterialTapTargetSize {
        padded,

        shrinkWrap
    }

    public class ThemeData : Diagnosticable {
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
            IconThemeData iconTheme = null,
            IconThemeData primaryIconTheme = null,
            IconThemeData accentIconTheme = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            ColorScheme colorScheme = null,
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
            cursorColor = cursorColor ?? Color.fromRGBO(66, 133, 244, 1.0);
            textSelectionHandleColor =
                textSelectionHandleColor ?? (isDark ? Colors.tealAccent[400] : primarySwatch[300]);

            backgroundColor = backgroundColor ?? (isDark ? Colors.grey[700] : primarySwatch[200]);
            dialogBackgroundColor = dialogBackgroundColor ?? (isDark ? Colors.grey[800] : Colors.white);
            indicatorColor = indicatorColor ?? (accentColor == primaryColor ? Colors.white : accentColor);
            hintColor = hintColor ?? (isDark ? new Color(0x80FFFFFF) : new Color(0x8A000000));
            errorColor = errorColor ?? Colors.red[700];

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
            D.assert(iconTheme != null);
            D.assert(primaryIconTheme != null);
            D.assert(accentIconTheme != null);
            D.assert(materialTapTargetSize != null);
            D.assert(colorScheme != null);
            D.assert(typography != null);

            D.assert(buttonColor != null);

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
            this.iconTheme = iconTheme;
            this.primaryIconTheme = primaryIconTheme;
            this.accentIconTheme = accentIconTheme;
            this.materialTapTargetSize = materialTapTargetSize ?? MaterialTapTargetSize.padded;
            this.colorScheme = colorScheme;
            this.typography = typography;
        }

        public static ThemeData raw(
            Brightness? brightness,
            Color primaryColor,
            Brightness? primaryColorBrightness,
            Color primaryColorLight,
            Color primaryColorDark,
            Color canvasColor,
            Color accentColor,
            Brightness? accentColorBrightness,
            Color scaffoldBackgroundColor,
            Color bottomAppBarColor,
            Color cardColor,
            Color dividerColor,
            Color highlightColor,
            Color splashColor,
            InteractiveInkFeatureFactory splashFactory,
            Color selectedRowColor,
            Color unselectedWidgetColor,
            Color disabledColor,
            ButtonThemeData buttonTheme,
            Color buttonColor,
            Color secondaryHeaderColor,
            Color textSelectionColor,
            Color cursorColor,
            Color textSelectionHandleColor,
            Color backgroundColor,
            Color dialogBackgroundColor,
            Color indicatorColor,
            Color hintColor,
            Color errorColor,
            Color toggleableActiveColor,
            TextTheme textTheme,
            TextTheme primaryTextTheme,
            TextTheme accentTextTheme,
            IconThemeData iconTheme,
            IconThemeData primaryIconTheme,
            IconThemeData accentIconTheme,
            MaterialTapTargetSize? materialTapTargetSize,
            ColorScheme colorScheme,
            Typography typography
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
            D.assert(iconTheme != null);
            D.assert(primaryIconTheme != null);
            D.assert(accentIconTheme != null);
            D.assert(materialTapTargetSize != null);
            D.assert(colorScheme != null);
            D.assert(typography != null);

            D.assert(buttonColor != null);

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
                iconTheme: iconTheme,
                primaryIconTheme: primaryIconTheme,
                accentIconTheme: accentIconTheme,
                materialTapTargetSize: materialTapTargetSize,
                colorScheme: colorScheme,
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

        public readonly IconThemeData iconTheme;

        public readonly IconThemeData primaryIconTheme;

        public readonly IconThemeData accentIconTheme;

        public readonly MaterialTapTargetSize materialTapTargetSize;

        public readonly ColorScheme colorScheme;

        public readonly Typography typography;

        public ThemeData copyWith(
            Brightness? brightness,
            Color primaryColor,
            Brightness? primaryColorBrightness,
            Color primaryColorLight,
            Color primaryColorDark,
            Color accentColor,
            Brightness? accentColorBrightness,
            Color canvasColor,
            Color scaffoldBackgroundColor,
            Color bottomAppBarColor,
            Color cardColor,
            Color dividerColor,
            Color highlightColor,
            Color splashColor,
            InteractiveInkFeatureFactory splashFactory,
            Color selectedRowColor,
            Color unselectedWidgetColor,
            Color disabledColor,
            ButtonThemeData buttonTheme,
            Color buttonColor,
            Color secondaryHeaderColor,
            Color textSelectionColor,
            Color cursorColor,
            Color textSelectionHandleColor,
            Color backgroundColor,
            Color dialogBackgroundColor,
            Color indicatorColor,
            Color hintColor,
            Color errorColor,
            Color toggleableActiveColor,
            TextTheme textTheme,
            TextTheme primaryTextTheme,
            TextTheme accentTextTheme,
            IconThemeData iconTheme,
            IconThemeData primaryIconTheme,
            IconThemeData accentIconTheme,
            MaterialTapTargetSize? materialTapTargetSize,
            ColorScheme colorScheme,
            Typography typography
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
                iconTheme: iconTheme ?? this.iconTheme,
                primaryIconTheme: primaryIconTheme ?? this.primaryIconTheme,
                accentIconTheme: accentIconTheme ?? this.accentIconTheme,
                materialTapTargetSize: materialTapTargetSize ?? this.materialTapTargetSize,
                colorScheme: colorScheme ?? this.colorScheme,
                typography: typography ?? this.typography
            );
        }


        public static Brightness estimateBrightnessForColor(Color color) {
            double relativeLuminance = color.computeLuminance();
            double kThreshold = 0.15;
            if ((relativeLuminance + 0.05) * (relativeLuminance + 0.05) > kThreshold) {
                return Brightness.light;
            }

            return Brightness.dark;
        }

        public static ThemeData lerp(ThemeData a, ThemeData b, double t) {
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
                iconTheme: IconThemeData.lerp(a.iconTheme, b.iconTheme, t),
                primaryIconTheme: IconThemeData.lerp(a.primaryIconTheme, b.primaryIconTheme, t),
                accentIconTheme: IconThemeData.lerp(a.accentIconTheme, b.accentIconTheme, t),
                materialTapTargetSize: t < 0.5 ? a.materialTapTargetSize : b.materialTapTargetSize,
                colorScheme: ColorScheme.lerp(a.colorScheme, b.colorScheme, t),
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
                   other.toggleableActiveColor == this.toggleableActiveColor &&
                   other.iconTheme == this.iconTheme &&
                   other.primaryIconTheme == this.primaryIconTheme &&
                   other.accentIconTheme == this.accentIconTheme &&
                   other.materialTapTargetSize == this.materialTapTargetSize &&
                   other.colorScheme == this.colorScheme &&
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

        public override int GetHashCode() {
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
                hashCode = (hashCode * 397) ^ this.iconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.primaryIconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.accentIconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.materialTapTargetSize.GetHashCode();
                hashCode = (hashCode * 397) ^ this.colorScheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.typography.GetHashCode();
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ThemeData defaultData = fallback();
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
            properties.add(new DiagnosticsProperty<Color>("toggleableActiveColor", this.toggleableActiveColor,
                defaultValue: defaultData.toggleableActiveColor));
            properties.add(new DiagnosticsProperty<IconThemeData>("iconTheme", this.iconTheme));
            properties.add(new DiagnosticsProperty<IconThemeData>("primaryIconTheme", this.primaryIconTheme));
            properties.add(new DiagnosticsProperty<IconThemeData>("accentIconTheme", this.accentIconTheme));
            properties.add(
                new DiagnosticsProperty<MaterialTapTargetSize>("materialTapTargetSize", this.materialTapTargetSize));
            properties.add(new DiagnosticsProperty<ColorScheme>("colorScheme", this.colorScheme,
                defaultValue: defaultData.colorScheme));
            properties.add(new DiagnosticsProperty<Typography>("typography", this.typography,
                defaultValue: defaultData.typography));
        }
    }
}