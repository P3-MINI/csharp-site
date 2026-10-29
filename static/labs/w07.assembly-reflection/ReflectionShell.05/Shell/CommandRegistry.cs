using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Shell.Abstractions;

namespace Shell;

public sealed class CommandRegistry
{
    private readonly List<CommandInfo> _commands = [];
    // Commands by their names and aliases
    private readonly Dictionary<string, CommandInfo> _lookup = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<CommandInfo> Commands => _commands.OrderBy(c => c.Name, StringComparer.Ordinal);

    public bool TryFind(string nameOrAlias, [NotNullWhen(true)] out CommandInfo? command) =>
        _lookup.TryGetValue(nameOrAlias, out command);

    // Registers all classes marked with [CommandModule] defined in the assembly
    public void RegisterAssembly(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsClass && type.IsDefined(typeof(CommandModuleAttribute), inherit: false))
            {
                RegisterModule(type);
            }
        }
    }

    public void RegisterModule(Type type)
    {
        bool isStatic = type.IsAbstract && type.IsSealed; // static classes are "abstract sealed" in IL
        if (!isStatic && (type.IsAbstract || type.GetConstructor(Type.EmptyTypes) is null))
        {
            Warn($"{type.FullName} has no public parameterless constructor - skipped.");
            return;
        }

        object? instance = isStatic ? null : Activator.CreateInstance(type);
        RegisterCommands(type, instance);
    }

    public void RegisterModule(object instance) => RegisterCommands(instance.GetType(), instance);

    public void Clear()
    {
        _commands.Clear();
        _lookup.Clear();
    }

    public string GetHelp(string? name = null)
    {
        if (name is not null)
        {
            return TryFind(name, out CommandInfo? command) ? command.GetDetails() : $"Unknown command '{name}'.";
        }

        var sb = new StringBuilder();
        foreach (var module in Commands.GroupBy(c => c.Method.DeclaringType!).OrderBy(g => g.Key.Name))
        {
            string? description = module.Key.GetCustomAttribute<DescriptionAttribute>()?.Text;
            sb.AppendLine(description is null ? $"{module.Key.Name}:" : $"{module.Key.Name} - {description}");
            foreach (CommandInfo command in module)
            {
                sb.AppendLine($"  {command.Usage,-40} {command.Description}".TrimEnd());
            }
        }
        return sb.ToString();
    }

    private void RegisterCommands(Type type, object? instance)
    {
        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            CommandAttribute? attribute = method.GetCustomAttribute<CommandAttribute>();
            if (attribute is null)
            {
                continue;
            }
            if (method.IsGenericMethodDefinition || method.GetParameters().Any(p => p.ParameterType.IsByRef))
            {
                Warn($"{type.Name}.{method.Name}: generic methods and ref/out parameters are not supported - skipped.");
                continue;
            }

            string name = attribute.Name ?? method.Name.ToLowerInvariant();
            Add(new CommandInfo(name, method, method.IsStatic ? null : instance));
        }
    }

    private void Add(CommandInfo command)
    {
        string[] keys = [command.Name, .. command.Aliases];
        if (keys.FirstOrDefault(_lookup.ContainsKey) is { } duplicate)
        {
            Warn($"{command.Method.DeclaringType!.Name}.{command.Method.Name}: name '{duplicate}' is already taken - skipped.");
            return;
        }

        _commands.Add(command);
        foreach (string key in keys)
        {
            _lookup[key] = command;
        }
    }

    private static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning: {message}");
        Console.ResetColor();
    }
}
