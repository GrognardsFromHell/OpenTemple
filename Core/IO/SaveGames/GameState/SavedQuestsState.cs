using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Logbook;

namespace OpenTemple.Core.IO.SaveGames.GameState;

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

    [TempleDllLocation(0x1005f3a0)]
    public void Write(BinaryWriter writer)
    {
        for (var i = 0; i < 200; i++)
        {
            if (!Quests.TryGetValue(i, out var quest))
            {
                // Write out a dummy record
                writer.WriteGameTime((GameTime) default);
                writer.WriteInt32(0); // State
                writer.WriteInt32(-1); // Discovered in area ID
                continue;
            }

            if (!TryConvertQuestState(quest.State, out var state))
            {
                throw new CorruptSaveException("Failed to convert quest state: " + quest.State);
            }

            writer.WriteGameTime(quest.LastStateChange);
            writer.WriteInt32(state);
            writer.WriteInt32(quest.QuestArea);
        }
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

    private static bool TryConvertQuestState(QuestState state, out int savedState)
    {
        savedState = default;

        switch (state & ~QuestState.BotchedFlag)
        {
            case QuestState.Unknown:
                savedState = 0;
                break;
            case QuestState.Mentioned:
                savedState = 1;
                break;
            case QuestState.Accepted:
                savedState = 2;
                break;
            case QuestState.Achieved:
                savedState = 3;
                break;
            case QuestState.Completed:
                savedState = 4;
                break;
            case QuestState.Other:
                savedState = 5;
                break;
            case QuestState.Botched:
                // I believe this should not occur
                savedState = 6;
                break;
            default:
                return false;
        }

        if ((state & QuestState.BotchedFlag) != 0)
        {
            savedState |= 0x100;
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