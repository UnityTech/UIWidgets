using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public enum ScriptCategory {
        englishLike,

        dense,

        tall
    }


    public class Typography : Diagnosticable {
        public Typography(
            TextTheme black = null,
            TextTheme white = null,
            TextTheme englishLike = null,
            TextTheme dense = null,
            TextTheme tall = null
        ) {
            black = black ?? blackMountainView;
            white = white ?? whiteMountainView;
            englishLike = englishLike ?? englishLike2014;
            dense = dense ?? dense2014;
            tall = tall ?? tall2014;

            D.assert(black != null);
            D.assert(white != null);
            D.assert(englishLike != null);
            D.assert(dense != null);
            D.assert(tall != null);

            this.black = black;
            this.white = white;
            this.englishLike = englishLike;
            this.dense = dense;
            this.tall = tall;
        }


        public readonly TextTheme black;

        public readonly TextTheme white;

        public readonly TextTheme englishLike;

        public readonly TextTheme dense;

        public readonly TextTheme tall;

        public TextTheme geometryThemeFor(ScriptCategory category) {
            switch (category) {
                case ScriptCategory.englishLike:
                    return this.englishLike;
                case ScriptCategory.dense:
                    return this.dense;
                case ScriptCategory.tall:
                    return this.tall;
            }

            return null;
        }


        public Typography copyWith(
            TextTheme black = null,
            TextTheme white = null,
            TextTheme englishLike = null,
            TextTheme dense = null,
            TextTheme tall = null) {
            return new Typography(
                black: black ?? this.black,
                white: white ?? this.white,
                englishLike: englishLike ?? this.englishLike,
                dense: dense ?? this.dense,
                tall: tall ?? this.tall);
        }

        public static Typography lerp(Typography a, Typography b, float t) {
            return new Typography(
                black: TextTheme.lerp(a.black, b.black, t),
                white: TextTheme.lerp(a.white, b.white, t),
                englishLike: TextTheme.lerp(a.englishLike, b.englishLike, t),
                dense: TextTheme.lerp(a.dense, b.dense, t),
                tall: TextTheme.lerp(a.tall, b.tall, t)
            );
        }


        public bool Equals(Typography other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.black == other.black
                   && this.white == other.white
                   && this.englishLike == other.englishLike
                   && this.dense == other.dense
                   && this.tall == other.tall;
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

            return this.Equals((Typography) obj);
        }

        public static bool operator ==(Typography left, Typography right) {
            return Equals(left, right);
        }

        public static bool operator !=(Typography left, Typography right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.black.GetHashCode();
                hashCode = (hashCode * 397) ^ this.white.GetHashCode();
                hashCode = (hashCode * 397) ^ this.englishLike.GetHashCode();
                hashCode = (hashCode * 397) ^ this.dense.GetHashCode();
                hashCode = (hashCode * 397) ^ this.tall.GetHashCode();
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            Typography defaultTypography = new Typography();
            properties.add(
                new DiagnosticsProperty<TextTheme>("black", this.black, defaultValue: defaultTypography.black));
            properties.add(
                new DiagnosticsProperty<TextTheme>("white", this.white, defaultValue: defaultTypography.white));
            properties.add(new DiagnosticsProperty<TextTheme>("englishLike", this.englishLike,
                defaultValue: defaultTypography.englishLike));
            properties.add(
                new DiagnosticsProperty<TextTheme>("dense", this.dense, defaultValue: defaultTypography.dense));
            properties.add(new DiagnosticsProperty<TextTheme>("tall", this.tall, defaultValue: defaultTypography.tall));
        }

        public static readonly TextTheme blackMountainView = new TextTheme(
            display4: new TextStyle(debugLabel: "blackMountainView display4", fontFamily: "Roboto", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            display3: new TextStyle(debugLabel: "blackMountainView display3", fontFamily: "Roboto", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            display2: new TextStyle(debugLabel: "blackMountainView display2", fontFamily: "Roboto", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            display1: new TextStyle(debugLabel: "blackMountainView display1", fontFamily: "Roboto", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            headline: new TextStyle(debugLabel: "blackMountainView headline", fontFamily: "Roboto", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            title: new TextStyle(debugLabel: "blackMountainView title", fontFamily: "Roboto", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            subhead: new TextStyle(debugLabel: "blackMountainView subhead", fontFamily: "Roboto", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            body2: new TextStyle(debugLabel: "blackMountainView body2", fontFamily: "Roboto", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            body1: new TextStyle(debugLabel: "blackMountainView body1", fontFamily: "Roboto", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            caption: new TextStyle(debugLabel: "blackMountainView caption", fontFamily: "Roboto", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            button: new TextStyle(debugLabel: "blackMountainView button", fontFamily: "Roboto", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            subtitle: new TextStyle(debugLabel: "blackMountainView subtitle", fontFamily: "Roboto", inherit: true,
                color: Colors.black, decoration: TextDecoration.none),
            overline: new TextStyle(debugLabel: "blackMountainView overline", fontFamily: "Roboto", inherit: true,
                color: Colors.black, decoration: TextDecoration.none)
        );

        public static readonly TextTheme whiteMountainView = new TextTheme(
            display4: new TextStyle(debugLabel: "whiteMountainView display4", fontFamily: "Roboto", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            display3: new TextStyle(debugLabel: "whiteMountainView display3", fontFamily: "Roboto", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            display2: new TextStyle(debugLabel: "whiteMountainView display2", fontFamily: "Roboto", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            display1: new TextStyle(debugLabel: "whiteMountainView display1", fontFamily: "Roboto", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            headline: new TextStyle(debugLabel: "whiteMountainView headline", fontFamily: "Roboto", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            title: new TextStyle(debugLabel: "whiteMountainView title", fontFamily: "Roboto", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            subhead: new TextStyle(debugLabel: "whiteMountainView subhead", fontFamily: "Roboto", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            body2: new TextStyle(debugLabel: "whiteMountainView body2", fontFamily: "Roboto", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            body1: new TextStyle(debugLabel: "whiteMountainView body1", fontFamily: "Roboto", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            caption: new TextStyle(debugLabel: "whiteMountainView caption", fontFamily: "Roboto", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            button: new TextStyle(debugLabel: "whiteMountainView button", fontFamily: "Roboto", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            subtitle: new TextStyle(debugLabel: "whiteMountainView subtitle", fontFamily: "Roboto", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            overline: new TextStyle(debugLabel: "whiteMountainView overline", fontFamily: "Roboto", inherit: true,
                color: Colors.white, decoration: TextDecoration.none)
        );

        public static readonly TextTheme blackCupertino = new TextTheme(
            display4: new TextStyle(debugLabel: "blackCupertino display4", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            display3: new TextStyle(debugLabel: "blackCupertino display3", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            display2: new TextStyle(debugLabel: "blackCupertino display2", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            display1: new TextStyle(debugLabel: "blackCupertino display1", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            headline: new TextStyle(debugLabel: "blackCupertino headline", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            title: new TextStyle(debugLabel: "blackCupertino title", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            subhead: new TextStyle(debugLabel: "blackCupertino subhead", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            body2: new TextStyle(debugLabel: "blackCupertino body2", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            body1: new TextStyle(debugLabel: "blackCupertino body1", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            caption: new TextStyle(debugLabel: "blackCupertino caption", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.black54, decoration: TextDecoration.none),
            button: new TextStyle(debugLabel: "blackCupertino button", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.black87, decoration: TextDecoration.none),
            subtitle: new TextStyle(debugLabel: "blackCupertino subtitle", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.black, decoration: TextDecoration.none),
            overline: new TextStyle(debugLabel: "blackCupertino overline", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.black, decoration: TextDecoration.none)
        );

        public static readonly TextTheme whiteCupertino = new TextTheme(
            display4: new TextStyle(debugLabel: "whiteCupertino display4", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            display3: new TextStyle(debugLabel: "whiteCupertino display3", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            display2: new TextStyle(debugLabel: "whiteCupertino display2", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            display1: new TextStyle(debugLabel: "whiteCupertino display1", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            headline: new TextStyle(debugLabel: "whiteCupertino headline", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            title: new TextStyle(debugLabel: "whiteCupertino title", fontFamily: ".SF UI Display", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            subhead: new TextStyle(debugLabel: "whiteCupertino subhead", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            body2: new TextStyle(debugLabel: "whiteCupertino body2", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            body1: new TextStyle(debugLabel: "whiteCupertino body1", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            caption: new TextStyle(debugLabel: "whiteCupertino caption", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.white70, decoration: TextDecoration.none),
            button: new TextStyle(debugLabel: "whiteCupertino button", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            subtitle: new TextStyle(debugLabel: "whiteCupertino subtitle", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.white, decoration: TextDecoration.none),
            overline: new TextStyle(debugLabel: "whiteCupertino overline", fontFamily: ".SF UI Text", inherit: true,
                color: Colors.white, decoration: TextDecoration.none)
        );


        public static readonly TextTheme englishLike2014 = new TextTheme(
            display4: new TextStyle(debugLabel: "englishLike display4 2014", inherit: false, fontSize: 112.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            display3: new TextStyle(debugLabel: "englishLike display3 2014", inherit: false, fontSize: 56.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            display2: new TextStyle(debugLabel: "englishLike display2 2014", inherit: false, fontSize: 45.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            display1: new TextStyle(debugLabel: "englishLike display1 2014", inherit: false, fontSize: 34.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            headline: new TextStyle(debugLabel: "englishLike headline 2014", inherit: false, fontSize: 24.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            title: new TextStyle(debugLabel: "englishLike title 2014", inherit: false, fontSize: 20.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            subhead: new TextStyle(debugLabel: "englishLike subhead 2014", inherit: false, fontSize: 16.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            body2: new TextStyle(debugLabel: "englishLike body2 2014", inherit: false, fontSize: 14.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            body1: new TextStyle(debugLabel: "englishLike body1 2014", inherit: false, fontSize: 14.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            caption: new TextStyle(debugLabel: "englishLike caption 2014", inherit: false, fontSize: 12.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            button: new TextStyle(debugLabel: "englishLike button 2014", inherit: false, fontSize: 14.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            subtitle: new TextStyle(debugLabel: "englishLike subtitle 2014", inherit: false, fontSize: 14.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.1f),
            overline: new TextStyle(debugLabel: "englishLike overline 2014", inherit: false, fontSize: 10.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 1.5f)
        );

        public static readonly TextTheme englishLike2018 = new TextTheme(
            display4: new TextStyle(debugLabel: "englishLike display4 2018", fontSize: 96.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: -1.5f),
            display3: new TextStyle(debugLabel: "englishLike display3 2018", fontSize: 60.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: -0.5f),
            display2: new TextStyle(debugLabel: "englishLike display2 2018", fontSize: 48.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.0f),
            display1: new TextStyle(debugLabel: "englishLike display1 2018", fontSize: 34.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.25f),
            headline: new TextStyle(debugLabel: "englishLike headline 2018", fontSize: 24.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.0f),
            title: new TextStyle(debugLabel: "englishLike title 2018", fontSize: 20.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic, letterSpacing: 0.15f),
            subhead: new TextStyle(debugLabel: "englishLike subhead 2018", fontSize: 16.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic, letterSpacing: 0.15f),
            body2: new TextStyle(debugLabel: "englishLike body2 2018", fontSize: 14.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic, letterSpacing: 0.25f),
            body1: new TextStyle(debugLabel: "englishLike body1 2018", fontSize: 16.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic, letterSpacing: 0.5f),
            button: new TextStyle(debugLabel: "englishLike button 2018", fontSize: 14.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic, letterSpacing: 0.75f),
            caption: new TextStyle(debugLabel: "englishLike caption 2018", fontSize: 12.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic, letterSpacing: 0.4f),
            subtitle: new TextStyle(debugLabel: "englishLike subtitle 2018", fontSize: 14.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.1f),
            overline: new TextStyle(debugLabel: "englishLike overline 2018", fontSize: 10.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 1.5f)
        );

        public static readonly TextTheme dense2014 = new TextTheme(
            display4: new TextStyle(debugLabel: "dense display4 2014", inherit: false, fontSize: 112.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            display3: new TextStyle(debugLabel: "dense display3 2014", inherit: false, fontSize: 56.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            display2: new TextStyle(debugLabel: "dense display2 2014", inherit: false, fontSize: 45.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            display1: new TextStyle(debugLabel: "dense display1 2014", inherit: false, fontSize: 34.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            headline: new TextStyle(debugLabel: "dense headline 2014", inherit: false, fontSize: 24.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            title: new TextStyle(debugLabel: "dense title 2014", inherit: false, fontSize: 21.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            subhead: new TextStyle(debugLabel: "dense subhead 2014", inherit: false, fontSize: 17.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            body2: new TextStyle(debugLabel: "dense body2 2014", inherit: false, fontSize: 15.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            body1: new TextStyle(debugLabel: "dense body1 2014", inherit: false, fontSize: 15.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            caption: new TextStyle(debugLabel: "dense caption 2014", inherit: false, fontSize: 13.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            button: new TextStyle(debugLabel: "dense button 2014", inherit: false, fontSize: 15.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            subtitle: new TextStyle(debugLabel: "dense subtitle 2014", inherit: false, fontSize: 15.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
            overline: new TextStyle(debugLabel: "dense overline 2014", inherit: false, fontSize: 11.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic)
        );

        public static readonly TextTheme dense2018 = new TextTheme(
            display4: new TextStyle(debugLabel: "dense display4 2018", fontSize: 96.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            display3: new TextStyle(debugLabel: "dense display3 2018", fontSize: 60.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            display2: new TextStyle(debugLabel: "dense display2 2018", fontSize: 48.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            display1: new TextStyle(debugLabel: "dense display1 2018", fontSize: 34.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            headline: new TextStyle(debugLabel: "dense headline 2018", fontSize: 24.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            title: new TextStyle(debugLabel: "dense title 2018", fontSize: 21.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            subhead: new TextStyle(debugLabel: "dense subhead 2018", fontSize: 17.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            body2: new TextStyle(debugLabel: "dense body2 2018", fontSize: 17.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            body1: new TextStyle(debugLabel: "dense body1 2018", fontSize: 15.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            caption: new TextStyle(debugLabel: "dense caption 2018", fontSize: 13.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            button: new TextStyle(debugLabel: "dense button 2018", fontSize: 15.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            subtitle: new TextStyle(debugLabel: "dense subtitle 2018", fontSize: 15.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic),
            overline: new TextStyle(debugLabel: "dense overline 2018", fontSize: 11.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.ideographic)
        );


        public static readonly TextTheme tall2014 = new TextTheme(
            display4: new TextStyle(debugLabel: "tall display4 2014", inherit: false, fontSize: 112.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            display3: new TextStyle(debugLabel: "tall display3 2014", inherit: false, fontSize: 56.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            display2: new TextStyle(debugLabel: "tall display2 2014", inherit: false, fontSize: 45.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            display1: new TextStyle(debugLabel: "tall display1 2014", inherit: false, fontSize: 34.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            headline: new TextStyle(debugLabel: "tall headline 2014", inherit: false, fontSize: 24.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            title: new TextStyle(debugLabel: "tall title 2014", inherit: false, fontSize: 21.0f,
                fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
            subhead: new TextStyle(debugLabel: "tall subhead 2014", inherit: false, fontSize: 17.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            body2: new TextStyle(debugLabel: "tall body2 2014", inherit: false, fontSize: 15.0f,
                fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
            body1: new TextStyle(debugLabel: "tall body1 2014", inherit: false, fontSize: 15.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            caption: new TextStyle(debugLabel: "tall caption 2014", inherit: false, fontSize: 13.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            button: new TextStyle(debugLabel: "tall button 2014", inherit: false, fontSize: 15.0f,
                fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
            subtitle: new TextStyle(debugLabel: "tall subtitle 2014", inherit: false, fontSize: 15.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
            overline: new TextStyle(debugLabel: "tall overline 2014", inherit: false, fontSize: 11.0f,
                fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic)
        );

        public static readonly TextTheme tall2018 = new TextTheme(
            display4: new TextStyle(debugLabel: "tall display4 2018", fontSize: 96.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            display3: new TextStyle(debugLabel: "tall display3 2018", fontSize: 60.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            display2: new TextStyle(debugLabel: "tall display2 2018", fontSize: 48.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            display1: new TextStyle(debugLabel: "tall display1 2018", fontSize: 34.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            headline: new TextStyle(debugLabel: "tall headline 2018", fontSize: 24.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            title: new TextStyle(debugLabel: "tall title 2018", fontSize: 21.0f, fontWeight: FontWeight.w700,
                textBaseline: TextBaseline.alphabetic),
            subhead: new TextStyle(debugLabel: "tall subhead 2018", fontSize: 17.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            body2: new TextStyle(debugLabel: "tall body2 2018", fontSize: 17.0f, fontWeight: FontWeight.w700,
                textBaseline: TextBaseline.alphabetic),
            body1: new TextStyle(debugLabel: "tall body1 2018", fontSize: 15.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            button: new TextStyle(debugLabel: "tall button 2018", fontSize: 15.0f, fontWeight: FontWeight.w700,
                textBaseline: TextBaseline.alphabetic),
            caption: new TextStyle(debugLabel: "tall caption 2018", fontSize: 13.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            subtitle: new TextStyle(debugLabel: "tall subtitle 2018", fontSize: 15.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic),
            overline: new TextStyle(debugLabel: "tall overline 2018", fontSize: 11.0f, fontWeight: FontWeight.w400,
                textBaseline: TextBaseline.alphabetic)
        );
    }
}