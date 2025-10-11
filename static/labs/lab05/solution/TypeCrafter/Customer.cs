namespace TypeCrafter;

public sealed class Customer
{
    public Customer() { }

    public Customer(int id, string name, decimal balance)
    {
        Id = id;
        Name = name;
        Balance = balance;
    }

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public override string ToString()
        => $"Customer {Id}: {Name} (Balance: {Balance:C})";
}