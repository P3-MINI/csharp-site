namespace ShopEvents;

/// <summary>
/// Represents a shopping cart that holds products.
/// It subscribes to product events and raises its own events when updated.
/// </summary>
public class ShoppingCart
{
    private readonly Dictionary<Product, int> _items = new();
    public IReadOnlyDictionary<Product, int> Items => _items;
    public decimal TotalPrice { get; private set; }

    public event EventHandler? CartUpdated;

    public void AddItem(Product product, int quantity = 1)
    {
        if (product.StockQuantity < quantity)
        {
            Console.WriteLine($"(Cart) Cannot add {product.Name}. Not enough stock.");
            return;
        }

        if (_items.ContainsKey(product))
        {
            _items[product] += quantity;
        }
        else
        {
            _items[product] = quantity;
            product.PriceChanged += OnProductPriceChanged; // Subscribe on first add
        }

        // This is a simplified logic. In a real app, you'd handle this more robustly.
        product.StockQuantity -= quantity;

        RecalculateTotalPrice();
        OnCartUpdated();
    }

    public void RemoveItem(Product product, int quantity = 1)
    {
        if (!_items.ContainsKey(product)) return;

        _items[product] -= quantity;
        if (_items[product] <= 0)
        {
            _items.Remove(product);
            product.PriceChanged -= OnProductPriceChanged; // Unsubscribe on last remove
        }
        
        product.StockQuantity += quantity;

        RecalculateTotalPrice();
        OnCartUpdated();
    }

    protected virtual void OnProductPriceChanged(object? sender, PriceChangedEventArgs e)
    {
        if (sender is Product p)
        {
            Console.WriteLine($"(Cart) Price for '{p.Name}' changed from {e.OldPrice:C} to {e.NewPrice:C}. Updating total.");
            RecalculateTotalPrice();
            OnCartUpdated();
        }
    }

    private void RecalculateTotalPrice()
    {
        TotalPrice = _items.Sum(item => item.Key.Price * item.Value);
    }

    protected virtual void OnCartUpdated()
    {
        CartUpdated?.Invoke(this, EventArgs.Empty);
    }
}
