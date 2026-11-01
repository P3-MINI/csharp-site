using System.Runtime.InteropServices;

namespace ImageFilters.Interop;

/// <summary>Functions exported by the operating system libraries.</summary>
internal static partial class OsNative
{
    // ----- Linux and macOS: the C standard library ("libc") -----

    // Classic P/Invoke: the marshalling stub is generated at run time
    [DllImport("libc", EntryPoint = "getpid")]
    internal static extern int GetPidDllImport();

    // Source-generated P/Invoke: the stub is generated at compile time
    [LibraryImport("libc", EntryPoint = "getpid")]
    internal static partial int GetPid();

    internal const int O_RDONLY = 0;

    // int open(const char* path, int flags, ...) - returns -1 and sets errno on failure
    [LibraryImport("libc", EntryPoint = "open", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int Open(string path, int flags);

    [LibraryImport("libc", EntryPoint = "close", SetLastError = true)]
    internal static partial int Close(int fd);

    // ----- Windows: kernel32.dll -----

    [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcessId")]
    internal static extern uint GetCurrentProcessIdDllImport();

    [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentProcessId")]
    internal static partial uint GetCurrentProcessId();

    internal const uint GENERIC_READ = 0x80000000;
    internal const uint FILE_SHARE_READ = 0x00000001;
    internal const uint OPEN_EXISTING = 3;
    internal static readonly nint INVALID_HANDLE_VALUE = -1;

    // HANDLE CreateFileW(...) - returns INVALID_HANDLE_VALUE and sets GetLastError() on failure
    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial nint CreateFile(string fileName, uint desiredAccess, uint shareMode,
        nint securityAttributes, uint creationDisposition, uint flagsAndAttributes, nint templateFile);

    // BOOL is a 4-byte int in WinAPI
    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(nint handle);
}
