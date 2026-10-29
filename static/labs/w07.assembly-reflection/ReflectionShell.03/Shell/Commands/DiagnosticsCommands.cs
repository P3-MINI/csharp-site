using Shell.Abstractions;

namespace Shell.Commands;

[CommandModule]
[Description("Diagnostic commands.")]
public static class DiagnosticsCommands
{
    [Command, Description("Describes the members of a type.")]
    public static string Inspect([Description("full or short name of the type")] string typeName)
    {
        Type? type = Type.GetType(typeName)
            ?? AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName);
        return type is null ? $"Type '{typeName}' not found." : TypeInspector.Describe(type);
    }
}
