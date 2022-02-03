using System;
using System.IO;
using OpenTemple.Core.IO.WorldMapPaths;
using OpenTemple.Tests.TestUtils;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class WorldMapPathReaderTest
{

    [Test]
    public void TestReadPaths()
    {
        var pathData = TestData.GetPath("worldmap_ui_paths.bin");

        using var reader = new BinaryReader(new FileStream(pathData, FileMode.Open));

        var paths = WorldMapPathReader.Read(reader);
        Console.WriteLine();
    }

}