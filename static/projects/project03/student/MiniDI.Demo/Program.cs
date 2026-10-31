using System.Globalization;
using MiniDI;
using MiniDI.Demo;
using MiniDI.Demo.Shop;
using MiniDI.Generated;

// Usage:
//   dotnet run --project MiniDI.Demo                                    (generated registrations)
//   dotnet run --project MiniDI.Demo -- reflection                      (assembly scanning)
//   dotnet run -c Release --project MiniDI.Demo -- bench reflection     (Part 4 benchmark)
//   dotnet run -c Release --project MiniDI.Demo -- bench generated
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; // the same output on every machine

string mode = args.Length > 0 ? args[0] : "generated";
if (mode == "bench")
{
    await Benchmarks.RunAsync(args.Length > 1 ? args[1] : "generated");
    return;
}

var services = new ServiceCollection();
switch (mode)
{
    case "generated":
        services.AddGenerated(); // emitted by MiniDI.Generator at compile time
        break;
    case "reflection":
        services.AddFromAssembly(typeof(Program).Assembly);
        break;
    default:
        Console.Error.WriteLine("Usage: MiniDI.Demo [generated | reflection | bench <reflection | generated>]");
        return;
}

Console.WriteLine($"=== MiniDI demo ({mode}): {services.Count} registrations ===");
await using var provider = services.BuildServiceProvider(validateOnBuild: true);

(string Customer, OrderLine[] Lines)[] requests =
[
    ("alice", [new("keyboard", 1, 249.00m), new("mouse", 2, 99.50m)]),
    ("bob", [new("cable", 12, 15.00m)]),
];

foreach (var (customer, lines) in requests)
{
    Console.WriteLine($"-- request for {customer} --");
    await using var scope = provider.CreateScope();
    var order = scope.GetRequiredService<IOrderService>().PlaceOrder(customer, lines);
    scope.GetRequiredService<ReceiptPrinter>().Print(order);
}

Console.WriteLine($"-- {provider.GetRequiredService<IRepository<Order>>().GetAll().Count} orders in the singleton repository --");

try
{
    provider.GetRequiredService<IOrderService>();
}
catch (ResolutionException e)
{
    Console.WriteLine($"{e.GetType().Name}: {e.Message}");
}
