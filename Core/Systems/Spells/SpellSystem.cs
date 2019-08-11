using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.InGameSelect;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.Spells
{
    public class SpellSystem : IGameSystem, IResetAwareSystem
    {
        public const int MAX_SPELLS_KNOWN = 384;

        public const int SPELL_ENUM_MAX_VANILLA = 802;
        public const int SPELL_ENUM_MAX_EXPANDED = 3999;

        public const int NORMAL_SPELL_RANGE = 600; // vanilla normal spells are up to here
        public const int SPELL_LIKE_ABILITY_RANGE = 699; // Monster spell-like abilities are up to here

        public const int
            CLASS_SPELL_LIKE_ABILITY_START = 3000; // new in Temple+ - this is the range used for class spells

        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10AAF428)]
        private Dictionary<int, SpellEntry> _spells = new Dictionary<int, SpellEntry>();

        [TempleDllLocation(0x10AAF218)]
        private Dictionary<int, ActiveSpell> _activeSpells = new Dictionary<int, ActiveSpell>();

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

        [TempleDllLocation(0x1007b740)]
        public SpellSystem()
        {
            // TODO SpellInitSpellEntries("rules\\spells")

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
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x100750f0)]
        public void Reset()
        {
            throw new NotImplementedException();
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
        public string GetSpellName(in int spellId)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10077970)]
        public string GetSpellHelpTopic(int spellId)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100756e0)]
        public bool TryGetActiveSpell(int activeSpellId, out SpellPacketBody spellPacketBody)
        {
            if (_activeSpells.TryGetValue(activeSpellId, out var spellPacket))
            {
                spellPacketBody = spellPacket.Body;
            }

            throw new ArgumentOutOfRangeException("Unknown spell id: " + activeSpellId);
        }

        public SpellPacketBody GetActiveSpell(int activeSpellId)
        {
            return _activeSpells[activeSpellId].Body;
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

            var spellArray = obj.GetSpellArray(spellListField);
            obj.SetSpell(spellListField, spellArray.Count, spData);
        }

        // TODO: Clean up arguments (use enums if possible)
        [TempleDllLocation(0x10075a10)]
        public void SpellMemorizeAdd(GameObjectBody obj, int spellId, int classCode, int spellLevel, int spellStoreData,
            uint metaMagicData)
        {
            var spData = new SpellStoreData(spellId, spellLevel, classCode, metaMagicData, spellStoreData);
            // TODO *(int*)&spData.spellStoreState = spellStoreData;
            // TODO *(int*)&spData.metaMagicData = metaMagicData;

            // Ensure it is flagged as SpellStoreMemorized if not specified
            if (spData.spellStoreState.spellStoreType == SpellStoreType.spellStoreNone)
            {
                spData.spellStoreState.spellStoreType |= SpellStoreType.spellStoreMemorized;
            }

            var size = obj.GetSpellArray(obj_f.critter_spells_memorized_idx).Count;
            obj.SetSpell(obj_f.critter_spells_memorized_idx, size, spData);
        }

        [TempleDllLocation(0x100766e0)]
        public void ObjOnSpellBeginRound(GameObjectBody obj)
        {
            var spellIds = _activeSpells.Keys.ToImmutableSortedSet();

            foreach (var spellId in spellIds)
            {
                var spellPkt = _activeSpells[spellId];
                for (var i = 0; i < spellPkt.Body.targetCount; i++)
                {
                    if (spellPkt.Body.targetListHandles[i] == obj)
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
        public void IdentifySpellCast(int spellId)
        {
            // TODO: This is replaced by TemplePlus (!)
            throw new NotImplementedException();
        }

        public IReadOnlyList<SpellEntryLevelSpec> GetSpellListExtension(int spellEnum)
        {
            Stub.TODO();
            return ImmutableList<SpellEntryLevelSpec>.Empty;
        }

        [TempleDllLocation(0x100754b0)]
        public SpellEntry GetSpellEntry(int spellEnum)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100754b0)]
        public bool TryGetSpellEntry(int spellEnum, out SpellEntry spellEntry)
        {
            throw new NotImplementedException();
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
                case SpontCastType.spontCastGoodCleric:
                    return SpontCastSpellsGoodCleric[spellSlotLevel];
                case SpontCastType.spontCastEvilCleric:
                    return SpontCastSpellsEvilCleric[spellSlotLevel];
                case SpontCastType.spontCastDruid:
                    return SpontCastSpellsDruid[spellSlotLevel];
                default:
                    throw new ArgumentOutOfRangeException(nameof(spontCastType));
            }
        }

        public int GetSpellClass(Stat classEnum, bool isDomain = false)
        {
            if (isDomain)
            {
                return (int) classEnum;
            }

            return 0x80 | (int) classEnum;
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

        [TempleDllLocation(0x100fdf00)]
        public bool IsForbiddenSchool(GameObjectBody critter, int spSchool)
        {
            var schoolData = critter.GetInt32(obj_f.critter_school_specialization);
            var forbSch1 = (schoolData & (0xFF00)) >> 8;
            var forbSch2 = (schoolData & (0xFF0000)) >> 16;
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
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10075da0)]
        public bool HashMatchingClassForSpell(GameObjectBody caster, int spellEnum)
        {
            var spEntry = GameSystems.Spell.GetSpellEntry(spellEnum);
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
        public int GetAnimIdWand(int spellSchool)
        {
            return WandAnimIds[spellSchool];
        }

        [TempleDllLocation(0x100757B0)]
        public int GetSpellSchoolAnimId(int spellSchool)
        {
            return SpellSchoolAnimIds[spellSchool];
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
                GameSystems.RollHistory.RollHistoryType4Add(caster, dc, historyText, dice, rollResult, bonusList);
            return rollResult + bonusList.OverallBonus - dc;
        }

        [TempleDllLocation(0x10077670)]
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

                return (spEntry.aiTypeBitmask & (1 << (int) AiSpellType.ai_action_offensive)) != default;
            }


            if (caster != null)
            {
                if (target != null)
                {
                    switch (spellEnum)
                    {
                        case 1:
                        case 6:
                        case 7:
                        case 19:
                        case 27:
                        case 37:
                        case 39:
                        case 41:
                        case 42:
                        case 44:
                        case 47:
                        case 48:
                        case 49:
                        case 62:
                        case 74:
                        case 94:
                        case 97:
                        case 98:
                        case 101:
                        case 104:
                        case 110:
                        case 111:
                        case 113:
                        case 114:
                        case 117:
                        case 120:
                        case 123:
                        case 125:
                        case 129:
                        case 130:
                        case 132:
                        case 133:
                        case 134:
                        case 137:
                        case 138:
                        case 147:
                        case 148:
                        case 149:
                        case 152:
                        case 155:
                        case 159:
                        case 169:
                        case 173:
                        case 188:
                        case 189:
                        case 195:
                        case 199:
                        case 204:
                        case 205:
                        case 213:
                        case 219:
                        case 229:
                        case 238:
                        case 244:
                        case 253:
                        case 255:
                        case 256:
                        case 257:
                        case 261:
                        case 272:
                        case 280:
                        case 282:
                        case 283:
                        case 284:
                        case 285:
                        case 286:
                        case 290:
                        case 291:
                        case 292:
                        case 303:
                        case 311:
                        case 315:
                        case 320:
                        case 326:
                        case 327:
                        case 334:
                        case 337:
                        case 359:
                        case 367:
                        case 368:
                        case 369:
                        case 370:
                        case 371:
                        case 372:
                        case 374:
                        case 379:
                        case 385:
                        case 390:
                        case 391:
                        case 392:
                        case 393:
                        case 394:
                        case 399:
                        case 400:
                        case 401:
                        case 402:
                        case 404:
                        case 407:
                        case 414:
                        case 426:
                        case 427:
                        case 430:
                        case 451:
                        case 457:
                        case 462:
                        case 467:
                        case 468:
                        case 469:
                        case 470:
                        case 471:
                        case 472:
                        case 473:
                        case 474:
                        case 475:
                        case 476:
                        case 477:
                        case 478:
                        case 479:
                        case 480:
                        case 481:
                        case 482:
                        case 483:
                        case 484:
                        case 485:
                        case 492:
                        case 505:
                        case 508:
                        case 509:
                        case 515:
                        case 519:
                        case 536:
                        case 543:
                        case 544:
                        case 545:
                        case 546:
                        case 547:
                        case 548:
                        case 549:
                        case 550:
                        case 552:
                        case 553:
                        case 554:
                        case 557:
                        case 558:
                        case 564:
                        case 565:
                        case 567:
                        case 700:
                        case 701:
                        case 702:
                        case 703:
                        case 704:
                        case 705:
                        case 706:
                        case 707:
                        case 708:
                        case 709:
                        case 710:
                        case 711:
                        case 714:
                        case 715:
                        case 716:
                        case 717:
                        case 718:
                        case 719:
                        case 720:
                        case 721:
                        case 722:
                        case 723:
                        case 724:
                        case 725:
                        case 726:
                        case 727:
                        case 728:
                        case 729:
                            return false;
                        case 89:
                        case 90:
                        case 91:
                        case 92:
                        case 93:
                            return GameSystems.Critter.IsUndead(target);
                        case 246:
                        case 247:
                        case 248:
                        case 249:
                        case 250:
                            return !GameSystems.Critter.IsUndead(target);
                        default:
                            return !GameSystems.Critter.IsFriendly(caster, target);
                        case 10:
                        case 11:
                        case 25:
                        case 28:
                        case 40:
                        case 43:
                        case 45:
                        case 46:
                        case 50:
                        case 51:
                        case 54:
                        case 55:
                        case 56:
                        case 57:
                        case 58:
                        case 59:
                        case 65:
                        case 66:
                        case 67:
                        case 72:
                        case 73:
                        case 76:
                        case 79:
                        case 96:
                        case 99:
                        case 100:
                        case 103:
                        case 107:
                        case 122:
                        case 139:
                        case 140:
                        case 141:
                        case 142:
                        case 151:
                        case 153:
                        case 163:
                        case 165:
                        case 167:
                        case 171:
                        case 176:
                        case 178:
                        case 179:
                        case 180:
                        case 183:
                        case 194:
                        case 196:
                        case 200:
                        case 201:
                        case 214:
                        case 217:
                        case 223:
                        case 226:
                        case 227:
                        case 228:
                        case 231:
                        case 235:
                        case 236:
                        case 237:
                        case 288:
                        case 304:
                        case 333:
                        case 335:
                        case 345:
                        case WellKnownSpells.ProduceFlame:
                        case 383:
                        case 384:
                        case 386:
                        case 396:
                        case 408:
                        case 412:
                        case 425:
                        case 431:
                        case 432:
                        case 434:
                        case 437:
                        case 438:
                        case 439:
                        case 440:
                        case 442:
                        case 443:
                        case 445:
                        case 455:
                        case 456:
                        case 460:
                        case 466:
                        case 490:
                        case 513:
                        case 531:
                        case 542:
                        case 551:
                        case 555:
                        case 556:
                        case 559:
                        case 560:
                        case 561:
                        case 562:
                        case 563:
                        case 566:
                        case 600:
                        case 601:
                        case 602:
                        case 712:
                        case 713:
                            return true;
                    }
                }

                return true;
            }

            if (spellEnum > 261)
            {
                if (spellEnum > 414)
                {
                    if (spellEnum > 515)
                    {
                        switch (spellEnum)
                        {
                            case 519:
                            case 536:
                            case 543:
                            case 544:
                            case 545:
                            case 546:
                            case 547:
                            case 548:
                            case 549:
                            case 550:
                            case 552:
                            case 553:
                            case 554:
                            case 557:
                            case 558:
                            case 564:
                            case 565:
                            case 567:
                            case 700:
                            case 701:
                            case 702:
                            case 703:
                            case 704:
                            case 705:
                            case 706:
                            case 707:
                            case 708:
                            case 709:
                            case 710:
                            case 711:
                            case 714:
                            case 715:
                            case 716:
                            case 717:
                            case 718:
                            case 719:
                            case 720:
                            case 721:
                            case 722:
                            case 723:
                            case 724:
                            case 725:
                            case 726:
                            case 727:
                            case 728:
                            case 729:
                                return false;
                            default:
                                return true;
                        }

                        return true;
                    }

                    if (spellEnum != 515)
                    {
                        switch (spellEnum)
                        {
                            case 426:
                            case 427:
                            case 430:
                            case 451:
                            case 457:
                            case 462:
                            case 467:
                            case 468:
                            case 469:
                            case 470:
                            case 471:
                            case 472:
                            case 473:
                            case 474:
                            case 475:
                            case 476:
                            case 477:
                            case 478:
                            case 479:
                            case 480:
                            case 481:
                            case 482:
                            case 483:
                            case 484:
                            case 485:
                            case 492:
                            case 505:
                            case 508:
                            case 509:
                                return false;
                            default:
                                return true;
                        }

                        return true;
                    }
                }
                else if (spellEnum != 414)
                {
                    switch (spellEnum)
                    {
                        case 272:
                        case 280:
                        case 282:
                        case 283:
                        case 284:
                        case 285:
                        case 286:
                        case 290:
                        case 291:
                        case 292:
                        case 303:
                        case 311:
                        case 315:
                        case 320:
                        case 326:
                        case 327:
                        case 334:
                        case 337:
                        case 359:
                        case 367:
                        case 368:
                        case 369:
                        case 370:
                        case 371:
                        case 372:
                        case 374:
                        case 379:
                        case 385:
                        case 390:
                        case 391:
                        case 392:
                        case 393:
                        case 394:
                        case 399:
                        case 400:
                        case 401:
                        case 402:
                        case 404:
                        case 407:
                            return false;
                        default:
                            return true;
                    }

                    return true;
                }

                return false;
            }

            if (spellEnum == 261)
            {
                return false;
            }

            if (spellEnum > 125)
            {
                switch (spellEnum)
                {
                    case 129:
                    case 130:
                    case 132:
                    case 133:
                    case 134:
                    case 137:
                    case 138:
                    case 147:
                    case 148:
                    case 149:
                    case 152:
                    case 155:
                    case 159:
                    case 169:
                    case 173:
                    case 188:
                    case 189:
                    case 195:
                    case 199:
                    case 204:
                    case 205:
                    case 213:
                    case 219:
                    case 229:
                    case 238:
                    case 244:
                    case 253:
                    case 255:
                    case 256:
                    case 257:
                        return false;
                    default:
                        return true;
                }

                return true;
            }

            if (spellEnum == 125)
            {
                return false;
            }

            switch (spellEnum)
            {
                case 1:
                case 6:
                case 7:
                case 19:
                case 27:
                case 37:
                case 39:
                case 41:
                case 42:
                case 44:
                case 47:
                case 48:
                case 49:
                case 62:
                case 74:
                case 89:
                case 90:
                case 91:
                case 92:
                case 93:
                case 94:
                case 97:
                case 98:
                case 101:
                case 104:
                case 110:
                case 111:
                case 113:
                case 114:
                case 117:
                case 120:
                case 123:
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

            if (GameSystems.Spell.IsLabel(spEnum))
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

            if (GameSystems.D20.D20Query(caster, D20DispatcherKey.QUE_CannotCast) )
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
                if (spellData.spellEnum == spellEnum && (spellData.spellStoreState.usedUp & 1) == 0)
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
                var casterClass = GameSystems.Spell.GetCastingClass(spellClassCode);
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
            args.modeTarget = (UiPickerType) spEntry.modeTargetSemiBitmask;
            args.incFlags = (UiPickerIncFlags) spEntry.incFlagsTargetBitmask;
            args.excFlags = (UiPickerIncFlags) spEntry.excFlagsTargetBitmask;
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
        public int GetSpellSchoolEnum(int spellEnum)
        {
            return GetSpellEntry(spellEnum).spellSchoolEnum;
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
            return spellPkt.targetCount > 0;
        }

        [TempleDllLocation(0x100B9690)]
        public void ConfigSpellTargetting(PickerArgs args, SpellPacketBody spPkt)
        {
            var flags = args.result.flags;

            if ((flags & PickerResultFlags.PRF_HAS_SINGLE_OBJ) != default)
            {
                var target = args.result.handle;
                Trace.Assert(target != null);

                spPkt.orgTargetCount = 1;
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

                    spPkt.targetListHandles = new GameObjectBody[targetCount];
                    Array.Fill(spPkt.targetListHandles, target);
                }
                else
                {
                    spPkt.targetListHandles = new[] {target};
                }
            }
            else
            {
                spPkt.targetListHandles = Array.Empty<GameObjectBody>();
                spPkt.orgTargetCount = 0;
            }

            if ((flags & PickerResultFlags.PRF_HAS_MULTI_OBJ) != default)
            {
                spPkt.targetListHandles = args.result.objList.ToArray();
                Trace.Assert(spPkt.targetListHandles.Length > 0);

                // else apply the rest of the targeting to the last object
                if (args.IsBaseModeTarget(UiPickerType.Multi) && !args.IsModeTargetFlagSet(UiPickerType.OnceMulti)
                                                              && args.result.objList.Count < args.maxTargets)
                {
                    var currentTargetCount = spPkt.targetListHandles.Length;
                    var replicateTarget = spPkt.targetListHandles[^1];
                    Array.Resize(ref spPkt.targetListHandles, args.maxTargets);
                    for (var i = currentTargetCount; i < args.maxTargets; i++)
                    {
                        spPkt.targetListHandles[i] = replicateTarget;
                    }
                }

                spPkt.orgTargetCount = spPkt.targetCount;
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
        public string GetSchoolOfMagicName(int school)
        {
            return _schoolsOfMagicNames[school];
        }

        [TempleDllLocation(0x100c3220)]
        public void PlayFizzle(GameObjectBody caster)
        {
            GameSystems.ParticleSys.CreateAtObj("Fizzle", caster);
            GameSystems.SoundGame.PositionalSound(17122, caster);
        }

        [TempleDllLocation(0x100794f0)]
        public int GetSpellComponentRegardMetamagic(int spellEnum, MetaMagicData metaMagicData)
        {
            if (!TryGetSpellEntry(spellEnum, out var spEntry))
            {
                Logger.Info("Could not find spell {0}.", spellEnum);
                return 0;
            }

            var result = spEntry.spellComponentBitmask;
            if (metaMagicData.IsSilent)
            {
                result &= ~1;
            }
            if (metaMagicData.IsStill)
            {
                result &= ~2;
            }
            return result;
        }

    }
}