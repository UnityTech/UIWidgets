using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsGallery.gallery {
    public class GalleryOptions : IEquatable<GalleryOptions> {
        public GalleryOptions(
            GalleryTheme theme = null,
            GalleryTextScaleValue textScaleFactor = null,
            float timeDilation = 1.0f,
            RuntimePlatform? platform = null,
            bool showOffscreenLayersCheckerboard = false,
            bool showRasterCacheImagesCheckerboard = false,
            bool showPerformanceOverlay = false
        ) {
            D.assert(theme != null);
            D.assert(textScaleFactor != null);

            this.theme = theme;
            this.textScaleFactor = textScaleFactor;
            this.timeDilation = timeDilation;
            this.platform = platform ?? Application.platform;
            this.showOffscreenLayersCheckerboard = showOffscreenLayersCheckerboard;
            this.showRasterCacheImagesCheckerboard = showRasterCacheImagesCheckerboard;
            this.showPerformanceOverlay = showPerformanceOverlay;
        }

        public readonly GalleryTheme theme;
        public readonly GalleryTextScaleValue textScaleFactor;
        public readonly float timeDilation;
        public readonly RuntimePlatform platform;
        public readonly bool showPerformanceOverlay;
        public readonly bool showRasterCacheImagesCheckerboard;
        public readonly bool showOffscreenLayersCheckerboard;

        public GalleryOptions copyWith(
            GalleryTheme theme = null,
            GalleryTextScaleValue textScaleFactor = null,
            float? timeDilation = null,
            RuntimePlatform? platform = null,
            bool? showPerformanceOverlay = null,
            bool? showRasterCacheImagesCheckerboard = null,
            bool? showOffscreenLayersCheckerboard = null
        ) {
            return new GalleryOptions(
                theme: theme ?? this.theme,
                textScaleFactor: textScaleFactor ?? this.textScaleFactor,
                timeDilation: timeDilation ?? this.timeDilation,
                platform: platform ?? this.platform,
                showPerformanceOverlay: showPerformanceOverlay ?? this.showPerformanceOverlay,
                showOffscreenLayersCheckerboard:
                showOffscreenLayersCheckerboard ?? this.showOffscreenLayersCheckerboard,
                showRasterCacheImagesCheckerboard: showRasterCacheImagesCheckerboard ??
                                                   this.showRasterCacheImagesCheckerboard
            );
        }

        public bool Equals(GalleryOptions other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return Equals(this.theme, other.theme) && Equals(this.textScaleFactor, other.textScaleFactor) &&
                   this.timeDilation.Equals(other.timeDilation) && this.platform == other.platform &&
                   this.showPerformanceOverlay == other.showPerformanceOverlay &&
                   this.showRasterCacheImagesCheckerboard == other.showRasterCacheImagesCheckerboard &&
                   this.showOffscreenLayersCheckerboard == other.showOffscreenLayersCheckerboard;
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
            return this.Equals((GalleryOptions) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.theme != null ? this.theme.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.textScaleFactor != null ? this.textScaleFactor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.timeDilation.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) this.platform;
                hashCode = (hashCode * 397) ^ this.showPerformanceOverlay.GetHashCode();
                hashCode = (hashCode * 397) ^ this.showRasterCacheImagesCheckerboard.GetHashCode();
                hashCode = (hashCode * 397) ^ this.showOffscreenLayersCheckerboard.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GalleryOptions left, GalleryOptions right) {
            return Equals(left, right);
        }

        public static bool operator !=(GalleryOptions left, GalleryOptions right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.theme})";
        }
    }

    class _OptionsItem : StatelessWidget {
        const float _kItemHeight = 48.0f;
        static readonly EdgeInsets _kItemPadding = EdgeInsets.only(left: 56.0f);

        public _OptionsItem(Key key = null, Widget child = null) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            float textScaleFactor = MediaQuery.textScaleFactorOf(context);

            return new Container(
                constraints: new BoxConstraints(minHeight: _kItemHeight * textScaleFactor),
                padding: _kItemPadding,
                alignment: Alignment.centerLeft,
                child: new DefaultTextStyle(
                    style: DefaultTextStyle.of(context).style,
                    maxLines: 2,
                    overflow: TextOverflow.fade,
                    child: new IconTheme(
                        data: Theme.of(context).primaryIconTheme,
                        child: this.child
                    )
                )
            );
        }
    }

    class _BooleanItem : StatelessWidget {
        public _BooleanItem(string title, bool value, ValueChanged<bool?> onChanged, Key switchKey = null) {
            this.title = title;
            this.value = value;
            this.onChanged = onChanged;
            this.switchKey = switchKey;
        }

        public readonly string title;
        public readonly bool value;
        public readonly ValueChanged<bool?> onChanged;
        public readonly Key switchKey;

        public override Widget build(BuildContext context) {
            bool isDark = Theme.of(context).brightness == Brightness.dark;
            return new _OptionsItem(
                child: new Row(
                    children: new List<Widget> {
                        new Expanded(child: new Text(this.title)),
                        new Switch(
                            key: this.switchKey,
                            value: this.value,
                            onChanged: this.onChanged,
                            activeColor: new Color(0xFF39CEFD),
                            activeTrackColor: isDark ? Colors.white30 : Colors.black26
                        )
                    }
                )
            );
        }
    }

    class _ActionItem : StatelessWidget {
        public _ActionItem(string text, VoidCallback onTap) {
            this.text = text;
            this.onTap = onTap;
        }

        public readonly string text;
        public readonly VoidCallback onTap;

        public override Widget build(BuildContext context) {
            return new _OptionsItem(
                child: new _FlatButton(
                    onPressed: this.onTap,
                    child: new Text(this.text)
                )
            );
        }
    }

    class _FlatButton : StatelessWidget {
        public _FlatButton(Key key = null, VoidCallback onPressed = null, Widget child = null) : base(key: key) {
            this.onPressed = onPressed;
            this.child = child;
        }

        public readonly VoidCallback onPressed;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new FlatButton(
                padding: EdgeInsets.zero,
                onPressed: this.onPressed,
                child: new DefaultTextStyle(
                    style: Theme.of(context).primaryTextTheme.subhead,
                    child: this.child
                )
            );
        }
    }

    class _Heading : StatelessWidget {
        public _Heading(string text) {
            this.text = text;
        }

        public readonly string text;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            return new _OptionsItem(
                child: new DefaultTextStyle(
                    style: theme.textTheme.body1.copyWith(
                        fontFamily: "GoogleSans",
                        color: theme.accentColor
                    ),
                    child: new Text(this.text)
                )
            );
        }
    }

    class _ThemeItem : StatelessWidget {
        public _ThemeItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged) {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        public override Widget build(BuildContext context) {
            return new _BooleanItem(
                "Dark Theme",
                this.options.theme == GalleryTheme.kDarkGalleryTheme,
                (bool? value) => {
                    this.onOptionsChanged(
                        this.options.copyWith(
                            theme: value == true ? GalleryTheme.kDarkGalleryTheme : GalleryTheme.kLightGalleryTheme
                        )
                    );
                },
                switchKey: Key.key("dark_theme")
            );
        }
    }

    class _TextScaleFactorItem : StatelessWidget {
        public _TextScaleFactorItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged) {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        public override Widget build(BuildContext context) {
            return new _OptionsItem(
                child: new Row(
                    children: new List<Widget> {
                        new Expanded(
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget> {
                                    new Text("Text size"),
                                    new Text(
                                        this.options.textScaleFactor.label,
                                        style: Theme.of(context).primaryTextTheme.body1
                                    ),
                                }
                            )
                        ),
                        new PopupMenuButton<GalleryTextScaleValue>(
                            padding: EdgeInsets.only(right: 16.0f),
                            icon: new Icon(Icons.arrow_drop_down),
                            itemBuilder: _ => {
                                return GalleryTextScaleValue.kAllGalleryTextScaleValues.Select(scaleValue =>
                                    (PopupMenuEntry<GalleryTextScaleValue>) new PopupMenuItem<GalleryTextScaleValue>(
                                        value: scaleValue,
                                        child: new Text(scaleValue.label)
                                    )).ToList();
                            },
                            onSelected: scaleValue => {
                                this.onOptionsChanged(
                                    this.options.copyWith(textScaleFactor: scaleValue)
                                );
                            }
                        ),
                    }
                )
            );
        }
    }


    class _TimeDilationItem : StatelessWidget {
        public _TimeDilationItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged) {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        public override Widget build(BuildContext context) {
            return new _BooleanItem(
                "Slow motion",
                this.options.timeDilation != 1.0f,
                (bool? value) => {
                    this.onOptionsChanged(
                        this.options.copyWith(
                            timeDilation: value == true ? 20.0f : 1.0f
                        )
                    );
                },
                switchKey: Key.key("slow_motion")
            );
        }
    }

    class _PlatformItem : StatelessWidget {
        public _PlatformItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged) {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        string _platformLabel(RuntimePlatform platform) {
            return platform.ToString();
        }

        public override Widget build(BuildContext context) {
            return new _OptionsItem(
                child: new Row(
                    children: new List<Widget> {
                        new Expanded(
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget> {
                                    new Text("Platform mechanics"),
                                    new Text(
                                        this._platformLabel(this.options.platform),
                                        style: Theme.of(context).primaryTextTheme.body1
                                    ),
                                }
                            )
                        ),
                        new PopupMenuButton<RuntimePlatform>(
                            padding: EdgeInsets.only(right: 16.0f),
                            icon: new Icon(Icons.arrow_drop_down),
                            itemBuilder: _ => {
                                var values = Enum.GetValues(typeof(RuntimePlatform)).Cast<RuntimePlatform>();
                                return values.Select(platform =>
                                    (PopupMenuEntry<RuntimePlatform>) new PopupMenuItem<RuntimePlatform>(
                                        value: platform,
                                        child: new Text(this._platformLabel(platform))
                                    )).ToList();
                            },
                            onSelected: platform => {
                                this.onOptionsChanged(
                                    this.options.copyWith(platform: platform)
                                );
                            }
                        ),
                    }
                )
            );
        }
    }

    public class GalleryOptionsPage : StatelessWidget {
        public GalleryOptionsPage(
            Key key = null,
            GalleryOptions options = null,
            ValueChanged<GalleryOptions> onOptionsChanged = null,
            VoidCallback onSendFeedback = null
        ) : base(key: key) {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
            this.onSendFeedback = onSendFeedback;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;
        public readonly VoidCallback onSendFeedback;

        List<Widget> _enabledDiagnosticItems() {
            List<Widget> items = new List<Widget> {
                new Divider(),
                new _Heading("Diagnostics"),
            };

            items.Add(
                new _BooleanItem(
                    "Highlight offscreen layers",
                    this.options.showOffscreenLayersCheckerboard,
                    (bool? value) => {
                        this.onOptionsChanged(this.options.copyWith(showOffscreenLayersCheckerboard: value));
                    }
                )
            );
            items.Add(
                new _BooleanItem(
                    "Highlight raster cache images",
                    this.options.showRasterCacheImagesCheckerboard,
                    (bool? value) => {
                        this.onOptionsChanged(this.options.copyWith(showRasterCacheImagesCheckerboard: value));
                    }
                )
            );
            items.Add(
                new _BooleanItem(
                    "Show performance overlay",
                    this.options.showPerformanceOverlay,
                    (bool? value) => { this.onOptionsChanged(this.options.copyWith(showPerformanceOverlay: value)); }
                )
            );

            return items;
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);

            var children = new List<Widget> {
                new _Heading("Display"),
                new _ThemeItem(this.options, this.onOptionsChanged),
                new _TextScaleFactorItem(this.options, this.onOptionsChanged),
                new _TimeDilationItem(this.options, this.onOptionsChanged),
                new Divider(),
                new _Heading("Platform mechanics"),
                new _PlatformItem(this.options, this.onOptionsChanged)
            };

            children.AddRange(this._enabledDiagnosticItems());
            children.AddRange(new List<Widget> {
                new Divider(),
                new _Heading("UIWidgets Gallery"),
                new _ActionItem("About UIWidgets Gallery", () => {
                    /* showGalleryAboutDialog(context); */
                }),
                new _ActionItem("Send feedback", this.onSendFeedback),
            });

            return new DefaultTextStyle(
                style: theme.primaryTextTheme.subhead,
                child: new ListView(
                    padding: EdgeInsets.only(bottom: 124.0f),
                    children: children
                ));
        }
    }
}
