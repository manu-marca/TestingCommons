namespace TestingCommons.Core.Utils;

public static class CommonOperations
{
    public static string GetMaskedIban(string iban)
    {
        var maskedIban = "";
        for (var i = 0; i < iban.Length - 4; i++)
        {
            maskedIban += "*";
        }
        maskedIban += iban[^4..];
        return maskedIban;
    }
}
