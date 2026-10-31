namespace MiniDI.Demo.Shop;

public sealed record Order(int Id, string Customer, IReadOnlyList<OrderLine> Lines, decimal Total, DateTime PlacedAt);

/// <summary>Per-request data; disposed together with the scope.</summary>
[Service(Lifetime.Scoped)]
public sealed class RequestContext : IDisposable
{
    private static int s_lastId;

    public int RequestId { get; } = Interlocked.Increment(ref s_lastId);

    public void Dispose() => Console.WriteLine($"  RequestContext #{RequestId} disposed");
}

public interface IOrderService
{
    Order PlaceOrder(string customer, params IReadOnlyList<OrderLine> lines);
}

[Service(Lifetime.Scoped, As = typeof(IOrderService))]
public sealed class OrderService(
    RequestContext request,
    IRepository<Order> orders,
    PriceCalculator calculator,
    IClock clock,
    ILogger<OrderService> logger) : IOrderService, IAsyncDisposable
{
    public Order PlaceOrder(string customer, params IReadOnlyList<OrderLine> lines)
    {
        var (total, discounts) = calculator.Calculate(lines);
        var order = new Order(orders.GetAll().Count + 1, customer, lines, total, clock.Now);
        orders.Add(order);
        string applied = discounts.Count == 0 ? "none" : string.Join(", ", discounts);
        logger.Log($"request #{request.RequestId}: order #{order.Id} for {customer}, discounts: {applied}");
        return order;
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine($"  OrderService of request #{request.RequestId} disposed asynchronously");
        return ValueTask.CompletedTask;
    }
}

[Service]
public sealed class ReceiptPrinter
{
    private readonly IClock? _clock;
    private readonly string _currency;

    public ReceiptPrinter() : this(clock: null)
    {
    }

    // [Inject] selects this constructor; `currency` is not registered, so its default value is used.
    [Inject]
    public ReceiptPrinter(IClock? clock, string currency = "PLN")
    {
        _clock = clock;
        _currency = currency;
    }

    public void Print(Order order)
    {
        Console.WriteLine($"  Receipt #{order.Id} ({order.Customer}, {_clock?.Now:yyyy-MM-dd HH:mm})");
        foreach (var line in order.Lines)
            Console.WriteLine($"    {line.Quantity,3} x {line.Product,-10} {line.Quantity * line.UnitPrice,9:0.00} {_currency}");
        Console.WriteLine($"    {"total",-16} {order.Total,9:0.00} {_currency}");
    }
}
