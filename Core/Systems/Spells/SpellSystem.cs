using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.Spells
{
    public struct SpellMultiOption
    {
        public int value;
        public bool isProto;
    }

    public class SpellSystem : IGameSystem, IResetAwareSystem, ISaveGameAwareGameSystem
    {
        public const int MAX_SPELLS_KNOWN = 384;

        public const int SPELL_ENUM_MAX_VANILLA = 802;
        public const int SPELL_ENUM_MAX_EXPANDED = 3999;

        public const int NORMAL_SPELL_RANGE = 600; // vanilla normal spells are up to here
        public const int SPELL_LIKE_ABILITY_RANGE = 699; // Monster spell-like abilities are up to here

        public const int
            CLASS_SPELL_LIKE_ABILITY_START = 3000; // new in Temple+ - this is the range used for class spells

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly Dictionary<int, List<SpellMultiOption>> _multiOptions
            = new Dictionary<int, List<SpellMultiOption>>();

        [TempleDllLocation(0x10AAF218)]
        private Dictionary<int, ActiveSpell> _activeSpells = new Dictionary<int, ActiveSpell>();

        [TempleDllLocation(0x10BD0238)]
        private Dictionary<int, string> _spellsRadialMenuOptions;

        private struct ActiveSpell
        {
            public bool IsActive { get; set; }

            public SpellPacketBody Body { get; set; }
        }

        [TempleDllLocation(0x118CE064)]
        private int numDomainStrings = 24;

        [TempleDllLocation(0x118CE074)]
        private int schoolsOfMagicCount = 9;

        [TempleDllLocation(0x118CE06C)]
        private int subschoolsOfMagicCount = 13;

        [TempleDllLocation(0x118CE070)]
        private int descriptorsOfMagicCount = 20;

        [TempleDllLocation(0x118CE068)]
        private int spellEntryParserNumClasses = 9;

        [TempleDllLocation(0x102BFB08)]
        private int spellPrepareCount = -1;

        [TempleDllLocation(0x10AAF204)]
        private int spellIdSerial = 1;

        // supplemental info for the OnBeginRound invocation to identify whose round is beginning...
        private GameObjectBody mSpellBeginRoundObj;

        private readonly Dictionary<int, string> _skillUiMes;
        private readonly Dictionary<int, string> _spellDurationMes;
        private readonly Dictionary<int, string> _spellMes;
        private readonly Dictionary<int, string> _spellEnumMes;

        [TempleDllLocation(0x10aaf3c0)]
        private string[] _schoolsOfMagicNames;

        private string[] _subschoolsOfMagicNames;
        private string[] _descriptorsOfMagicNames;

        private SpellDescriptors _spells;

        [TempleDllLocation(0x1007b740)]
        public SpellSystem()
        {
            _spells = new SpellDescriptors();

            _skillUiMes = Tig.FS.ReadMesFile("mes/skill_ui.mes");
            _spellDurationMes = Tig.FS.ReadMesFile("mes/spell_duration.mes");
            _spellMes = Tig.FS.ReadMesFile("mes/spell.mes");
            _spellEnumMes = Tig.FS.ReadMesFile("rules/spell_enum.mes");

            _schoolsOfMagicNames = new string[schoolsOfMagicCount];
            for (int i = 0; i < _schoolsOfMagicNames.Length; i++)
            {
                _schoolsOfMagicNames[i] = _spellMes[15000 + i];
            }

            _subschoolsOfMagicNames = new string[subschoolsOfMagicCount];
            for (int i = 0; i < _subschoolsOfMagicNames.Length; i++)
            {
                _subschoolsOfMagicNames[i] = _spellMes[15100 + i];
            }

            _descriptorsOfMagicNames = new string[descriptorsOfMagicCount];
            for (int i = 0; i < _descriptorsOfMagicNames.Length; i++)
            {
                _descriptorsOfMagicNames[i] = _spellMes[15200 + i];
            }

            // TODO: TemplePlus extensions from LegacySpellSystem::Init

            _spellsRadialMenuOptions = Tig.FS.ReadMesFile("mes/spells_radial_menu_options.mes");
        }

        [TempleDllLocation(0x100791d0)]
        public void Dispose()
        {
            Reset();
        }

        [TempleDllLocation(0x100750f0)]
        public void Reset()
        {
            _activeSpells.Clear();
        }

        /// <summary>
        /// Used by prototypes mostly. Gets the spell enum by it's english unlocalized name.
        /// </summary>
        [TempleDllLocation(0x100779a0)]
        public bool GetSpellEnumByEnglishName(string name, out int id)
        {
            for (int i = 0; i < 802; i++)
            {
                if (_spellEnumMes.TryGetValue(5000 + i, out var line))
                {
                    if (line.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        id = i;
                        return true;
                    }
                }
            }

            id = 0;
            return false;
        }

        [TempleDllLocation(0x10077a00)]
        public bool GetSpellClassCode(string name, out int code)
        {
            for (int i = 7; i < 18; i++)
            {
                var line = _spellEnumMes[9993 + i];
                if (line.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    code = (i & 0x7F) | 0x80;
                    return true;
                }
            }

            for (int i = 0; i < 24; i++)
            {
                var line = _spellEnumMes[10500 + i];
                if (line.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    code = (i & 0x7F);
                    return true;
                }
            }

            code = 0;
            return false;
        }

        [TempleDllLocation(0x100764d0)]
        public bool IsSpellKnown(GameObjectBody obj, int spellId, int classCode = -1, int spellLevel = -1)
        {
            Trace.Assert(obj.IsCritter());

            var spellArray = obj.GetSpellArray(obj_f.critter_spells_known_idx);
            var numSpellsKnown = spellArray.Count;
            for (var i = 0; i < numSpellsKnown; i++)
            {
                var spellData = spellArray[i];
                if (spellData.spellEnum == spellId)
                {
                    if (classCode != -1 && spellData.classCode != classCode)
                    {
                        continue;
                    }

                    if (spellLevel != -1 && spellData.spellLevel != spellLevel)
                    {
                        continue;
                    }

                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x1007ad80)]
        public string GetSpellName(int spellId)
        {
            // TODO: TP's version is more elaborate
            return _spellMes[spellId];
        }

        [TempleDllLocation(0x10077940)]
        public string GetSpellEnumName(int spellEnum)
        {
            return _spellEnumMes[spellEnum];
        }

        [TempleDllLocation(0x10077970)]
        public string GetSpellHelpTopic(int spellId)
        {
            // TODO: TP's version is more elaborate
            return _spellEnumMes[20000 + spellId];
        }

        [TempleDllLocation(0x100756e0)]
        public bool TryGetActiveSpell(int activeSpellId, out SpellPacketBody spellPacketBody)
        {
            if (_activeSpells.TryGetValue(activeSpellId, out var spellPacket))
            {
                spellPacketBody = spellPacket.Body;
                return true;
            }

            spellPacketBody = null;
            return false;
        }

        public SpellPacketBody GetActiveSpell(int activeSpellId)
        {
            if (TryGetActiveSpell(activeSpellId, out var spellPacketBody))
            {
                return spellPacketBody;
            }

            throw new ArgumentOutOfRangeException("Unknown spell id: " + activeSpellId);
        }

        // TODO: Clean up arguments (use enums if possible)
        [TempleDllLocation(0x10079ee0)]
        public void SpellKnownAdd(GameObjectBody obj, int spellId, int classCode, int spellLevel, int spellStoreData,
            uint mmData)
        {
            obj_f spellListField;
            if (!obj.IsItem())
            {
                if (IsSpellKnown(obj, spellId, classCode, spellLevel))
                {
                    var spellName = GetSpellName(spellId);
                    Logger.Warn("Spell '{0}' (Level: {1}, Class: {2}) already exists for {3} on critter spell list",
                        spellName, spellLevel, classCode, obj);
                    return;
                }

                if (spellStoreData == 0)
                {
                    spellStoreData = (int) SpellStoreType.spellStoreKnown;
                }

                spellListField = obj_f.critter_spells_known_idx;
            }
            else
            {
                // Items have a limited range of spell flags / no metamagic
                spellStoreData = (int) SpellStoreType.spellStoreKnown;
                mmData = 0;

                spellListField = obj_f.item_spell_idx;
            }

            var spData = new SpellStoreData(spellId, spellLevel, classCode, mmData, spellStoreData);
            obj.AppendSpell(spellListField, spData);
        }

        [TempleDllLocation(0x10075bc0)]
        public void SpellKnownRemove(GameObjectBody critter, int spellEnum, int spellLevel, int spellClassCode)
        {
            var knownSpellsArray = critter.GetSpellArray(obj_f.critter_spells_known_idx);
            for (int i = 0; i < knownSpellsArray.Count; i++)
            {
                var knownSpell = knownSpellsArray[i];
                if (spellEnum == knownSpell.spellEnum
                    && spellLevel == knownSpell.spellLevel
                    && spellClassCode == knownSpell.classCode)
                {
                    critter.RemoveSpell(obj_f.critter_spells_known_idx, i);
                    return;
                }
            }
        }

        // TODO: Clean up arguments (use enums if possible)
        [TempleDllLocation(0x10075a10)]
        public void SpellMemorizedAdd(GameObjectBody obj, int spellId, int classCode, int spellLevel,
            SpellStoreState spellStoreData,
            MetaMagicData metaMagicData = default)
        {
            var spData = new SpellStoreData(spellId, spellLevel, classCode, metaMagicData, spellStoreData);
            // TODO *(int*)&spData.spellStoreState = spellStoreData;
            // TODO *(int*)&spData.metaMagicData = metaMagicData;

            // Ensure it is flagged as SpellStoreMemorized if not specified
            if (spData.spellStoreState.spellStoreType == SpellStoreType.spellStoreNone)
            {
                spData.spellStoreState.spellStoreType |= SpellStoreType.spellStoreMemorized;
            }

            obj.AppendSpell(obj_f.critter_spells_memorized_idx, spData);
        }

        [TempleDllLocation(0x100766e0)]
        public void ObjOnSpellBeginRound(GameObjectBody obj)
        {
            var spellIds = _activeSpells.Keys.ToImmutableSortedSet();

            foreach (var spellId in spellIds)
            {
                var spellPkt = _activeSpells[spellId];
                if (!spellPkt.IsActive)
                {
                    continue;
                }

                foreach (var spell in spellPkt.Body.Targets)
                {
                    if (spell.Object == obj)
                    {
                        mSpellBeginRoundObj = obj;
                        GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.BeginRound);
                    }
                }
            }
        }

        [TempleDllLocation(0x10079390)]
        public void PrepareSpellTransport()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100793f0)]
        public void RestoreSpellTransport()
        {
            Stub.TODO();
        }

        public bool IsArcaneSpellClass(int spellClassCode)
        {
            Stub.TODO();
            /*
             *
	// take care of domain spells first
	if (IsDomainSpell(spellClass)) {
		return false;
	}

	auto casterClass = GetCastingClass(spellClass);

	if (d20ClassSys.IsArcaneCastingClass(casterClass))
		return true;

	return false;
             */
            return false;
        }

        [TempleDllLocation(0x1007a440)]
        [TemplePlusLocation("spell.cpp:160")]
        public void IdentifySpellCast(int spellId)
        {
            if (!TryGetActiveSpell(spellId, out var pktNew))
            {
                return;
            }

            if (IsSpellLike(pktNew.spellEnum))
            {
                return; // Cannot ID spell-like abilities
            }

            if ((pktNew.animFlags & SpellAnimationFlag.SAF_ID_ATTEMPTED) != 0)
            {
                return; // Already attempted to ID this spell
            }

            pktNew.animFlags |= SpellAnimationFlag.SAF_ID_ATTEMPTED;
            UpdateSpellPacket(pktNew);

            var identifiedSuccess = false;

            var caster = pktNew.caster;
            if (!GameSystems.Party.IsPlayerControlled(pktNew.caster))
            {
                var spComponents = GetSpellComponentRegardMetamagic(pktNew.spellEnum, pktNew.metaMagicData);
                using var listResult = ObjList.ListVicinity(caster, ObjectListFilter.OLC_PC);

                foreach (var playerNear in listResult)
                {
                    var isVerbal = (spComponents & SpellComponent.Verbal) != 0;
                    var isSomatic = (spComponents & SpellComponent.Somatic) != 0;
                    if ((!isVerbal || GameSystems.AI.CannotHear(playerNear, caster, 1) == 0)
                        && (!isSomatic || GameSystems.AI.HasLineOfSight(playerNear, caster) == 0))
                    {
                        if (pktNew.IsCastFromItem)
                        {
                            GameSystems.D20.Combat.FloatCombatLine(caster, 188);
                            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(55, caster, null);
                            return;
                        }

                        var spellSchool = GetSpellSchoolEnum(pktNew.spellEnum);
                        var skillCheckFlags = GameSystems.Skill.GetSkillCheckFlagsForSchool(spellSchool);

                        var dc = 15 + pktNew.spellKnownSlotLevel;
                        if (GameSystems.Skill.SkillRoll(playerNear, SkillId.spellcraft, dc, out _, skillCheckFlags))
                        {
                            identifiedSuccess = true;
                            break;
                        }
                    }
                }

                if (identifiedSuccess)
                {
                    GameSystems.TextFloater.FloatLine(caster, TextFloaterCategory.Generic, TextFloaterColor.White,
                        _skillUiMes[1200]);
                    var spellName = GetSpellName(pktNew.spellEnum);
                    GameSystems.TextFloater.FloatLine(caster, TextFloaterCategory.Generic, TextFloaterColor.White,
                        spellName);
                    GameSystems.RollHistory.AddSpellCast(caster, pktNew.spellEnum);
                    return;
                }
                else
                {
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(50, caster, null);
                    return;
                }
            }

            if (pktNew.IsCastFromItem)
            {
                GameSystems.D20.Combat.FloatCombatLine(caster, 188);
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(55, caster, null);
            }
            else
            {
                GameSystems.RollHistory.AddSpellCast(caster, pktNew.spellEnum);
            }
        }


        public IReadOnlyList<SpellEntryLevelSpec> GetSpellListExtension(int spellEnum)
        {
            Stub.TODO();
            return ImmutableList<SpellEntryLevelSpec>.Empty;
        }

        [TempleDllLocation(0x100754b0, true)]
        public SpellEntry GetSpellEntry(int spellEnum)
        {
            if (TryGetSpellEntry(spellEnum, out var spellEntry))
            {
                return spellEntry;
            }

            throw new ArgumentException($"Trying to get invalid spell {spellEnum}");
        }

        [TempleDllLocation(0x100754b0)]
        public bool TryGetSpellEntry(int spellEnum, out SpellEntry spellEntry)
        {
            return _spells.TryGetEntry(spellEnum, out spellEntry);
        }

        private static readonly int[] SpontCastSpellsDruid =
        {
            -1, 476, 477, 478, 479, 480, 481, 482, 483, 484, 4000
        };

        private static readonly int[] SpontCastSpellsEvilCleric =
        {
            248, 247, 249, 250, 246, 61, 581, 582, 583, 583, 0
        };

        private static readonly int[] SpontCastSpellsGoodCleric =
        {
            91, 90, 92, 93, 89, 221, 577, 578, 579, 579, 0
        };

        private static readonly int[] SpontCastSpellsDruidSummons =
        {
            -1, 2000, 2100, 2200, 2300, 2400, 2500, 2600, 2700, 2800, 0
        };

        [TempleDllLocation(0x100777d0)]
        [TempleDllLocation(0x100777da)]
        [TempleDllLocation(0x10077964)]
        public int GetSpontaneousCastSpell(SpontCastType spontCastType, in int spellSlotLevel)
        {
            switch (spontCastType)
            {
                case SpontCastType.GoodCleric:
                    return SpontCastSpellsGoodCleric[spellSlotLevel];
                case SpontCastType.EvilCleric:
                    return SpontCastSpellsEvilCleric[spellSlotLevel];
                case SpontCastType.Druid:
                    return SpontCastSpellsDruid[spellSlotLevel];
                default:
                    throw new ArgumentOutOfRangeException(nameof(spontCastType));
            }
        }

        public static int GetSpellClass(Stat classEnum)
        {
            return 0x80 | (int) classEnum;
        }

        public static int GetSpellClass(DomainId domain)
        {
            return (int) domain;
        }

        [TempleDllLocation(0x10076550)]
        public int GetSpellLevelBySpellClass(int spellEnum, int spellClass)
        {
            if (IsLabel(spellEnum))
                return spellEnum - SPELL_ENUM_LABEL_START;
            if (IsNewSlotDesignator(spellEnum))
                return spellEnum - SPELL_ENUM_NEW_SLOT_START;
            if (spellEnum == SPELL_ENUM_VACANT)
                return -1;

            var spEntry = GetSpellEntry(spellEnum);
            return spEntry.SpellLevelForSpellClass(spellClass);
        }

        public bool IsNewSlotDesignator(int spellEnum)
        {
            if (spellEnum >= SPELL_ENUM_NEW_SLOT_START
                && spellEnum < SPELL_ENUM_NEW_SLOT_START + NUM_SPELL_LEVELS)
                return true;
            return false;
        }

        public bool IsLabel(int spellEnum)
        {
            if (spellEnum >= SPELL_ENUM_LABEL_START
                && spellEnum < SPELL_ENUM_LABEL_START + NUM_SPELL_LEVELS)
                return true;
            return false;
        }

        public bool GetSchoolSpecialization(GameObjectBody critter, out SchoolOfMagic specializedSchool,
            out SchoolOfMagic forbiddenSchool1, out SchoolOfMagic forbiddenSchool2)
        {
            var packedValue = critter.GetInt32(obj_f.critter_school_specialization);
            specializedSchool = (SchoolOfMagic) (packedValue & 0xFF);
            forbiddenSchool1 = (SchoolOfMagic) ((packedValue >> 8) & 0xFF);
            forbiddenSchool2 = (SchoolOfMagic) ((packedValue >> 16) & 0xFF);
            return specializedSchool != SchoolOfMagic.None;
        }

        [TempleDllLocation(0x100fdf00)]
        public bool IsForbiddenSchool(GameObjectBody critter, SchoolOfMagic spSchool)
        {
            GetSchoolSpecialization(critter, out _, out var forbSch1, out var forbSch2);
            if (forbSch1 == spSchool || forbSch2 == spSchool)
                return true;
            return false;
        }

        public const int NUM_SPELL_LEVELS = 10; // spells are levels 0-9
        private const int NUM_SPELL_LEVELS_VANILLA = 6; // 0-5

        private const int MAX_SPELL_TARGETS = 32;

        // indicates that a spell is not an item spell
        private const int INV_IDX_INVALID = 255;

        // the range of 803-812 is reserved for "spell labels" in the chargen / levelup spell UI
        private const int SPELL_ENUM_LABEL_START = 803;

        // used for vacant spellbook slots
        private const int SPELL_ENUM_VACANT = 802;

        // for bard/sorc new spells; range is in 1605-1614
        private const int SPELL_ENUM_NEW_SLOT_START = 1605;

        public bool IsDomainSpell(int spellClassCode)
        {
            return (spellClassCode & 0x80) == 0;
        }

        public Stat GetCastingClass(int spellClassCode)
        {
            if (IsDomainSpell(spellClassCode))
            {
                throw new ArgumentException(
                    "GetCastingClass called with domain spell class code: " + spellClassCode
                );
            }

            return (Stat) (spellClassCode & 0x7F);
        }

        [TempleDllLocation(0x10075730)]
        public void UpdateSpellPacket(SpellPacketBody pkt)
        {
            // Probably a no-op now that pkt is a class
        }

        // TODO: Fix the name
        [TempleDllLocation(0x10075da0)]
        public bool HashMatchingClassForSpell(GameObjectBody caster, int spellEnum)
        {
            var spEntry = GetSpellEntry(spellEnum);
            foreach (var lvlSpec in spEntry.spellLvls)
            {
                // domain spell
                if (IsDomainSpell(lvlSpec.spellClass))
                {
                    // if is Cleric or NPC and the spell spec is Domain Special
                    if (caster.GetStat(Stat.level_cleric) > 0
                        || caster.IsNPC() && lvlSpec.spellClass == (int) DomainId.Special)
                    {
                        if (caster.GetStat(Stat.domain_1) == lvlSpec.spellClass
                            || caster.GetStat(Stat.domain_2) == lvlSpec.spellClass)
                            return true;
                    }
                }
                // normal spell
                else
                {
                    if (caster.GetStat(GetCastingClass(lvlSpec.spellClass)) > 0)
                        return true;
                }
            }

            var spellListExtension = GetSpellListExtension(spellEnum);
            foreach (var lvlSpec in spellListExtension)
            {
                // TODO: domain extension for PrCs (Domain Wizard?)
                {
                    if (caster.GetStat(GetCastingClass(lvlSpec.spellClass)) > 0)
                        return true;
                }
            }

            // Check for an Advanced Learning Class (character will know the spell but it will not be on their list standard)
            bool bHasAdvancedLearning = false;
            var advancedLearningClasses = GetClassesWithAdvancedLearning();
            foreach (var classEnum in advancedLearningClasses)
            {
                if (caster.GetStat(classEnum) > 0)
                {
                    bHasAdvancedLearning = true;
                }
            }

            if (bHasAdvancedLearning)
            {
                if (IsSpellKnown(caster, spellEnum))
                {
                    return true;
                }
            }

            return false;
        }

        public void RegisterAdvancedLearningClass(Stat classEnum)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Stat> GetClassesWithAdvancedLearning()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10075c60)]
        public bool CheckAbilityScoreReqForSpell(GameObjectBody handle, int spellEnum,
            Stat statBeingRaised = (Stat) (-1))
        {
            var spEntry = GetSpellEntry(spellEnum);

            int statLvl;
            Stat spellStat;
            foreach (var lvlSpec in spEntry.spellLvls)
            {
                // normal spell
                if (!IsDomainSpell(lvlSpec.spellClass))
                {
                    var classEnum = GetCastingClass(lvlSpec.spellClass);
                    spellStat = D20ClassSystem.GetSpellStat(classEnum);
                }
                else // domain spell
                {
                    var obj = handle;

                    // if is Cleric or NPC and the spell spec is Domain Special
                    if (handle.GetStat(Stat.level_cleric) <= 0
                        && (obj.IsNPC() && lvlSpec.spellClass != (int) DomainId.Special)
                    )
                    {
                        continue;
                    }

                    spellStat = Stat.wisdom;
                }

                statLvl = handle.GetStat(spellStat);
                if (statBeingRaised == spellStat)
                    ++statLvl;

                if (statLvl >= lvlSpec.slotLevel + 10)
                    return true;
            }

            foreach (var lvlSpec in GetSpellListExtension(spellEnum))
            {
                // TODO: domain extension for PrCs (Domain Wizard?)
                var classEnum = GetCastingClass(lvlSpec.spellClass);
                spellStat = D20ClassSystem.GetSpellStat(classEnum);
                statLvl = handle.GetStat(spellStat);
                if (statBeingRaised == spellStat)
                    ++statLvl;

                if (statLvl >= lvlSpec.slotLevel + 10)
                    return true;
            }

            return false;
        }

        private static readonly EncodedAnimId[] WandAnimIds =
        {
            new EncodedAnimId(NormalAnimType.WandAbjurationConjuring),
            new EncodedAnimId(NormalAnimType.WandAbjurationConjuring),
            new EncodedAnimId(NormalAnimType.WandConjurationConjuring),
            new EncodedAnimId(NormalAnimType.WandDivinationConjuring),
            new EncodedAnimId(NormalAnimType.WandEnchantmentConjuring),
            new EncodedAnimId(NormalAnimType.WandEvocationConjuring),
            new EncodedAnimId(NormalAnimType.WandIllusionConjuring),
            new EncodedAnimId(NormalAnimType.WandNecromancyConjuring),
            new EncodedAnimId(NormalAnimType.WandTransmutationConjuring)
        };

        private static readonly EncodedAnimId[] SpellSchoolAnimIds =
        {
            new EncodedAnimId(NormalAnimType.AbjurationConjuring),
            new EncodedAnimId(NormalAnimType.AbjurationConjuring),
            new EncodedAnimId(NormalAnimType.ConjurationConjuring),
            new EncodedAnimId(NormalAnimType.DivinationConjuring),
            new EncodedAnimId(NormalAnimType.EnchantmentConjuring),
            new EncodedAnimId(NormalAnimType.EvocationConjuring),
            new EncodedAnimId(NormalAnimType.IllusionConjuring),
            new EncodedAnimId(NormalAnimType.NecromancyConjuring),
            new EncodedAnimId(NormalAnimType.TransmutationConjuring)
        };

        [TempleDllLocation(0x100757c0)]
        public EncodedAnimId GetAnimIdWand(SchoolOfMagic spellSchool)
        {
            return WandAnimIds[(int) spellSchool];
        }

        [TempleDllLocation(0x100757B0)]
        public EncodedAnimId GetSpellSchoolAnimId(SchoolOfMagic spellSchool)
        {
            return SpellSchoolAnimIds[(int) spellSchool];
        }

        [TempleDllLocation(0x10076820)]
        public void FloatSpellLine(GameObjectBody obj, int lineId, TextFloaterColor color,
            string prefix = null, string suffix = null)
        {
            var line = _spellMes[lineId];
            if (prefix != null)
            {
                if (suffix != null)
                {
                    line = prefix + line + suffix;
                }
                else
                {
                    line = prefix + line;
                }
            }
            else if (suffix != null)
            {
                line += suffix;
            }

            GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, color, line);
        }

        [TempleDllLocation(0x100b51e0)]
        public int DispelRoll(GameObjectBody caster, BonusList bonusList, int rollModifier, int dc, string historyText)
        {
            var result = DispelRoll(caster, bonusList, rollModifier, dc, historyText, out var historyId);
            GameSystems.RollHistory.CreateRollHistoryString(historyId);
            return result;
        }

        [TempleDllLocation(0x100b51e0)]
        public int DispelRoll(GameObjectBody caster, BonusList bonusList, int rollModifier, int dc, string historyText,
            out int rollHistId)
        {
            var dice = new Dice(1, 20, rollModifier);
            var rollResult = dice.Roll();

            rollHistId =
                GameSystems.RollHistory.AddMiscCheck(caster, dc, historyText, dice, rollResult, bonusList);
            return rollResult + bonusList.OverallBonus - dc;
        }

        // This was previously part of RegisterSpell
        [TempleDllLocation(0x10077670, secondary: true)]
        public int GetNewSpellId()
        {
            if (spellIdSerial >= 0x7fffFFFF)
            {
                Logger.Warn("WARNING! wow, we have reached the maxium amount of spell ids. Resetting to zero!");
                spellIdSerial = 1;
                return spellIdSerial++;
            }

            Logger.Info("New spellid assigned: {0}", spellIdSerial);
            return spellIdSerial++;
        }

        [TempleDllLocation(0x10077670)]
        public bool RegisterSpell(SpellPacketBody spellPkt, in int spellId)
        {
            if (spellId == 0)
            {
                throw new ArgumentException("RegisterSpell: null spellId!");
            }

            var newPkt = new ActiveSpell();
            spellPkt.spellId = spellId;
            var spEnum = spellPkt.spellEnum;
            newPkt.Body = spellPkt;
            newPkt.IsActive = true;

            var spEntry = GetSpellEntry(spEnum);
            var spLvl = spEntry.SpellLevelForSpellClass(spellPkt.spellClass);
            if (spLvl < 0) // if none found
                spLvl = 0;

            MetaMagicData mmData = spellPkt.metaMagicData;

            var heightenCount = mmData.metaMagicHeightenSpellCount;
            spLvl += heightenCount;

            // Get Spell DC Base
            var spellClass = spellPkt.spellClass;
            var dc = 10 + spellPkt.caster.DispatchSpellDcBase(spEntry); // as far as I know this is always 0

            var spellStat = Stat.wisdom;
            if (!IsDomainSpell(spellPkt.spellClass))
            {
                spellStat = D20ClassSystem.GetSpellDcStat(GetCastingClass(spellClass));
            }

            DispIoBonusList evtObjAbScore = DispIoBonusList.Default;
            evtObjAbScore.flags |= 1; // effect unknown??
            dc += spLvl + D20StatSystem.GetModifierForAbilityScore(
                      spellPkt.caster.Dispatch10AbilityScoreLevelGet(spellStat, evtObjAbScore)
                  );

            // Spell DC Mod
            dc += spellPkt.caster.DispatchSpellDcMod(spEntry);

            if (dc < 1)
                dc = 1;

            newPkt.Body.spellRange = spellPkt.spellRange;
            newPkt.Body.casterLevel = spellPkt.casterLevel;
            newPkt.Body.dc = dc;
            newPkt.Body.animFlags |= SpellAnimationFlag.SAF_UNK8;

            _activeSpells[spellId] = newPkt;
            Logger.Info("New spell registered: id {0}, spell enum {1}", spellId, spEnum);
            return true;
        }

        [TempleDllLocation(0x100769f0)]
        public bool IsSpellHarmful(int spellEnum, GameObjectBody caster, GameObjectBody target)
        {
            // Vanilla used a hardcoded table.
            if (spellEnum > SPELL_ENUM_MAX_VANILLA)
            {
                SpellEntry spEntry = GetSpellEntry(spellEnum);
                if (spEntry.spellEnum == 0)
                {
                    Logger.Warn("Invalid spellEnum in IsSpellHarmfull");
                    return false;
                }

                return spEntry.HasAiType(AiSpellType.ai_action_offensive);
            }


            if (caster != null)
            {
                if (target != null)
                {
                    switch (spellEnum)
                    {
                        case WellKnownSpells.Aid:
                        case WellKnownSpells.AnimalFriendship:
                        case WellKnownSpells.AnimalGrowth:
                        case WellKnownSpells.ArcaneLock:
                        case WellKnownSpells.Barkskin:
                        case WellKnownSpells.Bless:
                        case WellKnownSpells.BlessWeapon:
                        case WellKnownSpells.Blink:
                        case WellKnownSpells.Blur:
                        case WellKnownSpells.BullsStrength:
                        case WellKnownSpells.CalmAnimals:
                        case WellKnownSpells.CalmEmotions:
                        case WellKnownSpells.CatsGrace:
                        case WellKnownSpells.ClairaudienceClairvoyance:
                        case WellKnownSpells.Consecrate:
                        case WellKnownSpells.CurseWater:
                        case WellKnownSpells.Darkvision:
                        case WellKnownSpells.Daylight:
                        case WellKnownSpells.DeathWard:
                        case WellKnownSpells.DelayPoison:
                        case WellKnownSpells.DetectChaos:
                        case WellKnownSpells.DetectEvil:
                        case WellKnownSpells.DetectLaw:
                        case WellKnownSpells.DetectMagic:
                        case WellKnownSpells.DetectSecretDoors:
                        case WellKnownSpells.DetectUndead:
                        case WellKnownSpells.DimensionDoor:
                        case WellKnownSpells.DiscernLies:
                        case WellKnownSpells.DispelChaos:
                        case WellKnownSpells.DispelEvil:
                        case WellKnownSpells.DispelLaw:
                        case WellKnownSpells.DispelMagic:
                        case WellKnownSpells.Displacement:
                        case WellKnownSpells.DivineFavor:
                        case WellKnownSpells.DivinePower:
                        case WellKnownSpells.Emotion:
                        case WellKnownSpells.Endurance:
                        case WellKnownSpells.EndureElements:
                        case WellKnownSpells.Enlarge:
                        case WellKnownSpells.EntropicShield:
                        case WellKnownSpells.ExpeditiousRetreat:
                        case WellKnownSpells.FindTraps:
                        case WellKnownSpells.FireShield:
                        case WellKnownSpells.FreedomOfMovement:
                        case WellKnownSpells.GaseousForm:
                        case WellKnownSpells.GiantVermin:
                        case WellKnownSpells.Goodberry:
                        case WellKnownSpells.GreaterMagicFang:
                        case WellKnownSpells.GreaterMagicWeapon:
                        case WellKnownSpells.Guidance:
                        case WellKnownSpells.Haste:
                        case WellKnownSpells.HoldPortal:
                        case WellKnownSpells.Identify:
                        case WellKnownSpells.ImprovedInvisibility:
                        case WellKnownSpells.Invisibility:
                        case WellKnownSpells.InvisibilitySphere:
                        case WellKnownSpells.InvisibilityToAnimals:
                        case WellKnownSpells.InvisibilityToUndead:
                        case WellKnownSpells.KeenEdge:
                        case WellKnownSpells.LesserRestoration:
                        case WellKnownSpells.MageArmor:
                        case WellKnownSpells.MagicCircleAgainstChaos:
                        case WellKnownSpells.MagicCircleAgainstEvil:
                        case WellKnownSpells.MagicCircleAgainstGood:
                        case WellKnownSpells.MagicCircleAgainstLaw:
                        case WellKnownSpells.MagicFang:
                        case WellKnownSpells.MagicStone:
                        case WellKnownSpells.MagicVestment:
                        case WellKnownSpells.MagicWeapon:
                        case WellKnownSpells.MeldIntoStone:
                        case WellKnownSpells.LesserGlobeOfInvulnerability:
                        case WellKnownSpells.MirrorImage:
                        case WellKnownSpells.MordenkainensFaithfulHound:
                        case WellKnownSpells.NegativeEnergyProtection:
                        case WellKnownSpells.NeutralizePoison:
                        case WellKnownSpells.OpenClose:
                        case WellKnownSpells.OtilukesResilientSphere:
                        case WellKnownSpells.Prayer:
                        case WellKnownSpells.ProtectionFromArrows:
                        case WellKnownSpells.ProtectionFromChaos:
                        case WellKnownSpells.ProtectionFromElements:
                        case WellKnownSpells.ProtectionFromEvil:
                        case WellKnownSpells.ProtectionFromGood:
                        case WellKnownSpells.ProtectionFromLaw:
                        case WellKnownSpells.PryingEyes:
                        case WellKnownSpells.RaiseDead:
                        case WellKnownSpells.ReadMagic:
                        case WellKnownSpells.RemoveBlindnessDeafness:
                        case WellKnownSpells.RemoveCurse:
                        case WellKnownSpells.RemoveDisease:
                        case WellKnownSpells.RemoveFear:
                        case WellKnownSpells.RemoveParalysis:
                        case WellKnownSpells.Resistance:
                        case WellKnownSpells.ResistElements:
                        case WellKnownSpells.Restoration:
                        case WellKnownSpells.Resurrection:
                        case WellKnownSpells.RighteousMight:
                        case WellKnownSpells.Sanctuary:
                        case WellKnownSpells.SeeInvisibility:
                        case WellKnownSpells.Shield:
                        case WellKnownSpells.ShieldOfFaith:
                        case WellKnownSpells.Shillelagh:
                        case WellKnownSpells.SpellResistance:
                        case WellKnownSpells.SpiritualWeapon:
                        case WellKnownSpells.Stoneskin:
                        case WellKnownSpells.SummonMonsterI:
                        case WellKnownSpells.SummonMonsterIi:
                        case WellKnownSpells.SummonMonsterIii:
                        case WellKnownSpells.SummonMonsterIv:
                        case WellKnownSpells.SummonMonsterV:
                        case WellKnownSpells.SummonMonsterVi:
                        case WellKnownSpells.SummonMonsterVii:
                        case WellKnownSpells.SummonMonsterViii:
                        case WellKnownSpells.SummonMonsterIx:
                        case WellKnownSpells.SummonNaturesAllyI:
                        case WellKnownSpells.SummonNaturesAllyIi:
                        case WellKnownSpells.SummonNaturesAllyIii:
                        case WellKnownSpells.SummonNaturesAllyIv:
                        case WellKnownSpells.SummonNaturesAllyV:
                        case WellKnownSpells.SummonNaturesAllyVi:
                        case WellKnownSpells.SummonNaturesAllyVii:
                        case WellKnownSpells.SummonNaturesAllyViii:
                        case WellKnownSpells.SummonNaturesAllyIx:
                        case WellKnownSpells.SummonSwarm:
                        case WellKnownSpells.Teleport:
                        case WellKnownSpells.TreeShape:
                        case WellKnownSpells.TrueSeeing:
                        case WellKnownSpells.TrueStrike:
                        case WellKnownSpells.VampiricTouch:
                        case WellKnownSpells.Virtue:
                        case WellKnownSpells.WindWall:
                        case WellKnownSpells.DispelAir:
                        case WellKnownSpells.DispelEarth:
                        case WellKnownSpells.DispelFire:
                        case WellKnownSpells.DispelWater:
                        case WellKnownSpells.Rage:
                        case WellKnownSpells.EaglesSplendor:
                        case WellKnownSpells.FoxsCunning:
                        case WellKnownSpells.OwlsWisdom:
                        case WellKnownSpells.Glibness:
                        case WellKnownSpells.FalseLife:
                        case WellKnownSpells.Longstrider:
                        case WellKnownSpells.Heroism:
                        case WellKnownSpells.GreaterHeroism:
                        case WellKnownSpells.GoodHope:
                        case WellKnownSpells.Heal:
                        case WellKnownSpells.Reincarnation:
                        case WellKnownSpells.RingOfFreedomOfMovement:
                        case WellKnownSpells.PotionOfEnlarge:
                        case WellKnownSpells.PotionOfHaste:
                        case WellKnownSpells.BootsOfSpeed:
                        case WellKnownSpells.DustOfDisappearance:
                        case WellKnownSpells.PotionOfCharisma:
                        case WellKnownSpells.PotionOfGlibness:
                        case WellKnownSpells.PotionOfHiding:
                        case WellKnownSpells.PotionOfSneaking:
                        case WellKnownSpells.PotionOfHeroism:
                        case WellKnownSpells.PotionOfSuperHeroism:
                        case WellKnownSpells.PotionOfProtectionFromFire:
                        case WellKnownSpells.PotionOfProtectionFromOutsiders:
                        case WellKnownSpells.PotionOfProtectionFromElementals:
                        case WellKnownSpells.PotionOfProtectionFromEarth:
                        case WellKnownSpells.PotionOfProtectionFromMagic:
                        case WellKnownSpells.PotionOfProtectionFromUndead:
                        case WellKnownSpells.RingOfAnimalSummoningDog:
                        case WellKnownSpells.PotionOfProtectionFromAcid:
                        case WellKnownSpells.PotionOfProtectionFromElectricity:
                        case WellKnownSpells.SummonAirElemental:
                        case WellKnownSpells.SummonEarthElemental:
                        case WellKnownSpells.SummonFireElemental:
                        case WellKnownSpells.SummonWaterElemental:
                        case WellKnownSpells.SummonBalor:
                        case WellKnownSpells.SummonGlabrezu:
                        case WellKnownSpells.SummonHezrou:
                        case WellKnownSpells.SummonVrock:
                            return false;
                        case WellKnownSpells.CureCriticalWounds:
                        case WellKnownSpells.CureLightWounds:
                        case WellKnownSpells.CureMinorWounds:
                        case WellKnownSpells.CureModerateWounds:
                        case WellKnownSpells.CureSeriousWounds:
                            return GameSystems.Critter.IsUndead(target);
                        case WellKnownSpells.InflictCriticalWounds:
                        case WellKnownSpells.InflictLightWounds:
                        case WellKnownSpells.InflictMinorWounds:
                        case WellKnownSpells.InflictModerateWounds:
                        case WellKnownSpells.InflictSeriousWounds:
                            return !GameSystems.Critter.IsUndead(target);
                        default:
                            return !GameSystems.Critter.IsFriendly(caster, target);
                        case WellKnownSpells.AnimalTrance:
                        case WellKnownSpells.AnimateDead:
                        case WellKnownSpells.Bane:
                        case WellKnownSpells.BestowCurse:
                        case WellKnownSpells.BlindnessDeafness:
                        case WellKnownSpells.BreakEnchantment:
                        case WellKnownSpells.BurningHands:
                        case WellKnownSpells.CallLightning:
                        case WellKnownSpells.CauseFear:
                        case WellKnownSpells.ChainLightning:
                        case WellKnownSpells.ChaosHammer:
                        case WellKnownSpells.CharmMonster:
                        case WellKnownSpells.CharmPerson:
                        case WellKnownSpells.CharmPersonOrAnimal:
                        case WellKnownSpells.ChillMetal:
                        case WellKnownSpells.ChillTouch:
                        case WellKnownSpells.Cloudkill:
                        case WellKnownSpells.ColorSpray:
                        case WellKnownSpells.Command:
                        case WellKnownSpells.ConeOfCold:
                        case WellKnownSpells.Confusion:
                        case WellKnownSpells.Contagion:
                        case WellKnownSpells.ControlPlants:
                        case WellKnownSpells.Darkness:
                        case WellKnownSpells.Daze:
                        case WellKnownSpells.DeathKnell:
                        case WellKnownSpells.DeeperDarkness:
                        case WellKnownSpells.Desecrate:
                        case WellKnownSpells.DimensionalAnchor:
                        case WellKnownSpells.DominateAnimal:
                        case WellKnownSpells.DominateMonster:
                        case WellKnownSpells.DominatePerson:
                        case WellKnownSpells.Doom:
                        case WellKnownSpells.Enervation:
                        case WellKnownSpells.Entangle:
                        case WellKnownSpells.FaerieFire:
                        case WellKnownSpells.Fear:
                        case WellKnownSpells.Feeblemind:
                        case WellKnownSpells.Fireball:
                        case WellKnownSpells.FlameArrow:
                        case WellKnownSpells.FlameStrike:
                        case WellKnownSpells.FlamingSphere:
                        case WellKnownSpells.Flare:
                        case WellKnownSpells.FogCloud:
                        case WellKnownSpells.GhoulTouch:
                        case WellKnownSpells.Glitterdust:
                        case WellKnownSpells.Grease:
                        case WellKnownSpells.GreaterCommand:
                        case WellKnownSpells.GustOfWind:
                        case WellKnownSpells.HaltUndead:
                        case WellKnownSpells.HeatMetal:
                        case WellKnownSpells.HoldAnimal:
                        case WellKnownSpells.HoldMonster:
                        case WellKnownSpells.HoldPerson:
                        case WellKnownSpells.HolySmite:
                        case WellKnownSpells.HypnoticPattern:
                        case WellKnownSpells.Hypnotism:
                        case WellKnownSpells.IceStorm:
                        case WellKnownSpells.MagicMissile:
                        case WellKnownSpells.MelfsAcidArrow:
                        case WellKnownSpells.ObscuringMist:
                        case WellKnownSpells.OrdersWrath:
                        case WellKnownSpells.PhantasmalKiller:
                        case WellKnownSpells.ProduceFlame:
                        case WellKnownSpells.RayOfEnfeeblement:
                        case WellKnownSpells.RayOfFrost:
                        case WellKnownSpells.Reduce:
                        case WellKnownSpells.RepelVermin:
                        case WellKnownSpells.Scare:
                        case WellKnownSpells.SearingLight:
                        case WellKnownSpells.Shatter:
                        case WellKnownSpells.ShockingGrasp:
                        case WellKnownSpells.Shout:
                        case WellKnownSpells.Silence:
                        case WellKnownSpells.SlayLiving:
                        case WellKnownSpells.Sleep:
                        case WellKnownSpells.SleetStorm:
                        case WellKnownSpells.Slow:
                        case WellKnownSpells.SoftenEarthAndStone:
                        case WellKnownSpells.SolidFog:
                        case WellKnownSpells.SoundBurst:
                        case WellKnownSpells.SpikeGrowth:
                        case WellKnownSpells.SpikeStones:
                        case WellKnownSpells.StinkingCloud:
                        case WellKnownSpells.Suggestion:
                        case WellKnownSpells.TashasHideousLaughter:
                        case WellKnownSpells.UnholyBlight:
                        case WellKnownSpells.Web:
                        case WellKnownSpells.Blight:
                        case WellKnownSpells.ReduceAnimal:
                        case WellKnownSpells.AcidSplash:
                        case WellKnownSpells.DazeMonster:
                        case 559:
                        case WellKnownSpells.CallLightningStorm:
                        case WellKnownSpells.LesserConfusion:
                        case WellKnownSpells.DeepSlumber:
                        case WellKnownSpells.CrushingDespair:
                        case WellKnownSpells.Harm2:
                        case WellKnownSpells.SpellMonsterFrogTongue:
                        case WellKnownSpells.SpellMonsterVrockScreech:
                        case WellKnownSpells.SpellMonsterVrockSpores:
                        case WellKnownSpells.JavelinOfLightning:
                        case WellKnownSpells.FlameTongue:
                            return true;
                    }
                }

                return true;
            }

            if (spellEnum > WellKnownSpells.KeenEdge)
            {
                if (spellEnum > WellKnownSpells.SeeInvisibility)
                {
                    if (spellEnum > WellKnownSpells.VampiricTouch)
                    {
                        switch (spellEnum)
                        {
                            case WellKnownSpells.Virtue:
                            case WellKnownSpells.WindWall:
                            case WellKnownSpells.DispelAir:
                            case WellKnownSpells.DispelEarth:
                            case WellKnownSpells.DispelFire:
                            case WellKnownSpells.DispelWater:
                            case WellKnownSpells.Rage:
                            case WellKnownSpells.EaglesSplendor:
                            case WellKnownSpells.FoxsCunning:
                            case WellKnownSpells.OwlsWisdom:
                            case WellKnownSpells.Glibness:
                            case WellKnownSpells.FalseLife:
                            case WellKnownSpells.Longstrider:
                            case WellKnownSpells.Heroism:
                            case WellKnownSpells.GreaterHeroism:
                            case WellKnownSpells.GoodHope:
                            case WellKnownSpells.Heal:
                            case WellKnownSpells.Reincarnation:
                            case WellKnownSpells.RingOfFreedomOfMovement:
                            case WellKnownSpells.PotionOfEnlarge:
                            case WellKnownSpells.PotionOfHaste:
                            case WellKnownSpells.BootsOfSpeed:
                            case WellKnownSpells.DustOfDisappearance:
                            case WellKnownSpells.PotionOfCharisma:
                            case WellKnownSpells.PotionOfGlibness:
                            case WellKnownSpells.PotionOfHiding:
                            case WellKnownSpells.PotionOfSneaking:
                            case WellKnownSpells.PotionOfHeroism:
                            case WellKnownSpells.PotionOfSuperHeroism:
                            case WellKnownSpells.PotionOfProtectionFromFire:
                            case WellKnownSpells.PotionOfProtectionFromOutsiders:
                            case WellKnownSpells.PotionOfProtectionFromElementals:
                            case WellKnownSpells.PotionOfProtectionFromEarth:
                            case WellKnownSpells.PotionOfProtectionFromMagic:
                            case WellKnownSpells.PotionOfProtectionFromUndead:
                            case WellKnownSpells.RingOfAnimalSummoningDog:
                            case WellKnownSpells.PotionOfProtectionFromAcid:
                            case WellKnownSpells.PotionOfProtectionFromElectricity:
                            case WellKnownSpells.SummonAirElemental:
                            case WellKnownSpells.SummonEarthElemental:
                            case WellKnownSpells.SummonFireElemental:
                            case WellKnownSpells.SummonWaterElemental:
                            case WellKnownSpells.SummonBalor:
                            case WellKnownSpells.SummonGlabrezu:
                            case WellKnownSpells.SummonHezrou:
                            case WellKnownSpells.SummonVrock:
                                return false;
                            default:
                                return true;
                        }

                        return true;
                    }

                    if (spellEnum != WellKnownSpells.VampiricTouch)
                    {
                        switch (spellEnum)
                        {
                            case WellKnownSpells.Shield:
                            case WellKnownSpells.ShieldOfFaith:
                            case WellKnownSpells.Shillelagh:
                            case WellKnownSpells.SpellResistance:
                            case WellKnownSpells.SpiritualWeapon:
                            case WellKnownSpells.Stoneskin:
                            case WellKnownSpells.SummonMonsterI:
                            case WellKnownSpells.SummonMonsterIi:
                            case WellKnownSpells.SummonMonsterIii:
                            case WellKnownSpells.SummonMonsterIv:
                            case WellKnownSpells.SummonMonsterV:
                            case WellKnownSpells.SummonMonsterVi:
                            case WellKnownSpells.SummonMonsterVii:
                            case WellKnownSpells.SummonMonsterViii:
                            case WellKnownSpells.SummonMonsterIx:
                            case WellKnownSpells.SummonNaturesAllyI:
                            case WellKnownSpells.SummonNaturesAllyIi:
                            case WellKnownSpells.SummonNaturesAllyIii:
                            case WellKnownSpells.SummonNaturesAllyIv:
                            case WellKnownSpells.SummonNaturesAllyV:
                            case WellKnownSpells.SummonNaturesAllyVi:
                            case WellKnownSpells.SummonNaturesAllyVii:
                            case WellKnownSpells.SummonNaturesAllyViii:
                            case WellKnownSpells.SummonNaturesAllyIx:
                            case WellKnownSpells.SummonSwarm:
                            case WellKnownSpells.Teleport:
                            case WellKnownSpells.TreeShape:
                            case WellKnownSpells.TrueSeeing:
                            case WellKnownSpells.TrueStrike:
                                return false;
                            default:
                                return true;
                        }

                        return true;
                    }
                }
                else if (spellEnum != WellKnownSpells.SeeInvisibility)
                {
                    switch (spellEnum)
                    {
                        case WellKnownSpells.LesserRestoration:
                        case WellKnownSpells.MageArmor:
                        case WellKnownSpells.MagicCircleAgainstChaos:
                        case WellKnownSpells.MagicCircleAgainstEvil:
                        case WellKnownSpells.MagicCircleAgainstGood:
                        case WellKnownSpells.MagicCircleAgainstLaw:
                        case WellKnownSpells.MagicFang:
                        case WellKnownSpells.MagicStone:
                        case WellKnownSpells.MagicVestment:
                        case WellKnownSpells.MagicWeapon:
                        case WellKnownSpells.MeldIntoStone:
                        case WellKnownSpells.LesserGlobeOfInvulnerability:
                        case WellKnownSpells.MirrorImage:
                        case WellKnownSpells.MordenkainensFaithfulHound:
                        case WellKnownSpells.NegativeEnergyProtection:
                        case WellKnownSpells.NeutralizePoison:
                        case WellKnownSpells.OpenClose:
                        case WellKnownSpells.OtilukesResilientSphere:
                        case WellKnownSpells.Prayer:
                        case WellKnownSpells.ProtectionFromArrows:
                        case WellKnownSpells.ProtectionFromChaos:
                        case WellKnownSpells.ProtectionFromElements:
                        case WellKnownSpells.ProtectionFromEvil:
                        case WellKnownSpells.ProtectionFromGood:
                        case WellKnownSpells.ProtectionFromLaw:
                        case WellKnownSpells.PryingEyes:
                        case WellKnownSpells.RaiseDead:
                        case WellKnownSpells.ReadMagic:
                        case WellKnownSpells.RemoveBlindnessDeafness:
                        case WellKnownSpells.RemoveCurse:
                        case WellKnownSpells.RemoveDisease:
                        case WellKnownSpells.RemoveFear:
                        case WellKnownSpells.RemoveParalysis:
                        case WellKnownSpells.Resistance:
                        case WellKnownSpells.ResistElements:
                        case WellKnownSpells.Restoration:
                        case WellKnownSpells.Resurrection:
                        case WellKnownSpells.RighteousMight:
                        case WellKnownSpells.Sanctuary:
                            return false;
                        default:
                            return true;
                    }

                    return true;
                }

                return false;
            }

            if (spellEnum == WellKnownSpells.KeenEdge)
            {
                return false;
            }

            if (spellEnum > WellKnownSpells.DiscernLies)
            {
                switch (spellEnum)
                {
                    case WellKnownSpells.DispelChaos:
                    case WellKnownSpells.DispelEvil:
                    case WellKnownSpells.DispelLaw:
                    case WellKnownSpells.DispelMagic:
                    case WellKnownSpells.Displacement:
                    case WellKnownSpells.DivineFavor:
                    case WellKnownSpells.DivinePower:
                    case WellKnownSpells.Emotion:
                    case WellKnownSpells.Endurance:
                    case WellKnownSpells.EndureElements:
                    case WellKnownSpells.Enlarge:
                    case WellKnownSpells.EntropicShield:
                    case WellKnownSpells.ExpeditiousRetreat:
                    case WellKnownSpells.FindTraps:
                    case WellKnownSpells.FireShield:
                    case WellKnownSpells.FreedomOfMovement:
                    case WellKnownSpells.GaseousForm:
                    case WellKnownSpells.GiantVermin:
                    case WellKnownSpells.Goodberry:
                    case WellKnownSpells.GreaterMagicFang:
                    case WellKnownSpells.GreaterMagicWeapon:
                    case WellKnownSpells.Guidance:
                    case WellKnownSpells.Haste:
                    case WellKnownSpells.HoldPortal:
                    case WellKnownSpells.Identify:
                    case WellKnownSpells.ImprovedInvisibility:
                    case WellKnownSpells.Invisibility:
                    case WellKnownSpells.InvisibilitySphere:
                    case WellKnownSpells.InvisibilityToAnimals:
                    case WellKnownSpells.InvisibilityToUndead:
                        return false;
                    default:
                        return true;
                }

                return true;
            }

            if (spellEnum == WellKnownSpells.DiscernLies)
            {
                return false;
            }

            switch (spellEnum)
            {
                case WellKnownSpells.Aid:
                case WellKnownSpells.AnimalFriendship:
                case WellKnownSpells.AnimalGrowth:
                case WellKnownSpells.ArcaneLock:
                case WellKnownSpells.Barkskin:
                case WellKnownSpells.Bless:
                case WellKnownSpells.BlessWeapon:
                case WellKnownSpells.Blink:
                case WellKnownSpells.Blur:
                case WellKnownSpells.BullsStrength:
                case WellKnownSpells.CalmAnimals:
                case WellKnownSpells.CalmEmotions:
                case WellKnownSpells.CatsGrace:
                case WellKnownSpells.ClairaudienceClairvoyance:
                case WellKnownSpells.Consecrate:
                case WellKnownSpells.CureCriticalWounds:
                case WellKnownSpells.CureLightWounds:
                case WellKnownSpells.CureMinorWounds:
                case WellKnownSpells.CureModerateWounds:
                case WellKnownSpells.CureSeriousWounds:
                case WellKnownSpells.CurseWater:
                case WellKnownSpells.Darkvision:
                case WellKnownSpells.Daylight:
                case WellKnownSpells.DeathWard:
                case WellKnownSpells.DelayPoison:
                case WellKnownSpells.DetectChaos:
                case WellKnownSpells.DetectEvil:
                case WellKnownSpells.DetectLaw:
                case WellKnownSpells.DetectMagic:
                case WellKnownSpells.DetectSecretDoors:
                case WellKnownSpells.DetectUndead:
                case WellKnownSpells.DimensionDoor:
                    return false;
                default:
                    return true;
            }

            return true;
        }

        public bool IsValidSpell(int spEnum)
        {
            if (spEnum <= 0)
            {
                return false;
            }

            if (IsLabel(spEnum))
            {
                return false;
            }

            if (spEnum > SPELL_ENUM_MAX_EXPANDED)
            {
                return false;
            }

            return _spells.ContainsKey(spEnum);
        }

        public bool IsSpellLike(int spellEnum)
        {
            return spellEnum >= NORMAL_SPELL_RANGE && spellEnum <= SPELL_LIKE_ABILITY_RANGE
                   || spellEnum >= CLASS_SPELL_LIKE_ABILITY_START;
        }

        [TempleDllLocation(0x1007a140)]
        public bool spellCanCast(GameObjectBody caster, int spellEnum, int spellClass, int spellLevel)
        {
            List<int> classCodes = new List<int>();
            List<int> spellLevels = new List<int>();

            if (GameSystems.D20.D20Query(caster, D20DispatcherKey.QUE_CannotCast))
            {
                return false;
            }

            // NOTE: This is more than vanilla checked here
            if (spellClass == GetSpellClass(Stat.level_paladin) && caster.D20Query(D20DispatcherKey.QUE_IsFallenPaladin))
            {
                return false;
            }

            if (IsDomainSpell(spellClass)) // domain spell
            {
                if (numSpellsMemorizedTooHigh(caster))
                {
                    return false;
                }

                spellMemorizedQueryGetData(caster, spellEnum, classCodes, spellLevels);
                for (int i = 0; i < classCodes.Count; i++)
                {
                    if (IsDomainSpell(classCodes[i])
                        && (classCodes[i] & 0x7F) == (spellClass & 0x7F)
                        && spellLevels[i] == spellLevel)
                        return true;
                }

                return false;
            }

            // non-domain
            var classEnum = GetCastingClass(spellClass);
            if (D20ClassSystem.IsNaturalCastingClass(classEnum))
            {
                if (numSpellsKnownTooHigh(caster))
                {
                    return false;
                }

                SpellKnownQueryGetData(caster, spellEnum, classCodes, spellLevels);
                for (var i = 0; i < classCodes.Count; i++)
                {
                    if (!IsDomainSpell(classCodes[i])
                        && GetCastingClass(classCodes[i]) == classEnum
                        && spellLevels[i] <= spellLevel)
                    {
                        //if (spellLevelsVec[i] < (int)spellLevel)
                        //	Logger.Info("Natural Spell Caster spellCanCast check - spell known is lower level than spellCanCast queried spell. Is this ok?? (this is vanilla code here...)"); // yes
                        return true;
                    }
                }

                return false;
            }

            if (numSpellsMemorizedTooHigh(caster))
            {
                return false;
            }

            spellMemorizedQueryGetData(caster, spellEnum, classCodes, spellLevels);
            for (int i = 0; i < classCodes.Count; i++)
            {
                if (!IsDomainSpell(classCodes[i])
                    && (classCodes[i] & 0x7F) == (spellClass & 0x7F)
                    && spellLevels[i] == spellLevel)
                    return true;
            }

            return false;
        }

        [TempleDllLocation(0x1007a370)]
        private bool spellCanCast(GameObjectBody caster, int spellEnum)
        {
            var spellClasses = new List<int>();
            var spellLevels = new List<int>();
            if (!SpellKnownQueryGetData(caster, spellEnum, spellClasses, spellLevels))
            {
                return false;
            }

            for (var i = 0; i < spellClasses.Count; i++)
            {
                if (spellCanCast(caster, spellEnum, spellClasses[i], spellLevels[i]))
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x100762d0)]
        public bool SpellKnownQueryGetData(GameObjectBody obj, int spellId, List<int> classCodesOut,
            List<int> spellLevels)
        {
            var n = 0;
            var spellArray = obj.GetSpellArray(obj_f.critter_spells_known_idx);
            var numSpellsKnown = spellArray.Count;
            for (var i = 0; i < numSpellsKnown; i++)
            {
                var spellData = spellArray[i];
                if (spellData.spellEnum == spellId)
                {
                    classCodesOut.Add(spellData.classCode);
                    spellLevels.Add(spellData.spellLevel);
                    ++n;
                }
            }

            return n > 0;
        }

        [TempleDllLocation(0x10076190)]
        private void spellMemorizedQueryGetData(GameObjectBody critter, int spellEnum, List<int> classCodesOut,
            List<int> slotLevelsOut)
        {
            var numSpellsMemod = critter.GetSpellArray(obj_f.critter_spells_memorized_idx).Count;
            for (var i = 0; i < numSpellsMemod; i++)
            {
                var spellData = critter.GetSpell(obj_f.critter_spells_memorized_idx, i);
                if (spellData.spellEnum == spellEnum && !spellData.spellStoreState.usedUp)
                {
                    classCodesOut.Add(spellData.classCode);
                    slotLevelsOut.Add(spellData.spellLevel);
                }
            }
        }

        private bool numSpellsKnownTooHigh(GameObjectBody objHnd)
        {
            if (objHnd.GetSpellArray(obj_f.critter_spells_known_idx).Count > MAX_SPELLS_KNOWN)
            {
                Logger.Info("spellCanCast(): ERROR! This critter knows WAAY too many spells! Returning 0.");
                return true;
            }

            return false;
        }

        private bool numSpellsMemorizedTooHigh(GameObjectBody critter)
        {
            if (critter.GetSpellArray(obj_f.critter_spells_memorized_idx).Count > MAX_SPELLS_KNOWN)
            {
                Logger.Info("spellCanCast(): ERROR! This critter memorized WAAY too many spells! Returning 0.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Does the critter know any arcane spells from their class levels.
        /// </summary>
        [TempleDllLocation(0x100760e0)]
        public bool CanCastArcaneSpells(GameObjectBody critter)
        {
            if ((critter.GetCritterFlags() & CritterFlag.HAS_ARCANE_ABILITY) != 0)
            {
                return true;
            }

            foreach (var classEnum in D20ClassSystem.ClassesWithSpellLists)
            {
                if (D20ClassSystem.GetSpellListType(classEnum) == SpellListType.Arcane
                    && critter.GetStat(classEnum) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetNumSpellsPerDay(GameObjectBody caster, Stat classCode, in int spellLvl)
        {
            var effLvl = GameSystems.Critter.GetSpellListLevelExtension(caster, classCode) + caster.GetStat(classCode);
            return D20ClassSystem.GetNumSpellsFromClass(caster, classCode, spellLvl, effLvl);
        }

        public int NumSpellsInLevel(GameObjectBody caster, obj_f spellField, int spellClass, int spellLvl)
        {
            var spArray = caster.GetSpellArray(spellField);
            var N = spArray.Count;
            var count = 0;
            for (var i = 0; i < N; i++)
            {
                var spData = spArray[i];
                if (spData.classCode == spellClass && spData.spellLevel == spellLvl)
                    count++;
            }

            return count;
        }

        [TempleDllLocation(0x10079b70)]
        public void SpellPacketSetCasterLevel(SpellPacketBody spellPkt)
        {
            var spellClassCode = spellPkt.spellClass;
            var caster = spellPkt.caster;
            var spellEnum = spellPkt.spellEnum;
            var spellName = GetSpellName(spellEnum);
            var casterName = GameSystems.MapObject.GetDisplayName(caster);

            var casterObj = caster;

            // normal spells
            if (!IsDomainSpell(spellClassCode))
            {
                var casterClass = GetCastingClass(spellClassCode);
                // casting class
                if (casterClass != default)
                {
                    var casterLvl = GameSystems.Critter.GetCasterLevelForClass(caster, casterClass);
                    spellPkt.casterLevel = casterLvl;
                    Logger.Info("Critter {0} is casting spell {1} at base caster_level {2}.", casterName, spellName,
                        casterLvl);
                }

                // item spell
                else if (spellPkt.invIdx != 255 &&
                         (spellPkt.spellEnum < NORMAL_SPELL_RANGE || spellPkt.spellEnum > SPELL_LIKE_ABILITY_RANGE))
                {
                    spellPkt.casterLevel = 0;
                    Logger.Info("Critter {0} is casting item spell {1} at base caster_level {2}.", casterName,
                        spellName, 0);
                }

                // monster
                else if (casterObj.IsCritter())
                {
                    if (caster.IsNPC())
                    {
                        spellPkt.casterLevel = casterObj.GetInt32Array(obj_f.npc_hitdice_idx)[0];
                    }
                    else
                    {
                        spellPkt.casterLevel = casterObj.GetInt32Array(obj_f.critter_level_idx).Count;
                    }

                    Logger.Info("Monster {0} is casting spell {1} at base caster_level {2}.", casterName, spellName,
                        spellPkt.casterLevel);
                }
                else
                {
                    spellPkt.casterLevel = 0;
                }
            }
            else
            {
                // domain spell
                if (spellPkt.spellClass == (int) DomainId.Special)
                {
                    // domain special (usually used for monsters)
                    if (spellPkt.invIdx != 255 && (spellPkt.spellEnum < NORMAL_SPELL_RANGE ||
                                                   spellPkt.spellEnum > SPELL_LIKE_ABILITY_RANGE))
                    {
                        spellPkt.casterLevel = 0;
                        Logger.Info("Critter {0} is casting item spell {1} at base caster_level {2}.", casterName,
                            spellName, 0);
                    }

                    else
                    {
                        spellPkt.casterLevel = GameSystems.Critter.GetHitDiceNum(caster);
                        Logger.Info("Monster {0} is casting spell {1} at base caster_level {2}.", casterName, spellName,
                            spellPkt.casterLevel);
                    }
                }
                else if (caster.GetStat(Stat.level_cleric) > 0)
                {
                    spellPkt.casterLevel = GameSystems.Critter.GetCasterLevelForClass(caster, Stat.level_cleric);
                    Logger.Info("Critter {0} is casting Domain spell {1} at base caster_level {2}.", casterName,
                        spellName, spellPkt.casterLevel);
                }
            }


            var orgCasterLvl = spellPkt.casterLevel;
            spellPkt.casterLevel = caster.Dispatch35CasterLevelModify(spellPkt);

            // if changed
            if (spellPkt.casterLevel != orgCasterLvl)
            {
                Logger.Info("Spell level modified to {0}", spellPkt.casterLevel);
            }
        }

        [TempleDllLocation(0x10077270)]
        public int GetSpellRange(SpellEntry spellEntry, in int casterLevel, GameObjectBody caster)
        {
            var spellRangeType = spellEntry.spellRangeType;
            if (spellRangeType == SpellRangeType.SRT_Specified)
            {
                return spellEntry.spellRange;
            }

            return GetSpellRangeExact(spellRangeType, casterLevel, caster);
        }

        [TempleDllLocation(0x100771e0)]
        public int GetSpellRangeExact(SpellRangeType spellRangeType, int casterLevel, GameObjectBody caster)
        {
            switch (spellRangeType)
            {
                case SpellRangeType.SRT_Personal:
                    return 5;
                case SpellRangeType.SRT_Touch:
                    return (int) caster.GetReach(D20ActionType.TOUCH_ATTACK);
                case SpellRangeType.SRT_Close:
                    return (casterLevel / 2 + 5) * 5;
                case SpellRangeType.SRT_Medium:
                    return 2 * (5 * casterLevel + 50);
                case SpellRangeType.SRT_Long:
                    return 8 * (5 * casterLevel + 50);
                case SpellRangeType.SRT_Special_Inivibility_Purge:
                    return 5 * casterLevel;
                default:
                    Logger.Warn("GetSpellRangeExact: unknown range specified for spell entry");
                    break;
            }

            return 0;
        }

        [TempleDllLocation(0x100772a0)]
        public void PickerArgsFromSpellEntry(SpellEntry spEntry, PickerArgs args, GameObjectBody caster,
            int casterLvl, int? radiusTargetOverride = null)
        {
            var radiusTarget = radiusTargetOverride.GetValueOrDefault(spEntry.radiusTarget);

            args.flagsTarget = spEntry.flagsTargetBitmask;
            args.modeTarget = spEntry.modeTargetSemiBitmask;
            args.incFlags = spEntry.incFlagsTargetBitmask;
            args.excFlags = spEntry.excFlagsTargetBitmask;
            args.minTargets = spEntry.minTarget;
            args.maxTargets = spEntry.maxTarget;
            args.radiusTarget = radiusTarget;
            args.degreesTarget = spEntry.degreesTarget;
            if (spEntry.spellRangeType != SpellRangeType.SRT_Specified)
            {
                args.range = GetSpellRangeExact(spEntry.spellRangeType, casterLvl, caster);
            }
            else
            {
                args.range = spEntry.spellRange;
            }

            args.callback = null;
            args.caster = caster;

            if (spEntry.IsBaseModeTarget(UiPickerType.Single)
                && ((ulong) spEntry.modeTargetSemiBitmask & 0xffffFFFF00000000) == 0
                && spEntry.spellRangeType == SpellRangeType.SRT_Touch)
            {
                args.flagsTarget &= ~UiPickerFlagsTarget.Range;
            }

            if (spEntry.IsBaseModeTarget(UiPickerType.Cone))
            {
                args.radiusTarget = args.range;
            }

            if (spEntry.IsBaseModeTarget(UiPickerType.Personal))
            {
                if (radiusTarget < 0)
                {
                    var srt = (SpellRangeType) (-radiusTarget);
                    args.radiusTarget = GetSpellRangeExact(srt, casterLvl, caster);
                }


                if (spEntry.flagsTargetBitmask.HasFlag(UiPickerFlagsTarget.Radius))
                {
                    args.range = radiusTarget;
                }
            }

            if (spEntry.spellRangeType == SpellRangeType.SRT_Personal
                && spEntry.IsBaseModeTarget(UiPickerType.Area))
            {
                /*if (spEntry.spellRangeType == SRT_Specified){
                    args.range = spEntry.spellRange;
                }
                else{
                    args.range = GetSpellRangeExact(spEntry.spellRangeType, casterLvl, caster);
                }*/
                // seems to do the spell range thing as above, so skipping this
            }

            if (args.maxTargets <= 0 && spEntry.IsBaseModeTarget(UiPickerType.Multi))
            {
                var maxTgts = -args.maxTargets;
                var lvlOffset = maxTgts / 10000;
                maxTgts = maxTgts % 10000;

                var cap = maxTgts / 100;
                var rem = maxTgts % 100;
                var b = rem / 10;
                var c = rem % 10;
                var nom = c + casterLvl - lvlOffset;
                var denom = b + 1;

                maxTgts = nom / denom;

                if (cap != 0 && maxTgts > cap)
                    maxTgts = cap;

                args.maxTargets = maxTgts;
            }

            if (spEntry.IsBaseModeTarget(UiPickerType.Area)
                && (spEntry.spellEnum == 133 // Dispel Magic
                    || spEntry.spellEnum == 434))
            {
                // Silence
                args.modeTarget |= UiPickerType.AreaOrObj;
            }
        }

        [TempleDllLocation(0x10075300)]
        public SchoolOfMagic GetSpellSchoolEnum(int spellEnum)
        {
            return GetSpellEntry(spellEnum).spellSchoolEnum;
        }

        [TempleDllLocation(0x100753d0)]
        public string GetSpellSchoolEnumName(SchoolOfMagic spellSchoolEnum)
        {
            return _spellEnumMes[15000 + (int) spellSchoolEnum];
        }

        private static bool TryGetCounterSpell(int spellEnum, out int counterSpellEnum)
        {
            switch (spellEnum)
            {
                case WellKnownSpells.Bless:
                    counterSpellEnum = WellKnownSpells.Bane;
                    return true;
                case WellKnownSpells.Bane:
                    counterSpellEnum = WellKnownSpells.Bless;
                    return true;
                case WellKnownSpells.CauseFear:
                    counterSpellEnum = WellKnownSpells.RemoveFear;
                    return true;
                case WellKnownSpells.ChillMetal:
                    counterSpellEnum = WellKnownSpells.HeatMetal;
                    return true;
                case WellKnownSpells.Desecrate:
                    counterSpellEnum = WellKnownSpells.Consecrate;
                    return true;
                case WellKnownSpells.Consecrate:
                    counterSpellEnum = WellKnownSpells.Desecrate;
                    return true;
                case WellKnownSpells.BlindnessDeafness:
                    counterSpellEnum = WellKnownSpells.RemoveBlindnessDeafness;
                    return true;
                case WellKnownSpells.BestowCurse:
                    counterSpellEnum = WellKnownSpells.RemoveCurse;
                    return true;
                case WellKnownSpells.Enlarge:
                    counterSpellEnum = WellKnownSpells.Reduce;
                    return true;
                case WellKnownSpells.Haste:
                    counterSpellEnum = WellKnownSpells.Slow;
                    return true;
                case WellKnownSpells.HeatMetal:
                    counterSpellEnum = WellKnownSpells.ChillMetal;
                    return true;
                case WellKnownSpells.RemoveBlindnessDeafness:
                    counterSpellEnum = WellKnownSpells.BlindnessDeafness;
                    return true;
                case WellKnownSpells.RemoveCurse:
                    counterSpellEnum = WellKnownSpells.BestowCurse;
                    return true;
                case WellKnownSpells.Reduce:
                    counterSpellEnum = WellKnownSpells.Enlarge;
                    return true;
                case WellKnownSpells.RemoveFear:
                    counterSpellEnum = WellKnownSpells.CauseFear;
                    return true;
                case WellKnownSpells.Slow:
                    counterSpellEnum = WellKnownSpells.Haste;
                    return true;
                case WellKnownSpells.CrushingDespair:
                    counterSpellEnum = WellKnownSpells.GoodHope;
                    return true;
                case WellKnownSpells.GoodHope:
                    counterSpellEnum = WellKnownSpells.CrushingDespair;
                    return true;
                default:
                    counterSpellEnum = -1;
                    return false;
            }
        }

        /// <summary>
        /// Try to find the right id for the spell to counter with.
        /// </summary>
        public int FindCounterSpellId(GameObjectBody caster, int spellEnum, int spellLevel)
        {
            if (TryGetCounterSpell(spellEnum, out var specificCounterSpell))
            {
                if (spellCanCast(caster, specificCounterSpell))
                {
                    return specificCounterSpell;
                }
            }

            if (spellCanCast(caster, spellEnum))
            {
                return spellEnum;
            }

            // Improved counterspell allows the caster to use any spell of the same school as the counterspell
            if (GameSystems.Feat.HasFeat(caster, FeatId.IMPROVED_COUNTERSPELL))
            {
                var schoolToCounter = GetSpellSchoolEnum(spellEnum);
                foreach (var memorizedSpell in EnumerateMemorizedSpells(caster))
                {
                    if (TryGetSpellEntry(memorizedSpell.spellEnum, out var spellEntry))
                    {
                        var memorySpellSchool = spellEntry.spellSchoolEnum;
                        if (memorySpellSchool == schoolToCounter)
                        {
                            return memorizedSpell.spellEnum;
                        }
                    }
                }
            }

            if (spellCanCast(caster, WellKnownSpells.DispelMagic))
            {
                return WellKnownSpells.DispelMagic;
            }

            return 0;
        }

        private IEnumerable<SpellStoreData> EnumerateMemorizedSpells(GameObjectBody caster)
        {
            var spellArray = caster.GetSpellArray(obj_f.critter_spells_memorized_idx);

            for (var index = 0; index < spellArray.Count; index++)
            {
                yield return spellArray[index];
            }
        }

        [TempleDllLocation(0x10078d90)]
        public bool TryGetMemorizedSpell(GameObjectBody caster, int spellEnum, out SpellStoreData spellData)
        {
            foreach (var memorizedSpell in EnumerateMemorizedSpells(caster))
            {
                if (memorizedSpell.spellEnum == spellEnum)
                {
                    spellData = memorizedSpell;
                    return true;
                }
            }

            spellData = default;
            return false;
        }

        [TempleDllLocation(0x10075a90)]
        public void DeleteMemorizedSpell(GameObjectBody critter, int spellEnum)
        {
            var memorizedSpells = critter.GetSpellArray(obj_f.critter_spells_memorized_idx);

            for (var i = 0; i < memorizedSpells.Count; i++)
            {
                var memorizedSpell = memorizedSpells[i];
                if (memorizedSpell.spellEnum == spellEnum)
                {
                    if ((memorizedSpell.classCode & 0x80) != 0)
                    {
                        critter.RemoveSpell(obj_f.critter_spells_memorized_idx, i);
                    }

                    return;
                }
            }
        }

        public bool IsNaturalSpellsPerDayDepleted(GameObjectBody critter, int spellLvl, int spellClass)
        {
            var classCode = GetCastingClass(spellClass);

            var spellsPerDay = GetNumSpellsPerDay(critter, classCode, spellLvl);
            var spellsCastNum = NumSpellsInLevel(critter, obj_f.critter_spells_cast_idx, spellClass, spellLvl);

            return spellsCastNum >= spellsPerDay;
        }

        [TempleDllLocation(0x10079030)]
        public bool GetSpellTargets(GameObjectBody obj, GameObjectBody tgt, SpellPacketBody spellPkt, int spellEnum)
        {
            // returns targets using the picker function
            if (!TryGetSpellEntry(spellEnum, out var spEntry))
            {
                return false;
            }

            var pickArgs = new PickerArgs();
            PickerArgsFromSpellEntry(spEntry, pickArgs, obj, spellPkt.casterLevel);

            var modeTgt = pickArgs.GetBaseModeTarget();
            LocAndOffsets loc;
            switch (modeTgt)
            {
                case UiPickerType.Single:
                case UiPickerType.Multi:
                    pickArgs.SetSingleTgt(tgt);
                    break;
                case UiPickerType.Personal:
                    if ((spEntry.flagsTargetBitmask & UiPickerFlagsTarget.Range) != default)
                    {
                        loc = obj.GetLocationFull();
                        pickArgs.SetAreaTargets(loc);
                    }
                    else
                    {
                        pickArgs.SetSingleTgt(obj);
                    }

                    break;
                case UiPickerType.Cone:
                    loc = tgt.GetLocationFull();
                    pickArgs.SetConeTargets(loc);
                    break;
                case UiPickerType.Area:
                    if (spEntry.spellRangeType == SpellRangeType.SRT_Personal)
                    {
                        loc = obj.GetLocationFull();
                    }
                    else
                    {
                        loc = tgt.GetLocationFull();
                    }

                    pickArgs.SetAreaTargets(loc);
                    break;
            }

            ConfigSpellTargetting(pickArgs, spellPkt);
            pickArgs.FreeObjlist();
            return spellPkt.Targets.Length > 0;
        }

        [TempleDllLocation(0x100B9690)]
        public void ConfigSpellTargetting(PickerArgs args, SpellPacketBody spPkt)
        {
            var flags = args.result.flags;

            if ((flags & PickerResultFlags.PRF_HAS_SINGLE_OBJ) != default)
            {
                var target = args.result.handle;
                Trace.Assert(target != null);

                // add for the benefit of AI casters
                if (args.IsBaseModeTarget(UiPickerType.Multi))
                {
                    var targetCount = 1;
                    if (!args.IsModeTargetFlagSet(UiPickerType.OnceMulti))
                    {
                        targetCount = MAX_SPELL_TARGETS;
                    }

                    if (targetCount > args.maxTargets)
                    {
                        targetCount = args.maxTargets;
                    }

                    var targets = new GameObjectBody[targetCount];
                    Array.Fill(targets, target);
                    spPkt.SetTargets(targets);
                    spPkt.InitialTargets = targets.ToImmutableSortedSet();
                }
                else
                {
                    spPkt.SetTargets(new[] {target});
                    spPkt.InitialTargets = new[] {target}.ToImmutableSortedSet();
                }
            }
            else
            {
                spPkt.SetTargets(Array.Empty<GameObjectBody>());
                spPkt.InitialTargets = ImmutableSortedSet<GameObjectBody>.Empty;
            }

            if ((flags & PickerResultFlags.PRF_HAS_MULTI_OBJ) != default)
            {
                spPkt.SetTargets(args.result.objList);
                Trace.Assert(spPkt.Targets.Length > 0);

                // else apply the rest of the targeting to the last object
                if (args.IsBaseModeTarget(UiPickerType.Multi) && !args.IsModeTargetFlagSet(UiPickerType.OnceMulti)
                                                              && args.result.objList.Count < args.maxTargets)
                {
                    var currentTargetCount = spPkt.Targets.Length;
                    var replicateTarget = spPkt.Targets[^1].Object;
                    for (var i = currentTargetCount; i < args.maxTargets; i++)
                    {
                        spPkt.AddTarget(replicateTarget);
                    }
                }

                spPkt.InitialTargets = spPkt.Targets.Select(t => t.Object).ToImmutableSortedSet();
            }

            if ((flags & PickerResultFlags.PRF_HAS_LOCATION) != default)
            {
                spPkt.aoeCenter = args.result.location;
                spPkt.aoeCenterZ = args.result.offsetz;
            }
            else
            {
                spPkt.aoeCenter = LocAndOffsets.Zero;
                spPkt.aoeCenterZ = 0;
            }

            if ((flags & PickerResultFlags.PRF_UNK8) != default)
            {
                Logger.Debug("ui_picker: not implemented - BECAME_TOUCH_ATTACK");
            }
        }

        [TempleDllLocation(0x100753c0)]
        public string GetSchoolOfMagicName(SchoolOfMagic school)
        {
            return _schoolsOfMagicNames[(int) school];
        }

        [TempleDllLocation(0x100c3220)]
        public void PlayFizzle(GameObjectBody caster)
        {
            GameSystems.ParticleSys.CreateAtObj("Fizzle", caster);
            GameSystems.SoundGame.PositionalSound(17122, caster);
        }

        [TempleDllLocation(0x100794f0)]
        public SpellComponent GetSpellComponentRegardMetamagic(int spellEnum, MetaMagicData metaMagicData)
        {
            if (!TryGetSpellEntry(spellEnum, out var spEntry))
            {
                Logger.Info("Could not find spell {0}.", spellEnum);
                return 0;
            }

            var result = spEntry.spellComponentBitmask;
            if (metaMagicData.IsSilent)
            {
                result &= ~SpellComponent.Verbal;
            }

            if (metaMagicData.IsStill)
            {
                result &= ~SpellComponent.Somatic;
            }

            return result;
        }

        // TODO: This needs to be replaced with code from D20ClassSystem
        [TempleDllLocation(0x100765b0)]
        public int GetMaxSpellLevel(GameObjectBody caster, Stat classCode, int casterLvl = 0)
        {
            Stat abilityType;

            var abilityScore = -1;
            if (casterLvl <= 0)
            {
                casterLvl = caster.GetStat(classCode);
            }

            switch (classCode)
            {
                case Stat.level_bard:
                    if (casterLvl == 1)
                    {
                        casterLvl = 0;
                        abilityType = Stat.charisma;
                    }
                    else
                    {
                        casterLvl = (casterLvl + 2) / 3;
                        abilityType = Stat.charisma;
                    }

                    break;
                case Stat.level_cleric:
                case Stat.level_druid:
                    abilityType = Stat.wisdom;
                    casterLvl = (casterLvl + 1) / 2;
                    break;
                case Stat.level_paladin:
                case Stat.level_ranger:
                    abilityType = Stat.wisdom;
                    casterLvl = GetPaladinRangerCasterLevel(casterLvl);
                    break;
                case Stat.level_sorcerer:
                    if (casterLvl == 1)
                    {
                        abilityType = Stat.charisma;
                    }
                    else
                    {
                        casterLvl /= 2;
                        abilityType = Stat.charisma;
                    }

                    break;
                case Stat.level_wizard:
                    abilityType = Stat.intelligence;
                    casterLvl = (casterLvl + 1) / 2;
                    break;
                default:
                    return 0;
            }

            if (caster.GetDispatcher() != null)
            {
                abilityScore = caster.GetStat(abilityType) - 10;
            }
            else
            {
                abilityScore = caster.GetBaseStat(abilityType);
            }

            var result = abilityScore;
            if (casterLvl <= abilityScore)
            {
                result = casterLvl;
            }

            return result;
        }

        [TempleDllLocation(0x11eb6437)]
        private static int GetPaladinRangerCasterLevel(int classLevel)
        {
            if (classLevel < 14)
            {
                if (classLevel > 10)
                {
                    return 3;
                }
                else
                {
                    return classLevel / 4;
                }
            }
            else
            {
                return 4;
            }
        }

        [TempleDllLocation(0x1007b210)]
        public IEnumerable<SpellEntry> EnumerateLearnableSpells(GameObjectBody caster)
        {
            foreach (var spellEntry in _spells)
            {
                if (SpellLearnableByObj(caster, (Stat) 3000, in spellEntry))
                {
                    yield return spellEntry;
                }
            }
        }

        [TempleDllLocation(0x10075eb0)]
        private bool SpellLearnableByObj(GameObjectBody caster, Stat classCode, in SpellEntry spEntry)
        {
            var isCleric = false;
            foreach (var spellLevel in spEntry.spellLvls)
            {
                // Non-Domain Spell
                if ((spellLevel.spellClass & 0x80) != 0)
                {
                    var entryClassCode = (Stat) (spellLevel.spellClass & 0x7F);
                    if (entryClassCode == classCode || caster.GetStat(entryClassCode) > 0)
                    {
                        return true;
                    }
                }
                else
                {
                    var domain = spellLevel.spellClass & 0x7F;

                    if (classCode == Stat.level_cleric || caster.GetStat(Stat.level_cleric) > 0)
                    {
                        isCleric = true;
                    }
                    else
                    {
                        if (!isCleric && (caster.IsPC() || domain != (int) DomainId.Special))
                        {
                            continue;
                        }
                    }

                    if (caster.GetStat(Stat.domain_1) == domain || caster.GetStat(Stat.domain_2) == domain)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x10078f20)]
        public bool TryParseSpellSpecString(string spellSpec, out SpellStoreData spellDataOut)
        {
            var tokenizer = new Tokenizer(spellSpec);
            if (tokenizer.NextToken() && tokenizer.IsQuotedString)
            {
                if (!GetSpellEnumByEnglishName(tokenizer.TokenText, out var spellEnum))
                {
                    Logger.Warn("Could not find spell '{0}'", tokenizer.TokenText);
                    spellDataOut = default;
                    return false;
                }

                if (tokenizer.NextToken() && tokenizer.IsIdentifier)
                {
                    if (!GetSpellClassCode(tokenizer.TokenText, out var classCode))
                    {
                        Logger.Warn("Unable to parse spell '{0}'", tokenizer.TokenText);
                        spellDataOut = default;
                        return false;
                    }

                    if (tokenizer.NextToken() && tokenizer.IsNumber)
                    {
                        var spellLevel = tokenizer.TokenInt;
                        spellDataOut = new SpellStoreData(
                            spellEnum,
                            spellLevel,
                            classCode
                        );
                        return true;
                    }
                }
            }

            Logger.Warn($"Malformed spell spec: '{spellSpec}'");
            spellDataOut = default;
            return false;
        }

        [TempleDllLocation(0x10075340)]
        public SubschoolOfMagic GetSpellSubSchool(int spEnum)
        {
            if (TryGetSpellEntry(spEnum, out var entry))
            {
                return entry.spellSubSchoolEnum;
            }

            return 0;
        }

        [TempleDllLocation(0x10075380)]
        public SpellDescriptor GetSpellDescriptor(int spellEnum)
        {
            if (TryGetSpellEntry(spellEnum, out var entry))
            {
                return entry.spellDescriptorBitmask;
            }

            return default;
        }


        // TODO: This actually hardcodes the staff's caster level at 7
        [TempleDllLocation(0x10079db0)]
        public bool CreateSpellPacketForStaff(int spellEnum, GameObjectBody caster, out SpellPacketBody spellPacket)
        {
            if (!TryGetSpellEntry(spellEnum, out var spellEntry))
            {
                spellPacket = default;
                return false;
            }

            var highestCasterLevel = -1;
            var highestCasterLevelClass = -1;
            var pkt = new SpellPacketBody();
            spellPacket = pkt;
            pkt.spellEnum = spellEnum;
            pkt.caster = caster;

            // TODO: This is kinda dirty and needs a better utility function
            foreach (var spellLvl in spellEntry.spellLvls.Concat(GetSpellListExtension(spellEnum)))
            {
                pkt.spellClass = spellLvl.spellClass;
                if (((pkt.spellClass & 0x80) != 0 || caster.GetStat(Stat.level_cleric) <= 0)
                    && (caster.IsPC() || (pkt.spellClass & 0x7F) != 23))
                {
                    if (((pkt.spellClass) & 0x80) == 0 || caster.GetStat((Stat) (pkt.spellClass & 0x7F)) <= 0)
                    {
                        continue;
                    }

                    SpellPacketSetCasterLevel(pkt);
                }
                else
                {
                    SpellPacketSetCasterLevel(pkt);
                }

                if (pkt.casterLevel > highestCasterLevel)
                {
                    highestCasterLevel = pkt.casterLevel;
                    highestCasterLevelClass = spellLvl.spellClass;
                }
            }

            if (highestCasterLevelClass == -1)
            {
                highestCasterLevel = 7;
            }

            spellPacket.caster = caster;
            spellPacket.casterLevel = highestCasterLevel;
            spellPacket.spellEnum = spellEnum;
            spellPacket.spellClass = highestCasterLevelClass;
            return true;
        }

        [TemplePlusLocation("spell.cpp:2443")]
        public bool SpellHasMultiSelection(int spellEnum)
        {
            return _multiOptions.ContainsKey(spellEnum);
        }

        [TemplePlusLocation("spell.cpp:2452")]
        public bool TryGetMultiSelectOptions(int spellEnum, out List<SpellMultiOption> multiOptions)
        {
            return _multiOptions.TryGetValue(spellEnum, out multiOptions);
        }

        public string GetSpellsRadialMenuOptions(int spellEnum)
        {
            return _spellsRadialMenuOptions[spellEnum];
        }

        private IEnumerable<SpellPacketBody> ActiveSpells => _activeSpells.Values
            .Where(e => e.IsActive)
            .Select(e => e.Body);

        /// <summary>
        /// Checks if the given object is the target of any active spell.
        /// </summary>
        /// <param name="obj"></param>
        [TempleDllLocation(0x10076370)]
        public bool IsAffectedBySpell(GameObjectBody obj)
        {
            foreach (var activeSpell in ActiveSpells)
            {
                if (activeSpell.HasTarget(obj))
                {
                    return true;
                }
            }

            return false;
        }

        // This was a hook applied in a DLLfix before, hence the high address
        [TempleDllLocation(0x11eb630c)]
        public void SanitizeSpellSlots(GameObjectBody caster)
        {
            foreach (var statClass in D20ClassSystem.ClassesWithSpellLists)
            {
                if (D20ClassSystem.IsVancianCastingClass(statClass))
                {
                    SanitizeSpellSlots(caster, statClass);
                }
            }
        }

        private int GetMemorizedSpellCount(GameObjectBody caster, Stat classCode, int slotLvl)
        {
            var numMemorizedThisLvl = 0;
            var memorizedTotal = caster.GetSpellArray(obj_f.critter_spells_memorized_idx).Count;
            var spellClassCode = GetSpellClass(classCode);

            for (var i = 0; i < memorizedTotal; i++)
            {
                var spellData = caster.GetSpell(obj_f.critter_spells_memorized_idx, i);
                if (spellData.classCode == spellClassCode && spellData.spellLevel == slotLvl)
                {
                    numMemorizedThisLvl++;
                }
            }

            return numMemorizedThisLvl;
        }

        private void RemoveSurplusSpells(int surplus, GameObjectBody caster, Stat classCode, int slotLvl)
        {
            var numMemorized = caster.GetSpellArray(obj_f.critter_spells_memorized_idx).Count;
            var spellClassCode = GetSpellClass(classCode);

            for (var i = numMemorized - 1; i >= 0 && surplus > 0; i--)
            {
                var spellData = caster.GetSpell(obj_f.critter_spells_memorized_idx, i);
                if (spellData.classCode == spellClassCode && slotLvl == spellData.spellLevel)
                {
                    SpellRemoveFromStorage(caster, obj_f.critter_spells_memorized_idx, spellData, 0);
                    surplus--;
                }
            }
        }

        [TempleDllLocation(0x100758a0)]
        public bool SpellRemoveFromStorage(GameObjectBody caster, obj_f field, SpellStoreData spellToRemove,
            int spellsStoredFlags)
        {
            var spellArray = caster.GetSpellArray(field);

            for (var i = 0; i < spellArray.Count; i++)
            {
                var existingSpell = spellArray[i];

                if (existingSpell.spellEnum == spellToRemove.spellEnum
                    && existingSpell.spellLevel == spellToRemove.spellLevel
                    && existingSpell.classCode == spellToRemove.classCode
                    && ((spellsStoredFlags & 3) == 3
                        || existingSpell.spellStoreState == spellToRemove.spellStoreState
                        && existingSpell.metaMagicData == spellToRemove.metaMagicData))
                {
                    caster.RemoveSpell(field, i);
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x11eb64c8)]
        private void SanitizeSpellSlots(GameObjectBody caster, Stat statClass)
        {
            for (var slotLvl = 1; slotLvl < 10; slotLvl++)
            {
                var maxSpells = GetNumSpellsPerDay(caster, statClass, slotLvl);
                if (maxSpells >= 0)
                {
                    if (GetSchoolSpecialization(caster, out _, out _, out _))
                    {
                        maxSpells++;
                    }
                }

                var numSpells = GetMemorizedSpellCount(caster, statClass, slotLvl);
                if (numSpells > maxSpells)
                {
                    RemoveSurplusSpells(numSpells - maxSpells, caster, statClass, slotLvl);
                }
            }
        }

        [TempleDllLocation(0x100757d0)]
        public void PendingSpellsToMemorized(GameObjectBody caster)
        {
            var spellsMemo = caster.GetSpellArray(obj_f.critter_spells_memorized_idx);
            for (var i = 0; i < spellsMemo.Count; i++)
            {
                var spData = spellsMemo[i];
                if (spData.spellStoreState.usedUp)
                {
                    spData.spellStoreState.usedUp = false;
                    caster.SetSpell(obj_f.critter_spells_memorized_idx, i, spData);
                }
            }
        }

        public void SpellsPendingToMemorizedByClass(GameObjectBody caster, Stat classEnum)
        {
            var spellsMemo = caster.GetSpellArray(obj_f.critter_spells_memorized_idx);
            if (classEnum == (Stat) (-1))
            {
                // do for all classes
                for (var i = 0; i < spellsMemo.Count; i++)
                {
                    var spData = spellsMemo[i];
                    spData.spellStoreState.usedUp = false;
                    caster.SetSpell(obj_f.critter_spells_memorized_idx, i, spData);
                }
            }
            else
            {
                var spellClassCode = GetSpellClass(classEnum);
                for (var i = 0; i < spellsMemo.Count; i++)
                {
                    var spData = spellsMemo[i];
                    if (spData.classCode == spellClassCode)
                    {
                        spData.spellStoreState.usedUp = false;
                        caster.SetSpell(obj_f.critter_spells_memorized_idx, i, spData);
                    }
                }
            }
        }

        [TempleDllLocation(0x10079a20)]
        public void EndSpellsOfType(int spellEnum)
        {
            var spellsToEnd = new List<int>();
            foreach (var (spellId, activeSpell) in _activeSpells)
            {
                if (activeSpell.IsActive && activeSpell.Body.spellEnum == spellEnum)
                {
                    spellsToEnd.Add(spellId);
                }
            }

            foreach (var spellId in spellsToEnd)
            {
                EndSpell(spellId);
            }
        }

        [TempleDllLocation(0x10079980)]
        public void EndSpell(int spellId, bool ignoreRemainingTargets = false)
        {
            if (!_activeSpells.TryGetValue(spellId, out var activeSpell) || !activeSpell.IsActive)
            {
                return;
            }

            if (!ignoreRemainingTargets && activeSpell.Body.Targets.Length > 0)
            {
                Logger.Info("Not ending active spell {0} because it has targets remaining.", spellId);
                return;
            }

            GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.EndSpellCast);
            activeSpell.IsActive = false;
            _activeSpells[spellId] = activeSpell;
        }

        [TemplePlusLocation("spell.cpp")]
        public void SpellsCastReset(GameObjectBody caster, Stat? forClass = null)
        {
            if (!forClass.HasValue)
            {
                caster.ClearArray(obj_f.critter_spells_cast_idx);
                return;
            }

            var spellsCast = caster.GetSpellArray(obj_f.critter_spells_cast_idx);
            int initialSize = spellsCast.Count;
            var spellClassCode = GetSpellClass(forClass.Value);
            for (int i = initialSize - 1; i >= 0; i--)
            {
                // must be int!!!
                var spData = caster.GetSpell(obj_f.critter_spells_cast_idx, i);
                if (spData.classCode == spellClassCode)
                {
                    caster.RemoveSpell(obj_f.critter_spells_cast_idx, i);
                }
            }
        }

        [TempleDllLocation(0x10075210)]
        public int GetTotalSpellsPerDay(GameObjectBody critter, in int classCode)
        {
            if (IsDomainSpell(classCode))
            {
                return 2 * critter.GetStat(Stat.level_cleric);
            }
            else
            {
                var classStat = GetCastingClass(classCode);
                var classLevel = critter.GetStat(classStat);
                var totalSpellsPerDay = 0;
                for (var i = 0; i < 10; i++)
                {
                    totalSpellsPerDay += D20ClassSystem.GetNumSpellsFromClass(critter, classStat, i, classLevel);
                }

                return totalSpellsPerDay;
            }
        }

        [TempleDllLocation(0x100751a0)]
        public int SpellNumByFieldAndClass(GameObjectBody critter, obj_f field, int spellClassCode)
        {
            var count = 0;
            var array = critter.GetSpellArray(field);
            for (var i = 0; i < array.Count; i++)
            {
                var spell = array[i];
                if (spell.classCode == spellClassCode)
                {
                    count++;
                }
            }

            return count;
        }

        private SpellSlot[] GetSpellSlots(GameObjectBody caster, Stat classCode, int spellLvl, int classLvl)
        {
            // Get spells per day without the bonus spells from attribute
            var slotsFromClass = D20ClassSystem.GetNumSpellsFromClass(caster, classCode, spellLvl, classLvl, false);
            if (slotsFromClass == -1)
            {
                // If *no* slots are granted (not even 0), then the bonus spells will not apply either
                return Array.Empty<SpellSlot>();
            }

            var slotsFromAttribute = D20ClassSystem.GetBonusSpells(caster, classCode, spellLvl);

            var slotsFromSpecialization = D20ClassSystem.GetSpecialisationSlots(caster, classCode, spellLvl);

            // Create the slots and assign their respective source
            var result = new SpellSlot[slotsFromClass + slotsFromAttribute + slotsFromSpecialization];
            var index = 0;
            for (var i = 0; i < slotsFromClass; i++)
            {
                result[index++].Source = SpellSlotSource.ClassLevels;
            }

            for (var i = 0; i < slotsFromAttribute; i++)
            {
                result[index++].Source = SpellSlotSource.BonusSpells;
            }

            for (var i = 0; i < slotsFromSpecialization; i++)
            {
                result[index++].Source = SpellSlotSource.WizardSpecialization;
            }

            return result;
        }

        public List<SpellsPerDay> GetSpellsPerDay(GameObjectBody critter)
        {
            var result = new List<SpellsPerDay>();

            foreach (var classEnum in D20ClassSystem.ClassesWithSpellLists)
            {
                var classLevels = critter.GetStat(classEnum);
                if (classLevels <= 0)
                {
                    continue;
                }

                var spellClass = GetSpellClass(classEnum);

                var spellsPerDay = new SpellsPerDay();
                spellsPerDay.Name = GameSystems.Stat.GetStatName(classEnum);
                spellsPerDay.ShortName = GameSystems.Stat.GetStatShortName(classEnum);
                spellsPerDay.ClassCode = spellClass;
                for (var i = 0; i < spellsPerDay.Levels.Length; i++)
                {
                    ref var level = ref spellsPerDay.Levels[i];
                    level.Slots = GetSpellSlots(critter, classEnum, i, classLevels);
                }

                if (D20ClassSystem.IsVancianCastingClass(classEnum))
                {
                    spellsPerDay.Type = SpellsPerDayType.Vancian;

                    UpdateMemorizedSpells(critter, spellsPerDay);
                }
                else
                {
                    spellsPerDay.Type = SpellsPerDayType.Spontaneous;
                }

                result.Add(spellsPerDay);

                // Handle domain slots gained from cleric levels, although "domain slots" should be a class feature
                // handled differently.
                if (classEnum == Stat.caster_level_cleric)
                {
                    // "Domain"
                    var domainSpellList = new SpellsPerDay();
                    domainSpellList.Type = SpellsPerDayType.Vancian;
                    domainSpellList.Name = "#{char_ui_spells:2}";
                    domainSpellList.ShortName = domainSpellList.ShortName;
                    domainSpellList.ClassCode = 24;
                    result.Add(domainSpellList);
                }
            }

            return result;
        }

        public void UpdateMemorizedSpells(GameObjectBody critter, SpellsPerDay spellsPerDay)
        {
            var memorizedSpells = critter.GetSpellArray(obj_f.critter_spells_memorized_idx);
            var domainSpells = IsDomainSpell(spellsPerDay.ClassCode);

            // Clear all memorized spells first
            for (var i = 0; i < spellsPerDay.Levels.Length; i++)
            {
                ref var level = ref spellsPerDay.Levels[i];

                for (var j = 0; j < level.Slots.Length; j++)
                {
                    level.Slots[j].ClearSpell();
                }
            }

            for (var i = 0; i < memorizedSpells.Count; i++)
            {
                var spell = memorizedSpells[i];

                // Skip memorized spells for other classes, but keep domain spells in mind
                if (domainSpells && !IsDomainSpell(spell.classCode)
                    || !domainSpells && spell.classCode != spellsPerDay.ClassCode)
                {
                    continue;
                }

                if (spellsPerDay.TryFindEmptyUnusedSlot(spell.spellLevel, out var slotIndex))
                {
                    ref var slot = ref spellsPerDay.Levels[spell.spellLevel].Slots[slotIndex];
                    slot.MemorizedSpell = spell;
                }
            }
        }

        [TempleDllLocation(0x100778e0)]
        public string GetSpellDomainName(int classCode)
        {
            if (!IsDomainSpell(classCode))
            {
                throw new ArgumentException($"Class code {classCode} is not for a domain spell.");
            }

            return _spellMes[4000 + classCode];
        }

        [TempleDllLocation(0x101b5ad0)]
        public bool SpellOpposesAlignment(GameObjectBody caster, int castingClass, int spellEnum)
        {
            if (IsDomainSpell(castingClass) || GetCastingClass(castingClass) == Stat.level_cleric)
            {
                if (!TryGetSpellEntry(spellEnum, out var spellEntry))
                {
                    return false;
                }

                var alignment = caster.GetAlignment();
                var descriptor = spellEntry.spellDescriptorBitmask;
                var alignmentChoice = caster.GetInt32(obj_f.critter_alignment_choice);

                return alignment.IsLawful() && (descriptor & SpellDescriptor.CHAOTIC) != 0
                       || alignment.IsChaotic() && (descriptor & SpellDescriptor.LAWFUL) != 0
                       || alignment.IsGood() && (descriptor & SpellDescriptor.EVIL) != 0
                       || alignmentChoice == 1 && (descriptor & SpellDescriptor.EVIL) != 0
                       || alignment.IsEvil() && (descriptor & SpellDescriptor.GOOD) != 0
                       || alignmentChoice == 2 && (descriptor & SpellDescriptor.GOOD) != 0;
            }

            return false;
        }

        [TempleDllLocation(0x10077910)]
        public string GetSpellDescription(int spellEnum)
        {
            return _spellMes[5000 + spellEnum];
        }

        [TempleDllLocation(0x10078e00)]
        private void GarbageCollectSpells()
        {
            // We need to buffer the IDs we want to delete to avoid concurrent modifications of the dictionary
            Span<int> idsToDelete = stackalloc int[_activeSpells.Count];
            var spellsToDeleteCount = 0;

            foreach (var (spellId, activeSpell) in _activeSpells)
            {
                if (IsSpellGarbageCollectable(activeSpell))
                {
                    idsToDelete[spellsToDeleteCount++] = spellId;
                }
            }

            for (var i = 0; i < spellsToDeleteCount; i++)
            {
                var spellId = idsToDelete[i];
                _activeSpells.Remove(spellId);
            }
            Logger.Info("Garbage collected {0} inactive spells.", spellsToDeleteCount);

            _activeSpells.TrimExcess();
        }

        private bool IsSpellGarbageCollectable(ActiveSpell activeSpell)
        {
            if (activeSpell.Body.caster == null)
            {
                Logger.Info("Pruning spell {0} ({1}) because it has no caster.",
                    activeSpell.Body.spellId,
                    GetSpellName(activeSpell.Body.spellEnum));
                return true;
            }

            // TODO: Vanilla was also pruning spells with targetCount==0 and no targets, which is weird...

            return !activeSpell.IsActive;
        }

        [TempleDllLocation(0x10079220)]
        public void SaveGame(SavedGameState savedGameState)
        {
            GarbageCollectSpells();

            savedGameState.SpellState = new SavedSpellState
            {
                SpellIdSerial = spellIdSerial,
                ActiveSpells = _activeSpells.ToDictionary(
                    kvp => kvp.Key,
                    kvp => ActiveSpellSaver.SaveActiveSpell(kvp.Value.Body, kvp.Value.IsActive)
                )
            };
        }

        [TempleDllLocation(0x100792a0)]
        public void LoadGame(SavedGameState savedGameState)
        {
            var spellState = savedGameState.SpellState;

            spellIdSerial = spellState.SpellIdSerial;

            _activeSpells.Clear(); // Vanilla relied on reset() first

            foreach (var savedActiveSpell in spellState.ActiveSpells.Values)
            {
                var spellPacket = ActiveSpellLoader.LoadActiveSpell(savedActiveSpell);
                _activeSpells[savedActiveSpell.Id] = new ActiveSpell
                {
                    IsActive = savedActiveSpell.IsActive,
                    Body = spellPacket
                };
            }
        }
    }
}