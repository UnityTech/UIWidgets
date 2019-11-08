using System;
using System.Collections.Generic;
using com.unity.uiwidgets.Runtime.rendering;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

/*
 * Differences between Dart & C#
 * Duration    =>  TimeSpan
 * -1 % 4 = 3  =>  -1 % 4 = -1
 * [Dart] [DateTime.weekday] provides a 1-based index (Start with Monday)
 * [C#] [DateTime.DayOfWeek] provides a 0-based index (Start with Sunday)
 * @IIzzaya
 */
namespace Unity.UIWidgets.material {
    public class DatePickerUtils {
        public const float _kDatePickerHeaderPortraitHeight = 100.0f;
        public const float _kDatePickerHeaderLandscapeWidth = 168.0f;
        public static readonly TimeSpan _kMonthScrollDuration = new TimeSpan(0, 0, 0, 0, 200);
        public const float _kDayPickerRowHeight = 42.0f;
        public const int _kMaxDayPickerRowCount = 6;

        public const float _kMaxDayPickerHeight = _kDayPickerRowHeight * (_kMaxDayPickerRowCount + 2);
        public const float _kMonthPickerPortraitWidth = 330.0f;
        public const float _kMonthPickerLandscapeWidth = 344.0f;
        public const float _kDialogActionBarHeight = 52.0f;
        public const float _kDatePickerLandscapeHeight = _kMaxDayPickerHeight + _kDialogActionBarHeight;

        internal static readonly _DayPickerGridDelegate _kDayPickerGridDelegate = new _DayPickerGridDelegate();

        public static IPromise<object> showDatePicker(
            BuildContext context,
            DateTime initialDate,
            DateTime firstDate,
            DateTime lastDate,
            SelectableDayPredicate selectableDayPredicate = null,
            DatePickerMode initialDatePickerMode = DatePickerMode.day,
            Locale locale = null,
            TransitionBuilder builder = null
        ) {
            D.assert(initialDate >= firstDate, () => "initialDate must be on or after firstDate");
            D.assert(initialDate <= lastDate, () => "initialDate must be on or before lastDate");
            D.assert(firstDate <= lastDate, () => "lastDate must be on or after firstDate");
            D.assert(
                selectableDayPredicate == null || selectableDayPredicate(initialDate),
                () => "Provided initialDate must satisfy provided selectableDayPredicate"
            );
            D.assert(context != null);
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            Widget child = new _DatePickerDialog(
                initialDate: initialDate,
                firstDate: firstDate,
                lastDate: lastDate,
                selectableDayPredicate: selectableDayPredicate,
                initialDatePickerMode: initialDatePickerMode
            );

            if (locale != null) {
                child = Localizations.overrides(
                    context: context,
                    locale: locale,
                    child: child
                );
            }

            return DialogUtils.showDialog(
                context: context,
                builder: (BuildContext _context) => { return builder == null ? child : builder(_context, child); }
            );
        }
    }

    public enum DatePickerMode {
        day,
        year
    }

    class _DatePickerHeader : StatelessWidget {
        public _DatePickerHeader(
            DateTime selectedDate,
            DatePickerMode mode,
            ValueChanged<DatePickerMode> onModeChanged,
            Orientation orientation,
            Key key = null
        ) : base(key: key) {
            this.selectedDate = selectedDate;
            this.mode = mode;
            this.onModeChanged = onModeChanged;
            this.orientation = orientation;
        }

        public readonly DateTime selectedDate;
        public readonly DatePickerMode mode;
        public readonly ValueChanged<DatePickerMode> onModeChanged;
        public readonly Orientation orientation;

        void _handleChangeMode(DatePickerMode value) {
            if (value != this.mode) {
                this.onModeChanged(value);
            }
        }

