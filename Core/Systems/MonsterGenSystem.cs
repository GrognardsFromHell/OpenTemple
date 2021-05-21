using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.GameObjects;

namespace OpenTemple.Core.Systems
{
    public class MonsterGenSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        /// <summary>
        /// Tracks global spawner state.
        /// </summary>
        private struct GlobalSpawnerState
        {
            // This flag was only ever set by old legacy scripts, probably Arkanum's script command to toggle monster gens
            public bool IsDisabled { get; set; } // Previously indicated by 0x80

            public int CurrentlySpawned { get; set; } // To a maximum of 0x1F
        }

        [TempleDllLocation(0x10aa3288)]
        private readonly GlobalSpawnerState[] _globalSpawnerState;

        [TempleDllLocation(0x100500c0)]
        public MonsterGenSystem()
        {
            _globalSpawnerState = new GlobalSpawnerState[0x100];
        }

        [TempleDllLocation(0x10050160)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10050140)]
        public void Reset()
        {
            Array.Fill(_globalSpawnerState, default);
        }

        [TempleDllLocation(0x100501d0)]
        public void SaveGame(SavedGameState savedGameState)
        {
            var savedSpawners = new List<SavedSpawnerState>();
            for (var i = 0; i < _globalSpawnerState.Length; i++)
            {
                var globalSpawnerState = _globalSpawnerState[i];
                if (globalSpawnerState.IsDisabled || globalSpawnerState.CurrentlySpawned > 0)
                {
                    savedSpawners.Add(new SavedSpawnerState
                    {
                        Id = i,
                        IsDisabled = globalSpawnerState.IsDisabled,
                        CurrentlySpawned = globalSpawnerState.CurrentlySpawned
                    });
                }
            }

            savedGameState.MonsterGenState = new SavedMonsterGenState
            {
                Spawners = savedSpawners
            };
        }

        [TempleDllLocation(0x100501a0)]
        public void LoadGame(SavedGameState savedGameState)
        {
            foreach (var savedSpawner in savedGameState.MonsterGenState.Spawners)
            {
                _globalSpawnerState[savedSpawner.Id].IsDisabled = savedSpawner.IsDisabled;
                _globalSpawnerState[savedSpawner.Id].CurrentlySpawned = savedSpawner.CurrentlySpawned;
            }
        }

        [TempleDllLocation(0x100508c0)]
        public void CritterKilled(GameObjectBody critter)
        {
            if ( critter.IsNPC() )
            {
                var npcFlags = critter.GetNPCFlags();
                if ( (npcFlags & NpcFlag.GENERATED) != 0 )
                {
                    GetNpcGeneratorInfoFromField(critter, out var npcGenInfo);
                    SetCurrentlySpawned(npcGenInfo.generatorId, npcGenInfo.currentlySpawned - 1);
                }
            }
        }

        enum SpawnRate
        {
            Second = 0,
            HalfMinute = 1,
            Minute = 2,
            Hour = 3,
            Day = 4,
            Week = 5,
            Month = 6,
            Year = 7
        }

        [TempleDllLocation(0x102b1d98)]
        private static readonly Dictionary<SpawnRate, TimeSpan> SpawnRateDelays = new Dictionary<SpawnRate, TimeSpan>
        {
            {SpawnRate.Second, TimeSpan.FromSeconds(1)},
            {SpawnRate.HalfMinute, TimeSpan.FromSeconds(30)},
            {SpawnRate.Minute, TimeSpan.FromMinutes(1)},
            {SpawnRate.Hour, TimeSpan.FromHours(1)},
            {SpawnRate.Day, TimeSpan.FromDays(1)},
            {SpawnRate.Week, TimeSpan.FromDays(7)},
            {SpawnRate.Month, TimeSpan.FromDays(28)},
            {SpawnRate.Year, TimeSpan.FromDays(364)}
        };

        [Flags]
        private enum SpawnerFlag
        {
            // Daytime and nighttime can be combined
            Daytime = 0x01,
            Nighttime = 0x02,
            InactiveOnScreen = 0x04,
            SpawnAllAtOnce = 0x08,
            IgnoreTotal = 0x10
        }

        private SpawnRate GetSpawnRate(GameObjectBody obj)
        {
            var npcFlags = obj.GetNPCFlags();
            var result = 0;
            if ((npcFlags & NpcFlag.GENERATOR_RATE1) != 0)
            {
                result += 1;
            }

            if ((npcFlags & NpcFlag.GENERATOR_RATE2) != 0)
            {
                result += 2;
            }

            if ((npcFlags & NpcFlag.GENERATOR_RATE3) != 0)
            {
                result += 4;
            }

            return (SpawnRate) result;
        }

        private void SetSpawnRate(GameObjectBody obj, SpawnRate spawnRate)
        {
            var npcFlags = obj.GetNPCFlags();
            npcFlags &= ~(NpcFlag.GENERATOR_RATE1 | NpcFlag.GENERATOR_RATE2 | NpcFlag.GENERATOR_RATE3);

            switch (spawnRate)
            {
                case SpawnRate.Second:
                    break;
                case SpawnRate.HalfMinute:
                    npcFlags |= NpcFlag.GENERATOR_RATE1;
                    break;
                case SpawnRate.Minute:
                    npcFlags |= NpcFlag.GENERATOR_RATE2;
                    break;
                case SpawnRate.Hour:
                    npcFlags |= NpcFlag.GENERATOR_RATE1 | NpcFlag.GENERATOR_RATE2;
                    break;
                case SpawnRate.Day:
                    npcFlags |= NpcFlag.GENERATOR_RATE3;
                    break;
                case SpawnRate.Week:
                    npcFlags |= NpcFlag.GENERATOR_RATE1 | NpcFlag.GENERATOR_RATE3;
                    break;
                case SpawnRate.Month:
                    npcFlags |= NpcFlag.GENERATOR_RATE2 | NpcFlag.GENERATOR_RATE3;
                    break;
                case SpawnRate.Year:
                    npcFlags |= NpcFlag.GENERATOR_RATE1 | NpcFlag.GENERATOR_RATE2 | NpcFlag.GENERATOR_RATE3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spawnRate), spawnRate, null);
            }

            obj.SetNPCFlags(npcFlags);
        }

        private bool GetGlobalSpawnerState(int generatorId, out GlobalSpawnerState state)
        {
            if (generatorId >= 0 && generatorId < _globalSpawnerState.Length)
            {
                state = _globalSpawnerState[generatorId];
                return true;
            }

            state = default;
            return false;
        }

        [TempleDllLocation(0x10050200)]
        private void SetCurrentlySpawned(int generatorId, int currentlySpawned)
        {
            if (generatorId >= 0
                && generatorId < _globalSpawnerState.Length
                && currentlySpawned >= 0
                && currentlySpawned <= 0x1F)
            {
                _globalSpawnerState[generatorId].CurrentlySpawned = currentlySpawned;
            }
        }

        private void SetGlobalSpawnerFlag(int generatorId, bool flag)
        {
            if (generatorId >= 0 && generatorId < _globalSpawnerState.Length)
            {
                _globalSpawnerState[generatorId].IsDisabled = flag;
            }
        }

        [TempleDllLocation(0x10050590)]
        private void GetNpcGeneratorInfoFromField(GameObjectBody obj, out NpcGeneratorInfo npcGenInfo)
        {
            var pickledGeneratorData = obj.GetInt32(obj_f.npc_generator_data);
            npcGenInfo = default;
            npcGenInfo.flags = (SpawnerFlag) ((pickledGeneratorData >> 27) & 0x1F);
            npcGenInfo.maxConcurrent = (pickledGeneratorData >> 14) & 0x1F;
            npcGenInfo.obj = obj;
            npcGenInfo.generatorId = (pickledGeneratorData >> 19) & 0xFF;
            npcGenInfo.maxTotal = (pickledGeneratorData >> 7) & 0x7F;
            npcGenInfo.totalSpawned = pickledGeneratorData & 0x7F;
            npcGenInfo.spawnRate = GetSpawnRate(obj);

            npcGenInfo.IsActive = true;
            if (GetGlobalSpawnerState(npcGenInfo.generatorId, out var globalSpawnerState))
            {
                npcGenInfo.currentlySpawned = globalSpawnerState.CurrentlySpawned;
                if (globalSpawnerState.IsDisabled)
                {
                    npcGenInfo.IsActive = false;
                }
            }
        }

        struct NpcGeneratorInfo
        {
            public SpawnerFlag flags;
            public GameObjectBody obj;
            public int generatorId;
            public int maxConcurrent;
            public int maxTotal;
            public SpawnRate spawnRate;
            public bool IsActive; // This flag was only filled by old legacy scripts
            public int currentlySpawned;
            public int totalSpawned;

            public bool IsActiveDuringDay => (flags & SpawnerFlag.Daytime) != 0;
            public bool IsActiveDuringNight => (flags & SpawnerFlag.Nighttime) != 0;
            public bool IsInactiveOnScreen => (flags & SpawnerFlag.InactiveOnScreen) != 0;
            public bool IsSpawnAllAtOnce => (flags & SpawnerFlag.SpawnAllAtOnce) != 0;
            public bool IsIgnoreTotal => (flags & SpawnerFlag.IgnoreTotal) != 0;
        }

        [TempleDllLocation(0x10050740)]
        public bool ProcessSpawnerTick(GameObjectBody generator, out TimeSpan delay)
        {
            if (!generator.IsNPC() || (generator.GetNPCFlags() & NpcFlag.GENERATOR) == 0)
            {
                delay = default;
                return false;
            }

            GetNpcGeneratorInfoFromField(generator, out var npcGenInfo);
            if (npcGenInfo.IsActive)
            {
                HandleSpawning(ref npcGenInfo);
            }

            delay = SpawnRateDelays[npcGenInfo.spawnRate];
            return true;
        }

        private bool IsRectOnScreen(Rectangle rect)
        {
            // TODO: This should probably check every game view not just the primary
            var screenRect = new Rectangle(Point.Empty, GameViews.Primary.Size);
            return rect.IntersectsWith(screenRect);
        }

        private void HandleSpawning(ref NpcGeneratorInfo npcGenInfo)
        {
            var maxSpawnBudget = npcGenInfo.maxConcurrent - npcGenInfo.currentlySpawned;
            if (maxSpawnBudget <= 0)
            {
                return;
            }

            if (GetGlobalSpawnerState(npcGenInfo.generatorId, out var globalSpawnerState)
                && globalSpawnerState.IsDisabled)
            {
                return;
            }

            if (GameSystems.TimeEvent.IsDaytime ? !npcGenInfo.IsActiveDuringDay : !npcGenInfo.IsActiveDuringNight)
            {
                return;
            }

            if (npcGenInfo.IsInactiveOnScreen)
            {
                // TODO: This should probably check every game view, not just the primary
                var rect = GameSystems.MapObject.GetObjectRect(
                    GameViews.Primary,
                    npcGenInfo.obj,
                    MapObjectSystem.ObjectRectFlags.IgnoreHidden);
                if (IsRectOnScreen(rect))
                {
                    return;
                }
            }

            if (!npcGenInfo.IsIgnoreTotal)
            {
                if (npcGenInfo.maxTotal - npcGenInfo.totalSpawned <= 0)
                {
                    return;
                }

                if (maxSpawnBudget > npcGenInfo.maxTotal - npcGenInfo.totalSpawned)
                {
                    maxSpawnBudget = npcGenInfo.maxTotal - npcGenInfo.totalSpawned;
                }
            }

            if (maxSpawnBudget > 0)
            {
                if (!npcGenInfo.IsSpawnAllAtOnce)
                {
                    maxSpawnBudget = 1;
                }

                var actuallySpawned = SpawnClones(ref npcGenInfo, maxSpawnBudget);
                npcGenInfo.currentlySpawned += actuallySpawned;
                if (!npcGenInfo.IsIgnoreTotal)
                {
                    npcGenInfo.totalSpawned += actuallySpawned;
                }

                SaveGeneratorState(ref npcGenInfo);
            }
        }

        [TempleDllLocation(0x10050660)]
        private void SaveGeneratorState(ref NpcGeneratorInfo npcGenInfo)
        {
            var spawner = npcGenInfo.obj;

            var pickledGeneratorData = 0;
            pickledGeneratorData |= ((int) npcGenInfo.flags & 0x1F) << 27;
            pickledGeneratorData |= (npcGenInfo.generatorId & 0xFF) << 19;
            pickledGeneratorData |= (npcGenInfo.maxConcurrent & 0x1F) << 14;
            pickledGeneratorData |= (npcGenInfo.maxTotal & 0x7F) << 7;
            pickledGeneratorData |= npcGenInfo.totalSpawned & 0x7F;
            spawner.SetInt32(obj_f.npc_generator_data, pickledGeneratorData);

            SetCurrentlySpawned(npcGenInfo.generatorId, npcGenInfo.currentlySpawned);
            SetSpawnRate(spawner, npcGenInfo.spawnRate);

            SetGlobalSpawnerFlag(npcGenInfo.generatorId, !npcGenInfo.IsActive);
        }

        [TempleDllLocation(0x10050240)]
        private int SpawnClones(ref NpcGeneratorInfo npcGenInfo, int count)
        {
            var actuallySpawned = 0;
            for (var i = 0; i < count; i++)
            {
                if (!FindNextSpawnLocation(ref npcGenInfo, out var locOut))
                {
                    continue;
                }

                var spawnedObj = GameSystems.MapObject.CloneObject(npcGenInfo.obj, locOut);

                // Clear the flags that kept the spawner from being visible
                GameSystems.MapObject.ClearFlags(spawnedObj, ObjectFlag.INVULNERABLE | ObjectFlag.OFF);

                var npcFlags = spawnedObj.GetNPCFlags();
                npcFlags &= ~NpcFlag.GENERATOR;
                npcFlags |= NpcFlag.GENERATED;
                spawnedObj.SetNPCFlags(npcFlags);
                actuallySpawned++;
            }

            return actuallySpawned;
        }

        private bool FindNextSpawnLocation(ref NpcGeneratorInfo npcGenInfo, out LocAndOffsets location)
        {
            var spawnerLocation = npcGenInfo.obj.GetLocationFull();
            // The spawned objects are just clones of the spawner, so the radius will stay the same
            var spawnedObjRadius = npcGenInfo.obj.GetRadius();

            if (npcGenInfo.maxConcurrent == 1)
            {
                location = spawnerLocation;
                return true;
            }

            var spawnerIsOutdoors = GameSystems.Tile.MapTileIsOutdoors(spawnerLocation.location);

            // this is a bit hoky and inprecise due to tiles vs. locwithoffsets
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var distance = GameSystems.Random.GetInt(1, 5);
                if (!GameSystems.Location.TargetRandomTile(npcGenInfo.obj, distance, out var tile))
                {
                    continue;
                }

                // I presume this is to prevent spawners from spawning through walls of houses and such
                // It's possible ToEE doesn't use this anymore since interiors are separate maps
                var locationIsOutdoors = GameSystems.Tile.MapTileIsOutdoors(tile);
                if (spawnerIsOutdoors != locationIsOutdoors)
                {
                    continue;
                }

                using var blockers = ObjList.ListRadius(
                    new LocAndOffsets(tile),
                    spawnedObjRadius,
                    ObjectListFilter.OLC_CRITTERS
                );
                if (blockers.Count > 0 && blockers[0] != npcGenInfo.obj)
                {
                    continue;
                }

                if (npcGenInfo.IsInactiveOnScreen)
                {
                    // Determine where on screen the spawned object _would_ be.
                    // TODO: This should not use the spawner object as the rect size, and test every game view
                    var spawnerRect = GameSystems.MapObject.GetObjectRect(
                        GameViews.Primary,
                        npcGenInfo.obj,
                        MapObjectSystem.ObjectRectFlags.IgnoreHidden);

                    var spawnedAtInUi = GameViews.Primary.WorldToScreen(tile.ToInches3D());
                    var spawnedExclusionRect = new Rectangle(
                        (int) spawnedAtInUi.X,
                        (int) spawnedAtInUi.Y,
                        spawnerRect.Width,
                        spawnerRect.Height
                    );

                    if (IsRectOnScreen(spawnedExclusionRect))
                    {
                        continue;
                    }
                }

                location = new LocAndOffsets(tile);
                return true;
            }

            location = default;
            return false;
        }
    }
}