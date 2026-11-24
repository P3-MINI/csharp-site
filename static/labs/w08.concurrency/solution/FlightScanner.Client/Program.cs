namespace FlightScanner.Client;

using FlightScanner.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

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

    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- Flight Scanner Client ---");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await RunFlightScannerAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FATAL ERROR] The application failed: {ex.Message}");
            Console.ResetColor();
        }

        stopwatch.Stop();
        Console.WriteLine("\n--- Aggregation Complete ---");
        Console.WriteLine($"Total operation time: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();

    }

    private static async Task RunFlightScannerAsync()
    {
        using var cts = new CancellationTokenSource(TimeoutMs);

        IProgress<string> progress =
            new Progress<string>(Console.WriteLine);

        List<PartnerAirlineDto>? providers;

        try
        {
            var endpoint = "/api/providers";
            progress.Report($"[Phase 1] Fetching providers from {endpoint}...");
            providers = await httpClient
                .GetFromJsonAsync<List<PartnerAirlineDto>>(endpoint, cts.Token);

            if (providers == null || providers.Count == 0)
            {
                progress.Report("[ERROR] No providers found, exiting...");
                return;
            }

            progress.Report($"[Phase 1] Found {providers.Count} providers.");
        }
        catch (OperationCanceledException)
        {
            progress.Report($"[TIMEOUT] Failed to fetch providers within {TimeoutMs}ms.");
            return;
        }
        catch (Exception ex)
        {
            progress.Report($"[ERROR] Failed to fetch providers: {ex.Message}");
            return;
        }

        progress.Report("\n[Phase 2] Fetching all flights concurrently...");

        var tasks = new List<Task<ProviderResponseDto?>>();

        foreach (var provider in providers)
        {
            tasks.Add(GetFlightsFromProviderAsync(provider, cts.Token, progress));
        }

        var results = await Task.WhenAll(tasks);

        progress.Report("\n[Phase 3] Aggregating and displaying results...");

        var top10CheapestFlights = results
            .Where(response => response != null && response.Flights != null)
            .SelectMany(
                response => response!.Flights,
                (response, flight) => new AggregatedFlightOffer(
                    response!.ProviderName,
                    flight.FlightId,
                    flight.Origin,
                    flight.Destination,
                    flight.Price
                ))
            .OrderBy(offer => offer.Price)
            .Take(10)
            .ToList();

        DisplayTop10Flights(top10CheapestFlights);
    }

    private static async Task<ProviderResponseDto?> GetFlightsFromProviderAsync(
        PartnerAirlineDto provider,
        CancellationToken ct,
        IProgress<string> progress)
    {
        try
        {
            progress.Report($"\t[START] Querying {provider.Name} ({provider.Endpoint})");

            var response = await httpClient.GetAsync(provider.Endpoint, ct);

            if (!response.IsSuccessStatusCode)
            {
                progress.Report($"\t[FAIL] {provider.Name} returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase})");
                return null;
            }

            var providerResponse =
                await response.Content.ReadFromJsonAsync<ProviderResponseDto>(cancellationToken: ct);

            progress.Report($"\t[SUCCESS] {provider.Name} returned {providerResponse?.Flights.Count ?? 0} flights.");
            return providerResponse;
        }
        catch (OperationCanceledException)
        {
            progress.Report($"\t[TIMEOUT] {provider.Name} did not respond in time.");
            return null;
        }
        catch (Exception ex)
        {
            progress.Report($"\t[ERROR] {provider.Name} failed: {ex.Message}");
            return null;
        }
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