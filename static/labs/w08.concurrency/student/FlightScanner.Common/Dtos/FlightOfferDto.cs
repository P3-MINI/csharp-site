namespace FlightScanner.Common.Dtos;

public sealed record FlightOfferDto(
    string FlightId,
    string Origin,
    string Destination,
    decimal Price
);