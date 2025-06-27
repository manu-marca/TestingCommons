namespace TestingCommons.Core.Utils;

public static class NumberExtensions
{
    public static int GetNegativeFromPositive(this int value)
    {
        if (value < 0)
            return value;
        return value * -1;
    }
    public static decimal GetNegativeFromPositive(this decimal value)
    {
        if (value < 0)
            return value;
        return value * -1;
    }
    public static double GetNegativeFromPositive(this double value)
    {
        if (value < 0)
            return value;
        return value * -1;
    }
}
