namespace MiniDI.Demo.Shop;

public interface IClock
{
    DateTime Now { get; }
}

[Service(Lifetime.Singleton, As = typeof(IClock))]
public sealed class FixedClock : IClock
{
    // A fixed time (a Saturday) keeps the demo output deterministic.
    public DateTime Now { get; } = new(2026, 1, 17, 10, 30, 0);
}

public interface ILogger<T>
{
    void Log(string message);
}

[Service(Lifetime.Singleton, As = typeof(ILogger<>))]
public sealed class ConsoleLogger<T> : ILogger<T>
{
    public void Log(string message) => Console.WriteLine($"  [{typeof(T).Name}] {message}");
}

public interface IRepository<T>
{
    void Add(T item);
    IReadOnlyList<T> GetAll();
}

[Service(Lifetime.Singleton, As = typeof(IRepository<>))]
public sealed class InMemoryRepository<T> : IRepository<T>
{
    private readonly List<T> _items = [];
    private readonly Lock _lock = new();

    public void Add(T item)
    {
        lock (_lock)
            _items.Add(item);
    }

    public IReadOnlyList<T> GetAll()
    {
        lock (_lock)
            return [.. _items];
    }
}
