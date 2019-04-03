using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.gallery {
    public class ShrineStyle : TextStyle {
        public ShrineStyle(bool inherit, Color color, float fontSize, FontWeight fontWeight, TextBaseline textBaseline,
            string fontFamily = null
        ) : base(inherit: inherit, color: color, fontSize: fontSize, fontWeight: fontWeight, fontFamily: fontFamily,
            textBaseline: textBaseline) {
        }

        public static ShrineStyle roboto(float size, FontWeight weight, Color color) {
            return new ShrineStyle(inherit: false, color: color, fontSize: size, fontWeight: weight,
                textBaseline: TextBaseline.alphabetic);
        }

        public static ShrineStyle abrilFatface(float size, FontWeight weight, Color color) {
            return new ShrineStyle(inherit: false, color: color, fontFamily: "AbrilFatface", fontSize: size,
                fontWeight: weight, textBaseline: TextBaseline.alphabetic);
        }
    }

    public class ShrineThemeUtils {
        public static TextStyle robotoRegular12(Color color) {
            return ShrineStyle.roboto(12.0f, FontWeight.w400, color);
        }

        public static TextStyle robotoLight12(Color color) {
            return ShrineStyle.roboto(12.0f, FontWeight.w400, color);
        }

        public static TextStyle robotoRegular14(Color color) {
            return ShrineStyle.roboto(14.0f, FontWeight.w400, color);
        }

        public static TextStyle robotoMedium14(Color color) {
            return ShrineStyle.roboto(14.0f, FontWeight.w700, color);
        }

        public static TextStyle robotoLight14(Color color) {
            return ShrineStyle.roboto(14.0f, FontWeight.w400, color);
        }

        public static TextStyle robotoRegular16(Color color) {
            return ShrineStyle.roboto(16.0f, FontWeight.w400, color);
        }

        public static TextStyle robotoRegular20(Color color) {
            return ShrineStyle.roboto(20.0f, FontWeight.w400, color);
        }

        public static TextStyle abrilFatfaceRegular24(Color color) {
            return ShrineStyle.abrilFatface(24.0f, FontWeight.w400, color);
        }

        public static TextStyle abrilFatfaceRegular34(Color color) {
            return ShrineStyle.abrilFatface(34.0f, FontWeight.w400, color);
        }
    }

    public class ShrineTheme : InheritedWidget {
        public ShrineTheme(Key key = null, Widget child = null)
            : base(key: key, child: child) {
            D.assert(child != null);
        }

        public readonly Color cardBackgroundColor = Colors.white;
        public readonly Color appBarBackgroundColor = Colors.white;
        public readonly Color dividerColor = new Color(0xFFD9D9D9);
        public readonly Color priceHighlightColor = new Color(0xFFFFE0E0);

        public readonly TextStyle appBarTitleStyle = ShrineThemeUtils.robotoRegular20(Colors.black87);
        public readonly TextStyle vendorItemStyle = ShrineThemeUtils.robotoRegular12(new Color(0xFF81959D));
        public readonly TextStyle priceStyle = ShrineThemeUtils.robotoRegular14(Colors.black87);

        public readonly TextStyle featureTitleStyle =
            ShrineThemeUtils.abrilFatfaceRegular34(new Color(0xFF0A3142));

        public readonly TextStyle featurePriceStyle = ShrineThemeUtils.robotoRegular16(Colors.black87);
        public readonly TextStyle featureStyle = ShrineThemeUtils.robotoLight14(Colors.black54);
        public readonly TextStyle orderTitleStyle = ShrineThemeUtils.abrilFatfaceRegular24(Colors.black87);
        public readonly TextStyle orderStyle = ShrineThemeUtils.robotoLight14(Colors.black54);
        public readonly TextStyle vendorTitleStyle = ShrineThemeUtils.robotoMedium14(Colors.black87);
        public readonly TextStyle vendorStyle = ShrineThemeUtils.robotoLight14(Colors.black54);
        public readonly TextStyle quantityMenuStyle = ShrineThemeUtils.robotoLight14(Colors.black54);

        public static ShrineTheme of(BuildContext context) {
            return (ShrineTheme) context.inheritFromWidgetOfExactType(typeof(ShrineTheme));
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return false;
        }
    }
}