        public override Widget build(BuildContext context) {
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            ThemeData themeData = Theme.of(context);
            TextTheme headerTextTheme = themeData.primaryTextTheme;
            Color dayColor = null;
            Color yearColor = null;
            switch (themeData.primaryColorBrightness) {
                case Brightness.light:
                    dayColor = this.mode == DatePickerMode.day ? Colors.black87 : Colors.black54;
                    yearColor = this.mode == DatePickerMode.year ? Colors.black87 : Colors.black54;
                    break;
                case Brightness.dark:
                    dayColor = this.mode == DatePickerMode.day ? Colors.white : Colors.white70;
                    yearColor = this.mode == DatePickerMode.year ? Colors.white : Colors.white70;
                    break;
            }

            TextStyle dayStyle = headerTextTheme.display1.copyWith(color: dayColor, height: 1.4f);
            TextStyle yearStyle = headerTextTheme.subhead.copyWith(color: yearColor, height: 1.4f);
            Color backgroundColor = null;
            switch (themeData.brightness) {
                case Brightness.light:
                    backgroundColor = themeData.primaryColor;
                    break;
                case Brightness.dark:
                    backgroundColor = themeData.backgroundColor;
                    break;
            }

            float width = 0f;
            float height = 0f;
            EdgeInsets padding = null;
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.center;
            switch (this.orientation) {
                case Orientation.portrait:
                    height = DatePickerUtils._kDatePickerHeaderPortraitHeight;
                    padding = EdgeInsets.symmetric(horizontal: 16.0f);
                    mainAxisAlignment = MainAxisAlignment.center;
                    break;
                case Orientation.landscape:
                    width = DatePickerUtils._kDatePickerHeaderLandscapeWidth;
                    padding = EdgeInsets.all(8.0f);
                    mainAxisAlignment = MainAxisAlignment.start;
                    break;
            }

            Widget yearButton = new IgnorePointer(
                ignoring: this.mode != DatePickerMode.day,
                child: new _DateHeaderButton(
                    color: backgroundColor,
                    onTap: Feedback.wrapForTap(() => this._handleChangeMode(DatePickerMode.year), context),
                    child: new Text(localizations.formatYear(this.selectedDate), style: yearStyle)
                )
            );
            Widget dayButton = new IgnorePointer(
                ignoring: this.mode == DatePickerMode.day,
                child: new _DateHeaderButton(
                    color: backgroundColor,
                    onTap: Feedback.wrapForTap(() => this._handleChangeMode(DatePickerMode.day), context),
                    child: new Text(localizations.formatMediumDate(this.selectedDate), style: dayStyle)
                )
            );
            return new Container(
                width: width,
                height: height,
                padding: padding,
                color: backgroundColor,
                child: new Column(
                    mainAxisAlignment: mainAxisAlignment,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {yearButton, dayButton}
                )
            );
        }
    }

    class _DateHeaderButton : StatelessWidget {
        public _DateHeaderButton(
            GestureTapCallback onTap,
            Color color,
            Widget child,
            Key key = null
        ) : base(key: key) {
            this.onTap = onTap;
            this.color = color;
            this.child = child;
        }

