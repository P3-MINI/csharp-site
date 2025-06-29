using System.Text.RegularExpressions;

namespace tasks;

public sealed class Task04 : IExecutable
{
	public void Execute(string[] args)
	{
		Console.WriteLine($"Executing {nameof(Task04)}...");

		// pass the path to the CSV file as a command-line argument with index 4
		var logs = File.ReadAllText(args[4]);

		var matches = Regex.Matches(logs, RegexPatterns.LogEntry, RegexOptions.Multiline);

		var entries = new List<LogEntry>(capacity: matches.Count);

		foreach (Match match in matches)
		{
			var entry = new LogEntry
			(
				Level: match.Groups[RegexPatterns.Groups.LogLevel].Value,
				Resource: match.Groups[RegexPatterns.Groups.Resource].Value,
				Id: match.Groups[RegexPatterns.Groups.Id].Value,
				HttpCode: int.Parse(match.Groups[RegexPatterns.Groups.HttpCode].Value),
				HttpStatus: match.Groups[RegexPatterns.Groups.HttpStatus].Value
			);
			entries.Add(entry);
		}

		foreach (var log in entries)
		{
			Console.WriteLine($"{log.Level}: {log.Resource}/{log.Id} => {log.HttpCode} {log.HttpStatus}");
		}
	}
}

public record LogEntry(
	string Level,
	string Resource,
	string Id,
	int HttpCode,
	string HttpStatus
);

/// <summary>
/// To construct regular expressions interactively visit: https://regex101.com/
/// </summary>
public sealed class RegexPatterns
{
	public const string LogLevel = @$"(?<{Groups.LogLevel}>[A-Z]+)";
	public const string Resource = @$"(?<{Groups.Resource}>[^\/]+)";
	public const string Id = @$"(?<{Groups.Id}>\d+)";
	public const string HttpCode = @$"(?<{Groups.HttpCode}>\d+)";
	public const string HttpStatus = @$"(?<{Groups.HttpStatus}>[a-zA-Z]+)";
	public const string LogEntry = @$"\[.*\] {LogLevel}[^\/]*\/api\/{Resource}\/{Id} - {HttpCode} {HttpStatus}";

	public static class Groups
	{
		public const string LogLevel = "level";
		public const string HttpCode = "code";
		public const string HttpStatus = "status";
		public const string Resource = "resource";
		public const string Id = "id";
	}
}