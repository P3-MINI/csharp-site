using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileInteraction
{
    internal partial class MyFile
    {
        public MyFile(string? fileName)
            : this(Environment.OSVersion.Platform)
        {
            this.fileName = fileName;
            if (fileName != null)
                Open(fileName);
        }
        public void Open(string fileName) => NativeOpen(fileName);
        public string? Read(int amount) => NativeRead(amount);

        protected string? fileName;
    }
}
