public sealed record AirlinesConfig
{
    public List<AirlineProvider> Providers { get; init; } = new();
}