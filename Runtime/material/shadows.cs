using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public class ShadowConstants {
        static readonly Color _kKeyUmbraOpacity = new Color(0x33000000); // alpha = 0.2
        static readonly Color _kKeyPenumbraOpacity = new Color(0x24000000); // alpha = 0.14
        static readonly Color _kAmbientShadowOpacity = new Color(0x1F000000); // alpha = 0.12

        public static readonly Dictionary<int, List<BoxShadow>> kElevationToShadow =
            new Dictionary<int, List<BoxShadow>> {
                {
                    1, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 2.0), blurRadius: 1.0, spreadRadius: -1.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 1.0), blurRadius: 1.0, spreadRadius: 0.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 1.0), blurRadius: 3.0, spreadRadius: 0.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    2, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 3.0), blurRadius: 1.0, spreadRadius: -2.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 2.0), blurRadius: 2.0, spreadRadius: 0.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 1.0), blurRadius: 5.0, spreadRadius: 0.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    3, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 3.0), blurRadius: 3.0, spreadRadius: -2.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 3.0), blurRadius: 4.0, spreadRadius: 0.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 1.0), blurRadius: 8.0, spreadRadius: 0.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    4, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 2.0), blurRadius: 4.0, spreadRadius: -1.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 4.0), blurRadius: 5.0, spreadRadius: 0.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 1.0), blurRadius: 10.0, spreadRadius: 0.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    6, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 3.0), blurRadius: 5.0, spreadRadius: -1.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 6.0), blurRadius: 10.0, spreadRadius: 0.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 1.0), blurRadius: 18.0, spreadRadius: 0.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    8, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 5.0), blurRadius: 5.0, spreadRadius: -3.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 8.0), blurRadius: 10.0, spreadRadius: 1.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 3.0), blurRadius: 14.0, spreadRadius: 2.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    9, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 5.0), blurRadius: 6.0, spreadRadius: -3.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 9.0), blurRadius: 12.0, spreadRadius: 1.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 3.0), blurRadius: 16.0, spreadRadius: 2.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    12, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 7.0), blurRadius: 8.0, spreadRadius: -4.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 12.0), blurRadius: 17.0, spreadRadius: 2.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 5.0), blurRadius: 22.0, spreadRadius: 4.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    16, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 8.0), blurRadius: 10.0, spreadRadius: -5.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 16.0), blurRadius: 24.0, spreadRadius: 2.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 6.0), blurRadius: 30.0, spreadRadius: 5.0,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    24, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0, 11.0), blurRadius: 15.0, spreadRadius: -7.0,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 24.0), blurRadius: 38.0, spreadRadius: 3.0,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0, 9.0), blurRadius: 46.0, spreadRadius: 8.0,
                            color: _kAmbientShadowOpacity)
                    }
                }
            };
    }
}