using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.gallery {
    public delegate IPromise<string> UpdateUrlFetcher();

    public class GalleryApp : StatefulWidget {
        public GalleryApp(
            Key key = null,
            UpdateUrlFetcher updateUrlFetcher = null,
            bool enablePerformanceOverlay = true,
            bool enableRasterCacheImagesCheckerboard = true,
            bool enableOffscreenLayersCheckerboard = true,
            VoidCallback onSendFeedback = null,
            bool testMode = false
        ) : base(key: key) {
        }

        public readonly UpdateUrlFetcher updateUrlFetcher;

        public readonly bool enablePerformanceOverlay;

        public readonly bool enableRasterCacheImagesCheckerboard;

        public readonly bool enableOffscreenLayersCheckerboard;

        public readonly VoidCallback onSendFeedback;

        public readonly bool testMode;

        public override State createState() {
            return new _GalleryAppState();
        }
    }

    class _GalleryAppState : State<GalleryApp> {
        GalleryOptions _options;
        Timer _timeDilationTimer;

        Dictionary<string, WidgetBuilder> _buildRoutes() {
            return GalleryDemo.kAllGalleryDemos.ToDictionary(
                (demo) => $"{demo.routeName}",
                (demo) => demo.buildRoute);
        }

        public override void initState() {
            base.initState();
            this._options = new GalleryOptions(
                theme: GalleryTheme.kLightGalleryTheme,
                textScaleFactor: GalleryTextScaleValue.kAllGalleryTextScaleValues[0],
                timeDilation: SchedulerBinding.instance.timeDilation,
                platform: Application.platform
            );
        }

        public override void dispose() {
            this._timeDilationTimer?.cancel();
            this._timeDilationTimer = null;
            base.dispose();
        }

        void _handleOptionsChanged(GalleryOptions newOptions) {
            this.setState(() => {
                if (this._options.timeDilation != newOptions.timeDilation) {
                    this._timeDilationTimer?.cancel();
                    this._timeDilationTimer = null;
                    if (newOptions.timeDilation > 1.0) {
                        // We delay the time dilation change long enough that the user can see
                        // that UI has started reacting and then we slam on the brakes so that
                        // they see that the time is in fact now dilated.
                        this._timeDilationTimer = Window.instance.run(new TimeSpan(0, 0, 0, 0, 150),
                            () => { SchedulerBinding.instance.timeDilation = newOptions.timeDilation; });
                    } else {
                        SchedulerBinding.instance.timeDilation = newOptions.timeDilation;
                    }
                }

                this._options = newOptions;
            });
        }

        Widget _applyTextScaleFactor(Widget child) {
            return new Builder(
                builder: context => {
                    return new MediaQuery(
                        data: MediaQuery.of(context).copyWith(
                            textScaleFactor: this._options.textScaleFactor.scale
                        ),
                        child: child
                    );
                }
            );
        }

        public override Widget build(BuildContext context) {
//            Widget home = new GalleryHome(
//                    testMode: this.widget.testMode,
//                    optionsPage: new GalleryOptionsPage(
//                        options: this._options,
//                        onOptionsChanged: _handleOptionsChanged,
//                        onSendFeedback: this.widget.onSendFeedback ?? () => { Application.OpenURL("https://github.com/UnityTech/UIWidgets/issues"); }
//                    )
//                );

            Widget home = null;
            if (this.widget.updateUrlFetcher != null) {
                home = new Updater(
                    updateUrlFetcher: this.widget.updateUrlFetcher,
                    child: home
                );
            }

            return new MaterialApp(
                theme: this._options.theme.data.copyWith(/*platform: this._options.platform*/),
                title: "UIWidgets Gallery",
                color: Colors.grey,
                showPerformanceOverlay: this._options.showPerformanceOverlay,
                //checkerboardOffscreenLayers: this._options.showOffscreenLayersCheckerboard,
                //checkerboardRasterCacheImages: this._options.showRasterCacheImagesCheckerboard,
                routes: this._buildRoutes(),
                builder: (BuildContext _, Widget child) => this._applyTextScaleFactor(child),
                home: home
            );
        }
    }
}
