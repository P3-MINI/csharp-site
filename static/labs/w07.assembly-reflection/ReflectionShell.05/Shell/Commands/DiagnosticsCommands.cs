using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Shell.Abstractions;

namespace Shell.Commands;

[CommandModule]
[Description("Diagnostic commands.")]
public static class DiagnosticsCommands
{
    [Command, Description("Describes the members of a type.")]
    public static string Inspect([Description("full or short name of the type")] string typeName)
    {
        Type? type = Type.GetType(typeName)
            ?? AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName);
        return type is null ? $"Type '{typeName}' not found." : TypeInspector.Describe(type);
    }

    [Command, Description("Compares the speed of direct and reflective calls.")]
    public static string Bench(int iterations = 1_000_000)
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        MethodInfo method = typeof(DiagnosticsCommands).GetMethod(nameof(Twice), flags)!;
        MethodInvoker invoker = MethodInvoker.Create(method);
        Func<int, int> twice = method.CreateDelegate<Func<int, int>>();

        var sb = new StringBuilder();
        Measure("Direct call", i => Twice(i));
        Measure("Delegate", i => twice(i));
        Measure("MethodInvoker", i => (int)invoker.Invoke(null, i)!);
        Measure("MethodInfo.Invoke", i => (int)method.Invoke(null, [i])!);
        Measure("GetMethod + Invoke", i => (int)typeof(DiagnosticsCommands).GetMethod(nameof(Twice), flags)!.Invoke(null, [i])!);
        return sb.ToString();

        void Measure(string name, Func<int, int> call)
        {
            long sum = 0;
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                sum += call(i);
            }
            stopwatch.Stop();
            double nanoseconds = stopwatch.Elapsed.TotalNanoseconds / iterations;
            sb.AppendLine($"{name,-20} {stopwatch.Elapsed.TotalMilliseconds,8:F1} ms {nanoseconds,8:F1} ns/call");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Twice(int x) => 2 * x;
}
