using System;
using System.IO;
using System.Linq;
using OpenTemple.Core.IO.WorldMapPaths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ConvertWorldMapPaths;

static class Program
{
    static void Main(string[] args)
    {
        Directory.CreateDirectory("paths");

        var path = args[0];
        using var binaryReader = new BinaryReader(new FileStream(path, FileMode.Open));

        var paths = WorldMapPathReader.Read(binaryReader);

        DumpPathsToImage(paths);
    }

    private static void DumpPathsToImage(WorldMapPath[] paths)
    {
        var width = paths.Select(p => Math.Max(p.Start.X, p.End.X)).Max() + 32;
        var height = paths.Select(p => Math.Max(p.Start.Y, p.End.Y)).Max() + 32;

        using var combinedImage = new Image<Rgb24>(
            Configuration.Default,
            width,
            height
        );

        var red = new Rgb24(255, 0, 0);

        var pathId = 0;
        foreach (var mapPath in paths)
        {
            using var image = new Image<Rgb24>(
                Configuration.Default,
                width,
                height
            );

            var pos = mapPath.Start;
            var x = pos.X;
            var y = pos.Y;

            foreach (var direction in mapPath.Directions)
            {
                switch (direction)
                {
                    case 5:
                        y--;
                        break;
                    case 6:
                        y++;
                        break;
                    case 7:
                        x--;
                        break;
                    case 8:
                        x++;
                        break;
                    case 9:
                        y--;
                        x--;
                        break;
                    case 10:
                        y--;
                        x++;
                        break;
                    case 11:
                        y++;
                        x--;
                        break;
                    case 12:
                        y++;
                        x++;
                        break;
                    case 13:
                        break;
                }

                image[x, y] = red;
                combinedImage[x, y] = red;
            }

            var deltaX = mapPath.End.X - x;
            var deltaY = mapPath.End.Y - y;
            if (deltaX != 0 || deltaY != 0)
            {
                Console.WriteLine($"Error dX={deltaX} dY={deltaY} for Start={mapPath.Start},End={mapPath.End}");
            }

            image.Save($"paths/{pathId}.png");
            pathId++;
        }

        combinedImage.Save($"paths/combined.png");
    }
}