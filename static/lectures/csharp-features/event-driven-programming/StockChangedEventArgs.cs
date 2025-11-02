namespace ShopEvents;

/// <summary>
/// Custom event arguments for the StockChanged event.
/// Contains the old and new stock quantity.
/// </summary>
public class StockChangedEventArgs : EventArgs
{
    public int OldQuantity { get; }
    public int NewQuantity { get; }

    public StockChangedEventArgs(int oldQuantity, int newQuantity)
    {
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
    }
}
