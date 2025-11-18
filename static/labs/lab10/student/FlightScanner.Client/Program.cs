namespace FlightScanner.Client;

using System;
using System.Collections.Generic;
using System.Net.Http;

public record AggregatedFlightOffer(
    string ProviderName,
    string FlightId,
    string Origin,
    string Destination,
    decimal Price
);

public class Program
{
    public const int TimeoutMs = 3000;

    private static readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5222")
    };

    public static void Main(string[] args)
    {
        DisplayTop10Flights([]);
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void DisplayTop10Flights(List<AggregatedFlightOffer> offers)
    {
        if (offers.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No flight offers could be aggregated.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("\n--- Top 10 Cheapest Flights Found ---");
        Console.ForegroundColor = ConsoleColor.Green;

        var header = $"{"Price",-12} {"Provider",-25} {"Flight",-10} {"Route",-10}";
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var offer in offers)
        {
            var price = $"{offer.Price:C}";
            var route = $"{offer.Origin} -> {offer.Destination}";
            Console.WriteLine(
                $"{price,-12} {offer.ProviderName,-25} {offer.FlightId,-10} {route,-10}"
            );
        }

        Console.ResetColor();
    }
}