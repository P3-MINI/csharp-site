namespace MiniDI.Demo.Shop;

public sealed record OrderLine(string Product, int Quantity, decimal UnitPrice);

public interface IDiscountRule
{
    string Name { get; }
    decimal GetDiscount(IReadOnlyList<OrderLine> lines, decimal subtotal);
}

[Service(As = typeof(IDiscountRule))]
public sealed class BulkDiscount : IDiscountRule
{
    public string Name => "bulk -10%";

    public decimal GetDiscount(IReadOnlyList<OrderLine> lines, decimal subtotal) =>
        lines.Sum(l => l.Quantity) >= 10 ? subtotal * 0.10m : 0m;
}

[Service(As = typeof(IDiscountRule))]
public sealed class WeekendDiscount(IClock clock) : IDiscountRule
{
    public string Name => "weekend -5%";

    public decimal GetDiscount(IReadOnlyList<OrderLine> lines, decimal subtotal) =>
        clock.Now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? subtotal * 0.05m : 0m;
}

/// <summary>Applies every registered <see cref="IDiscountRule"/> (injected as IEnumerable).</summary>
[Service]
public sealed class PriceCalculator(IEnumerable<IDiscountRule> rules)
{
    public (decimal Total, IReadOnlyList<string> Discounts) Calculate(IReadOnlyList<OrderLine> lines)
    {
        decimal subtotal = lines.Sum(l => l.Quantity * l.UnitPrice);
        decimal total = subtotal;
        var applied = new List<string>();
        foreach (var rule in rules)
        {
            decimal discount = rule.GetDiscount(lines, subtotal);
            if (discount > 0)
            {
                total -= discount;
                applied.Add(rule.Name);
            }
        }
        return (total, applied);
    }
}
