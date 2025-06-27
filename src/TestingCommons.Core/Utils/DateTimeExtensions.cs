using System.Globalization;

namespace TestingCommons.Core.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTime StripMilliseconds(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
        }

        public static DateTime StripTime(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, date.Kind);
        }

        public static DateTime? ParseDateTimeWithFormat(this string dateTimeStr, string format = "yyyy-MM-ss HH:mm:ss")
        {
            if (DateTime.TryParseExact(dateTimeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDateTime))
            {
                return parsedDateTime;
            }
            return null;
        }
        
        public static DateTime ShiftTo1StDayOfNextMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1);
        }

        public static DateTime ShiftTo1StDayOfCurrentMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime ShiftToLastDayOfCurrentMonth(this DateTime date)
        {
            var day = DateTime.DaysInMonth(date.Year, date.Month);
            return new DateTime(date.Year, date.Month, day);
        }

        public static DateTime MakeBirthDateForAdult(this DateTime date, int adultAge)
        {
            var age = DateTime.Now.Year - date.Year;
            return age < adultAge ? date.AddYears(age-adultAge) : date;
        }

        public static DateTime ShiftJobRunDateFromSunday(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(1) : date;
        }

        public static DateTime ShiftJobRunDateFromSaturday(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ? date.AddDays(2) : date;
        }

        public static DateTime ShiftJobRunDateFromWeekend(this DateTime date)
        {
            date = date.ShiftJobRunDateFromSunday();
            date = date.ShiftJobRunDateFromSaturday();
            return date;
        }
    }
}
