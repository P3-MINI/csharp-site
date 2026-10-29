using System.Reflection;

namespace MiniDI;

/// <summary>Formats types the way they are written in C#, e.g. <c>IRepository&lt;Order&gt;</c>.</summary>
public static class TypeNameFormatter
{
    public static string Format(Type type)
    {
        if (type.IsArray)
            return $"{Format(type.GetElementType()!)}[{new string(',', type.GetArrayRank() - 1)}]";
        if (!type.IsGenericType)
            return type.Name;

        string name = type.Name;
        int backtick = name.IndexOf('`');
        if (backtick >= 0)
            name = name[..backtick];
        return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(Format))}>";
    }

    /// <summary>Formats a resolution path as <c>A -> B -> C</c>.</summary>
    public static string FormatPath(IEnumerable<Type> path) => string.Join(" -> ", path.Select(Format));

    /// <summary>Formats a constructor as <c>Foo(IBar, Int32)</c>.</summary>
    public static string Format(ConstructorInfo constructor) =>
        $"{Format(constructor.DeclaringType!)}({string.Join(", ", constructor.GetParameters().Select(p => Format(p.ParameterType)))})";
}
