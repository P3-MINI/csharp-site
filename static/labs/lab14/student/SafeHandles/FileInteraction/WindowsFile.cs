using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FileInteraction
{
    internal partial class MyFile
    {
        private MyFile(PlatformID platformID)
        {
            if (platformID != PlatformID.Win32NT)
                throw new PlatformNotSupportedException();
        }

        [SuppressUnmanagedCodeSecurity()]
        private partial class NativeFile
        {
            [LibraryImport("kernel32", EntryPoint = "CreateFileA", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
            internal static partial nint CreateFile(string fileName,
               System.IO.FileAccess dwDesiredAccess, System.IO.FileShare dwShareMode,
               IntPtr securityAttrs_MustBeZero, System.IO.FileMode dwCreationDisposition,
               int dwFlagsAndAttributes, IntPtr hTemplateFile_MustBeZero);
            [LibraryImport("kernel32", SetLastError = true)]
            internal static unsafe partial int ReadFile(/*MyFile*/ IntPtr handle, byte* bytes,
           int numBytesToRead, out int numBytesRead, IntPtr overlapped_MustBeZero);
            [return: MarshalAs(UnmanagedType.Bool)]
            [LibraryImport("kernel32", SetLastError = true)]
            internal static partial bool CloseHandle(/*MyFile*/ IntPtr handle);
        }
    }
}
