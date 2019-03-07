using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;

namespace UIWidgetsGallery.gallery {
    public class GalleryTheme {
        GalleryTheme(string name, ThemeData data) {
            this.name = name;
            this.data = data;
        }

        public readonly string name;
        public readonly ThemeData data;

        public static readonly GalleryTheme kDarkGalleryTheme = new GalleryTheme("Dark", _buildDarkTheme());
        
        public static readonly GalleryTheme kLightGalleryTheme = new GalleryTheme("Light", _buildLightTheme());

        static TextTheme _buildTextTheme(TextTheme baseTheme) {
            return baseTheme.copyWith(
                title: baseTheme.title.copyWith(
                    fontFamily: "GoogleSans"
                )
            );
        }

        static ThemeData _buildDarkTheme() {
            Color primaryColor = new Color(0xFF0175c2);
            Color secondaryColor = new Color(0xFF13B9FD);
            ThemeData baseTheme = ThemeData.dark();
            ColorScheme colorScheme = ColorScheme.dark().copyWith(
                primary: primaryColor,
                secondary: secondaryColor
            );
            return baseTheme.copyWith(
                primaryColor: primaryColor,
                buttonColor: primaryColor,
                indicatorColor: Colors.white,
                accentColor: secondaryColor,
                canvasColor: new Color(0xFF202124),
                scaffoldBackgroundColor: new Color(0xFF202124),
                backgroundColor: new Color(0xFF202124),
                errorColor: new Color(0xFFB00020),
                buttonTheme: new ButtonThemeData(
                    colorScheme: colorScheme,
                    textTheme: ButtonTextTheme.primary
                ),
                textTheme: _buildTextTheme(baseTheme.textTheme),
                primaryTextTheme: _buildTextTheme(baseTheme.primaryTextTheme),
                accentTextTheme: _buildTextTheme(baseTheme.accentTextTheme)
            );
        }

        static ThemeData _buildLightTheme() {
            Color primaryColor = new Color(0xFF0175c2);
            Color secondaryColor = new Color(0xFF13B9FD);
            ColorScheme colorScheme = ColorScheme.light().copyWith(
                primary: primaryColor,
                secondary: secondaryColor
            );
            ThemeData baseTheme = ThemeData.light();
            return baseTheme.copyWith(
                colorScheme: colorScheme,
                primaryColor: primaryColor,
                buttonColor: primaryColor,
                indicatorColor: Colors.white,
                splashColor: Colors.white24,
                splashFactory: InkRipple.splashFactory,
                accentColor: secondaryColor,
                canvasColor: Colors.white,
                scaffoldBackgroundColor: Colors.white,
                backgroundColor: Colors.white,
                errorColor: new Color(0xFFB00020),
                buttonTheme: new ButtonThemeData(
                    colorScheme: colorScheme,
                    textTheme: ButtonTextTheme.primary
                ),
                textTheme: _buildTextTheme(baseTheme.textTheme),
                primaryTextTheme: _buildTextTheme(baseTheme.primaryTextTheme),
                accentTextTheme: _buildTextTheme(baseTheme.accentTextTheme)
            );
        }
    }
}
