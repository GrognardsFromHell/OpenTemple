using System;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.FogOfWar
{
    internal class MapFogdataChunk
    {
        public bool AllExplored;

        public byte[] Data = new byte[8192];
    }

    public class MapFoggingSystem : IGameSystem, IBufferResettingSystem, IResetAwareSystem
    {
        private readonly RenderingDevice mDevice;

        private const int sFogBufferDim = 102;

        [TempleDllLocation(0x10824468)]
        internal int mFogMinX;

        [TempleDllLocation(0x108EC4C8)]
        internal int mFogMinY;

        [TempleDllLocation(0x10820458)]
        internal int mSubtilesX;

        [TempleDllLocation(0x10824490)]
        internal int mSubtilesY;

        [TempleDllLocation(0x108254A0)]
        internal bool mFoggingEnabled;

        [TempleDllLocation(0x108A5498)]
        internal byte[] mFogCheckData;

        // 8 entries, one for each controllable party member
        [TempleDllLocation(0x10824470)]
        private byte[][] mFogBuffers;

        [TempleDllLocation(0x11E61560)]
        private MapFogdataChunk[] mFogUnexploredData = new MapFogdataChunk[4];

        // 32 entries
        [TempleDllLocation(0x108EC598)]
        private SectorLoc[] mEsdSectorLocs;

        [TempleDllLocation(0x108EC6B0)]
        private int mEsdLoaded;

        [TempleDllLocation(0x108EC590)]
        private bool mDoFullUpdate;

        [TempleDllLocation(0x102ACEFC)]
        private int mFogChecks;

        [TempleDllLocation(0x108EC6A0)]
        private int mScreenWidth => mScreenSize.Width;

        [TempleDllLocation(0x108EC6A4)]
        private int mScreenHeight => mScreenSize.Height;

        private Size mScreenSize;

        // TODO This seems to be fully unused
        [TempleDllLocation(0x102ACF00)]
        private int fogcol_field1;

        // TODO This is used in the townmap UI
        [TempleDllLocation(0x102ACF04)]
        private int fogcol_field2;

        public FogOfWarRenderer Renderer { get; }

        [TempleDllLocation(0x10032020)]
        public MapFoggingSystem(RenderingDevice renderingDevice)
        {
            mDevice = renderingDevice;

            mFogCheckData = null;

            mFoggingEnabled = true;
            mFogBuffers = new byte[8][];
            for (int i = 0; i < 8; i++)
            {
                mFogBuffers[i] = new byte[16 * sFogBufferDim * sFogBufferDim];
            }

            InitScreenBuffers();

            mEsdLoaded = 0;
            mEsdSectorLocs = new SectorLoc[32];

            Renderer = new FogOfWarRenderer(this, renderingDevice);
        }

        private void InitScreenBuffers()
        {
            mScreenSize = mDevice.GetCamera().ScreenSize;

            UpdateFogLocation();
        }

        private void UpdateFogLocation()
        {
            var camera = mDevice.GetCamera();

            // Calculate the tile locations in each corner of the screen
            var topLeftLoc = camera.ScreenToTile(0, 0);
            var topRightLoc = camera.ScreenToTile(mScreenSize.Width, 0);
            var bottomLeftLoc = camera.ScreenToTile(0, mScreenSize.Height);
            var bottomRightLoc = camera.ScreenToTile(mScreenSize.Width, mScreenSize.Height);

            mFogMinX = topRightLoc.location.locx;
            mFogMinY = topLeftLoc.location.locy;

            // Whatever the point of this may be ...
            if (topLeftLoc.off_y < topLeftLoc.off_x || topLeftLoc.off_y < -topLeftLoc.off_x)
            {
                mFogMinY--;
            }

            mSubtilesX = (bottomLeftLoc.location.locx - mFogMinX + 3) * 3;
            mSubtilesY = (bottomRightLoc.location.locy - mFogMinY + 3) * 3;

            if (mFogCheckData == null || mFogCheckData.Length != mSubtilesX * mSubtilesY)
            {
                mFogCheckData = new byte[mSubtilesX * mSubtilesY];
                mDoFullUpdate = true;
            }
        }

        [TempleDllLocation(0x1002eb80)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x1002ebd0)]
        public void Reset()
        {
            mEsdLoaded = 0;
            mEsdSectorLocs = new SectorLoc[32];
            mDoFullUpdate = false;
        }

        [TempleDllLocation(0x100336B0)]
        public void PerformFogChecks()
        {
            UpdateFogLocation();

            // TODO
        }

        [TempleDllLocation(0x1002ec20)]
        public void ResetBuffers()
        {
            InitScreenBuffers();
        }

        [TempleDllLocation(0x1002ECB0)]
        public byte GetFogStatus(locXY loc, float offsetX, float offsetY)
        {
            return 0xFF;
        }

        public byte GetFogStatus(LocAndOffsets loc) => GetFogStatus(loc.location, loc.off_x, loc.off_y);

        [TempleDllLocation(0x10030f40)]
        public void SaveEsd()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10030e20)]
        public void SaveExploredTileData(int id)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10030BF0)]
        public void LoadFogColor(string dataDir)
        {
            var path = $"{dataDir}/fog.col";
            if (Tig.FS.FileExists(path))
            {
                using var reader = Tig.FS.OpenBinaryReader(path);
                fogcol_field1 = reader.ReadInt32();
                fogcol_field2 = reader.ReadInt32();
            }
        }

        [TempleDllLocation(0x1002ec90)]
        public void Disable()
        {
            // TODO
        }

        [TempleDllLocation(0x1002ec80)]
        public void Enable()
        {
            // TODO
        }

        [TempleDllLocation(0x10030d10)]
        public void LoadExploredTileData(string baseDir)
        {
            if (mFoggingEnabled)
            {
                int idx = 0;
                var otherIdx = 0;
                do
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var fileId = i + otherIdx;
                        var path = $"{baseDir}/etd{fileId:D6}";

                        var unexploredData = new MapFogdataChunk();
                        if (Tig.FS.FileExists(path))
                        {
                            using var reader = Tig.FS.OpenBinaryReader(path);
                            unexploredData.AllExplored = reader.ReadByte() != 0;
                            var unexploredRaw = unexploredData.Data.AsSpan();
                            if (unexploredData.AllExplored)
                            {
                                unexploredRaw.Fill(0xFF);
                            }
                            else
                            {
                                if (reader.Read(unexploredRaw) != unexploredRaw.Length)
                                {
                                    throw new InvalidOperationException("Failed to read unexplored sector data.");
                                }
                            }
                        }
                        else
                        {
                            unexploredData.AllExplored = false;
                        }

                        mFogUnexploredData[idx] = unexploredData;
                        ++i;
                        ++idx;
                    }

                    otherIdx += 1000;
                } while (idx < 4);
            }
        }

        [TempleDllLocation(0x1002eca0)]
        public void SetMapDoFoggingUpdate()
        {
            throw new NotImplementedException();
        }
    }
}