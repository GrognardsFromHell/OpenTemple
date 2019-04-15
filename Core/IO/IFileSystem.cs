using System.Buffers;
using System.IO;

namespace SpicyTemple.Core.IO
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        string ReadTextFile(string path);

        BinaryReader OpenBinaryReader(string path);

        byte[] ReadBinaryFile(string path);

        IMemoryOwner<byte> ReadFile(string path);
    }
}