        public readonly GestureTapCallback onTap;
        public readonly Color color;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            return new Material(
                type: MaterialType.button,
                color: this.color,
                child: new InkWell(
                    borderRadius: MaterialConstantsUtils.kMaterialEdges[MaterialType.button],
                    highlightColor: theme.highlightColor,
                    splashColor: theme.splashColor,
                    onTap: this.onTap,
                    child: new Container(
                        padding: EdgeInsets.symmetric(horizontal: 8.0f),
                        child: this.child
                    )
                )
            );
        }
    }

    class _DayPickerGridDelegate : SliverGridDelegate {
        public _DayPickerGridDelegate() { }

        public override SliverGridLayout getLayout(SliverConstraints constraints) {
            const int columnCount = 7; // DateTime.daysPerWeek = 7
            float tileWidth = constraints.crossAxisExtent / columnCount;
            float tileHeight = Mathf.Min(
                DatePickerUtils._kDayPickerRowHeight,
                constraints.viewportMainAxisExtent / (DatePickerUtils._kMaxDayPickerRowCount + 1)
            );
            return new SliverGridRegularTileLayout(
                crossAxisCount: columnCount,
                mainAxisStride: tileHeight,
                crossAxisStride: tileWidth,
                childMainAxisExtent: tileHeight,
                childCrossAxisExtent: tileWidth,
                reverseCrossAxis: AxisUtils.axisDirectionIsReversed(constraints.crossAxisDirection)
            );
        }

        public override bool shouldRelayout(SliverGridDelegate oldDelegate) {
            return false;
        }
    }

    public class DayPicker : StatelessWidget {
        public DayPicker(
            DateTime selectedDate,
            DateTime currentDate,
            ValueChanged<DateTime> onChanged,
            DateTime firstDate,
            DateTime lastDate,
            DateTime displayedMonth,
            SelectableDayPredicate selectableDayPredicate = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            Key key = null
        ) : base(key: key) {
            D.assert(onChanged != null);
            D.assert(firstDate <= lastDate);
            D.assert(selectedDate >= firstDate);
            this.selectedDate = selectedDate;
            this.currentDate = currentDate;
            this.onChanged = onChanged;
            this.firstDate = firstDate;
            this.lastDate = lastDate;
            this.displayedMonth = displayedMonth;
            this.selectableDayPredicate = selectableDayPredicate;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly DateTime selectedDate;
        public readonly DateTime currentDate;
        public readonly ValueChanged<DateTime> onChanged;
        public readonly DateTime firstDate;
        public readonly DateTime lastDate;
        public readonly DateTime displayedMonth;
        public readonly SelectableDayPredicate selectableDayPredicate;
        public readonly DragStartBehavior dragStartBehavior;

        List<Widget> _getDayHeaders(TextStyle headerStyle, MaterialLocalizations localizations) {
            List<Widget> result = new List<Widget>();
            for (int i = localizations.firstDayOfWeekIndex; true; i = (i + 1) % 7) {
                string weekday = localizations.narrowWeekdays[i];
                result.Add(new Center(child: new Text(weekday, style: headerStyle)));
                if (i == (localizations.firstDayOfWeekIndex + 6) % 7) {
                    break;
                }
            }

            return result;
        }

        static readonly List<int> _daysInMonth = new List<int> {31, -1, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

        static int getDaysInMonth(int year, int month) {
            if (month == 2) {
                bool isLeapYear = (year % 4 == 0) && (year % 100 != 0) || (year % 400 == 0);
                if (isLeapYear) {
                    return 29;
                }

                return 28;
            }

            return _daysInMonth[month - 1];
        }

        int _computeFirstDayOffset(int year, int month, MaterialLocalizations localizations) {
            int weekdayFromMonday = new DateTime(year, month, 1).DayOfWeek.GetHashCode();
            if (weekdayFromMonday == 0) {
                weekdayFromMonday = 7;
            }

            weekdayFromMonday--;

            int firstDayOfWeekFromSunday = localizations.firstDayOfWeekIndex;
            int firstDayOfWeekFromMonday = (firstDayOfWeekFromSunday - 1) % 7;
            return (weekdayFromMonday - firstDayOfWeekFromMonday) % 7;
        }

        public override Widget build(BuildContext context) {
            ThemeData themeData = Theme.of(context);
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            int year = this.displayedMonth.Year;
            int month = this.displayedMonth.Month;
            int daysInMonth = getDaysInMonth(year, month);
            int firstDayOffset = this._computeFirstDayOffset(year, month, localizations);
            List<Widget> labels = new List<Widget>();
            labels.AddRange(this._getDayHeaders(themeData.textTheme.caption, localizations));
            for (int i = 0; true; i += 1) {
                int day = i - firstDayOffset + 1;
                if (day > daysInMonth) {
                    break;
                }

                if (day < 1) {
                    labels.Add(new Container());
                }
                else {
                    DateTime dayToBuild = new DateTime(year, month, day);
                    bool disabled = dayToBuild > this.lastDate
                                    || dayToBuild < this.firstDate
                                    || (this.selectableDayPredicate != null &&
                                        !this.selectableDayPredicate(dayToBuild));
                    BoxDecoration decoration = null;
                    TextStyle itemStyle = themeData.textTheme.body1;
                    bool isSelectedDay = this.selectedDate.Year == year && this.selectedDate.Month == month &&
                                         this.selectedDate.Day == day;
                    if (isSelectedDay) {
                        itemStyle = themeData.accentTextTheme.body2;
                        decoration = new BoxDecoration(
                            color: themeData.accentColor,
                            shape: BoxShape.circle
                        );
                    }
                    else if (disabled) {
                        itemStyle = themeData.textTheme.body1.copyWith(color: themeData.disabledColor);
                    }
                    else if (this.currentDate.Year == year && this.currentDate.Month == month &&
                             this.currentDate.Day == day) {
                        itemStyle = themeData.textTheme.body2.copyWith(color: themeData.accentColor);
                    }

                    Widget dayWidget = new Container(
                        decoration: decoration,
                        child: new Center(
                            child: new Text(localizations.formatDecimal(day), style: itemStyle)
                        )
                    );
                    if (!disabled) {
                        dayWidget = new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onTap: () => { this.onChanged(dayToBuild); },
                            child: dayWidget,
                            dragStartBehavior: this.dragStartBehavior
                        );
                    }

                    labels.Add(dayWidget);
                }
            }

            return new Padding(
                padding: EdgeInsets.symmetric(horizontal: 8.0f),
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            height: DatePickerUtils._kDayPickerRowHeight,
                            child: new Center(
                                child: new Text(
                                    localizations.formatMonthYear(this.displayedMonth),
                                    style: themeData.textTheme.subhead
                                )
                            )
                        ),
                        new Flexible(
                            child: GridView.custom(
                                gridDelegate: DatePickerUtils._kDayPickerGridDelegate,
                                childrenDelegate: new SliverChildListDelegate(labels, addRepaintBoundaries: false)
                            )
                        )
                    }
                )
            );
        }
    }

    class MonthPicker : StatefulWidget {
        public MonthPicker(
            DateTime selectedDate,
            ValueChanged<DateTime> onChanged,
            DateTime firstDate,
            DateTime lastDate,
            SelectableDayPredicate selectableDayPredicate,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            Key key = null
        ) : base(key: key) {
            D.assert(selectedDate != null);
            D.assert(onChanged != null);
            D.assert(firstDate <= lastDate);
            D.assert(selectedDate >= firstDate);
            this.selectedDate = selectedDate;
            this.onChanged = onChanged;
            this.firstDate = firstDate;
            this.lastDate = lastDate;
            this.selectableDayPredicate = selectableDayPredicate;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly DateTime selectedDate;
        public readonly ValueChanged<DateTime> onChanged;
        public readonly DateTime firstDate;
        public readonly DateTime lastDate;
        public readonly SelectableDayPredicate selectableDayPredicate;
        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _MonthPickerState();
        }
    }

    class _MonthPickerState : SingleTickerProviderStateMixin<MonthPicker> {
        static Animatable<float> __chevronOpacityTween;

        static Animatable<float> _chevronOpacityTween {
            get {
                if (__chevronOpacityTween == null) {
                    __chevronOpacityTween = new FloatTween(begin: 1.0f, end: 0.0f) { };
                    __chevronOpacityTween.chain(new CurveTween(curve: Curves.easeInOut));
                }

                return __chevronOpacityTween;
            }
        }

        public override void initState() {
            base.initState();
            int monthPage = _monthDelta(this.widget.firstDate, this.widget.selectedDate);
            this._dayPickerController = new PageController(initialPage: monthPage);
            this._handleMonthPageChanged(monthPage);
            this._updateCurrentDate();
            this._chevronOpacityController = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 250),
                vsync: this
            );
            this._chevronOpacityAnimation = this._chevronOpacityController.drive(_chevronOpacityTween);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (this.widget.selectedDate != ((MonthPicker) oldWidget).selectedDate) {
                int monthPage = _monthDelta(this.widget.firstDate, this.widget.selectedDate);
                this._dayPickerController = new PageController(initialPage: monthPage);
                this._handleMonthPageChanged(monthPage);
            }
        }

        MaterialLocalizations localizations;

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this.localizations = MaterialLocalizations.of(this.context);
        }

        DateTime _todayDate;
        DateTime _currentDisplayedMonthDate;
        Timer _timer;
        PageController _dayPickerController;
        AnimationController _chevronOpacityController;
        Animation<float> _chevronOpacityAnimation;

        void _updateCurrentDate() {
            this._todayDate = DateTime.Now;
            DateTime tomorrow = this._todayDate.AddDays(1);
            TimeSpan timeUntilTomorrow = tomorrow.TimeOfDay - this._todayDate.TimeOfDay;
            this._timer?.cancel();
            this._timer = Window.instance.run(timeUntilTomorrow,
                () => { this.setState(() => { this._updateCurrentDate(); }); });
        }

        static int _monthDelta(DateTime startDate, DateTime endDate) {
            return (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
        }

        DateTime _addMonthsToMonthDate(DateTime monthDate, int monthsToAdd) {
            return monthDate.AddMonths(monthsToAdd);
        }

        Widget _buildItems(BuildContext context, int index) {
            DateTime month = this._addMonthsToMonthDate(this.widget.firstDate, index);
            return new DayPicker(
                key: new ValueKey<DateTime>(month),
                selectedDate: this.widget.selectedDate,
                currentDate: this._todayDate,
                onChanged: this.widget.onChanged,
                firstDate: this.widget.firstDate,
                lastDate: this.widget.lastDate,
                displayedMonth: month,
                selectableDayPredicate: this.widget.selectableDayPredicate,
                dragStartBehavior: this.widget.dragStartBehavior
            );
        }

        void _handleNextMonth() {
            if (!this._isDisplayingLastMonth) {
                this._dayPickerController.nextPage(duration: DatePickerUtils._kMonthScrollDuration, curve: Curves.ease);
            }
        }

        void _handlePreviousMonth() {
            if (!this._isDisplayingFirstMonth) {
                this._dayPickerController.previousPage(duration: DatePickerUtils._kMonthScrollDuration,
                    curve: Curves.ease);
            }
        }

        bool _isDisplayingFirstMonth {
            get {
                return this._currentDisplayedMonthDate <=
                       new DateTime(this.widget.firstDate.Year, this.widget.firstDate.Month, 1);
            }
        }

        bool _isDisplayingLastMonth {
            get {
                return this._currentDisplayedMonthDate >=
                       new DateTime(this.widget.lastDate.Year, this.widget.lastDate.Month, 1);
            }
        }

        DateTime _previousMonthDate;
        DateTime _nextMonthDate;

        void _handleMonthPageChanged(int monthPage) {
            this.setState(() => {
                this._previousMonthDate = this._addMonthsToMonthDate(this.widget.firstDate, monthPage - 1);
                this._currentDisplayedMonthDate = this._addMonthsToMonthDate(this.widget.firstDate, monthPage);
                this._nextMonthDate = this._addMonthsToMonthDate(this.widget.firstDate, monthPage + 1);
            });
        }

        public override Widget build(BuildContext context) {
            return new SizedBox(
                width: DatePickerUtils._kMonthPickerPortraitWidth,
                height: DatePickerUtils._kMaxDayPickerHeight,
                child: new Stack(
                    children: new List<Widget> {
                        new NotificationListener<ScrollStartNotification>(
                            onNotification: (_) => {
                                this._chevronOpacityController.forward();
                                return false;
                            },
                            child: new NotificationListener<ScrollEndNotification>(
                                onNotification: (_) => {
                                    this._chevronOpacityController.reverse();
                                    return false;
                                },
                                child: PageView.builder(
                                    dragStartBehavior: this.widget.dragStartBehavior,
                                    key: new ValueKey<DateTime>(this.widget.selectedDate),
                                    controller: this._dayPickerController,
                                    scrollDirection: Axis.horizontal,
                                    itemCount: _monthDelta(this.widget.firstDate, this.widget.lastDate) + 1,
                                    itemBuilder: this._buildItems,
                                    onPageChanged: this._handleMonthPageChanged
                                )
                            )
                        ),
                        new Positioned(
                            top: 0.0f,
                            left: 8.0f,
                            child: new FadeTransition(
                                opacity: this._chevronOpacityAnimation,
                                child: new IconButton(
                                    icon: new Icon(Icons.chevron_left),
                                    tooltip: this._isDisplayingFirstMonth
                                        ? null
                                        : $"{this.localizations.previousMonthTooltip} {this.localizations.formatMonthYear(this._previousMonthDate)}",
                                    onPressed: this._isDisplayingFirstMonth
                                        ? (VoidCallback) null
                                        : this._handlePreviousMonth
                                )
                            )
                        ),
                        new Positioned(
                            top: 0.0f,
                            right: 8.0f,
                            child: new FadeTransition(
                                opacity: this._chevronOpacityAnimation,
                                child: new IconButton(
                                    icon: new Icon(Icons.chevron_right),
                                    tooltip: this._isDisplayingLastMonth
                                        ? null
                                        : $"{this.localizations.nextMonthTooltip} {this.localizations.formatMonthYear(this._nextMonthDate)}",
                                    onPressed: this._isDisplayingLastMonth
                                        ? (VoidCallback) null
                                        : this._handleNextMonth
                                )
                            )
                        )
                    }
                )
            );
        }

        public override void dispose() {
            this._timer?.cancel();
            this._dayPickerController?.dispose();
            base.dispose();
        }
    }

    class _MonthPickerSortKey : Diagnosticable {
        public _MonthPickerSortKey(float order) { }
        public static readonly _MonthPickerSortKey previousMonth = new _MonthPickerSortKey(1.0f);
        public static readonly _MonthPickerSortKey nextMonth = new _MonthPickerSortKey(2.0f);
        public static readonly _MonthPickerSortKey calendar = new _MonthPickerSortKey(3.0f);
    }

    public class YearPicker : StatefulWidget {
        public YearPicker(
            DateTime selectedDate,
            ValueChanged<DateTime> onChanged,
            DateTime firstDate,
            DateTime lastDate,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            Key key = null
        ) : base(key: key) {
            D.assert(selectedDate != null);
            D.assert(onChanged != null);
            D.assert(firstDate <= lastDate);
            this.selectedDate = selectedDate;
            this.onChanged = onChanged;
            this.firstDate = firstDate;
            this.lastDate = lastDate;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly DateTime selectedDate;
        public readonly ValueChanged<DateTime> onChanged;
        public readonly DateTime firstDate;
        public readonly DateTime lastDate;
        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _YearPickerState();
        }
    }

    class _YearPickerState : State<YearPicker> {
        const float _itemExtent = 50.0f;
        ScrollController scrollController;

        public override void initState() {
            base.initState();
            this.scrollController = new ScrollController(
                initialScrollOffset: (this.widget.selectedDate.Year - this.widget.firstDate.Year) * _itemExtent
            );
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            ThemeData themeData = Theme.of(context);
            TextStyle style = themeData.textTheme.body1;
            return ListView.builder(
                dragStartBehavior: this.widget.dragStartBehavior,
                controller: this.scrollController,
                itemExtent: _itemExtent,
                itemCount: this.widget.lastDate.Year - this.widget.firstDate.Year + 1,
                itemBuilder: (BuildContext _context, int index) => {
                    int year = this.widget.firstDate.Year + index;
                    bool isSelected = year == this.widget.selectedDate.Year;
                    TextStyle itemStyle = isSelected
                        ? themeData.textTheme.headline.copyWith(color: themeData.accentColor)
                        : style;
                    return new InkWell(
                        key: new ValueKey<int>(year),
                        onTap: () => {
                            this.widget.onChanged(new DateTime(year, this.widget.selectedDate.Month,
                                this.widget.selectedDate.Day));
                        },
                        child: new Center(
                            child: new Text(year.ToString(), style: itemStyle)
                        )
                    );
                }
            );
        }
    }

    class _DatePickerDialog : StatefulWidget {
        public _DatePickerDialog(
            DateTime initialDate,
            DateTime firstDate,
            DateTime lastDate,
            SelectableDayPredicate selectableDayPredicate,
            DatePickerMode initialDatePickerMode,
            Key key = null
        ) : base(key: key) {
            this.initialDate = initialDate;
            this.firstDate = firstDate;
            this.lastDate = lastDate;
            this.selectableDayPredicate = selectableDayPredicate;
            this.initialDatePickerMode = initialDatePickerMode;
        }

        public readonly DateTime initialDate;
        public readonly DateTime firstDate;
        public readonly DateTime lastDate;
        public readonly SelectableDayPredicate selectableDayPredicate;
        public readonly DatePickerMode initialDatePickerMode;

        public override State createState() {
            return new _DatePickerDialogState();
        }
    }

    class _DatePickerDialogState : State<_DatePickerDialog> {
        public override void initState() {
            base.initState();
            this._selectedDate = this.widget.initialDate;
            this._mode = this.widget.initialDatePickerMode;
        }

        bool _announcedInitialDate = false;
        public MaterialLocalizations localizations;

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this.localizations = MaterialLocalizations.of(this.context);
            if (!this._announcedInitialDate) {
                this._announcedInitialDate = true;
            }
        }

        DateTime _selectedDate;

        DatePickerMode _mode;
        GlobalKey _pickerKey = GlobalKey.key();

        void _vibrate() {
            switch (Theme.of(this.context).platform) {
                case RuntimePlatform.Android:
                    // case RuntimePlatform.fuchsia:
                    // HapticFeedback.vibrate();
                    break;
            }
        }

        void _handleModeChanged(DatePickerMode mode) {
            this._vibrate();
            this.setState(() => {
                this._mode = mode;
                if (this._mode == DatePickerMode.day) {
                    // SemanticsService.announce(localizations.formatMonthYear(_selectedDate), textDirection);
                }
                else {
                    // SemanticsService.announce(localizations.formatYear(_selectedDate), textDirection);
                }
            });
        }

        void _handleYearChanged(DateTime value) {
            if (value < this.widget.firstDate) {
                value = this.widget.firstDate;
            }
            else if (value > this.widget.lastDate) {
                value = this.widget.lastDate;
            }

            if (value == this._selectedDate) {
                return;
            }

            this._vibrate();
            this.setState(() => {
                this._mode = DatePickerMode.day;
                this._selectedDate = value;
            });
        }

        void _handleDayChanged(DateTime value) {
            this._vibrate();
            this.setState(() => { this._selectedDate = value; });
        }

        void _handleCancel() {
            Navigator.pop(this.context);
        }

        void _handleOk() {
            Navigator.pop(this.context, this._selectedDate);
        }

        Widget _buildPicker() {
            switch (this._mode) {
                case DatePickerMode.day:
                    return new MonthPicker(
                        key: this._pickerKey,
                        selectedDate: this._selectedDate,
                        onChanged: this._handleDayChanged,
                        firstDate: this.widget.firstDate,
                        lastDate: this.widget.lastDate,
                        selectableDayPredicate: this.widget.selectableDayPredicate
                    );
                case DatePickerMode.year:
                    return new YearPicker(
                        key: this._pickerKey,
                        selectedDate: this._selectedDate,
                        onChanged: this._handleYearChanged,
                        firstDate: this.widget.firstDate,
                        lastDate: this.widget.lastDate
                    );
            }

            return null;
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            Widget picker = new Flexible(
                child: new SizedBox(
                    height: DatePickerUtils._kMaxDayPickerHeight,
                    child: this._buildPicker()
                )
            );
            Widget actions = ButtonTheme.bar(
                child: new ButtonBar(
                    children: new List<Widget> {
                        new FlatButton(
                            child: new Text(this.localizations.cancelButtonLabel),
                            onPressed: this._handleCancel
                        ),
                        new FlatButton(
                            child: new Text(this.localizations.okButtonLabel),
                            onPressed: this._handleOk
                        )
                    }
                )
            );
            Dialog dialog = new Dialog(
                child: new OrientationBuilder(
                    builder: (BuildContext _context, Orientation orientation) => {
                        Widget header = new _DatePickerHeader(
                            selectedDate: this._selectedDate,
                            mode: this._mode,
                            onModeChanged: this._handleModeChanged,
                            orientation: orientation
                        );

                        switch (orientation) {
                            case Orientation.portrait:
                                return new SizedBox(
                                    width: DatePickerUtils._kMonthPickerPortraitWidth,
                                    child: new Column(
                                        mainAxisSize: MainAxisSize.min,
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: new List<Widget> {
                                            header,
                                            new Container(
                                                color: theme.dialogBackgroundColor,
                                                child: new Column(
                                                    mainAxisSize: MainAxisSize.min,
                                                    crossAxisAlignment: CrossAxisAlignment.stretch,
                                                    children: new List<Widget> {
                                                        picker,
                                                        actions,
                                                    }
                                                )
                                            )
                                        }
                                    )
                                );
                            case Orientation.landscape:
                                return new SizedBox(
                                    height: DatePickerUtils._kDatePickerLandscapeHeight,
                                    child: new Row(
                                        mainAxisSize: MainAxisSize.min,
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: new List<Widget> {
                                            header,
                                            new Flexible(
                                                child: new Container(
                                                    width: DatePickerUtils._kMonthPickerLandscapeWidth,
                                                    color: theme.dialogBackgroundColor,
                                                    child: new Column(
                                                        mainAxisSize: MainAxisSize.min,
                                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                                        children: new List<Widget> {picker, actions}
                                                    )
                                                )
                                            )
                                        }
                                    )
                                );
                        }

                        return null;
                    }
                )
            );

            return new Theme(
                data: theme.copyWith(
                    dialogBackgroundColor: Colors.transparent
                ),
                child: dialog
            );
        }
    }

    public delegate bool SelectableDayPredicate(DateTime day);
}