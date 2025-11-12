namespace FlightScanner.API.Services;

public interface IProviderService
{
    Task<IResult> GetProviderResponseAsync(AirlineProvider provider);
}