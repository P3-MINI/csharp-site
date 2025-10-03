namespace TypeCrafter;

public sealed class Invoice
{
    public Invoice() { }

    public Invoice(
        Guid id,
        string description,
        decimal amount,
        Customer customer)
    {
        Id = id;
        Description = description;
        Amount = amount;
        Customer = customer;
    }

    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public Customer Customer { get; set; } = null!;

    public override string ToString()
        => $"Invoice {Id}: '{Description}', Amount: {Amount:C}, Customer: {Customer}";
}