using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Spells
{
    class Spell
    {
    }

    public class SpellSystem : IGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10AAF428)]
        private Dictionary<int, Spell> _spells = new Dictionary<int, Spell>();

        [TempleDllLocation(0x10AAF218)]
        private Dictionary<int, SpellPacketBody> _activeSpells = new Dictionary<int, SpellPacketBody>();

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

        private readonly Dictionary<int, string> _skillUiMes;
        private readonly Dictionary<int, string> _spellDurationMes;
        private readonly Dictionary<int, string> _spellMes;
        private readonly Dictionary<int, string> _spellEnumMes;

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
            return _activeSpells.TryGetValue(activeSpellId, out spellPacketBody);
        }

        public SpellPacketBody GetActiveSpell(int activeSpellId)
        {
            return _activeSpells[activeSpellId];
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
            Stub.TODO();
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
	if (isDomainSpell(spellClass)) {
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

    }
}