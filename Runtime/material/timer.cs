using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {

    public enum DayPeriod {
        am,
        pm,
    }

    public class TimeOfDay : IEquatable<TimeOfDay> {
        public TimeOfDay(int hour, int minute) {
            this.hour = hour;
            this.minute = minute;
        }

        public static TimeOfDay fromDateTime(DateTime time) {
            return new TimeOfDay(
                hour: time.Hour,
                minute: time.Minute
            );
        }

        public static TimeOfDay now() {
            return TimeOfDay.fromDateTime(DateTime.Now);
        }

        public static readonly int hoursPerDay = 24;

        public static readonly int hoursPerPeriod = 12;

        public static readonly int minutesPerHour = 60;

        public TimeOfDay replacing(int? hour = null, int? minute = null) {
            D.assert(hour == null || (hour >= 0 && hour < hoursPerDay));
            D.assert(minute == null || (minute >= 0 && minute < minutesPerHour));

            return new TimeOfDay(hour: hour ?? this.hour, minute: minute ?? this.minute);
        }

        public readonly int hour;

        public readonly int minute;

        public DayPeriod period {
            get { return this.hour < hoursPerPeriod ? DayPeriod.am : DayPeriod.pm; }
        }

        public int hourOfPeriod {
            get { return this.hour - this.periodOffset; }
        }

        public int periodOffset {
            get { return this.period == DayPeriod.am ? 0 : hoursPerPeriod; }
        }

        public string format(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            return localizations.formatTimeOfDay(
                this,
                alwaysUse24HourFormat: MediaQuery.of(context).alwaysUse24HourFormat
            );
        }

        static string _addLeadingZeroIfNeeded(int value) {
            if (value < 10) {
                return "0" + value;
            }
            return value.ToString();
        }

        public bool Equals(TimeOfDay other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return this.hour == other.hour && this.minute == other.minute;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return this.Equals((TimeOfDay) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.hour * 397) ^ this.minute;
            }
        }

        public static bool operator ==(TimeOfDay left, TimeOfDay right) {
            return Equals(left, right);
        }

        public static bool operator !=(TimeOfDay left, TimeOfDay right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            string hourLabel = _addLeadingZeroIfNeeded(this.hour);
            string minuteLabel = _addLeadingZeroIfNeeded(this.minute);

            return $"TimeOfDay({hourLabel}:{minuteLabel})";
        }
    }

    public enum TimeOfDayFormat {
        HH_colon_mm,
        HH_dot_mm,

        frenchCanadian,
        H_colon_mm,
        h_colon_mm_space_a,
        a_space_h_colon_mm,
    }

    public enum HourFormat {
        HH,
        H,
        h,
    }

    public static class HourFormatUtils {
        public static HourFormat hourFormat(TimeOfDayFormat of) {
            switch (of) {
                case TimeOfDayFormat.h_colon_mm_space_a:
                case TimeOfDayFormat.a_space_h_colon_mm:
                    return HourFormat.h;
                case TimeOfDayFormat.H_colon_mm:
                    return HourFormat.H;
                case TimeOfDayFormat.HH_dot_mm:
                case TimeOfDayFormat.HH_colon_mm:
                case TimeOfDayFormat.frenchCanadian:
                    return HourFormat.HH;
            }

            throw new Exception("unknown format: " + of);
        }
    }
}
