using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using Shell.Abstractions;

namespace Shell;

[Description("Built-in commands.")]
public sealed partial class CommandShell
{
    private readonly PluginManager _plugins;
    private bool _running = true;

    public CommandShell(string pluginsDirectory)
    {
        _plugins = new PluginManager(pluginsDirectory);
        LoadCommands();
    }

    public CommandRegistry Registry { get; } = new();

    public void Run()
    {
        Console.WriteLine("Reflection shell. Type 'help' to list commands, 'exit' to quit.");
        while (_running)
        {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line is null)
            {
                break; // end of input
            }
            if (Console.IsInputRedirected)
            {
                Console.WriteLine(line); // echo commands read from a file
            }
            Execute(line);
        }
    }

    public void Execute(string line)
    {
        string[] tokens = Tokenize(line);
        if (tokens.Length == 0)
        {
            return;
        }
        if (!Registry.TryFind(tokens[0], out CommandInfo? command))
        {
            WriteError($"Unknown command '{tokens[0]}'. Type 'help' to list commands.");
            return;
        }

        object?[] arguments;
        try
        {
            arguments = ArgumentConverter.Bind(command.Parameters, tokens[1..]);
        }
        catch (ArgumentException ex)
        {
            WriteError(ex.Message);
            return;
        }

        object? result;
        try
        {
            // Unlike MethodInfo.Invoke, MethodInvoker does not wrap exceptions in TargetInvocationException
            result = command.Invoker.Invoke(command.Target, arguments);
        }
        catch (Exception ex)
        {
            WriteError($"{ex.GetType().Name}: {ex.Message}");
            return;
        }

        if (command.Method.ReturnType != typeof(void))
        {
            string output = result as string ?? ObjectDumper.Dump(result);
            Console.WriteLine(output.TrimEnd());
        }
    }

    private void LoadCommands()
    {
        // Built-in commands are methods of this very class
        Registry.RegisterModule(this);
        Registry.RegisterAssembly(typeof(CommandShell).Assembly);

        _plugins.LoadAll();
        foreach (Assembly plugin in _plugins.Assemblies)
        {
            Registry.RegisterAssembly(plugin);
            Console.WriteLine($"Loaded plugin {plugin.GetName().Name} {plugin.GetName().Version}");
        }
    }

    // Not inlined, so that no local variable of the caller keeps a reference to plugin objects
    [MethodImpl(MethodImplOptions.NoInlining)]
    private List<WeakReference> UnloadCommands()
    {
        // Commands reference plugin methods and module instances - they must go first
        Registry.Clear();
        return _plugins.UnloadAll();
    }

    [Command, Description("Unloads all plugins and loads them again.")]
    public void Reload()
    {
        List<WeakReference> contexts = UnloadCommands();
        // Unloading is cooperative: a context is freed by the GC once nothing references its assemblies
        for (int i = 0; i < 10 && contexts.Any(c => c.IsAlive); i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        int alive = contexts.Count(c => c.IsAlive);
        Console.WriteLine(alive == 0
            ? $"Unloaded {contexts.Count} plugins."
            : $"{alive} of {contexts.Count} plugins are still in memory.");
        LoadCommands();
    }

    [Command, Description("Lists the commands or describes one of them.")]
    public string Help(string? command = null) => Registry.GetHelp(command);

    [Command, Alias("quit"), Description("Exits the shell.")]
    public void Exit() => _running = false;

    [Command, Description("Describes the loaded plugins.")]
    public string Plugins()
    {
        var sb = new StringBuilder();
        foreach (Assembly plugin in _plugins.Assemblies)
        {
            IEnumerable<string> references = plugin.GetReferencedAssemblies().Select(r => $"{r.Name} {r.Version}");
            IEnumerable<string> commands = Registry.Commands
                .Where(c => c.Method.DeclaringType!.Assembly == plugin)
                .Select(c => c.Name);
            // Assemblies loaded from a stream have no location
            string location = plugin.Location.Length > 0
                ? Path.GetRelativePath(AppContext.BaseDirectory, plugin.Location)
                : "(loaded from memory)";

            sb.AppendLine(plugin.FullName);
            sb.AppendLine($"  Description:     {plugin.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description}");
            sb.AppendLine($"  Product version: {plugin.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
            sb.AppendLine($"  Location:        {location}");
            sb.AppendLine($"  Load context:    {AssemblyLoadContext.GetLoadContext(plugin)?.Name}");
            sb.AppendLine($"  References:      {string.Join(", ", references)}");
            sb.AppendLine($"  Commands:        {string.Join(", ", commands)}");
        }
        return sb.ToString();
    }

    // A word or a text in double quotes
    [GeneratedRegex(@"""([^""]*)""|(\S+)")]
    private static partial Regex TokenRegex();

    private static string[] Tokenize(string line) =>
        [.. TokenRegex().Matches(line).Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)];

    private static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
        Console.ResetColor();
    }
}
