namespace Chat.Common;

public sealed record MessageDTO
{
    public string Content { get; init; } = string.Empty;
    public string Sender { get; init; } = string.Empty;
    public DateTime Time { get; init; } = DateTime.UtcNow;
}
