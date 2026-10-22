using System;
using System.Collections.Generic;

namespace Task2;

public readonly record struct Reading(string Sensor, double Value, DateTime At);

public record class Data(IDictionary<string, int> Counts, ISet<string> Sensors, IDictionary<string, double> Averages);

public class SensorLog {
    private readonly List<Reading> _readings = new();

    public void Add(Reading r) => _readings.Add(r);
    public IReadOnlyList<Reading> All => _readings;
}

// =============================================================
// TODO 1 — Write a lazy-evaluated method
//
// Return every reading whose Value is within [min, max] inclusive.
//
// Use `yield` keyword.
//
// =============================================================

public static class T1 {
    public static IEnumerable<Reading> InRange(IEnumerable<Reading> source, double min, double max) {
        throw new NotImplementedException();
    }
}

// =============================================================
// TODO 2 — Write a lazy-evaluated factory
//
// Produce readings from a single sensor, starting at `value`
// and increasing by 0.1/minute, timestamped 2 minute apart starting at `start`.
//
// Use `yield` keyword.
//
// =============================================================

public static class T2 {
    public static IEnumerable<Reading> Repeat(string sensor, double value, int count, DateTime start) {
        throw new NotImplementedException();
    }
}

// =============================================================
// TODO 3 — Implement comparer derieved from Comparer<Reading>
//
// Order readings by:
//   1. Sensor name (ordinal, ascending)
//   2. Then by Value (ascending)
//   3. Then by At (ascending)
//
// In case of nulls, sort them first.
//
// =============================================================

public class ReadingBySensorThenValue : Comparer<Reading> {
    public override int Compare(Reading x, Reading y) {
        throw new NotImplementedException();
    }
}

// =============================================================
// TODO 4 — Summarize sensor data
//
// Return:
// - count of readings per sensor
// - set of distinct sensor names
// - average value per sensor
//
// =============================================================

public static class T4 {
    public static Data Summarize(IEnumerable<Reading> source) {
        throw new NotImplementedException();
    }
}

// =============================================================
// TODO 5 — Pattern matching
//
// `Classify` inspects a list of readings and returns a short label:
//  "empty"              if there are 0 readings
//  "single"             if there is 1 reading
//  "pair-ascending"     if there are exactly 2 and values are ascending (v1 < v2)
//  "pair-descending"    if there are exactly 2 and values are descending (v1 > v2)
//  "pair-equal"         if there are exactly 2 and values are equal
//  "starts-high"        if it starts with a value > 100
//  "ends-low"           if it ends with a value < 0
//  "other"              otherwise
//
// Use slice patterns.
// Do not use Count / IndexOf / if-chains / indices etc.
//
// Hint: use switch expression (https://csharp.mini.pw.edu.pl/en/lectures/03.basics/statements/)
// =============================================================

public static class T5 {
    public static string Classify(IReadOnlyList<Reading> readings) {
        throw new NotImplementedException();
    }
}
