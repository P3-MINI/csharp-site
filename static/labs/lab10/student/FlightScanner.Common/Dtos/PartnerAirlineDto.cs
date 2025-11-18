namespace FlightScanner.Common.Dtos;

public sealed record PartnerAirlineDto(
    string Id,
    string Name,
    string Endpoint
);