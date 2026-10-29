using Shell;

var shell = new CommandShell(Path.Combine(AppContext.BaseDirectory, "plugins"));
shell.Run();
