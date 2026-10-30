using System.Globalization;

namespace Task2;
public readonly record struct Reading(string Sensor, double Value, DateTime At);
public record class Data(
    double HighestValue,                        // highest recorded value across all sensors
    double LowestTemp,                          // lowest recorded temperature
    string MostReadings,                        // name of the sensor with most readings
    TimeSpan TotalDuration,                     // difference between first and last recorded reading
    IList<string> UniqueSensors,                // list of all sensor names without duplicates
    IList<(string, double)> AverageReadings,    // average reading per sensor
    IList<(string, TimeSpan)> LargestGaps,      // the largest timespan between two consecutive readings (time order) per sensor
    IList<(Reading, Reading)> SignChanges       // two consecutive readings (time order) having two different signs (first < second) 
);

public class SensorLog {
    private readonly List<Reading> _readings = new();
    public void Add(Reading r) { _readings.Add(r); }
    public IReadOnlyList<Reading> All => _readings;

    public SensorLog LoadFromCsv(string path) {
        bool first = true;
        foreach (var line in File.ReadLines(path)) {
            if (first) { first = false; continue; } // assuming header            
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }
            var parts = line.Split(',');
            Add(new Reading(parts[0].Trim(), double.Parse(parts[1], CultureInfo.InvariantCulture), DateTime.Parse(parts[2], CultureInfo.InvariantCulture)));
        }
        return this;
    }
}

// =============================================================
// TODO 1
// =============================================================

public static class T1 {
    public static IEnumerable<Reading> InRange(IEnumerable<Reading> source, double min, double max) {
        throw new NotImplementedException();
    }
}

// =============================================================
// TODO 2 
// =============================================================

public static class T2 {
    public static IEnumerable<Reading> Repeat(string sensor, double value, int count, DateTime start) {
        throw new NotImplementedException();
    }
}

// =============================================================
// TODO 3
// =============================================================

public class ReadingBySensorThenValue : Comparer<Reading> {
    public override int Compare(Reading x, Reading y) {
        throw new NotImplementedException();
    }
}

// =============================================================
// TODO 4 
// =============================================================

public static class T4 {
    public static Data Summarize(IEnumerable<Reading> source) {
        throw new NotImplementedException();
    }
}

// =============================================================
// TODO 5 
// =============================================================

public static class T5 {
    public static string Classify(IReadOnlyList<Reading> readings) {
        throw new NotImplementedException();
    }
}