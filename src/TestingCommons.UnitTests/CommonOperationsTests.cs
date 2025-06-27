using TestingCommons.Core.Utils;

namespace TestingCommons.UnitTests
{
    public class CommonOperationsTests
    {
        [Fact]
        public void IbanWasMasked()
        {
            var iban = "DE89370400440532013000";
            var ibanMasked = CommonOperations.GetMaskedIban(iban);
            Assert.Equal("******************3000", ibanMasked);
        }
    }
}