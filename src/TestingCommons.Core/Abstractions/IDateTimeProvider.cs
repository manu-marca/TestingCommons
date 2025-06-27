namespace TestingCommons.Core.Abstractions;

public interface IDateTimeProvider
{
    public DateTime Now { get; }
    public DateTimeOffset NowOffset { get; }
}
