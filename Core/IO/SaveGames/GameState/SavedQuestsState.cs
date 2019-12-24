using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.IO.SaveGames.GameState
{
    public class SavedQuestsState
    {
        public Dictionary<int, SavedQuestProgress> Quests = new Dictionary<int, SavedQuestProgress>(200);

        [TempleDllLocation(0x1005f320)]
        public static SavedQuestsState Read(BinaryReader reader)
        {
            var result = new SavedQuestsState();
            for (var i = 0; i < 200; i++)
            {
                var lastChange = reader.ReadGameTime().ToTimePoint();
                var state = reader.ReadInt32();
                var areaId = reader.ReadInt32();

                if (lastChange != default || state != 0 || areaId != -1)
                {
                    if (!TryConvertQuestState(state, out var actualState))
                    {
                        throw new CorruptSaveException($"Unknown quest state: {state}");
                    }

                    result.Quests[i] = new SavedQuestProgress(
                        lastChange,
                        actualState,
                        areaId
                    );
                }
            }

            return result;
        }

        private static bool TryConvertQuestState(int savedState, out QuestState state)
        {
            state = default;
            var setBotchedFlag = (savedState & 0x100) != 0;
            savedState &= ~0x100;

            switch (savedState)
            {
                case 0:
                    state = QuestState.Unknown;
                    break;
                case 1:
                    state = QuestState.Mentioned;
                    break;
                case 2:
                    state = QuestState.Accepted;
                    break;
                case 3:
                    state = QuestState.Achieved;
                    break;
                case 4:
                    state = QuestState.Completed;
                    break;
                case 5:
                    state = QuestState.Other;
                    break;
                case 6:
                    // I believe this should not occur
                    state = QuestState.Botched;
                    break;
                default:
                    return false;
            }

            if (setBotchedFlag)
            {
                state |= QuestState.BotchedFlag;
            }

            return true;
        }
    }

    public readonly struct SavedQuestProgress
    {
        public readonly TimePoint LastStateChange;

        public readonly QuestState State;

        public readonly int QuestArea;

        public SavedQuestProgress(TimePoint lastStateChange, QuestState state, int questArea)
        {
            LastStateChange = lastStateChange;
            State = state;
            QuestArea = questArea;
        }
    }
}