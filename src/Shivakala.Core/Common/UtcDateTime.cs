namespace Shivakala.Core.Common;

public static class UtcDateTime
{
    private static readonly TimeZoneInfo AppTimeZone = ResolveAppTimeZone();

    public static DateTime NowInAppTimeZone()
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, AppTimeZone);

    public static DateOnly Today()
        => DateOnly.FromDateTime(NowInAppTimeZone());

    public static DateTime StartOfToday()
    {
        var localMidnight = Today().ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localMidnight, AppTimeZone);
    }

    public static string CurrentMonthKey()
        => NowInAppTimeZone().ToString("yyyy-MM");

    public static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => TimeZoneInfo.ConvertTimeToUtc(value, AppTimeZone)
    };

    private static TimeZoneInfo ResolveAppTimeZone()
    {
        foreach (var timeZoneId in new[] { "Asia/Kolkata", "India Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
