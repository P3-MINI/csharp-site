//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Globalization;

//namespace Task2;

//public readonly record struct Reading(string Sensor, double Value, DateTime At);
//public record class Data(
//    double HighestValue,                        // highest recorded value across all sensors
//    double LowestTemp,                          // lowest recorded temperature
//    string MostReadings,                        // name of the sensor with most readings
//    TimeSpan TotalDuration,                     // difference between first and last recorded reading
//    IList<string> UniqueSensors,                // list of all sensor names without duplicates
//    IList<(string, double)> AverageReadings,    // average reading per sensor
//    IList<(string, TimeSpan)> LargestGaps,      // a largest timespan between two consecutive readings (time order) per sensor
//    IList<(Reading, Reading)> SignChanges       // two consecutive readings (time order) having two differnet signs (first < second) 
//);

//public class SensorLog {
//    private readonly List<Reading> _readings = new();
//    public void Add(Reading r) { _readings.Add(r); }
//    public IReadOnlyList<Reading> All => _readings;

//    public SensorLog LoadFromCsv(string path) {
//        bool first = true;
//        foreach (var line in File.ReadLines(path)) {
//            if (first) { first = false; continue; } // assuming header            
//            if (string.IsNullOrWhiteSpace(line)) {
//                continue;
//            }
//            var parts = line.Split(',');
//            Add(new Reading(parts[0].Trim(), double.Parse(parts[1], CultureInfo.InvariantCulture), DateTime.Parse(parts[2], CultureInfo.InvariantCulture)));
//        }
//        return this;
//    }
//}

//public static class T1 {
//    public static IEnumerable<Reading> InRange(IEnumerable<Reading> source, double min, double max) {
//        foreach (var r in source) {
//            if (r.Value >= min && r.Value <= max) {
//                yield return r;
//            }
//        }
//    }
//}

//public static class T2 {
//    public static IEnumerable<Reading> Repeat(string sensor, double value, int count, DateTime start) {
//        for (int i = 0; i >= 0; i += 2) {
//            yield return new Reading(sensor, value + i * 0.1, start.AddMinutes(i));
//        }
//    }
//}

//public class ReadingBySensorThenValue : Comparer<Reading> {
//    public override int Compare(Reading a, Reading b) {
//        int c = a.Sensor.CompareTo(b.Sensor);
//        if (c != 0) {
//            return c;
//        }
//        c = b.Value.CompareTo(a.Value);
//        if (c != 0) {
//            return c;
//        }
//        return a.At.CompareTo(b.At);
//    }
//}

//public static class T4 {
//    public class ReadingByTime : Comparer<Reading> {
//        public override int Compare(Reading a, Reading b) {
//            if (Equals(a, b)) {
//                return 0;
//            }     
//            return a.At.CompareTo(b.At);
//        }
//    }

//    public static Data Summarize(IEnumerable<Reading> source) {
//        List<Reading> sortedReadings = [.. source];
//        sortedReadings.Sort(new ReadingByTime());

//        var sensors = new HashSet<string>();
//        var counts = new Dictionary<string, int>();
//        var sums = new Dictionary<string, double>();
//        double highest = double.NegativeInfinity;
//        double lowestTemp = double.PositiveInfinity;

//        foreach (var r in sortedReadings) {
//            sensors.Add(r.Sensor);
//            counts[r.Sensor] = counts.TryGetValue(r.Sensor, out var c) ? c + 1 : 1;
//            sums[r.Sensor] = sums.TryGetValue(r.Sensor, out var s) ? s + r.Value : r.Value;

//            if (r.Value > highest) {
//                highest = r.Value;
//            }
//            if (r.Sensor == "tmp" && r.Value < lowestTemp) {
//                lowestTemp = r.Value;
//            }
//        }

//        List<string> sortedSensors = [.. sensors];
//        sortedSensors.Sort();

//        string mostReadings = "";
//        int best = -1;
//        foreach (var sensor in sortedSensors) {
//            if (counts[sensor] > best) {
//                best = counts[sensor];
//                mostReadings = sensor;
//            }
//        }

//        var totalDuration = sortedReadings[sortedReadings.Count - 1].At - sortedReadings[0].At;

//        var maxGap = new Dictionary<string, TimeSpan>();
//        var lastRead = new Dictionary<string, Reading>();
//        var signChanges = new List<(Reading, Reading)>();

//        foreach (var r in sortedReadings) {
//            if (lastRead.TryGetValue(r.Sensor, out var prev)) {
//                var gap = r.At - prev.At;
//                if (!maxGap.TryGetValue(r.Sensor, out var current) || gap > current) {
//                    maxGap[r.Sensor] = gap;
//                }

//                if ((prev.Value < 0 && r.Value > 0) || (prev.Value > 0 && r.Value < 0)) {
//                    signChanges.Add((prev, r));
//                }
//            }
//            lastRead[r.Sensor] = r;
//        }

//        var averages = new List<(string, double)>(sortedSensors.Count);
//        var gaps = new List<(string, TimeSpan)>();
//        foreach (var sensor in sortedSensors) {
//            averages.Add((sensor, sums[sensor] / counts[sensor]));
//            if (maxGap.TryGetValue(sensor, out var g))
//                gaps.Add((sensor, g));
//        }

//        return new Data(
//            highest,
//            lowestTemp,
//            mostReadings,
//            totalDuration,
//            sortedSensors,
//            averages,
//            gaps,
//            signChanges);
//    }
//}


//public static class T5 {
//    public static string Classify(IReadOnlyList<Reading> readings) {
//        return readings switch {
//            [] => "empty",
//            [_] => "single",
//            [var first, ..] when first.Value > 100 => "starts-high",
//            [.., var last] when last.Value < 0 => "ends-low",
//            [var a, var b] when a.Value < b.Value => "pair-asc",
//            [var a, var b] when a.Value > b.Value => "pair-desc",
//            [var a, var b] when a.Value == b.Value => "pair-eq",
//            _ => "other",
//        };
//    }
//}