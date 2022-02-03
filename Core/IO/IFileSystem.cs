using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

#nullable enable

namespace OpenTemple.Core.IO;

public interface IFileSystem
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    string ReadTextFile(string path);

    ISet<string> ListDirectory(string path);

    BinaryReader OpenBinaryReader(string path);

    TextReader OpenTextReader(string path, Encoding encoding);

    byte[] ReadBinaryFile(string path);

    IMemoryOwner<byte> ReadFile(string path);

    bool TryGetRealPath(string path, [NotNullWhen(true)] out string? realPath);
}