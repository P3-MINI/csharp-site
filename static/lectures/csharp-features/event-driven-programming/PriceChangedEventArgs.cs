namespace ShopEvents;

/// <summary>
/// Custom event arguments for the PriceChanged event.
/// Contains the old and new price of the product.
/// </summary>
public class PriceChangedEventArgs : EventArgs
{
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }

    public PriceChangedEventArgs(decimal oldPrice, decimal newPrice)
    {
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
