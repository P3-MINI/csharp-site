using Microsoft.Win32.SafeHandles;

namespace ImageFilters.Interop;

/// <summary>Owns an <c>imgf_image*</c> and destroys it exactly once.</summary>
public sealed class SafeImageHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    // Called by the marshaller when a native function returns imgf_image*
    public SafeImageHandle() : base(ownsHandle: true)
    {
    }

    protected override bool ReleaseHandle()
    {
        ImgFilterNative.ImageDestroy(handle);
        return true;
    }
}
