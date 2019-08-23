using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoTextThemeDataUtils {
        public static readonly TextStyle _kDefaultLightTextStyle = new TextStyle(
            inherit: false,
            fontFamily: ".SF Pro Text",
            fontSize: 17.0f,
            letterSpacing: -0.41f,
            color: CupertinoColors.black,
            decoration: TextDecoration.none
        );

        public static readonly TextStyle _kDefaultDarkTextStyle = new TextStyle(
            inherit: false,
            fontFamily: ".SF Pro Text",
            fontSize: 17.0f,
            letterSpacing: -0.41f,
            color: CupertinoColors.white,
            decoration: TextDecoration.none
        );

        public static readonly TextStyle _kDefaultActionTextStyle = new TextStyle(
            inherit: false,
            fontFamily: ".SF Pro Text",
            fontSize: 17.0f,
            letterSpacing: -0.41f,
            color: CupertinoColors.activeBlue,
            decoration: TextDecoration.none
        );

        public static readonly TextStyle _kDefaultTabLabelTextStyle = new TextStyle(
            inherit: false,
            fontFamily: ".SF Pro Text",
            fontSize: 10.0f,
            letterSpacing: -0.24f,
            color: CupertinoColors.inactiveGray
        );

        public static readonly TextStyle _kDefaultMiddleTitleLightTextStyle = new TextStyle(
            inherit: false,
            fontFamily: ".SF Pro Text",
            fontSize: 17.0f,
            fontWeight: FontWeight.w600,
            letterSpacing: -0.41f,
            color: CupertinoColors.black
        );

        public static readonly TextStyle _kDefaultMiddleTitleDarkTextStyle = new TextStyle(
            inherit: false,
            fontFamily: ".SF Pro Text",
            fontSize: 17.0f,
            fontWeight: FontWeight.w600,
            letterSpacing: -0.41f,
            color: CupertinoColors.white
        );

        public static readonly TextStyle _kDefaultLargeTitleLightTextStyle = new TextStyle(
            inherit: false,
            fontFamily: ".SF Pro Display",
            fontSize: 34.0f,
            fontWeight: FontWeight.w700,
            letterSpacing: 0.41f,
            color: CupertinoColors.black
        );

        public static readonly TextStyle _kDefaultLargeTitleDarkTextStyle = new TextStyle(
            inherit: false,
            fontFamily: ".SF Pro Display",
            fontSize: 34.0f,
            fontWeight: FontWeight.w700,
            letterSpacing: 0.41f,
            color: CupertinoColors.white
        );
    }


    public class CupertinoTextThemeData : Diagnosticable {
        public CupertinoTextThemeData(
            Color primaryColor = null,
            Brightness? brightness = null,
            TextStyle textStyle = null,
            TextStyle actionTextStyle = null,
            TextStyle tabLabelTextStyle = null,
            TextStyle navTitleTextStyle = null,
            TextStyle navLargeTitleTextStyle = null,
            TextStyle navActionTextStyle = null
        ) {
            this._primaryColor = primaryColor ?? CupertinoColors.activeBlue;
            this._brightness = brightness;
            this._textStyle = textStyle;
            this._actionTextStyle = actionTextStyle;
            this._tabLabelTextStyle = tabLabelTextStyle;
            this._navTitleTextStyle = navTitleTextStyle;
            this._navLargeTitleTextStyle = navLargeTitleTextStyle;
            this._navActionTextStyle = navActionTextStyle;
        }

        readonly Color _primaryColor;
        readonly Brightness? _brightness;

        bool _isLight {
            get { return this._brightness != Brightness.dark; }
        }

        readonly TextStyle _textStyle;

        public TextStyle textStyle {
            get {
                return this._textStyle ?? (this._isLight
                           ? CupertinoTextThemeDataUtils._kDefaultLightTextStyle
                           : CupertinoTextThemeDataUtils._kDefaultDarkTextStyle);
            }
        }

        readonly TextStyle _actionTextStyle;

        public TextStyle actionTextStyle {
            get {
                return this._actionTextStyle ?? CupertinoTextThemeDataUtils._kDefaultActionTextStyle.copyWith(
                           color: this._primaryColor
                       );
            }
        }

        readonly TextStyle _tabLabelTextStyle;

        public TextStyle tabLabelTextStyle {
            get { return this._tabLabelTextStyle ?? CupertinoTextThemeDataUtils._kDefaultTabLabelTextStyle; }
        }

        readonly TextStyle _navTitleTextStyle;

        public TextStyle navTitleTextStyle {
            get {
                return this._navTitleTextStyle ??
                       (this._isLight
                           ? CupertinoTextThemeDataUtils._kDefaultMiddleTitleLightTextStyle
                           : CupertinoTextThemeDataUtils._kDefaultMiddleTitleDarkTextStyle);
            }
        }

        readonly TextStyle _navLargeTitleTextStyle;

        /// Typography of large titles in sliver navigation bars.
        public TextStyle navLargeTitleTextStyle {
            get {
                return this._navLargeTitleTextStyle ??
                       (this._isLight
                           ? CupertinoTextThemeDataUtils._kDefaultLargeTitleLightTextStyle
                           : CupertinoTextThemeDataUtils._kDefaultLargeTitleDarkTextStyle);
            }
        }

        readonly TextStyle _navActionTextStyle;

        public TextStyle navActionTextStyle {
            get {
                return this._navActionTextStyle ?? CupertinoTextThemeDataUtils._kDefaultActionTextStyle.copyWith(
                           color: this._primaryColor
                       );
            }
        }

        public CupertinoTextThemeData copyWith(
            Color primaryColor,
            Brightness? brightness,
            TextStyle textStyle,
            TextStyle actionTextStyle,
            TextStyle tabLabelTextStyle,
            TextStyle navTitleTextStyle,
            TextStyle navLargeTitleTextStyle,
            TextStyle navActionTextStyle
        ) {
            return new CupertinoTextThemeData(
                primaryColor: primaryColor ?? this._primaryColor,
                brightness: brightness ?? this._brightness,
                textStyle: textStyle ?? this._textStyle,
                actionTextStyle: actionTextStyle ?? this._actionTextStyle,
                tabLabelTextStyle: tabLabelTextStyle ?? this._tabLabelTextStyle,
                navTitleTextStyle: navTitleTextStyle ?? this._navTitleTextStyle,
                navLargeTitleTextStyle: navLargeTitleTextStyle ?? this._navLargeTitleTextStyle,
                navActionTextStyle: navActionTextStyle ?? this._navActionTextStyle
            );
        }
    }
}