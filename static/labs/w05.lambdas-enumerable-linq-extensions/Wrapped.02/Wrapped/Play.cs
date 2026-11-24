namespace Wrapped;

/// <summary>A single entry of the listening history.</summary>
/// <param name="Timestamp">When the playback started.</param>
/// <param name="TrackId">Id of the played track.</param>
/// <param name="MsPlayed">How long the track was played (in milliseconds).</param>
public record Play(DateTime Timestamp, int TrackId, int MsPlayed)
{
    public TimeSpan Duration => TimeSpan.FromMilliseconds(MsPlayed);
}
