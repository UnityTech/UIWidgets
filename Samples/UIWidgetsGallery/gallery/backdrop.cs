using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.gallery {
    class BackdropConstants {
        public const float _kFrontHeadingHeight = 32.0f;
        public const float _kFrontClosedHeight = 92.0f;
        public const float _kBackAppBarHeight = 56.0f;

        public static readonly Animatable<BorderRadius> _kFrontHeadingBevelRadius = new BorderRadiusTween(
            begin: BorderRadius.only(
                topLeft: Radius.circular(12.0f),
                topRight: Radius.circular(12.0f)
            ),
            end: BorderRadius.only(
                topLeft: Radius.circular(_kFrontHeadingHeight),
                topRight: Radius.circular(_kFrontHeadingHeight)
            )
        );
    }

    class _TappableWhileStatusIs : StatefulWidget {
        public _TappableWhileStatusIs(
            AnimationStatus status,
            Key key = null,
            AnimationController controller = null,
            Widget child = null
        ) : base(key: key) {
            this.controller = controller;
            this.status = status;
            this.child = child;
        }

        public readonly AnimationController controller;
        public readonly AnimationStatus status;
        public readonly Widget child;

        public override State createState() {
            return new _TappableWhileStatusIsState();
        }
    }

    class _TappableWhileStatusIsState : State<_TappableWhileStatusIs> {
        bool _active;

        public override void initState() {
            base.initState();
            this.widget.controller.addStatusListener(this._handleStatusChange);
            this._active = this.widget.controller.status == this.widget.status;
        }

        public override void dispose() {
            this.widget.controller.removeStatusListener(this._handleStatusChange);
            base.dispose();
        }

        void _handleStatusChange(AnimationStatus status) {
            bool value = this.widget.controller.status == this.widget.status;
            if (this._active != value) {
                this.setState(() => { this._active = value; });
            }
        }

        public override Widget build(BuildContext context) {
            return new AbsorbPointer(
                absorbing: !this._active,
                child: this.widget.child
            );
        }
    }

    class _CrossFadeTransition : AnimatedWidget {
        public _CrossFadeTransition(
            Key key = null,
            Alignment alignment = null,
            Animation<float> progress = null,
            Widget child0 = null,
            Widget child1 = null
        ) : base(key: key, listenable: progress) {
            this.alignment = alignment ?? Alignment.center;
            this.child0 = child0;
            this.child1 = child1;
        }

        public readonly Alignment alignment;
        public readonly Widget child0;
        public readonly Widget child1;

        protected override Widget build(BuildContext context) {
            Animation<float> progress = this.listenable as Animation<float>;

            float opacity1 = new CurvedAnimation(
                parent: new ReverseAnimation(progress),
                curve: new Interval(0.5f, 1.0f)
            ).value;

            float opacity2 = new CurvedAnimation(
                parent: progress,
                curve: new Interval(0.5f, 1.0f)
            ).value;

            return new Stack(
                alignment: this.alignment,
                children: new List<Widget> {
                    new Opacity(
                        opacity: opacity1,
                        child: this.child1
                    ),
                    new Opacity(
                        opacity: opacity2,
                        child: this.child0
                    )
                }
            );
        }
    }

    class _BackAppBar : StatelessWidget {
        public _BackAppBar(
            Key key = null,
            Widget leading = null,
            Widget title = null,
            Widget trailing = null
        ) : base(key: key) {
            D.assert(title != null);
            this.leading = leading ?? new SizedBox(width: 56.0f);
            this.title = title;
            this.trailing = trailing;
        }

        public readonly Widget leading;
        public readonly Widget title;
        public readonly Widget trailing;

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget> {
                new Container(
                    alignment: Alignment.center,
                    width: 56.0f,
                    child: this.leading
                ),
                new Expanded(
                    child: this.title
                ),
            };

            if (this.trailing != null) {
                children.Add(
                    new Container(
                        alignment: Alignment.center,
                        width: 56.0f,
                        child: this.trailing
                    )
                );
            }

            ThemeData theme = Theme.of(context);

            return IconTheme.merge(
                data: theme.primaryIconTheme,
                child: new DefaultTextStyle(
                    style: theme.primaryTextTheme.title,
                    child: new SizedBox(
                        height: BackdropConstants._kBackAppBarHeight,
                        child: new Row(children: children)
                    )
                )
            );
        }
    }

    public class Backdrop : StatefulWidget {
        public Backdrop(
            Widget frontAction = null,
            Widget frontTitle = null,
            Widget frontHeading = null,
            Widget frontLayer = null,
            Widget backTitle = null,
            Widget backLayer = null
        ) {
            this.frontAction = frontAction;
            this.frontTitle = frontTitle;
            this.frontHeading = frontHeading;
            this.frontLayer = frontLayer;
            this.backTitle = backTitle;
            this.backLayer = backLayer;
        }

        public readonly Widget frontAction;
        public readonly Widget frontTitle;
        public readonly Widget frontLayer;
        public readonly Widget frontHeading;
        public readonly Widget backTitle;
        public readonly Widget backLayer;

        public override State createState() {
            return new _BackdropState();
        }
    }

    class _BackdropState : SingleTickerProviderStateMixin<Backdrop> {
        GlobalKey _backdropKey = GlobalKey.key(debugLabel: "Backdrop");
        AnimationController _controller;
        Animation<float> _frontOpacity;

        static Animatable<float> _frontOpacityTween = new FloatTween(begin: 0.2f, end: 1.0f)
            .chain(new CurveTween(curve: new Interval(0.0f, 0.4f, curve: Curves.easeInOut)));

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 300),
                value: 1.0f,
                vsync: this
            );
            this._frontOpacity = this._controller.drive(_frontOpacityTween);
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        float? _backdropHeight {
            get {
                RenderBox renderBox = (RenderBox) this._backdropKey.currentContext.findRenderObject();
                return Mathf.Max(0.0f,
                    renderBox.size.height - BackdropConstants._kBackAppBarHeight -
                    BackdropConstants._kFrontClosedHeight);
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
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
                this._controller.fling(velocity: this._controller.value < 0.5 ? -2.0f : 2.0f);
            }
        }

        void _toggleFrontLayer() {
            AnimationStatus status = this._controller.status;
            bool isOpen = status == AnimationStatus.completed || status == AnimationStatus.forward;
            this._controller.fling(velocity: isOpen ? -2.0f : 2.0f);
        }

        Widget _buildStack(BuildContext context, BoxConstraints constraints) {
            Animation<RelativeRect> frontRelativeRect = this._controller.drive(new RelativeRectTween(
                begin: RelativeRect.fromLTRB(0.0f, constraints.biggest.height - BackdropConstants._kFrontClosedHeight,
                    0.0f, 0.0f),
                end: RelativeRect.fromLTRB(0.0f, BackdropConstants._kBackAppBarHeight, 0.0f, 0.0f)
            ));

            List<Widget> layers = new List<Widget> {
                new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: new List<Widget> {
                        new _BackAppBar(
                            leading: this.widget.frontAction,
                            title: new _CrossFadeTransition(
                                progress: this._controller,
                                alignment: Alignment.centerLeft,
                                child0: this.widget.frontTitle,
                                child1: this.widget.backTitle
                            ),
                            trailing: new IconButton(
                                onPressed: this._toggleFrontLayer,
                                tooltip: "Toggle options page",
                                icon: new AnimatedIcon(
                                    icon: AnimatedIcons.close_menu,
                                    progress: this._controller
                                )
                            )
                        ),
                        new Expanded(
                            child: new Visibility(
                                child: this.widget.backLayer,
                                visible: this._controller.status != AnimationStatus.completed,
                                maintainState: true
                            )
                        )
                    }
                ),
                new PositionedTransition(
                    rect: frontRelativeRect,
                    child: new AnimatedBuilder(
                        animation: this._controller,
                        builder: (BuildContext _context, Widget child) => {
                            return new PhysicalShape(
                                elevation: 12.0f,
                                color: Theme.of(_context).canvasColor,
                                clipper: new ShapeBorderClipper(
                                    shape: new BeveledRectangleBorder(
                                        borderRadius: BackdropConstants._kFrontHeadingBevelRadius.evaluate(
                                            this._controller)
                                    )
                                ),
                                clipBehavior: Clip.antiAlias,
                                child: child
                            );
                        },
                        child: new _TappableWhileStatusIs(
                            AnimationStatus.completed,
                            controller: this._controller,
                            child: new FadeTransition(
                                opacity: this._frontOpacity,
                                child: this.widget.frontLayer
                            )
                        )
                    )
                )
            };

            if (this.widget.frontHeading != null) {
                layers.Add(
                    new PositionedTransition(
                        rect: frontRelativeRect,
                        child: new Container(
                            alignment: Alignment.topLeft,
                            child: new GestureDetector(
                                behavior: HitTestBehavior.opaque,
                                onTap: this._toggleFrontLayer,
                                onVerticalDragUpdate: this._handleDragUpdate,
                                onVerticalDragEnd: this._handleDragEnd,
                                child: this.widget.frontHeading
                            )
                        )
                    )
                );
            }

            return new Stack(
                key: this._backdropKey,
                children: layers
            );
        }

        public override Widget build(BuildContext context) {
            return new LayoutBuilder(builder: this._buildStack);
        }
    }
}