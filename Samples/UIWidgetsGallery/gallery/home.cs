using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.animation;
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
    public static class HomeUtils {
        internal static readonly Color _kUIWidgetsBlue = new Color(0xFF003D75);
        internal const float _kDemoItemHeight = 64.0f;
        internal static TimeSpan _kFrontLayerSwitchDuration = new TimeSpan(0, 0, 0, 0, 300);
    }

    class _UIWidgetsLogo : StatelessWidget {
        public _UIWidgetsLogo(Key key = null, bool isDark = false) : base(key: key) {
            this._isDark = isDark;
        }

        readonly bool _isDark;

        public override Widget build(BuildContext context) {
            return new Center(
                child: new Container(
                    width: 34.0f,
                    height: 34.0f,
                    decoration: new BoxDecoration(
                        image: new DecorationImage(
                            image: new AssetImage(
                                this._isDark ? "unity-black" : "unity-white")
                        )
                    )
                )
            );
        }
    }

    class _CategoryItem : StatelessWidget {
        public _CategoryItem(
            Key key = null,
            GalleryDemoCategory category = null,
            VoidCallback onTap = null
        ) : base(key: key) {
            this.category = category;
            this.onTap = onTap;
        }

        public readonly GalleryDemoCategory category;

        public readonly VoidCallback onTap;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;

            return new RepaintBoundary(
                child: new RawMaterialButton(
                    padding: EdgeInsets.zero,
                    splashColor: theme.primaryColor.withOpacity(0.12f),
                    highlightColor: Colors.transparent,
                    onPressed: this.onTap,
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.end,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget> {
                            new Padding(
                                padding: EdgeInsets.all(6.0f),
                                child: new Icon(
                                    this.category.icon,
                                    size: 60.0f,
                                    color: isDark ? Colors.white : HomeUtils._kUIWidgetsBlue
                                )
                            ),
                            new SizedBox(height: 10.0f),
                            new Container(
                                height: 48.0f,
                                alignment: Alignment.center,
                                child: new Text(
                                    this.category.name,
                                    textAlign: TextAlign.center,
                                    style: theme.textTheme.subhead.copyWith(
                                        fontFamily: "GoogleSans",
                                        color: isDark ? Colors.white : HomeUtils._kUIWidgetsBlue
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }
    }

    class _CategoriesPage : StatelessWidget {
        public _CategoriesPage(
            Key key = null,
            IEnumerable<GalleryDemoCategory> categories = null,
            ValueChanged<GalleryDemoCategory> onCategoryTap = null
        ) : base(key: key) {
            this.categories = categories;
            this.onCategoryTap = onCategoryTap;
        }

        public readonly IEnumerable<GalleryDemoCategory> categories;

        public readonly ValueChanged<GalleryDemoCategory> onCategoryTap;

        public override Widget build(BuildContext context) {
            float aspectRatio = 160.0f / 180.0f;
            List<GalleryDemoCategory> categoriesList = this.categories.ToList();
            int columnCount = (MediaQuery.of(context).orientation == Orientation.portrait) ? 2 : 3;

            return new SingleChildScrollView(
                key: new PageStorageKey<string>("categories"),
                child: new LayoutBuilder(
                    builder: (_, constraints) => {
                        float columnWidth = constraints.biggest.width / columnCount;
                        float rowHeight = Mathf.Min(225.0f, columnWidth * aspectRatio);
                        int rowCount = (categoriesList.Count + columnCount - 1) / columnCount;

                        return new RepaintBoundary(
                            child: new Column(
                                mainAxisSize: MainAxisSize.min,
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children: Enumerable.Range(0, rowCount).Select(rowIndex => {
                                    int columnCountForRow = rowIndex == rowCount - 1
                                        ? categoriesList.Count - columnCount * Mathf.Max(0, rowCount - 1)
                                        : columnCount;

                                    return (Widget) new Row(
                                        children: Enumerable.Range(0, columnCountForRow).Select(columnIndex => {
                                            int index = rowIndex * columnCount + columnIndex;
                                            GalleryDemoCategory category = categoriesList[index];

                                            return (Widget) new SizedBox(
                                                width: columnWidth,
                                                height: rowHeight,
                                                child: new _CategoryItem(
                                                    category: category,
                                                    onTap: () => { this.onCategoryTap(category); }
                                                )
                                            );
                                        }).ToList()
                                    );
                                }).ToList()
                            )
                        );
                    }
                )
            );
        }
    }

    class _DemoItem : StatelessWidget {
        public _DemoItem(Key key = null, GalleryDemo demo = null) : base(key: key) {
            this.demo = demo;
        }

        public readonly GalleryDemo demo;

        void _launchDemo(BuildContext context) {
            if (this.demo.routeName != null) {
                Navigator.pushNamed(context, this.demo.routeName);
            }
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;
            float textScaleFactor = MediaQuery.textScaleFactorOf(context);

            List<Widget> titleChildren = new List<Widget> {
                new Text(
                    this.demo.title,
                    style: theme.textTheme.subhead.copyWith(
                        color: isDark ? Colors.white : new Color(0xFF202124)
                    )
                ),
            };

            if (this.demo.subtitle != null) {
                titleChildren.Add(
                    new Text(
                        this.demo.subtitle,
                        style: theme.textTheme.body1.copyWith(
                            color: isDark ? Colors.white : new Color(0xFF60646B)
                        )
                    )
                );
            }

            return new RawMaterialButton(
                padding: EdgeInsets.zero,
                splashColor: theme.primaryColor.withOpacity(0.12f),
                highlightColor: Colors.transparent,
                onPressed: () => { this._launchDemo(context); },
                child: new Container(
                    constraints: new BoxConstraints(minHeight: HomeUtils._kDemoItemHeight * textScaleFactor),
                    child: new Row(
                        children: new List<Widget> {
                            new Container(
                                width: 56.0f,
                                height: 56.0f,
                                alignment: Alignment.center,
                                child: new Icon(
                                    this.demo.icon,
                                    size: 24.0f,
                                    color: isDark ? Colors.white : HomeUtils._kUIWidgetsBlue
                                )
                            ),
                            new Expanded(
                                child: new Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    crossAxisAlignment: CrossAxisAlignment.stretch,
                                    children: titleChildren
                                )
                            ),
                            new SizedBox(width: 44.0f),
                        }
                    )
                )
            );
        }
    }

    class _DemosPage : StatelessWidget {
        public _DemosPage(GalleryDemoCategory category) {
            this.category = category;
        }

        public readonly GalleryDemoCategory category;

        public override Widget build(BuildContext context) {
            float windowBottomPadding = MediaQuery.of(context).padding.bottom;
            return new KeyedSubtree(
                key: new ValueKey<string>("GalleryDemoList"),
                child: new ListView(
                    key: new PageStorageKey<string>(this.category.name),
                    padding: EdgeInsets.only(top: 8.0f, bottom: windowBottomPadding),
                    children: DemoUtils.kGalleryCategoryToDemos[this.category]
                        .Select(demo => (Widget) new _DemoItem(demo: demo)).ToList()
                )
            );
        }
    }

    public class GalleryHome : StatefulWidget {
        public GalleryHome(
            Key key = null,
            bool testMode = false,
            Widget optionsPage = null,
            GalleryOptions options = null
        ) : base(key: key) {
            this.testMode = testMode;
            this.optionsPage = optionsPage;
            this.options = options;
        }

        public readonly Widget optionsPage;
        public readonly bool testMode;
        public readonly GalleryOptions options;

        public static bool showPreviewBanner = true;

        public override State createState() {
            return new _GalleryHomeState();
        }
    }

    class _GalleryHomeState : SingleTickerProviderStateMixin<GalleryHome> {
        readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();
        AnimationController _controller;
        GalleryDemoCategory _category;

        static Widget _topHomeLayout(Widget currentChild, List<Widget> previousChildren) {
            List<Widget> children = previousChildren;
            if (currentChild != null) {
                children = children.ToList();
                children.Add(currentChild);
            }

            return new Stack(
                children: children,
                alignment: Alignment.topCenter
            );
        }

        public static AnimatedSwitcherLayoutBuilder _centerHomeLayout = AnimatedSwitcher.defaultLayoutBuilder;

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 600),
                debugLabel: "preview banner",
                vsync: this
            );
            this._controller.forward();
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;
            MediaQueryData media = MediaQuery.of(context);
            bool centerHome = media.orientation == Orientation.portrait && media.size.height < 800.0;

            Curve switchOutCurve = new Interval(0.4f, 1.0f, curve: Curves.fastOutSlowIn);
            Curve switchInCurve = new Interval(0.4f, 1.0f, curve: Curves.fastOutSlowIn);

            Widget home = new Scaffold(
                key: this._scaffoldKey,
                backgroundColor: isDark ? HomeUtils._kUIWidgetsBlue : theme.primaryColor,
                body: new SafeArea(
                    bottom: false,
                    child: new WillPopScope(
                        onWillPop: () => {
                            if (this._category != null) {
                                this.setState(() => this._category = null);
                                return Promise<bool>.Resolved(false);
                            }

                            return Promise<bool>.Resolved(true);
                        },
                        child: new Backdrop(
                            backTitle: new Text("Options"),
                            backLayer: this.widget.optionsPage,
                            frontAction: new AnimatedSwitcher(
                                duration: HomeUtils._kFrontLayerSwitchDuration,
                                switchOutCurve: switchOutCurve,
                                switchInCurve: switchInCurve,
                                child: this._category == null
                                    ? (Widget) new _UIWidgetsLogo(isDark: this.widget.options?.theme == GalleryTheme.kDarkGalleryTheme)
                                    : new IconButton(
                                        icon: new BackButtonIcon(),
                                        tooltip: "Back",
                                        onPressed: () => this.setState(() => this._category = null)
                                    )
                            ),
                            frontTitle: new AnimatedSwitcher(
                                duration: HomeUtils._kFrontLayerSwitchDuration,
                                child: this._category == null
                                    ? new Text("UIWidgets gallery")
                                    : new Text(this._category.name)
                            ),
                            frontHeading: this.widget.testMode ? null : new Container(height: 24.0f),
                            frontLayer: new AnimatedSwitcher(
                                duration: HomeUtils._kFrontLayerSwitchDuration,
                                switchOutCurve: switchOutCurve,
                                switchInCurve: switchInCurve,
                                layoutBuilder: centerHome ? _centerHomeLayout : _topHomeLayout,
                                child: this._category != null
                                    ? (Widget) new _DemosPage(this._category)
                                    : new _CategoriesPage(
                                        categories: DemoUtils.kAllGalleryDemoCategories,
                                        onCategoryTap: (GalleryDemoCategory category) => {
                                            this.setState(() => this._category = category);
                                        }
                                    )
                            )
                        )
                    )
                )
            );

            D.assert(() => {
                GalleryHome.showPreviewBanner = false;
                return true;
            });

            if (GalleryHome.showPreviewBanner) {
                home = new Stack(
                    fit: StackFit.expand,
                    children: new List<Widget> {
                        home,
                        new FadeTransition(
                            opacity: new CurvedAnimation(parent: this._controller, curve: Curves.easeInOut),
                            child: new Banner(
                                message: "PREVIEW",
                                location: BannerLocation.topEnd
                            )
                        )
                    }
                );
            }

            home = new AnnotatedRegion<SystemUiOverlayStyle>(
                child: home,
                value: SystemUiOverlayStyle.light
            );

            return home;
        }
    }
}