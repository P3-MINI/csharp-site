using System.Globalization;
using Wrapped;

// Format numbers and dates the same way on every machine
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var library = MusicLibrary.Load(Path.Combine(AppContext.BaseDirectory, "Data"));

// Deferred: every query in the report runs the join again
int created = 0;
IEnumerable<PlayInfo> deferred = library.GetPlayInfos().Tap(_ => created++);
string report = WrappedReport.Build(deferred, 2025);
int createdDeferred = created;

// Materialized: the join runs once, the report reads a list
created = 0;
List<PlayInfo> materialized = library.GetPlayInfos().Tap(_ => created++).ToList();
string sameReport = WrappedReport.Build(materialized, 2025);

Console.WriteLine(report);
Console.WriteLine();
Console.WriteLine($"PlayInfo objects created: deferred {createdDeferred:N0}, materialized {created:N0}");
Console.WriteLine($"Identical reports: {report == sameReport}");
