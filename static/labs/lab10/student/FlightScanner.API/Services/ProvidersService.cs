using FlightScanner.Common.Dtos;

namespace FlightScanner.API.Services;

public sealed class ProviderService : IProviderService
{
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(ILogger<ProviderService> logger)
    {
        _logger = logger;
    }

    public async Task<IResult> GetProviderResponseAsync(AirlineProvider provider)
    {
        _logger.LogInformation(
            "Request received for {ProviderName} ({ProviderId}).",
            provider.Name, provider.Id);

        var config = provider.Configuration;
        var failureChance = Random.Shared.NextDouble();

        if (failureChance < config.Probabilities.NotFound)
        {
            _logger.LogWarning(
                "Not Found for {ProviderName}.",
                provider.Name);
            return Results.NotFound(new { Message = "No flights found for this route." });
        }

        failureChance -= config.Probabilities.NotFound;

        if (failureChance < config.Probabilities.Unauthorized)
        {
            _logger.LogWarning(
                "Unauthorized for {ProviderName}.",
                provider.Name);
            return Results.Unauthorized();
        }

        failureChance -= config.Probabilities.Unauthorized;

        if (failureChance < config.Probabilities.ServerError)
        {
            _logger.LogError(
                "Service Unavailable for {ProviderName}.",
                provider.Name);
            return Results.Problem(
                detail: "An internal error occurred.",
                statusCode: 503
            );
        }

        var delayMs = Random.Shared.Next(config.MinDelayMs, config.MaxDelayMs);

        _logger.LogInformation(
            "Delay of {Delay}ms for {ProviderName}.",
            delayMs, provider.Name);

        await Task.Delay(delayMs);

        var flights = GenerateRandomFlights(provider.Name);
        var response = new ProviderResponseDto(provider.Name, flights);

        _logger.LogInformation(
            "Successfully generated {Count} flights for {ProviderName}.",
            flights.Count, provider.Name);

        return Results.Ok(response);
    }

    private List<FlightOfferDto> GenerateRandomFlights(string providerName)
    {
        var offers = new List<FlightOfferDto>();
        var originPool = new[] { "WAW", "JFK", "LHR", "CDG", "ATL" };
        var destinationPool = new[] { "MIA", "LAX", "DXB", "HND", "SFO" };

        var flightCount = Random.Shared.Next(1, 4);
        var prefix = providerName[..2].ToUpper();

        for (var i = 0; i < flightCount; i++)
        {
            var origin = RandomElement(originPool);
            var destination = RandomElement(destinationPool);
            while (origin == destination)
            {
                destination = RandomElement(destinationPool);
            }

            offers.Add(new FlightOfferDto(
                FlightId: $"{prefix}-{Random.Shared.Next(1000, 9999)}",
                Origin: origin,
                Destination: destination,
                Price: Math.Round((decimal)(Random.Shared.NextDouble() * 800 + 200), 2)
            ));
        }
        return offers;
    }

    private static T RandomElement<T>(T[] array) => array[Random.Shared.Next(array.Length)];
}