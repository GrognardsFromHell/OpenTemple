using System;
using System.Collections.Generic;
using System.Linq;
using SpicyTemple.Core.IO.SaveGames.GameState;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems
{
    public class RandomEncounterSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        [TempleDllLocation(0x10045bb0)]
        [TempleDllLocation(0x109dd854)]
        public SleepStatus SleepStatus { get; set; }

        private List<int> _encounterQueue = new List<int>();

        public bool TryTakeQueuedEncounter(out int encounterId)
        {
            if (_encounterQueue.Count > 0)
            {
                encounterId = _encounterQueue[0];
                _encounterQueue.RemoveAt(0);
                return true;
            }

            encounterId = -1;
            return false;
        }

        public bool IsEncounterQueued(int encounterId) => _encounterQueue.Contains(encounterId);

        public void Dispose()
        {
        }

        public void SaveGame(SavedGameState savedGameState)
        {
            if (savedGameState.ScriptState == null)
            {
                savedGameState.ScriptState = new SavedScriptState();
            }

            savedGameState.ScriptState.EncounterQueue = _encounterQueue.ToList();
        }

        public void LoadGame(SavedGameState savedGameState)
        {
            _encounterQueue = savedGameState.ScriptState.EncounterQueue.ToList();
            UpdateSleepStatus();
        }

        [TempleDllLocation(0x10045850)]
        public void UpdateSleepStatus()
        {
            Stub.TODO();
        }

        public void QueueRandomEncounter(int encounterId)
        {
            _encounterQueue.Add(encounterId);
        }

        public void RemoveQueuedEncounter(int encounterId)
        {
            _encounterQueue.Remove(encounterId);
        }

        [TempleDllLocation(0x100461E0)]
        public bool Query(RandomEncounterQuery query, out object o)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100462F0)]
        public void CreateEncounter(object o)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            // NOTE: This was previously being reset in the script system
            _encounterQueue.Clear();

        }
    }

    public enum RandomEncounterType
    {
        Traveling = 0,
        Resting = 1
    }

    public class RandomEncounterQuery
    {
        public MapTerrain Terrain { get; }

        public RandomEncounterType Type { get; }

        public RandomEncounterQuery(MapTerrain terrain, RandomEncounterType type)
        {
            Terrain = terrain;
            Type = type;
        }
    }

    public readonly struct RandomEncounterEnemy
    {
        public readonly int ProtoId;
        public readonly int Count;

        public RandomEncounterEnemy(int protoId, int count)
        {
            ProtoId = protoId;
            Count = count;
        }
    }

    public class RandomEncounter
    {
        public int Id { get; set; }
        public int Flags { get; set; }
        public int Title { get; set; }
        public int DC { get; set; }
        public int Map { get; set; }
        public List<RandomEncounterEnemy> Enemies { get; set; } = new List<RandomEncounterEnemy>();
        public locXY Location { get; set; }

        public void AddEnemies(int protoId, int count)
        {
            // Merge with existing enemies of the same type
            for (var i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i].ProtoId == protoId)
                {
                    Enemies[i] = new RandomEncounterEnemy(protoId, Enemies[i].Count + count);
                    return;
                }
            }

            Enemies.Add(new RandomEncounterEnemy(protoId, count));
        }

    }
}