using System.Globalization;
using System.Text;

namespace tasks;

public sealed class Task02 : IExecutable
{
	public void Execute(string[] args)
	{
		Console.WriteLine($"Executing {nameof(Task02)}...");

		// pass the path to the CSV file as a command-line argument with index 2
		var content = File.ReadAllText(args[2]);

		var measurements = ParseMeasurements(content);

		var currentYear = DateTime.Now.Year;
		var from = new DateTime(currentYear, 6, 8);
		var to = new DateTime(currentYear, 9, 13);

		var today = DateTime.Today;
		var weekBefore = today.AddDays(-7);

		// an array of lambda expressions (uses standard Action<T> delegate)
		var filters = new List<Action<Measurement>>()
		{
			(m) =>
			{
				if (from <= m.Date && m.Date <= to) Console.WriteLine(m);
			},
			(m) =>
			{
				if (m.Date.Year == 2025) Console.WriteLine(m);
			},
			(m) =>
			{
				if (m.Date.DayOfWeek == DayOfWeek.Saturday ||
					m.Date.DayOfWeek == DayOfWeek.Sunday) Console.WriteLine(m);
			},
			(m) =>
			{
				if (weekBefore <= m.Date && m.Date <= today) Console.WriteLine(m);
			},
		};

		var counter = 1;

		filters.ForEach(filter =>
		{
			Console.WriteLine($"Applying filter {counter++}:\n");
			measurements.ForEach(measurement => filter(measurement));
		});
	}

	public static List<Measurement> ParseMeasurements(string content)
	{
		var splitOptions = StringSplitOptions.RemoveEmptyEntries |
			StringSplitOptions.TrimEntries;

		var records = content.Split("\n", splitOptions)[1..];

		var measurements = new List<Measurement>(capacity: records.Length);

		foreach (var record in records)
		{
			var tokens = record.Split(";", splitOptions);

			var location = tokens[0].Split([' ', '\t'], splitOptions);
			var country = location[0];
			var city = location[1];

			var code = tokens[1];

			var date = DateTime.Parse(tokens[2], CultureInfo.InvariantCulture);

			var temperatureTokens = tokens[3]
				.Trim('[', ']')
				.Split(',', splitOptions);

			var temperatures = new List<double>(capacity: temperatureTokens.Length);

			foreach (var temperatureToken in temperatureTokens)
			{
				temperatures.Add(double.Parse(temperatureToken, CultureInfo.InvariantCulture));
			}

			var measurement = new Measurement()
			{
				Country = country,
				City = city,
				Code = code,
				Date = date,
				Temperatures = [.. temperatures],
			};

			measurements.Add(measurement);
		}

		return measurements;
	}
}

public sealed class Measurement
{
	public string Country { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public DateTime Date { get; set; }
	public double[] Temperatures { get; set; } = [];

	public override string ToString()
	{
		var culture = CultureInfo.GetCultureInfo(Code);

		var sb = new StringBuilder();

		sb.AppendLine($"Location: {Country} {City}");
		sb.AppendLine($"Date: {Date.ToString("D", culture)}");
		sb.AppendLine($"Temperatures:");

		for (var i = 0; i < Temperatures.Length; i++)
		{
			sb.Append($"{Temperatures[i].ToString("F2", culture).PadLeft(7)} °C");

			if (i % 4 == 3 || i == Temperatures.Length - 1)
			{
				sb.AppendLine();
			}
		}

		return sb.ToString();
	}
}
