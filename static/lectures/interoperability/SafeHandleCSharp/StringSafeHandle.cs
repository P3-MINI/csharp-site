using System.Runtime.InteropServices;

namespace SafeHandleCSharp;

public class StringSafeHandle : SafeHandle
{
    public StringSafeHandle() : base(nint.Zero, true) {}

    protected override bool ReleaseHandle()
    {
        NativeString.DestroyString(handle);
        handle = nint.Zero;
        return true;
    }

    public override bool IsInvalid => handle == nint.Zero;
}