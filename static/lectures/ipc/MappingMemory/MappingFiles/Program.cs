using System.IO.MemoryMappedFiles;
using System.Text;

long fileSize = 1024;
string filePath = "test.dat";
string message = "Hello, world!";

using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Create, null, fileSize);
using (var accessor = mmf.CreateViewAccessor(0, message.Length * 2, MemoryMappedFileAccess.Write))
{
    byte[] buffer = Encoding.Unicode.GetBytes(message);
    accessor.WriteArray(0, buffer, 0, buffer.Length);
}

using (var accessor = mmf.CreateViewAccessor(0, message.Length * 2, MemoryMappedFileAccess.Read))
{
    byte[] buffer = new byte[message.Length * 2];
    accessor.ReadArray(0, buffer, 0, buffer.Length);
    string readMessage = Encoding.Unicode.GetString(buffer);
    Console.WriteLine(readMessage);
}