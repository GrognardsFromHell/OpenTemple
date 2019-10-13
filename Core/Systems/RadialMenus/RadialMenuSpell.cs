using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RadialMenus
{
    public partial class RadialMenuSystem
    {
        [TempleDllLocation(0x100f1470)]
        [TemplePlusLocation("radialmenu.cpp")]
        private void AddSpell(GameObjectBody caster, SpellStoreData spData)
        {
            if (spData.spellStoreState.usedUp)
            {
                return;
            }

            if (!GameSystems.Spell.spellCanCast(caster, spData.spellEnum, spData.classCode, spData.spellLevel))
            {
                return;
            }

            if (spData.classCode == GameSystems.Spell.GetSpellClass(Stat.level_paladin) &&
                GameSystems.D20.D20Query(caster, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                return;
            }

            if (!TryGetParentNodeForSpell(caster, spData.classCode, spData.spellLevel, out var specNode))
            {
                return;
            }

            RadialMenuEntry entry;
            if (!GameSystems.Spell.SpellHasMultiSelection(spData.spellEnum))
            {
                var d20SpellData = new D20SpellData(spData.spellEnum, spData.classCode, spData.spellLevel, -1,
                    spData.metaMagicData);
                entry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.CAST_SPELL);
                entry.d20ActionData1 = 0; // TODO: Check the CAST_SPELL action whether this is needed

                var nodeIdx = GameSystems.D20.RadialMenu.AddToStandardNode(caster, ref entry, specNode);
                SetSpontaneousCastingAltNode(caster, nodeIdx, spData);
                return;
            }

            // Multiselection spells section
            entry = RadialMenuEntry.CreateParent(GameSystems.Spell.GetSpellName(spData.spellEnum));

            // the parent node
            var parentNodeIdx = AddParentChildNode(caster, ref entry, GetStandardNode(specNode));
            SetSpontaneousCastingAltNode(caster, parentNodeIdx, spData);

            // the options
            if (!GameSystems.Spell.TryGetMultiSelectOptions(spData.spellEnum, out var multiOptions))
            {
                Logger.Error("Spell multiselect options not found!");
                return;
            }

            // populate options
            for (var i = 0; i < multiOptions.Count; i++)
            {
                var op = multiOptions[i];

                var d20SpellData = new D20SpellData(spData.spellEnum, spData.classCode, spData.spellLevel, -1, spData.metaMagicData);
                var castEntry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.CAST_SPELL);
                castEntry.d20ActionData1 = 0; // TODO: Check if this is really necessary or if it's okay what CreateSpellAction does
                SetCallbackCopyEntryToSelected(ref castEntry);

                if (op.isProto)
                {
                    var protoId = op.value;
                    entry.minArg = protoId;

                    var protoObj = GameSystems.Proto.GetProtoById(protoId);
                    entry.text = GameSystems.Description.Get(protoObj.GetInt32(obj_f.description));
                }
                else
                {
                    entry.text = GameSystems.Spell.GetSpellsRadialMenuOptions(op.value);
                    entry.minArg = i + 1;
                }

                entry.AddAsChild(caster, parentNodeIdx);
            }
        }

        private static bool TryGetParentNodeForSpell(GameObjectBody caster,
            int spellClass,
            int spellLevel,
            out RadialMenuStandardNode parentNode)
        {
            if (spellLevel >= NUM_SPELL_LEVELS)
            {
                parentNode = default;
                return false;
            }

            List<int> spellClasses = new List<int>();
            foreach (var classEnum in D20ClassSystem.ClassesWithSpellLists)
            {
                if (caster.GetStat(classEnum) > 0)
                {
                    spellClasses.Add(GameSystems.Spell.GetSpellClass(classEnum));
                }
            }

            // domain spells go last
            if (GameSystems.Spell.IsDomainSpell(spellClass))
            {
                parentNode = RadialMenuStandardNode.SpellsDomain + 1
                                                                 + spellClasses.Count * NUM_SPELL_LEVELS
                                                                 + spellLevel;
                return true;
            }

            for (var i = 0; i < spellClasses.Count; i++)
            {
                if (spellClasses[i] == spellClass)
                {
                    parentNode = RadialMenuStandardNode.SpellsDomain
                                 + 1
                                 + i * NUM_SPELL_LEVELS
                                 + spellLevel;
                    return true;
                }
            }

            Logger.Warn("No matching class found for memorized/known spell with class {0} and level {1}???",
                spellClass, spellLevel);
            parentNode = default;
            return false;
        }

        [TempleDllLocation(0x100f1010)]
        [TemplePlusLocation("spell.cpp")]
        private void SetSpontaneousCastingAltNode(GameObjectBody obj, int nodeIdx, SpellStoreData spellData)
        {
            var spellClassCode = spellData.classCode;
            if (GameSystems.Spell.IsDomainSpell(spellClassCode))
            {
                // Domain slots cannot be used for spontaneous casting
                return;
            }

            var castingClassCode = GameSystems.Spell.GetCastingClass(spellClassCode);
            if (castingClassCode == Stat.level_cleric)
            {
                var alignmentChoice = obj.GetInt32(obj_f.critter_alignment_choice);
                var spellLevel = spellData.spellLevel;

                var type = alignmentChoice == 1 ? SpontCastType.GoodCleric : SpontCastType.EvilCleric;
                if (!SpontaneousCastSpells.TryGet(type, spellLevel, out var spontSpellEnum))
                {
                    return;
                }

                // NOTE: This clears metamagic (deliberately)
                var d20SpellData = new D20SpellData(spontSpellEnum, spellData.classCode, spellData.spellLevel);

                var radEntry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.CAST_SPELL);
                radEntry.d20ActionData1 = 0;
                radEntry.helpSystemHashkey = "TAG_CLASS_FEATURES_CLERIC_SPONTANEOUS_CASTING";

                SetMorphsTo(obj, nodeIdx, AddRootNode(obj, ref radEntry));
            }
            else if (castingClassCode == Stat.level_druid)
            {
                var spellLevel = spellData.spellLevel;
                if (!SpontaneousCastSpells.TryGet(SpontCastType.Druid, spellLevel, out var spontSpellEnum)
                    || !SpontaneousCastSpells.TryGetDruidOptionsKey(spellLevel, out var optionsKey))
                {
                    return;
                }

                var radEntry = RadialMenuEntry.CreateParent(GameSystems.Spell.GetSpellName(spontSpellEnum));
                var parentIdx = AddRootNode(obj, ref radEntry);
                SetMorphsTo(obj, nodeIdx, parentIdx);

                // Now add sub-entries for each summonable creature
                // TODO: Move this functionality into SpellSystem, since it's used in multiple places
                var numSummonsTxt = GameSystems.Spell.GetSpellsRadialMenuOptions(optionsKey);
                var numSummons = int.Parse(numSummonsTxt);
                for (int i = 1; i <= numSummons; i++)
                {
                    var protoNumTxt = GameSystems.Spell.GetSpellsRadialMenuOptions(i + optionsKey);
                    var protoNum = int.Parse(protoNumTxt);
                    var protoHandle = GameSystems.Proto.GetProtoById(protoNum);

                    var summonSpellData = new D20SpellData(spontSpellEnum, spellData.classCode, spellLevel);
                    summonSpellData.spontCastType = SpontCastType.Druid;

                    var summonRadEntry = RadialMenuEntry.CreateSpellAction(summonSpellData, D20ActionType.CAST_SPELL);
                    summonRadEntry.text = GameSystems.Description.Get(protoHandle.GetInt32(obj_f.description));
                    summonRadEntry.d20ActionData1 = 0; // TODO Check if restting is necessary vs. CreateSpellAction
                    SetCallbackCopyEntryToSelected(ref summonRadEntry);
                    summonRadEntry.minArg = protoNum;
                    summonRadEntry.AddAsChild(obj, parentIdx);
                }
            }
        }
    }
}