using FrameRent.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FrameRent.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    private readonly TimeZoneInfo _timeZone;

    public DateTimeProvider(IConfiguration configuration)
    {
        var timeZoneId = configuration["App:TimeZone"] ?? "America/Bogota";
        try
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            _timeZone = TimeZoneInfo.Utc;
        }
    }

    public DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone));
}
