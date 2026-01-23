using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Security;
using System.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileInteraction
{
    internal partial class MyFile : SafeHandleMinusOneIsInvalid
    {
        private MyFile(PlatformID platformID) : base(true)
        {
            if (platformID != PlatformID.Unix)
                throw new PlatformNotSupportedException();
        }
        private void NativeOpen(string fileName)
        {
            if (!IsInvalid)
                if (!ReleaseHandle())
                    throw new Exception("");
            handle = NativeFile.open(fileName, 0);
        }
        private string? NativeRead(int characters)
        {
            byte[] buf = ArrayPool<byte>.Shared.Rent(characters);
            int read = NativeFile.read(this, buf, (uint)characters);
            if (read < 0)
                throw new Exception("");
            var ret = Encoding.UTF8.GetString(buf, 0, read);
            ArrayPool<byte>.Shared.Return(buf);
            return ret;
        }
        protected override bool ReleaseHandle()
        {
            return NativeFile.close(this) != 0;
        }

        [SuppressUnmanagedCodeSecurity()]
        private partial class NativeFile
        {
#if OSX
            private const string lib = "libSystem";
#else
            private const string lib = "libc";
#endif
            [LibraryImport(lib, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
            internal static partial nint open(string fileName, int oFlag);
            [LibraryImport(lib, SetLastError = true)]
            internal static partial int read(MyFile fd, byte[] buf, uint nByte);
            [LibraryImport(lib, SetLastError = true)]
            internal static partial int close(MyFile fd);
        }
    }
}
