using System.Reflection;
using Shell;

var registry = new CommandRegistry();
registry.RegisterAssembly(Assembly.GetExecutingAssembly());

Console.WriteLine(registry.GetHelp());
Console.WriteLine(registry.GetHelp("power"));
