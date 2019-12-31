using System;
using System.IO;
using OpenTemple.Core.IO.WorldMapPaths;
using Xunit;

namespace OpenTemple.Tests
{
    public class WorldMapPathReaderTest
    {

        [Fact]
        public void TestReadPaths()
        {
            var pathData = TestData.GetPath("worldmap_ui_paths.bin");

            using var reader = new BinaryReader(new FileStream(pathData, FileMode.Open));

            var paths = WorldMapPathReader.Read(reader);
            Console.WriteLine();
        }

    }
}