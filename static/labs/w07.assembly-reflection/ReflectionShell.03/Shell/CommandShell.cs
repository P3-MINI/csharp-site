using System.Reflection;
using System.Text.RegularExpressions;
using Shell.Abstractions;

namespace Shell;

[Description("Built-in commands.")]
public sealed partial class CommandShell
{
    private bool _running = true;

    public CommandShell()
    {
        // Built-in commands are methods of this very class
        Registry.RegisterModule(this);
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
            result = command.Method.Invoke(command.Target, arguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is { } inner)
        {
            // The command itself has thrown - Invoke wraps every such exception
            WriteError($"{inner.GetType().Name}: {inner.Message}");
            return;
        }

        if (command.Method.ReturnType != typeof(void))
        {
            string output = result as string ?? ObjectDumper.Dump(result);
            Console.WriteLine(output.TrimEnd());
        }
    }

    [Command, Description("Lists the commands or describes one of them.")]
    public string Help(string? command = null) => Registry.GetHelp(command);

    [Command, Alias("quit"), Description("Exits the shell.")]
    public void Exit() => _running = false;

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
