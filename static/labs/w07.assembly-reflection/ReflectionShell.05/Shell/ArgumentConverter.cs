using System.Globalization;
using System.Reflection;

namespace Shell;

public static class ArgumentConverter
{
    // Converts the textual arguments to values of the method's parameters
    public static object?[] Bind(ParameterInfo[] parameters, string[] tokens)
    {
        var arguments = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            if (parameter.IsDefined(typeof(ParamArrayAttribute)))
            {
                // params T[] is always the last parameter - it takes all the remaining tokens
                Type elementType = parameter.ParameterType.GetElementType()!;
                string[] rest = i < tokens.Length ? tokens[i..] : [];
                Array array = Array.CreateInstance(elementType, rest.Length);
                for (int j = 0; j < rest.Length; j++)
                {
                    array.SetValue(ConvertValue(rest[j], elementType), j);
                }
                arguments[i] = array;
                return arguments;
            }

            if (i < tokens.Length)
            {
                arguments[i] = ConvertValue(tokens[i], parameter.ParameterType);
            }
            else if (parameter.HasDefaultValue)
            {
                arguments[i] = parameter.DefaultValue;
            }
            else
            {
                throw new ArgumentException($"Missing value for parameter '{parameter.Name}'.");
            }
        }

        if (tokens.Length > parameters.Length)
        {
            throw new ArgumentException($"Too many arguments: expected at most {parameters.Length}, got {tokens.Length}.");
        }
        return arguments;
    }

    public static object? ConvertValue(string text, Type type)
    {
        try
        {
            return ConvertCore(text, type);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or InvalidCastException)
        {
            throw new ArgumentException($"Cannot convert '{text}' to {type.FriendlyName}.", ex);
        }
    }

    private static object? ConvertCore(string text, Type type)
    {
        if (type == typeof(string))
        {
            return text;
        }
        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            return ConvertCore(text, underlying);
        }
        if (type.IsEnum)
        {
            return Enum.TryParse(type, text, ignoreCase: true, out object? value) && Enum.IsDefined(type, value!)
                ? value
                : throw new ArgumentException($"'{text}' is not one of: {string.Join(", ", Enum.GetNames(type))}.");
        }
        if (typeof(IConvertible).IsAssignableFrom(type))
        {
            // Primitive types, decimal, DateTime, ...
            return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
        }

        // Other parsable types (DateOnly, TimeSpan, Guid, ...) have a static method
        // bool TryParse(string s, IFormatProvider provider, out T result)
        MethodInfo? tryParse = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static,
            [typeof(string), typeof(IFormatProvider), type.MakeByRefType()]);
        if (tryParse is null)
        {
            throw new ArgumentException($"Parameters of type {type.FriendlyName} are not supported.");
        }

        object?[] arguments = [text, CultureInfo.InvariantCulture, null];
        bool parsed = (bool)tryParse.Invoke(null, arguments)!;
        // Invoke stores the values of out parameters back in the arguments array
        return parsed ? arguments[2] : throw new FormatException();
    }
}
