using System.Diagnostics;
using System.Text.RegularExpressions;
using Reqnroll.Assist;
namespace TestingCommons.Reqnroll;

public partial class RelativeDateValueRetriever : IValueRetriever
{
    public static DateTime TestMoment { get; set; } = DateTime.Now;

    [GeneratedRegex(@"^(\d+) (years?|months?|days?|hours?|minutes?|seconds?) ago$")]
    private static partial Regex RegexSomeTimeAgo();

    [GeneratedRegex(@"^in (\d+) (years?|months?|days?|hours?|minutes?|seconds?)$")]
    private static partial Regex RegexInSomeTime();

    [GeneratedRegex("^null date$")]
    private static partial Regex RegexNullDate();

    [GeneratedRegex("^now$")]
    private static partial Regex RegexNowDate();

    public bool CanRetrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
    {
        return (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))  &&
               (RegexSomeTimeAgo().IsMatch(keyValuePair.Value)
                || RegexInSomeTime().IsMatch(keyValuePair.Value)
                || RegexNullDate().IsMatch(keyValuePair.Value)
                || RegexNowDate().IsMatch(keyValuePair.Value));
    }

    public object Retrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
    {
        Trace.WriteLine($"Retrieving {propertyType} for {targetType} from value \"{keyValuePair.Value}\"");
        var dateTime = GetRelativeDateTime(keyValuePair.Value);
        if (propertyType == typeof(DateTime)) return dateTime ?? DateTime.MinValue;
        return dateTime!;
    }

    public static DateTime? GetRelativeDateTime(string dateTimeString)
    {
        if (RegexNullDate().IsMatch(dateTimeString)) return null;
        if (RegexNowDate().IsMatch(dateTimeString)) return TestMoment;

        int multiplier;
        if (RegexInSomeTime().IsMatch(dateTimeString)) multiplier = 1;
        else if (RegexSomeTimeAgo().IsMatch(dateTimeString)) multiplier = -1;
        else return null;

        var matchCollection = (multiplier > 0)
            ? RegexInSomeTime().Matches(dateTimeString)
            : RegexSomeTimeAgo().Matches(dateTimeString);

        var amount = int.Parse(matchCollection[0].Groups[1].Value);
        var result = matchCollection[0].Groups[2].Value.ToLowerInvariant() switch
        {
            "year" or "years" => TestMoment.AddYears(amount * multiplier),
            "month" or "months" => TestMoment.AddMonths(amount * multiplier),
            "day" or "days" => TestMoment.AddDays(amount * multiplier),
            "hour" or "hours" => TestMoment.AddHours(amount * multiplier),
            "minute" or "minutes" => TestMoment.AddMinutes(amount * multiplier),
            "second" or "seconds" => TestMoment.AddSeconds(amount * multiplier),
            _ => throw new ArgumentException($"Unknown timespan value of {matchCollection[0].Groups[2].Value}!")
        };
        return result;
    }
}
