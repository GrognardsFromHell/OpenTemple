using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public class SkillSystem : IGameSystem, ISaveGameAwareGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly Dictionary<SkillId, string> _skillNames = new Dictionary<SkillId, string>();

        private readonly Dictionary<SkillId, string> _skillNamesEnglish = new Dictionary<SkillId, string>();

        private readonly Dictionary<SkillId, string> _skillShortDescriptions = new Dictionary<SkillId, string>();

        private readonly Dictionary<string, SkillId> _skillByEnumNames = new Dictionary<string, SkillId>();
        private readonly Dictionary<SkillId, string> _skillEnumNames = new Dictionary<SkillId, string>();

        private readonly Dictionary<SkillId, string> _skillHelpTopics = new Dictionary<SkillId, string>();

        private readonly Dictionary<SkillMessageId, string> _skillMessages = new Dictionary<SkillMessageId, string>();

        private readonly Dictionary<int, string> _skillUiMessages = new Dictionary<int, string>();

        [TempleDllLocation(0x1007cfa0)]
        public SkillSystem()
        {
            Globals.Config.AddVanillaSetting("follower skills", "1");

            var localization = Tig.FS.ReadMesFile("mes/skill.mes");
            var skillRules = Tig.FS.ReadMesFile("rules/skill.mes");
            _skillUiMessages = Tig.FS.ReadMesFile("mes/skill_ui.mes");

            for (int i = 0; i < 42; i++)
            {
                // These two are localized
                _skillNames[(SkillId) i] = localization[i];
                _skillShortDescriptions[(SkillId) i] = localization[5000 + i];

                // This is the original english name
                _skillNamesEnglish[(SkillId) i] = skillRules[i];

                // Maps names such as skill_appraise to the actual enum
                _skillByEnumNames[skillRules[200 + i]] = (SkillId) i;
                _skillEnumNames[(SkillId) i] = skillRules[200 + i];

                // Help topics are optional
                var helpTopic = skillRules[10200 + i];
                if (!string.IsNullOrWhiteSpace(helpTopic))
                {
                    _skillHelpTopics[(SkillId) i] = helpTopic;
                }
            }

            foreach (var msgType in Enum.GetValues(typeof(SkillMessageId)))
            {
                _skillMessages[(SkillMessageId) msgType] = localization[1000 + (int) msgType];
            }
        }

        public string GetSkillUiMessage(int key) => _skillUiMessages[key];

        public string GetSkillEnumName(SkillId skill) => _skillEnumNames[skill];

        [TempleDllLocation(0x1007d2b0)]
        public string GetSkillEnglishName(SkillId skill) => _skillNamesEnglish[skill];

        [TempleDllLocation(0x1007d210)]
        public void ShowSkillMessage(GameObjectBody obj, SkillMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public string GetSkillMessage(SkillMessageId messageId) => _skillMessages[messageId];

        [TempleDllLocation(0x1007d2f0)]
        public bool GetSkillIdFromEnglishName(string enumName, out SkillId skillId)
        {
            foreach (var entry in _skillNamesEnglish)
            {
                if (entry.Value.Equals(enumName, StringComparison.InvariantCultureIgnoreCase))
                {
                    skillId = entry.Key;
                    return true;
                }
            }

            skillId = default;
            return false;
        }

        [TempleDllLocation(0x1007d2c0)]
        public string GetHelpTopic(SkillId skillId)
        {
            return _skillHelpTopics.GetValueOrDefault(skillId, null);
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

        [TempleDllLocation(0x1007daa0)]
        public int AddSkillRanks(GameObjectBody obj, SkillId skillId, int ranksToAdd)
        {
            // Get the last class the character leveled up as
            var levClass = Stat.level_fighter; // default
            var numClasses = obj.GetInt32Array(obj_f.critter_level_idx).Count;
            if (numClasses > 0)
            {
                levClass = (Stat) obj.GetInt32(obj_f.critter_level_idx, numClasses - 1);
            }

            if (D20ClassSystem.IsClassSkill(skillId, levClass) ||
                (levClass == Stat.level_cleric && DeitySystem.IsDomainSkill(obj, skillId))
                || GameSystems.D20.D20QueryPython(obj, "Is Class Skill", skillId) != 0)
            {
                ranksToAdd *= 2;
            }

            var skillPtNew = ranksToAdd + obj.GetInt32(obj_f.critter_skill_idx, (int) skillId);
            if (obj.IsPC())
            {
                var expectedMax = 2 * GameSystems.Stat.StatLevelGet(obj, Stat.level) + 6;
                if (skillPtNew > expectedMax)
                    Logger.Warn("PC {0} has more skill points than they should (has: {1} , expected: {2}",
                        obj, skillPtNew, expectedMax);
            }

            obj.SetInt32(obj_f.critter_skill_idx, (int) skillId, skillPtNew);
            return skillPtNew;
        }

        // 0x4 - search attempt - show only successful attempts
        // 0x8 - search attempt - show only successful attempts (same as 0x4???)
        // 0x10 and on - spell schools
        // 0x2000 - take 20
        // Flags can be various things
        // 1 << (spellSchool + 4)
        [TempleDllLocation(0x1007D530)]
        public bool SkillRoll(GameObjectBody critter, SkillId skill, int dc, out int missedDcBy, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1007d400)]
        public int GetSkillRanks(GameObjectBody critter, SkillId skill)
        {
            return critter.GetInt32(obj_f.critter_skill_idx, (int) skill) / 2;
        }
    }

    public static class CritterSkillExtensions
    {

        public static bool HasRanksIn(this GameObjectBody critter, SkillId skill)
        {
            return GameSystems.Skill.GetSkillRanks(critter, skill) > 0;
        }

    }

}