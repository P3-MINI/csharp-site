using System.Globalization;

namespace ShopEvents;

/// <summary>
/// Main program to run the shop simulation.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var product = new Product { Name = "Laptop", Price = 1199.90m };
        var notifier = new Notifier();

        product.PriceChanged += notifier.HandlePriceChanged;

        product.Price -= 200.0m;

        // Unsubscribing, to avoid memory leaks:
        product.PriceChanged -= notifier.HandlePriceChanged;
    }
}
