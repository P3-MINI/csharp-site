using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Task2;

public class Program {
    public static void Main() {
        var t0 = new DateTime(2026, 10, 10, 10, 0, 0);
        var log = new SensorLog();
        log.LoadFromCsv("..\\..\\..\\readings.csv");

        // =============================================================

        Console.WriteLine("--- T1 ---");
        Console.WriteLine("Expected:\r\n  hum=50\r\n  tmp=12,5\r\n  tmp=15\r\n  hum=48,5\r\n  hum=45");
        Console.WriteLine("Received:");

        foreach (var r in T1.InRange(log.All, 10, 50.0)) {
            Console.WriteLine($"  {r.Sensor}={r.Value}");
        }
        Console.WriteLine();

        // =============================================================

        Console.WriteLine("--- T2 ---");
        Console.WriteLine("Expected:\r\n  tmp=1,10@10:00\r\n  tmp=1,30@10:02\r\n  tmp=1,50@10:04\r\n  tmp=1,70@10:06\r\n  tmp=1,90@10:08");
        Console.WriteLine("Received:");

        int maxCount = 5;
        int count = 0;
        foreach (var r in T2.Repeat("tmp", 1.1, 3, t0)) {
            if (count++ >= maxCount) { break; }
            Console.WriteLine($"  {r.Sensor}={r.Value:F2}@{r.At:HH:mm}");
        }
        Console.WriteLine();

        // =============================================================

        Console.WriteLine("--- T3 ---");
        Console.WriteLine("Expected:\r\n  hum=55,00@00:31\r\n  hum=55,00@00:58\r\n  hum=52,00@01:26\r\n  hum=50,00@00:07\r\n  hum=48,50@01:53\r\n  hum=45,00@02:22");
        Console.WriteLine("Received:");

        var sorted = new List<Reading>(log.All);
        sorted.Sort(new ReadingBySensorThenValue());
        foreach (var r in sorted) {
            if (r.Sensor != "hum") { continue; }
            Console.WriteLine($"  {r.Sensor}={r.Value:F2}@{r.At:HH:mm}");
        }
        Console.WriteLine();

        // =============================================================

        Console.WriteLine("--- T4 ---");
        Console.WriteLine("Expected:");
        Console.WriteLine(
            "  HighestValue:     900\r\n  LowestTemp:       -5,5\r\n  MostReadings:     tmp\r\n  TotalDuration:    03:27:40\r\n" +
            "  UniqueSensors:    co2, hum, pres, tmp\r\n  AverageReadings:\r\n    co2  660,00\r\n    hum  50,92\r\n    pres 104,23\r\n    tmp  2,54\r\n" +
            "  LargestGaps:\r\n    co2  34min 7s\r\n    hum  29min 4s\r\n    pres 47min 14s\r\n    tmp  27min 56s\r\n  SignChanges:\r\n    tmp: 2,50@02:28:44 -> -3,00@02:48:26"
        );
        Console.WriteLine("Received:");

        var data = T4.Summarize(log.All);


        Console.WriteLine($"  HighestValue:     {data.HighestValue}");
        Console.WriteLine($"  LowestTemp:       {data.LowestTemp}");
        Console.WriteLine($"  MostReadings:     {data.MostReadings}");
        Console.WriteLine($"  TotalDuration:    {data.TotalDuration}");
        Console.WriteLine($"  UniqueSensors:    {string.Join(", ", data.UniqueSensors)}");
        Console.WriteLine("  AverageReadings:");
        foreach (var (sensor, avg) in data.AverageReadings) {
            Console.WriteLine($"    {sensor,-4} {avg:F2}");
        }
        Console.WriteLine("  LargestGaps:");
        foreach (var (sensor, gap) in data.LargestGaps) {
            Console.WriteLine($"    {sensor,-4} {gap.Minutes}min {gap.Seconds}s");
        }
        Console.WriteLine("  SignChanges:");
        if (data.SignChanges.Count == 0) {
            Console.WriteLine("    (none)");
        } else {
            foreach (var (prev, next) in data.SignChanges) {
                Console.WriteLine($"    {prev.Sensor}: {prev.Value:F2}@{prev.At:HH:mm:ss} -> {next.Value:F2}@{next.At:HH:mm:ss}");
            }
        }
        Console.WriteLine();

        // =============================================================

        Console.WriteLine("--- T5 ---");

        void Show(string label, IReadOnlyList<Reading> readings) => Console.WriteLine($"  {label,-15}{T5.Classify(readings)}");

        Console.WriteLine($"  {"Expected:",-15}{"Received:"}");
        Show("empty", []);
        Show("single", [new("a", 1.0, t0)]);
        Show("pair-asc", [new("a", 1.0, t0), new("a", 2.0, t0)]);
        Show("pair-desc", [new("a", 2.0, t0), new("a", 1.0, t0)]);
        Show("pair-eq", [new("a", 1.0, t0), new("a", 1.0, t0)]);
        Show("starts-high", [new("a", 150.0, t0), new("a", 10.0, t0)]);
        Show("ends-low", [new("a", 10.0, t0), new("a", -5.0, t0)]);
        Show("other", [new("a", 10.0, t0), new("a", 20.0, t0), new("a", 30.0, t0)]);

        // =============================================================
    }
}