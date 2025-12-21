using System.Runtime.InteropServices;
using System.Text;

namespace StringsCSharp;

public partial class Program
{
    [LibraryImport("StringsCpp", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void PrintAnsiString(string str);
    
    [LibraryImport("StringsCpp", StringMarshalling = StringMarshalling.Utf16)]
    public static partial void PrintUnicodeString(string str);
    
    [LibraryImport("StringsCpp")]
    public static partial nint GetAnsiString();
    
    [LibraryImport("StringsCpp")]
    public static partial nint GetUnicodeString();

    [LibraryImport("StringsCpp")]
    public static partial void Encode([In, Out] byte[] str);

    static void Main()
    {
        PrintAnsiString("Hello World!");
        PrintUnicodeString("Hello \U0001F30D!");
        
        string? ansi = Marshal.PtrToStringAnsi(GetAnsiString());
        string? unicode = Marshal.PtrToStringUni(GetUnicodeString());
        Console.WriteLine($"Ansi string: {ansi}");
        Console.WriteLine($"Unicode string: {unicode}");
        
        byte[] arr = "Initial content"u8.ToArray();
        Encode(arr);
        Console.WriteLine($"Encoded: {Encoding.UTF8.GetString(arr)}");
    }
}