using System.Globalization;

namespace ShopEvents;

/// <summary>
/// Main program to run the shop simulation.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        // Set culture for consistent currency formatting
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        Console.WriteLine("--- Setting up the shop simulation ---");

        // 1. Create products (event sources)
        var laptop = new Product("Laptop", 1200.00m, 5);
        var mouse = new Product("Mouse", 25.50m, 10);

        // 2. Create subscribers
        var cart = new ShoppingCart();
        var display = new Display();
        var notifier = new Notifier();

        // 3. Establish subscriptions
        display.Subscribe(cart);
        notifier.Subscribe(laptop);
        notifier.Subscribe(mouse);

        Console.WriteLine("\n--- Simulation starts ---\n");

        // --- ACTION 1: Add items to cart ---
        Console.WriteLine(">>> ACTION: Adding 1 Laptop and 2 Mice to the cart.");
        cart.AddItem(laptop);
        cart.AddItem(mouse, 2);

        // --- ACTION 2: Change a product's price ---
        Console.WriteLine("\n>>> ACTION: Laptop price is increasing to $1350.50.");
        laptop.Price = 1350.50m; // This triggers a chain of events

        // --- ACTION 3: Add more items, causing out of stock ---
        Console.WriteLine("\n>>> ACTION: Trying to buy the last 4 laptops.");
        cart.AddItem(laptop, 4); // This will make the laptop go out of stock

        // --- ACTION 4: Try to add an out-of-stock item ---
        Console.WriteLine("\n>>> ACTION: Trying to buy one more laptop.");
        cart.AddItem(laptop, 1); // Should fail

        // --- ACTION 5: Restock an item ---
        Console.WriteLine("\n>>> ACTION: Restocking laptops (10 units).");
        laptop.StockQuantity += 10; // This will trigger the notifier

        // --- ACTION 6: Remove an item from the cart ---
        Console.WriteLine("\n>>> ACTION: Removing 1 mouse from the cart.");
        cart.RemoveItem(mouse);

        Console.WriteLine("\n--- Simulation finished ---");
    }
}
