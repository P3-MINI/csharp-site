using System.Text;

namespace tasks;

public sealed class Task01 : IExecutable
{
    public void Execute(string[] args)
    {
        var pascalSnakeNAmes = new List<(string pascal, string snake)>()
        {
            ("UserProfileId", "user_profile_id"),
            ("IsAdminUser", "is_admin_user"),
            ("EmailAddress", "email_address"),
            ("HttpRequestHeaders", "http_request_headers"),
            ("StartDateTime", "start_date_time"),
            ("XmlParserSettings", "xml_parser_settings"),
            ("HtmlElementId", "html_element_id"),
            ("OAuthToken", "o_auth_token"),
            ("JsonResponseData", "json_response_data"),
        };

        Console.WriteLine($"Testing {nameof(StringExtensions.PascalToSnakeCase)}:");
        pascalSnakeNAmes.ForEach(name =>
        {
            var snake = name.pascal.PascalToSnakeCase();
            Console.WriteLine($"{name.pascal,-20} -> {snake,-20} | {snake == name.snake}");
        });

        Console.WriteLine();

        Console.WriteLine($"Testing {nameof(StringExtensions.SnakeToPascalCase)}:");
        pascalSnakeNAmes.ForEach(name =>
        {
            var pascal = name.snake.SnakeToPascalCase();
            Console.WriteLine($"{name.snake,-20} -> {pascal,-20} | {pascal == name.pascal}");
        });
    }
}

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