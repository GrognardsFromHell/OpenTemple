using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Utils;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Particles.Instances;

namespace OpenTemple.Core.Systems.D20.Conditions
{
    public static class CommonConditionCallbacks
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public static IReadOnlyList<ConditionSpec> Conditions { get; } = new List<ConditionSpec>
        {
        };

        [DispTypes(DispatcherType.D20Signal, DispatcherType.SkillLevel, DispatcherType.D20Query,
            DispatcherType.Initiative, DispatcherType.DealingDamage, DispatcherType.NewDay, DispatcherType.BeginRound)]
        [TempleDllLocation(0x100ed030)]
        [TemplePlusLocation("condition.cpp:392")]
        public static void conditionRemoveCallback(in DispatcherCallbackArgs evt)
        {
            evt.RemoveThisCondition();
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100c6fa0)]
        public static void BreakFreeRadial(in DispatcherCallbackArgs evt, int data)
        {
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Is_BreakFree_Possible))
            {
                var radMenuEntry = RadialMenuEntry.CreateAction(5061, D20ActionType.BREAK_FREE,
                    evt.GetConditionArg1(), "TAG_RADIAL_MENU_BREAK_FREE");
                radMenuEntry.spellIdMaybe = evt.GetConditionArg1();
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Movement);
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ecf60)]
        [TemplePlusLocation("condition.cpp:399")]
        public static void CondPreventSameArg(in DispatcherCallbackArgs args, ConditionSpec data)
        {
            var condArg1 = args.GetConditionArg1();
            var dispIo = args.GetDispIoCondStruct();
            if (dispIo.condStruct == data && dispIo.arg1 == condArg1)
            {
                dispIo.outputFlag = false;
            }
        }

        /// <summary>
        /// ID for the negative levels imbued by wearing an item with opposed alignment.
        /// </summary>
        private const int ItemNegativeLevelBonusId = 273;

        /// <summary>
        /// Adds a negative level bonus of -1 and considers the item's alignment if the bonus is caused by
        /// an aligned weapon or such.
        /// </summary>
        private static void AddNegativeLevelBonus(in DispatcherCallbackArgs evt,
            ref BonusList bonusList, int bonusValue, int bonusDescriptionId, int alignmentMask)
        {
            var critter = evt.objHndCaller;

            // Denotes Holy/Lawful/Anarchic/Evil weapon malluses
            if (bonusDescriptionId == ItemNegativeLevelBonusId)
            {
                if ((critter.GetBaseStat(Stat.alignment) & alignmentMask) == alignmentMask)
                {
                    var itemInvIdx = evt.GetConditionArg3();
                    var item = GameSystems.Item.GetItemAtInvIdx(critter, itemInvIdx);
                    var itemName = GameSystems.MapObject.GetDisplayName(item, critter);
                    bonusList.AddBonus(-1, 0, bonusDescriptionId, itemName);
                }
            }
            else
            {
                bonusList.AddBonus(-1, 0, bonusDescriptionId);
            }
        }

        [DispTypes(DispatcherType.GetLevel)]
        [TempleDllLocation(0x100ef8b0)]
        public static void NegativeLevel(in DispatcherCallbackArgs evt, int bonusDescriptionId, int itemAlignmentMask)
        {
            var classCode = evt.dispKey - D20DispatcherKey.CL_Level;

            // TODO Why is it always added to monks.... what???
            if (classCode == evt.GetConditionArg1() || classCode == 6)
            {
                var dispIo = evt.GetDispIoObjBonus();
                AddNegativeLevelBonus(in evt, ref dispIo.bonlist, -1, bonusDescriptionId, itemAlignmentMask);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ef780)]
        public static void NegativeLevelToHitBonus(in DispatcherCallbackArgs evt, int bonusDescriptionId,
            int itemAlignmentMask)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            AddNegativeLevelBonus(in evt, ref dispIo.bonlist, -1, bonusDescriptionId, itemAlignmentMask);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ed0e0)]
        public static void D20Query_Callback_GetSDDKey1(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = evt.GetConditionArg(data);
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay, DispatcherType.BeginRound)]
        [TempleDllLocation(0x100ed110)]
        public static void CondNodeSetArg0FromSubDispDef(in DispatcherCallbackArgs evt, int data)
        {
            evt.SetConditionArg1(data);
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay, DispatcherType.BeginRound,
            DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x100ed130)]
        public static void CondNodeSetArg1FromSubDispDef(in DispatcherCallbackArgs evt, int data)
        {
            evt.SetConditionArg2(data);
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100ed150)]
        public static void TooltipNoRepetitionCallback(in DispatcherCallbackArgs evt, int combatMesLine, int data2)
        {
            var dispIo = evt.GetDispIoTooltip();
            dispIo.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesLine));
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c7180)]
        public static void QuerySetReturnVal1(in DispatcherCallbackArgs evt)
        {
            evt.GetDispIoD20Query().return_val = 1;
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ef9d0)]
        public static void D20SignalPackHandler(in DispatcherCallbackArgs evt, int data)
        {
            throw new NotImplementedException();
        }

        private static Guid UnpackGuid(ReadOnlySpan<int> packedInts)
        {
            var packedBytes = MemoryMarshal.Cast<int, byte>(packedInts);
            Span<byte> unpackedBytes = stackalloc byte[16]
            {
                // The 32-bit uint is unswiveled
                packedBytes[0],
                packedBytes[1],
                packedBytes[2],
                packedBytes[3],
                // The next two shorts are swapped
                packedBytes[6],
                packedBytes[7],
                packedBytes[4],
                packedBytes[5],
                // Now it get's really wild, the next two 4byte pairs are flipped
                packedBytes[11],
                packedBytes[10],
                packedBytes[9],
                packedBytes[8],
                packedBytes[15],
                packedBytes[14],
                packedBytes[13],
                packedBytes[12]
            };

            return new Guid(unpackedBytes);
        }

        /// <summary>
        /// Converts a stored object id back into an object handle when loading condition data
        /// from hydrated object properties.
        /// </summary>
        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100efb20)]
        public static void D20SignalUnpackHandler(in DispatcherCallbackArgs evt, int data)
        {
            Span<int> packedInts = stackalloc int[4];
            packedInts[0] = evt.GetConditionArg(data);
            packedInts[1] = evt.GetConditionArg(data + 1);
            packedInts[2] = evt.GetConditionArg(data + 2);
            packedInts[3] = evt.GetConditionArg(data + 3);

            if (packedInts[0] != 0 || packedInts[1] != 0 || packedInts[2] != 0 || packedInts[3] != 0)
            {
                // Create the GUID and a corresponding ObjectId
                var guid = UnpackGuid(packedInts);
                var objectId = ObjectId.CreatePermanent(guid);

                var obj = GameSystems.Object.GetObject(objectId);
                if (obj == null)
                {
                    Logger.Error("Failed to find object with id {0} stored in condition {1} attached to {2}",
                        objectId, evt.subDispNode.condNode.condStruct.condName, evt.objHndCaller);
                }

                evt.SetConditionObjArg(data, obj);
            }
            else
            {
                evt.SetConditionObjArg(data, null);
            }
        }

        [DispTypes(DispatcherType.EffectTooltip)]
        [TempleDllLocation(0x100edf10)]
        public static void EffectTooltipGeneral(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            dispIo.bdb.AddEntry(data);
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100ed2f0)]
        public static void AcBonusCapper(in DispatcherCallbackArgs evt, int bonusDescriptionId)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddCap(8, 0, bonusDescriptionId);
            dispIo.bonlist.AddCap(3, 0, bonusDescriptionId);
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100ef640)]
        public static void NegativeLevelSkillPenalty(in DispatcherCallbackArgs evt, int bonusDescriptionId,
            int itemAlignmentMask)
        {
            var dispIo = evt.GetDispIoObjBonus();
            AddNegativeLevelBonus(in evt, ref dispIo.bonlist, -1, bonusDescriptionId, itemAlignmentMask);
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ecfa0)]
        [TemplePlusLocation("condition.cpp:400")]
        public static void CondOverrideBy(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                evt.RemoveThisCondition();
            }
        }

        [DispTypes(DispatcherType.D20Signal, DispatcherType.ConditionRemove)]
        [TempleDllLocation(0x100ed3a0)]
        public static void EndParticlesFromArg(in DispatcherCallbackArgs evt, int data)
        {
            var particleSystem = evt.GetConditionPartSysArg(data);
            if (particleSystem != null)
            {
                GameSystems.ParticleSys.End(particleSystem);
                evt.SetConditionPartSysArg(data, null);
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100ed290)]
        public static void ConditionDurationTicker(in DispatcherCallbackArgs evt, int remainingTimeArgIdx)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var remainingTime = evt.GetConditionArg(remainingTimeArgIdx) - dispIo.data1;
            if (remainingTime >= 0)
            {
                evt.SetConditionArg(remainingTimeArgIdx, remainingTime);
            }
            else
            {
                evt.RemoveThisCondition();
            }
        }

        [DispTypes(DispatcherType.MaxHP)]
        [TempleDllLocation(0x100ef820)]
        public static void NegativeLevelMaxHp(in DispatcherCallbackArgs evt, int bonusDescriptionId,
            int itemAlignmentMask)
        {
            var dispIo = evt.GetDispIoBonusList();
            AddNegativeLevelBonus(in evt, ref dispIo.bonlist, -5, bonusDescriptionId, itemAlignmentMask);
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100ed250)]
        public static void D20ActionCheckRemainingCharges(in DispatcherCallbackArgs evt, int requiredCharges)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var remainingCharges = evt.GetConditionArg1();
            if (remainingCharges <= requiredCharges)
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100efcf0)]
        public static void SightImpairmentSkillPenalty(in DispatcherCallbackArgs evt, int stat, int malusValue)
        {
            var dispIo = evt.GetDispIoObjBonus();

            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_True_Seeing))
            {
                var skillId = evt.GetSkillIdFromDispatcherKey();
                if (GameSystems.Skill.GetDecidingStat(skillId) == (Stat) stat)
                {
                    dispIo.bonOut.AddBonus(-malusValue, 0, 189);
                }
            }
        }

        private static bool IsMindAffectingSpell(SpellEntry spell)
        {
            if (spell.spellSubSchoolEnum == SubschoolOfMagic.Charm &&
                !spell.HasDescriptor(SpellDescriptor.MIND_AFFECTING))
            {
                // I wonder which spell is in the charm subschool, but does not have
                // the mind affecting descriptor ???
                Debugger.Break();
            }

            return spell.spellSubSchoolEnum == SubschoolOfMagic.Charm
                   || spell.HasDescriptor(SpellDescriptor.MIND_AFFECTING);
        }

        [DispTypes(DispatcherType.SpellImmunityCheck)]
        [TempleDllLocation(0x100ed650)]
        [TemplePlusLocation("spell_condition.cpp:269")]
        public static void ImmunityCheckHandler(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var v1 = evt.GetDispIoImmunity();
            if (v1.returnVal == 1)
            {
                return;
            }

            var dispIoTrigger = DispIoTypeImmunityTrigger.Default;
            dispIoTrigger.condNode = evt.subDispNode.condNode;

            switch (evt.objHndCaller.DispatchHasImmunityTrigger(dispIoTrigger))
            {
                case D20DispatcherKey.IMMUNITY_SPELL:
                    HandleSpellImmunity(evt);
                    return;
                case D20DispatcherKey.IMMUNITY_COURAGE:
                    GameSystems.Spell.TryGetSpellEntry(v1.spellPkt.spellEnum, out var protectionSpellEntry);
                    if (data1 == 0 && protectionSpellEntry.HasDescriptor(SpellDescriptor.FEAR))
                    {
                        v1.returnVal = 1;
                    }

                    if (v1.returnVal == 1)
                    {
                        var attackSpellName = GameSystems.Spell.GetSpellName(v1.spellPkt.spellEnum);
                        Logger.Info(
                            "d20_mods_global.c / D20MF_immunity_check_handler(): spell ({0}) cast by obj( {1} ) resisted by target because of immunity.( {2} )",
                            attackSpellName, v1.spellPkt.caster, evt.objHndCaller);
                        GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
                        var suffix = $" {GameSystems.Feat.GetFeatName((FeatId) data2)}";
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30019, TextFloaterColor.White,
                            suffix: suffix);
                    }

                    return;
                case D20DispatcherKey.IMMUNITY_RACIAL:
                    GameSystems.Spell.TryGetSpellEntry(v1.spellPkt.spellEnum, out var offendingSpell);
                    if (data1 == 1 && IsMindAffectingSpell(offendingSpell))
                    {
                        v1.returnVal = 1;
                    }
                    else if (data1 == 0 && IsMindAffectingSpell(offendingSpell)
                             || data1 == 0 && offendingSpell.savingThrowType == SpellSavingThrow.Fortitude)
                    {
                        v1.returnVal = 1;
                    }

                    if (v1.returnVal == 1)
                    {
                        var offendingSpellName = GameSystems.Spell.GetSpellName(v1.spellPkt.spellEnum);
                        Logger.Info(
                            "d20_mods_global.c / D20MF_immunity_check_handler(): spell ({0}) cast by obj( {1} ) resisted by target because of immunity.( {2} )",
                            offendingSpellName, v1.spellPkt.caster, evt.objHndCaller);
                        GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);

                        var suffix = " " + GameSystems.D20.BonusSystem.GetBonusDescription(319);
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30019, TextFloaterColor.White,
                            suffix: suffix);
                    }

                    return;
                case D20DispatcherKey.IMMUNITY_SPECIAL:
                    GameSystems.Spell.TryGetSpellEntry(v1.spellPkt.spellEnum, out offendingSpell);
                    switch (data1)
                    {
                        case 0:
                            if (IsMindAffectingSpell(offendingSpell))
                            {
                                v1.returnVal = 1;
                                break;
                            }

                            if (v1.spellPkt.spellEnum == WellKnownSpells.Sleep
                                || v1.spellPkt.spellEnum == WellKnownSpells.DeepSlumber
                                || (v1.spellPkt.spellClass & 0x7F) == 4
                                || offendingSpell.HasDescriptor(SpellDescriptor.DEATH)
                                || offendingSpell.spellSchoolEnum == SchoolOfMagic.Necromancy)
                            {
                                v1.returnVal = 1;
                                break;
                            }

                            if (offendingSpell.savingThrowType == SpellSavingThrow.Fortitude)
                            {
                                v1.returnVal = 1;
                            }

                            break;
                        case 1:
                            if (IsMindAffectingSpell(offendingSpell))
                            {
                                v1.returnVal = 1;
                                break;
                            }

                            if (v1.spellPkt.spellEnum == WellKnownSpells.Entangle)
                            {
                                v1.returnVal = 1;
                            }

                            break;
                        case 2:
                            if (v1.spellPkt.spellEnum == WellKnownSpells.Web)
                            {
                                v1.returnVal = 1;
                            }

                            break;
                        case 3:
                            if (v1.spellPkt.spellEnum == WellKnownSpells.Confusion)
                            {
                                v1.returnVal = 1;
                            }

                            break;
                        default:
                            break;
                    }

                    if (v1.returnVal == 1)
                    {
                        var offendingSpellName = GameSystems.Spell.GetSpellName(v1.spellPkt.spellEnum);
                        Logger.Info(
                            "d20_mods_global.c / D20MF_immunity_check_handler(): spell ({0}) cast by obj( {1} ) resisted by target because of immunity.( {2} )",
                            offendingSpellName, v1.spellPkt.caster, evt.objHndCaller);
                        GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);

                        var suffix = " " + GameSystems.D20.BonusSystem.GetBonusDescription(318);
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30019, TextFloaterColor.White,
                            suffix: suffix);
                    }

                    return;
                default:
                    return;
            }
        }


        private static void HandleSpellImmunity(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoImmunity();
            var protectionSpellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(protectionSpellId, out var protectionSpellPkt))
            {
                return;
            }

            GameSystems.Spell.TryGetSpellEntry(protectionSpellPkt.spellEnum, out var protectionSpellEntry);
            GameSystems.Spell.TryGetSpellEntry(dispIo.spellPkt.spellEnum, out var offendingSpell);
            var protectionSpellEnum = protectionSpellPkt.spellEnum;

            switch (protectionSpellPkt.spellEnum)
            {
                case WellKnownSpells.GreaterHeroism:
                    if (dispIo.flag == 1)
                    {
                        if (offendingSpell.HasDescriptor(SpellDescriptor.FEAR))
                        {
                            dispIo.returnVal = 1;
                        }
                    }

                    break;
                case WellKnownSpells.Shield:
                    if (dispIo.flag == 1 && dispIo.spellPkt.spellEnum == WellKnownSpells.MagicMissile)
                    {
                        dispIo.returnVal = 1;
                        break;
                    }

                    break;
                case WellKnownSpells.SpellResistance:
                    if (dispIo.flag == 1)
                    {
                        var bonlist = BonusList.Create();
                        var casterLevelSrBonus = dispIo.spellPkt.caster.Dispatch35CasterLevelModify(dispIo.spellPkt);
                        bonlist.AddBonus(casterLevelSrBonus, 0, 203);

                        if (GameSystems.Feat.HasFeat(dispIo.spellPkt.caster, FeatId.SPELL_PENETRATION))
                        {
                            bonlist.AddBonusFromFeat(2, 0, 114, FeatId.SPELL_PENETRATION);
                        }

                        if (GameSystems.Feat.HasFeat(dispIo.spellPkt.caster, FeatId.GREATER_SPELL_PENETRATION))
                        {
                            bonlist.AddBonusFromFeat(2, 0, 114, FeatId.GREATER_SPELL_PENETRATION);
                        }

                        var spellResistanceDc = evt.objHndCaller.Dispatch45SpellResistanceMod(protectionSpellEntry);
                        if (spellResistanceDc > 0)
                        {
                            if (GameSystems.Critter.IsFriendly(dispIo.spellPkt.caster, evt.objHndCaller)
                                && !GameSystems.Spell.IsSpellHarmful(dispIo.spellPkt.spellEnum,
                                    dispIo.spellPkt.caster, evt.objHndCaller))
                            {
                                spellResistanceDc = 0;
                            }


                            string prefixText, suffixText;
                            var rollTypeText = GameSystems.D20.Combat.GetCombatMesLine(5048); // "Spell Resistance"
                            if (GameSystems.Spell.DispelRoll(dispIo.spellPkt.caster, bonlist, 0, spellResistanceDc,
                                    rollTypeText,
                                    out var rollHistoryId) >= 0)
                            {
                                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30009,
                                    TextFloaterColor.Red);
                                prefixText = GameSystems.D20.Combat.GetCombatMesLine(121);
                                suffixText = GameSystems.D20.Combat.GetCombatMesLine(122);
                            }
                            else
                            {
                                dispIo.returnVal = 1;
                                prefixText = GameSystems.D20.Combat.GetCombatMesLine(0x77);
                                suffixText = GameSystems.D20.Combat.GetCombatMesLine(0x78);
                            }

                            var text = $"{prefixText}{rollHistoryId}{suffixText}\n\n";
                            GameSystems.RollHistory.CreateFromFreeText(text);
                        }
                    }

                    break;
                case WellKnownSpells.SpiritualWeapon:
                    if (dispIo.flag == 1)
                    {
                        if (dispIo.spellPkt.spellEnum != WellKnownSpells.DispelMagic)
                        {
                            dispIo.returnVal = 1;
                        }
                    }
                    else
                    {
                        dispIo.returnVal = 1;
                    }

                    break;
                case WellKnownSpells.LesserGlobeOfInvulnerability:
                    if (dispIo.flag == 1 && dispIo.spellPkt.spellKnownSlotLevel < 4)
                    {
                        dispIo.returnVal = 1;
                    }

                    break;
                case WellKnownSpells.DeathWard:
                    if (dispIo.flag == 1)
                    {
                        if ((protectionSpellPkt.spellClass & 0x80) != 0)
                        {
                            if (offendingSpell.HasDescriptor(SpellDescriptor.DEATH))
                            {
                                dispIo.returnVal = 1;
                            }
                        }
                        else if ((protectionSpellPkt.spellClass & 0x7F) == 4)
                        {
                            dispIo.returnVal = 1;
                        }
                    }

                    break;
                case WellKnownSpells.ProtectionFromEvil:
                case WellKnownSpells.ProtectionFromGood:
                case WellKnownSpells.ProtectionFromLaw:
                case WellKnownSpells.ProtectionFromChaos:
                    if (dispIo.flag == 1 && IsMindAffectingSpell(offendingSpell))
                    {
                        dispIo.returnVal = 1;
                    }

                    break;
                case WellKnownSpells.MagicCircleAgainstChaos:
                case WellKnownSpells.MagicCircleAgainstEvil:
                case WellKnownSpells.MagicCircleAgainstGood:
                case WellKnownSpells.MagicCircleAgainstLaw:
                    // TODO Why is this check needed??? And is it even correct?
                    if (evt.subDispNode.condNode.condStruct == SpellEffects.SpellMagicCircleOutward)
                    {
                        if (dispIo.flag == 1 && IsMindAffectingSpell(offendingSpell))
                        {
                            dispIo.returnVal = 1;
                        }
                    }

                    break;
                default:
                    break;
            }

            if (dispIo.flag == 1 && dispIo.returnVal == 1 &&
                protectionSpellEnum != WellKnownSpells.LesserGlobeOfInvulnerability)
            {
                var protectionSpellName = GameSystems.Spell.GetSpellName(protectionSpellEnum);
                var resistedSpellName = GameSystems.Spell.GetSpellName(offendingSpell.spellEnum);
                Logger.Info(
                    "d20_mods_global.c / D20MF_immunity_check_handler(): spell ({0}) cast by obj( {1} ) resisted by target( {2} ) via ( {3} )",
                    resistedSpellName, dispIo.spellPkt.caster, evt.objHndCaller, protectionSpellName);
                GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);

                var suffix = $" [{protectionSpellName}]";
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30019, TextFloaterColor.White, suffix: suffix);
                if (protectionSpellPkt.spellEnum != WellKnownSpells.SpellResistance)
                {
                    var targetName = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
                    var explanation = GameSystems.Spell.GetSpellName(30019); // Unaffected due to

                    var text = $"{targetName} {explanation} [{protectionSpellName}]";
                    GameSystems.RollHistory.CreateFromFreeText(text);
                }
            }
        }

        [DispTypes(DispatcherType.AbilityCheckModifier)]
        [TempleDllLocation(0x100f78d0)]
        public static void AbilityModCheckStabilityBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            const SkillCheckFlags mask = SkillCheckFlags.UnderDuress | SkillCheckFlags.Unk2;
            if ((dispIo.flags & mask) == mask)
            {
                dispIo.bonOut.AddBonus(4, 22, 317);
            }
        }

        [DispTypes(DispatcherType.ToHitBonusFromDefenderCondition)]
        [TempleDllLocation(0x100efca0)]
        public static void AddAttackerInvisibleBonus(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_True_Seeing))
            {
                dispIo.bonlist.AddBonus(data, 0, 161);
            }
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100e9150)]
        public static void TempHPTooltipCallback(in DispatcherCallbackArgs evt, int combatMesLineId)
        {
            var dispIo = evt.GetDispIoTooltip();
            var condArg3 = evt.GetConditionArg3();
            var line = GameSystems.D20.Combat.GetCombatMesLine(combatMesLineId);
            dispIo.Append($"{line}{condArg3}");
        }

        [DispTypes(DispatcherType.D20Signal, DispatcherType.GetMoveSpeed, DispatcherType.D20Query,
            DispatcherType.ConditionAdd, DispatcherType.ConditionAddFromD20StatusInit, DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x10262530)]
        public static int return_0()
        {
            return 0;
        }

        [DispTypes(DispatcherType.ImmunityTrigger)]
        [TempleDllLocation(0x100ed5a0)]
        public static void ImmunityTriggerCallback(in DispatcherCallbackArgs evt, D20DispatcherKey data)
        {
            var dispIo = evt.GetDispIoTypeImmunityTrigger();
            if (evt.subDispNode.condNode == dispIo.condNode && evt.dispKey == data)
            {
                dispIo.interrupt = 1;
                dispIo.SDDKey1 = (int) data;
                if (data == D20DispatcherKey.IMMUNITY_SPELL)
                {
                    dispIo.spellId = evt.GetConditionArg1();
                }
            }
        }

        // TODO: THIS version was used by the race conditions (for favored class)
        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100fdd00)]
        public static void D20QueryConditionHasHandler(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.data1 == data && dispIo.data2 == 0)
            {
                dispIo.return_val = 1;
            }
        }

        [DispTypes(DispatcherType.NewDay, DispatcherType.ConditionRemove)]
        [TempleDllLocation(0x100f7790)]
        public static void CondNodeSetArgToZero(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(0);
        }


        [DispTypes(DispatcherType.SpellResistanceMod)]
        [TempleDllLocation(0x100ed4c0)]
        public static void SpellResistanceMod_Callback(in DispatcherCallbackArgs evt, int data)
        {
            int condArg1;
            DispIoBonusAndSpellEntry dispIo;

            condArg1 = evt.GetConditionArg1();
            dispIo = evt.GetDispIOBonusListAndSpellEntry();
            dispIo.bonList.AddBonus(condArg1, 36, 203);
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100edee0)]
        public static void SubdualImmunityDamageCallback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddModFactor(0, DamageType.Subdual, 0x84);
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x100ed460)]
        public static void SpellResistanceDebug(in DispatcherCallbackArgs evt)
        {
            evt.GetConditionArg1();
        }


        [DispTypes(DispatcherType.GetMoveSpeed)]
        [TempleDllLocation(0x100efd60)]
        public static void sub_100EFD60(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_True_Seeing))
            {
                dispIo.factor *= 0.5F;
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ed480)]
        public static void SpellResistanceQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.data1 = evt.GetConditionArg1();
            dispIo.data2 = 0;
            dispIo.return_val = 1;
        }

        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x100ef540)]
        public static void TempNegativeLvlOnAdd(in DispatcherCallbackArgs evt, int bonusDescriptionId,
            int itemAlignmentMask)
        {
            if (bonusDescriptionId == ItemNegativeLevelBonusId &&
                (evt.objHndCaller.GetBaseStat(Stat.alignment) & itemAlignmentMask) != itemAlignmentMask)
            {
                return;
            }

            GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, -5);
            if (evt.objHndCaller.DispatchGetLevel(6, BonusList.Create(), null) + 1 <= 1)
            {
                GameSystems.D20.Combat.Kill(evt.objHndCaller, null);
            }

            var highestLvl = 0;
            var highestLevelClass = 0;

            // Findest highest individual class-level that remains
            for (Stat classCode = Stat.level_barbarian; classCode <= Stat.level_wizard; classCode++)
            {
                var level = evt.objHndCaller.DispatchGetLevel((int) classCode, BonusList.Create(), null);
                if (level > highestLvl)
                {
                    highestLvl = level;
                    highestLevelClass = (int) classCode;
                }
            }


            evt.SetConditionArg1(highestLevelClass);
            GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, 0);
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100ef6e0)]
        public static void sub_100EF6E0(in DispatcherCallbackArgs evt, int bonusDescriptionId, int itemAlignmentMask)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            AddNegativeLevelBonus(in evt, ref dispIo.bonlist, -1, bonusDescriptionId, itemAlignmentMask);
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ecf30)]
        [TemplePlusLocation("condition.cpp:398")]
        public static void CondPrevent(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                dispIo.outputFlag = false;
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100ed220)]
        public static void CompetenceBonus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddBonus(data1, 0, data2);
        }

        [DispTypes(DispatcherType.SkillLevel, DispatcherType.AbilityCheckModifier)]
        [TempleDllLocation(0x100eba70)]
        public static void EncumbranceSkillLevel(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddBonus(-data1, 0, data2);
        }

        [DispTypes(DispatcherType.GetMoveSpeed, DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100cb890)]
        [TemplePlusLocation("ability_fixes.cpp:42")]
        public static void GrappledMoveSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            dispIo.bonlist.SetOverallCap(1, data1, 0, data2);
            dispIo.bonlist.SetOverallCap(2, data1, 0, data2);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c93d0)]
        public static void QueryReturnSpellId(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            dispIo.resultData = (ulong) evt.GetConditionArg1();
        }

        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.ConditionRemove2, DispatcherType.BeginRound,
            DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x100ed360)]
        public static void PlayParticlesSavePartsysId(in DispatcherCallbackArgs evt, int data1, string data2)
        {
            var partSys = GameSystems.ParticleSys.CreateAtObj(data2, evt.objHndCaller);
            evt.SetConditionPartSysArg(data1, (PartSys) partSys);
        }

        [DispTypes(DispatcherType.GetAttackerConcealmentMissChance)]
        [TempleDllLocation(0x100efda0)]
        public static void AddAttackerInvisibleBonusWithCustomMessage(in DispatcherCallbackArgs evt, int data1,
            int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_True_Seeing))
            {
                dispIo.bonOut.AddBonus(data1, 0, data2);
            }
        }

        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100ed080)]
        public static void turnBasedStatusInitSingleActionOnly(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            if (dispIo != null)
            {
                if (dispIo.tbStatus.hourglassState > HourglassState.PARTIAL)
                {
                    dispIo.tbStatus.hourglassState = HourglassState.PARTIAL;
                }
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ecf10)]
        public static void QuerySetReturnVal0(in DispatcherCallbackArgs evt)
        {
            evt.GetDispIoD20Query().return_val = 0;
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100ed500)]
        public static void TooltipSpellResistanceCallback(in DispatcherCallbackArgs evt, int combatMesLineId)
        {
            var dispIo = evt.GetDispIoTooltip();
            var text = GameSystems.D20.Combat.GetCombatMesLine(combatMesLineId);
            var condArg1 = evt.GetConditionArg1();
            dispIo.Append($"{text} [{condArg1}]");
        }

        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100ed050)]
        public static void turnBasedStatusInitNoActions(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            if (dispIo != null)
            {
                dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
                dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            }
        }
    }
}