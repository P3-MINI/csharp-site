//using System;
//using System.Collections.Generic;

//namespace Task2;

//public readonly record struct Reading(string Sensor, double Value, DateTime At);
//public record class Data(IDictionary<string, int> Counts, ISet<string> Sensors, IDictionary<string, double> Averages);

//public class SensorLog {
//    private readonly List<Reading> _readings = new();
//    public void Add(Reading r) { _readings.Add(r); }
//    public IReadOnlyList<Reading> All => _readings;
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
//        int c = string.CompareOrdinal(a.Sensor, b.Sensor);
//        if (c != 0) {
//            return c;
//        }
//        c = a.Value.CompareTo(b.Value);
//        if (c != 0) {
//            return c;
//        }
//        return a.At.CompareTo(b.At);
//    }
//}

//public static class T4 {
//    public static Data Summarize(IEnumerable<Reading> source) {
//        var counts = new Dictionary<string, int>();
//        var sensors = new HashSet<string>();
//        var sums = new Dictionary<string, double>();

//        foreach (var r in source) {
//            sensors.Add(r.Sensor);
//            counts[r.Sensor] = counts.GetValueOrDefault(r.Sensor) + 1;
//            sums[r.Sensor] = sums.GetValueOrDefault(r.Sensor) + r.Value;
//        }

//        var averages = new Dictionary<string, double>();
//        foreach (var (sensor, sum) in sums) {
//            averages[sensor] = sum / counts[sensor];
//        }
//        return new Data(counts, sensors, averages);
//    }
//}


//public static class T5 {
//    public static string Classify(IReadOnlyList<Reading> readings) {
//        return readings switch {
//            [] => "empty",
//            [_] => "single",
//            [var first, ..] when first.Value > 100 => "starts-high",
//            [.., var last] when last.Value < 0 => "ends-low",
//            [var a, var b] when a.Value < b.Value => "pair-ascending",
//            [var a, var b] when a.Value > b.Value => "pair-descending",
//            [var a, var b] when a.Value == b.Value => "pair-equal",
//            _ => "other",
//        };
//    }
//}