using Shell.Abstractions;

namespace Shell.Commands;

[CommandModule]
[Description("Arithmetic commands.")]
public class MathCommands
{
    private decimal _memory;

    [Command, Description("Adds two numbers.")]
    public double Add(double a, double b) => a + b;

    [Command("div"), Description("Divides two integers.")]
    public int Divide(int a, int b) => a / b;

    [Command("pow"), Alias("power"), Description("Raises x to the n-th power.")]
    public double Power(double x, [Description("the exponent")] int n = 2) => Math.Pow(x, n);

    [Command, Alias("r"), Description("Rounds a number to the given number of fractional digits.")]
    public decimal Round(
        decimal value,
        [Description("number of fractional digits")] int digits = 0,
        [Description("how to round midpoint values")] MidpointRounding mode = MidpointRounding.ToEven) =>
        Math.Round(value, digits, mode);

    [Command("avg"), Description("Computes the arithmetic mean.")]
    public static double Average(params double[] values) => values.Length > 0 ? values.Average() : 0;

    [Command, Description("Counts the days between two dates.")]
    public static int Days(DateOnly from, DateOnly to) => to.DayNumber - from.DayNumber;

    [Command("m+"), Description("Adds a value to the memory.")]
    public decimal MemoryAdd(decimal value) => _memory += value;

    [Command("mr"), Description("Recalls the value stored in the memory.")]
    public decimal MemoryRecall() => _memory;
}
