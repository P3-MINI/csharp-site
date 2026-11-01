using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImageFilters;
using ImageFilters.Interop;

// Usage: dotnet run [-- image.ppm]
Image input = args.Length > 0 ? Image.Load(args[0]) : Image.CreateTestPattern(640, 360);
Console.WriteLine($"Input: {(args.Length > 0 ? args[0] : "test pattern")} ({input.Width}x{input.Height})");
input.Save("input.ppm");

ShowProcessInfo();
ShowLibraryInfo();
RunNativeFilters(input);
RunManagedFilters(input);
RunNativeImage(input);
Benchmark.Run();

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

static void ShowLibraryInfo()
{
    Console.WriteLine("=== imgfilter library ===");
    Console.WriteLine($"Version:    {ImgFilter.Version}");
    Console.WriteLine($"2 + 3 =     {ImgFilter.Add(2, 3)}");
    Console.WriteLine($"Build info: {ImgFilter.BuildInfo}");
}

static void RunNativeFilters(Image input)
{
    Console.WriteLine("=== Native filters ===");
    Console.WriteLine($"Input stats:     {ImgFilter.ComputeStats(input)}");

    Image inverted = input.Clone();
    ImgFilter.Invert(inverted);
    inverted.Save("inverted.ppm");
    Console.WriteLine($"Inverted stats:  {ImgFilter.ComputeStats(inverted)}");

    Image grayscale = input.Clone();
    ImgFilter.Grayscale(grayscale);
    grayscale.Save("grayscale.ppm");
    Console.WriteLine($"Grayscale stats: {ImgFilter.ComputeStats(grayscale)}");

    // Native code trusts the arguments - the buffer size has to be passed explicitly
    byte[] tooSmall = new byte[10];
    int code = ImgFilterNative.Invert(tooSmall, (nuint)tooSmall.Length, input.Info);
    Console.WriteLine($"Invert with a 10-byte buffer: error {code} - {ImgFilter.ErrorMessage(code)}");
}

static void RunManagedFilters(Image input)
{
    Console.WriteLine("=== Managed filters ===");
    Image native = input.Clone(), safe = input.Clone(), span = input.Clone(), pointers = input.Clone();
    ImgFilter.Grayscale(native);
    ManagedFilters.GrayscaleSafe(safe.Pixels);
    ManagedFilters.GrayscaleSpan(span.Pixels);
    ManagedFilters.GrayscaleUnsafe(pointers.Pixels);
    Console.WriteLine($"Grayscale same as native: safe {Same(safe, native)}, Span {Same(span, native)}, unsafe {Same(pointers, native)}");

    Image blurSafe = ManagedFilters.BoxBlur(input, useUnsafe: false);
    Image blurUnsafe = ManagedFilters.BoxBlur(input, useUnsafe: true);
    Console.WriteLine($"Box blur: safe same as unsafe: {Same(blurSafe, blurUnsafe)}");
    blurUnsafe.Save("blur.ppm");
}

static bool Same(Image a, Image b) => a.Pixels.AsSpan().SequenceEqual(b.Pixels);

static void RunNativeImage(Image input)
{
    Console.WriteLine("=== Native image ===");
    using (NativeImage native = NativeImage.FromImage(input))
    {
        Console.WriteLine($"Created native image {native.Info}, live native images: {ImgFilter.LiveImages}");

        Console.WriteLine("Box blur (native):");
        native.Convolve3x3([1, 1, 1, 1, 1, 1, 1, 1, 1], 9, reportProgress: true);
        Image managed = ManagedFilters.BoxBlur(input, useUnsafe: true);
        Console.WriteLine($"Same as managed box blur: {Same(native.ToImage(), managed)}");

        // Sharpen: negative weights around the center, the weights sum up to 1
        input.Pixels.CopyTo(native.Pixels);
        native.Convolve3x3([0, -1, 0, -1, 5, -1, 0, -1, 0], 1);
        native.ToImage().Save("sharpen.ppm");
    }
    Console.WriteLine($"After Dispose, live native images: {ImgFilter.LiveImages}");

    CreateAndForget();
    Console.WriteLine($"Without Dispose, live native images: {ImgFilter.LiveImages}");
    GC.Collect();
    GC.WaitForPendingFinalizers();
    Console.WriteLine($"After GC, live native images: {ImgFilter.LiveImages}");
}

// Not disposed - the SafeHandle finalizer releases the native image
[MethodImpl(MethodImplOptions.NoInlining)]
static void CreateAndForget() => _ = new NativeImage(100, 100, 3);
