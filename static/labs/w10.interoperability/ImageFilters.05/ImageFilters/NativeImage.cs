using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImageFilters.Interop;

namespace ImageFilters;

/// <summary>Image whose pixels live in native memory owned by the imgfilter library.</summary>
public sealed unsafe class NativeImage : IDisposable
{
    private readonly SafeImageHandle _handle;

    public ImageInfo Info { get; }

    public NativeImage(int width, int height, int channels)
    {
        _handle = ImgFilterNative.ImageCreate(width, height, channels);
        if (_handle.IsInvalid)
            throw new ArgumentException($"Cannot create a native image {width}x{height}x{channels}");
        Info = ImgFilterNative.ImageGetInfo(_handle);
    }

    /// <summary>View of the native pixel buffer - valid only until the image is disposed.</summary>
    public Span<byte> Pixels
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle.IsClosed, this);
            byte* pixels = ImgFilterNative.ImageGetPixels(_handle);
            return new Span<byte>(pixels, Info.Width * Info.Height * Info.Channels);
        }
    }

    public static NativeImage FromImage(Image image)
    {
        var native = new NativeImage(image.Width, image.Height, Image.Channels);
        image.Pixels.CopyTo(native.Pixels);
        return native;
    }

    public Image ToImage()
    {
        var image = new Image(Info.Width, Info.Height);
        Pixels.CopyTo(image.Pixels);
        return image;
    }

    /// <summary>Applies a 3x3 convolution in native code, optionally reporting progress.</summary>
    public void Convolve3x3(ReadOnlySpan<int> kernel, int divisor, bool reportProgress = false)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(kernel.Length, 9);
        delegate* unmanaged[Cdecl]<int, int, void> progress = reportProgress ? &ReportProgress : null;
        ImgFilter.Check(ImgFilterNative.ImageConvolve3x3(_handle, kernel, divisor, progress));
    }

    // Called from native code: static, blittable parameters only, must not throw
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void ReportProgress(int doneRows, int totalRows)
    {
        int step = Math.Max(1, totalRows / 4);
        if (doneRows % step == 0 || doneRows == totalRows)
            Console.WriteLine($"  progress: {100 * doneRows / totalRows,3}% ({doneRows}/{totalRows} rows)");
    }

    public void Dispose() => _handle.Dispose();
}
