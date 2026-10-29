using System.Globalization;
using System.Reflection;
using System.Text;

namespace Shell;

public static class TypeInspector
{
    // Members declared directly in the type (inherited members are skipped), regardless of their visibility
    private const BindingFlags Declared = BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    public static string Describe(Type type)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Kind(type)} {type.FullName}");
        if (type.BaseType is { } baseType)
        {
            sb.AppendLine($"  Base type: {baseType.FriendlyName}");
        }
        if (type.GetInterfaces() is { Length: > 0 } interfaces)
        {
            sb.AppendLine($"  Interfaces: {string.Join(", ", interfaces.Select(i => i.FriendlyName))}");
        }
        sb.AppendLine($"  Assembly: {type.Assembly.GetName().Name}");

        AppendSection(sb, "Constructors", type.GetConstructors(Declared)
            .Select(c => $"{Access(c)} {(c.IsStatic ? "static " : "")}{type.Name}({FormatParameters(c)})"));
        AppendSection(sb, "Fields", type.GetFields(Declared)
            .OrderBy(f => f.Name)
            .Select(FormatField));
        AppendSection(sb, "Properties", type.GetProperties(Declared)
            .OrderBy(p => p.Name)
            .Select(FormatProperty));
        AppendSection(sb, "Methods", type.GetMethods(Declared)
            .Where(m => !m.IsSpecialName) // skip property accessors (get_X, set_X) and operators
            .OrderBy(m => m.Name)
            .Select(m => $"{Access(m)} {Modifiers(m)}{m.ReturnType.FriendlyName} {m.Name}({FormatParameters(m)})"));
        return sb.ToString();
    }

    // Formats a value the way it would appear in C# source code
    public static string FormatValue(object? value) => value switch
    {
        null => "null",
        string text => $"\"{text}\"",
        bool flag => flag ? "true" : "false",
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "",
    };

    private static void AppendSection(StringBuilder sb, string title, IEnumerable<string> lines)
    {
        sb.AppendLine($"{title}:");
        bool empty = true;
        foreach (string line in lines)
        {
            sb.AppendLine($"  {line}");
            empty = false;
        }
        if (empty)
        {
            sb.AppendLine("  (none)");
        }
    }

    private static string Kind(Type type) => type switch
    {
        { IsInterface: true } => "interface",
        { IsEnum: true } => "enum",
        { IsValueType: true } => "struct",
        { IsAbstract: true, IsSealed: true } => "static class", // static classes are "abstract sealed" in IL
        { IsAbstract: true } => "abstract class",
        _ => "class",
    };

    private static string Access(MethodBase method) =>
        method.IsPublic ? "public"
        : method.IsFamily ? "protected"
        : method.IsAssembly ? "internal"
        : method.IsFamilyOrAssembly ? "protected internal"
        : "private";

    private static string Access(FieldInfo field) =>
        field.IsPublic ? "public"
        : field.IsFamily ? "protected"
        : field.IsAssembly ? "internal"
        : field.IsFamilyOrAssembly ? "protected internal"
        : "private";

    private static string Modifiers(MethodInfo method) =>
        method.IsStatic ? "static "
        : method.IsAbstract ? "abstract "
        : method.GetBaseDefinition().DeclaringType != method.DeclaringType ? "override "
        : method.IsVirtual && !method.IsFinal ? "virtual "
        : "";

    private static string FormatField(FieldInfo field)
    {
        string modifiers = field.IsLiteral ? "const "
            : (field.IsStatic ? "static " : "") + (field.IsInitOnly ? "readonly " : "");
        return $"{Access(field)} {modifiers}{field.FieldType.FriendlyName} {field.Name}";
    }

    private static string FormatProperty(PropertyInfo property)
    {
        // A property is just metadata linking a pair of accessor methods: get_Name and set_Name
        MethodInfo main = (property.GetMethod ?? property.SetMethod)!;
        string Accessor(MethodInfo? accessor, string keyword) =>
            accessor is null ? ""
            : Access(accessor) == Access(main) ? $"{keyword}; "
            : $"{Access(accessor)} {keyword}; ";

        return $"{Access(main)} {Modifiers(main)}{property.PropertyType.FriendlyName} {property.Name} "
            + $"{{ {Accessor(property.GetMethod, "get")}{Accessor(property.SetMethod, "set")}}}";
    }

    private static string FormatParameters(MethodBase method) =>
        string.Join(", ", method.GetParameters().Select(FormatParameter));

    private static string FormatParameter(ParameterInfo parameter)
    {
        string prefix = parameter.IsDefined(typeof(ParamArrayAttribute)) ? "params "
            : parameter.ParameterType.IsByRef ? (parameter.IsOut ? "out " : "ref ")
            : "";
        string suffix = parameter.HasDefaultValue ? $" = {FormatValue(parameter.DefaultValue)}" : "";
        return $"{prefix}{parameter.ParameterType.FriendlyName} {parameter.Name}{suffix}";
    }
}
