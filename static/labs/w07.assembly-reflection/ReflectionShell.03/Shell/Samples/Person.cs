namespace Shell.Samples;

public record Address(string City, string Street);

public class Person(string name, int age) : IComparable<Person>
{
    private readonly List<Person> _friends = [];

    public string Name { get; set; } = name;
    public int Age { get; private set; } = age;
    public Address? Address { get; set; }
    public IReadOnlyList<Person> Friends => _friends;
    public bool IsAdult => Age >= 18;

    public void AddFriend(Person friend) => _friends.Add(friend);

    public void CelebrateBirthday() => Age++;

    public string Greet(string greeting = "Hello", bool loud = false)
    {
        string text = $"{greeting}, {Name}!";
        return loud ? text.ToUpperInvariant() : text;
    }

    public static Person Parse(string text)
    {
        string[] parts = text.Split(',', StringSplitOptions.TrimEntries);
        return new Person(parts[0], int.Parse(parts[1]));
    }

    public int CompareTo(Person? other) => string.Compare(Name, other?.Name, StringComparison.Ordinal);

    public override string ToString() => $"{Name} ({Age})";
}
