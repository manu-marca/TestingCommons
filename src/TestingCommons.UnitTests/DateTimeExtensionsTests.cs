using TestingCommons.Core.Utils;

namespace TestingCommons.UnitTests
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void DateShiftedTo1StOfNextMonth()
        {
            DateTime date = new(2022, 4, 21);
            var firstOfTheNextMonth = date.ShiftTo1StDayOfNextMonth();
            Assert.Equal(new DateTime(2022, 5, 1), firstOfTheNextMonth);
        }

        [Fact]
        public void DateShiftedTo1StOfNextMonthAndNextYear()
        {
            DateTime date = new(2022, 12, 21);
            var firstOfTheNextMonth = date.ShiftTo1StDayOfNextMonth();
            Assert.Equal(new DateTime(2023, 1, 1), firstOfTheNextMonth);
        }

        [Fact]
        public void DateShiftedTo1StOfCurrentMonth()
        {
            DateTime date = new(1985, 7, 21);
            var firstOfTheCurrentMonth = date.ShiftTo1StDayOfCurrentMonth();
            Assert.Equal(new DateTime(1985, 7, 1), firstOfTheCurrentMonth);
        }

        [Fact]
        public void DateShiftedToLastOfCurrentMonth28DaysInMonth()
        {
            DateTime date = new(1999, 2, 14);
            var lastDayOfTheCurrentMonth = date.ShiftToLastDayOfCurrentMonth();
            Assert.Equal(new DateTime(1999, 2, 28), lastDayOfTheCurrentMonth);
        }

        [Fact]
        public void DateShiftedToLastOfCurrentMonth29DaysInMonth()
        {
            DateTime date = new(2000, 2, 5);
            var lastDayOfTheCurrentMonth = date.ShiftToLastDayOfCurrentMonth();
            Assert.Equal(new DateTime(2000, 2, 29), lastDayOfTheCurrentMonth);
        }

        [Fact]
        public void DateShiftedToLastOfCurrentMonth30DaysInMonth()
        {
            DateTime date = new(2008, 4, 23);
            var lastDayOfTheCurrentMonth = date.ShiftToLastDayOfCurrentMonth();
            Assert.Equal(new DateTime(2008, 4, 30), lastDayOfTheCurrentMonth);
        }

        [Fact]
        public void DateShiftedToLastOfCurrentMonth31DaysInMonth()
        {
            DateTime date = new(2011, 10, 25);
            var lastDayOfTheCurrentMonth = date.ShiftToLastDayOfCurrentMonth();
            Assert.Equal(new DateTime(2011, 10, 31), lastDayOfTheCurrentMonth);
        }

        [Fact]
        public void BirthDateForAdult()
        {
            var now = DateTime.Now;
            DateTime date = now.AddYears(-16);
            var adultBirthDay = date.MakeBirthDateForAdult(21);
            Assert.Equal(now.AddYears(-21), adultBirthDay);
        }

        [Fact]
        public void DateShiftedFromSundayToMonday()
        {
            DateTime date = new(2022, 12, 4);
            var currentDate = date.ShiftJobRunDateFromSunday();
            Assert.Equal(new DateTime(2022, 12, 5), currentDate);
        }

        [Fact]
        public void DateShiftedFromSaturdayToMonday()
        {
            DateTime date = new(2023, 02, 4);
            var currentDate = date.ShiftJobRunDateFromSaturday();
            Assert.Equal(new DateTime(2023, 02, 6), currentDate);
        }

        [Fact]
        public void DateShiftedFromSundayToMondayViaWeekendShifter()
        {
            DateTime date = new(2022, 12, 4);
            var currentDate = date.ShiftJobRunDateFromWeekend();
            Assert.Equal(new DateTime(2022, 12, 5), currentDate);
        }

        [Fact]
        public void DateShiftedFromSaturdayToMondayWeekendShifter()
        {
            DateTime date = new(2023, 02, 4);
            var currentDate = date.ShiftJobRunDateFromWeekend();
            Assert.Equal(new DateTime(2023, 02, 6), currentDate);
        }
    }
}