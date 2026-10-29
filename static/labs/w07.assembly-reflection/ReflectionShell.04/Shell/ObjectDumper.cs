using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Shell;

public static class ObjectDumper
{
    public static string Dump(object? value, int maxDepth = 5)
    {
        var sb = new StringBuilder();
        // Objects on the path from the root to the current object. Compared by reference -
        // two different (but equal) records must not be mistaken for a cycle.
        var path = new HashSet<object>(ReferenceEqualityComparer.Instance);
        DumpValue(sb, value, indent: 0, maxDepth, path);
        return sb.ToString();
    }

    // Values printed in a single line instead of being expanded into properties
    private static bool IsSimple(Type type) =>
        type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
        || typeof(IFormattable).IsAssignableFrom(type); // DateTime, DateOnly, TimeSpan, Guid, ...

    private static void DumpValue(StringBuilder sb, object? value, int indent, int depthLeft, HashSet<object> path)
    {
        if (value is null)
        {
            sb.AppendLine("null");
            return;
        }

        Type type = value.GetType();
        if (IsSimple(type))
        {
            sb.AppendLine(TypeInspector.FormatValue(value));
            return;
        }
        if (depthLeft == 0)
        {
            sb.AppendLine($"{type.FriendlyName} {{ ... }}");
            return;
        }
        if (!path.Add(value))
        {
            sb.AppendLine($"<cycle: {type.FriendlyName}>");
            return;
        }

        try
        {
            if (value is IEnumerable items)
            {
                List<object?> elements = [.. items.Cast<object?>()];
                sb.AppendLine($"{type.FriendlyName} (Count = {elements.Count})");
                for (int i = 0; i < elements.Count; i++)
                {
                    sb.Append(' ', 2 * (indent + 1)).Append($"[{i}]: ");
                    DumpValue(sb, elements[i], indent + 1, depthLeft - 1, path);
                }
                return;
            }

            sb.AppendLine(type.FriendlyName);
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Indexers are properties with parameters - they cannot be read without an index
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                sb.Append(' ', 2 * (indent + 1)).Append($"{property.Name}: ");
                object? propertyValue;
                try
                {
                    propertyValue = property.GetValue(value);
                }
                catch (TargetInvocationException ex)
                {
                    // The getter itself has thrown an exception
                    sb.AppendLine($"<{ex.InnerException?.GetType().Name}>");
                    continue;
                }
                DumpValue(sb, propertyValue, indent + 1, depthLeft - 1, path);
            }
        }
        finally
        {
            path.Remove(value);
        }
    }
}
