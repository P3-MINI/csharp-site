namespace ShopEvents;

/// <summary>
/// Simulates a UI display that renders the state of a shopping cart.
/// This class is an event subscriber to the ShoppingCart's events.
/// </summary>
public class Display
{
    public void Subscribe(ShoppingCart cart)
    {
        cart.CartUpdated += OnCartUpdated;
    }

    protected virtual void OnCartUpdated(object? sender, EventArgs e)
    {
        if (sender is ShoppingCart cart)
        {
            Console.WriteLine("\n---[ Shopping Cart Display ]---");
            if (cart.Items.Count == 0)
            {
                Console.WriteLine("  Cart is empty.");
            }
            else
            {
                foreach (var item in cart.Items)
                {
                    Console.WriteLine($"  - {item.Key.Name,-10} | {item.Value} unit(s) | {item.Key.Price:C} each");
                }
            }
            Console.WriteLine("-------------------------------");
            Console.WriteLine($"  Total Price: {cart.TotalPrice:C}");
            Console.WriteLine("-------------------------------\n");
        }
    }
}
