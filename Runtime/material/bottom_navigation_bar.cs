using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.material {
    public enum BottomNavigationBarType {
        fix,
        shifting
    }

    public class BottomNavigationBar : StatefulWidget {
        public BottomNavigationBar(
            Key key = null,
            List<BottomNavigationBarItem> items = null,
            ValueChanged<int> onTap = null,
            int currentIndex = 0,
            float elevation = 8.0f,
            BottomNavigationBarType? type = null,
            Color fixedColor = null,
            Color backgroundColor = null,
            float iconSize = 24.0f,
            Color selectedItemColor = null,
            Color unselectedItemColor = null,
            float selectedFontSize = 14.0f,
            float unselectedFontSize = 12.0f,
            bool showSelectedLabels = true,
            bool? showUnselectedLabels = null
        ) : base(key: key) {
            D.assert(items != null);
            D.assert(items.Count >= 2);
            D.assert(items.All((BottomNavigationBarItem item) => item.title != null) == true,
                () => "Every item must have a non-null title"
            );
            D.assert(0 <= currentIndex && currentIndex < items.Count);
            D.assert(elevation >= 0.0f);
            D.assert(iconSize >= 0.0f);
            D.assert(selectedItemColor == null || fixedColor == null,
                () => "Either selectedItemColor or fixedColor can be specified, but not both!");
            D.assert(selectedFontSize >= 0.0f);
            D.assert(unselectedFontSize >= 0.0f);
            type = _type(type, items);
            this.items = items;
            this.onTap = onTap;
            this.currentIndex = currentIndex;
            this.elevation = elevation;
            this.type = type ?? (items.Count <= 3 ? BottomNavigationBarType.fix : BottomNavigationBarType.shifting);
            this.backgroundColor = backgroundColor;
            this.iconSize = iconSize;
            this.selectedItemColor = selectedItemColor ?? fixedColor;
            this.unselectedItemColor = unselectedItemColor;
            this.selectedFontSize = selectedFontSize;
            this.unselectedFontSize = unselectedFontSize;
            this.showSelectedLabels = showSelectedLabels;
            this.showUnselectedLabels = showUnselectedLabels ?? _defaultShowUnselected(_type(type, items));
        }

        public readonly List<BottomNavigationBarItem> items;

        public readonly ValueChanged<int> onTap;

        public readonly int currentIndex;

        public readonly float elevation;

        public readonly BottomNavigationBarType? type;

        public Color fixedColor {
            get { return this.selectedItemColor; }
        }

        public readonly Color backgroundColor;

        public readonly float iconSize;


        public readonly Color selectedItemColor;

        public readonly Color unselectedItemColor;

        public readonly float selectedFontSize;

        public readonly float unselectedFontSize;

        public readonly bool showUnselectedLabels;

        public readonly bool showSelectedLabels;

        static BottomNavigationBarType _type(
            BottomNavigationBarType? type,
            List<BottomNavigationBarItem> items
        ) {
            if (type != null) {
                return type.Value;
            }

            return items.Count <= 3 ? BottomNavigationBarType.fix : BottomNavigationBarType.shifting;
        }

        static bool _defaultShowUnselected(BottomNavigationBarType type) {
            switch (type) {
                case BottomNavigationBarType.shifting:
                    return false;
                case BottomNavigationBarType.fix:
                    return true;
            }

            D.assert(false);
            return false;
        }

        public override State createState() {
            return new _BottomNavigationBarState();
        }
    }

    class _BottomNavigationTile : StatelessWidget {
        public _BottomNavigationTile(
            BottomNavigationBarType? type,
            BottomNavigationBarItem item,
            Animation<float> animation,
            float? iconSize = null,
            VoidCallback onTap = null,
            ColorTween colorTween = null,
            float? flex = null,
            bool selected = false,
            float? selectedFontSize = null,
            float? unselectedFontSize = null,
            bool? showSelectedLabels = null,
            bool? showUnselectedLabels = null,
            string indexLabel = null
        ) {
            D.assert(type != null);
            D.assert(item != null);
            D.assert(animation != null);
            D.assert(selectedFontSize != null && selectedFontSize >= 0);
            D.assert(unselectedFontSize != null && unselectedFontSize >= 0);
            this.type = type;
            this.item = item;
            this.animation = animation;
            this.iconSize = iconSize;
            this.onTap = onTap;
            this.colorTween = colorTween;
            this.flex = flex;
            this.selected = selected;
            this.selectedFontSize = selectedFontSize.Value;
            this.unselectedFontSize = unselectedFontSize.Value;
            this.showSelectedLabels = showSelectedLabels ?? false;
            this.showUnselectedLabels = showUnselectedLabels ?? false;
            this.indexLabel = indexLabel;
        }

        public readonly BottomNavigationBarType? type;
        public readonly BottomNavigationBarItem item;
        public readonly Animation<float> animation;
        public readonly float? iconSize;
        public readonly VoidCallback onTap;
        public readonly ColorTween colorTween;
        public readonly float? flex;
        public readonly bool selected;
        public readonly float selectedFontSize;
        public readonly float unselectedFontSize;
        public readonly string indexLabel;
        public readonly bool showSelectedLabels;
        public readonly bool showUnselectedLabels;

        public override Widget build(BuildContext context) {
            int size;
            float bottomPadding = this.selectedFontSize / 2.0f;
            float topPadding = this.selectedFontSize / 2.0f;
            if (this.showSelectedLabels && !this.showUnselectedLabels) {
                bottomPadding = new FloatTween(
                    begin: 0.0f,
                    end: this.selectedFontSize / 2.0f
                ).evaluate(this.animation);
                topPadding = new FloatTween(
                    begin: this.selectedFontSize,
                    end: this.selectedFontSize / 2.0f
                ).evaluate(this.animation);
            }

            if (!this.showSelectedLabels && !this.showUnselectedLabels) {
                bottomPadding = 0.0f;
                topPadding = this.selectedFontSize;
            }
            switch (this.type) {
                case BottomNavigationBarType.fix:
                    size = 1;
                    break;
                case BottomNavigationBarType.shifting:
                    size = ((this.flex * 1000.0f) ?? 0.0f).round();
                    break;
                default:
                    throw new Exception("Unknown BottomNavigationBarType: " + this.type);
            }

            return new Expanded(
                flex: size,
                child: new Stack(
                    children: new List<Widget> {
                        new InkResponse(
                            onTap: this.onTap == null ? (GestureTapCallback) null : () => { this.onTap(); },
                            child: new Padding(
                                padding: EdgeInsets.only(top: topPadding, bottom: bottomPadding),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.center,
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    mainAxisSize: MainAxisSize.min,
                                    children: new List<Widget> {
                                        new _TileIcon(
                                            colorTween: this.colorTween,
                                            animation: this.animation,
                                            iconSize: this.iconSize,
                                            selected: this.selected,
                                            item: this.item
                                        ),
                                        new _Label(
                                            colorTween: this.colorTween,
                                            animation: this.animation,
                                            item: this.item,
                                            selectedFontSize: this.selectedFontSize,
                                            unselectedFontSize: this.unselectedFontSize,
                                            showSelectedLabels: this.showSelectedLabels,
                                            showUnselectedLabels: this.showUnselectedLabels
                                        )
                                    }
                                )
                            )
                        )
                    }
                )
            );
        }
    }


    class _TileIcon : StatelessWidget {
        public _TileIcon(
            Key key = null,
            ColorTween colorTween = null,
            Animation<float> animation = null,
            float? iconSize = null,
            bool? selected = null,
            BottomNavigationBarItem item = null
        ) : base(key: key) {
            D.assert(selected != null);
            D.assert(item != null);
            this.colorTween = colorTween;
            this.animation = animation;
            this.iconSize = iconSize;
            this.selected = selected;
            this.item = item;
        }

        ColorTween colorTween;
        Animation<float> animation;
        float? iconSize;
        bool? selected;
        BottomNavigationBarItem item;

        public override Widget build(BuildContext context) {
            Color iconColor = this.colorTween.evaluate(this.animation);

            return new Align(
                alignment: Alignment.topCenter,
                heightFactor: 1.0f,
                child: new Container(
                    child: new IconTheme(
                        data: new IconThemeData(
                            color: iconColor,
                            size: this.iconSize
                        ),
                        child: this.selected == true ? this.item.activeIcon : this.item.icon
                    )
                )
            );
        }
    }

    class _Label : StatelessWidget {
        public _Label(
            Key key = null,
            ColorTween colorTween = null,
            Animation<float> animation = null,
            BottomNavigationBarItem item = null,
            float? selectedFontSize = null,
            float? unselectedFontSize = null,
            bool? showSelectedLabels = null,
            bool? showUnselectedLabels = null
        ) : base(key: key) {
            D.assert(colorTween != null);
            D.assert(animation != null);
            D.assert(item != null);
            D.assert(selectedFontSize != null);
            D.assert(unselectedFontSize != null);
            D.assert(showSelectedLabels != null);
            D.assert(showUnselectedLabels != null);
            this.colorTween = colorTween;
            this.animation = animation;
            this.item = item;
            this.selectedFontSize = selectedFontSize.Value;
            this.unselectedFontSize = unselectedFontSize.Value;
            this.showSelectedLabels = showSelectedLabels.Value;
            this.showUnselectedLabels = showUnselectedLabels.Value;
        }

        public readonly ColorTween colorTween;
        public readonly Animation<float> animation;
        public readonly BottomNavigationBarItem item;
        public readonly float selectedFontSize;
        public readonly float unselectedFontSize;
        public readonly bool showSelectedLabels;
        public readonly bool showUnselectedLabels;

        public override Widget build(BuildContext context) {
            float t = new FloatTween(begin: this.unselectedFontSize / this.selectedFontSize, end: 1.0f)
                    .evaluate(this.animation);
            Widget text = DefaultTextStyle.merge(
                style: new TextStyle(
                    fontSize: this.selectedFontSize,
                    color: this.colorTween.evaluate(this.animation)
                ),
                child: new Transform(
                    transform: Matrix3.makeAll(t, 0, 0,
                                               0, t, 0,
                                               0, 0, 1),
                    alignment: Alignment.bottomCenter,
                    child: this.item.title
                )
            );
            if (!this.showUnselectedLabels && !this.showSelectedLabels) {
                text = new Opacity(
                    opacity: 0.0f,
                    child: text
                );
            }
            else if (!this.showUnselectedLabels) {
                text = new FadeTransition(
                    opacity: this.animation,
                    child: text
                );
            }
            else if (!this.showSelectedLabels) {
                text = new FadeTransition(
                    opacity: new FloatTween(begin: 1.0f, end: 0.0f).animate(this.animation),
                    child: text
                );
            }
            return new Align(
                alignment: Alignment.bottomCenter,
                heightFactor: 1.0f,
                child: new Container(child: text)
            );
        }
    }

    class _BottomNavigationBarState : TickerProviderStateMixin<BottomNavigationBar> {
        public List<AnimationController> _controllers = new List<AnimationController> { };
        public List<CurvedAnimation> _animations;

        Queue<_Circle> _circles = new Queue<_Circle>();

        Color _backgroundColor;

        static readonly Animatable<float> _flexTween = new FloatTween(begin: 1.0f, end: 1.5f);

        public _BottomNavigationBarState() {
        }

        void _resetState() {
            foreach (AnimationController controller in this._controllers) {
                controller.dispose();
            }

            foreach (_Circle circle in this._circles) {
                circle.dispose();
            }

            this._circles.Clear();

            this._controllers = new List<AnimationController>(capacity: this.widget.items.Count);
            for (int index = 0; index < this.widget.items.Count; index++) {
                AnimationController controller = new AnimationController(
                    duration: ThemeUtils.kThemeAnimationDuration,
                    vsync: this
                );
                controller.addListener(this._rebuild);
                this._controllers.Add(controller);
            }

            this._animations = new List<CurvedAnimation>(capacity: this.widget.items.Count);
            for (int index = 0; index < this.widget.items.Count; index++) {
                this._animations.Add(new CurvedAnimation(
                    parent: this._controllers[index],
                    curve: Curves.fastOutSlowIn,
                    reverseCurve: Curves.fastOutSlowIn.flipped
                ));
            }

            this._controllers[this.widget.currentIndex].setValue(1.0f);
            this._backgroundColor = this.widget.items[this.widget.currentIndex].backgroundColor;
        }

        public override void initState() {
            base.initState();
            this._resetState();
        }

        void _rebuild() {
            this.setState(() => { });
        }

        public override void dispose() {
            foreach (AnimationController controller in this._controllers) {
                controller.dispose();
            }

            foreach (_Circle circle in this._circles) {
                circle.dispose();
            }

            base.dispose();
        }

        public float _evaluateFlex(Animation<float> animation) {
            return _flexTween.evaluate(animation);
        }

        void _pushCircle(int index) {
            if (this.widget.items[index].backgroundColor != null) {
                _Circle circle = new _Circle(
                    state: this,
                    index: index,
                    color: this.widget.items[index].backgroundColor,
                    vsync: this
                );
                circle.controller.addStatusListener(
                    (AnimationStatus status) => {
                        switch (status) {
                            case AnimationStatus.completed:
                                this.setState(() => {
                                    _Circle cir = this._circles.Dequeue();
                                    this._backgroundColor = cir.color;
                                    cir.dispose();
                                });
                                break;
                            case AnimationStatus.dismissed:
                            case AnimationStatus.forward:
                            case AnimationStatus.reverse:
                                break;
                        }
                    }
                );
                this._circles.Enqueue(circle);
            }
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            BottomNavigationBar oldWidget = _oldWidget as BottomNavigationBar;
            if (this.widget.items.Count != oldWidget.items.Count) {
                this._resetState();
                return;
            }

            if (this.widget.currentIndex != oldWidget.currentIndex) {
                switch (this.widget.type) {
                    case BottomNavigationBarType.fix:
                        break;
                    case BottomNavigationBarType.shifting:
                        this._pushCircle(this.widget.currentIndex);
                        break;
                }

                this._controllers[oldWidget.currentIndex].reverse();
                this._controllers[this.widget.currentIndex].forward();
            }
            else {
                if (this._backgroundColor != this.widget.items[this.widget.currentIndex].backgroundColor) {
                    this._backgroundColor = this.widget.items[this.widget.currentIndex].backgroundColor;
                }
            }
        }

        List<Widget> _createTiles() {
            MaterialLocalizations localizations = MaterialLocalizations.of(this.context);
            D.assert(localizations != null);
            ThemeData themeData = Theme.of(this.context);
            Color themeColor;
            switch (themeData.brightness) {
                case Brightness.light:
                    themeColor = themeData.primaryColor;
                    break;
                case Brightness.dark:
                    themeColor = themeData.accentColor;
                    break;
                default:
                    throw new Exception("Unknown brightness: " + themeData.brightness);
            }

            ColorTween colorTween;
            switch (this.widget.type) {
                case BottomNavigationBarType.fix:
                    colorTween = new ColorTween(
                        begin: this.widget.unselectedItemColor ?? themeData.textTheme.caption.color,
                        end: this.widget.selectedItemColor ?? this.widget.fixedColor ?? themeColor
                    );
                    break;
                case BottomNavigationBarType.shifting:
                    colorTween = new ColorTween(
                        begin: this.widget.unselectedItemColor ?? Colors.white,
                        end: this.widget.selectedItemColor ?? Colors.white
                    );
                    break;
                default:
                    throw new UIWidgetsError($"Unknown bottom navigation bar type: {this.widget.type}");
            }

            List<Widget> tiles = new List<Widget>();
            for (int i = 0; i < this.widget.items.Count; i++) {
                int index = i;
                tiles.Add(new _BottomNavigationTile(
                    this.widget.type,
                    this.widget.items[i],
                    this._animations[i],
                    this.widget.iconSize,
                    selectedFontSize: this.widget.selectedFontSize,
                    unselectedFontSize: this.widget.unselectedFontSize,
                    onTap: () => {
                        if (this.widget.onTap != null) {
                            this.widget.onTap(index);
                        }
                    },
                    colorTween: colorTween,
                    flex: this._evaluateFlex(this._animations[i]),
                    selected: i == this.widget.currentIndex,
                    showSelectedLabels: this.widget.showSelectedLabels,
                    showUnselectedLabels: this.widget.showUnselectedLabels,
                    indexLabel: localizations.tabLabel(tabIndex: i+1, tabCount: this.widget.items.Count)
                ));
            }

            return tiles;
        }

        Widget _createContainer(List<Widget> tiles) {
            return DefaultTextStyle.merge(
                overflow: TextOverflow.ellipsis,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: tiles
                )
            );
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            float additionalBottomPadding =
                Mathf.Max(MediaQuery.of(context).padding.bottom - this.widget.selectedFontSize / 2.0f, 0.0f);
            Color backgroundColor = null;
            switch (this.widget.type) {
                case BottomNavigationBarType.fix:
                    backgroundColor = this.widget.backgroundColor;
                    break;
                case BottomNavigationBarType.shifting:
                    backgroundColor = this._backgroundColor;
                    break;
            }


            return new Material(
                elevation: this.widget.elevation,
                color: backgroundColor,
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(
                        minHeight: Constants.kBottomNavigationBarHeight + additionalBottomPadding),
                    child: new CustomPaint(
                        painter: new _RadialPainter(
                            circles: this._circles.ToList()
                        ),
                        child: new Material( // Splashes.
                            type: MaterialType.transparency,
                            child: new Padding(
                                padding: EdgeInsets.only(bottom: additionalBottomPadding),
                                child: MediaQuery.removePadding(
                                    context: context,
                                    removeBottom: true,
                                    child: this._createContainer(this._createTiles())
                                )
                            )
                        )
                    )
                )
            );
        }
    }

    class _Circle {
        public _Circle(
            _BottomNavigationBarState state = null,
            int? index = null,
            Color color = null,
            TickerProvider vsync = null
        ) {
            D.assert(state != null);
            D.assert(index != null);
            D.assert(color != null);
            this.state = state;
            this.index = index;
            this.color = color;
            this.controller = new AnimationController(
                duration: ThemeUtils.kThemeAnimationDuration,
                vsync: vsync
            );
            this.animation = new CurvedAnimation(
                parent: this.controller,
                curve: Curves.fastOutSlowIn
            );
            this.controller.forward();
        }

        public readonly _BottomNavigationBarState state;
        public readonly int? index;
        public readonly Color color;
        public readonly AnimationController controller;
        public readonly CurvedAnimation animation;

        public float horizontalLeadingOffset {
            get {
                float weightSum(IEnumerable<Animation<float>> animations) {
                    return animations.Select(this.state._evaluateFlex).Sum();
                }

                float allWeights = weightSum(this.state._animations);
                float leadingWeights = weightSum(this.state._animations.GetRange(0, this.index ?? 0));

                return (leadingWeights + this.state._evaluateFlex(this.state._animations[this.index ?? 0]) / 2.0f) /
                       allWeights;
            }
        }

        public void dispose() {
            this.controller.dispose();
        }
    }

    class _RadialPainter : AbstractCustomPainter {
        public _RadialPainter(
            List<_Circle> circles
        ) {
            D.assert(circles != null);
            this.circles = circles;
        }

        public readonly List<_Circle> circles;

        static float _maxRadius(Offset center, Size size) {
            float maxX = Mathf.Max(center.dx, size.width - center.dx);
            float maxY = Mathf.Max(center.dy, size.height - center.dy);
            return Mathf.Sqrt(maxX * maxX + maxY * maxY);
        }

        public override bool shouldRepaint(CustomPainter _oldPainter) {
            _RadialPainter oldPainter = _oldPainter as _RadialPainter;
            if (this.circles == oldPainter.circles) {
                return false;
            }

            if (this.circles.Count != oldPainter.circles.Count) {
                return true;
            }

            for (int i = 0; i < this.circles.Count; i += 1) {
                if (this.circles[i] != oldPainter.circles[i]) {
                    return true;
                }
            }

            return false;
        }

        public override void paint(Canvas canvas, Size size) {
            foreach (_Circle circle in this.circles) {
                Paint paint = new Paint();
                paint.color = circle.color;
                Rect rect = Rect.fromLTWH(0.0f, 0.0f, size.width, size.height);
                canvas.clipRect(rect);
                float leftFraction = circle.horizontalLeadingOffset;
                Offset center = new Offset(leftFraction * size.width, size.height / 2.0f);
                FloatTween radiusTween = new FloatTween(
                    begin: 0.0f,
                    end: _maxRadius(center, size)
                );
                canvas.drawCircle(
                    center,
                    radiusTween.evaluate(circle.animation),
                    paint
                );
            }
        }
    }
}