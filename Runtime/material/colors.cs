using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public class MaterialColor : ColorSwatch<int> {
        public MaterialColor(
            long primary,
            Dictionary<int, Color> swatch) : base(primary: primary, swatch: swatch) {
        }

        public Color shade50 {
            get { return this[50]; }
        }

        public Color shade100 {
            get { return this[100]; }
        }

        public Color shade200 {
            get { return this[200]; }
        }

        public Color shade300 {
            get { return this[300]; }
        }

        public Color shade400 {
            get { return this[400]; }
        }

        public Color shade500 {
            get { return this[500]; }
        }

        public Color shade600 {
            get { return this[600]; }
        }

        public Color shade700 {
            get { return this[700]; }
        }

        public Color shade800 {
            get { return this[800]; }
        }

        public Color shade900 {
            get { return this[900]; }
        }
    }


    public class MaterialAccentColor : ColorSwatch<int> {
        public MaterialAccentColor(
            long primary,
            Dictionary<int, Color> swatch) : base(primary: primary, swatch: swatch) {
        }

        public Color shade50 {
            get { return this[50]; }
        }

        public Color shade100 {
            get { return this[100]; }
        }

        public Color shade200 {
            get { return this[200]; }
        }

        public Color shade400 {
            get { return this[400]; }
        }

        public Color shade700 {
            get { return this[700]; }
        }
    }


    public class Colors {
        public static readonly Color transparent = new Color(0x00000000);

        public static readonly Color black = new Color(0xFF000000);

        public static readonly Color black87 = new Color(0xDD000000);

        public static readonly Color black54 = new Color(0x8A000000);

        public static readonly Color black45 = new Color(0x73000000);

        public static readonly Color black38 = new Color(0x61000000);

        public static readonly Color black26 = new Color(0x42000000);

        public static readonly Color black12 = new Color(0x1F000000);

        public static readonly Color white = new Color(0xFFFFFFFF);

        public static readonly Color white70 = new Color(0xB3FFFFFF);

        public static readonly Color white54 = new Color(0x8AFFFFFF);

        public static readonly Color white30 = new Color(0x4DFFFFFF);

        public static readonly Color white24 = new Color(0x3DFFFFFF);

        public static readonly Color white12 = new Color(0x1FFFFFFF);

        public static readonly Color white10 = new Color(0x1AFFFFFF);

        public static readonly MaterialColor red = new MaterialColor(
            _redPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFFFEBEE)},
                {100, new Color(0xFFFFCDD2)},
                {200, new Color(0xFFEF9A9A)},
                {300, new Color(0xFFE57373)},
                {400, new Color(0xFFEF5350)},
                {500, new Color(_redPrimaryValue)},
                {600, new Color(0xFFE53935)},
                {700, new Color(0xFFD32F2F)},
                {800, new Color(0xFFC62828)},
                {900, new Color(0xFFB71C1C)}
            }
        );

        const long _redPrimaryValue = 0xFFF44336;

        public static readonly MaterialAccentColor redAccent = new MaterialAccentColor(
            _redAccentValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFFF8A80)},
                {200, new Color(_redAccentValue)},
                {400, new Color(0xFFFF1744)},
                {700, new Color(0xFFD50000)}
            }
        );

        const long _redAccentValue = 0xFFFF5252;

        public static readonly MaterialColor pink = new MaterialColor(
            _pinkPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFFCE4EC)},
                {100, new Color(0xFFF8BBD0)},
                {200, new Color(0xFFF48FB1)},
                {300, new Color(0xFFF06292)},
                {400, new Color(0xFFEC407A)},
                {500, new Color(_pinkPrimaryValue)},
                {600, new Color(0xFFD81B60)},
                {700, new Color(0xFFC2185B)},
                {800, new Color(0xFFAD1457)},
                {900, new Color(0xFF880E4F)}
            }
        );

        const long _pinkPrimaryValue = 0xFFE91E63;


        public static readonly MaterialAccentColor pinkAccent = new MaterialAccentColor(
            _pinkAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFFF80AB)},
                {200, new Color(_pinkAccentPrimaryValue)},
                {400, new Color(0xFFF50057)},
                {700, new Color(0xFFC51162)}
            }
        );

        const long _pinkAccentPrimaryValue = 0xFFFF4081;

        public static readonly MaterialColor purple = new MaterialColor(
            _purplePrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFF3E5F5)},
                {100, new Color(0xFFE1BEE7)},
                {200, new Color(0xFFCE93D8)},
                {300, new Color(0xFFBA68C8)},
                {400, new Color(0xFFAB47BC)},
                {500, new Color(_purplePrimaryValue)},
                {600, new Color(0xFF8E24AA)},
                {700, new Color(0xFF7B1FA2)},
                {800, new Color(0xFF6A1B9A)},
                {900, new Color(0xFF4A148C)}
            }
        );

        const long _purplePrimaryValue = 0xFF9C27B0;

        public static readonly MaterialAccentColor purpleAccent = new MaterialAccentColor(
            _purpleAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFEA80FC)},
                {200, new Color(_purpleAccentPrimaryValue)},
                {400, new Color(0xFFD500F9)},
                {700, new Color(0xFFAA00FF)}
            }
        );

        const long _purpleAccentPrimaryValue = 0xFFE040FB;

        public static readonly MaterialColor deepPurple = new MaterialColor(
            _deepPurplePrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFEDE7F6)},
                {100, new Color(0xFFD1C4E9)},
                {200, new Color(0xFFB39DDB)},
                {300, new Color(0xFF9575CD)},
                {400, new Color(0xFF7E57C2)},
                {500, new Color(_deepPurplePrimaryValue)},
                {600, new Color(0xFF5E35B1)},
                {700, new Color(0xFF512DA8)},
                {800, new Color(0xFF4527A0)},
                {900, new Color(0xFF311B92)}
            }
        );

        const long _deepPurplePrimaryValue = 0xFF673AB7;

        public static readonly MaterialAccentColor deepPurpleAccent = new MaterialAccentColor(
            _deepPurpleAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFB388FF)},
                {200, new Color(_deepPurpleAccentPrimaryValue)},
                {400, new Color(0xFF651FFF)},
                {700, new Color(0xFF6200EA)}
            }
        );

        const long _deepPurpleAccentPrimaryValue = 0xFF7C4DFF;


        public static readonly MaterialColor indigo = new MaterialColor(
            _indigoPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFE8EAF6)},
                {100, new Color(0xFFC5CAE9)},
                {200, new Color(0xFF9FA8DA)},
                {300, new Color(0xFF7986CB)},
                {400, new Color(0xFF5C6BC0)},
                {500, new Color(_indigoPrimaryValue)},
                {600, new Color(0xFF3949AB)},
                {700, new Color(0xFF303F9F)},
                {800, new Color(0xFF283593)},
                {900, new Color(0xFF1A237E)}
            }
        );

        const long _indigoPrimaryValue = 0xFF3F51B5;

        public static readonly MaterialAccentColor indigoAccent = new MaterialAccentColor(
            _indigoAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFF8C9EFF)},
                {200, new Color(_indigoAccentPrimaryValue)},
                {400, new Color(0xFF3D5AFE)},
                {700, new Color(0xFF304FFE)}
            }
        );

        const long _indigoAccentPrimaryValue = 0xFF536DFE;

        public static readonly MaterialColor blue = new MaterialColor(
            _bluePrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFE3F2FD)},
                {100, new Color(0xFFBBDEFB)},
                {200, new Color(0xFF90CAF9)},
                {300, new Color(0xFF64B5F6)},
                {400, new Color(0xFF42A5F5)},
                {500, new Color(_bluePrimaryValue)},
                {600, new Color(0xFF1E88E5)},
                {700, new Color(0xFF1976D2)},
                {800, new Color(0xFF1565C0)},
                {900, new Color(0xFF0D47A1)}
            }
        );

        const long _bluePrimaryValue = 0xFF2196F3;

        public static readonly MaterialAccentColor blueAccent = new MaterialAccentColor(
            _blueAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFF82B1FF)},
                {200, new Color(_blueAccentPrimaryValue)},
                {400, new Color(0xFF2979FF)},
                {700, new Color(0xFF2962FF)}
            }
        );

        const long _blueAccentPrimaryValue = 0xFF448AFF;

        public static readonly MaterialColor lightBlue = new MaterialColor(
            _lightBluePrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFE1F5FE)},
                {100, new Color(0xFFB3E5FC)},
                {200, new Color(0xFF81D4FA)},
                {300, new Color(0xFF4FC3F7)},
                {400, new Color(0xFF29B6F6)},
                {500, new Color(_lightBluePrimaryValue)},
                {600, new Color(0xFF039BE5)},
                {700, new Color(0xFF0288D1)},
                {800, new Color(0xFF0277BD)},
                {900, new Color(0xFF01579B)},
            }
        );

        const long _lightBluePrimaryValue = 0xFF03A9F4;

        public static readonly MaterialAccentColor lightBlueAccent = new MaterialAccentColor(
            _lightBlueAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFF80D8FF)},
                {200, new Color(_lightBlueAccentPrimaryValue)},
                {400, new Color(0xFF00B0FF)},
                {700, new Color(0xFF0091EA)}
            }
        );

        const long _lightBlueAccentPrimaryValue = 0xFF40C4FF;

        public static readonly MaterialColor cyan = new MaterialColor(
            _cyanPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFE0F7FA)},
                {100, new Color(0xFFB2EBF2)},
                {200, new Color(0xFF80DEEA)},
                {300, new Color(0xFF4DD0E1)},
                {400, new Color(0xFF26C6DA)},
                {500, new Color(_cyanPrimaryValue)},
                {600, new Color(0xFF00ACC1)},
                {700, new Color(0xFF0097A7)},
                {800, new Color(0xFF00838F)},
                {900, new Color(0xFF006064)}
            }
        );

        const long _cyanPrimaryValue = 0xFF00BCD4;

        public static readonly MaterialAccentColor cyanAccent = new MaterialAccentColor(
            _cyanAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFF84FFFF)},
                {200, new Color(_cyanAccentPrimaryValue)},
                {400, new Color(0xFF00E5FF)},
                {700, new Color(0xFF00B8D4)}
            }
        );

        const long _cyanAccentPrimaryValue = 0xFF18FFFF;

        public static readonly MaterialColor teal = new MaterialColor(
            _tealPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFE0F2F1)},
                {100, new Color(0xFFB2DFDB)},
                {200, new Color(0xFF80CBC4)},
                {300, new Color(0xFF4DB6AC)},
                {400, new Color(0xFF26A69A)},
                {500, new Color(_tealPrimaryValue)},
                {600, new Color(0xFF00897B)},
                {700, new Color(0xFF00796B)},
                {800, new Color(0xFF00695C)},
                {900, new Color(0xFF004D40)}
            }
        );

        const long _tealPrimaryValue = 0xFF009688;

        public static readonly MaterialAccentColor tealAccent = new MaterialAccentColor(
            _tealAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFA7FFEB)},
                {200, new Color(_tealAccentPrimaryValue)},
                {400, new Color(0xFF1DE9B6)},
                {700, new Color(0xFF00BFA5)}
            }
        );

        const long _tealAccentPrimaryValue = 0xFF64FFDA;

        public static readonly MaterialColor green = new MaterialColor(
            _greenPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFE8F5E9)},
                {100, new Color(0xFFC8E6C9)},
                {200, new Color(0xFFA5D6A7)},
                {300, new Color(0xFF81C784)},
                {400, new Color(0xFF66BB6A)},
                {500, new Color(_greenPrimaryValue)},
                {600, new Color(0xFF43A047)},
                {700, new Color(0xFF388E3C)},
                {800, new Color(0xFF2E7D32)},
                {900, new Color(0xFF1B5E20)}
            }
        );

        const long _greenPrimaryValue = 0xFF4CAF50;

        public static readonly MaterialAccentColor greenAccent = new MaterialAccentColor(
            _greenAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFB9F6CA)},
                {200, new Color(_greenAccentPrimaryValue)},
                {400, new Color(0xFF00E676)},
                {700, new Color(0xFF00C853)}
            }
        );

        const long _greenAccentPrimaryValue = 0xFF69F0AE;

        public static readonly MaterialColor lightGreen = new MaterialColor(
            _lightGreenPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFF1F8E9)},
                {100, new Color(0xFFDCEDC8)},
                {200, new Color(0xFFC5E1A5)},
                {300, new Color(0xFFAED581)},
                {400, new Color(0xFF9CCC65)},
                {500, new Color(_lightGreenPrimaryValue)},
                {600, new Color(0xFF7CB342)},
                {700, new Color(0xFF689F38)},
                {800, new Color(0xFF558B2F)},
                {900, new Color(0xFF33691E)}
            }
        );

        const long _lightGreenPrimaryValue = 0xFF8BC34A;

        public static readonly MaterialAccentColor lightGreenAccent = new MaterialAccentColor(
            _lightGreenAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFCCFF90)},
                {200, new Color(_lightGreenAccentPrimaryValue)},
                {400, new Color(0xFF76FF03)},
                {700, new Color(0xFF64DD17)}
            }
        );

        const long _lightGreenAccentPrimaryValue = 0xFFB2FF59;

        public static readonly MaterialColor lime = new MaterialColor(
            _limePrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFF9FBE7)},
                {100, new Color(0xFFF0F4C3)},
                {200, new Color(0xFFE6EE9C)},
                {300, new Color(0xFFDCE775)},
                {400, new Color(0xFFD4E157)},
                {500, new Color(_limePrimaryValue)},
                {600, new Color(0xFFC0CA33)},
                {700, new Color(0xFFAFB42B)},
                {800, new Color(0xFF9E9D24)},
                {900, new Color(0xFF827717)}
            }
        );

        const long _limePrimaryValue = 0xFFCDDC39;

        public static readonly MaterialAccentColor limeAccent = new MaterialAccentColor(
            _limeAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFF4FF81)},
                {200, new Color(_limeAccentPrimaryValue)},
                {400, new Color(0xFFC6FF00)},
                {700, new Color(0xFFAEEA00)}
            }
        );

        const long _limeAccentPrimaryValue = 0xFFEEFF41;

        public static readonly MaterialColor yellow = new MaterialColor(
            _yellowPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFFFFDE7)},
                {100, new Color(0xFFFFF9C4)},
                {200, new Color(0xFFFFF59D)},
                {300, new Color(0xFFFFF176)},
                {400, new Color(0xFFFFEE58)},
                {500, new Color(_yellowPrimaryValue)},
                {600, new Color(0xFFFDD835)},
                {700, new Color(0xFFFBC02D)},
                {800, new Color(0xFFF9A825)},
                {900, new Color(0xFFF57F17)}
            }
        );

        const long _yellowPrimaryValue = 0xFFFFEB3B;

        public static readonly MaterialAccentColor yellowAccent = new MaterialAccentColor(
            _yellowAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFFFFF8D)},
                {200, new Color(_yellowAccentPrimaryValue)},
                {400, new Color(0xFFFFEA00)},
                {700, new Color(0xFFFFD600)}
            }
        );

        const long _yellowAccentPrimaryValue = 0xFFFFFF00;

        public static readonly MaterialColor amber = new MaterialColor(
            _amberPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFFFF8E1)},
                {100, new Color(0xFFFFECB3)},
                {200, new Color(0xFFFFE082)},
                {300, new Color(0xFFFFD54F)},
                {400, new Color(0xFFFFCA28)},
                {500, new Color(_amberPrimaryValue)},
                {600, new Color(0xFFFFB300)},
                {700, new Color(0xFFFFA000)},
                {800, new Color(0xFFFF8F00)},
                {900, new Color(0xFFFF6F00)}
            }
        );

        const long _amberPrimaryValue = 0xFFFFC107;

        public static readonly MaterialAccentColor amberAccent = new MaterialAccentColor(
            _amberAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFFFE57F)},
                {200, new Color(_amberAccentPrimaryValue)},
                {400, new Color(0xFFFFC400)},
                {700, new Color(0xFFFFAB00)}
            }
        );

        const long _amberAccentPrimaryValue = 0xFFFFD740;

        public static readonly MaterialColor orange = new MaterialColor(
            _orangePrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFFFF3E0)},
                {100, new Color(0xFFFFE0B2)},
                {200, new Color(0xFFFFCC80)},
                {300, new Color(0xFFFFB74D)},
                {400, new Color(0xFFFFA726)},
                {500, new Color(_orangePrimaryValue)},
                {600, new Color(0xFFFB8C00)},
                {700, new Color(0xFFF57C00)},
                {800, new Color(0xFFEF6C00)},
                {900, new Color(0xFFE65100)}
            }
        );

        const long _orangePrimaryValue = 0xFFFF9800;

        public static readonly MaterialAccentColor orangeAccent = new MaterialAccentColor(
            _orangeAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFFFD180)},
                {200, new Color(_orangeAccentPrimaryValue)},
                {400, new Color(0xFFFF9100)},
                {700, new Color(0xFFFF6D00)}
            }
        );

        const long _orangeAccentPrimaryValue = 0xFFFFAB40;

        public static readonly MaterialColor deepOrange = new MaterialColor(
            _deepOrangePrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFFBE9E7)},
                {100, new Color(0xFFFFCCBC)},
                {200, new Color(0xFFFFAB91)},
                {300, new Color(0xFFFF8A65)},
                {400, new Color(0xFFFF7043)},
                {500, new Color(_deepOrangePrimaryValue)},
                {600, new Color(0xFFF4511E)},
                {700, new Color(0xFFE64A19)},
                {800, new Color(0xFFD84315)},
                {900, new Color(0xFFBF360C)}
            }
        );

        const long _deepOrangePrimaryValue = 0xFFFF5722;


        public static readonly MaterialAccentColor deepOrangeAccent = new MaterialAccentColor(
            _deepOrangeAccentPrimaryValue,
            new Dictionary<int, Color> {
                {100, new Color(0xFFFF9E80)},
                {200, new Color(_deepOrangeAccentPrimaryValue)},
                {400, new Color(0xFFFF3D00)},
                {700, new Color(0xFFDD2C00)}
            }
        );

        const long _deepOrangeAccentPrimaryValue = 0xFFFF6E40;

        public static readonly MaterialColor brown = new MaterialColor(
            _brownPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFEFEBE9)},
                {100, new Color(0xFFD7CCC8)},
                {200, new Color(0xFFBCAAA4)},
                {300, new Color(0xFFA1887F)},
                {400, new Color(0xFF8D6E63)},
                {500, new Color(_brownPrimaryValue)},
                {600, new Color(0xFF6D4C41)},
                {700, new Color(0xFF5D4037)},
                {800, new Color(0xFF4E342E)},
                {900, new Color(0xFF3E2723)}
            }
        );

        const long _brownPrimaryValue = 0xFF795548;

        public static readonly MaterialColor grey = new MaterialColor(
            _greyPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFFAFAFA)},
                {100, new Color(0xFFF5F5F5)},
                {200, new Color(0xFFEEEEEE)},
                {300, new Color(0xFFE0E0E0)},
                {350, new Color(0xFFD6D6D6)},
                {400, new Color(0xFFBDBDBD)},
                {500, new Color(_greyPrimaryValue)},
                {600, new Color(0xFF757575)},
                {700, new Color(0xFF616161)},
                {800, new Color(0xFF424242)},
                {850, new Color(0xFF303030)},
                {900, new Color(0xFF212121)}
            }
        );

        const long _greyPrimaryValue = 0xFF9E9E9E;

        public static readonly MaterialColor blueGrey = new MaterialColor(
            _blueGreyPrimaryValue,
            new Dictionary<int, Color> {
                {50, new Color(0xFFECEFF1)},
                {100, new Color(0xFFCFD8DC)},
                {200, new Color(0xFFB0BEC5)},
                {300, new Color(0xFF90A4AE)},
                {400, new Color(0xFF78909C)},
                {500, new Color(_blueGreyPrimaryValue)},
                {600, new Color(0xFF546E7A)},
                {700, new Color(0xFF455A64)},
                {800, new Color(0xFF37474F)},
                {900, new Color(0xFF263238)}
            }
        );

        const long _blueGreyPrimaryValue = 0xFF607D8B;


        public static readonly List<MaterialColor> primaries = new List<MaterialColor> {
            red,
            pink,
            purple,
            deepPurple,
            indigo,
            blue,
            lightBlue,
            cyan,
            teal,
            green,
            lightGreen,
            lime,
            yellow,
            amber,
            orange,
            deepOrange,
            brown,
            blueGrey
        };

        public static readonly List<MaterialAccentColor> accents = new List<MaterialAccentColor> {
            redAccent,
            pinkAccent,
            purpleAccent,
            deepPurpleAccent,
            indigoAccent,
            blueAccent,
            lightBlueAccent,
            cyanAccent,
            tealAccent,
            greenAccent,
            lightGreenAccent,
            limeAccent,
            yellowAccent,
            amberAccent,
            orangeAccent,
            deepOrangeAccent
        };
    }
}