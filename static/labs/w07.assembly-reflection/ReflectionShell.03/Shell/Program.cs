using System.Reflection;
using Shell;

var shell = new CommandShell();
shell.Registry.RegisterAssembly(Assembly.GetExecutingAssembly());
shell.Run();
