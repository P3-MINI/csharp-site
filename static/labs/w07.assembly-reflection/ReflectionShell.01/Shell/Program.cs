using Shell;
using Shell.Samples;

Console.WriteLine(TypeInspector.Describe(typeof(Person)));

var alice = new Person("Alice", 30) { Address = new Address("Warsaw", "Koszykowa 75") };
var bob = new Person("Bob", 17);
alice.AddFriend(bob);
bob.AddFriend(alice); // a cycle: alice -> bob -> alice

Console.WriteLine(ObjectDumper.Dump(alice));
Console.WriteLine(ObjectDumper.Dump(new Dictionary<string, DateOnly> { ["Christmas"] = new(2026, 12, 24) }));
