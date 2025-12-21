using System.Runtime.InteropServices;

namespace HelloCSharp;

public partial class Program
{
    [LibraryImport("HelloCpp")]
    private static partial void Hello([MarshalAs(UnmanagedType.U1)]bool val);
    
    private static void Main()
    {
        Hello();
    }
}