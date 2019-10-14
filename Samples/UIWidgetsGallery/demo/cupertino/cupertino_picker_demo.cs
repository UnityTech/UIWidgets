using System;
using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class CupertinoPickerDemoUtils {
        public const float _kPickerSheetHeight = 216.0f;
        public const float _kPickerItemHeight = 32.0f;
    }

    class CupertinoPickerDemo : StatefulWidget {
        public const string routeName = "/cupertino/picker";

        public override State createState() {
            return new _CupertinoPickerDemoState();
        }
    }

    class _CupertinoPickerDemoState : State<CupertinoPickerDemo> {
        int _selectedColorIndex = 0;
        TimeSpan timer = new TimeSpan();

        // Value that is shown in the date picker in date mode.
        DateTime date = DateTime.Now;

        // Value that is shown in the date picker in time mode.
        DateTime time = DateTime.Now;

        // Value that is shown in the date picker in dateAndTime mode.
        DateTime dateTime = DateTime.Now;

        Widget _buildMenu(List<Widget> children) {
            return new Container(
                decoration: new BoxDecoration(
                    color: CupertinoTheme.of(this.context).scaffoldBackgroundColor,
                    border: new Border(
                        top: new BorderSide(color: new Color(0xFFBCBBC1), width: 0.0f),
                        bottom: new BorderSide(color: new Color(0xFFBCBBC1), width: 0.0f)
                    )
                ),
                height: 44.0f,
                child: new Padding(
                    padding: EdgeInsets.symmetric(horizontal: 16.0f),
                    child: new SafeArea(
                        top: false,
                        bottom: false,
                        child: new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: children
                        )
                    )
                )
            );
        }

        Widget _buildBottomPicker(Widget picker) {
            return new Container(
                height: CupertinoPickerDemoUtils._kPickerSheetHeight,
                padding: EdgeInsets.only(top: 6.0f),
                color: CupertinoColors.white,
                child: new DefaultTextStyle(
                    style: new TextStyle(
                        color: CupertinoColors.black,
                        fontSize: 22.0f
                    ),
                    child: new GestureDetector(
                        // Blocks taps from propagating to the modal sheet and popping.
                        onTap: () => { },
                        child: new SafeArea(
                            top: false,
                            child: picker
                        )
                    )
                )
            );
        }

        Widget _buildColorPicker(BuildContext context) {
            FixedExtentScrollController scrollController =
                new FixedExtentScrollController(initialItem: this._selectedColorIndex);

            List<Widget> generateList() {
                var list = new List<Widget>();
                foreach (var item in CupertinoNavigationDemoUtils.coolColorNames) {
                    list.Add(new Center(child:
                        new Text(item)
                    ));
                }

                return list;
            }


            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return this._buildBottomPicker(
                                new CupertinoPicker(
                                    scrollController: scrollController,
                                    itemExtent: CupertinoPickerDemoUtils._kPickerItemHeight,
                                    backgroundColor: CupertinoColors.white,
                                    onSelectedItemChanged: (int index) => {
                                        this.setState(() => this._selectedColorIndex = index);
                                    },
                                    children: generateList()
                                )
                            );
                        }
                    );
                },
                child: this._buildMenu(new List<Widget> {
                        new Text("Favorite Color"),
                        new Text(
                            CupertinoNavigationDemoUtils.coolColorNames[this._selectedColorIndex],
                            style: new TextStyle(
                                color: CupertinoColors.inactiveGray
                            )
                        )
                    }
                )
            );
        }

        Widget _buildCountdownTimerPicker(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return this._buildBottomPicker(
                                new CupertinoTimerPicker(
                                    initialTimerDuration: this.timer,
                                    onTimerDurationChanged: (TimeSpan newTimer) => {
                                        this.setState(() => this.timer = newTimer);
                                    }
                                )
                            );
                        }
                    );
                },
                child: this._buildMenu(new List<Widget> {
                        new Text("Countdown Timer"),
                        new Text(
                            $"{this.timer.Hours}:" +
                            $"{(this.timer.Minutes % 60).ToString("00")}:" +
                            $"{(this.timer.Seconds % 60).ToString("00")}",
                            style: new TextStyle(color: CupertinoColors.inactiveGray)
                        )
                    }
                )
            );
        }

        Widget _buildDatePicker(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return this._buildBottomPicker(
                                new CupertinoDatePicker(
                                    mode: CupertinoDatePickerMode.date,
                                    initialDateTime: this.date,
                                    onDateTimeChanged: (DateTime newDateTime) => {
                                        this.setState(() => this.date = newDateTime);
                                    }
                                )
                            );
                        }
                    );
                },
                child: this._buildMenu(new List<Widget> {
                        new Text("Date"),
                        new Text(
                            this.date.ToString("MMMM dd, yyyy"),
                            style: new TextStyle(color: CupertinoColors.inactiveGray)
                        )
                    }
                )
            );
        }

        Widget _buildTimePicker(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return this._buildBottomPicker(
                                new CupertinoDatePicker(
                                    mode: CupertinoDatePickerMode.time,
                                    initialDateTime: this.time,
                                    onDateTimeChanged: (DateTime newDateTime) => {
                                        this.setState(() => this.time = newDateTime);
                                    }
                                )
                            );
                        }
                    );
                },
                child: this._buildMenu(new List<Widget> {
                        new Text("Time"),
                        new Text(
                            this.time.ToString("h:mm tt"),
                            style: new TextStyle(color: CupertinoColors.inactiveGray)
                        )
                    }
                )
            );
        }

        Widget _buildDateAndTimePicker(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return this._buildBottomPicker(
                                new CupertinoDatePicker(
                                    mode: CupertinoDatePickerMode.dateAndTime,
                                    initialDateTime: this.dateTime,
                                    onDateTimeChanged: (DateTime newDateTime) => {
                                        this.setState(() => this.dateTime = newDateTime);
                                    }
                                )
                            );
                        }
                    );
                },
                child: this._buildMenu(new List<Widget> {
                        new Text("Date and Time"),
                        new Text(
                            this.dateTime.ToString("MMMM dd, yyyy h:mm tt"),
                            style: new TextStyle(color: CupertinoColors.inactiveGray)
                        )
                    }
                )
            );
        }

        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    middle: new Text("Picker"),
                    // We"re specifying a back label here because the previous page is a
                    // Material page. CupertinoPageRoutes could auto-populate these back
                    // labels.
                    previousPageTitle: "Cupertino",
                    trailing: new CupertinoDemoDocumentationButton(CupertinoPickerDemo.routeName)
                ),
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new DecoratedBox(
                        decoration: new BoxDecoration(
                            color: CupertinoTheme.of(context).brightness == Brightness.light
                                ? CupertinoColors.extraLightBackgroundGray
                                : CupertinoColors.darkBackgroundGray
                        ),
                        child: new SafeArea(
                            child: new ListView(
                                children: new List<Widget> {
                                    new Padding(padding: EdgeInsets.only(top: 32.0f)),
                                    this._buildColorPicker(context),
                                    this._buildCountdownTimerPicker(context),
                                    this._buildDatePicker(context),
                                    this._buildTimePicker(context),
                                    this._buildDateAndTimePicker(context)
                                }
                            )
                        )
                    )
                )
            );
        }
    }
}