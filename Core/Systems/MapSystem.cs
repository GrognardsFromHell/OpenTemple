using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.MesFiles;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems
{
    public class MapSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private D20System mD20System;

        // Startup tips related fields
        private bool mEnableTips;
        private string mTipsDialogTitle;
        private string mTipsDialogOk;
        private string mTipsDialogNext;
        private string mTipsDialogShowTips;
        private Dictionary<int, string> mTips;

        // Related to fleeing from combat (for whatever reason this is here)
        [TempleDllLocation(0x10AA9558)]
        private MapFleeInfo mFleeInfo;

        private string mSectorDataDir;
        private string mSectorSaveDir;

        // List of maps, scope: module
        private Dictionary<int, MapListEntry> mMaps;
        private MapListEntry mCurrentMap = null;

        // Visited maps, scope: game session
        private HashSet<int> mVisitedMaps = new HashSet<int>();

        // Picked when opening the map
        private locXY mStartLoc;

        // Indicates that the map is currently being cleared
        private bool mClearingMap = false;

        // Indicates that a map is currently opened
        private bool mMapOpen = false;
        private bool mMapClosed = false;

        [TempleDllLocation(0x1006f4d0)]
        public MapSystem(D20System d20)
        {
            mD20System = d20;
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10072050)]
        public void SaveGame(SavedGameState savedGameState)
        {
            // Vanilla did clear the dispatchers for the party here, but it's done for _all_ objects
            // in map flush anyway.
            FlushMap(true);

            savedGameState.MapState = new SavedMapState
            {
                CurrentMapName = mCurrentMap.name,
                VisitedMaps = mVisitedMaps.ToHashSet()
            };

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                GameSystems.D20.Status.D20StatusInit(partyMember);
            }

            savedGameState.MapFleeState = new SavedMapFleeState
            {
                Location = mFleeInfo.location,
                EnterX = mFleeInfo.enterLocation.locx,
                EnterY = mFleeInfo.enterLocation.locy,
                IsFleeing = mFleeInfo.isFleeing,
                MapId = mFleeInfo.mapId
            };
        }

        [TempleDllLocation(0x10072c40)]
        public void LoadGame(SavedGameState savedGameState)
        {
            var mapState = savedGameState.MapState;

            var mapEntry = mMaps.Values.First(m => m.name == mapState.CurrentMapName);
            if (mapEntry == null)
            {
                throw new CorruptSaveException(
                    $"Save game references map {mapState.CurrentMapName} which doesn't exist.");
            }

            mVisitedMaps = mapState.VisitedMaps.ToHashSet();

            OpenMap(mapEntry);

            var mapFleeState = savedGameState.MapFleeState;
            mFleeInfo = new MapFleeInfo
            {
                location = mapFleeState.Location,
                isFleeing = mapFleeState.IsFleeing,
                enterLocation = new locXY(mapFleeState.EnterX, mapFleeState.EnterY),
                mapId = mapFleeState.MapId
            };
        }

        public void LoadModule()
        {
            bool alwaysFog = false;
            bool alwaysUnfog = false;
            if (Globals.Config.fogOfWar.ToLowerInvariant() == "always")
            {
                alwaysFog = true;
            }
            else if (Globals.Config.fogOfWar.ToLowerInvariant() == "unfogged")
            {
                alwaysUnfog = true;
            }

            mMaps = MapListParser.Parse(Tig.FS, alwaysFog, alwaysUnfog);
        }

        public void UnloadModule()
        {
            mMaps = null;
        }

        [TempleDllLocation(0x10071e40)]
        public void Reset()
        {
            ResetFleeTo();

            Logger.Info("Resetting map");

            GameSystems.Anim.InterruptAll();

            CloseMap();
            mCurrentMap = null; // TODO: Redundant?

            mVisitedMaps.Clear();
        }

        [TempleDllLocation(0x1006f8f0)]
        public void ResetFleeTo()
        {
            mFleeInfo = new MapFleeInfo();
        }

        [TempleDllLocation(0x1006f920)]
        public void SetFleeInfo(int mapId, LocAndOffsets loc, locXY enterLoc)
        {
            mFleeInfo.mapId = mapId;
            mFleeInfo.enterLocation = enterLoc;
            mFleeInfo.location = loc;
        }

        [TempleDllLocation(0x1006f990)]
        public bool HasFleeInfo()
        {
            return mFleeInfo.mapId != 0;
        }

        [TempleDllLocation(0x1006f970)]
        public bool GetFleeInfo(out MapFleeInfo fleeInfo)
        {
            fleeInfo = mFleeInfo;
            return HasFleeInfo();
        }

        [TempleDllLocation(0x1006f9b0)]
        public bool IsFleeing()
        {
            return mFleeInfo.isFleeing;
        }

        [TempleDllLocation(0x1006f9a0)]
        public void SetFleeing(bool fleeing)
        {
            mFleeInfo.isFleeing = fleeing;
        }

        private string GetExplorationDataPath(SectorLoc sectorLoc)
        {
            return Path.Combine(mSectorSaveDir, $"esd{sectorLoc.Pack()}");
        }

        [TempleDllLocation(0x1006faa0)]
        public SectorExploration LoadSectorExploration(SectorLoc sectorLoc)
        {
            var exploration = new SectorExploration();

            var filename = GetExplorationDataPath(sectorLoc);
            try
            {
                using var stream = new FileStream(filename, FileMode.Open);
                exploration.Load(stream, filename);
            }
            catch (FileNotFoundException)
            {
            }

            return exploration;
        }

        [TempleDllLocation(0x1006fb50)]
        public void SaveSectorExploration(SectorLoc sectorLoc, SectorExploration exploration)
        {
            var filename = GetExplorationDataPath(sectorLoc);

            if (exploration.State == SectorExplorationState.Unexplored)
            {
                File.Delete(filename);
            }
            else
            {
                using var stream = new FileStream(filename, FileMode.Create);

                exploration.Save(stream);
            }
        }

        [TempleDllLocation(0x1006fd70)]
        public int GetMapIdByType(MapType type)
        {
            foreach (var entry in mMaps.Values)
            {
                if (entry.type == type)
                {
                    return entry.id;
                }
            }

            if (type == MapType.ArenaMap)
            {
                return 5119; // Shai you naughty naughty man.
            }

            return 0;
        }

        [TempleDllLocation(0x1006fe10)]
        public bool IsRandomEncounterMap(int mapId)
        {
            var maxRange = 5077;

            // TODO: This was enabled for Co8 maxRange = 5078;

            return mapId >= 5070 & mapId <= maxRange;
        }

        [TempleDllLocation(0x1006fe30)]
        public bool IsVignetteMap(int mapId)
        {
            return mapId >= 5096 & mapId <= 5104;
        }

        [TempleDllLocation(0x1006fe80)]
        public bool IsCurrentMapOutdoors()
        {
            if (mCurrentMap != null)
            {
                return mCurrentMap.IsOutdoors;
            }

            return false;
        }

        [TempleDllLocation(0x1006fea0)]
        public bool IsCurrentMapBedrest
        {
            get
            {
                if (mCurrentMap != null)
                {
                    return mCurrentMap.IsBedrest;
                }

                return false;
            }
        }

        private void ClearDispatchers()
        {
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                if (!obj.IsStatic())
                {
                    mD20System.RemoveDispatcher(obj);
                }
            }
        }

        [TempleDllLocation(0x100706d0)]
        private void ClearObjects()
        {
            Trace.Assert(!mClearingMap);

            mClearingMap = true;

            GameSystems.TimeEvent.ClearForMapClose();

            // We need to make a copy because we are about to modify it
            List<GameObjectBody> objects = new List<GameObjectBody>(GameSystems.Object.EnumerateNonProtos());

            foreach (var obj in objects)
            {
                if (!obj.IsStatic())
                {
                    GameSystems.MapObject.FreeRenderState(obj);
                    GameSystems.MapObject.RemoveMapObj(obj);
                }
            }

            GameSystems.MapSector.Clear();
            GameSystems.Object.CompactIndex();

            mClearingMap = false;
        }

        public void ShowGameTip()
        {
            if (mEnableTips)
            {
                if (!GameUiBridge.IsTutorialActive())
                {
                    var tip = Globals.Config.StartupTip;
                    if (tip >= 0)
                    {
                        mEnableTips = true;
                        ShowGameTip(tip);
                    }
                }
            }
        }

        private void ShowGameTip(int tipId)
        {
            // Roll over to the first tip
            if (tipId >= mTips.Count)
            {
                tipId = 0;
            }

            var sTipText = mTips[tipId];
            GameUiBridge.ShowTip(
                mTipsDialogTitle,
                sTipText,
                mTipsDialogOk,
                mTipsDialogNext,
                mTipsDialogShowTips,
                out mEnableTips,
                okButton =>
                {
                    if (okButton)
                    {
                        if (!mEnableTips)
                        {
                            Globals.Config.StartupTip = -1;
                            Globals.ConfigManager.Save();
                        }
                    }
                    else
                    {
                        ShowGameTip(Globals.Config.StartupTip);
                    }
                });

            Globals.Config.StartupTip = tipId + 1;
            Globals.ConfigManager.Save();
        }

        [TempleDllLocation(0x10070ef0)]
        public bool IsValidMapId(int mapId) => mMaps.ContainsKey(mapId);

        [TempleDllLocation(0x10070f90)]
        public int GetCurrentMapId()
        {
            if (mCurrentMap != null)
            {
                return mCurrentMap.id;
            }

            return 0;
        }

        private int GetHighestMapId()
        {
            int highestId = 0;
            foreach (var entry in mMaps)
            {
                if (entry.Value.id > highestId)
                {
                    highestId = entry.Value.id;
                }
            }

            return highestId;
        }

        [TempleDllLocation(0x10071dd0)]
        public locXY GetStartPos(int mapId)
        {
            var result = new locXY();

            if (mMaps.TryGetValue(mapId, out var map))
            {
                result.locx = map.startPosX;
                result.locy = map.startPosY;
            }

            return result;
        }

        public string GetMapName(int mapId)
        {
            if (mMaps.TryGetValue(mapId, out var map))
            {
                return map.name;
            }

            return "";
        }

        public string GetMapDescription(int mapId)
        {
            if (mMaps.TryGetValue(mapId, out var map))
            {
                return map.description;
            }

            return "";
        }

        public bool IsMapOutdoors(int mapId)
        {
            if (mMaps.TryGetValue(mapId, out var map))
            {
                return map.IsOutdoors;
            }

            return false;
        }

        public int GetEnterMovie(int mapId, bool ignoreVisited)
        {
            if (!ignoreVisited && mVisitedMaps.Contains(mapId))
            {
                return 0;
            }

            if (mMaps.TryGetValue(mapId, out var map))
            {
                return map.movie;
            }

            return 0;
        }

        [TempleDllLocation(0x1006fe50)]
        public IEnumerable<int> VisitedMaps => mVisitedMaps;

        [TempleDllLocation(0x10071700)]
        public void MarkVisitedMap(GameObjectBody obj)
        {
            if (!obj.IsPC())
            {
                return;
            }

            if (mCurrentMap == null)
            {
                return;
            }

            int mapId = mCurrentMap.id;
            MarkVisitedMap(mapId);
        }

        public void MarkVisitedMap(int mapId)
        {
            if (!IsRandomEncounterMap(mapId) && !IsVignetteMap(mapId))
            {
                mVisitedMaps.Add(mapId);
            }
        }

        public bool IsUnfogged(int mapId)
        {
            if (mMaps.TryGetValue(mapId, out var map))
            {
                return map.IsUnfogged;
            }

            return false;
        }

        private int GetArea(int mapId)
        {
            if (mMaps.TryGetValue(mapId, out var map))
            {
                return map.area;
            }

            return 0;
        }

        [TempleDllLocation(0x1006fd90)]
        public bool IsClearingMap() => mClearingMap;

        public bool IsMapOpen() => mMapOpen;

        [TempleDllLocation(0x10072a90)]
        public bool OpenMap(int mapId, bool preloadSectors, bool dontSaveCurrentMap, bool ignoreParty = false)
        {
            if (!mMaps.TryGetValue(mapId, out var mapEntry))
            {
                Logger.Warn("Trying to open unknown map id {0}", mapId);
                return false;
            }

            if (!ignoreParty)
            {
                GameSystems.Party.SaveCurrent();
            }

            if (IsMapOpen() && !dontSaveCurrentMap)
            {
                FlushMap(false);
            }

            Tig.Sound.ProcessEvents();

            if (!GameSystems.MapObject.ValidateSector())
            {
                throw new Exception("Object system validate failed pre-load.");
            }

            OpenMap(mapEntry);

            mStartLoc.locx = mapEntry.startPosX;
            mStartLoc.locy = mapEntry.startPosY;

            GameSystems.Location.CenterOn(mapEntry.startPosX, mapEntry.startPosY);

            Tig.Sound.ProcessEvents();

            if (preloadSectors)
            {
                PreloadSectorsAround(mStartLoc);
            }

            GameSystems.TimeEvent.ValidateEvents();
            GameSystems.TimeEvent.LoadFromMap(mapId);
            GameSystems.Anim.LoadFromMap(mapId);

            if (!ignoreParty)
            {
                GameSystems.Party.RestoreCurrent();
            }

            GameSystems.Critter.RescheduleNpcHealingTimers();

            GameUiBridge.OnAfterMapLoad();

            if (!GameSystems.MapObject.ValidateSector())
            {
                throw new Exception("Object system validate failed post-load.");
            }

            return true;
        }

        [TempleDllLocation(0x10071780)]
        public void CloseMap()
        {
            if (!mMapClosed)
            {
                mMapClosed = true;

                GameSystems.D20.turnBasedReset();

                GameSystems.AAS.ModelFactory.FreeAll();

                mMapOpen = false;

                foreach (var mapSystem in GameSystems.MapCloseAwareSystems)
                {
                    mapSystem.CloseMap();
                }

                ClearObjects();
                GameSystems.ParticleSys.RemoveAll();

                mSectorSaveDir = "";
                mSectorDataDir = "";
            }
        }

        private void LoadTips()
        {
            var tips = Tig.FS.ReadMesFile("mes/tips.mes");

            mTipsDialogTitle = tips[0];
            mTipsDialogOk = tips[1];
            mTipsDialogNext = tips[2];
            mTipsDialogShowTips = tips[3];

            var count = int.Parse(tips[99]);

            for (int i = 0; i < count; ++i)
            {
                mTips[i] = tips[100 + i];
            }

            if (Globals.Config.StartupTip >= 0)
            {
                mEnableTips = true;
            }
            else
            {
                mEnableTips = false;
            }
        }

        [TempleDllLocation(0x10071170)]
        private void FlushMap(bool keepLoaded)
        {
            // Freeze all IDs
            GameSystems.Object.ForEachObj(obj =>
            {
                // The assumption seems to be that static objs are not saved here
                if (!obj.IsStatic())
                {
                    obj.FreezeIds();
                }

                GameSystems.D20.RemoveDispatcher(obj);
            });

            SaveMapMobiles();

            // Unfreeze the IDs
            MapLoadPostprocess();

            // Both are fogging related, but I have no idea how they differ?
            GameSystems.MapFogging.FlushSectorExploration();

            if (mCurrentMap != null)
            {
                GameSystems.MapFogging.SaveExploredTileData(mCurrentMap.id);
            }

            GameSystems.MapSector.FlushSectors(keepLoaded);
            GameSystems.SectorVisibility.Flush();
            // Previously several other subsystems saved their data here if they were
            // in editor mode

            // Flushes townmap data for the current map
            GameSystems.TownMap.Flush();

            // flushes the ObjectEvents (which are tied to spell ObjectHandles.anyway and should go away)
            if (!keepLoaded)
            {
                GameSystems.ObjectEvent.FlushEvents();
            }
        }

        struct MapProperties
        {
            // ID for terrain art
            public int groundArtId;
            public int y;
            public ulong limitX;
            public ulong limitY;
        };


        [TempleDllLocation(0x10072370)]
        private void OpenMap(MapListEntry mapEntry)
        {
            var dataDir = GetDataDir(mapEntry.id);
            var saveDir = GetSaveDir(mapEntry.id);

            Logger.Info("Loading Map {0} Data={1}, Save={2}", mapEntry.id, dataDir, saveDir);

            // Close opened map
            CloseMap();

            if (!Tig.FS.DirectoryExists(dataDir))
            {
                throw new Exception($"Cannot open map '{dataDir}' because it doesn't exist.");
            }

            Directory.CreateDirectory(saveDir);

            mSectorSaveDir = saveDir;
            mSectorDataDir = dataDir;

            GameSystems.Height.SetDataDirs(dataDir, saveDir);
            GameSystems.PathNode.SetDataDirs(dataDir, saveDir);

            Logger.Info("Reading map properties from map.prp");

            var prpFilename = $"{dataDir}/map.prp";
            var prpContent = Tig.FS.ReadBinaryFile(prpFilename);
            var mapProperties = MemoryMarshal.Read<MapProperties>(prpContent);

            // Previously startloc.txt was loaded here, but that only seems to be relevant
            // for worlded, and not for the actual game.

            ReadMapMobiles(dataDir, saveDir);

            GameSystems.Height.Clear();

            GameSystems.Raycast.GoalDestinationsClear();

            GameSystems.Clipping.Load(dataDir);

            GameSystems.GMesh.Load(dataDir);

            GameSystems.Light.Load(dataDir);

            GameSystems.MapFogging.LoadFogColor(dataDir);

            GameSystems.PathNode.Load(dataDir, saveDir);

            MapLoadPostprocess();

            // TODO: a global sector_datadir and sector_savedir were set here
            // But only  seemed to be used from within map systems and at one spot
            // in editor mode

            GameSystems.Terrain.Load(mapProperties.groundArtId);

            // Previously a "map.sbf" file was loaded here, which is only used
            // by the old scripting system though

            GameSystems.Location.SetLimits(mapProperties.limitX, mapProperties.limitY);

            GameSystems.Sector.SetLimits(mapProperties.limitX / 64, mapProperties.limitY / 64);

            GameSystems.MapSector.SetDirectories(dataDir, saveDir);
            GameSystems.SectorVisibility.SetDirectories(dataDir, saveDir);

            var center = GameSystems.Location.GetLimitsCenter();
            GameSystems.Location.CenterOn(center.locx, center.locy);

            mCurrentMap = mapEntry;

            if (mapEntry.IsUnfogged)
            {
                GameSystems.MapFogging.Disable();
            }
            else
            {
                GameSystems.MapFogging.Enable();
                GameSystems.MapFogging.LoadCurrentTownMapFogOfWar(saveDir);
            }

            GameSystems.Light.SetMapId(mapEntry.id);

            GameSystems.Scroll.SetMapId(mapEntry.id);

            LoadMapInfo(dataDir);

            // Previously there was a function call here that disabled ObjectHandles.outside the
            // Arkanum demo bounds, which are obviously unused here.

            mMapOpen = true;
            mMapClosed = false;

            Logger.Info("Finished loading the map");
        }

        private void LoadMapInfo(string dataDir)
        {
            var filename = $"{dataDir}/mapinfo.txt";

            if (!Tig.FS.FileExists(filename))
            {
                Logger.Info("Couldn't find or read optional file: {0}", filename);
                return;
            }

            // This file is optional
            var mapInfoContent = Tig.FS.ReadTextFile(filename);

            var lines = mapInfoContent.Split("\n");
            foreach (var line in lines)
            {
                var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }

                var prop = parts[0].ToLowerInvariant();
                if (prop == "lightscheme")
                {
                    var lightSchemeId = int.Parse(parts[1]);
                    GameSystems.LightScheme.SetDefaultScheme(lightSchemeId);
                    var hour = GameSystems.LightScheme.GetHourOfDay();
                    GameSystems.LightScheme.SetCurrentScheme(0, hour);
                }
                else if (prop == "soundscheme")
                {
                    var schemes = parts[1].Split(",");
                    var scheme1 = int.Parse(schemes[0]);
                    var scheme2 = int.Parse(schemes[1]);
                    GameSystems.SoundGame.SetScheme(scheme1, scheme2);
                }
                else if (prop == "reverb")
                {
                    var reverbs = parts[1].Split(",");
                    var roomType = int.Parse(reverbs[0]);
                    var reverbDry = int.Parse(reverbs[1]);
                    var reverbWet = int.Parse(reverbs[2]);
                    Tig.Sound.SetReverb((ReverbRoomType) roomType, reverbDry, reverbWet);
                }
                else if (prop == "ground")
                {
                    var groundId = int.Parse(parts[1]);
                    GameSystems.Terrain.Load(groundId);
                }
                else
                {
                    Logger.Warn("Unknown command in map extension file {0}: {1}", filename, line);
                }
            }
        }

        [TempleDllLocation(0x10072370)]
        private void ReadMapMobiles(string dataDir, string saveDir)
        {
            var loader = new MapMobileLoader(Tig.FS);
            loader.Load(dataDir, saveDir);

            foreach (var mobile in loader.Mobiles)
            {
                GameSystems.Object.Add(mobile);
            }
        }

        private void MapLoadPostprocess()
        {
            GameSystems.D20.ResetRadialMenus();

            GameSystems.Object.ForEachObj(obj =>
            {
                if (!obj.IsStatic())
                {
                    obj.UnfreezeIds();
                }

                if (obj.HasFlag(ObjectFlag.TELEPORTED))
                {
                    var e = new TimeEvent(TimeEventType.Teleported);
                    e.arg1.handle = obj;
                    GameSystems.TimeEvent.ScheduleNow(e);
                    obj.SetFlag(ObjectFlag.TELEPORTED, false);
                    return;
                }

                // Initialize everything's D20 state
                if (GameSystems.Party.IsInParty(obj))
                {
                    return; // The party is initialized elsewhere (in the Party system)
                }

                // This logic is a bit odd really. Apparently obj_f.dispatcher will not be -1 for non-critters anyway?
                if (obj.IsNPC() || obj.GetInt32(obj_f.dispatcher) == -1)
                {
                    GameSystems.D20.Status.D20StatusInit(obj);
                }
            });
        }

        private void SaveMapMobiles()
        {
            // This file will contain the differences from the mobile object stored in the sector's data files
            var diffFilename = Path.Join(mSectorSaveDir, MapMobileLoader.MobileDifferencesFile);
            using var diffOut = new BinaryWriter(new FileStream(diffFilename, FileMode.Create));

            // This file will contain the dynamic ObjectHandles.that have been created on this map
            var dynFilename = Path.Join(mSectorSaveDir, MapMobileLoader.DynamicMobilesFile);
            using var dynOut = new BinaryWriter(new FileStream(dynFilename, FileMode.Create));

            // This file will contain the object ids of mobile sector ObjectHandles.that have been destroyed
            var destrFilename = Path.Join(mSectorSaveDir, MapMobileLoader.DestroyedMobilesFile);
            using var destrFh = new BinaryWriter(new FileStream(destrFilename, FileMode.Append));

            var prevDestroyedObjs = destrFh.BaseStream.Length / Marshal.SizeOf<ObjectId>();
            var destroyedObjs = 0;
            var dynamicObjs = 0;
            var diffObjs = 0;

            GameSystems.Object.ForEachObj(obj =>
            {
                if (obj.IsStatic())
                {
                    return; // Diffs for static ObjectHandles.are stored in the sector files
                }

                // Dynamic ObjectHandles.are stored in their entirety in mobile.mdy
                if (obj.HasFlag(ObjectFlag.DYNAMIC))
                {
                    // If a dynamic object has been destroyed, it wont be recreated on mapload
                    // anyway (since there is no mob file for it)
                    if (obj.HasFlag(ObjectFlag.DESTROYED) || obj.HasFlag(ObjectFlag.EXTINCT))
                    {
                        return;
                    }

                    obj.Write(dynOut);
                    ++dynamicObjs;
                    return;
                }

                if (!obj.hasDifs)
                {
                    return; // Object is unchanged
                }

                if (obj.HasFlag(ObjectFlag.DESTROYED) || obj.HasFlag(ObjectFlag.EXTINCT))
                {
                    // Write the object id of the destroyed obj to mobile.des
                    destrFh.WriteObjectId(obj.id);
                    ++destroyedObjs;
                }
                else
                {
                    // Write the object id followed by a diff record to mobile.mdy
                    diffOut.WriteObjectId(obj.id);

                    obj.WriteDiffsToStream(diffOut);
                    ++diffObjs;
                }
            });

            diffOut.Dispose();
            destrFh.Dispose();
            dynOut.Dispose();

            Logger.Info("Saved {0} dynamic, {1} destroyed ({2} previously), and {3} mobiles with differences in {4}",
                dynamicObjs, destroyedObjs, prevDestroyedObjs, diffObjs, mSectorSaveDir);

            if (diffObjs == 0)
            {
                File.Delete(diffFilename);
            }

            if (destroyedObjs == 0 && prevDestroyedObjs == 0)
            {
                File.Delete(destrFilename);
            }

            if (dynamicObjs == 0)
            {
                File.Delete(dynFilename);
            }
        }

        private void SaveSectors(bool flags)
        {
            GameSystems.MapSector.FlushSectors(flags);
        }

        [TempleDllLocation(0x1006fc90)]
        public void PreloadSectorsAround(locXY loc)
        {
            // Center sector
            var sectorLoc = new SectorLoc(loc);
            var startX = sectorLoc.X - 1;
            var startY = sectorLoc.Y - 1;

            for (int x = 0; x < 3; ++x)
            {
                for (int y = 0; y < 3; ++y)
                {
                    var currentLoc = new SectorLoc(startX + x, startY + y);
                    using var lockedSector = new LockedMapSector(currentLoc);
                }
            }
        }

        public string GetDataDir(int mapId)
        {
            var mapEntry = mMaps[mapId];
            return $"maps/{mapEntry.name}";
        }

        public string GetSaveDir(int mapId)
        {
            var mapEntry = mMaps[mapId];
            return Path.Join(Globals.GameFolders.CurrentSaveFolder, "maps", mapEntry.name);
        }
    }

    // Contains info on how to flee from combat
    public struct MapFleeInfo
    {
        public bool isFleeing;
        public int mapId;
        public LocAndOffsets location;
        public locXY enterLocation;
    }
}