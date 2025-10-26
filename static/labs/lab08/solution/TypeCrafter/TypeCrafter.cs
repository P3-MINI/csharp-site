using System.Reflection;

namespace TypeCrafter;

public class ParseException : Exception
{
    public ParseException() { }

    public ParseException(string message) 
        : base(message) { }

    public ParseException(string message, Exception innerException) 
        : base(message, innerException) { }
}

public static class TypeCrafter
{
    public static T CraftInstance<T>()
    {
        var type = typeof(T);
        var parsableType = typeof(IParsable<>);
        var stringType = typeof(string);
        var tryParseMethodName = nameof(int.TryParse);

        Console.WriteLine($"Constructing instance of type {type.FullName}...");

        var properties = type.GetProperties();

        var constructor = type.GetConstructor(Type.EmptyTypes)
            ?? throw new InvalidOperationException($"Type {type.FullName} has no parameterless constructor.");
        
        var result = (T)constructor.Invoke(null);

        foreach (var property in properties)
        {
            var propertyType = property.PropertyType;
            var isParsable = propertyType
                .GetInterfaces()
                .Any(t => t.IsGenericType && 
                    t.GetGenericTypeDefinition() == parsableType &&
                    t.GetGenericArguments()[0] == propertyType);

            if (propertyType == stringType)
            {
                var input = AskForInput(property.Name, propertyType.Name);
                property.SetValue(result, input);
            }
            else if (isParsable)
            {
                var input = AskForInput(property.Name, propertyType.Name);
                var parseMethod = propertyType
                    .GetMethod(
                        tryParseMethodName,
                        BindingFlags.Public | BindingFlags.Static,
                        binder: null,
                        types: [typeof(string), typeof(IFormatProvider), propertyType.MakeByRefType()],
                        modifiers: null) 
                    ?? throw new Exception($"{propertyType.FullName} does not have a static {tryParseMethodName} method.");
                
                var args = new object[] { input, null!, null! };

                if (parseMethod.Invoke(null, args) is bool status && status)
                {
                    property.SetValue(result, args[2]);
                }
                else 
                {
                    throw new ParseException($"Could not parse {input} to {propertyType}.");
                }
            }
            else 
            {
                Console.WriteLine($"Type of property '{property.PropertyType} {property.Name}' is not parsable.");
                Console.WriteLine("Attempting to craft object recursively:");
                
                var craftMethod = typeof(TypeCrafter).GetMethod(nameof(CraftInstance), BindingFlags.Public | BindingFlags.Static);
                var genericMethod = craftMethod?.MakeGenericMethod(property.PropertyType);
                var complexProperty = genericMethod?.Invoke(null, null);
                
                property.SetValue(result, complexProperty);
            }
        }

        return result;
    }

    private static string AskForInput(string propertyName, string type)
    {
        Console.WriteLine($"Provide a value of type {type} for {propertyName}:");

        if (Console.ReadLine() is { } line)
        {
            return line;
        }

        throw new IOException("Line from the Console is not available.");
    }
}