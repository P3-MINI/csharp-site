using System.Text;

namespace Weather.Common;

public class Forecast
{
    public string Location { get; set; }
    public Dictionary<DateTime, double> Temperatures { get; set; }
    public string[] Summary { get; set; }

    public Forecast(string location, Dictionary<DateTime, double> temperatures, string[] summary)
    {
        Location = location;
        Temperatures = temperatures;
        Summary = summary;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Weather: {Location}");
        foreach (var (time, temp) in Temperatures)
        {
            builder.AppendLine($"  {time:dd/MM/yyyy HH:mm} -> {temp:F1}°C");
        }
        builder.AppendLine($"  Summary: {string.Join(", ", Summary)}");
        return builder.ToString();
    }

    public static Forecast Generate(string location)
    {
        string[] summaryPossibilities = ["Hot", "Cold", "Humid", "Windy", "Rain"];
        string[] summary = Random.Shared
            .GetItems(summaryPossibilities, 3)
            .Distinct()
            .ToArray();
        var temperatures = new Dictionary<DateTime, double>();
        DateTime now = DateTime.Now;
        for (int i = 0; i < 24; i++, now += TimeSpan.FromHours(1))
        {
            temperatures[now] = Random.Shared.NextDouble() * 50 - 20;
        }
        return new Forecast(location, temperatures, summary);
    }
}