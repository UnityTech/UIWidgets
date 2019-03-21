using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Image = Unity.UIWidgets.widgets.Image;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsGallery.gallery {
    class BackdropDemoConstants {
        public static readonly List<Category> allCategories = new List<Category> {
            new Category(
                title: "Accessories",
                assets: new List<string> {
                    "products/belt",
                    "products/earrings",
                    "products/backpack",
                    "products/hat",
                    "products/scarf",
                    "products/sunnies"
                }
            ),
            new Category(
                title: "Blue",
                assets: new List<string> {
                    "products/backpack",
                    "products/cup",
                    "products/napkins",
                    "products/top"
                }
            ),
            new Category(
                title: "Cold Weather",
                assets: new List<string> {
                    "products/jacket",
                    "products/jumper",
                    "products/scarf",
                    "products/sweater",
                    "products/sweats"
                }
            ),
            new Category(
                title: "Home",
                assets: new List<string> {
                    "products/cup",
                    "products/napkins",
                    "products/planters",
                    "products/table",
                    "products/teaset"
                }
            ),
            new Category(
                title: "Tops",
                assets: new List<string> {
                    "products/jumper",
                    "products/shirt",
                    "products/sweater",
                    "products/top"
                }
            ),
            new Category(
                title: "Everything",
                assets: new List<string> {
                    "products/backpack",
                    "products/belt",
                    "products/cup",
                    "products/dress",
                    "products/earrings",
                    "products/flatwear",
                    "products/hat",
                    "products/jacket",
                    "products/jumper",
                    "products/napkins",
                    "products/planters",
                    "products/scarf",
                    "products/shirt",
                    "products/sunnies",
                    "products/sweater",
                    "products/sweats",
                    "products/table",
                    "products/teaset",
                    "products/top"
                }
            ),
        };
    }

    public class Category {
        public Category(string title = null, List<string> assets = null) {
            this.title = title;
            this.assets = assets;
        }

        public readonly string title;
        public readonly List<string> assets;

        public override string ToString() {
            return $"{this.GetType()}('{this.title}')";
        }
    }


    public class CategoryView : StatelessWidget {
        public CategoryView(Key key = null, Category category = null) : base(key: key) {
            this.category = category;
        }

        public readonly Category category;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            return new ListView(
                key: new PageStorageKey<Category>(this.category),
                padding: EdgeInsets.symmetric(
                    vertical: 16.0f,
                    horizontal: 64.0f
                ),
                children: this.category.assets.Select<string, Widget>((string asset) => {
                    return new Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: new List<Widget> {
                            new Card(
                                child: new Container(
                                    width: 144.0f,
                                    alignment: Alignment.center,
                                    child: new Column(
                                        children: new List<Widget> {
                                            Image.asset(
                                                asset,
                                                fit: BoxFit.contain
                                            ),
                                            new Container(
                                                padding: EdgeInsets.only(bottom: 16.0f),
                                                alignment: Alignment.center,
                                                child: new Text(
                                                    asset,
                                                    style: theme.textTheme.caption
                                                )
                                            ),
                                        }
                                    )
                                )
                            ),
                            new SizedBox(height: 24.0f)
                        }
                    );
                }).ToList()
            );
        }
    }

    public class BackdropPanel : StatelessWidget {
        public BackdropPanel(
            Key key = null,
            VoidCallback onTap = null,
            GestureDragUpdateCallback onVerticalDragUpdate = null,
            GestureDragEndCallback onVerticalDragEnd = null,
            Widget title = null,
            Widget child = null
        ) : base(key: key) {
            this.onTap = onTap;
            this.onVerticalDragUpdate = onVerticalDragUpdate;
            this.onVerticalDragEnd = onVerticalDragEnd;
            this.title = title;
            this.child = child;
        }

        public readonly VoidCallback onTap;
        public readonly GestureDragUpdateCallback onVerticalDragUpdate;
        public readonly GestureDragEndCallback onVerticalDragEnd;
        public readonly Widget title;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            return new Material(
                elevation: 2.0f,
                borderRadius: BorderRadius.only(
                    topLeft: Radius.circular(16.0f),
                    topRight: Radius.circular(16.0f)
                ),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: new List<Widget> {
                        new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onVerticalDragUpdate: this.onVerticalDragUpdate,
                            onVerticalDragEnd: this.onVerticalDragEnd,
                            onTap: this.onTap != null ? (GestureTapCallback) (() => { this.onTap(); }) : null,
                            child: new Container(
                                height: 48.0f,
                                padding: EdgeInsets.only(left: 16.0f),
                                alignment: Alignment.centerLeft,
                                child: new DefaultTextStyle(
                                    style: theme.textTheme.subhead,
                                    child: new Tooltip(
                                        message: "Tap to dismiss",
                                        child: this.title
                                    )
                                )
                            )
                        ),
                        new Divider(height: 1.0f),
                        new Expanded(child: this.child)
                    }
                )
            );
        }
    }

    public class BackdropTitle : AnimatedWidget {
        public BackdropTitle(
            Key key = null,
            Listenable listenable = null
        ) : base(key: key, listenable: listenable) {
        }

        protected override Widget build(BuildContext context) {
            Animation<float> animation = (Animation<float>) this.listenable;
            return new DefaultTextStyle(
                style: Theme.of(context).primaryTextTheme.title,
                softWrap: false,
                overflow: TextOverflow.ellipsis,
                child: new Stack(
                    children: new List<Widget> {
                        new Opacity(
                            opacity: new CurvedAnimation(
                                parent: new ReverseAnimation(animation),
                                curve: new Interval(0.5f, 1.0f)
                            ).value,
                            child: new Text("Select a Category")
                        ),
                        new Opacity(
                            opacity: new CurvedAnimation(
                                parent: animation,
                                curve: new Interval(0.5f, 1.0f)
                            ).value,
                            child: new Text("Asset Viewer")
                        ),
                    }
                )
            );
        }
    }

    public class BackdropDemo : StatefulWidget {
        public const string routeName = "/material/backdrop";

        public override State createState() {
            return new _BackdropDemoState();
        }
    }

    class _BackdropDemoState : SingleTickerProviderStateMixin<BackdropDemo> {
        GlobalKey _backdropKey = GlobalKey.key(debugLabel: "Backdrop");
        AnimationController _controller;
        Category _category = BackdropDemoConstants.allCategories[0];

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 300),
                value: 1.0f,
                vsync: this
            );
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        void _changeCategory(Category category) {
            this.setState(() => {
                this._category = category;
                this._controller.fling(velocity: 2.0f);
            });
        }

        bool _backdropPanelVisible {
            get {
                AnimationStatus status = this._controller.status;
                return status == AnimationStatus.completed || status == AnimationStatus.forward;
            }
        }

        void _toggleBackdropPanelVisibility() {
            this._controller.fling(velocity: this._backdropPanelVisible ? -2.0f : 2.0f);
        }

        float? _backdropHeight {
            get {
                RenderBox renderBox = (RenderBox) this._backdropKey.currentContext.findRenderObject();
                return renderBox.size.height;
            }
        }


        void _handleDragUpdate(DragUpdateDetails details) {
            if (this._controller.isAnimating || this._controller.status == AnimationStatus.completed) {
                return;
            }

            this._controller.setValue(this._controller.value -
                                      details.primaryDelta / (this._backdropHeight ?? details.primaryDelta) ?? 0.0f);
        }

        void _handleDragEnd(DragEndDetails details) {
            if (this._controller.isAnimating || this._controller.status == AnimationStatus.completed) {
                return;
            }

            float? flingVelocity = details.velocity.pixelsPerSecond.dy / this._backdropHeight;
            if (flingVelocity < 0.0f) {
                this._controller.fling(velocity: Mathf.Max(2.0f, -flingVelocity ?? 0.0f));
            }
            else if (flingVelocity > 0.0f) {
                this._controller.fling(velocity: Mathf.Min(-2.0f, -flingVelocity ?? 0.0f));
            }
            else {
                this._controller.fling(velocity: this._controller.value < 0.5f ? -2.0f : 2.0f);
            }
        }

        Widget _buildStack(BuildContext context, BoxConstraints constraints) {
            const float panelTitleHeight = 48.0f;
            Size panelSize = constraints.biggest;
            float panelTop = panelSize.height - panelTitleHeight;

            Animation<RelativeRect> panelAnimation = this._controller.drive(
                new RelativeRectTween(
                    begin: RelativeRect.fromLTRB(
                        0.0f,
                        panelTop - MediaQuery.of(context).padding.bottom,
                        0.0f,
                        panelTop - panelSize.height
                    ),
                    end: RelativeRect.fromLTRB(0.0f, 0.0f, 0.0f, 0.0f)
                )
            );

            ThemeData theme = Theme.of(context);
            List<Widget> backdropItems = BackdropDemoConstants.allCategories.Select<Category, Widget>(
                (Category category) => {
                    bool selected = category == this._category;
                    return new Material(
                        shape: new RoundedRectangleBorder(
                            borderRadius: BorderRadius.all(Radius.circular(4.0f))
                        ),
                        color: selected
                            ? Colors.white.withOpacity(0.25f)
                            : Colors.transparent,
                        child: new ListTile(
                            title: new Text(category.title),
                            selected: selected,
                            onTap: () => { this._changeCategory(category); }
                        )
                    );
                }).ToList();

            return new Container(
                key: this._backdropKey,
                color: theme.primaryColor,
                child: new Stack(
                    children: new List<Widget> {
                        new ListTileTheme(
                            iconColor: theme.primaryIconTheme.color,
                            textColor: theme.primaryTextTheme.title.color.withOpacity(0.6f),
                            selectedColor: theme.primaryTextTheme.title.color,
                            child: new Padding(
                                padding: EdgeInsets.symmetric(horizontal: 16.0f),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.stretch,
                                    children: backdropItems
                                )
                            )
                        ),
                        new PositionedTransition(
                            rect: panelAnimation,
                            child: new BackdropPanel(
                                onTap: this._toggleBackdropPanelVisibility,
                                onVerticalDragUpdate: this._handleDragUpdate,
                                onVerticalDragEnd: this._handleDragEnd,
                                title: new Text(this._category.title),
                                child: new CategoryView(category: this._category)
                            )
                        ),
                    }
                )
            );
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                appBar: new AppBar(
                    elevation: 0.0f,
                    title: new BackdropTitle(
                        listenable: this._controller.view
                    ),
                    actions: new List<Widget> {
                        new IconButton(
                            onPressed: this._toggleBackdropPanelVisibility,
                            icon: new AnimatedIcon(
                                icon: AnimatedIcons.close_menu,
                                progress: this._controller.view
                            )
                        )
                    }
                ),
                body: new LayoutBuilder(
                    builder: this._buildStack
                )
            );
        }
    }
}