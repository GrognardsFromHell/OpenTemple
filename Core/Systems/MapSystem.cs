using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.MesFiles;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public class MapSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

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

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        public void LoadModule()
        {
            var mapList = Tig.FS.ReadMesFile("rules/MapList.mes");
            var mapNames = Tig.FS.ReadMesFile("mes/map_names.mes");

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

            mMaps = new Dictionary<int, MapListEntry>(mapList.Count);

            foreach (var (mapId, line) in mapList)
            {
                var entry = new MapListEntry();

                var parts = line.Split(',');

                entry.id = mapId;
                entry.name = parts[0];
                entry.startPosX = int.Parse(parts[1]);
                entry.startPosY = int.Parse(parts[2]);
                entry.flags = 0;
                if (alwaysUnfog)
                    entry.flags |= 4;

                // The rest are key value pairs
                for (var i = 3; i < parts.Length; ++i)
                {
                    var subParts = parts[i].Split(':');
                    var key = subParts[0].Trim();
                    var value = subParts[1].Trim();

                    switch (key)
                    {
                        case "Type":
                            switch (value)
                            {
                                case "NONE":
                                    entry.type = MapType.None;
                                    break;
                                case "START_MAP":
                                    entry.type = MapType.StartMap;
                                    break;
                                case "SHOPPING_MAP":
                                    entry.type = MapType.ShoppingMap;
                                    break;
                                case "TUTORIAL_MAP":
                                    entry.type = MapType.TutorialMap;
                                    break;
                            }

                            break;
                        case "WorldMap":
                            entry.worldmap = int.Parse(value);
                            break;
                        case "Area":
                            entry.area = int.Parse(value);
                            break;
                        case "Movie":
                            entry.movie = int.Parse(value);
                            break;
                        case "Flag":
                            switch (value)
                            {
                                case "DAYNIGHT_XFER":
                                    entry.flags |= 1;
                                    break;
                                case "OUTDOOR":
                                    entry.flags |= 2;
                                    break;
                                case "UNFOGGED":
                                {
                                    if (!alwaysFog)
                                        entry.flags |= 4;
                                    break;
                                }

                                case "BEDREST":
                                    entry.flags |= 8;
                                    break;
                            }

                            break;
                        default:
                            Logger.Warn("Unknown map key '{1}' for map {0}", mapId, key);
                            break;
                    }
                }

                // Copy the description from map_names.mes
                if (mapNames.TryGetValue(mapId, out var mapName))
                {
                    entry.description = mapName;
                }
                else
                {
                    Logger.Warn("Missing map description for {0}", mapId);
                    entry.description = entry.name;
                }

                mMaps[mapId] = entry;
            }

            // Get info from hardcoded map areas table (bleh)
            foreach (var entry in mMaps.Values)
            {
                if (entry.area == 0)
                {
                    entry.area = GetAreaForVanillaMap(entry.id);
                }
            }
        }

        private int GetAreaForVanillaMap(int mapId)
        {
            int result;
            switch (mapId)
            {
                case 5000:
                case 5001:
                case 5006:
                case 5007:
                case 5008:
                case 5009:
                case 5010:
                case 5011:
                case 5012:
                case 5013:
                case 5014:
                case 5015:
                case 5016:
                case 5017:
                case 5018:
                case 5019:
                case 5020:
                case 5021:
                case 5022:
                case 5023:
                case 5024:
                case 5025:
                case 5026:
                case 5027:
                case 5028:
                case 5029:
                case 5030:
                case 5031:
                case 5032:
                case 5033:
                case 5034:
                case 5035:
                case 5036:
                case 5037:
                case 5038:
                case 5039:
                case 5040:
                case 5041:
                case 5042:
                case 5043:
                case 5044:
                case 5045:
                case 5046:
                case 5047:
                case 5048:
                case 5049:
                case 5063:
                case 5096:
                case 5097:
                case 5098:
                case 5099:
                case 5100:
                case 5101:
                case 5102:
                case 5103:
                case 5104:
                case 5115:
                    result = 1;
                    break;
                case 5002:
                case 5003:
                case 5004:
                case 5005:
                    result = 2;
                    break;
                case 5051:
                case 5052:
                case 5053:
                case 5054:
                case 5055:
                case 5056:
                case 5057:
                case 5058:
                case 5059:
                case 5060:
                case 5061:
                case 5085:
                case 5086:
                case 5087:
                case 5088:
                    result = 3;
                    break;
                case 5062:
                case 5064:
                case 5065:
                case 5066:
                case 5067:
                case 5078:
                case 5079:
                case 5080:
                case 5081:
                case 5082:
                case 5083:
                case 5084:
                case 5106:
                    result = 4;
                    break;
                case 5094:
                    result = 5;
                    break;
                case 5068:
                    result = 6;
                    break;
                case 5092:
                case 5093:
                    result = 7;
                    break;
                case 5091:
                    result = 8;
                    break;
                case 5095:
                case 5114:
                    result = 9;
                    break;
                case 5069:
                    result = 10;
                    break;
                case 5112:
                    result = 11;
                    break;
                case 5113:
                    result = 12;
                    break;
                default:
                    result = 0;
                    break;
            }

            return result;
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

        private bool IsRandomEncounterMap(int mapId)
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
                    var tip = Globals.Config.GetVanillaInt("startup_tip");
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
                            Globals.Config.SetVanillaInt("startup_tip", -1);
                        }
                    }
                    else
                    {
                        var nextTip = Globals.Config.GetVanillaInt("startup_tip");
                        ShowGameTip(nextTip);
                    }
                });

            Globals.Config.SetVanillaInt("startup_tip", tipId + 1);
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
            if (!IsRandomEncounterMap(mapId) && !IsVignetteMap(mapId))
            {
                mVisitedMaps.Add(mapId);
            }
        }

        private bool IsUnfogged(int mapId)
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
        public bool OpenMap(int mapId, bool preloadSectors, bool dontSaveCurrentMap)
        {
            if (!mMaps.TryGetValue(mapId, out var mapEntry))
            {
                Logger.Warn("Trying to open unknown map id {0}", mapId);
                return false;
            }

            GameSystems.Party.SaveCurrent();

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

            GameSystems.TimeEvent.LoadForCurrentMap();
            GameSystems.Party.RestoreCurrent();
            GameSystems.Critter.UpdateNpcHealingTimers();

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

            if (Globals.Config.GetVanillaInt("startup_tip") >= 0)
            {
                mEnableTips = true;
            }
            else
            {
                mEnableTips = false;
            }
        }

        [TempleDllLocation(0x10071170)]
        private void FlushMap(bool flags)
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

            // Previously a "map.sbf" file was saved here, which is only used
            // by the old scripting system though
            SaveSectors(flags);
            GameSystems.SectorVisibility.Flush();
            // Previously several other subsystems saved their data here if they were
            // in editor mode

            // Flushes townmap data for the current map
            GameSystems.TownMap.Flush();

            // flushes the ObjectEvents (which are tied to spell ObjectHandles.anyway and should go away)
            if (!flags)
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
            var dataDir = $"maps/{mapEntry.name}";
            var saveDir = Path.Join(Globals.GameFolders.CurrentSaveFolder, "maps", mapEntry.name);

            Logger.Info("Loading Map: {0}", dataDir);

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
                GameSystems.MapFogging.LoadExploredTileData(saveDir);
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
            // Read all mobiles that shipped with the game files
            var mobFiles = Tig.FS.ListDirectory(dataDir).Where(f => f.EndsWith(".mob")).ToArray();

            Logger.Info("Loading {0} map mobiles from {1}", mobFiles.Length, dataDir);

            foreach (var mobFilename in mobFiles)
            {
                var filename = $"{dataDir}/{mobFilename}";

                using var reader = Tig.FS.OpenBinaryReader(filename);
                try
                {
                    GameSystems.Object.LoadFromFile(reader);
                }
                catch (Exception e)
                {
                    Logger.Warn("Unable to load mobile object {0} for level {1}: {2}", filename, dataDir, e);
                }
            }

            Logger.Info("Done loading map mobiles");

            // Read all mobile differences that have accumulated for this map in the save dir
            var diffFilename = Path.Join(saveDir, "mobile.md");

            if (Tig.FS.FileExists(diffFilename))
            {
                Logger.Info("Loading mobile diffs from {0}", diffFilename);

                using var reader = new BinaryReader(new FileStream(diffFilename, FileMode.Open));

                while (!reader.AtEnd())
                {
                    var objId = reader.ReadObjectId();

                    // Get the active handle for the mob so we can apply diffs to it
                    var obj = GameSystems.Object.GetObject(objId);
                    if (obj == null)
                    {
                        throw new Exception(
                            $"Could not retrieve handle for {objId} to apply differences to from {diffFilename}");
                    }

                    obj.LoadDiffsFromFile(reader);

                    if (obj.HasFlag(ObjectFlag.EXTINCT))
                    {
                        GameSystems.Object.Remove(obj);
                    }
                }

                Logger.Info("Done loading map mobile diffs");
            }
            else
            {
                Logger.Info("Skipping mobile diffs, because {0} is missing", diffFilename);
            }

            // Destroy all mobiles that had previously been destroyed
            var desFilename = Path.Join(saveDir, "mobile.des");

            if (File.Exists(desFilename))
            {
                Logger.Info("Loading destroyed mobile file from {0}", desFilename);

                using var reader = new BinaryReader(new FileStream(desFilename, FileMode.Open));

                while (!reader.AtEnd())
                {
                    var objId = reader.ReadObjectId();
                    var obj = GameSystems.Object.GetObject(objId);
                    if (obj != null)
                    {
                        Logger.Debug("{0} ({1}) is destroyed.", GameSystems.MapObject.GetDisplayName(obj), objId);
                        GameSystems.Object.Remove(obj);
                    }
                }

                Logger.Info("Done loading destroyed map mobiles");
            }
            else
            {
                Logger.Info("Skipping destroyed mobile files, because {0} is missing", desFilename);
            }

            ReadDynamicMobiles(saveDir);
        }

        [TempleDllLocation(0x10070610)]
        private void ReadDynamicMobiles(string saveDir)
        {
            var filename = $"{saveDir}/mobile.mdy";

            if (!File.Exists(filename))
            {
                Logger.Info("Skipping dynamic mobiles because {0} doesn't exist.", filename);
                return;
            }

            Logger.Info("Loading dynamic mobiles from {0}", filename);

            using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));

            while (!reader.AtEnd())
            {
                try
                {
                    GameSystems.Object.LoadFromFile(reader);
                }
                catch (Exception e)
                {
                    Logger.Error("Unable to load object: {0}", e);
                    break;
                }
            }

            if (!reader.AtEnd())
            {
                throw new Exception($"Error while reading dynamic mobile file {filename}");
            }

            Logger.Info("Done reading dynamic mobiles.");
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
            var diffFilename = Path.Join(mSectorSaveDir, "mobile.md");
            using var diffOut = new BinaryWriter(new FileStream(diffFilename, FileMode.Create));

            // This file will contain the dynamic ObjectHandles.that have been created on this map
            var dynFilename = Path.Join(mSectorSaveDir, "mobile.mdy");
            using var dynOut = new BinaryWriter(new FileStream(dynFilename, FileMode.Create));

            // This file will contain the object ids of mobile sector ObjectHandles.that have been destroyed
            var destrFilename = Path.Join(mSectorSaveDir, "mobile.des");
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
            GameSystems.MapSector.SaveSectors(flags);
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