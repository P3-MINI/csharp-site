using System.Text;

namespace tasks;

public static class StringExtensions
{
    public static string PascalToSnakeCase(this string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append(char.ToLower(name[0]));

        var i = 1;

        while (i < name.Length)
        {
            if (char.IsUpper(name[i]))
            {
                sb.Append('_');
                sb.Append(char.ToLower(name[i]));
            }
            else
            {
                sb.Append(name[i]);
            }
            i++;
        }

        return sb.ToString();
    }

    public static string SnakeToPascalCase(this string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append(char.ToUpper(name[0]));

        var i = 1;

        while (i < name.Length)
        {
            if (name[i] == '_')
            {
                sb.Append(char.ToUpper(name[i + 1]));
                i += 2;
            }
            else
            {
                sb.Append(name[i]);
                i++;
            }
        }

        return sb.ToString();
    }
}