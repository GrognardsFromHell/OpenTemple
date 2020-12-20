using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.TownMap
{
    /// <summary>
    /// Static data associated with a townmap.
    /// </summary>
    public class TownMapData : IDisposable
    {
        [TempleDllLocation(0x10be1f54)]
        [TempleDllLocation(0x10be1f58)]
        [TempleDllLocation(0x10be1f60)]
        [TempleDllLocation(0x10be1f5c)]
        public Rectangle WorldRectangle { get; }

        // 81 entries (9x9)
        [TempleDllLocation(0x11E69A60)]
        public ResourceRef<ITexture>[,] SmallTextures { get; }

        // 306 entries (17x18)
        [TempleDllLocation(0x11E69580)]
        public ResourceRef<ITexture>[,] BigTextures { get; }

        public TownMapData(Rectangle worldRectangle,
            ResourceRef<ITexture>[,] smallTextures,
            ResourceRef<ITexture>[,] bigTextures)
        {
            WorldRectangle = worldRectangle;
            SmallTextures = smallTextures;
            BigTextures = bigTextures;
        }

        public static TownMapData LoadFromFolder(string dataFolder)
        {
            var bigTextures = new ResourceRef<ITexture>[17, 18];
            for (var y = 0; y < 18; y++)
            {
                for (var x = 0; x < 17; x++)
                {
                    var filename = $"{dataFolder}/z4{y:D3}{x:D3}.jpg";
                    if (Tig.FS.FileExists(filename))
                    {
                        bigTextures[x, y] = Tig.Textures.Resolve(filename, false);
                    }
                }
            }

            var smallTextures = new ResourceRef<ITexture>[9, 9];
            for (var y = 0; y < 9; y++)
            {
                for (var x = 0; x < 9; x++)
                {
                    var filename = $"{dataFolder}/z8{y:D3}{x:D3}.jpg";
                    if (Tig.FS.FileExists(filename))
                    {
                        smallTextures[x, y] = Tig.Textures.Resolve(filename, false);
                    }
                }
            }

            var mapInfoFilename = $"{dataFolder}/map.dat";
            using var reader = Tig.FS.OpenBinaryReader(mapInfoFilename);
            var worldRect = new Rectangle(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32()
            );

            return new TownMapData(
                worldRect,
                smallTextures,
                bigTextures
            );
        }

        public void Dispose()
        {
            foreach (var resourceRef in BigTextures)
            {
                resourceRef.Dispose();
            }

            foreach (var resourceRef in SmallTextures)
            {
                resourceRef.Dispose();
            }
        }
    }
}