using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Task2;

public class Program {
    public static void Main() {
        var t0 = new DateTime(2026, 10, 10, 10, 0, 0);

        var log = new SensorLog();
        log.Add(new Reading("tmp", 20.5, t0));
        log.Add(new Reading("tmp", 21.0, t0.AddMinutes(1)));
        log.Add(new Reading("tmp", 19.0, t0.AddMinutes(2)));
        log.Add(new Reading("hum", 50.0, t0.AddMinutes(3)));
        log.Add(new Reading("hum", 55.0, t0.AddMinutes(4)));
        log.Add(new Reading("pres", 150.0, t0.AddMinutes(5)));

        // =============================================================

        Console.WriteLine("--- T1 ---");
        Console.WriteLine("Expected:\r\n  tmp=20,5\r\n  tmp=21\r\n  hum=50\r\n  hum=55");
        Console.WriteLine("Received:");

        foreach (var r in T1.InRange(log.All, 20.0, 60.0)) {
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
        Console.WriteLine("Expected:\r\n  hum 50\r\n  hum 55\r\n  pres 150\r\n  tmp 19\r\n  tmp 20,5\r\n  tmp 21");
        Console.WriteLine("Received:");

        var sorted = new List<Reading>(log.All);
        sorted.Sort(new ReadingBySensorThenValue());
        foreach (var r in sorted) {
            Console.WriteLine($"  {r.Sensor} {r.Value}");
        }
        Console.WriteLine();

        // =============================================================

        Console.WriteLine("--- T4 ---");
        Console.WriteLine("Expected:\r\n  distinct sensors: tmp, hum, pres\r\n  tmp: count=3, avg=20,17\r\n  hum: count=2, avg=52,50\r\n  pres: count=1, avg=150,00");
        Console.WriteLine("Received:");

        var (counts, sensors, averages) = T4.Summarize(log.All);
        Console.WriteLine($"  distinct sensors: {string.Join(", ", sensors)}");
        foreach (var (sensor, n) in counts) {
            Console.WriteLine($"  {sensor}: count={n}, avg={averages[sensor]:F2}");
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