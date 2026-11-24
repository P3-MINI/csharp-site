namespace FlightScanner.Common.Dtos;

public sealed record ProviderResponseDto(
    string ProviderName,
    List<FlightOfferDto> Flights
);