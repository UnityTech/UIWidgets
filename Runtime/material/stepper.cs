using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public enum StepState {
        indexed,
        editing,
        complete,
        disabled,
        error
    }

    public enum StepperType {
        vertical,
        horizontal
    }

    public class Step {
        public Step(
            Widget title,
            Widget content,
            Widget subtitle = null,
            StepState state = StepState.indexed,
            bool isActive = false
        ) {
            D.assert(title != null);
            D.assert(content != null);

            this.title = title;
            this.content = content;
            this.subtitle = subtitle;
            this.state = state;
            this.isActive = isActive;
        }

        public readonly Widget title;
        public readonly Widget subtitle;
        public readonly Widget content;
        public readonly StepState state;
        public readonly bool isActive;
    }

    public class Stepper : StatefulWidget {
        public Stepper(
            List<Step> steps,
            Key key = null,
            StepperType type = StepperType.vertical,
            int currentStep = 0,
            ValueChanged<int> onStepTapped = null,
            VoidCallback onStepContinue = null,
            VoidCallback onStepCancel = null,
            ControlsWidgetBuilder controlsBuilder = null
        ) : base(key) {
            this.steps = steps;
            this.type = type;
            this.currentStep = currentStep;
            this.onStepTapped = onStepTapped;
            this.onStepContinue = onStepContinue;
            this.onStepCancel = onStepCancel;
            this.controlsBuilder = controlsBuilder;
        }

        public readonly List<Step> steps;
        public readonly StepperType type;
        public readonly int currentStep;
        public readonly ValueChanged<int> onStepTapped;
        public readonly VoidCallback onStepContinue;
        public readonly VoidCallback onStepCancel;
        public readonly ControlsWidgetBuilder controlsBuilder;

        public override State createState() {
            return new _StepperState();
        }
    }

    class _StepperState : TickerProviderStateMixin<Stepper> {
        static readonly TextStyle _kStepStyleLight = new TextStyle(
            fontSize: 12.0f,
            color: Colors.white
        );

        static readonly TextStyle _kStepStyleDark = new TextStyle(
            fontSize: 12.0f,
            color: Colors.black87
        );

        static readonly Color _kErrorLight = Colors.red;
        static readonly Color _kErrorDark = Colors.red.shade400;
        static readonly Color _kCircleActiveLight = Colors.white;
        static readonly Color _kCircleActiveDark = Colors.black87;
        static readonly Color _kDisabledLight = Colors.black38;
        static readonly Color _kDisabledDark = Colors.white30;
        static readonly float _kStepSize = 24.0f;
        static readonly float _kTriangleHeight = 24.0f * 0.866025f;

        List<GlobalKey> _keys;
        readonly Dictionary<int, StepState> _oldStates = new Dictionary<int, StepState>();

        public override void initState() {
            base.initState();
            this._keys = new List<GlobalKey>();
            for (int i = 0; i < this.widget.steps.Count; i++) {
                this._keys.Add(GlobalKey.key());
                this._oldStates[i] = this.widget.steps[i].state;
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            Stepper _oldWidget = (Stepper) oldWidget;
            D.assert(this.widget.steps.Count == _oldWidget.steps.Count);

            for (int i = 0; i < _oldWidget.steps.Count; i++) {
                this._oldStates[i] = _oldWidget.steps[i].state;
            }
        }

        bool _isFirst(int index) {
            return index == 0;
        }

        bool _isLast(int index) {
            return this.widget.steps.Count - 1 == index;
        }

        bool _isCurrent(int index) {
            return this.widget.currentStep == index;
        }

        bool _isDark() {
            return Theme.of(this.context).brightness == Brightness.dark;
        }

        Widget _buildLine(bool visible) {
            return new Container(
                width: visible ? 1.0f : 0.0f,
                height: 16.0f,
                color: Colors.grey.shade400
            );
        }

        Widget _buildCircleChild(int index, bool oldState) {
            StepState state = oldState ? this._oldStates[index] : this.widget.steps[index].state;
            bool isDarkActive = this._isDark() && this.widget.steps[index].isActive;

            switch (state) {
                case StepState.indexed:
                case StepState.disabled:
                    return new Text(
                        (index + 1).ToString(),
                        style: isDarkActive ? _kStepStyleDark : _kStepStyleLight
                    );
                case StepState.editing:
                    return new Icon(
                        Icons.edit,
                        color: isDarkActive ? _kCircleActiveDark : _kCircleActiveLight,
                        size: 18.0f
                    );
                case StepState.complete:
                    return new Icon(
                        Icons.check,
                        color: isDarkActive ? _kCircleActiveDark : _kCircleActiveLight,
                        size: 18.0f
                    );
                case StepState.error:
                    return new Text(
                        "!",
                        style: _kStepStyleLight
                    );
            }

            return null;
        }

        Color _circleColor(int index) {
            ThemeData themeData = Theme.of(this.context);
            if (this._isDark()) {
                return this.widget.steps[index].isActive ? themeData.primaryColor : Colors.black38;
            }
            else {
                return this.widget.steps[index].isActive ? themeData.accentColor : themeData.backgroundColor;
            }
        }

        Widget _buildCircle(int index, bool oldState) {
            return new Container(
                margin: EdgeInsets.symmetric(vertical: 8.0f),
                width: _kStepSize,
                height: _kStepSize,
                child: new AnimatedContainer(
                    curve: Curves.fastOutSlowIn,
                    duration: ThemeUtils.kThemeAnimationDuration,
                    decoration: new BoxDecoration(
                        color: this._circleColor(index),
                        shape: BoxShape.circle
                    ),
                    child: new Center(
                        child: this._buildCircleChild(index,
                            oldState && this.widget.steps[index].state == StepState.error
                        )
                    )
                )
            );
        }

        Widget _buildTriangle(int index, bool oldState) {
            return new Container(
                margin: EdgeInsets.symmetric(vertical: 8.0f),
                width: _kStepSize,
                height: _kStepSize,
                child: new Center(
                    child: new SizedBox(
                        width: _kStepSize,
                        height: _kTriangleHeight,
                        child: new CustomPaint(
                            painter: new _TrianglePainter(
                                color: this._isDark() ? _kErrorDark : _kErrorLight),
                            child: new Align(
                                alignment: new Alignment(0.0f, 0.8f),
                                child: this._buildCircleChild(index,
                                    oldState && this.widget.steps[index].state != StepState.error)
                            )
                        )
                    )
                )
            );
        }

        Widget _buildIcon(int index) {
            if (this.widget.steps[index].state != this._oldStates[index]) {
                return new AnimatedCrossFade(
                    firstChild: this._buildCircle(index, true),
                    secondChild: this._buildTriangle(index, true),
                    firstCurve: new Interval(0.0f, 0.6f, curve: Curves.fastOutSlowIn),
                    secondCurve: new Interval(0.4f, 1.0f, curve: Curves.fastOutSlowIn),
                    sizeCurve: Curves.fastOutSlowIn,
                    crossFadeState: this.widget.steps[index].state == StepState.error
                        ? CrossFadeState.showSecond
                        : CrossFadeState.showFirst,
                    duration: ThemeUtils.kThemeAnimationDuration
                );
            }
            else {
                if (this.widget.steps[index].state != StepState.error) {
                    return this._buildCircle(index, false);
                }
                else {
                    return this._buildTriangle(index, false);
                }
            }
        }

        Widget _buildVerticalControls() {
            if (this.widget.controlsBuilder != null) {
                return this.widget.controlsBuilder(
                    this.context,
                    onStepContinue: this.widget.onStepContinue,
                    onStepCancel: this.widget.onStepCancel
                );
            }

            Color cancelColor = null;

            switch (Theme.of(this.context).brightness) {
                case Brightness.light:
                    cancelColor = Colors.black54;
                    break;
                case Brightness.dark:
                    cancelColor = Colors.white70;
                    break;
            }

            D.assert(cancelColor != null);

            ThemeData themeData = Theme.of(this.context);
            MaterialLocalizations localizations = MaterialLocalizations.of(this.context);

            return new Container(
                margin: EdgeInsets.only(top: 16.0f),
                child: new ConstrainedBox(
                    constraints: BoxConstraints.tightFor(height: 48.0f),
                    child: new Row(
                        children: new List<Widget> {
                            new FlatButton(
                                onPressed: this.widget.onStepContinue,
                                color: this._isDark() ? themeData.backgroundColor : themeData.primaryColor,
                                textColor: Colors.white,
                                textTheme: ButtonTextTheme.normal,
                                child: new Text(localizations.continueButtonLabel)
                            ),
                            new Container(
                                margin: EdgeInsets.only(8.0f),
                                child: new FlatButton(
                                    onPressed: this.widget.onStepCancel,
                                    textColor: cancelColor,
                                    textTheme: ButtonTextTheme.normal,
                                    child: new Text(localizations.cancelButtonLabel)
                                )
                            )
                        }
                    )
                )
            );
        }

        TextStyle _titleStyle(int index) {
            ThemeData themeData = Theme.of(this.context);
            TextTheme textTheme = themeData.textTheme;

            switch (this.widget.steps[index].state) {
                case StepState.indexed:
                case StepState.editing:
                case StepState.complete:
                    return textTheme.body2;
                case StepState.disabled:
                    return textTheme.body2.copyWith(
                        color: this._isDark() ? _kDisabledDark : _kDisabledLight
                    );
                case StepState.error:
                    return textTheme.body2.copyWith(
                        color: this._isDark() ? _kErrorDark : _kErrorLight
                    );
            }

            return null;
        }

        TextStyle _subTitleStyle(int index) {
            ThemeData themeData = Theme.of(this.context);
            TextTheme textTheme = themeData.textTheme;

            switch (this.widget.steps[index].state) {
                case StepState.indexed:
                case StepState.editing:
                case StepState.complete:
                    return textTheme.caption;
                case StepState.disabled:
                    return textTheme.caption.copyWith(
                        color: this._isDark() ? _kDisabledDark : _kDisabledLight
                    );
                case StepState.error:
                    return textTheme.caption.copyWith(
                        color: this._isDark() ? _kErrorDark : _kErrorLight
                    );
            }

            return null;
        }

        Widget _buildheaderText(int index) {
            List<Widget> children = new List<Widget> {
                new AnimatedDefaultTextStyle(
                    style: this._titleStyle(index),
                    duration: ThemeUtils.kThemeAnimationDuration,
                    curve: Curves.fastOutSlowIn,
                    child: this.widget.steps[index].title
                )
            };

            if (this.widget.steps[index].subtitle != null) {
                children.Add(
                    new Container(
                        margin: EdgeInsets.only(top: 2.0f),
                        child: new AnimatedDefaultTextStyle(
                            style: this._subTitleStyle(index),
                            duration: ThemeUtils.kThemeAnimationDuration,
                            curve: Curves.fastOutSlowIn,
                            child: this.widget.steps[index].subtitle
                        )
                    )
                );
            }

            return new Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: children
            );
        }

        Widget _buildVerticalHeader(int index) {
            return new Container(
                margin: EdgeInsets.symmetric(horizontal: 24.0f),
                child: new Row(
                    children: new List<Widget> {
                        new Column(
                            children: new List<Widget> {
                                this._buildLine(!this._isFirst(index)),
                                this._buildIcon(index),
                                this._buildLine(!this._isLast(index))
                            }
                        ),
                        new Container(
                            margin: EdgeInsets.only(12.0f),
                            child: this._buildheaderText(index)
                        )
                    }
                )
            );
        }

        Widget _buildVerticalBody(int index) {
            return new Stack(
                children: new List<Widget> {
                    new Positioned(
                        left: 24.0f,
                        top: 0.0f,
                        bottom: 0.0f,
                        child: new SizedBox(
                            width: 24.0f,
                            child: new Center(
                                child: new SizedBox(
                                    width: this._isLast(index) ? 0.0f : 1.0f,
                                    child: new Container(
                                        color: Colors.grey.shade400)
                                )
                            )
                        )
                    ),
                    new AnimatedCrossFade(
                        firstChild: new Container(height: 0.0f),
                        secondChild: new Container(
                            margin: EdgeInsets.only(
                                left: 60.0f,
                                right: 24.0f,
                                bottom: 24.0f
                            ),
                            child: new Column(
                                children: new List<Widget> {
                                    this.widget.steps[index].content,
                                    this._buildVerticalControls()
                                }
                            )
                        ),
                        firstCurve: new Interval(0.0f, 0.6f, Curves.fastOutSlowIn),
                        secondCurve: new Interval(0.4f, 1.0f, Curves.fastOutSlowIn),
                        sizeCurve: Curves.fastOutSlowIn,
                        crossFadeState: this._isCurrent(index) ? CrossFadeState.showSecond : CrossFadeState.showFirst,
                        duration: ThemeUtils.kThemeAnimationDuration
                    )
                }
            );
        }

        Widget _buildVertical() {
            List<Widget> children = new List<Widget>();

            for (int i = 0; i < this.widget.steps.Count; i++) {
                int _i = i;
                children.Add(
                    new Column(
                        key: this._keys[_i],
                        children: new List<Widget> {
                            new InkWell(
                                onTap: this.widget.steps[_i].state != StepState.disabled
                                    ? () => {
                                        Scrollable.ensureVisible(
                                            this._keys[_i].currentContext,
                                            curve: Curves.fastOutSlowIn,
                                            duration: ThemeUtils.kThemeAnimationDuration
                                        );

                                        if (this.widget.onStepTapped != null) {
                                            this.widget.onStepTapped(_i);
                                        }
                                    }
                                    : (GestureTapCallback) null,
                                child: this._buildVerticalHeader(_i)
                            ),
                            this._buildVerticalBody(_i)
                        }
                    )
                );
            }

            return new ListView(
                shrinkWrap: true,
                children: children
            );
        }

        Widget _buildHorizontal() {
            List<Widget> children = new List<Widget>();

            for (int i = 0; i < this.widget.steps.Count; i++) {
                int _i = i;
                children.Add(
                    new InkResponse(
                        onTap: this.widget.steps[_i].state != StepState.disabled
                            ? new GestureTapCallback(
                                () => {
                                    if (this.widget.onStepTapped != null) {
                                        this.widget.onStepTapped(_i);
                                    }
                                })
                            : null,
                        child: new Row(
                            children: new List<Widget> {
                                new Container(
                                    height: 72.0f,
                                    child: new Center(child: this._buildIcon(_i))
                                ),
                                new Container(
                                    margin: EdgeInsets.only(left: 12.0f),
                                    child: this._buildheaderText(_i)
                                )
                            }
                        )
                    )
                );

                if (!this._isLast(_i)) {
                    children.Add(
                        new Expanded(
                            child: new Container(
                                margin: EdgeInsets.symmetric(horizontal: 8.0f),
                                height: 1.0f,
                                color: Colors.grey.shade400
                            )
                        )
                    );
                }
            }

            return new Column(
                children: new List<Widget> {
                    new Material(
                        elevation: 2.0f,
                        child: new Container(
                            margin: EdgeInsets.symmetric(horizontal: 24.0f),
                            child: new Row(children: children)
                        )
                    ),
                    new Expanded(
                        child: new ListView(
                            padding: EdgeInsets.all(24.0f),
                            children: new List<Widget> {
                                new AnimatedSize(
                                    curve: Curves.fastOutSlowIn,
                                    duration: ThemeUtils.kThemeAnimationDuration,
                                    vsync: this,
                                    child: this.widget.steps[this.widget.currentStep].content
                                ),
                                this._buildVerticalControls()
                            }
                        )
                    )
                }
            );
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            D.assert(() => {
                if (context.ancestorWidgetOfExactType(typeof(Stepper)) != null) {
                    throw new UIWidgetsError(
                        "Steppers must not be nested. The material specification advises that one should avoid embedding steppers within steppers. "
                    );
                }

                return true;
            });

            switch (this.widget.type) {
                case StepperType.vertical:
                    return this._buildVertical();
                case StepperType.horizontal:
                    return this._buildHorizontal();
            }

            return null;
        }
    }


    class _TrianglePainter : AbstractCustomPainter {
        public _TrianglePainter(
            Color color) {
            this.color = color;
        }

        public readonly Color color;

        public override bool? hitTest(Offset position) {
            return true;
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            return ((_TrianglePainter) oldPainter).color != this.color;
        }

        public override void paint(Canvas canvas, Size size) {
            float baseline = size.width;
            float halfBase = size.width / 2.0f;
            float height = size.height;
            List<Offset> points = new List<Offset> {
                new Offset(0.0f, height),
                new Offset(baseline, height),
                new Offset(halfBase, 0.0f)
            };

            Path newPath = new Path();
            newPath.addPolygon(points, true);
            Paint newPaint = new Paint();
            newPaint.color = this.color;
            canvas.drawPath(newPath, newPaint);
        }
    }
}