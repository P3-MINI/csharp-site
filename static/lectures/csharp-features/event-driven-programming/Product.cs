namespace ShopEvents;

/// <summary>
/// Represents a product in the shop.
/// This class is an event source for price and stock changes.
/// </summary>
public class Product
{
    public string Name { get; }
    private decimal _price;
    private int _stockQuantity;

    // Event declarations
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;
    public event EventHandler<StockChangedEventArgs>? StockChanged;

    public Product(string name, decimal price, int stockQuantity)
    {
        Name = name;
        _price = price;
        _stockQuantity = stockQuantity;
    }

    public decimal Price
    {
        get => _price;
        set
        {
            if (_price != value)
            {
                decimal oldPrice = _price;
                _price = value;
                OnPriceChanged(new PriceChangedEventArgs(oldPrice, _price));
            }
        }
    }

    public int StockQuantity
    {
        get => _stockQuantity;
        set
        {
            if (_stockQuantity != value)
            {
                int oldStock = _stockQuantity;
                _stockQuantity = value;
                OnStockChanged(new StockChangedEventArgs(oldStock, _stockQuantity));
            }
        }
    }

    // Methods to raise events
    protected virtual void OnPriceChanged(PriceChangedEventArgs e)
    {
        PriceChanged?.Invoke(this, e);
    }

    protected virtual void OnStockChanged(StockChangedEventArgs e)
    {
        StockChanged?.Invoke(this, e);
    }
}
