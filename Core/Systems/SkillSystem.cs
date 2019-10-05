using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.InGameSelect;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    internal struct SkillProps
    {
        public readonly Stat Stat; // associated stat (e.g. stat_intelligence for Appraise)
        public readonly bool UntrainedUse;
        public readonly bool Disabled;

        public SkillProps(Stat stat, bool untrainedUse = false, bool disabled = false)
        {
            Stat = stat;
            UntrainedUse = untrainedUse;
            Disabled = disabled;
        }
    }

    public class SkillSystem : IGameSystem, ISaveGameAwareGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x102cba30)]
        private readonly Dictionary<SkillId, SkillProps> _skills = new Dictionary<SkillId, SkillProps>();

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

            InitVanillaSkills();
        }

        private void InitVanillaSkills()
        {
            _skills[SkillId.appraise] = new SkillProps(Stat.intelligence, true);
            _skills[SkillId.bluff] = new SkillProps(Stat.charisma, true);
            _skills[SkillId.concentration] = new SkillProps(Stat.constitution, true);
            _skills[SkillId.diplomacy] = new SkillProps(Stat.charisma, true);
            _skills[SkillId.disable_device] = new SkillProps(Stat.intelligence);
            _skills[SkillId.gather_information] = new SkillProps(Stat.charisma, true);
            _skills[SkillId.heal] = new SkillProps(Stat.wisdom, true);
            _skills[SkillId.hide] = new SkillProps(Stat.dexterity, true);
            _skills[SkillId.intimidate] = new SkillProps(Stat.charisma, true);
            _skills[SkillId.listen] = new SkillProps(Stat.wisdom, true);
            _skills[SkillId.move_silently] = new SkillProps(Stat.dexterity, true);
            _skills[SkillId.open_lock] = new SkillProps(Stat.dexterity);
            _skills[SkillId.pick_pocket] = new SkillProps(Stat.dexterity);
            _skills[SkillId.search] = new SkillProps(Stat.intelligence, true);
            _skills[SkillId.sense_motive] = new SkillProps(Stat.wisdom, true);
            _skills[SkillId.spellcraft] = new SkillProps(Stat.intelligence);
            _skills[SkillId.spot] = new SkillProps(Stat.wisdom, true);
            _skills[SkillId.tumble] = new SkillProps(Stat.dexterity);
            _skills[SkillId.use_magic_device] = new SkillProps(Stat.charisma);
            _skills[SkillId.wilderness_lore] = new SkillProps(Stat.wisdom, true);
            _skills[SkillId.perform] = new SkillProps(Stat.charisma, true);
            _skills[SkillId.alchemy] = new SkillProps(Stat.intelligence, true, true);
            _skills[SkillId.balance] = new SkillProps(Stat.dexterity, false, true);
            _skills[SkillId.climb] = new SkillProps(Stat.strength, false, true);
            _skills[SkillId.craft] = new SkillProps(Stat.intelligence, false, true);
            _skills[SkillId.decipher_script] = new SkillProps(Stat.intelligence, false, true);
            _skills[SkillId.disguise] = new SkillProps(Stat.charisma, false, true);
            _skills[SkillId.escape_artist] = new SkillProps(Stat.dexterity, false, true);
            _skills[SkillId.forgery] = new SkillProps(Stat.intelligence, false, true);
            _skills[SkillId.handle_animal] = new SkillProps(Stat.charisma, false, true);
            _skills[SkillId.innuendo] = new SkillProps(Stat.wisdom, false, true);
            _skills[SkillId.intuit_direction] = new SkillProps(Stat.wisdom, false, true);
            _skills[SkillId.jump] = new SkillProps(Stat.strength, false, true);
            _skills[SkillId.knowledge_arcana] = new SkillProps(Stat.intelligence, false, true);
            _skills[SkillId.knowledge_religion] = new SkillProps(Stat.intelligence, false, true);
            _skills[SkillId.knowledge_nature] = new SkillProps(Stat.intelligence, false, true);
            _skills[SkillId.knowledge_all] = new SkillProps(Stat.intelligence, false, true);
            _skills[SkillId.profession] = new SkillProps(Stat.wisdom, false, true);
            _skills[SkillId.read_lips] = new SkillProps(Stat.intelligence, false, true);
            _skills[SkillId.ride] = new SkillProps(Stat.dexterity, false, true);
            _skills[SkillId.swim] = new SkillProps(Stat.strength, false, true);
            _skills[SkillId.use_rope] = new SkillProps(Stat.dexterity, false, true);
        }

        [TempleDllLocation(0x1007d280)]
        public string GetSkillName(SkillId skill) => _skillNames[skill];

        public string GetSkillUiMessage(int key) => _skillUiMessages[key];

        public string GetSkillEnumName(SkillId skill) => _skillEnumNames[skill];

        [TempleDllLocation(0x1007d290)]
        public string GetShortDescription(SkillId skill) => _skillShortDescriptions[skill];

        [TempleDllLocation(0x1007d2b0)]
        public string GetSkillEnglishName(SkillId skill) => _skillNamesEnglish[skill];

        [TempleDllLocation(0x1007d210)]
        public void ShowSkillMessage(GameObjectBody obj, SkillMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public string GetSkillMessage(SkillMessageId messageId) => _skillMessages[messageId];

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

        [TempleDllLocation(0x1007d2f0)]
        public bool GetSkillIdFromEnumName(string enumName, out SkillId skillId)
        {
            return _skillByEnumNames.TryGetValue(enumName.ToLowerInvariant(), out skillId);
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
        [TempleDllLocation(0x1007dba0)]
        public bool SkillRoll(GameObjectBody critter, SkillId skill, int dc, out int missedDcBy, SkillCheckFlags flags)
        {
            if (!CanUseSkill(critter, skill))
            {
                missedDcBy = -dc;
                return false;
            }

            var skillBonusList = BonusList.Create();
            var skillLvl = DispatcherExtensions.dispatch1ESkillLevel(critter, skill, ref skillBonusList, null, flags);

            Dice dice;
            if ((flags & SkillCheckFlags.TakeTwenty) != 0)
            {
                dice = Dice.Constant(20);
            }
            else
            {
                dice = Dice.D20;
            }

            var rollResult = dice.Roll();
            var effectiveResult = rollResult + skillLvl;
            var succeeded = effectiveResult >= dc;
            missedDcBy = effectiveResult - dc;


            bool showResultInHistory;
            if (skill == SkillId.search)
            {
                if ((flags & SkillCheckFlags.SearchForTraps) != 0)
                {
                    showResultInHistory = succeeded;
                }
                else if ((flags & SkillCheckFlags.SearchForSecretDoors) != 0)
                {
                    showResultInHistory = succeeded;
                }
                else
                {
                    showResultInHistory = true;
                }
            }
            else
            {
                showResultInHistory = true;
            }

            if (showResultInHistory)
            {
                var histId =
                    GameSystems.RollHistory.AddSkillCheck(critter, null, skill, dice, rollResult, dc, skillBonusList);
                GameSystems.RollHistory.CreateRollHistoryString(histId);
            }

            return succeeded;
        }

        [TempleDllLocation(0x1007d400)]
        public int GetSkillRanks(GameObjectBody critter, SkillId skill)
        {
            return critter.GetInt32(obj_f.critter_skill_idx, (int) skill) / 2;
        }

        [TempleDllLocation(0x1007d260)]
        public int GetSkillHalfRanks(GameObjectBody critter, SkillId skill)
        {
            return critter.GetInt32(obj_f.critter_skill_idx, (int) skill);
        }

        [TempleDllLocation(0x1007da10)]
        public bool PickInventoryScroll(GameObjectBody critter)
        {
            var pickargs = new PickerArgs();
            pickargs.caster = critter;
            pickargs.modeTarget = UiPickerType.InventoryItem;
            pickargs.incFlags = UiPickerIncFlags.UIPI_Scroll;
            pickargs.callback = (ref PickerResult result, object o) =>
                PickerInventoryCallback(ref result, critter);
            pickargs.maxTargets = 1;
            return GameUiBridge.ShowPicker(pickargs);
        }

        [TempleDllLocation(0x1007d8b0)]
        private void PickerInventoryCallback(ref PickerResult result, GameObjectBody critter)
        {
            throw new NotImplementedException();
        }

        // TODO Shouldn't this be a list... You could be able to find more than one door with a single use of the skill
        [TempleDllLocation(0x1007dbd0)]
        public bool TryUseSearchSkill(GameObjectBody seeker, out GameObjectBody found)
        {
            found = null;

            var bonlist = BuildBonusListWithSearchSupporters(seeker);

            GameSystems.Anim.Interrupt(seeker, AnimGoalPriority.AGP_3);
            GameSystems.Anim.PushAnimate(seeker, NormalAnimType.SkillSearch);

            using var listResult = ObjList.ListVicinity(seeker, ObjectListFilter.OLC_ALL);

            foreach (var nearObj in listResult)
            {
                if (GameSystems.Combat.HasLineOfAttack(seeker, nearObj))
                {
                    if (GameSystems.Trap.WillTrigger(nearObj) || nearObj.type == ObjectType.trap)
                    {
                        if (GameSystems.Trap.TryToDetect(seeker, nearObj, bonlist))
                        {
                            found = nearObj;
                            break;
                        }
                    }
                    else if (nearObj.type == ObjectType.portal
                             && nearObj.IsSecretDoor()
                             && GameSystems.Secretdoor.TryFindSecretDoor(nearObj, seeker, bonlist))
                    {
                        found = nearObj;
                        break;
                    }
                }
            }

            return found != null;
        }

        /// <summary>
        /// Let other party members roll against DC10 to support the searching character.
        /// </summary>
        private BonusList BuildBonusListWithSearchSupporters(GameObjectBody seeker)
        {
            var bonlist = BonusList.Default;
            using var listResult = ObjList.ListVicinity(seeker, ObjectListFilter.OLC_CRITTERS);
            foreach (var supportingCritter in listResult)
            {
                if (supportingCritter != seeker)
                {
                    if (GameSystems.Party.IsInParty(supportingCritter))
                    {
                        if (!GameSystems.Critter.IsDeadOrUnconscious(supportingCritter))
                        {
                            if (TrySupportingSkillCheck(SkillId.search, supportingCritter, SkillCheckFlags.UnderDuress))
                            {
                                GameSystems.Anim.Interrupt(supportingCritter, AnimGoalPriority.AGP_3);
                                GameSystems.Anim.PushAnimate(supportingCritter, NormalAnimType.SkillSearch);
                                var supporterName = GameSystems.MapObject.GetDisplayName(supportingCritter);

                                bonlist.AddBonus(2, 21, 144, supporterName);
                            }
                        }
                    }
                }
            }

            return bonlist;
        }

        private bool CanUseSkill(GameObjectBody critter, SkillId skill)
        {
            if (!_skills.TryGetValue(skill, out var skillProps))
            {
                return false;
            }

            if (skillProps.Stat == Stat.intelligence)
            {
                if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_CannotUseIntSkill))
                {
                    return false;
                }
            }
            else if (skillProps.Stat == Stat.charisma)
            {
                if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_CannotUseChaSkill))
                {
                    return false;
                }
            }

            if (!skillProps.UntrainedUse && GetSkillRanks(critter, skill) <= 0)
            {
                return false;
            }

            return true;
        }

        // Try a DC10 skill check to support another critter while performing a skill check
        [TempleDllLocation(0x1007d720)]
        public bool TrySupportingSkillCheck(SkillId skill, GameObjectBody critter, SkillCheckFlags flag)
        {
            if (!CanUseSkill(critter, skill))
            {
                return false;
            }

            var skillBonus = critter.dispatch1ESkillLevel(skill, null, flag);

            var roll = Dice.D20.Roll();
            return roll + skillBonus >= 10;
        }

        [TempleDllLocation(0x1007d330)]
        public Stat GetDecidingStat(SkillId skill)
        {
            return _skills[skill].Stat;
        }

        public SkillCheckFlags GetSkillCheckFlagsForSchool(SchoolOfMagic schoolOfMagic)
        {
            switch (schoolOfMagic)
            {
                case SchoolOfMagic.None:
                    return SkillCheckFlags.SchoolNone;
                case SchoolOfMagic.Abjuration:
                    return SkillCheckFlags.SchoolAbjuration;
                case SchoolOfMagic.Conjuration:
                    return SkillCheckFlags.SchoolConjuration;
                case SchoolOfMagic.Divination:
                    return SkillCheckFlags.SchoolDivination;
                case SchoolOfMagic.Enchantment:
                    return SkillCheckFlags.SchoolEnchantment;
                case SchoolOfMagic.Evocation:
                    return SkillCheckFlags.SchoolEvocation;
                case SchoolOfMagic.Illusion:
                    return SkillCheckFlags.SchoolIllusion;
                case SchoolOfMagic.Necromancy:
                    return SkillCheckFlags.SchoolNecromancy;
                case SchoolOfMagic.Transmutation:
                    return SkillCheckFlags.SchoolTransmutation;
                default:
                    throw new ArgumentOutOfRangeException(nameof(schoolOfMagic), schoolOfMagic, null);
            }
        }
    }

    public static class CritterSkillExtensions
    {
        public static bool HasRanksIn(this GameObjectBody critter, SkillId skill)
        {
            return GameSystems.Skill.GetSkillRanks(critter, skill) > 0;
        }
    }

    [Flags]
    public enum SkillCheckFlags
    {
        /// <summary>
        /// Used in combat and other stressful situations (i.e. pickpocketing is under duress even out of combat).
        /// </summary>
        UnderDuress = 1,
        // Likely indicates checks against being tripped/bullrushed
        Unk2 = 2,
        SearchForSecretDoors = 0x4,
        SearchForTraps = 0x8,
        SchoolNone = 0x10,
        SchoolAbjuration = 0x20,
        SchoolConjuration = 0x40,
        SchoolDivination = 0x80,
        SchoolEnchantment = 0x100,
        SchoolEvocation = 0x200,
        SchoolIllusion = 0x400,
        SchoolNecromancy = 0x800,
        SchoolTransmutation = 0x1000,
        TakeTwenty = 0x2000
    }

}