using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.MapSector
{
    public class MapSectorSystem : IGameSystem, IBufferResettingSystem, IResetAwareSystem, IMapCloseAwareGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const bool IsEditor = false;

        public bool RenderDebugInfo { get; set; }

        [TempleDllLocation(0x10AB73FC)]
        private string _dataDir;

        [TempleDllLocation(0x10AB745C)]
        private string _saveDir;

        public void Dispose()
        {
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

            _sectorLocking = true;
            try
            {
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
            finally
            {
                _sectorLocking = false;
            }
        }

        [TempleDllLocation(0x10082b40)]
        public void UnlockSector(SectorLoc loc, Sector sector)
        {
            for (var i = _sectorCache.Count - 1; i >= 0; i--)
            {
                var entry = _sectorCache[i];
                if (entry.Loc == loc && entry.Sector == sector)
                {
                    --entry.LockCount;
                    return;
                }
            }

            throw new InvalidOperationException($"Trying to unlock sector @ {loc}, which wasn't locked!");
        }

        [TempleDllLocation(0x10081e10)]
        private void UnloadSector(Sector sector)
        {
            SectorFreeLights(ref sector.lights);
            sector.townmapInfo = 0;
            sector.aptitudeAdj = 0;
            sector.lightScheme = 0;

            sector.objects.Dispose();

        }

        [TempleDllLocation(0x10105ca0)]
        private void SectorFreeLights(ref SectorLights lights)
        {

            Span<SectorLight> lightSpan = lights.list;
            foreach (ref var sectorLight in lightSpan)
            {
                if (sectorLight.obj == null)
                {
                    UnloadStaticLight(ref sectorLight);
                }
                else
                {
                    UnloadDynamicLight(ref sectorLight);
                }
            }

            lights.dirty = false;
            lights.list = null;
        }

        [TempleDllLocation(0x100a80d0)]
        [TempleDllLocation(0x100a7ba0)]
        private void UnloadStaticLight(ref SectorLight a1)
        {

            GameSystems.TimeEvent.Remove(TimeEventType.Light, evt =>
            {
                // TODO: Check that the event is for the sectorlight
                return false;
            });

            if (a1.partSys.handle != null)
            {
                GameSystems.ParticleSys.Remove(a1.partSys.handle);
                a1.partSys.handle = null;
            }
            if (a1.light2.partSys.handle != null)
            {
                GameSystems.ParticleSys.Remove(a1.light2.partSys.handle);
                a1.light2.partSys.handle = null;
            }

        }

        [TempleDllLocation(0x100a7fe0)]
        private void UnloadDynamicLight(ref SectorLight a1)
        {

            GameSystems.TimeEvent.Remove(TimeEventType.Light, evt =>
            {
                // TODO: Check that the event is for the sectorlight
                return false;
            });

        }

        [Flags]
        private enum SectorDiffFlag : uint
        {
            Lights = 0x01,
            Objects = 0x8,
            TileScripts = 0x10,
            SectorScripts = 0x20,
            TownmapInfo = 0x40,
            AptitudeAdjustment = 0x80,
            LightScheme = 0x100,
            SoundList = 0x200
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

            var sector = new Sector(loc);

            // Load lights
            if (diffFlags.HasFlag(SectorDiffFlag.Lights))
            {
                if (!SectorLoadLightsWithDiff(ref sector.lights, sectorReader, diffReader, loc))
                {
                    Logger.Error("Error loading lights with differences from files {0} and {1}", path, diffPath);
                    return null;
                }
            }
            else if (!SectorLoadLights(ref sector.lights, sectorReader))
            {
                Logger.Error("Error loading lights from sector file {0}", path);
                return null;
            }

            if (!SectorLoadTiles(ref sector.tilePkt, sectorReader))
            {
                Logger.Error("Error loading lights from sector file {0}", path);
                return null;
            }

            if (!SectorSkipRoofList(sectorReader))
            {
                Logger.Error("Failed to skip the roof-list in {0}", path);
            }

            // What follows is a bunch of optional sections
            var sectionHeader = sectorReader.ReadInt32();

            if (sectionHeader != 0xAA0000)
            {
                if (sectionHeader < 0xAA0000 || sectionHeader > 0xAA0004)
                {
                    Logger.Error("Failed to load section header from {0}", path);
                    return null;
                }

                if (sectionHeader >= 0xAA0001)
                {
                    if (!SectorLoadTilescripts(ref sector.tileScripts, sectorReader))
                    {
                        Logger.Error("Failed to load tilescripts from sector file {0}", path);
                        return null;
                    }

                    if (diffFlags.HasFlag(SectorDiffFlag.TileScripts) &&
                        !SectorLoadTileScriptsDiffs(ref sector.tileScripts, diffReader))
                    {
                        Logger.Error("Failed to load tilescripts from sector difference file {0}", diffPath);
                        return null;
                    }
                }

                if (sectionHeader >= 0xAA0002)
                {
                    if (!SectorLoadSectorScript(ref sector.sectorScript, sectorReader))
                    {
                        Logger.Error("Failed to load sectorscripts from sector file {0}", path);
                        return null;
                    }

                    if (diffFlags.HasFlag(SectorDiffFlag.SectorScripts)
                        && !SectorLoadSectorScriptDiffs(ref sector.sectorScript, diffReader))
                    {
                        Logger.Error("Failed to load sectorscripts from sector difference file {0}", diffPath);
                        return null;
                    }
                }

                if (sectionHeader >= 0xAA0003)
                {
                    try
                    {
                        sector.townmapInfo = sectorReader.ReadInt32();
                    }
                    catch (EndOfStreamException)
                    {
                        Logger.Error("Failed to load townmap info from sector file {0}", path);
                        return null;
                    }

                    if (diffFlags.HasFlag(SectorDiffFlag.TownmapInfo))
                    {
                        try
                        {
                            sector.townmapInfo = diffReader.ReadInt32();
                        }
                        catch (EndOfStreamException)
                        {
                            Logger.Error("Failed to load townmap from sector difference file {0}", diffPath);
                            return null;
                        }
                    }

                    try
                    {
                        sector.aptitudeAdj = sectorReader.ReadInt32();
                    }
                    catch (EndOfStreamException)
                    {
                        Logger.Error("Failed to load aptitude adjustment from sector file {0}", path);
                        return null;
                    }

                    if (diffFlags.HasFlag(SectorDiffFlag.AptitudeAdjustment))
                    {
                        try
                        {
                            sector.aptitudeAdj = diffReader.ReadInt32();
                        }
                        catch (EndOfStreamException)
                        {
                            Logger.Error("Failed to load aptitude adjustment from sector difference file {0}",
                                diffPath);
                            return null;
                        }
                    }

                    try
                    {
                        sector.lightScheme = sectorReader.ReadInt32();
                    }
                    catch (EndOfStreamException)
                    {
                        Logger.Error("Failed to load lightscheme from sector file {0}", path);
                        return null;
                    }

                    if (diffFlags.HasFlag(SectorDiffFlag.LightScheme))
                    {
                        try
                        {
                            sector.lightScheme = diffReader.ReadInt32();
                        }
                        catch (EndOfStreamException)
                        {
                            Logger.Error("Failed to load lightscheme from sector difference file {0}", diffPath);
                            return null;
                        }
                    }


                    if (!SectorLoadSoundList(ref sector.soundList, sectorReader))
                    {
                        Logger.Error("Failed to load soundlist from sector file {0}", path);
                        return null;
                    }

                    if (diffFlags.HasFlag(SectorDiffFlag.SoundList))
                    {
                        Logger.Debug("Sound list flagged as having diffs, but " +
                                     "diffs for soundlists are not supported.");
                    }

                    sector.flags = (int) (sector.flags & 0xFFFFFFF8);
                }
            }

            if (diffFlags.HasFlag(SectorDiffFlag.Objects))
            {
                if (!SectorLoadObjectListWithDiffs(ref sector.objects, sectorReader, diffReader, loc))
                {
                    Logger.Error("Error loading objects with differences from sector file {0} and difference file {1}",
                        path, diffPath);
                    return null;
                }
            }
            else if (!SectorLoadObjects(ref sector.objects, sectorReader))
            {
                Logger.Error("Error loading objects from sector file {0}", path);
                return null;
            }

            if (!GameSystems.MapObject.ValidateSector())
            {
                Logger.Error("Object system validate failed in sector post-load (pre-fold) of {0}", path);
                return null;
            }

            bool v21 = false;
            // TODO: Old stuff v21 = is_time_greater_than_ms(&pSector->time, 3000);
            GameSystems.MapObject.AddDynamicObjectsToSector(ref sector.objects, loc, v21);

            SectorPostprocessLights(sector, loc);
            if (!GameSystems.MapObject.ValidateSector())
            {
                Logger.Error("Object system validate failed in sector post-load (post-fold) of {0}", path);
                return null;
            }

            return sector;
        }

        // TODO: This entire mechanism might be unused
        [TempleDllLocation(0x100a8130)]
        private void ClearLightHandleFlag(GameObjectBody obj, int flag)
        {
            var lightHandle = obj.GetInt32(obj_f.light_handle);
            SectorLight light = default; // TODO
            light.flags &= ~flag;
        }

        // TODO: This entire mechanism might be unused
        [TempleDllLocation(0x100a8650)]
        public void SetLightHandleFlag(GameObjectBody obj, int flag)
        {
            var lightHandle = obj.GetInt32(obj_f.light_handle);
            // TODO
            SectorLight light = default;
            if ((light.flags & 0x40) == 0)
                light = MakeSectorLightNocturnal(light);
            light.flags |= flag;
        }

        [TempleDllLocation(0x100a8370)]
        private SectorLight MakeSectorLightNocturnal(SectorLight sectorLight)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101064e0)]
        private void SectorPostprocessLights(Sector sector, SectorLoc loc)
        {
            bool daytime;
            var hourOfDay = GameSystems.TimeEvent.HourOfDay;
            if (hourOfDay < 6 || hourOfDay >= 18)
                daytime = false;
            else
                daytime = true;

            foreach (var obj in GameSystems.Object.SpatialIndex.EnumerateInSector(loc))
            {
                var v5 = obj.GetFlags();
                if (!(v5.HasFlag(ObjectFlag.INVENTORY)))
                {
                    if (obj.type == ObjectType.scenery && obj.GetSceneryFlags().HasFlag(SceneryFlag.NOCTURNAL))
                    {
                        var tileIdx = sector.GetTileOffset(obj.GetLocation());
                        ref var tile = ref sector.tilePkt.tiles[tileIdx];

                        if (!daytime || tile.flags.HasFlag(TileFlags.TF_Indoor))
                        {
                            if (v5.HasFlag(ObjectFlag.OFF))
                            {
                                obj.SetFlag(ObjectFlag.OFF, false);
                            }
                        }
                        else if (!v5.HasFlag(ObjectFlag.OFF))
                        {
                            obj.SetFlag(ObjectFlag.OFF, true);
                            SetLightHandleFlag(obj, 0);
                        }
                    }

                    MapSectorResetLightHandle(obj);

                    var lightHandle = obj.GetInt32(obj_f.light_handle);
                    if (lightHandle != 0)
                    {
                        // Adds the associated light to the sector and marks it dirty
                        throw new NotImplementedException();
                    }
                }
            }
        }

        [TempleDllLocation(0x100a8590)]
        public void MapSectorResetLightHandle(GameObjectBody obj)
        {
            var renderFlags = obj.GetUInt32(obj_f.render_flags);

            if ((renderFlags & 0x80000000) == 0)
            {
                var lightHandle = obj.GetInt32(obj_f.light_handle);
                if (lightHandle != 0)
                {
                    // TODO FreeSectorLight(lightHandle); @ 0x100a84b0
                    throw new NotImplementedException();
                }

                obj.SetUInt32(obj_f.render_flags, renderFlags | 0x80000000);
            }
        }

        [TempleDllLocation(0x100c1b20)]
        private bool SectorLoadObjects(ref SectorObjects sectorObjects, BinaryReader reader)
        {
            sectorObjects = new SectorObjects();

            // Read the object count from the end of the file
            var startOfObjects = reader.BaseStream.Position;
            if (reader.BaseStream.Seek(-4, SeekOrigin.End) != reader.BaseStream.Length - 4)
            {
                return false;
            }

            var objectCount = reader.ReadInt32();
            reader.BaseStream.Position = startOfObjects;

            sectorObjects.objectsRead = 0;

            for (var i = 0; i < objectCount; i++)
            {
                var obj = GameSystems.Object.LoadFromFile(reader);

                obj.UnfreezeIds();
                obj.SetInt32(obj_f.temp_id, sectorObjects.objectsRead);

                if (!SectorInsertStaticObject(ref sectorObjects, obj))
                {
                    break;
                }

                sectorObjects.objectsRead++;
            }

            // This should now be the object count we've just read
            var trailingObjectCount = reader.ReadInt32();
            if (trailingObjectCount != objectCount || sectorObjects.objectsRead != objectCount)
            {
                UnloadSectorObjects(ref sectorObjects);
                return false;
            }

            sectorObjects.staticObjsDirty = false;
            return true;
        }

        [TempleDllLocation(0x100C1A20)]
        private bool SectorInsertStaticObject(ref SectorObjects sectorObjects, GameObjectBody obj)
        {
            var flags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.INVENTORY))
            {
                Logger.Error("Tried to insert an inventory item {0} into a sector's object list.", obj);
                return false;
            }

            sectorObjects.Insert(obj);

            // TODO: This causes side-effects and should be moved to a post-load stage

            if (obj.IsStatic() || obj.IsCritter())
            {
                GameSystems.MapObject.StartAnimating(obj);
            }

            if (!obj.IsStatic())
            {
                GameSystems.Anim.PushFidget(obj);
            }

            return true;
        }

        [TempleDllLocation(0x100c1360)]
        private void UnloadSectorObjects(ref SectorObjects sectorObjects)
        {
        }

        [TempleDllLocation(0x100c1d50)]
        private bool SectorLoadObjectListWithDiffs(ref SectorObjects sectorObjects,
            BinaryReader reader, BinaryReader diffReader, SectorLoc sectorLoc)
        {
            return false; // TODO
        }

        [TempleDllLocation(0x10105930)]
        private bool SectorLoadSoundList(ref SectorSoundList soundList, BinaryReader reader)
        {
            soundList.field00 = reader.ReadInt32();
            soundList.scheme1 = reader.ReadInt32();
            soundList.scheme2 = reader.ReadInt32();
            soundList.field00 = (int) (soundList.field00 & 0xF7FFFFFF);
            return true;
        }

        [TempleDllLocation(0x10105280)]
        private bool SectorLoadSectorScriptDiffs(ref SectorScript sectorScript, BinaryReader diffReader)
        {
            if (SectorLoadSectorScript(ref sectorScript, diffReader))
            {
                sectorScript.field0 |= 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x10105220)]
        private bool SectorLoadSectorScript(ref SectorScript sectorScript, BinaryReader reader)
        {
            sectorScript.data1 = reader.ReadInt32();
            sectorScript.data2 = reader.ReadUInt32();
            sectorScript.data3 = reader.ReadInt32();
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RawTileScript
        {
            public int Field00;
            public int TileIndex;
            public int ScriptUnk1;
            public uint ScriptCounters;
            public int ScriptId;
            public uint Next; // Stale pointer to the next linked-list entry

            static RawTileScript()
            {
                Trace.Assert(Marshal.SizeOf<RawTileScript>() == 0x18);
            }
        }

        [TempleDllLocation(0x10105540)]
        private bool SectorLoadTilescripts(ref SectorTileScript[] tileScripts, BinaryReader reader)
        {
            var count = reader.ReadInt32();

            if (count == 0)
            {
                tileScripts = Array.Empty<SectorTileScript>();
                return true;
            }

            Span<RawTileScript> rawTileScripts = stackalloc RawTileScript[count];
            Span<byte> rawTileScriptsBytes = MemoryMarshal.Cast<RawTileScript, byte>(rawTileScripts);
            if (reader.Read(rawTileScriptsBytes) != rawTileScriptsBytes.Length)
            {
                return false;
            }

            Array.Resize(ref tileScripts, count);
            for (var i = 0; i < tileScripts.Length; i++)
            {
                ref var tileScript = ref tileScripts[i];
                ref var rawTileScript = ref rawTileScripts[i];

                tileScript.field00 = rawTileScript.Field00;
                tileScript.tileIndex = rawTileScript.TileIndex;
                tileScript.scriptUnk1 = rawTileScript.ScriptUnk1;
                tileScript.scriptCounters = rawTileScript.ScriptCounters;
                tileScript.scriptId = rawTileScript.ScriptId;
            }

            return true;
        }

        [TempleDllLocation(0x10105660)]
        private bool SectorLoadTileScriptsDiffs(ref SectorTileScript[] tileScripts, BinaryReader reader)
        {
            return false; // TODO
        }

        [TempleDllLocation(0x101062f0)]
        private bool SectorLoadLightsWithDiff(ref SectorLights lights, BinaryReader sectorReader,
            BinaryReader diffReader, SectorLoc sectorLoc)
        {
            return false; // TODO
        }

        [TempleDllLocation(0x10106160)]
        private bool SectorLoadLights(ref SectorLights lights, BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count == 0)
            {
                lights.dirty = false;
                return true;
            }

            lights.list = new SectorLight[count];

            for (var i = 0; i < count; i++)
            {
                if (!ReadLight(reader, out lights.list[i]))
                {
                    return false;
                }
            }

            lights.dirty = false;
            return true;
        }

        [TempleDllLocation(0x10105c00)]
        private bool SectorLoadTiles(ref SectorTilePacket tilePacket, BinaryReader reader)
        {
            var tiles = new SectorTile[Sector.SectorSideSize * Sector.SectorSideSize];
            var rawSectorTiles = MemoryMarshal.Cast<SectorTile, byte>(tiles);
            Trace.Assert(rawSectorTiles.Length == Sector.SectorSideSize * Sector.SectorSideSize * 0x10);
            if (reader.Read(rawSectorTiles) != rawSectorTiles.Length)
            {
                return false;
            }

            tilePacket = new SectorTilePacket();
            tilePacket.tiles = tiles;
            return true;
        }

        private bool SectorSkipRoofList(BinaryReader sectorReader)
        {
            var flag = sectorReader.ReadInt32();
            if (flag == 1)
            {
                return true; // No roof list present
            }

            // Skip 0x100 integers
            var curPos = sectorReader.BaseStream.Position;
            if (curPos + 0x400 != sectorReader.BaseStream.Seek(0x400, SeekOrigin.Current))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RawSectorLight
        {
            public ulong StaleHandle;
            public int Flags; // 0x40 -> light2 is present
            public int Type;
            public byte ColorR;
            public byte ColorB;
            public byte ColorG;
            public byte Padding;
            public int padding;
            public LocAndOffsets Position;
            public float OffsetZ;
            public Vector3 Direction;
            public float Range;
            public float Phi;

            public static readonly int Size = Marshal.SizeOf<RawSectorLight>();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RawSectorLight2
        {
            public int PartSysHash;
            public int StalePartSysHandle;
            public int Type;
            public byte ColorR;
            public byte ColorB;
            public byte ColorG;
            public byte Padding;
            public Vector3 Direction;
            public float Range;
            public float Phi;
            public int NightPartSysHash;
            public int NightStalePartSysHandle;

            public static readonly int Size = Marshal.SizeOf<RawSectorLight2>();
        }

        [TempleDllLocation(0x100a6890)]
        private bool ReadLight(BinaryReader reader, out SectorLight sectorLight)
        {
            Trace.Assert(RawSectorLight.Size == 0x40);
            Span<byte> lightData = stackalloc byte[RawSectorLight.Size];
            if (reader.Read(lightData) != lightData.Length)
            {
                sectorLight = default;
                return false;
            }

            var rawLight = MemoryMarshal.Read<RawSectorLight>(lightData);

            var light = new SectorLight
            {
                flags = rawLight.Flags,
                type = rawLight.Type,
                color = new LinearColor(
                    rawLight.ColorR / 255.0f,
                    rawLight.ColorG / 255.0f,
                    rawLight.ColorB / 255.0f
                ),
                position = rawLight.Position,
                offsetZ = rawLight.OffsetZ,
                direction = Vector3.Normalize(rawLight.Direction),
                range = rawLight.Range,
                phi = rawLight.Phi
            };

            var lightPos = light.position.ToInches3D(light.offsetZ);

            if ((light.flags & 0x10) != 0)
            {
                light.partSys.hashCode = reader.ReadInt32();
                reader.ReadInt32(); // Skip stale partsys handle

                if (light.partSys.hashCode != 0)
                {
                    light.partSys.handle = GameSystems.ParticleSys.CreateAt(light.partSys.hashCode, lightPos);
                }
            }
            else if ((light.flags & 0x40) != 0)
            {
                Trace.Assert(RawSectorLight2.Size == 0x24 + 0x8);

                Span<byte> lightData2 = stackalloc byte[RawSectorLight2.Size];
                if (reader.Read(lightData2) != lightData2.Length)
                {
                    sectorLight = default;
                    return false;
                }

                var rawLight2 = MemoryMarshal.Read<RawSectorLight2>(lightData2);

                light.partSys.hashCode = rawLight2.PartSysHash;
                light.light2.type = rawLight2.Type;
                light.light2.color = new LinearColor(
                    rawLight2.ColorR / 255.0f,
                    rawLight2.ColorG / 255.0f,
                    rawLight2.ColorB / 255.0f
                );
                light.light2.direction = Vector3.Normalize(rawLight2.Direction);
                light.light2.range = rawLight2.Range;
                light.light2.phi = rawLight2.Phi;
                light.light2.partSys.hashCode = rawLight2.NightPartSysHash;

                if (GameSystems.Light.IsNight)
                {
                    if (light.light2.partSys.hashCode != 0)
                    {
                        light.light2.partSys.handle =
                            GameSystems.ParticleSys.CreateAt(light.light2.partSys.hashCode, lightPos);
                    }
                }
                else
                {
                    if (light.partSys.hashCode != 0)
                    {
                        light.partSys.handle = GameSystems.ParticleSys.CreateAt(light.partSys.hashCode, lightPos);
                    }
                }
            }

            sectorLight = light;
            return true;
        }

        [TempleDllLocation(0x100826b0)]
        public bool IsSectorLoaded(SectorLoc secLoc)
        {
            return _sectorCache.Any(s => s.Loc == secLoc);
        }

        [TempleDllLocation(0x10081b50)]
        public IEnumerable<Sector> LoadedSectors
        {
            get { return _sectorCache.Select(s => s.Sector); }
        }

        [TempleDllLocation(0x10082b90)]
        public void Clear()
        {
            foreach (var sector in _sectorCache)
            {
                if (sector.LockCount != 0)
                {
                    Logger.Error("Unloading sector {0} while still being locked: {1}",
                        sector.Loc, sector.LockCount);
                }

                if (sector.Sector != null)
                {
                    UnloadSector(sector.Sector);
                }
            }

            _sectorCache.Clear();
        }

        [TempleDllLocation(0x10082C00)]
        public void SaveSectors(bool forceUnload)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10082670)]
        public void SetDirectories(string dataDir, string saveDir)
        {
            _dataDir = dataDir;
            _saveDir = saveDir;
        }

        [TempleDllLocation(0x10081570)]
        public void ResetBuffers()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10084120)]
        public void Reset()
        {
            Clear();
        }

        [TempleDllLocation(0x100842e0)]
        public void CloseMap()
        {
            Clear();
        }
    }
}