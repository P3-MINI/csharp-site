using System.Text.Json;
using System.Text.RegularExpressions;

namespace tasks;

public class Task01 : IExecutable
{
	public void Execute(string[] args)
	{
		Console.WriteLine($"Executing {nameof(Task01)}...");

		var input1 = "http://example.com/v1/users/42?lang=pl";
		var (parsed1, status1) = ParseUrl(input1);
		/*
		status1              == ParsingStatus.Success
		parsed1.Scheme       == UrlScheme.Http
		parsed1.Host         == "example.com"
		parsed1.Version      == 1
		parsed1.PathSegments == [ { Name = "users", Id = 42 } ]
		parsed1.QueryParams  == { "lang": ["pl"] }
		*/

		Console.WriteLine(JsonSerializer.Serialize(parsed1));

		var input2 = "https://api.test/v2/orders/100/items/200" +
				"?tag=new&tag=discount&active=true";
		var (parsed2, status2) = ParseUrl(input2);
		/*
		status2              == ParsingStatus.Success
		parsed2.Scheme       == UrlScheme.Https
		parsed2.Host         == "api.test"
		parsed2.Version      == 2
		parsed2.PathSegments == [
			{ Name = "orders", Id = 100 },
			{ Name = "items",  Id = 200 }
		]
		parsed2.QueryParams  == {
			"tag":    ["new","discount"],
			"active": ["true"]
		}
		*/

		Console.WriteLine(JsonSerializer.Serialize(parsed2));

		var input3 = "smtp://host/v1/res/1";
		var (_, status3) = ParseUrl(input3);
		// status3 == ParsingStatus.InvalidScheme

		var input4 = "https://host/v1/resOnly";
		var (_, status4) = ParseUrl(input4);
		// status4 == ParsingStatus.InvalidPath

		var input5 = "http://host/v1/r/10?badparam";
		var (_, status5) = ParseUrl(input5);
		// status5 == ParsingStatus.InvalidQuery
	}

	public static (ParsedUrl url, ParsingStatus status) ParseUrl(string url)
	{
		var parsedUrl = new ParsedUrl();

		if(string.IsNullOrEmpty(url)) 
		{
			return (parsedUrl, ParsingStatus.UnexpectedFormat);
		}

		var colonWithSlashes = url.IndexOf("://");

		if (colonWithSlashes == -1)
		{
			return (parsedUrl, ParsingStatus.UnexpectedFormat);
		}

		var schemeToken = url[..colonWithSlashes];
		url = url[(colonWithSlashes + 3)..];

		if (!Enum.TryParse(typeof(UrlScheme), schemeToken, ignoreCase: true, out var scheme))
		{
			return (parsedUrl, ParsingStatus.InvalidScheme);
		}

		parsedUrl.Scheme = (UrlScheme)scheme;

		var slashAfterHost = url.IndexOf('/');

		if (slashAfterHost == -1)
		{
			return (parsedUrl, ParsingStatus.UnexpectedFormat);
		}

		var host = url[..slashAfterHost];
		url = url[(slashAfterHost + 1)..];

		if (string.IsNullOrEmpty(host))
		{
			return (parsedUrl, ParsingStatus.InvalidHost);
		}

		parsedUrl.Host = host;

		var slashAfterVersion = url.IndexOf('/');

		if (slashAfterVersion == -1)
		{
			return (parsedUrl, ParsingStatus.UnexpectedFormat);
		}

		var versionToken = url[..slashAfterVersion];
		url = url[(slashAfterVersion + 1)..];

		if(string.IsNullOrEmpty(versionToken) || !versionToken.StartsWith('v'))
		{
			return (parsedUrl, ParsingStatus.InvalidVersion);
		}

		if (!int.TryParse(versionToken[1..], out var version) || version < 1)
		{
			return (parsedUrl, ParsingStatus.InvalidVersion);
		}

		parsedUrl.Version =	version;

		var segmentsAndQuery = url.Split('?');

		if(segmentsAndQuery.Length > 2)
		{
			return (parsedUrl, ParsingStatus.UnexpectedFormat);
		}

		var segmentsToken = segmentsAndQuery[0];
		var segmentsTokens = segmentsToken.Split('/');

		if (segmentsTokens.Length % 2 == 1)
		{
			return (parsedUrl, ParsingStatus.InvalidPath);
		}

		var nameRegex = new Regex(@"\w");

		for (var i = 0; i < segmentsTokens.Length; i += 2)
		{
			var name = segmentsTokens[i];
			var idToken = segmentsTokens[i + 1];

			if (!nameRegex.IsMatch(name))
			{
				return (parsedUrl, ParsingStatus.InvalidPath);
			}
			if (!int.TryParse(idToken, out var id) || id < 1)
			{
				return (parsedUrl, ParsingStatus.InvalidId);
			}
				
			parsedUrl.PathSegments.Add(new ResourceSegment()
			{
				Name = name,
				Id = id
			});
		}

		if (segmentsAndQuery.Length == 1)
		{
			return (parsedUrl, ParsingStatus.Success);
		}

		var queryToken = segmentsAndQuery[1];
		var queryTokens = queryToken.Split(['&', '=']);

		if(queryTokens.Length % 2 == 1)
		{
			return (parsedUrl, ParsingStatus.InvalidQuery);
		}

		for (var i = 0; i < queryTokens.Length; i += 2)
		{
			var name = queryTokens[i];
			var value = queryTokens[i + 1];

			if(!nameRegex.IsMatch(name) || !nameRegex.IsMatch(value))
			{
				return (parsedUrl, ParsingStatus.InvalidQuery);
			}

			if (parsedUrl.QueryParams.TryGetValue(name, out var values))
			{
				values.Add(value);
			}
			else
			{
				parsedUrl.QueryParams.Add(name, [value]);
			}
		}

		return (parsedUrl, ParsingStatus.Success);
	}
}

public enum UrlScheme
{
	Http,
	Https,
	Ftp,
	Wss
}

public sealed class ResourceSegment
{
	public string Name { get; set; } = string.Empty;
	public int Id { get; set; }
}

public enum ParsingStatus
{
	UnexpectedFormat,
	Success,
	InvalidScheme,
	InvalidHost,
	InvalidVersion,
	InvalidPath,
	InvalidId,
	InvalidQuery,
}

public sealed class ParsedUrl
{
	public UrlScheme Scheme { get; set; }
	public string Host { get; set; } = string.Empty;
	public int Version { get; set; }
	public List<ResourceSegment> PathSegments { get; set; } = [];
	public Dictionary<string, List<string>> QueryParams { get; set; } = [];
}