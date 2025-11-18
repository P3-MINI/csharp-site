public sealed record FailureProbabilities(
    double NotFound,
    double Unauthorized,
    double ServerError
);