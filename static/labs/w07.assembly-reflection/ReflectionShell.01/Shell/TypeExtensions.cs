namespace Shell;

public static class TypeExtensions
{
    private static readonly Dictionary<Type, string> Keywords = new()
    {
        [typeof(void)] = "void",
        [typeof(object)] = "object",
        [typeof(string)] = "string",
        [typeof(bool)] = "bool",
        [typeof(char)] = "char",
        [typeof(byte)] = "byte",
        [typeof(short)] = "short",
        [typeof(int)] = "int",
        [typeof(long)] = "long",
        [typeof(float)] = "float",
        [typeof(double)] = "double",
        [typeof(decimal)] = "decimal",
    };

    extension(Type type)
    {
        // C#-like name of a type, e.g. "int", "List<string>", "int?", "double[]"
        public string FriendlyName => type switch
        {
            _ when Keywords.TryGetValue(type, out string? keyword) => keyword,
            { IsArray: true } => $"{type.GetElementType()!.FriendlyName}[{new string(',', type.GetArrayRank() - 1)}]",
            { IsByRef: true } => type.GetElementType()!.FriendlyName,
            _ when Nullable.GetUnderlyingType(type) is { } underlying => $"{underlying.FriendlyName}?",
            { IsGenericType: true } =>
                $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(t => t.FriendlyName))}>",
            _ => type.Name,
        };
    }
}
