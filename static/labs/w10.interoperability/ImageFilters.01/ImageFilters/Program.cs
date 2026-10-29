using System.Runtime.InteropServices;
using ImageFilters;
using ImageFilters.Interop;

// Usage: dotnet run [-- image.ppm]
Image input = args.Length > 0 ? Image.Load(args[0]) : Image.CreateTestPattern(640, 360);
Console.WriteLine($"Input: {(args.Length > 0 ? args[0] : "test pattern")} ({input.Width}x{input.Height})");
input.Save("input.ppm");

ShowProcessInfo();

static void ShowProcessInfo()
{
    Console.WriteLine("=== Operating system ===");
    // Functions of the other operating system are declared, but never called
    int pid = OperatingSystem.IsWindows() ? (int)OsNative.GetCurrentProcessId() : OsNative.GetPid();
    int pidDllImport = OperatingSystem.IsWindows() ? (int)OsNative.GetCurrentProcessIdDllImport() : OsNative.GetPidDllImport();
    Console.WriteLine($"Process ID: {pid} (LibraryImport), {pidDllImport} (DllImport), {Environment.ProcessId} (Environment)");

    TryOpenFile("input.ppm");
    TryOpenFile("missing.ppm");
}

static void TryOpenFile(string path)
{
    if (OperatingSystem.IsWindows())
    {
        nint handle = OsNative.CreateFile(path, OsNative.GENERIC_READ, OsNative.FILE_SHARE_READ, 0, OsNative.OPEN_EXISTING, 0, 0);
        if (handle == OsNative.INVALID_HANDLE_VALUE)
        {
            PrintLastError($"CreateFileW(\"{path}\")");
            return;
        }
        Console.WriteLine($"CreateFileW(\"{path}\") succeeded, handle = 0x{handle:X}");
        OsNative.CloseHandle(handle);
    }
    else
    {
        int fd = OsNative.Open(path, OsNative.O_RDONLY);
        if (fd == -1)
        {
            PrintLastError($"open(\"{path}\")");
            return;
        }
        Console.WriteLine($"open(\"{path}\") succeeded, fd = {fd}");
        OsNative.Close(fd);
    }
}

static void PrintLastError(string call)
{
    // errno (Unix) or GetLastError() (Windows), saved right after the call thanks to SetLastError = true
    int error = Marshal.GetLastPInvokeError();
    Console.WriteLine($"{call} failed: error {error} - {Marshal.GetPInvokeErrorMessage(error)}");
}
