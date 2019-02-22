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
                        new BoxShadow(offset: new Offset(0.0f, 2.0f), blurRadius: 1.0f, spreadRadius: -1.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 1.0f), blurRadius: 1.0f, spreadRadius: 0.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 1.0f), blurRadius: 3.0f, spreadRadius: 0.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    2, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 3.0f), blurRadius: 1.0f, spreadRadius: -2.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 2.0f), blurRadius: 2.0f, spreadRadius: 0.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 1.0f), blurRadius: 5.0f, spreadRadius: 0.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    3, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 3.0f), blurRadius: 3.0f, spreadRadius: -2.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 3.0f), blurRadius: 4.0f, spreadRadius: 0.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 1.0f), blurRadius: 8.0f, spreadRadius: 0.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    4, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 2.0f), blurRadius: 4.0f, spreadRadius: -1.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 4.0f), blurRadius: 5.0f, spreadRadius: 0.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 1.0f), blurRadius: 10.0f, spreadRadius: 0.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    6, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 3.0f), blurRadius: 5.0f, spreadRadius: -1.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 6.0f), blurRadius: 10.0f, spreadRadius: 0.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 1.0f), blurRadius: 18.0f, spreadRadius: 0.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    8, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 5.0f), blurRadius: 5.0f, spreadRadius: -3.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 8.0f), blurRadius: 10.0f, spreadRadius: 1.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 3.0f), blurRadius: 14.0f, spreadRadius: 2.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    9, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 5.0f), blurRadius: 6.0f, spreadRadius: -3.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 9.0f), blurRadius: 12.0f, spreadRadius: 1.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 3.0f), blurRadius: 16.0f, spreadRadius: 2.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    12, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 7.0f), blurRadius: 8.0f, spreadRadius: -4.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 12.0f), blurRadius: 17.0f, spreadRadius: 2.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 5.0f), blurRadius: 22.0f, spreadRadius: 4.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    16, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 8.0f), blurRadius: 10.0f, spreadRadius: -5.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 16.0f), blurRadius: 24.0f, spreadRadius: 2.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 6.0f), blurRadius: 30.0f, spreadRadius: 5.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }, {
                    24, new List<BoxShadow> {
                        new BoxShadow(offset: new Offset(0.0f, 11.0f), blurRadius: 15.0f, spreadRadius: -7.0f,
                            color: _kKeyUmbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 24.0f), blurRadius: 38.0f, spreadRadius: 3.0f,
                            color: _kKeyPenumbraOpacity),
                        new BoxShadow(offset: new Offset(0.0f, 9.0f), blurRadius: 46.0f, spreadRadius: 8.0f,
                            color: _kAmbientShadowOpacity)
                    }
                }
            };
    }
}