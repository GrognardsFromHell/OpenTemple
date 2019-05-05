using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public class MapSectorSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const bool IsEditor = false;

        [TempleDllLocation(0x10AB73FC)]
        private string _dataDir;

        [TempleDllLocation(0x10AB745C)]
        private string _saveDir;

        public void Dispose()
        {
        }

        [TempleDllLocation(0x100a8590)]
        public void RemoveSectorLight(GameObjectBody handle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fills a given span with the range from tileStart to tileEnd given stride steps.
        /// </summary>
        private static int FillRange(int start, int end, int stride, Span<int> tilesOut)
        {
            var idxOut = 0;
            tilesOut[idxOut++] = start;
            for (var i = (start / stride + 1) * stride; i < end; i += stride)
            {
                tilesOut[idxOut++] = i;
            }

            tilesOut[idxOut++] = end;
            return idxOut;
        }

        [TempleDllLocation(0x10AB747C)]
        private bool _sectorLocking;

        [TempleDllLocation(0x10AB7458)]
        private bool _sectorLockingDisabled;

        private class CachedSector
        {
            public TimePoint Loaded { get; }

            public SectorLoc Loc { get; }

            public Sector Sector { get; }

            public int LockCount { get; set; }

            public CachedSector(SectorLoc loc, Sector sector)
            {
                Loaded = TimePoint.Now;
                Loc = loc;
                Sector = sector;
                LockCount = 0;
            }
        }

        private List<CachedSector> _sectorCache = new List<CachedSector>();

        [TempleDllLocation(0x10082700)]
        public Sector LockSector(SectorLoc loc)
        {
            if (_sectorLocking)
            {
                throw new InvalidOperationException("A sector is already being locked.");
            }

            if (_sectorLockingDisabled)
            {
                return null;
            }

            if (!GameSystems.Map.IsMapOpen())
            {
                throw new InvalidOperationException("Trying to lock a sector when no map is opened.");
            }

            if (loc.X < 0 || loc.Y < 0
                          || loc.X >= GameSystems.Sector.SectorLimitX
                          || loc.Y >= GameSystems.Sector.SectorLimitY)
            {
                return null;
            }

            // Is the same sector already locked or in the cache?
            foreach (var lockedSector in _sectorCache)
            {
                if (lockedSector.Loc == loc)
                {
                    lockedSector.LockCount++;
                    return lockedSector.Sector;
                }
            }

            Sector sector;
            if (!IsEditor)
            {
                sector = LoadSectorGame(loc);
            }
            else
            {
                throw new NotImplementedException();
            }

            // Create a cache entry even for missing sectors to avoid hitting the disk needlessly
            var cacheEntry = new CachedSector(loc, sector);
            _sectorCache.Add(cacheEntry);
            cacheEntry.LockCount++;

            return sector;
        }

        [TempleDllLocation(0x10082b40)]
        public void UnlockSector(SectorLoc loc, Sector sector)
        {
            for (var i = _sectorCache.Count - 1; i >= 0; i--)
            {
                var entry = _sectorCache[i];
                if (entry.Loc == loc && entry.Sector == sector)
                {
                    // TODO: Do not clean up immediately...
                    if (--entry.LockCount == 0)
                    {
                        if (sector != null)
                        {
                            UnloadSector(sector);
                        }

                        _sectorCache.RemoveAt(i);
                    }

                    return;
                }
            }

            throw new InvalidOperationException($"Trying to unlock sector @ {loc}, which wasn't locked!");
        }

        private void UnloadSector(Sector sector)
        {
            // TODO: Save dif / etc.
        }

        [Flags]
        private enum SectorDiffFlag : uint
        {
            Lights = 1
        }

        [TempleDllLocation(0x10083210)]
        private Sector LoadSectorGame(SectorLoc loc)
        {
            var path = Path.Join(_dataDir, loc.Pack() + ".sec");

            if (!Tig.FS.FileExists(path))
            {
                return null;
            }

            using var sectorReader = Tig.FS.OpenBinaryReader(path);
            var diffPath = Path.Join(_saveDir, loc.Pack() + ".dif");
            using var diffReader = File.Exists(diffPath)
                ? new BinaryReader(new FileStream(diffPath, FileMode.Open))
                : null;
            var diffFlags = (SectorDiffFlag) (diffReader?.ReadUInt32() ?? default);

            var sector = new Sector();

            // Load lights
            if (diffFlags.HasFlag(SectorDiffFlag.Lights))
            {
                if (!SectorLoadLightsWithDiff(ref sector.lights, sectorReader, diffReader, loc))
                {
                    Logger.Error("Error loading lights with differences from files {0} and {1}", path, diffPath);
                    return null;
                }
            }
            else if (!SectorLoadLights(ref sector.lights, sectorReader, loc))
            {
                Logger.Error("Error loading lights from sector file {0}", path);
                return null;
            }

            return sector;
        }

        [TempleDllLocation(0x101062f0)]
        private bool SectorLoadLightsWithDiff(ref SectorLights lights, BinaryReader sectorReader,
            BinaryReader diffReader, SectorLoc sectorLoc)
        {
            return false;
        }

        [TempleDllLocation(0x10106160)]
        private bool SectorLoadLights(ref SectorLights lights, BinaryReader reader, SectorLoc sectorLoc)
        {
            var count = reader.ReadInt32();
            if (count == 0)
            {
                lights.enabled = false;
                return true;
            }

            for (var i = 0; i < count; i++)
            {
                ReadLight(reader);
            }

            return false;
        }

        [TempleDllLocation(0x100a6890)]
        private bool ReadLight(BinaryReader reader)
        {

            Span<byte> lightData = stackalloc byte[0x40];
            if (reader.Read(lightData) != lightData.Length)
            {
                return false;
            }

            var light = new SectorLight();

            // The first 8 byte are junk because it's a stale handle
            light.flags = BitConverter.ToInt32(lightData.Slice(8));
            light.type = BitConverter.ToInt32(lightData.Slice(12));
            light.color = new PackedLinearColorA(BitConverter.ToUInt32(lightData.Slice(16)));
            light.field14 = BitConverter.ToInt32(lightData.Slice(20)); // TODO: Probably padding
            light.position = MemoryMarshal.Read<LocAndOffsets>(lightData.Slice(24));
            light.direction = MemoryMarshal.Read<Vector3>(lightData.Slice(44));
            light.range = BitConverter.ToSingle(lightData.Slice(56));
            light.phi = BitConverter.ToSingle(lightData.Slice(60));
            return true;
/*
            auto lightPos = light.position.ToInches3D(light.offsetZ);

            SectorLight *realLight;
            if (light.flags & 0x10) {
                realLight = (SectorLight *)malloc(0x48u);
                memcpy(realLight, &light, 0x40);
                if (tio_fread(&realLight->partSys, 8u, 1u, file) != 1)
                    return 0;

                if (realLight->partSys.hashCode) {
                    realLight->partSys.handle = particles.CreateAt(realLight->partSys.hashCode, lightPos);
                } else {
                    realLight->partSys.handle = 0;
                }
            } else if (light.flags & 0x40) {
                realLight = (SectorLight *)malloc(0xB0u);
                memcpy(realLight, &light, 0x40);
                if (tio_fread(&realLight->partSys, 8u, 1u, file) != 1)
                    return 0;
                if (tio_fread(&realLight->light2, 0x24u, 1u, file) != 1)
                    return 0;

                // reset the handles so whatever the on-disk sector may say, the particle systems are not running
                realLight->partSys.handle = 0;
                realLight->light2.partSys.handle = 0;

                static auto& sIsNight = temple::GetRef<BOOL>(0x10B5DC80);
                SectorLightPartSys *partSys;
                if (sIsNight) {
                    partSys = &realLight->light2.partSys;
                } else {
                    partSys = &realLight->partSys;
                }

                if (partSys->hashCode) {
                    partSys->handle = particles.CreateAt(partSys->hashCode, lightPos);
                }
            } else {
                realLight = (SectorLight *)malloc(0x40u);
                memcpy(realLight, &light, 0x40);
            }
            *lightOut = realLight;
            return 1;*/
        }

        [TempleDllLocation(0x100826b0)]
        public bool IsSectorLoaded(in SectorLoc secLoc)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10082b90)]
        public void Clear()
        {
            // TODO
        }

        [TempleDllLocation(0x10082C00)]
        public void SaveStatics(bool flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10082670)]
        public void SetDirectories(string dataDir, string saveDir)
        {
            _dataDir = dataDir;
            _saveDir = saveDir;
        }
    }
}