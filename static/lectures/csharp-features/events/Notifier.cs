namespace ShopEvents;

/// <summary>
/// Simulates a store notifier that listens to product stock changes.
/// This class is an event subscriber.
/// </summary>
public class Notifier
{
    // Event handler, with matching signature:
    public void HandlePriceChanged(object? sender, PriceChangedEventArgs e)
    {
        if (sender is Product product)
        {
            Console.WriteLine($"Price of {product.Name} changed. {e.OldPrice:C} -> {e.NewPrice:C}");
        }
    }
}
