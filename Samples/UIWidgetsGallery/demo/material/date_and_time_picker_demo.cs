using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class _InputDropdown : StatelessWidget {
        public _InputDropdown(
            Key key = null,
            string labelText = null,
            string valueText = null,
            TextStyle valueStyle = null,
            VoidCallback onPressed = null,
            Widget child = null
        ) : base(key: key) {
            this.labelText = labelText;
            this.valueText = valueText;
            this.valueStyle = valueStyle;
            this.onPressed = onPressed;
            this.child = child;
        }

        public readonly string labelText;
        public readonly string valueText;
        public readonly TextStyle valueStyle;
        public readonly VoidCallback onPressed;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new InkWell(
                onTap: () => this.onPressed(),
                child: new InputDecorator(
                    decoration: new InputDecoration(
                        labelText: this.labelText
                    ),
                    baseStyle: this.valueStyle,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        mainAxisSize: MainAxisSize.min,
                        children: new List<Widget> {
                            new Text(this.valueText, style: this.valueStyle),
                            new Icon(Icons.arrow_drop_down,
                                color: Theme.of(context).brightness == Brightness.light
                                    ? Colors.grey.shade700
                                    : Colors.white70
                            )
                        }
                    )
                )
            );
        }
    }

    class _DateTimePicker : StatelessWidget {
        public _DateTimePicker(
            DateTime selectedDate,
            Key key = null,
            string labelText = null,
            TimeOfDay selectedTime = null,
            ValueChanged<DateTime> selectDate = null,
            ValueChanged<TimeOfDay> selectTime = null
        ) : base(key: key) {
            this.labelText = labelText;
            this.selectedDate = selectedDate;
            this.selectedTime = selectedTime;
            this.selectDate = selectDate;
            this.selectTime = selectTime;
        }

        public readonly string labelText;
        public readonly DateTime selectedDate;
        public readonly TimeOfDay selectedTime;
        public readonly ValueChanged<DateTime> selectDate;
        public readonly ValueChanged<TimeOfDay> selectTime;

        IPromise _selectDate(BuildContext context) {
            return DatePickerUtils.showDatePicker(
                context: context,
                initialDate: this.selectedDate,
                firstDate: new DateTime(2015, 8, 1),
                lastDate: new DateTime(2101, 1, 1)
            ).Then((date) => {
                if (date == null) {
                    return;
                }

                DateTime picked = (DateTime) date;
                if (picked != null && picked != this.selectedDate) {
                    this.selectDate(picked);
                }
            });
        }

        // Future<void> _selectTime(BuildContext context) async {
        //     final TimeOfDay picked = await showTimePicker(
        //         context: context,
        //         initialTime: selectedTime,
        //     );
        //     if (picked != null && picked != selectedTime)
        //         selectTime(picked);
        // }

        public override Widget build(BuildContext context) {
            TextStyle valueStyle = Theme.of(context).textTheme.title;
            return new Row(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: new List<Widget> {
                    new Expanded(
                        flex: 4,
                        child: new _InputDropdown(
                            labelText: this.labelText,
                            valueText: this.selectedDate.ToString("MM/dd/yyyy"),
                            valueStyle: valueStyle,
                            onPressed: () => { this._selectDate(context); }
                        )
                    ),
                    new SizedBox(width: 12.0f),
                    // new Expanded(
                    //     flex: 3,
                    //     child: new _InputDropdown(
                    //         valueText: this.selectedTime.format(context),
                    //         valueStyle: valueStyle,
                    //         onPressed: () => { this._selectTime(context); }
                    //     )
                    // )
                }
            );
        }
    }

    public class DateAndTimePickerDemo : StatefulWidget {
        public const string routeName = "/material/date-and-time-pickers";

        public override State createState() {
            return new _DateAndTimePickerDemoState();
        }
    }

    class _DateAndTimePickerDemoState : State<DateAndTimePickerDemo> {
        DateTime _fromDate = DateTime.Now;
        TimeOfDay _fromTime = new TimeOfDay(hour: 7, minute: 28);
        DateTime _toDate = DateTime.Now;
        TimeOfDay _toTime = new TimeOfDay(hour: 7, minute: 28);
        readonly List<string> _allActivities = new List<string> {"hiking", "swimming", "boating", "fishing"};
        string _activity = "fishing";

        public override Widget build(BuildContext context) {
            var allActiviesList = new List<DropdownMenuItem<string>>();
            foreach (var item in this._allActivities) {
                allActiviesList.Add(
                    new DropdownMenuItem<string>(
                        value: item,
                        child: new Text(item)
                    )
                );
            }

            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Date and time pickers"),
                    actions: new List<Widget> {new MaterialDemoDocumentationButton(DateAndTimePickerDemo.routeName)}
                ),
                body: new DropdownButtonHideUnderline(
                    child: new SafeArea(
                        top: false,
                        bottom: false,
                        child: new ListView(
                            padding: EdgeInsets.all(16.0f),
                            children: new List<Widget> {
                                new TextField(
                                    enabled: true,
                                    decoration: new InputDecoration(
                                        labelText: "Event name",
                                        border: new OutlineInputBorder()
                                    ),
                                    style: Theme.of(context).textTheme.display1
                                ),
                                new TextField(
                                    decoration: new InputDecoration(
                                        labelText: "Location"
                                    ),
                                    style: Theme.of(context).textTheme.display1.copyWith(fontSize: 20.0f)
                                ),
                                new _DateTimePicker(
                                    labelText: "From",
                                    selectedDate: this._fromDate,
                                    selectedTime: this._fromTime,
                                    selectDate: (DateTime date) => { this.setState(() => { this._fromDate = date; }); },
                                    selectTime: (TimeOfDay time) => { this.setState(() => { this._fromTime = time; }); }
                                ),
                                new _DateTimePicker(
                                    labelText: "To",
                                    selectedDate: this._toDate,
                                    selectedTime: this._toTime,
                                    selectDate: (DateTime date) => { this.setState(() => { this._toDate = date; }); },
                                    selectTime: (TimeOfDay time) => { this.setState(() => { this._toTime = time; }); }
                                ),
                                new SizedBox(height: 8.0f),
                                new InputDecorator(
                                    decoration: new InputDecoration(
                                        labelText: "Activity",
                                        hintText: "Choose an activity",
                                        contentPadding: EdgeInsets.zero
                                    ),
                                    isEmpty: this._activity == null,
                                    child: new DropdownButton<string>(
                                        value: this._activity,
                                        onChanged: (string newValue) => {
                                            this.setState(() => { this._activity = newValue; });
                                        },
                                        items: allActiviesList
                                    )
                                )
                            }
                        )
                    )
                )
            );
        }
    }
}