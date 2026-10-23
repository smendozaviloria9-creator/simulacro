namespace FrameRent.Application.Interfaces;

/// <summary>
/// Provides the current business date in the configured time zone
/// (avoids using UTC "today", which is already tomorrow in Colombia after 7 p.m.).
/// </summary>
public interface IDateTimeProvider
{
    DateOnly Today { get; }
}
