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
    internal partial class MyFile : SafeHandleZeroOrMinusOneIsInvalid
    {
        private MyFile(PlatformID platformID) : base(true)
        {
            if (platformID != PlatformID.Win32NT)
                throw new PlatformNotSupportedException();
        }
        private void NativeOpen(string fileName)
        {
            if (!IsInvalid)
                if (!ReleaseHandle())
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            handle = NativeFile.CreateFile(fileName, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None, 0, System.IO.FileMode.Open, 0, 0);
        }
        unsafe private string? NativeRead(int characters)
        {
            string? readString;

            // byte* buf = stackalloc byte[characters];
            var array = ArrayPool<byte>.Shared.Rent(characters);
            fixed (byte* buf = array)
            {
                if (0 == NativeFile.ReadFile(this, buf, characters, out int read, 0))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                readString = Encoding.UTF8.GetString(buf, read);
            }
            ArrayPool<byte>.Shared.Return(array);
            return readString;
        }
        protected override bool ReleaseHandle()
        {
            return NativeFile.CloseHandle(this);
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
            internal static unsafe partial int ReadFile(MyFile handle, byte* bytes,
           int numBytesToRead, out int numBytesRead, IntPtr overlapped_MustBeZero);
            [return: MarshalAs(UnmanagedType.Bool)]
            [LibraryImport("kernel32", SetLastError = true)]
            internal static partial bool CloseHandle(MyFile handle);
        }
    }
}
