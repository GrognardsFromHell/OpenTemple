using System;
using System.Drawing;
using System.IO;

namespace OpenTemple.Core.IO.WorldMapPaths
{
    public static class WorldMapPathReader
    {
        public static WorldMapPath[] Read(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var result = new WorldMapPath[count];

            for (var i = 0; i < count; i++)
            {
                var start = new Point(reader.ReadInt32(), reader.ReadInt32());
                var end = new Point(reader.ReadInt32(), reader.ReadInt32());
                var dotCount = reader.ReadInt32();

                // Paths in ToEE had a busted offset, we also make them relative to the actual map content
                // rather than the main window here. 12, 10 was determined by just moving the layer around
                // in Gimp until the paths fit the actual map best. ToEE had a hardcoded offset of 14,10
                // applies to all paths.
                const int xOffset = 30 - 12;
                const int yOffset = 26 - 10;
                start.X -= xOffset;
                start.Y -= yOffset;
                end.X -= xOffset;
                end.Y -= yOffset;

                var pathDirections = new byte[dotCount];
                reader.Read(pathDirections);
                // ToEE padded up to 4 byte alignment
                if (dotCount % 4 != 0)
                {
                    for (var j = 0; j < 4 - dotCount % 4; j++)
                    {
                        reader.ReadByte();
                    }
                }
                result[i] = new WorldMapPath(start, end, pathDirections);
            }

            return result;
        }
    }

    public readonly struct WorldMapPath
    {
        public readonly Point Start;
        public readonly Point End;
        public readonly byte[] Directions;

        public WorldMapPath(Point start, Point end, byte[] directions)
        {
            Start = start;
            End = end;
            Directions = directions;
        }
    }
}