public sealed record ProviderConfiguration(
    int MinDelayMs,
    int MaxDelayMs,
    FailureProbabilities Probabilities
);