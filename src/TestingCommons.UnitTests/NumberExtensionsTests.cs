using TestingCommons.Core.Utils;

namespace TestingCommons.UnitTests
{
    public class NumberExtensionsTests
    {
        [Fact]
        public void IntSetAsNegative()
        {
            var number = 123;
            var negativeNumber = number.GetNegativeFromPositive();
            Assert.Equal(-123, negativeNumber);
        }

        [Fact]
        public void DoubleSetAsNegative()
        {
            double number = 123.34;
            var negativeNumber = number.GetNegativeFromPositive();
            Assert.Equal(-123.34, negativeNumber);
        }

        [Fact]
        public void DecimalSetAsNegative()
        {
            decimal number = new(1323.24);
            var negativeNumber = number.GetNegativeFromPositive();
            Assert.Equal((decimal)-1323.24, negativeNumber);
        }
    }
}
