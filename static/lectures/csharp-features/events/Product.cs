namespace ShopEvents;

public class Product
{
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;
    
    public required string Name { get; init; }
    private decimal _price;
    public decimal Price 
    {
        get => _price;
        set
        {
            if (value == _price) return;
            decimal oldPrice = _price;
            _price = value;
            OnPriceChanged(new PriceChangedEventArgs(oldPrice, value));
        }
    }

    protected virtual void OnPriceChanged(PriceChangedEventArgs e)
    {
        PriceChanged?.Invoke(this, e);
    }
}
