using Shell.Abstractions;

namespace Shell.Commands;

public record TextStats(int Characters, int Words, int Vowels);

[CommandModule]
[Description("Text manipulation commands.")]
public static class TextCommands
{
    [Command, Description("Converts the text to upper case.")]
    public static string Upper(string text) => text.ToUpperInvariant();

    [Command, Description("Reverses the text.")]
    public static string Reverse(string text)
    {
        char[] chars = text.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    [Command, Description("Repeats the text several times.")]
    public static string Repeat(string text, [Description("how many times")] int times = 2, string separator = " ") =>
        string.Join(separator, Enumerable.Repeat(text, times));

    [Command("wc"), Description("Counts characters, words and vowels.")]
    public static TextStats WordCount(string text) => new(
        Characters: text.Length,
        Words: text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
        Vowels: text.Count(c => "aeiouy".Contains(char.ToLowerInvariant(c))));
}
