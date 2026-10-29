using System.Reflection;
using System.Text;
using Shell.Abstractions;

namespace Shell;

// A command discovered with reflection: the method to call and the object to call it on
public sealed class CommandInfo(string name, MethodInfo method, object? target)
{
    public string Name { get; } = name;
    public MethodInfo Method { get; } = method;
    public object? Target { get; } = target; // null for static methods
    public ParameterInfo[] Parameters { get; } = method.GetParameters();
    public IReadOnlyList<string> Aliases { get; } = [.. method.GetCustomAttributes<AliasAttribute>().Select(a => a.Alias)];
    public string Description { get; } = method.GetCustomAttribute<DescriptionAttribute>()?.Text ?? "";

    // Created on first use and then reused - faster than MethodInfo.Invoke
    public MethodInvoker Invoker => field ??= MethodInvoker.Create(Method);

    // e.g. "pow <x> [n=2]"
    public string Usage => string.Join(" ", Parameters.Select(FormatParameter).Prepend(Name));

    public string GetDetails()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Usage);
        if (Description.Length > 0)
        {
            sb.AppendLine($"  {Description}");
        }
        if (Aliases.Count > 0)
        {
            sb.AppendLine($"  Aliases: {string.Join(", ", Aliases)}");
        }
        if (Parameters.Length > 0)
        {
            sb.AppendLine("  Parameters:");
            foreach (ParameterInfo parameter in Parameters)
            {
                string description = parameter.GetCustomAttribute<DescriptionAttribute>()?.Text ?? "";
                if (parameter.ParameterType.IsEnum)
                {
                    description += $" ({string.Join(", ", Enum.GetNames(parameter.ParameterType))})";
                }
                sb.AppendLine($"    {parameter.Name,-10} {parameter.ParameterType.FriendlyName,-17} {description.Trim()}".TrimEnd());
            }
        }
        sb.AppendLine($"  Defined in: {Method.DeclaringType!.FullName} ({Method.DeclaringType.Assembly.GetName().Name})");
        return sb.ToString();
    }

    private static string FormatParameter(ParameterInfo parameter) =>
        parameter.IsDefined(typeof(ParamArrayAttribute)) ? $"[{parameter.Name}...]"
        : !parameter.HasDefaultValue ? $"<{parameter.Name}>"
        : parameter.DefaultValue is null ? $"[{parameter.Name}]"
        : $"[{parameter.Name}={TypeInspector.FormatValue(parameter.DefaultValue)}]";
}
