public sealed record AirlineProvider(
    string Id,
    string Name,
    string Endpoint,
    ProviderConfiguration Configuration
);