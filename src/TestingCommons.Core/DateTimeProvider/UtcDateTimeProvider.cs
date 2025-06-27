using TestingCommons.Core.Abstractions;

namespace TestingCommons.Core.DateTimeProvider;

/// <summary>
/// Makes sure that all dates are in UTC
/// </summary>
public class UtcDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;

    public DateTimeOffset NowOffset => DateTimeOffset.UtcNow;
}
