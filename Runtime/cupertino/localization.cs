using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    public enum DatePickerDateTimeOrder {
        date_time_dayPeriod,
        date_dayPeriod_time,
        time_dayPeriod_date,
        dayPeriod_time_date
    }

    public enum DatePickerDateOrder {
        dmy,
        mdy,
        ymd,
        ydm
    }

    public abstract class CupertinoLocalizations {
        public abstract string datePickerYear(int yearIndex);

        public abstract string datePickerMonth(int monthIndex);

        public abstract string datePickerDayOfMonth(int dayIndex);

        public abstract string datePickerMediumDate(DateTime date);

        public abstract string datePickerHour(int hour);

        public abstract string datePickerHourSemanticsLabel(int hour);

        public abstract string datePickerMinute(int minute);

        public abstract string datePickerMinuteSemanticsLabel(int minute);

        public abstract DatePickerDateOrder datePickerDateOrder { get; }

        public abstract DatePickerDateTimeOrder datePickerDateTimeOrder { get; }

        public abstract string anteMeridiemAbbreviation { get; }

        public abstract string postMeridiemAbbreviation { get; }

        public abstract string alertDialogLabel { get; }

        public abstract string timerPickerHour(int hour);

        public abstract string timerPickerMinute(int minute);

        public abstract string timerPickerSecond(int second);

        public abstract string timerPickerHourLabel(int hour);

        public abstract string timerPickerMinuteLabel(int minute);

        public abstract string timerPickerSecondLabel(int second);

        public abstract string cutButtonLabel { get; }

        public abstract string copyButtonLabel { get; }

        public abstract string pasteButtonLabel { get; }

        public abstract string selectAllButtonLabel { get; }

        public static CupertinoLocalizations of(BuildContext context) {
            return Localizations.of<CupertinoLocalizations>(context, typeof(CupertinoLocalizations));
        }
    }

    class _CupertinoLocalizationsDelegate : LocalizationsDelegate<CupertinoLocalizations> {
        public _CupertinoLocalizationsDelegate() { }

        public override bool isSupported(Locale locale) {
            return locale.languageCode == "en";
        }

        public override IPromise<object> load(Locale locale) {
            return DefaultCupertinoLocalizations.load(locale);
        }

        public override bool shouldReload(LocalizationsDelegate old) {
            return false;
        }

        public override string ToString() {
            return "DefaultCupertinoLocalizations.delegate(en_US)";
        }
    }

    public class DefaultCupertinoLocalizations : CupertinoLocalizations {
        public DefaultCupertinoLocalizations() { }

        static readonly List<string> _shortWeekdays = new List<string> {
            "Mon",
            "Tue",
            "Wed",
            "Thu",
            "Fri",
            "Sat",
            "Sun"
        };

        static readonly List<string> _shortMonths = new List<string> {
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sep",
            "Oct",
            "Nov",
            "Dec"
        };

        static readonly List<string> _months = new List<string> {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        };

        public override string datePickerYear(int yearIndex) {
            return yearIndex.ToString();
        }

        public override string datePickerMonth(int monthIndex) {
            return _months[monthIndex - 1];
        }

        public override string datePickerDayOfMonth(int dayIndex) {
            return dayIndex.ToString();
        }

        public override string datePickerHour(int hour) {
            return hour.ToString();
        }

        public override string datePickerHourSemanticsLabel(int hour) {
            return hour.ToString() + " o'clock";
        }

        public override string datePickerMinute(int minute) {
            return minute.ToString().PadLeft(2, '0');
        }

        public override string datePickerMinuteSemanticsLabel(int minute) {
            if (minute == 1) {
                return "1 minute";
            }

            return minute.ToString() + " minutes";
        }

        public override string datePickerMediumDate(DateTime date) {
            var day = _shortWeekdays[((int) date.DayOfWeek + 6) % 7];
            var month = _shortMonths[date.Month - 1];
            return $"{day} {month} {date.Day.ToString().PadRight(2)} ";
        }

        public override DatePickerDateOrder datePickerDateOrder {
            get { return DatePickerDateOrder.mdy; }
        }

        public override DatePickerDateTimeOrder datePickerDateTimeOrder {
            get { return DatePickerDateTimeOrder.date_time_dayPeriod; }
        }

        public override string anteMeridiemAbbreviation {
            get { return "AM"; }
        }

        public override string postMeridiemAbbreviation {
            get { return "PM"; }
        }

        public override string alertDialogLabel {
            get { return "Alert"; }
        }

        public override string timerPickerHour(int hour) {
            return hour.ToString();
        }

        public override string timerPickerMinute(int minute) {
            return minute.ToString();
        }

        public override string timerPickerSecond(int second) {
            return second.ToString();
        }

        public override string timerPickerHourLabel(int hour) {
            return hour == 1 ? "hour" : "hours";
        }

        public override string timerPickerMinuteLabel(int minute) {
            return "min";
        }

        public override string timerPickerSecondLabel(int second) {
            return "sec";
        }

        public override string cutButtonLabel {
            get { return "Cut"; }
        }

        public override string copyButtonLabel {
            get { return "Copy"; }
        }

        public override string pasteButtonLabel {
            get { return "Paste"; }
        }

        public override string selectAllButtonLabel {
            get { return "Select All"; }
        }

        public static IPromise<object> load(Locale locale) {
            return Promise<object>.Resolved(new DefaultCupertinoLocalizations());
        }

        public static readonly LocalizationsDelegate<CupertinoLocalizations>
            del = new _CupertinoLocalizationsDelegate();
    }
}