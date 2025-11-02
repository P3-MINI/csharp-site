namespace ShopEvents;

/// <summary>
/// Simulates a store notifier that listens to product stock changes.
/// This class is an event subscriber.
/// </summary>
public class Notifier
{
    public void Subscribe(Product product)
    {
        product.StockChanged += OnProductStockChanged;
    }

    public void Unsubscribe(Product product)
    {
        product.StockChanged -= OnProductStockChanged;
    }

    protected virtual void OnProductStockChanged(object? sender, StockChangedEventArgs e)
    {
        if (sender is not Product product) return;

        switch (e.NewQuantity)
        {
            case 0 when e.OldQuantity > 0:
                Console.WriteLine($"[NOTIFIER] ALERT: '{product.Name}' is now OUT OF STOCK.");
                break;
            case > 0 when e.OldQuantity == 0:
                Console.WriteLine($"[NOTIFIER] GOOD NEWS: '{product.Name}' is BACK IN STOCK ({e.NewQuantity} units).");
                break;
        }
    }
}
