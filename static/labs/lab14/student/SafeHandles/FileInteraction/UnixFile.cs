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
    internal partial class MyFile
    {
        private MyFile(PlatformID platformID)
        {
            if (platformID != PlatformID.Unix)
                throw new PlatformNotSupportedException();
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
