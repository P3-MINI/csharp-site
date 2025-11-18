using FlightScanner.API.Services;
using FlightScanner.Common.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IProviderService, ProviderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        
    app.UseSwaggerUI();      
}

var providersConfig = app.Configuration
    .GetSection("AirlinesConfig:Providers")
    .Get<List<AirlineProvider>>() ?? [];

app.MapGet("/api/providers", (ILogger<Program> logger) =>
{
    logger.LogInformation("Fetching providers...");
    var providerDtos = providersConfig
        .Select(p => new PartnerAirlineDto(p.Id, p.Name, p.Endpoint))
        .ToList();
    return Results.Ok(providerDtos);
});

foreach (var provider in providersConfig)
{
    var currentProvider = provider;
    app.MapGet(currentProvider.Endpoint,
        async (IProviderService simulationService) =>
        {
            return await simulationService.GetProviderResponseAsync(currentProvider);
        });
}

app.Run();