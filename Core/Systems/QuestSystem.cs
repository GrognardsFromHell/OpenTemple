using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems
{
    [Flags]
    public enum QuestState
    {
        Unknown = 0,
        Mentioned = 1,
        Accepted = 2,
        Achieved = 3,
        Completed = 4,
        Other = 5,
        Botched = 6,
        BotchedFlag = 0x100
    }

    internal class Quest
    {
        public int challengeRating = 1;
        public string title;
        public string description;
        public int fieldC;
        public TimePoint field10;
        public QuestState state;
        public int questArea = -1;

        public bool IsInDefaultState => state == QuestState.Unknown
                                        && field10 == default
                                        && questArea == -1;

        public void Reset()
        {
            state = QuestState.Unknown;
            field10 = default;
            questArea = -1; // NOTE: Vanilla did not reset this
        }
    }

    public class QuestSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        [TempleDllLocation(0x10aa73dc)]
        private readonly Dictionary<int, Quest> _quests = new Dictionary<int, Quest>();

        [TempleDllLocation(0x10aa73d8)]
        private readonly Dictionary<int, QuestState> _questStates = new Dictionary<int, QuestState>();

        [TempleDllLocation(0x1005f660)]
        public QuestSystem()
        {
            var translations = Tig.FS.ReadMesFile("mes/gamequestlog.mes");
            var lines = Tig.FS.ReadMesFile("rules/gamequest.mes");
            foreach (var (questId, challengeRatingStr) in lines)
            {
                var challengeRating = int.Parse(challengeRatingStr);
                _quests[questId] = new Quest
                {
                    challengeRating = challengeRating,
                    title = translations[questId],
                    description = translations[200 + questId]
                };
                _questStates[questId] = QuestState.Unknown;
            }
        }

        [TempleDllLocation(0x1005f2d0)]
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
            foreach (var questId in _quests.Keys)
            {
                _quests[questId].Reset();
                _questStates[questId] = QuestState.Accepted;
            }
        }

        [TempleDllLocation(0x1005f3a0)]
        public void SaveGame(SavedGameState savedGameState)
        {
            var savedQuests = new Dictionary<int, SavedQuestProgress>(_quests.Count);
            // Unlike vanilla, we'll only save progress for quests that are not in the default state
            for (var questId = 0; questId < _quests.Count; questId++)
            {
                var quest = _quests[questId];
                if (!quest.IsInDefaultState)
                {
                    savedQuests[questId] = new SavedQuestProgress(quest.field10, quest.state, quest.questArea);
                }
            }

            savedGameState.QuestsState = new SavedQuestsState
            {
                Quests = savedQuests
            };
        }

        [TempleDllLocation(0x1005f320)]
        public void LoadGame(SavedGameState savedGameState)
        {
            // This assumes reset was already called
            foreach (var (questId, savedQuestProgress) in savedGameState.QuestsState.Quests)
            {
                if (!_quests.TryGetValue(questId, out var quest))
                {
                    throw new CorruptSaveException($"Save references quest id {questId}, which doesn't exist.");
                }

                quest.field10 = savedQuestProgress.LastStateChange;
                quest.state = savedQuestProgress.State;
                quest.questArea = savedQuestProgress.QuestArea;
            }
        }

        [TempleDllLocation(0x1005f4c0)]
        public QuestState GetState(int questId)
        {
            var quest = _quests[questId];
            if ((quest.state & QuestState.BotchedFlag) != 0)
            {
                return QuestState.Botched;
            }

            return quest.state;
        }

        [TempleDllLocation(0x1005f780)]
        public void SetState(int questId, QuestState newState)
        {
            var quest = _quests[questId];

            if ((quest.state & QuestState.BotchedFlag) == 0
                && quest.state != QuestState.Completed
                && quest.state != QuestState.Other
                && quest.state != QuestState.Botched
                && quest.state < newState)
            {
                if (quest.state == QuestState.Unknown)
                {
                    var mapId = GameSystems.Map.GetCurrentMapId();
                    quest.questArea = GameSystems.Area.GetAreaFromMap(mapId);
                }

                GameUiBridge.PulseLogbookButton();
                if (newState == QuestState.Completed)
                {
                    GameSystems.D20.Combat.AwardExperienceForChallengeRating(quest.challengeRating);
                    if (!GameSystems.Combat.IsCombatActive())
                    {
                        GameSystems.D20.Combat.AwardExperience();
                    }
                }

                SetStateInternal(questId, newState);
            }
        }

        [TempleDllLocation(0x1005f4e0)]
        private QuestState SetStateInternal(int questId, QuestState newstate)
        {
            var curState = _questStates[questId];
            if (curState != QuestState.Accepted)
            {
                if (curState != QuestState.Completed)
                {
                    newstate = QuestState.Botched;
                    _quests[questId].state |= QuestState.BotchedFlag;
                }
                else
                {
                    newstate = QuestState.Other;
                    _quests[questId].state = QuestState.Other;
                }
            }
            else
            {
                switch (newstate)
                {
                    case QuestState.Accepted:
                    case QuestState.Other:
                    {
                        _questStates[questId] = QuestState.Accepted;
                        if (newstate == QuestState.Botched)
                        {
                            _quests[questId].state |= QuestState.BotchedFlag;
                        }
                        else
                        {
                            _quests[questId].state = newstate;
                        }

                        break;
                    }
                    case QuestState.Botched:
                        _questStates[questId] = QuestState.Botched;
                        _quests[questId].state |= QuestState.BotchedFlag;
                        break;
                    default:
                        _quests[questId].state = newstate;
                        break;
                }
            }

            _quests[questId].field10 = GameSystems.TimeEvent.GameTime;
            if (newstate == QuestState.Completed)
            {
                Tig.Sound.PlaySoundEffect(3028);
            }

            return newstate;
        }

        [TempleDllLocation(0x1005f820)]
        public void Unbotch(int questId)
        {
            var result = _questStates[questId];
            if (result == QuestState.Botched)
            {
                _questStates[questId] = QuestState.Accepted;
                _quests[questId].state &= ~QuestState.BotchedFlag;
                _quests[questId].field10 = GameSystems.TimeEvent.GameTime;

                if (_quests[questId].state == QuestState.Completed)
                {
                    Tig.Sound.PlaySoundEffect(3028);
                }
            }
        }
    }
}