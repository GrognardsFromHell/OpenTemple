using System;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems
{
    public enum QuestState
    {
        Unknown = 0,
        Mentioned = 1,
        Accepted = 2,
        Achieved = 3,
        Completed = 4,
        Other = 5,
        Botched = 6
    }

    public class QuestSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        public QuestSystem()
        {
            // TODO quests
        }

        public void Dispose()
        {
            // TODO quests
        }

        [TempleDllLocation(0x1005f310)]
        public void LoadModule()
        {
            Reset();
        }

        public void UnloadModule()
        {
            // TODO quests
        }

        [TempleDllLocation(0x1005f2a0)]
        public void Reset()
        {
            // TODO quests
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005f4c0)]
        public QuestState GetState(int questId)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005f780)]
        public void SetState(int questId, QuestState newState)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005f820)]
        public void Unbotch(int questId)
        {
            throw new NotImplementedException();
        }
    }
}