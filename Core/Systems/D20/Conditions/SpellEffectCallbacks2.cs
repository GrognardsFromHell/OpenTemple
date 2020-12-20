using System;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Utils;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Conditions
{
    public static partial class SpellEffects
    {
        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc500)]
        public static void sub_100CC500(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20014, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100cb4f0)]
        public static void WebBreakfreeRadial(in DispatcherCallbackArgs evt, int data)
        {
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Is_BreakFree_Possible))
            {
                var spellId = evt.GetConditionArg1();
                var radMenuEntry = RadialMenuEntry.CreateAction(5061, D20ActionType.BREAK_FREE, spellId,
                    "TAG_RADIAL_MENU_BREAK_FREE");
                radMenuEntry.spellIdMaybe = evt.GetConditionArg1();
                if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
                {
                    var v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Movement);
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
                }
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d6d70)]
        public static void WebObjEvent(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        if (GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc,
                            SavingThrowType.Reflex, 0, spellPkt.spellId))
                        {
                            GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30001, TextFloaterColor.White);
                            var v10 = dispIo.tgt;
                            var v12 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v10);
                            spellPkt.AddTarget(dispIo.tgt, v12, true);
                            dispIo.tgt.AddCondition("sp-Web Off", spellPkt.spellId, spellPkt.durationRemaining,
                                dispIo.evtId, 20);
                        }
                        else
                        {
                            GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30002, TextFloaterColor.White);
                            var v5 = dispIo.tgt;
                            var v7 = GameSystems.ParticleSys.CreateAtObj("sp-Web Hit", v5);
                            spellPkt.AddTarget(dispIo.tgt, v7, true);
                            dispIo.tgt.AddCondition("sp-Web On", spellPkt.spellId, spellPkt.durationRemaining,
                                dispIo.evtId);
                        }
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _web_hit_trigger(): cannot remove target");
                            return;
                        }

                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100c41f0)]
        public static void DispelAlignmentAcBonus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            bool v3;
            int bonAmt;

            var dispIo = evt.GetDispIoAttackBonus();
            var v2 = data2;
            switch (v2)
            {
                case 175:
                    v3 = !(dispIo.attackPacket.attacker.HasChaoticAlignment());
                    goto LABEL_8;
                case 176:
                    if (!(dispIo.attackPacket.attacker.HasEvilAlignment()))
                    {
                        return;
                    }

                    v2 = data2;
                    bonAmt = data1;
                    goto LABEL_11;
                case 177:
                    if (dispIo.attackPacket.attacker.HasGoodAlignment())
                    {
                        dispIo.bonlist.AddBonus(data1, 11, data2);
                    }

                    break;
                case 178:
                    v3 = !(dispIo.attackPacket.attacker.HasLawfulAlignment());
                    LABEL_8:
                    if (!v3)
                    {
                        dispIo.bonlist.AddBonus(data1, 11, data2);
                    }

                    break;
                default:
                    bonAmt = data1;
                    LABEL_11:
                    dispIo.bonlist.AddBonus(bonAmt, 11, v2);
                    break;
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c6440)]
        public static void sub_100C6440(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (evt.GetConditionArg3() > 0)
            {
                dispIo.return_val = 1;
            }

            dispIo.data1 = evt.GetConditionArg1();
            dispIo.data2 = 0;
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf760)]
        public static void BeginSpellSilence(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = (float) (int) a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 32, 33, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_silence(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c7450)]
        public static void DivinePowerStrengthBonus(in DispatcherCallbackArgs evt, Stat attribute, int data2)
        {
            var dispIo = evt.GetDispIoBonusList();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute == attribute)
            {
                dispIo.bonlist.AddBonus(6, 12, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd430)]
        public static void enlargeModelScaleInc(in DispatcherCallbackArgs evt)
        {
            ApplyModelScale(evt.objHndCaller, 1.8f);
        }

        [DispTypes(DispatcherType.ConditionRemove)]
        public static void EnlargeEnded(in DispatcherCallbackArgs evt)
        {
            UnequipTooLargeWeapons(evt.objHndCaller);
            RefreshReach(evt.objHndCaller);
        }

        /// <summary>
        /// Checks that the weapons wielded by the creature aren't too large after a size category change.
        /// </summary>
        private static void UnequipTooLargeWeapons(GameObjectBody critter)
        {
            if (critter.IsMonsterCategory(MonsterCategory.giant))
            {
                return;
            }

            // TODO: I think in general this should use GetWieldType rather than the replicated size checks here

            var primaryWeapon = critter.ItemWornAt(EquipSlot.WeaponPrimary);
            var secondaryWeapon = critter.ItemWornAt(EquipSlot.WeaponSecondary);
            if (primaryWeapon != null) {
                var size = (SizeCategory) primaryWeapon.GetInt32(obj_f.size);
                // TODO: Shouldn't this check against the new size category of the critter rather than Medium?
                if (size > SizeCategory.Medium && secondaryWeapon != null)
                {
                    Logger.Debug("Unequipping {0} from {1} because it became too large to wield",
                        primaryWeapon, critter);
                    GameSystems.Item.UnequipItem(primaryWeapon);
                }
            }

            if (secondaryWeapon != null)
            {
                var size = (SizeCategory) secondaryWeapon.GetInt32(obj_f.size);
                // TODO: Shouldn't this check against the new size category of the critter rather than Medium?
                if (size > SizeCategory.Medium)
                {
                    Logger.Debug("Unequipping {0} from {1} because it became too large to wield",
                        secondaryWeapon, critter);
                    GameSystems.Item.UnequipItem(secondaryWeapon);
                }

            }
        }

        /// <summary>
        /// Update a creature's reach after a size category change.
        /// </summary>
        private static void RefreshReach(GameObjectBody critter)
        {
            // TODO: Instead of changing a creature's reach field when a condition ends, the reach should be queried using a dispatcher type,
            // TODO: to which this condition would subscribe.
            var sizeCat = (SizeCategory) critter.GetInt32(obj_f.size);
            critter.SetInt32(obj_f.critter_reach, GetReachForSizeCategory(sizeCat));
        }

        private static int GetReachForSizeCategory(SizeCategory category)
        {
            if (category <= SizeCategory.Medium)
            {
                return 0;
            }
            else
            {
                return 10;
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce470)]
        public static void InvisibilitySphereBegin(in DispatcherCallbackArgs evt)
        {
            SpellEntry spEntry;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out spEntry);
                var radiusInches = (float) (int) spEntry.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 20, 21, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_invisibility_sphere(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100c6160)]
        public static void BestowCurseActionsTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            if (GameSystems.Random.GetInt(0, 1) == 1 && dispIo != null)
            {
                dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
                dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc2e0)]
        public static void sub_100CC2E0(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20019, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100c8340)]
        public static void Guidance_RadialMenuEntry_Callback(in DispatcherCallbackArgs evt, int data)
        {
            var parentEntry = RadialMenuEntry.CreateAction(5055, D20ActionType.CAST_SPELL, 0, null);
            var parentId =
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry,
                    RadialMenuStandardNode.Spells);

            // next attack roll
            var attackRollEntry = evt.CreateToggleForArg(2);
            attackRollEntry.text = GameSystems.D20.Combat.GetCombatMesLine(5056);
            attackRollEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(213);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref attackRollEntry, parentId);

            // next saving throw
            var savingThrowEntry = evt.CreateToggleForArg(3);
            savingThrowEntry.text = GameSystems.D20.Combat.GetCombatMesLine(5057);
            savingThrowEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(213);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref savingThrowEntry, parentId);

            // next skill check
            var skillCheckEntry = evt.CreateToggleForArg(4);
            skillCheckEntry.text = GameSystems.D20.Combat.GetCombatMesLine(5058);
            skillCheckEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(213);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref skillCheckEntry, parentId);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c6b80)]
        public static void sub_100C6B80(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt) && evt.GetConditionArg3() == 3)
            {
                dispIo.return_val = 1;
                dispIo.obj = spellPkt.caster;
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d5010)]
        public static void Condition__36__invisibility_purge(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var v7 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v5);
                        spellPkt.AddTarget(dispIo.tgt, v7, true);
                        dispIo.tgt.AddCondition("sp-Invisibility Purge Hit", spellPkt.spellId,
                            spellPkt.durationRemaining, dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _invisibility_purge_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100c6050)]
        public static void sub_100C6050(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(data1, 9, data2);
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d5d90)]
        public static void SleetStormAoE(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var partsysId = GameSystems.ParticleSys.CreateAtObj("sp-Sleet Storm-Hit", v5);
                        spellPkt.AddTarget(dispIo.tgt, partsysId, true);
                        dispIo.tgt.AddCondition("sp-Sleet Storm Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _sleet_storm_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.SkillLevel, DispatcherType.AbilityCheckModifier)]
        [TempleDllLocation(0x100c5a30)]
        public static void sub_100C5A30(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (data2 == 223)
            {
                dispIo.bonOut.AddBonus(-data1, 0, 223);
            }
            else
            {
                dispIo.bonOut.AddBonus(data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100caed0)]
        public static void StinkingCloudRemoveConcentration(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20026, TextFloaterColor.Red);
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Concentrating))
            {
                var spellId = (int) GameSystems.D20.D20QueryReturnData(evt.objHndCaller,
                    D20DispatcherKey.QUE_Critter_Is_Concentrating);
                GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Remove_Concentration, spellId);
            }
        }


        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100c3390)]
        public static void TooltipHoldingCharges(in DispatcherCallbackArgs evt, int meslineKey, int spellCondType)
        {
            int condArg3;

            var dispIo = evt.GetDispIoTooltip();
            if (spellCondType == 29)
            {
                // Chill touch stores remaining charges in arg3
                condArg3 = evt.GetConditionArg3();
            }
            else
            {
                // Vampiric touch will always show 0...?
                condArg3 = spellCondType != 232 ? 1 : 0;
            }

            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            dispIo.Append($"{meslineValue}{condArg3}");
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c43d0)]
        public static void HasConditionReturnSpellId(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            // TODO: This is only semi-useful for conditions that stack (i.e. failed Sanctuary Saves)
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.obj == data && dispIo.resultData == 0)
            {
                dispIo.return_val = 1;
                dispIo.resultData = (ulong) evt.GetConditionArg1();
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c7490)]
        public static void sub_100C7490(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (evt.dispKey == D20DispatcherKey.SAVE_REFLEX)
            {
                dispIo.bonlist.AddBonus(data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd100)]
        public static void BeginSpellDivinePower(in DispatcherCallbackArgs evt)
        {
            var strName = GameSystems.Stat.GetStatName(Stat.strength);
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20023, TextFloaterColor.White, suffix: $" [{strName}]");
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                // Temporary hit points gained
                var prefix = $"[{spellPkt.casterLevel}] ";
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, prefix);
                Logger.Info("d20_mods_spells.c / _begin_aid(): gained {0} temporary hit points", spellPkt.casterLevel);
                var condArg2 = evt.GetConditionArg2();
                var v7 = evt.GetConditionArg1();
                if (!evt.objHndCaller.AddCondition("Temporary_Hit_Points", v7, condArg2, spellPkt.casterLevel))
                {
                    Logger.Info("d20_mods_spells.c / _begin_spell_divine_power(): unable to add condition");
                }
            }
            else
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_divine_power(): unable to get spell_packet");
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100ca440)]
        public static void RighteousMightAbilityBonus(in DispatcherCallbackArgs evt, Stat attribute, int bonusMesLine)
        {
            var dispIo = evt.GetDispIoBonusList();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute == attribute)
            {
                switch (attribute)
                {
                    case Stat.strength:
                        dispIo.bonlist.AddBonus(4, 35, bonusMesLine);
                        break;
                    case Stat.constitution:
                        dispIo.bonlist.AddBonus(2, 35, bonusMesLine);
                        break;
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf070)]
        public static void RageBeginSpell(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (!spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0))
                {
                    Logger.Info("d20_mods_spells.c / _begin_spell_rage(): unable to add condition to spell_caster");
                }
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20046, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100cb480)]
        public static void d20_mods_spells_vampiric_touch_add_temp_hp(in DispatcherCallbackArgs evt, int data)
        {
            if (evt.GetDispIoDamage().attackPacket.d20ActnType == D20ActionType.TOUCH_ATTACK)
            {
                var dispIo = evt.GetDispIoDamage();
                var condArg3 = evt.GetConditionArg3();
                var v3 = dispIo.damage.finalDamage;
                if (v3 > 0)
                {
                    evt.SetConditionArg3(v3 + condArg3);
                }

                var v4 = evt.GetConditionArg3();
                Logger.Info("d20_mods_spells.c / _vampiric_touch_add_temp_hp(): took ({0}) damage, temp_hp=( {1} )", v3,
                    v4);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d00b0)]
        public static void BeginSpellStinkingCloud(in DispatcherCallbackArgs evt)
        {
            SpellEntry spEntry;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out spEntry);
                var radiusInches = (float) (int) spEntry.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var objEvtId = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 44, 45, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(objEvtId);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_stinking_cloud(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc4c0)]
        public static void FloatMessageAfraid(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg3()) == 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20013, TextFloaterColor.Red);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c6d60)]
        public static void sub_100C6D60(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ((evt.GetConditionArg3()) != 0)
            {
                dispIo.return_val = 1;
                dispIo.obj = evt.GetConditionArg1();
            }
            else
            {
                dispIo.return_val = 0;
            }
        }


        [DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
        [TempleDllLocation(0x100c7da0)]
        public static void FogCloudConcealmentMissChance(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var v3 = dispIo.attackPacket.attacker.DistanceToObjInFeet(dispIo.attackPacket.victim);
            if (!GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing))
            {
                if (v3 <= 5F)
                {
                    dispIo.bonlist.AddBonus(20, 19, 233);
                }
                else
                {
                    dispIo.bonlist.AddBonus(50, 19, 233);
                }
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c6230)]
        public static void BestowCurseAbilityMalus(in DispatcherCallbackArgs evt, int data1, int bonusMesLine)
        {
            var dispIo = evt.GetDispIoBonusList();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            var attribute = (Stat) evt.GetConditionArg3();
            if (queryAttribute == attribute)
            {
                dispIo.bonlist.AddBonus(-6, 0, bonusMesLine);
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100c9580)]
        public static void ProtectionFromAlignmentPreventDamage(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            var attacker = dispIo.attackPacket.attacker;
            if (attacker != null)
            {
                if (!IsUsingNaturalAttacks(dispIo.attackPacket))
                {
                    return;
                }

                var spellId = evt.GetConditionArg1();
                if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
                {
                    Logger.Info(
                        "d20_mods_spells.c / _protection_from_alignment_prevent_damage(): unable to retrieve spell_packet");
                    return;
                }

                if (!attacker.HasCondition(SpellEffects.SpellSummoned))
                {
                    return;
                }

                if (!DoesAlignmentProtectionApply(attacker, spellPkt.spellEnum))
                {
                    return;
                }

                if (D20ModSpells.CheckSpellResistance(evt.objHndCaller, spellPkt))
                {
                    return;
                }

                dispIo.damage.AddModFactor(0F, DamageType.Unspecified, 104);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100c34c0)]
        public static void ConcentratingRadialMenu(in DispatcherCallbackArgs evt, int data)
        {
            var radMenuEntry =
                RadialMenuEntry.CreateAction(5060, D20ActionType.STOP_CONCENTRATION, 0, "TAG_STOP_CONCENTRATION");
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                RadialMenuStandardNode.Spells);
        }

        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c9220)]
        public static void OtilukesSphereOnDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            GameSystems.ParticleSys.CreateAtObj("sp-Otilukes Resilient Sphere-Hit", evt.objHndCaller);
            var condArg1 = evt.GetConditionArg1();
            GameSystems.Script.Spells.SpellTrigger(condArg1, SpellEvent.SpellStruck);
            dispIo.damage.AddModFactor(0F, DamageType.Unspecified, 0x68);
        }


        [DispTypes(DispatcherType.GetBonusAttacks)]
        [TempleDllLocation(0x100c87c0)]
        [TemplePlusLocation("spell_condition.cpp:257")]
        public static void HasteBonusAttack(in DispatcherCallbackArgs evt)
        {
            // TemplePlus fix: Prevent haste bonuses from stacking
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            dispIo.bonlist.AddBonus(1, 34, 174); // Haste
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dd3f0)]
        public static void MirrorImageStruck(in DispatcherCallbackArgs evt, int data)
        {
            var condArg3 = evt.GetConditionArg3();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                GameSystems.ParticleSys.CreateAtObj("sp-Mirror Image Loss", evt.objHndCaller);
                GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.SpellStruck);
                var v4 = condArg3 - 1;
                evt.SetConditionArg3(v4);
                if (v4 <= 0)
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                    SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cde50)]
        public static void sub_100CDE50(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg3(0);
            evt.SetConditionArg4(0);
            evt.SetConditionArg(4, 0);
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c40a0)]
        public static void ChaosHammer_ToHit_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (data2 == 282)
            {
                dispIo.bonlist.AddBonus(-data1, 0, 282);
            }
            else
            {
                dispIo.bonlist.AddBonus(data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce940)]
        [TemplePlusLocation("spell_condition.cpp:245")]
        public static void AcidDamage(in DispatcherCallbackArgs evt)
        {
            // TemplePlus: This is a templeplus fix that is very different from the original...
            var isCritical = evt.GetConditionArg3() != 0;
            var condArg1 = evt.GetConditionArg1();
            GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt);

            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 46);

            var flags = D20CAF.HIT;
            if (isCritical)
            {
                flags |= D20CAF.CRITICAL;
                evt.SetConditionArg3(0);
            }

            // only the first shot gets sneak attack damage
            if (spellPkt.durationRemaining < spellPkt.duration)
            {
                flags |= D20CAF.NO_PRECISION_DAMAGE;
            }

            var damDice = new Dice(2, 4);
            GameSystems.D20.Combat.DealWeaponlikeSpellDamage(spellPkt.Targets[0].Object, spellPkt.caster, damDice,
                DamageType.Acid, D20AttackPower.UNSPECIFIED, 100, 103, D20ActionType.CAST_SPELL, spellPkt.spellId,
                flags);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100dbd90)]
        public static void sub_100DBD90(in DispatcherCallbackArgs evt)
        {
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Remove_Disease, 0, 0);
            SpellEffects.Spell_remove_spell(in evt, 0, 0);
            SpellEffects.Spell_remove_mod(in evt, 0);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ceb20)]
        public static void BeginSpellMindFog(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = (float) (int) a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 24, 25, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_mind_fog(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c6630)]
        [TemplePlusLocation("spell_condition.cpp:89")]
        public static void CalmEmotionsActionInvalid(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoD20Query();
            var action = (D20Action) dispIo.obj;
            if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget))
            {
                // TemplePlus fix: Inverted the condition here
                if (!GameSystems.Critter.IsFriendly(action.d20APerformer, action.d20ATarget))
                {
                    dispIo.return_val = 1;
                    dispIo.data1 = 0;
                    dispIo.data2 = 0;
                }
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc2a0)]
        public static void sub_100CC2A0(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20032, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.InitiativeMod)]
        [TempleDllLocation(0x100c5b00)]
        public static void DeafnessInitiativeMod(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (data2 == 190)
            {
                dispIo.bonOut.AddBonus(-data1, 0, 190);
            }
            else
            {
                dispIo.bonOut.AddBonus(data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce320)]
        public static void BeginSpellInvisibilityPurge(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out var spellEntry);
                spellPkt.aoeObj = evt.objHndCaller;
                var rangeType = (SpellRangeType) (-spellEntry.radiusTarget);
                var radiusInches =
                    GameSystems.Spell.GetSpellRangeExact(rangeType, spellPkt.casterLevel, spellPkt.caster)
                    * locXY.INCH_PER_FEET;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 18, 19, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c5cd0)]
        [TemplePlusLocation("spell_condition.cpp:344")]
        public static void IsCritterAfraidQuery(in DispatcherCallbackArgs evt)
        {
            var isShaken = evt.GetConditionArg3() != 0;
            if (isShaken)
            {
                return;
            }

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spPkt))
            {
                Logger.Warn("IsCritterAfraidQuery: Unable to get spell packet. Id {0}", spellId);
                return;
            }

            if (evt.objHndCaller.HasCondition(SpellCalmEmotions))
            {
                return;
            }

            if (evt.objHndCaller.HasCondition(SpellRemoveFear))
            {
                // added in Temple+
                return;
            }

            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            dispIo.obj = spPkt.caster;
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100c7510)]
        public static void sub_100C7510(in DispatcherCallbackArgs evt, int data)
        {
            evt.SetConditionArg4(5);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd0e0)]
        public static void sub_100CD0E0(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20011, TextFloaterColor.White);
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100ca620)]
        public static void SleetStormTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var bonlist = BonusList.Create();
            DispatcherExtensions.dispatch1ESkillLevel(evt.objHndCaller, SkillId.balance, ref bonlist, null,
                SkillCheckFlags.UnderDuress);
            var balanceName = GameSystems.Skill.GetSkillName(SkillId.balance);
            var v2 = GameSystems.Spell.DispelRoll(evt.objHndCaller, bonlist, 0, 10, balanceName);
            var suffix = $" [{balanceName}]";
            if (v2 < 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20042, TextFloaterColor.White, suffix: suffix);
                evt.SetConditionArg4(-v2);
                if (v2 <= -5)
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20041, TextFloaterColor.Red);
                    evt.objHndCaller.AddCondition(StatusEffects.Prone);
                    GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.Falldown);
                }
            }
            else
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20043, TextFloaterColor.White, suffix: suffix);
                evt.SetConditionArg4(0);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ccf80)]
        public static void BeginSpellDesecrate(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = (float) (int) a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 6, 7, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_desecrate(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100dd2d0)]
        public static void SavingThrow_sp_Guidance_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            if (evt.GetConditionArg4() > 0)
            {
                var dispIo = evt.GetDispIoSavingThrow();
                dispIo.bonlist.AddBonus(data1, 34, data2);
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d71a0)]
        public static void FrogTongue_breakfree_callback(in DispatcherCallbackArgs evt, int data)
        {
            var strengthMod = evt.objHndCaller.GetStat(Stat.str_mod);
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            var bonList = BonusList.Create();
            bonList.AddBonus(strengthMod, 0, 103);
            var strName = GameSystems.Stat.GetStatName(0);
            if (GameSystems.Spell.DispelRoll(evt.objHndCaller, bonList, 0, 20, strName) < 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21002, TextFloaterColor.Red);
                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                return;
            }

            GameSystems.D20.Actions.PerformOnAnimComplete(evt.objHndCaller, -1);
            FrogGrappleController.PlayRetractTongue(spellPkt.caster);
            spellPkt.caster.SetAnimId(spellPkt.caster.GetIdleAnimId());
            evt.objHndCaller.SetAnimId(evt.objHndCaller.GetIdleAnimId());
            FrogGrappleEnding(spellPkt, evt.objHndCaller);

            if (spellPkt.Targets.Length < 0)
            {
                var target = spellPkt.Targets[0].Object;
                GameSystems.D20.D20SendSignal(target, D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
                GameSystems.D20.D20SendSignal(target, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            }

            GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId,
                0);
            GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            if (spellPkt.RemoveTarget(evt.objHndCaller))
            {
                GameSystems.Spell.EndSpell(spellId);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
            else
            {
                Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100d7450)]
        public static void SlipperyMindActivate(in DispatcherCallbackArgs evt, int data)
        {
            var spellId = evt.GetConditionArg1();
            var targetIdx = evt.GetConditionArg2();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                var spellEntry = GameSystems.Spell.GetSpellEntry(spellPkt.spellEnum);
                var target = spellPkt.Targets[targetIdx].Object;

                if (!GameSystems.D20.Combat.SavingThrowSpell(target, evt.objHndCaller, spellPkt.dc,
                    spellEntry.savingThrowType, 0,
                    spellPkt.spellId))
                {
                    GameSystems.Spell.FloatSpellLine(target, 30002, TextFloaterColor.White);
                }
                else
                {
                    var spellName = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
                    GameSystems.Spell.FloatSpellLine(target, 30000, TextFloaterColor.White, suffix: $" [{spellName}]");

                    var dispIo = new DispIoDispelCheck();
                    dispIo.spellId = spellPkt.spellId;
                    dispIo.returnVal = 0;
                    dispIo.flags = 32;
                    target.DispatchDispelCheck(dispIo);
                }
            }

            SpellEffects.Spell_remove_mod(in evt, 0);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c6d10)]
        public static void ConfusionHasAiOverride(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var condArg3 = evt.GetConditionArg3();
            if ((condArg3) != 0 && condArg3 != 15)
            {
                dispIo.data1 = condArg3;
                dispIo.return_val = 1;
                dispIo.data2 = 0;
            }
            else
            {
                dispIo.return_val = 0;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100c99e0)]
        public static void RepelVerminOnAdd(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var v2 = GameSystems.Critter.GetHitDiceNum(evt.objHndCaller);
                if (v2 >= spellPkt.casterLevel / 3)
                {
                    var v3 = spellPkt.spellId;
                    Dice v4 = new Dice(2, 6);
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v4, DamageType.Magic,
                        D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, v3, 0);
                }
                else
                {
                    GameSystems.AI.FleeFrom(evt.objHndCaller, spellPkt.caster);
                }
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100c7e70)]
        public static void sub_100C7E70(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var v1 = data1;
            var dispIo = evt.GetDispIoMoveSpeed();
            dispIo.bonlist.SetOverallCap(1, v1, 0, data2);
            dispIo.bonlist.SetOverallCap(2, v1, 0, data2);
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100caf30)]
        [TemplePlusLocation("spell_condition.cpp:264")]
        public static void AoeObjEventStinkingCloud(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var spellId = evt.GetConditionArg1();
                if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
                {
                    return;
                }

                if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                {
                    return;
                }

                /*
                    AoE Entered;
                     - add the target to the Spell's Target List
                     - Do a saving throw
                  */
                if (evt.dispKey == D20DispatcherKey.OnEnterAoE && data == 0)
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);

                    var partSys = GameSystems.ParticleSys.CreateAtObj("sp-Stinking Cloud Hit", dispIo.tgt);
                    spellPkt.AddTarget(dispIo.tgt, partSys, true);
                    if (GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc,
                        SavingThrowType.Fortitude, 0, spellPkt.spellId))
                    {
                        /*
                          save succeeded; add the "Hit Pre" condition, which will attempt
                          to apply the condition in the subsequent turns
                        */
                        dispIo.tgt.AddCondition("sp-Stinking Cloud Hit Pre", spellPkt.spellId,
                            spellPkt.durationRemaining, dispIo.evtId);
                    }
                    else
                    {
                        /*
                          Save failed; apply the condition
                        */
                        dispIo.tgt.AddCondition("sp-Stinking Cloud Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId, 0);
                    }
                }
                /*
                  AoE exited;
                   - If "Hit Pre" (identified by data1 = 223), remove the condition so the character doesn't keep making saves outside the cloud
                   - If "Hit" (identified by data1 = 222), reduce the remaining duration to 1d4+1
                */
                else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                {
                    if (data == 222) // the sp-Stinking Cloud Hit condition
                    {
                        evt.SetConditionArg4(1);
                        // sets the remaining duration to 1d4+1
                        var remainingDuration = Dice.D4.WithModifier(1).Roll();
                        evt.SetConditionArg2(remainingDuration);
                        var targetName = GameSystems.MapObject.GetDisplayName(dispIo.tgt);
                        GameSystems.RollHistory.CreateFromFreeText(
                            $"{targetName} exited Stinking Cloud; Nauseated for {remainingDuration} more rounds.\n");
                    }
                    else if (data == 223) // the sp-Stinking Cloud Hit Pre condition
                    {
                        // remove the condition (cloud has been exited)
                        // it will get re-added if the target re-enters via this same callback (see above)
                        evt.RemoveThisCondition();
                    }
                }

                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.D20Signal)]
        [TempleDllLocation(0x100c8270)]
        [TemplePlusLocation("spell_condition.cpp:267")]
        public static void GreaseSlippage(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                evt.RemoveThisCondition();
                return;
            }

            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
                return;

            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Untripable))
                return;

            var spEntry = GameSystems.Spell.GetSpellEntry(spellPkt.spellEnum);

            if (!GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                spEntry.savingThrowType, 0, spellPkt.spellId))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(48, evt.objHndCaller, null);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 104);
                evt.objHndCaller.AddCondition(StatusEffects.Prone);
                GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.Falldown);
            }
        }

        [DispTypes(DispatcherType.ToHitBonusFromDefenderCondition)]
        [TempleDllLocation(0x100cb8e0)]
        public static void sub_100CB8E0(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(data, 0, 211);
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c8780)]
        public static void sub_100C8780(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (evt.dispKey == D20DispatcherKey.SAVE_REFLEX)
            {
                dispIo.bonlist.AddBonus(data1, 8, data2);
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100c97c0)]
        public static void sub_100C97C0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(-data1, 13, data2);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100de3f0)]
        [TemplePlusLocation("condition.cpp:3591")]
        public static void SpellDismissSignalHandler(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            if (dispIo.data1 != spellPkt.spellId)
            {
                return;
            }

            if (spellPkt.spellEnum == WellKnownSpells.MirrorImage || data == 1 || spellPkt.Targets.Length > 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100d6850)]
        public static void StinkingCloudPreBeginRound(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                    SavingThrowType.Fortitude, 0, spellPkt.spellId))
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
                }
                else
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);

                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _stinking_cloud_pre_fort_save(): cannot remove target");
                        return;
                    }

                    SpellEffects.Spell_remove_mod(in evt, 0);
                    var v5 = GameSystems.ParticleSys.CreateAtObj("sp-Stinking Cloud Hit", evt.objHndCaller);
                    spellPkt.AddTarget(evt.objHndCaller, v5, true);
                    var condArg3 = evt.GetConditionArg3();
                    evt.objHndCaller.AddCondition("sp-Stinking Cloud Hit", spellPkt.spellId, spellPkt.durationRemaining,
                        condArg3);
                }
            }

            {
                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            } /*  else
  {
    Logger.Info("d20_mods_spells.c / _stinking_cloud_pre_fort_save(): unable to save new spell_packet");
  }
*/
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c60e0)]
        public static void StatLevel_callback_AnimalGrowth(in DispatcherCallbackArgs evt, Stat attribute, int data2)
        {
            var dispIo = evt.GetDispIoBonusList();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute == attribute)
            {
                if (attribute == Stat.strength)
                {
                    dispIo.bonlist.AddBonus(-data2, 35, 274);
                }
                else
                {
                    dispIo.bonlist.AddBonus(data2, 35, 274);
                }
            }
        }


        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100c3530)]
        public static void ConcentratingTooltipCallback(in DispatcherCallbackArgs evt, int data)
        {
            int v3;
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoTooltip();
            var meslineKey = data;
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
                var spellLine = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
                var textbuf = $"{meslineValue}[{spellLine}]";
                dispIo.Append(textbuf);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cdd50)]
        public static void sub_100CDD50(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var amount = Math.Min(20, spellPkt.casterLevel);
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White,
                    prefix: $"[{amount}] ");
                Logger.Info("d20_mods_spells.c / _begin_spell_greater_heroism(): gained {0} temporary hit points",
                    amount);
                var condArg2 = evt.GetConditionArg2();
                var v7 = evt.GetConditionArg1();
                if (!evt.objHndCaller.AddCondition("Temporary_Hit_Points", v7, condArg2, amount))
                {
                    Logger.Info("d20_mods_spells.c / _begin_spell_greater_heroism(): unable to add condition");
                }
            }
        }


        [DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
        [TempleDllLocation(0x100c91a0)]
        public static void ObscuringMist_Concealment_Callback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var v3 = LocationExtensions.DistanceToObjInFeet(dispIo.attackPacket.attacker, dispIo.attackPacket.victim);
            if (!GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing))
            {
                if (v3 <= 5F)
                {
                    dispIo.bonlist.AddBonus(20, 19, 238);
                }
                else
                {
                    dispIo.bonlist.AddBonus(50, 19, 238);
                }
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c4140)]
        public static void SavingThrowPenaltyCallback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (data2 == 169)
            {
                dispIo.bonlist.AddBonus(-data1, 0, 169);
            }
            else
            {
                dispIo.bonlist.AddBonus(data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100cac00)]
        public static void SpikeStonesHitCombatCritterMovedHandler(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var v2 = evt.GetDispIoD20Signal().data1;
            if (v2 > 0)
            {
                var v3 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(v3, out spellPkt))
                {
                    GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
                    GameSystems.SoundGame.PositionalSound(15127, 1, evt.objHndCaller);
                    var v5 = spellPkt.spellId;
                    var v6 = new Dice(v2 / 5, 8);
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v6, DamageType.Magic,
                        D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, v5, 0);
                    if (!GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                        SavingThrowType.Reflex, 0, spellPkt.spellId))
                    {
                        if (!evt.objHndCaller.AddCondition("sp-Spike Stones Damage", condArg1, 14400, condArg3))
                        {
                            Logger.Info("d20_mods_spells.c / _spike_stones_hit(): unable to add condition");
                        }
                    }
                }
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100c4970)]
        public static void sub_100C4970(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();
            var v2 = dispIo;
            switch (data2)
            {
                case 0xD2:
                    if ((dispIo.attackPacket.weaponUsed == null))
                    {
                        var v3 = data2;
                        var condArg3 = evt.GetConditionArg3();
                        v2.damage.AddDamageBonus(condArg3, 12, v3);
                    }

                    break;
                case 0xD4:
                    if (dispIo.attackPacket.weaponUsed != null || (evt.objHndCaller.GetStat(Stat.level_monk)) != 0)
                    {
                        var v5 = data2;
                        var condArg4 = evt.GetConditionArg4();
                        v2.damage.AddDamageBonus(condArg4, 12, v5);
                    }

                    break;
                case 0xD1:
                    if ((dispIo.attackPacket.weaponUsed == null))
                    {
                        goto LABEL_8;
                    }

                    break;
                default:
                    LABEL_8:
                    dispIo.damage.AddDamageBonus(data1, 12, data2);
                    break;
                case 0xD0:
                    if (dispIo.attackPacket.weaponUsed != null || (evt.objHndCaller.GetStat(Stat.level_monk)) != 0)
                    {
                        v2.damage.AddDamageBonus(data1, 12, data2);
                    }

                    break;
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100ca5e0)]
        public static void SleetStormHitMovementSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var condArg4 = evt.GetConditionArg4();
            var dispIo = evt.GetDispIoMoveSpeed();
            var previousFactor = dispIo.factor;
            if ((condArg4) != 0)
            {
                dispIo.factor = previousFactor * 0F;
            }
            else
            {
                dispIo.factor = previousFactor * 0.5F;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100c8a70)]
        public static void InvisibilitySphereHitBegin(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20017, TextFloaterColor.Red);
            if (!evt.objHndCaller.AddCondition("Invisible", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _invisibility_sphere_hit(): unable to add condition");
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100dbb30)]
        public static void WeaponEnhBonusOnAdd(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus))
            {
                var condArg1 = evt.GetConditionArg1();
                var condArg3 = evt.GetConditionArg3();
                evt.objHndCaller.AddConditionToItem(ItemEffects.WeaponEnhancementBonus, condArg3, 0, 0, 0, condArg1);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc640)]
        public static void sub_100CC640(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            if (!evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_charm_person_or_animal(): unable to add condition");
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc040)]
        public static void AnimateDeadOnAdd(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                return;
            }

            var target = spellPkt.Targets[0].Object;
            GameSystems.D20.Initiative.RemoveFromInitiative(target);
            if (GameSystems.Party.IsInParty(target))
            {
                GameSystems.Critter.RemoveFollower(target, true);
                GameUiBridge.UpdatePartyUi();
            }

            GameSystems.Item.PoopInventory(target, true);
            GameSystems.MapObject.SetFlags(target, ObjectFlag.OFF);
            int protoId;
            if (condArg3 == 1)
            {
                protoId = 14107; // Skeleton
            }
            else if (condArg3 == 2)
            {
                protoId = 14123; // Zombie
            }
            else
            {
                return;
            }

            var handleNew = GameSystems.MapObject.CreateObject(protoId, spellPkt.aoeCenter);
            if (GameSystems.Critter.AddFollower(handleNew, spellPkt.caster, true, true))
            {
                GameSystems.D20.Initiative.AddToInitiative(handleNew);
                var v6 = GameSystems.D20.Initiative.GetInitiative(spellPkt.caster);
                GameSystems.D20.Initiative.SetInitiative(handleNew, v6);
                GameUiBridge.UpdateInitiativeUi();
                GameUiBridge.UpdatePartyUi();
                Logger.Info("animate dead: new_obj=( {0} )", handleNew);
            }
            else
            {
                Logger.Info("animate dead: failed to add obj to party!");
                GameSystems.Object.Destroy(handleNew);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c58a0)]
        public static void sub_100C58A0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            int v3;

            var dispIo = evt.GetDispIoAttackBonus();
            switch (data2)
            {
                case 0xAB:
                case 0xDF:
                case 0x102:
                    v3 = -data1;
                    goto LABEL_6;
                case 0xAD:
                    if (!GameSystems.D20.D20Query(evt.objHndCaller,
                        D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
                    {
                        dispIo.bonlist.AddBonus(-data1, 0, data2);
                    }

                    break;
                default:
                    v3 = data1;
                    LABEL_6:
                    dispIo.bonlist.AddBonus(v3, 0, data2);
                    break;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce590)]
        public static void SpLesserRestorationOnConditionAdd(in DispatcherCallbackArgs evt)
        {
            var dispIo = new DispIoAbilityLoss();
            var condArg3 = evt.GetConditionArg3();
            dispIo.statDamaged = (Stat) condArg3;
            dispIo.flags |= 9;
            dispIo.fieldC = 1;
            dispIo.spellId = evt.GetConditionArg1();
            dispIo.result = Dice.D4.Roll();
            var amount = dispIo.result;
            var remaining = amount - evt.objHndCaller.DispatchGetAbilityLoss(dispIo);
            var statName = GameSystems.Stat.GetStatName(dispIo.statDamaged);
            Logger.Info(
                "d20_mods_spells.c / _begin_spell_lesser_restoration(): used {0}/{1} points to heal ({2}) damage",
                remaining, amount, statName);
            var suffix = $": {statName} [{remaining}]";
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20035, TextFloaterColor.White, suffix: suffix);
            SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100dba20)]
        public static void RemoveSpellWhenPreAddThis(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == (ConditionSpec) data)
            {
                dispIo.outputFlag = false;
                SpellEffects.Spell_remove_spell(in evt, 0, 0);
                SpellEffects.Spell_remove_mod(in evt, 0);
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c7300)]
        public static void sub_100C7300(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(data1, 17, data2);
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c4a90)]
        public static void DivineFavorToHitBonus2(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (data2 == 170)
            {
                var condArg3 = evt.GetConditionArg3();
                dispIo.bonlist.AddBonus(condArg3, 14, 170);
            }
            else
            {
                dispIo.bonlist.AddBonus(data1, 14, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100c7e20)]
        public static void AoESpellPreAddCheck(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data && evt.objHndCaller.HasCondition(data))
            {
                dispIo.outputFlag = false;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cdc00)]
        public static void sub_100CDC00(in DispatcherCallbackArgs evt)
        {
            evt.GetConditionArg3();
            evt.SetConditionArg3(1);
        }


        [DispTypes(DispatcherType.GetAttackDice)]
        [TempleDllLocation(0x100ca2b0)]
        [TemplePlusLocation("condition.cpp:447")]
        public static void AttackDiceEnlargePerson(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackDice();

            if (dispIo.weapon == null)
            {
                return;
            }

            var dice = dispIo.dicePacked;
            var diceCount = dice.Count;
            var diceSide = dice.Sides;
            var diceMod = dice.Modifier;

            // get wield type
            var weaponUsed = dispIo.weapon;
            var wieldType = GameSystems.Item.GetWieldType(evt.objHndCaller, weaponUsed, true);
            var wieldTypeWeaponModified =
                GameSystems.Item.GetWieldType(evt.objHndCaller, weaponUsed,
                    false); // the wield type if the weapon is not enlarged along with the critter

            // check offhand
            var offhandWeapon = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary);
            var shield = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.Shield);
            var regardOffhand = offhandWeapon != null || shield != null && !GameSystems.Item.IsBuckler(shield);

            bool enlargeWeapon = true; // by default enlarge the weapon
            // case 1
            switch (wieldType)
            {
                case 0: // light weapon
                    switch (wieldTypeWeaponModified)
                    {
                        case 2: // shouldn't really be possible, but just in case...
                            if (regardOffhand)
                                enlargeWeapon = false;
                            break;
                        default:
                            break;
                    }

                    break;
                case 1: // single handed wield if weapon is unaffected
                    switch (wieldTypeWeaponModified)
                    {
                        case 0
                            : // only in reduce person; going to assume the "beneficial" case that the reduction was made voluntarily and hence you let the weapon stay larger
                        case 1: // weapon can be enlarged
                            break;
                        case 2
                            : // this is the main case - weapon gets enlarged along with the character so it's now a THW
                            if (regardOffhand) // if holding something in offhand, hold off on increasing the damage
                                enlargeWeapon = false;
                            break;
                        default:
                            break;
                    }

                    break;
                case 2: // two handed wield if weapon is unaffected
                    switch (wieldTypeWeaponModified)
                    {
                        case 0: // these cases shouldn't exist for Enlarge ...
                        case 1
                            : // only in reduce person; going to assume the "beneficial" case that the reduction was made voluntarily and hence you let the weapon stay larger
                            if (regardOffhand) // has offhand item, so assume the weapon stayed the old size
                                enlargeWeapon = false;
                            break;
                        case 2:
                            if (regardOffhand) // shouldn't really be possible... maybe if player is cheating
                            {
                                enlargeWeapon = false;
                                Logger.Warn("Illegally wielding weapon along withoffhand!");
                            }

                            break;
                        default:
                            break;
                    }

                    break;
                case 3:
                case 4:
                default:
                    break;
            }

            if (!enlargeWeapon)
            {
                return;
            }

            switch (dice.Sides)
            {
                case 2:
                    diceSide = 3;
                    break;
                case 3:
                    diceSide = 4;
                    break;
                case 4:
                    diceSide = 6;
                    break;
                case 6:
                    if (diceCount == 1)
                        diceSide = 8;
                    else if (diceCount <= 3)
                        diceCount++;
                    else
                        diceCount += 2;
                    break;
                case 8:
                    if (diceCount == 1)
                    {
                        diceCount = 2;
                        diceSide = 6;
                    }
                    else if (diceCount <= 3)
                    {
                        diceCount++;
                    }
                    else if (diceCount <= 6)
                    {
                        diceCount += 2;
                    }
                    else
                        diceCount += 4;

                    break;
                case 10:
                    diceCount *= 2;
                    diceSide = 8;
                    break;
                case 12:
                    diceCount = 3;
                    diceSide = 6;
                    break;
                default:
                    break;
            }

            dispIo.dicePacked = new Dice(diceCount, diceSide, diceMod);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cbfa0)]
        public static void AnimalTranceBeginSpell(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spPkt))
            {
                if (!spPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0))
                {
                    Logger.Info(
                        "d20_mods_spells.c / _begin_spell_animal_trance(): unable to add condition to spell_caster");
                }
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20021, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c8f40)]
        public static void sub_100C8F40(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
            dispIo.damage.AddModFactor(0F, DamageType.Unspecified, 0x68);
        }


        [DispTypes(DispatcherType.EffectTooltip)]
        [TempleDllLocation(0x100c3dd0)]
        public static void EffectTooltip_Duration_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoEffectTooltip();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var duration = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.duration);
                var text = $" {duration}: {spellPkt.durationRemaining}/{spellPkt.duration}";
                dispIo.bdb.AddEntry(data1, text, spellPkt.spellEnum);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c9160)]
        public static void sub_100C9160(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = evt.GetConditionArg3();
            dispIo.data1 = evt.GetConditionArg1();
            dispIo.data2 = 0;
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100c65b0)]
        public static void CallLightningStormRadial(in DispatcherCallbackArgs evt, int data)
        {
            var helpId = GameSystems.Spell.GetSpellHelpTopic(560);
            var radMenuEntry = RadialMenuEntry.CreateAction(108, D20ActionType.SPELL_CALL_LIGHTNING, 0, helpId);
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                RadialMenuStandardNode.Spells);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cedd0)]
        public static void BeginSpellObscuringMist(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = (float) (int) a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 28, 29, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_obscuring_mist(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc220)]
        public static void sub_100CC220(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20007, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c5a80)]
        public static void SavingThrow_sp_Slow_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (evt.dispKey == D20DispatcherKey.SAVE_REFLEX)
            {
                if (data2 == 173)
                {
                    if (!GameSystems.D20.D20Query(evt.objHndCaller,
                        D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
                    {
                        dispIo.bonlist.AddBonus(-data1, 13, data2);
                    }
                }
                else
                {
                    dispIo.bonlist.AddBonus(data1, 13, data2);
                }
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100de2a0)]
        public static void FrongTongueSwallowedDamage(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (GameSystems.Critter.IsDeadNullDestroyed(spellPkt.caster))
                {
                    SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                }
                else
                {
                    var v4 = 1;
                    var v5 = 3;
                    if (spellPkt.caster.GetStat(Stat.size) > 6)
                    {
                        v4 = 2;
                        v5 = 4;
                    }

                    var v6 = spellPkt.spellId;
                    Dice v7 = new Dice(v4, v5, 0);
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v7, 0,
                        D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, v6, 0);
                    var v8 = spellPkt.spellId;
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v7, DamageType.Acid,
                        D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, v8, 0);
                }
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d3de0)]
        public static void Condition__36__control_plants_sthg(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        if (GameSystems.Critter.IsFriendly(spellPkt.caster, dispIo.tgt))
                        {
                            dispIo.tgt.AddCondition("sp-Control Plants Disentangle", spellPkt.spellId,
                                spellPkt.durationRemaining, dispIo.evtId);
                        }
                        else
                        {
                            var v6 = GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc,
                                SavingThrowType.Will, 0, spellPkt.spellId);
                            if (v6)
                            {
                                GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30001, TextFloaterColor.White);
                                spellPkt.AddTarget(dispIo.tgt, null, true);
                                dispIo.tgt.AddCondition("sp-Control Plants Entangle Pre", spellPkt.spellId,
                                    spellPkt.durationRemaining, dispIo.evtId);
                            }
                            else
                            {
                                GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30002, TextFloaterColor.White);
                                var v9 = dispIo.tgt;
                                var v11 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", v9);
                                spellPkt.AddTarget(dispIo.tgt, v11, true);
                                dispIo.tgt.AddCondition("sp-Control Plants Entangle", spellPkt.spellId,
                                    spellPkt.durationRemaining, dispIo.evtId);
                            }
                        }
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100cb650)]
        public static void WebAbilityScoreMalus(in DispatcherCallbackArgs evt, Stat attribute, int data2)
        {
            var dispIo = evt.GetDispIoBonusList();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute == attribute
                && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                dispIo.bonlist.AddBonus(-4, 0, data2);
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100cabc0)]
        public static void sub_100CABC0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            dispIo.factor = dispIo.factor * 0.5F;
        }


        [DispTypes(DispatcherType.GetSizeCategory)]
        [TempleDllLocation(0x100c6140)]
        public static void EnlargeSizeCategory(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoD20Query();
            var v2 = dispIo.return_val;
            if (v2 < 10)
            {
                dispIo.return_val = v2 + 1;
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100c9370)]
        public static void SkillLevelPrayer(in DispatcherCallbackArgs evt, int data)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = data;
            var v3 = condArg3;
            var dispIo = evt.GetDispIoObjBonus();
            if (v3 == -3)
            {
                dispIo.bonOut.AddBonus(v2, 14, 151);
            }
            else
            {
                dispIo.bonOut.AddBonus(-v2, 14, 151);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100dbdf0)]
        public static void RemoveSpellOnAdd(in DispatcherCallbackArgs evt)
        {
            SpellEffects.Spell_remove_spell(in evt, 0, 0);
            SpellEffects.Spell_remove_mod(in evt, 0);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ddf40)]
        public static void WebBurningDamage(in DispatcherCallbackArgs evt, int data)
        {
            if (evt.GetConditionArg(4) != 1)
            {
                evt.SetConditionArg(4, 1);
                var spellId = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
                {
                    GameSystems.ParticleSys.CreateAtObj("sp-Web Flamed", spellPkt.aoeObj);

                    foreach (var target in spellPkt.Targets)
                    {
                        var dice = new Dice(2, 4);
                        GameSystems.D20.Combat.SpellDamageFull(target.Object, null, dice, DamageType.Fire,
                            D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spellId, 0);
                        GameSystems.ParticleSys.CreateAtObj("sp-Flame Tongue-hit", target.Object);
                    }
                }

                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c7f00)]
        public static void GaseousFormSpellInterruptedQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.return_val != 1)
            {
                var spellData = (D20SpellData) dispIo.obj;
                var spellComponent =
                    GameSystems.Spell.GetSpellComponentRegardMetamagic(spellData.SpellEnum, spellData.metaMagicData);

                if ((spellComponent & (SpellComponent.Verbal | SpellComponent.Somatic | SpellComponent.Material)) != 0)
                {
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x26, evt.objHndCaller, null);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 100);
                    dispIo.return_val = 1;
                }
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100c4d00)]
        public static void EmotionDamageBonus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();
            switch (data2)
            {
                case 0xA9:
                    dispIo.damage.AddDamageBonus(-data1, 13, data2);
                    break;
                case 0xAC:
                case 0x103:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
                        && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear))
                    {
                        dispIo.damage.AddDamageBonus(-data1, 13, data2);
                    }

                    break;
                case 0x104:
                    dispIo.damage.AddDamageBonus(data1, 13, data2);
                    break;
                default:
                    dispIo.damage.AddDamageBonus(data1, 13, data2);
                    break;
            }
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100cb910)]
        public static void VrockSporesDamage(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            evt.GetDispIOTurnBasedStatus();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) &&
                !evt.objHndCaller.HasCondition(SpellEffects.SpellDelayPoison))
            {
                if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Immune_Poison))
                {
                    GameSystems.Spell.PlayFizzle(evt.objHndCaller);
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 32000, TextFloaterColor.White);
                }
                else
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20501, TextFloaterColor.Red);
                    var v2 = spellPkt.spellId;
                    Dice v3 = new Dice(1, 2, 0);
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v3, DamageType.Poison,
                        D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, v2, 0);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d0610)]
        public static void BeginSpellWindWall(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = (float) spellPkt.casterLevel * 36F;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 48, 49, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100ca9a0)]
        public static void SolidFogDamageResistanceVsRanged(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                var damTotal = dispIo.damage.GetOverallDamageByType();
                dispIo.damage.AddDR(damTotal, DamageType.Unspecified, 104);
                dispIo.damage.finalDamage = dispIo.damage.GetOverallDamageByType();
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100c6790)]
        public static void ChillMetalDamage(in DispatcherCallbackArgs evt, int data)
        {
            var spellId = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                if (evt.GetConditionArg3() > 0)
                {
                    var condArg3 = evt.GetConditionArg3();
                    var dice = Dice.Constant(condArg3);
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, dice, DamageType.Cold,
                        D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spellPkt.spellId, 0);
                }

                switch (spellPkt.durationRemaining)
                {
                    case 2:
                    case 6:
                        evt.SetConditionArg3(Dice.D4.Roll());
                        break;
                    case 3:
                    case 4:
                    case 5:
                        var dice = new Dice(2, 4);
                        evt.SetConditionArg3(dice.Roll());
                        break;
                    default:
                        evt.SetConditionArg3(0);
                        return;
                }
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100d2ba0)]
        public static void sub_100D2BA0(in DispatcherCallbackArgs evt, int data)
        {
            var v1 = data;
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(v1, 34, 113);
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100c77d0)]
        public static void sub_100C77D0(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == (ConditionSpec) data
                && evt.GetConditionArg3() == dispIo.arg2)
            {
                dispIo.outputFlag = false;
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100c40f0)]
        public static void sub_100C40F0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();
            DamagePacket v3 = dispIo.damage;
            if (data2 == 282)
            {
                v3.AddDamageBonus(-data1, 0, 282);
            }
            else
            {
                v3.AddDamageBonus(data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cce30)]
        public static void DeafenedFloatMsg(in DispatcherCallbackArgs args)
        {
            GameSystems.Spell.FloatSpellLine(args.objHndCaller, 20020, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d2a90)]
        public static void sub_100D2A90(in DispatcherCallbackArgs evt)
        {
            evt.GetConditionArg1();
            evt.GetConditionArg2();
            evt.GetConditionArg3();
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dbe40)]
        public static void sub_100DBE40(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 != evt.GetConditionArg1())
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(in evt, 0, 0);
                SpellEffects.Spell_remove_mod(in evt, 0);
            }
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c7bd0)]
        public static void FireShieldDamageResistance(in DispatcherCallbackArgs evt)
        {
            DamageType type;
            int damageMesLine;

            var dispIo = evt.GetDispIoDamage();
            var condArg3 = evt.GetConditionArg3();
            if (condArg3 == 3)
            {
                type = DamageType.Fire;
                damageMesLine = 110;
            }
            else if (condArg3 == 9)
            {
                type = DamageType.Cold;
                damageMesLine = 111;
            }
            else
            {
                return;
            }

            DamagePacket v7 = dispIo.damage;
            var amount = (int) (v7.GetOverallDamageByType() * 0.5F);
            v7.AddDR(amount, type, damageMesLine);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dc800)]
        public static void FrogTongueCritterKilled(in DispatcherCallbackArgs evt, int condType)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                return;
            }

            var killed = (GameObjectBody) dispIo.obj;
            GameObjectBody spellTarget = null;
            if (spellPkt.Targets.Length > 0)
            {
                spellTarget = spellPkt.Targets[0].Object;
            }

            // Either the grappled target or the frog have been killed
            if (killed == spellTarget || killed == evt.objHndCaller)
            {
                if (spellTarget != null)
                {
                    GameSystems.D20.D20SendSignal(spellTarget, D20DispatcherKey.SIG_Spell_Grapple_Removed,
                        spellPkt.spellId, 0);
                }

                GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed,
                    spellPkt.spellId, 0);
                SpellEffects.Spell_remove_spell(in evt, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d0340)]
        public static void TreeShapeBeginSpell(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (!spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0))
                {
                    Logger.Info(
                        "d20_mods_spells.c / _begin_spell_tree_shape(): unable to add condition to spell_caster");
                }
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d4830)]
        public static void Condition__36__fog_cloud_sthg(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var v7 = GameSystems.ParticleSys.CreateAtObj("sp-Fog Cloud-hit", v5);
                        spellPkt.AddTarget(dispIo.tgt, v7, true);
                        dispIo.tgt.AddCondition("sp-Fog Cloud Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _fog_cloud_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100cb1a0)]
        public static void SuggestionIsCharmed(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (evt.GetConditionArg3() == 1)
            {
                dispIo.return_val = 1;
                dispIo.data1 = evt.GetConditionArg1();
                dispIo.data2 = 0;
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d4a00)]
        [TemplePlusLocation("spell_condition.cpp:83")]
        public static void GhoulTouchAttackHandler(in DispatcherCallbackArgs evt, int data)
        {
            var action = (D20Action) evt.GetDispIoD20Signal().obj;
            if ((action.d20Caf & D20CAF.HIT) == 0)
            {
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
                return;
            }

            var spellId = evt.GetConditionArg1();
            GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPktBody);
            GameSystems.Script.Spells.SpellSoundPlay(spellPktBody, SpellEvent.AreaOfEffectHit);
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
            GameSystems.Script.Spells.SpellSoundPlay(spellPktBody, SpellEvent.SpellStruck);

            if (D20ModSpells.CheckSpellResistance(action.d20ATarget, spellPktBody))
            {
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                evt.RemoveThisCondition();
                return;
            }

            // TemplePlus: Fixed target not getting a saving throw
            if (spellPktBody.SavingThrow(action.d20ATarget))
            {
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                evt.RemoveThisCondition();
                return;
            }

            spellPktBody.duration = new Dice(1, 6, 2).Roll();
            if (!action.d20ATarget.AddCondition("sp-Ghoul Touch Paralyzed", spellPktBody.spellId, spellPktBody.duration,
                0))
            {
                Logger.Info("d20_mods_spells.c / _ghoul_touch_stench_hit(): unable to add condition");
            }

            var partSys = GameSystems.ParticleSys.CreateAtObj("sp-Ghoul Touch", action.d20ATarget);
            if (!action.d20ATarget.AddCondition("sp-Ghoul Touch Stench", spellPktBody.spellId, spellPktBody.duration, 0,
                partSys))
            {
                Logger.Info("d20_mods_spells.c / _ghoul_touch_stench_hit(): unable to add condition");
            }

            if (!spellPktBody.RemoveTarget(evt.objHndCaller))
            {
                Logger.Info("d20_mods_spells.c / _ghoul_touch_hit_trigger(): cannot remove target");
            }

            spellPktBody.AddTarget(action.d20ATarget, partSys);
            GameSystems.Spell.UpdateSpellPacket(spellPktBody);
            GameSystems.Script.Spells.UpdateSpell(spellPktBody.spellId);
            SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100d2e90)]
        public static void SpellResistanceTooltipCallback(in DispatcherCallbackArgs evt, int data)
        {
            int v2;

            var dispIo = evt.GetDispIoTooltip();
            var meslineKey = data;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            var condArg3 = evt.GetConditionArg3();
            dispIo.Append($"{meslineValue} [{condArg3}]");
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100cb6b0)]
        public static void sub_100CB6B0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                dispIo.bonlist.AddBonus(-data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeed, DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100c8b00)]
        public static void sub_100C8B00(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            dispIo.bonlist.AddBonus(data1, 12, data2);
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d4e60)]
        public static void IceStormHitTrigger(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        spellPkt.AddTarget(dispIo.tgt, null, true);
                        dispIo.tgt.AddCondition("sp-Ice Storm Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _ice_storm_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d6110)]
        public static void SolidFogAoEEvent(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var v7 = GameSystems.ParticleSys.CreateAtObj("sp-Solid Fog-hit", v5);
                        spellPkt.AddTarget(dispIo.tgt, v7, true);
                        dispIo.tgt.AddCondition("sp-Solid Fog Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _solid_fog_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100cb5a0)]
        public static void WebOffMovementSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            int v4;

            var dispIo = evt.GetDispIoMoveSpeed();
            var condArg4 = evt.GetConditionArg4();
            var v3 = 10;
            if (condArg4 >= 10)
            {
                v3 = condArg4;
            }

            if (v3 - 10 <= 5)
            {
                v4 = 5;
            }
            else
            {
                if (condArg4 < 10)
                {
                    condArg4 = 10;
                }

                v4 = condArg4 - 10;
            }

            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement)
                && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                dispIo.bonlist.SetOverallCap(1, v4, 0, data2);
                dispIo.bonlist.SetOverallCap(2, v4, 0, data2);
            }
        }


        [DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
        [TempleDllLocation(0x100ca920)]
        public static void sub_100CA920(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var v3 = LocationExtensions.DistanceToObjInFeet(dispIo.attackPacket.attacker, dispIo.attackPacket.victim);
            if (!GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing))
            {
                if (v3 <= 5F)
                {
                    dispIo.bonlist.AddBonus(50, 19, 258);
                }
                else
                {
                    dispIo.bonlist.AddBonus(100, 19, 258);
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100caa70)]
        public static void SpikeGrowthHit(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var v2 = evt.GetDispIoD20Signal().data1;
            if (v2 >= 5)
            {
                var v3 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(v3, out spellPkt))
                {
                    GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
                    GameSystems.SoundGame.PositionalSound(15107, 1, evt.objHndCaller);
                    var v5 = spellPkt.spellId;
                    Dice v6 = new Dice(v2 / 5, 4, 0);
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v6, DamageType.Magic,
                        D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, v5, 0);
                    if (!GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                        SavingThrowType.Reflex, 0, spellPkt.spellId))
                    {
                        if (!evt.objHndCaller.AddCondition("sp-Spike Growth Damage", condArg1, 14400, condArg3))
                        {
                            Logger.Info("d20_mods_spells.c / _spike_growth_hit(): unable to add condition");
                        }
                    }
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100cbab0)]
        [TemplePlusLocation("condition.cpp:501")]
        public static void Spell_remove_mod(in DispatcherCallbackArgs evt, int data)
        {
            DispIoD20Signal evtObj = null;
            if (evt.dispIO != null)
            {
                evtObj = evt.GetDispIoD20Signal();
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Sequence)
            {
                Logger.Warn("Caught a D20DispatcherKey.SIG_Sequence, make sure we are removing spell_mod properly...");
            }

            switch (evt.dispKey)
            {
                case D20DispatcherKey.SIG_Killed:
                case D20DispatcherKey.SIG_Critter_Killed:
                case D20DispatcherKey.SIG_Sequence:
                case D20DispatcherKey.SIG_Spell_Cast:
                case D20DispatcherKey.SIG_Action_Recipient:
                case D20DispatcherKey.SIG_Remove_Concentration:
                case D20DispatcherKey.SIG_TouchAttackAdded:
                case D20DispatcherKey.SIG_Teleport_Prepare:
                case D20DispatcherKey.SIG_Teleport_Reconnect:
                case D20DispatcherKey.SIG_Combat_End:
                    break;
                default:
                    if (evtObj != null && evtObj.data1 != evt.GetConditionArg1())
                    {
                        return;
                    }

                    break;
            }

            var spellId = evt.GetConditionArg1();
            GameSystems.Spell.TryGetActiveSpell(spellId, out var spPkt);

            var conditionName = evt.GetConditionName();
            if (conditionName == SpellConcentrating.condName &&
                evt.dispKey == D20DispatcherKey.SIG_Remove_Concentration)
            {
                if (spPkt.spellEnum != 0)
                {
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 5060); // Stop Concentration
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Concentration_Broken, spellId,
                        0);

                    if (spPkt.caster != null && spPkt.caster != evt.objHndCaller)
                    {
                        GameSystems.D20.D20SendSignal(spPkt.caster, D20DispatcherKey.SIG_Concentration_Broken, spellId,
                            0);
                    }

                    foreach (var target in spPkt.Targets)
                    {
                        if (evt.objHndCaller != target.Object)
                            GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Concentration_Broken,
                                spellId, 0);
                    }

                    // TemplePlus: Concentration_Broken on the spell ObjectHandles.
                    foreach (var spellObj in spPkt.spellObjs)
                    {
                        if (spellObj.obj != evt.objHndCaller)
                        {
                            GameSystems.D20.D20SendSignal(spellObj.obj, D20DispatcherKey.SIG_Concentration_Broken,
                                spellId, 0);
                        }
                    }
                }
            }

            // TODO: This should be moved to ConditionRemove callbacks of the respective conditions!
            if (conditionName == SpellChillTouch.condName
                || conditionName == SpellDispelChaos.condName
                || conditionName == SpellDispelEvil.condName
                || conditionName == SpellDispelGood.condName
                || conditionName == SpellDispelLaw.condName
                || conditionName == SpellGuidance.condName
                || conditionName == SpellProduceFlame.condName
                || conditionName == SpellVampiricTouch.condName)
            {
                GameSystems.Critter.BuildRadialMenu(evt.objHndCaller);
            }

            evt.RemoveThisCondition();
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100c7ec0)]
        public static void GaseousFormAcBonusCapper(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddCap(28, 0, 0xE3);
            dispIo.bonlist.AddCap(9, 0, 0xE3);
        }

        [DispTypes(DispatcherType.ToHitBonusBase)]
        [TempleDllLocation(0x100c7390)]
        [TemplePlusLocation("spell_condition.cpp:80")]
        public static void DivinePowerToHitBonus(in DispatcherCallbackArgs evt, int data1, int bonusMesLine)
        {
            var dispIo = evt.GetDispIoAttackBonus();

            var charLvl = GameSystems.Critter.GetEffectiveLevel(evt.objHndCaller);
            var fighterBab = D20ClassSystem.GetBaseAttackBonus(Stat.level_fighter, charLvl);

            dispIo.bonlist.AddCap(1, 0, bonusMesLine); // caps the initial value to 0

            var overallBon = dispIo.bonlist.OverallBonus;
            if (overallBon < fighterBab)
            {
                fighterBab -= overallBon;
            }
            else
                return; // TemplePlus: fixed vanilla that would cause it to virtually stack with the pre-existing bonus

            // Divine Power BAB bouns type change from 12 to 40 so it stacks with Weapon Enh Bonus but doesn't stack with itself
            dispIo.bonlist.AddBonus(fighterBab, 40, bonusMesLine);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100c9410)]
        public static void ProduceFlameTouchAttackHandler(in DispatcherCallbackArgs evt, int data)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPktBody))
            {
                return;
            }

            var action = (D20Action) evt.GetDispIoD20Signal().obj;
            var target = action.d20ATarget;
            if ((action.d20Caf & D20CAF.HIT) != 0)
            {
                GameSystems.Script.Spells.SpellSoundPlay(spellPktBody, SpellEvent.AreaOfEffectHit);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
                GameSystems.ParticleSys.CreateAtObj("sp-Produce Flame-Hit", target);
                if (D20ModSpells.CheckSpellResistance(target, spellPktBody))
                {
                    return;
                }

                var dice = Dice.D6.WithModifier(spellPktBody.casterLevel);
                if ((action.d20Caf & D20CAF.CRITICAL) != 0)
                {
                    dice = dice.WithCount(2);
                }

                GameSystems.D20.Combat.SpellDamageFull(target, evt.objHndCaller, dice, DamageType.Fire,
                    D20AttackPower.UNSPECIFIED, action.d20ActType, spellPktBody.spellId, 0);
            }
            else
            {
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
            }

            var condArg2 = evt.GetConditionArg2();
            evt.SetConditionArg2(condArg2 - 10);
            spellPktBody.durationRemaining = evt.GetConditionArg2();
            GameSystems.Spell.UpdateSpellPacket(spellPktBody);
            GameSystems.Script.Spells.UpdateSpell(spellPktBody.spellId);
        }


        [DispTypes(DispatcherType.GetCriticalHitRange)]
        [TempleDllLocation(0x100cae70)]
        public static void SpiritualWeapon_Callback23(in DispatcherCallbackArgs evt)
        {
            // TODO: this does nothing
            var dispIo = evt.GetDispIoAttackBonus();
            var v2 = dispIo.attackPacket.GetWeaponUsed();
            if (v2 != null)
            {
                v2.GetInt32(obj_f.weapon_crit_range);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cce70)]
        public static void DeathKnellBegin(in DispatcherCallbackArgs evt)
        {
            var amount = Dice.D8.Roll();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, $"[{amount}] ");
            Logger.Info("d20_mods_spells.c / _begin_death_knell(): gained {0} temporary hit points", amount);

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out _))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_death_knell(): unable to get spell_packet");
                return;
            }

            var condArg2 = evt.GetConditionArg2();
            if (!evt.objHndCaller.AddCondition("Temporary_Hit_Points", spellId, condArg2, amount))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_death_knell(): unable to add condition");
            }
        }

        [DispTypes(DispatcherType.GetCriticalHitExtraDice)]
        [TempleDllLocation(0x100caea0)]
        public static void sub_100CAEA0(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var v2 = dispIo.attackPacket.GetWeaponUsed();
            if (v2 != null)
            {
                v2.GetInt32(obj_f.weapon_crit_hit_chart);
            }
        }


        [DispTypes(DispatcherType.DispelCheck)]
        [TempleDllLocation(0x100db380)]
        public static void BreakEnchantmentDispelCheck(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDispelCheck();
            if ((dispIo.flags & 0x20) != 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(in evt, 0, 0);
                SpellEffects.Spell_remove_mod(in evt, 0);
            }

            if ((dispIo.flags & 0x40) == 0)
            {
                return;
            }

            if (!GameSystems.Spell.TryGetActiveSpell(dispIo.spellId, out var dispelSpellPkt))
            {
                Logger.Info(
                    "d20_mods_spells.c / _break_enchantment_dispel_check(): error getting spellid packet for dispel_packet");
                return;
            }

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                Logger.Info(
                    "d20_mods_spells.c / _break_enchantment_dispel_check(): error getting spellid packet for spell_packet");
                return;
            }

            var bonlist = BonusList.Create();
            bonlist.AddBonus(dispelSpellPkt.casterLevel, 0, 203);
            var dispelSpellName = GameSystems.Spell.GetSpellName(dispelSpellPkt.spellEnum);
            if (GameSystems.Spell.DispelRoll(dispelSpellPkt.caster, bonlist, 0, spellPkt.casterLevel + 11,
                    dispelSpellName) >= 0
                || dispelSpellPkt.caster == spellPkt.caster)
            {
                if ((dispIo.flags & 1) == 0)
                {
                    --dispIo.returnVal;
                }

                if ((dispIo.flags & 1) == 1
                    || (dispIo.flags & 0x40) != 0
                    || (dispIo.flags & 2) != 0 && spellPkt.caster.HasChaoticAlignment()
                    || (dispIo.flags & 4) != 0 && spellPkt.caster.HasEvilAlignment()
                    || (dispIo.flags & 8) != 0 && spellPkt.caster.HasGoodAlignment()
                    || (dispIo.flags & 0x10) != 0 && spellPkt.caster.HasLawfulAlignment())
                {
                    var spellName = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20002, TextFloaterColor.White,
                        suffix: $" [{spellName}]");
                    SpellEffects.Spell_remove_spell(in evt, 0, 0);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                }
            }
            else
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20003, TextFloaterColor.Red);
                GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
            }
        }


        [DispTypes(DispatcherType.GetAttackDice)]
        [TempleDllLocation(0x100c9810)]
        public static void AttackDiceReducePerson(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackDice();

            // 245 : Reduce, 295: Reduce Animal
            if (data2 == 245 && dispIo.weapon == null || data2 == 295 && dispIo.weapon != null)
            {
                return;
            }

            var dice = dispIo.dicePacked;
            switch (dice.Sides)
            {
                case 2:
                    dispIo.dicePacked = dice.WithSides(1);
                    break;
                case 3:
                    dispIo.dicePacked = dice.WithSides(2);
                    break;
                case 4:
                    dispIo.dicePacked = dice.WithSides(3);
                    break;
                case 6:
                    dispIo.dicePacked = dice.WithSides(4);
                    break;
                case 8:
                    dispIo.dicePacked = dice.WithSides(6);
                    break;
                case 10:
                    dispIo.dicePacked = dice.WithSides(8);
                    break;
                case 12:
                    dispIo.dicePacked = dice.WithSides(10);
                    break;
                default:
                    break;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d04c0)]
        public static void BeginSpellWeb(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            evt.SetConditionArg(4, 0);
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = (float) (int) a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 46, 47, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_web(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d6660)]
        [TemplePlusLocation("spell_condition.cpp:348")]
        public static void SpikeStonesHitTrigger(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            var evtId = evt.GetConditionArg3();
            if (dispIo.evtId != evtId)
            {
                return;
            }

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spPkt))
            {
                Logger.Error("SpikeStonesHitTrigger: Could not retrieve spell for spellID {0}", spellId);
                return;
            }

            var tgt = dispIo.tgt;

            SpellPktTriggerAoeHitScript(spPkt.spellId);
            if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spPkt))
            {
                return;
            }

            if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
            {
                var particleId = GameSystems.ParticleSys.CreateAtObj("sp-Spike Stones-HIT", tgt);
                spPkt.AddTarget(tgt, particleId, true);
                tgt.AddCondition("sp-Spike Stones Hit", spellId, spPkt.durationRemaining, evtId);
            }
            else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
            {
                /*
                 * TemplePlus crash fix:
                 * isPerforming() is now retrieving the target's actual action sequence,
                 * rather than "current sequence" (which may be different than the target's
                 * action sequence due to simultaneous actions for several actors)
                 */
                if (GameSystems.D20.Actions.IsCurrentlyPerforming(tgt, out var actSeq))
                {
                    var distTraversed = actSeq.d20ActArray[actSeq.d20aCurIdx].distTraversed;
                    GameSystems.D20.D20SendSignal(tgt, D20DispatcherKey.SIG_Combat_Critter_Moved, (int) (distTraversed),
                        0);
                }

                if (!spPkt.RemoveTarget(evt.objHndCaller))
                {
                    Logger.Error("SpikeStonesHitTrigger: Cannot remove target {0}", tgt);
                    return;
                }

                SpellEffects.Spell_remove_mod(in evt, 0);
            }

            GameSystems.Spell.UpdateSpellPacket(spPkt);
            GameSystems.Script.Spells.UpdateSpell(spPkt.spellId);
        }

        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d36b0)]
        public static void AoeObjEventCloudkill(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var v7 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v5);
                        spellPkt.AddTarget(dispIo.tgt, v7, true);
                        dispIo.tgt.AddCondition("sp-Cloudkill-Damage", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _cloudkill_hit_trigger(): cannot remove target");
                            return;
                        }

                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dbec0)]
        public static void d20_mods_spells__teleport_prepare(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            var objName = GameSystems.MapObject.GetDisplayName(evt.objHndCaller);
            var spellName = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
            Logger.Info("preparing spell=( {0} ) on obj=( {1} ) for teleport", spellName, objName);

            if (!GameSystems.Party.IsInParty(spellPkt.caster))
            {
                Logger.Info("ending spell=( {0} ) on obj=( {1} ) because caster is not in party!", spellName, objName);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                return;
            }

            if (!GameSystems.Party.IsInParty(evt.objHndCaller))
            {
                // TODO: Uhm... This seems broken!
                if (spellPkt.Targets.Length <= 1)
                {
                    Logger.Info("ending spell=( {0} ) on obj=( {1} ) because target is not in party!", spellName,
                        objName);
                    SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                }
                else
                {
                    Logger.Info(
                        "processing spell=( {0} ), removing obj=( {1} ) from target_list because target is not in party!",
                        spellName, objName);
                    spellPkt.RemoveTarget(evt.objHndCaller);
                }
            }

            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ddd20)]
        public static void TrueStrikeAttackBonus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(data1, 18, data2);
            if ((dispIo.attackPacket.flags & D20CAF.FINAL_ATTACK_ROLL) != 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d03b0)]
        public static void sub_100D03B0(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20026, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c6020)]
        public static void AnimalGrowthDamageResistance(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddPhysicalDR(10, D20AttackPower.MAGIC, 104);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf880)]
        public static void SleepOnAdd(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20004, TextFloaterColor.Red);
            evt.objHndCaller.AddCondition(StatusEffects.Prone);
            // TODO: Shouldn't the animation be added in condadd of prone?
            GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.Falldown);
            if (!evt.objHndCaller.AddCondition("Sleeping", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_sleep(): unable to add condition");
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100c6530)]
        public static void CallLightningRadial(in DispatcherCallbackArgs evt, int data)
        {
            var helpId = GameSystems.Spell.GetSpellHelpTopic(46);
            var radMenuEntry = RadialMenuEntry.CreateAction(108, D20ActionType.SPELL_CALL_LIGHTNING, 0, helpId);
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                RadialMenuStandardNode.Spells);
        }

        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100c85b0)]
        public static void GustOfWindTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            var size = (SizeCategory) evt.objHndCaller.GetStat(Stat.size);
            if (size >= SizeCategory.Large)
            {
                return;
            }

            if (size < SizeCategory.Medium)
            {
                if (size < SizeCategory.Small)
                {
                    // TODO: This is wrong... It should be 1d4 per 10 feet rolled, and it rolls 1d4*10 feet, so this is taking the actual damage * 10
                    // TODO: Also, it should be subdual damage
                    var distanceRolled = Dice.D4.Roll();
                    var dice = new Dice(10 * distanceRolled, 4, 0);
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, dice,
                        DamageType.Bludgeoning,
                        D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spellId, 0);
                }

                evt.objHndCaller.AddCondition(StatusEffects.Prone);
                GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.Falldown);
            }

            // TODO Setting it to empty doesnt seem like the rules...
            // TODO Rules say: Medium creatures are unable to move forward against the force of the wind, [...], not
            // TODO that they lose their entire turn
            dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
            dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
        }


        [DispTypes(DispatcherType.SpellResistanceMod)]
        [TempleDllLocation(0x100d2e50)]
        public static void SpellResistanceMod_ProtFromMagic_Callback(in DispatcherCallbackArgs evt, int data)
        {
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIOBonusListAndSpellEntry();
            dispIo.bonList.AddBonus(condArg3, 36, 203);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf260)]
        public static void RayOfEnfeeblementOnAdd(in DispatcherCallbackArgs evt)
        {
            var amount = evt.GetConditionArg3();
            var strengthLabel = GameSystems.Stat.GetStatName(0);
            var spellName = GameSystems.Spell.GetSpellName(25013); // TODO: dont abuse this function for this shit
            var text = $"{spellName} [{strengthLabel}: {amount}]";
            GameSystems.RollHistory.CreateFromFreeText(text);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc6e0)]
        public static void BeginSpellCloudkill(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                return;
            }

            GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out var entry);
            var radiusInches = entry.radiusTarget * locXY.INCH_PER_FEET;
            spellPkt.aoeObj = evt.objHndCaller;
            var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 0, 1, ObjectListFilter.OLC_CRITTERS,
                radiusInches);
            evt.SetConditionArg3(v3);
            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ca4d0)]
        public static void sub_100CA4D0(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20039, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cec40)]
        public static void MirrorImageAdd(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            spellPkt.ClearTargets();

            // TODO: bad hackery...
            var imageCount = evt.GetConditionArg3();
            spellPkt.Targets = new SpellTarget[imageCount];
            Array.Fill(spellPkt.Targets, new SpellTarget(evt.objHndCaller, null));

            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
        }


        [DispTypes(DispatcherType.SpellResistanceMod)]
        [TempleDllLocation(0x100caa30)]
        public static void SpellResistanceMod_spSpellResistance_Callback(in DispatcherCallbackArgs evt, int data)
        {
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIOBonusListAndSpellEntry();
            dispIo.bonList.AddBonus(condArg3, 36, 203);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c7860)]
        public static void sub_100C7860(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (evt.GetConditionArg3() == 9)
            {
                dispIo.return_val = 1;
            }
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100c88a0)]
        public static void HeatMetalTurnBasedStatusInit(in DispatcherCallbackArgs evt, int data)
        {
            var spellId = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                if (evt.GetConditionArg3() > 0)
                {
                    var condArg3 = evt.GetConditionArg3();
                    var dice = Dice.Constant(condArg3);
                    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, dice, DamageType.Fire,
                        D20AttackPower.UNSPECIFIED,
                        D20ActionType.CAST_SPELL, spellPkt.spellId, 0);
                }

                switch (spellPkt.durationRemaining)
                {
                    case 2:
                    case 6:
                        evt.SetConditionArg3(Dice.D4.Roll());
                        break;
                    case 3:
                    case 4:
                    case 5:
                        evt.SetConditionArg3(new Dice(2, 4).Roll());
                        break;
                    default:
                        evt.SetConditionArg3(0);
                        return;
                }
            }
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100dd4d0)]
        public static void ProtectionFromArrowsTakingDamage(in DispatcherCallbackArgs evt, int amount)
        {
            int v8;
            SpellPacketBody spellPkt;
            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Arrows-Hit", evt.objHndCaller);
            var dispIo = evt.GetDispIoDamage();
            var v11 = dispIo;
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
                {
                    DamagePacket v4 = dispIo.damage;
                    var v5 = v4.GetOverallDamageByType();
                    if (v5 <= evt.GetConditionArg3())
                    {
                        v4.AddPhysicalDR(amount, D20AttackPower.MAGIC, 0x68);
                    }
                    else
                    {
                        var condArg3 = evt.GetConditionArg3();
                        v4.AddPhysicalDR(condArg3, D20AttackPower.MAGIC, 0x68);
                    }

                    var v7 = v4.GetOverallDamageByType();
                    v11.damage.finalDamage = v7;
                    if (v5 > evt.GetConditionArg3())
                    {
                        v8 = -v11.damage.finalDamage;
                    }
                    else
                    {
                        v8 = v7 - v5 + evt.GetConditionArg3();
                    }

                    evt.SetConditionArg3(v8);
                    Logger.Info("absorbed {0} points of damage, DR points left: {1}", v5 - v7, v8);
                    if (v8 <= 0)
                    {
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(in evt, 0, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }
                    else
                    {
                        var suffix = $" {v8} ({amount}/{4:+#;-#;0})";
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20009, TextFloaterColor.White, suffix:suffix);
                    }
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c6200)]
        public static void sub_100C6200(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(-data1, 0, data2);
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100d2b20)]
        public static void PotionOfHidingSneaking(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddBonus(10, 21, 113);
        }


        [DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
        [TempleDllLocation(0x100c5be0)]
        public static void sub_100C5BE0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (!GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing))
            {
                dispIo.bonlist.AddBonus(data1, 19, data2);
            }
        }


        [DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
        [TempleDllLocation(0x100cb850)]
        public static void WindWall_Concealment_Chance(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                dispIo.bonlist.AddBonus(data1, 19, data2);
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d5f60)]
        public static void sub_100D5F60(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var v7 = GameSystems.ParticleSys.CreateAtObj("sp-Soften Earth-hit", v5);
                        spellPkt.AddTarget(dispIo.tgt, v7, true);
                        dispIo.tgt.AddCondition("sp-Soften Earth and Stone Hit", spellPkt.spellId,
                            spellPkt.durationRemaining, dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info(
                                "d20_mods_spells.c / _soften_earth_and_stone_hit_trigger(): cannot remove target");
                            return;
                        }
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cbd60)]
        [TemplePlusLocation("condition.cpp:3588")]
        public static void SpellAddDismissCondition(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var spellId_1 = evt.GetConditionArg1();
                if (!spellPkt.caster.AddCondition("Dismiss", spellId_1, 0, 0))
                {
                    Logger.Info("d20_mods_spells.c / _begin_spell_dismiss(): unable to add condition");
                }
            }
            else
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_dismiss(): unable to get spell_packet");
            }
        }
/* Orphan comments:
TP Replaced @ condition.cpp:3588
*/


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c6f70)]
        public static void sub_100C6F70(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            dispIo.data1 = data1;
            dispIo.data2 = 0;
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100ca740)]
        public static void SlowTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                if (dispIo.tbStatus.hourglassState < HourglassState.STD)
                {
                    dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
                }
                else
                {
                    dispIo.tbStatus.hourglassState = HourglassState.STD;
                }
                dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            }
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c87e0)]
        public static void HeatMetalDamageResistance(in DispatcherCallbackArgs evt)
        {
            int v6;
            int v7;

            var condArg3 = evt.GetConditionArg3();
            if (condArg3 <= 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoDamage();
            var coldDamage = 0;
            foreach (var damageDice in dispIo.damage.dice)
            {
                if (damageDice.type == DamageType.Cold)
                {
                    coldDamage += damageDice.rolledDamage;
                }
            }

            if (coldDamage <= condArg3)
            {
                v7 = condArg3 - coldDamage;
                v6 = 0;
            }
            else
            {
                v6 = coldDamage - condArg3;
                v7 = 0;
            }

            if (coldDamage > 0)
            {
                DamagePacket v8 = dispIo.damage;
                if (dispIo.damage.GetOverallDamageByType() > 0)
                {
                    v8.AddDR(coldDamage - v6, DamageType.Cold, 104);
                    dispIo.damage.finalDamage = v8.GetOverallDamageByType();
                    evt.SetConditionArg3(v7);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce800)]
        public static void MagicMissileOnAdd(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out spellPkt))
            {
                return;
            }

            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                WellKnownSpells.Shield))
            {
                GameSystems.Spell.PlayFizzle(evt.objHndCaller);
            }
            else
            {
                var dice = new Dice(1, 4, 1);
                GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, dice, DamageType.Force,
                    D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spellId, 0);
            }
            SpellEffects.Spell_remove_mod(in evt, 0);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d3430)]
        [TemplePlusLocation("condition.cpp:503")]
        public static void AoESpellRemove(in DispatcherCallbackArgs evt, int data)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var pkt))
            {
                return;
            }

            // Added in Temple+: fixes where failed Dispel Magic still causes AoE spells to stop (but without removing their effects! Which caused effect permanency. E.g. Lareth fight in Co8)
            if (evt.dispKey == D20DispatcherKey.SIG_Spell_End && evt.dispIO is DispIoD20Signal dispIo) {
                var spellIdFromSignal = dispIo.data1;
                if (spellIdFromSignal != 0 && spellIdFromSignal != spellId) {
                    return;
                }
            }

            switch (data) {
                case 38:
                    GameSystems.ParticleSys.CreateAtObj("sp-consecrate-END", evt.objHndCaller);
                    break;
                case 53:
                    GameSystems.ParticleSys.CreateAtObj("sp-Desecrate-END", evt.objHndCaller);
                    break;
                case 102:
                    GameSystems.ParticleSys.CreateAtObj("sp-Fog Cloud-END", evt.objHndCaller);
                    break;
                case 139:
                    GameSystems.ParticleSys.CreateAtObj("sp-Invisibility Sphere-END", evt.objHndCaller);
                    break;
                case 157:
                    GameSystems.ParticleSys.CreateAtObj("sp-Minor Globe of Invulnerability-END", evt.objHndCaller);
                    break;
                case 159:
                    GameSystems.ParticleSys.CreateAtObj("sp-Mind Fog-END", evt.objHndCaller);
                    break;
                case 210:
                    GameSystems.ParticleSys.CreateAtObj("sp-Solid Fog-END", evt.objHndCaller);
                    break;
                case 237:
                    GameSystems.ParticleSys.CreateAtObj("sp-Wind Wall-END", evt.objHndCaller);
                    break;
                default:
                    break;
            }

            pkt.EndPartSysForSpellObjects();

            var evtId = evt.GetConditionArg3();
            GameSystems.ObjectEvent.Remove(evtId);
            SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100c90a0)]
        public static void MirrorImageTooltipCallback(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoTooltip();
            var meslineKey = data;
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
                var condArg3 = evt.GetConditionArg3();
                dispIo.Append($"{meslineValue} [{condArg3}]");
            }
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c7530)]
        public static void EndureElementsDamageResistance(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var endureType = evt.GetConditionArg3();
            if (!TryGetDamageTypeForResistanceSpell(endureType, out var damType))
            {
                return;
            }

            var condArg4 = evt.GetConditionArg4();
            if (condArg4 <= 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoDamage();

            // Make sure endure elements doesn't stack with protection from elements / resist elements
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Protection_From_Elements))
            {
                // TODO: This is btw just wrong because it doesnt manage multiple resistance spells being active
                var otherResistanceType = (int) GameSystems.D20.D20QueryReturnData(evt.objHndCaller,
                    D20DispatcherKey.QUE_Critter_Has_Protection_From_Elements);
                if (otherResistanceType == endureType)
                {
                    return;
                }
            }
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Resist_Elements))
            {
                // TODO: This is btw just wrong because it doesnt manage multiple resistance spells being active
                var otherResistanceType = (int) GameSystems.D20.D20QueryReturnData(evt.objHndCaller,
                    D20DispatcherKey.QUE_Critter_Has_Resist_Elements);
                if (otherResistanceType == endureType)
                {
                    return;
                }
            }

            var damPkt = dispIo.damage;
            if (!damPkt.HasDamageOfType(damType))
            {
                return;
            }

            var damAmt = damPkt.GetOverallDamageByType(damType);
            if (damAmt > 0)
            {
                int remainingDr;
                if (damAmt <= condArg4)
                {
                    damPkt.AddDR(damAmt, damType, 104);
                    remainingDr = condArg4 - damAmt;
                }
                else
                {
                    damPkt.AddDR(condArg4, damType, 104);
                    remainingDr = 0;
                }

                var newOverallDamage = damPkt.GetOverallDamageByType();
                dispIo.damage.finalDamage = newOverallDamage;
                evt.SetConditionArg4(remainingDr);
                Logger.Info("absorbed {0} points of [{1}] damage, DR points left: {2}", damAmt - newOverallDamage,
                    damType, remainingDr);
                var suffix = $" {remainingDr} ({data1}/{data2:+#;-#;0})";
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20025, TextFloaterColor.White, suffix:suffix);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c8570)]
        public static void sub_100C8570(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                dispIo.bonlist.AddBonus(-data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100dd6b0)]
        public static void ProtFromElementsDamageResistance(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();

            var protectionType = evt.GetConditionArg3();
            if (!TryGetDamageTypeForResistanceSpell(protectionType, out var damType))
            {
                return;
            }

            var damPkt = dispIo.damage;
            if (!damPkt.HasDamageOfType(damType))
            {
                return;
            }

            var currentAbsorption = evt.GetConditionArg4();
            if (currentAbsorption <= 0)
            {
                return;
            }

            var damOfType = damPkt.GetOverallDamageByType(damType);
            if (damOfType > 0)
            {
                int remainingAbsorption;
                if (damOfType <= currentAbsorption)
                {
                    damPkt.AddDR(damOfType, damType, 104);
                    remainingAbsorption = currentAbsorption - damOfType;
                }
                else
                {
                    damPkt.AddDR(currentAbsorption, damType, 104);
                    remainingAbsorption = 0;
                }

                var newDmgTotal = damPkt.GetOverallDamageByType();
                dispIo.damage.finalDamage = newDmgTotal;
                evt.SetConditionArg4(remainingAbsorption);
                var condArg3 = evt.GetConditionArg3();
                Logger.Info("absorbed {0} points of [{1}] damage, DR points left: {2}", damOfType - newDmgTotal,
                    damType, remainingAbsorption);
                if (remainingAbsorption <= 0)
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                    SpellEffects.Spell_remove_spell(in evt, 0, 0);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                }
                else
                {
                    var suffix = $" {remainingAbsorption} ({data1}/{data2:+#;-#;0})";
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20025, TextFloaterColor.White, suffix:suffix);
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c6f10)]
        public static void sub_100C6F10(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(-data1, 17, data2);
        }


        [DispTypes(DispatcherType.Unused63)]
        [TempleDllLocation(0x100c8f90)]
        public static void MinorGlobeCallback3F(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoTypeImmunityTrigger();
            var spellId = dispIo.spellId;
            if (spellId != evt.GetConditionArg1()
                && GameSystems.Spell.TryGetActiveSpell(spellId, out spellPkt) && spellPkt.spellKnownSlotLevel < 4
                && dispIo.field_C != 48
                && spellPkt.spellEnum != WellKnownSpells.BestowCurse)
            {
                dispIo.interrupt = 1;
                dispIo.val2 = 10;
                dispIo.spellId = evt.GetConditionArg1();
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c7950)]
        public static void EnlargeStatLevelGet(in DispatcherCallbackArgs evt, Stat attribute, int bonusMesLine)
        {
            var dispIo = evt.GetDispIoBonusList();
            var spellId = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out _))
            {
                var queryAttribute = evt.GetAttributeFromDispatcherKey();
                if (queryAttribute == attribute)
                {
                    int amount;
                    if (attribute == Stat.strength)
                    {
                        amount = 2;
                    }
                    else if (attribute == Stat.dexterity)
                    {
                        amount = -2;
                    }
                    else
                    {
                        return;
                    }

                    dispIo.bonlist.AddBonus(amount, 20, bonusMesLine);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf110)]
        public static void RaiseDeadOnConditionAdd(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                if (evt.objHndCaller.GetStat(Stat.hp_current) > -10)
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30016, TextFloaterColor.White);
                    GameSystems.Spell.PlayFizzle(evt.objHndCaller);
                }
                else
                {
                    var partSys = GameSystems.ParticleSys.CreateAtObj("sp-Raise Dead", evt.objHndCaller);
                    spellPkt.AddTarget(evt.objHndCaller, partSys, true);

                    if (Resurrection.Resurrect(evt.objHndCaller, ResurrectionType.RaiseDead))
                    {
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20037, TextFloaterColor.White);
                        GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.Getup);
                    }
                    else
                    {
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20036, TextFloaterColor.Red);
                    }
                }
            }

            SpellEffects.Spell_remove_mod(in evt, 0);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ca9f0)]
        public static void QueryHasSpellResistance(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoD20Query();
            dispIo.data1 = condArg3;
            dispIo.return_val = 1;
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d5950)]
        public static void sub_100D5950(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var spellId = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript /*0x100c37d0*/(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        if (GameSystems.Critter.IsCategory(dispIo.tgt, MonsterCategory.animal) &&
                            dispIo.tgt.GetStat(Stat.size) < 6)
                        {
                            spellPkt.AddTarget(dispIo.tgt, null, true);
                            dispIo.tgt.AddCondition("sp-Repel Vermin Hit", spellPkt.spellId, spellPkt.durationRemaining,
                                dispIo.evtId);
                        }
                        else
                        {
                            var v7 = dispIo.tgt;
                            GameSystems.ParticleSys.CreateAtObj("Fizzle", v7);
                        }
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _repel_vermin_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100c7140)]
        public static void sub_100C7140(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (evt.dispKey == D20DispatcherKey.D20A_FALL_TO_PRONE)
            {
                dispIo.bonOut.AddBonus(data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100c7360)]
        public static void AddBonusType17(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddDamageBonus(data1, 17, data2);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c7820)]
        public static void sub_100C7820(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            dispIo.obj = evt.GetConditionArg3();
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d3100)]
        [TemplePlusLocation("condition.cpp:3526")]
        public static void OnSequenceConcentrating(in DispatcherCallbackArgs evt, int data)
        {

            var dispIo = evt.GetDispIoD20Signal();
            var actSeq = (ActionSequence)dispIo.obj;
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                Logger.Debug("Cannot fetch spell packet");
                return;
            }

            if (spellPkt.spellEnum == 303) { // Meld Into Stone hardcoding...
                return;
            }

            foreach (var d20a in actSeq.d20ActArray)
            {
                var actionFlags = d20a.GetActionDefinitionFlags();
                if ((actionFlags & D20ADF.D20ADF_Breaks_Concentration) == 0)
                {
                    continue;
                }

                if (d20a.d20ActType == D20ActionType.CAST_SPELL && d20a.spellId == spellId)
                {
                    break;
                }

                // TemplePlus: free actions won't take up your standard action
                if ((d20a.d20Caf & D20CAF.FREE_ACTION) != 0)
                {
                    continue;
                }

                var dca = new DispatcherCallbackArgs(evt.subDispNode, evt.objHndCaller, DispatcherType.D20Signal,
                    D20DispatcherKey.SIG_Remove_Concentration, null);
                SpellEffects.Spell_remove_mod(dca, 0);
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100c8240)]
        public static void GoodberryTallyPreAdd(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoCondStruct();
            dispIo.outputFlag = (condArg3 <= 8);
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100c8d60)]
        [TemplePlusLocation("spell_condition.cpp:77")]
        public static void MagicCircleTakingDamage(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();

            // fix - sometimes there was no attacker...
            var attacker = dispIo.attackPacket.attacker;
            if (attacker == null)
            {
                return;
            }

            if (!IsUsingNaturalAttacks(dispIo.attackPacket))
            {
                return;
            }

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                Logger.Info("d20_mods_spells.c / _magic_circle_prevent_damage(): unable to retrieve spell_packet");
                return;
            }

            if (!DoesAlignmentProtectionApply(attacker, spellPkt.spellEnum))
            {
                return;
            }

            if (!attacker.HasCondition(SpellEffects.SpellSummoned))
            {
                return;
            }

            if (!D20ModSpells.CheckSpellResistance(evt.objHndCaller, spellPkt))
            {
                dispIo.damage.AddModFactor(0F, DamageType.Unspecified, 104);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c9280)]
        public static void sub_100C9280(in DispatcherCallbackArgs evt, int data)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = data;
            var v3 = condArg3;
            if (v3 != -3)
            {
                v2 = -v2;
            }

            evt.GetDispIoAttackBonus().bonlist.AddBonus(v2, 14, 151);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cdeb0)]
        public static void HarmOnAdd(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            var damageAmount = 10 * spellPkt.casterLevel;
            var v3 = evt.objHndCaller.GetStat(Stat.hp_current);
            if (10 * spellPkt.casterLevel >= v3)
            {
                var v4 = evt.objHndCaller.GetStat(Stat.hp_current);
                damageAmount = v4 - 1;
            }

            Dice dice;
            if (GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                SavingThrowType.Will, 0, spellPkt.spellId))
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
                GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
                evt.SetConditionArg3(1);
                dice = Dice.Constant(damageAmount / 2);
            }
            else
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
                dice = Dice.Constant(damageAmount);
            }

            GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, dice, DamageType.NegativeEnergy,
                D20AttackPower.MAGIC, D20ActionType.CAST_SPELL, spellId, 0);
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c66d0)]
        public static void ChillMetalDamageResistance(in DispatcherCallbackArgs evt)
        {
            var currentBudget = evt.GetConditionArg3();
            if (currentBudget <= 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoDamage();
            var dmgOfType = 0;
            foreach (var damageDice in dispIo.damage.dice)
            {
                if (damageDice.type == DamageType.Fire)
                {
                    dmgOfType += damageDice.rolledDamage;
                }
            }

            if (dmgOfType <= 0)
            {
                return;
            }

            int leftOverDamage;
            int remainingBudget;
            if (dmgOfType <= currentBudget)
            {
                remainingBudget = currentBudget - dmgOfType;
                leftOverDamage = 0;
            }
            else
            {
                leftOverDamage = dmgOfType - currentBudget;
                remainingBudget = 0;
            }

            if (dispIo.damage.GetOverallDamageByType() > 0)
            {
                dispIo.damage.AddDR(dmgOfType - leftOverDamage, DamageType.Fire, 104);
                dispIo.damage.finalDamage = dispIo.damage.GetOverallDamageByType();
                evt.SetConditionArg3(remainingBudget);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ceef0)]
        public static void D20ModsSpells_ProtectionElementsDamageReductionRestore(in DispatcherCallbackArgs evt)
        {
            int condArg3;
            int condArg2;
            int v5;
            GameObjectBody v6;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var v2 = 12 * spellPkt.casterLevel;
                if (12 * spellPkt.casterLevel > 120)
                {
                    v2 = 120;
                }

                evt.SetConditionArg4(v2);
                switch (evt.GetConditionArg2())
                {
                    case 1:
                        v6 = evt.objHndCaller;
                        v5 = 13384;
                        goto LABEL_10;
                    case 3:
                        v6 = evt.objHndCaller;
                        v5 = 13386;
                        goto LABEL_10;
                    case 6:
                        v6 = evt.objHndCaller;
                        v5 = 13392;
                        goto LABEL_10;
                    case 9:
                        v6 = evt.objHndCaller;
                        v5 = 13388;
                        goto LABEL_10;
                    case 16:
                        v6 = evt.objHndCaller;
                        v5 = 13390;
                        LABEL_10:
                        GameSystems.SoundGame.PositionalSound(v5, 1, v6);
                        condArg3 = evt.GetConditionArg3();
                        condArg2 = evt.GetConditionArg2();
                        evt.SetConditionArg3(condArg2);
                        evt.SetConditionArg2(condArg3);
                        break;
                    default:
                        return;
                }
            }
            else
            {
                Logger.Info(
                    "d20_mods_spells.c / _protection_elements_damage_reduction_restore(): unable to get spell_packet");
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ca7a0)]
        public static void sub_100CA7A0(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) &&
                !GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                    SavingThrowType.Reflex, 0, spellPkt.spellId))
            {
                int v10 = new Dice(1, 2).Roll();
                if (!evt.objHndCaller.AddCondition("sp-Soften Earth and Stone Hit Save Failed", spellPkt.spellId, v10,
                    0))
                {
                    Logger.Info("d20_mods_spells.c / _soften_earth_and_stone_hit(): unable to add condition");
                }
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c9790)]
        public static void AddBonusType13(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(data1, 13, data2);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100c71f0)]
        public static void d20_mods_spells__desecrate_undead_temp_hp(in DispatcherCallbackArgs evt, int data1,
            int data2)
        {
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt) &&
                evt.objHndCaller.HasCondition(SpellEffects.SpellSummoned))
            {
                var v2 = GameSystems.Critter.GetHitDiceNum(evt.objHndCaller) * data1;
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, $"[{v2}] ");
                Logger.Info("d20_mods_spells.c / _desecrate_undead_temp_hp(): gained {0} temporary hit points", v2);
                var condArg2 = evt.GetConditionArg2();
                if (!evt.objHndCaller.AddCondition("Temporary_Hit_Points", spellPkt.spellId, condArg2, v2))
                {
                    Logger.Info("d20_mods_spells.c / _desecrate_undead_temp_hp(): unable to add condition");
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c60b0)]
        public static void RighteousMightToHitBonus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(data1, 35, data2);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce8d0)]
        public static void MeldIntoStoneBeginSpell(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (!spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0))
                {
                    Logger.Info(
                        "d20_mods_spells.c / _begin_spell_meld_into_stone(): unable to add condition to spell_caster");
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ddda0)]
        public static void VampiricTouchSignalTouchAttack(in DispatcherCallbackArgs evt, int data)
        {
            int v8;
            int v9;
            SpellPacketBody spellPkt;

            var v1 = (D20Action) evt.GetDispIoD20Signal().obj;
            if ((v1.d20Caf & D20CAF.HIT) != 0)
            {
                var condArg1 = evt.GetConditionArg1();
                GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt);
                GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.AreaOfEffectHit);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
                if (D20ModSpells.CheckSpellResistance(v1.d20ATarget, spellPkt))
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                    SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                }
                else
                {
                    if ((v1.d20Caf & D20CAF.CRITICAL) != 0)
                    {
                        v9 = spellPkt.casterLevel;
                        v8 = 2;
                    }
                    else
                    {
                        v9 = spellPkt.casterLevel;
                        v8 = 1;
                    }

                    var v5 = new Dice(v8, 6, v9);
                    GameSystems.D20.Combat.SpellDamageFull(v1.d20ATarget, evt.objHndCaller, v5, DamageType.Magic, D20AttackPower.UNSPECIFIED,
                        D20ActionType.CAST_SPELL, spellPkt.spellId, 0);
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                    SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                }
            }
            else
            {
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100cb1f0)]
        public static void sub_100CB1F0(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                    SavingThrowType.Will, 0, spellPkt.spellId))
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
                    GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
                    evt.SetConditionArg3(0);
                    GameSystems.AI.AiProcess(evt.objHndCaller);
                }
                else
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
                    evt.SetConditionArg3(1);
                }
            }
        }


        [DispTypes(DispatcherType.EffectTooltip)]
        [TempleDllLocation(0x100c3f20)]
        public static void EffectTooltipBestowCurse(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoEffectTooltip();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var condArg3 = evt.GetConditionArg3();
                var v4 = GameSystems.D20.RadialMenu.GetAbilityReducedName(condArg3 + 161);
                var v5 = spellPkt.duration;
                var v6 = spellPkt.durationRemaining;
                var v7 = v4;
                var v8 = GameSystems.D20.Combat.GetCombatMesLine(0xAF);
                var text = $"({v7}) {v8}: {v6}/{v5}";
                dispIo.bdb.AddEntry(data, text, spellPkt.spellEnum);
            }
        }


        [DispTypes(DispatcherType.GetAttackerConcealmentMissChance)]
        [TempleDllLocation(0x100c62a0)]
        public static void sub_100C62A0(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddBonus(data, 19, 254);
        }


        [DispTypes(DispatcherType.AbilityCheckModifier)]
        [TempleDllLocation(0x100c9060)]
        public static void sub_100C9060(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoBonusList();
            if (evt.dispKey == D20DispatcherKey.STAT_WISDOM)
            {
                dispIo.bonlist.AddBonus(-data1, 34, data2);
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100c8530)]
        public static void sub_100C8530(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (evt.dispKey == D20DispatcherKey.D20A_DOUBLE_MOVE)
            {
                dispIo.bonOut.AddBonus(-data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100cb7d0)]
        public static void WebOnBurningCallback(in DispatcherCallbackArgs evt, int data)
        {
            var spellId = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                var dispIo = evt.GetDispIoDamage();
                if (dispIo.damage.GetOverallDamageByType(DamageType.Fire) > 0)
                {
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Web_Burning, spellPkt.spellId,
                        0);
                }
            }
        }


        [DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
        [TempleDllLocation(0x100c7a00)]
        public static void sub_100C7A00(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                GameSystems.ParticleSys.CreateAtObj("sp-Entropic Shield-HIT", evt.objHndCaller);
                dispIo.bonlist.AddBonus(data1, 19, data2);
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100c4e50)]
        public static void EmotionSkillBonus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            int v3;

            var dispIo = evt.GetDispIoObjBonus();
            switch (data2)
            {
                case 0xA9:
                    v3 = -data1;
                    goto LABEL_11;
                case 0xAC:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
                        && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear))
                    {
                        dispIo.bonOut.AddBonus(-data1, 13, data2);
                    }

                    break;
                case 0x103:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions))
                    {
                        dispIo.bonOut.AddBonus(-data1, 13, data2);
                    }

                    break;
                case 0x104:
                case 0x12A:
                case 0x12B:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions))
                    {
                        dispIo.bonOut.AddBonus(data1, 13, data2);
                    }

                    break;
                default:
                    v3 = data1;
                    LABEL_11:
                    dispIo.bonOut.AddBonus(v3, 13, data2);
                    break;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d0730)]
        public static void FrogTongueOnAdd(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            if (spellPkt.Targets.Length == 0)
            {
                return;
            }

            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious))
            {
                var tgt = spellPkt.Targets[0].Object;
                if (!tgt.AddCondition("Grappled", spellId, 1, 0))
                {
                    Logger.Info("d20_mods_spells.c / _begin_spell_frog_tongue(): unable to add condition");
                }

                if (!tgt.AddCondition("sp-Frog Tongue Grappled", spellId, 0, 0))
                {
                    Logger.Info("d20_mods_spells.c / _begin_spell_frog_tongue(): unable to add condition");
                }

                FrogGrappleController.PlayLatch(evt.objHndCaller);
                GameSystems.D20.Actions.PerformOnAnimComplete(evt.objHndCaller, -1);
                if (!evt.objHndCaller.AddCondition("Grappled", spellId, 0, 0))
                {
                    Logger.Info("d20_mods_spells.c / _begin_spell_frog_tongue(): unable to add condition");
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cfd50)]
        public static void sub_100CFD50(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20034, TextFloaterColor.White, suffix:$" [{condArg3}]");
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100de090)]
        public static void FrogTongueHpChanged(in DispatcherCallbackArgs evt, int condType)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            if (spellPkt.caster != evt.objHndCaller)
            {
                return;
            }

            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious))
            {
                GameSystems.D20.Actions.PerformOnAnimComplete(evt.objHndCaller, -1);
                FrogGrappleController.PlayRetractTongue(spellPkt.caster);

                spellPkt.caster.SetAnimId(spellPkt.caster.GetIdleAnimId());
                evt.objHndCaller.SetAnimId(evt.objHndCaller.GetIdleAnimId());

                FrogGrappleEnding(spellPkt, evt.objHndCaller);

                if (spellPkt.Targets.Length > 0)
                {
                    var target = spellPkt.Targets[0].Object;
                    GameSystems.D20.D20SendSignal(target,
                        D20DispatcherKey.SIG_Spell_Grapple_Removed, spellId, 0);
                    GameSystems.D20.D20SendSignal(target, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                }

                GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed,
                    spellPkt.spellId, 0);
                GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                    0);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100ca4a0)]
        public static void sub_100CA4A0(in DispatcherCallbackArgs evt)
        {
            GameSystems.ParticleSys.CreateAtObj("sp-Shield-hit", evt.objHndCaller);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc520)]
        public static void BeginSpellCharmMonster(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            if (!evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_charm_monster(): unable to add condition");
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.GetSizeCategory)]
        [TempleDllLocation(0x100c97f0)]
        public static void sub_100C97F0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoD20Query();
            var v2 = dispIo.return_val;
            if (v2 > 1)
            {
                dispIo.return_val = v2 - 1;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf560)]
        public static void SpRestorationOnConditionAdd(in DispatcherCallbackArgs evt)
        {
            // Heal all temporary attribute damage
            for (var attribute = Stat.strength; attribute <= Stat.charisma; attribute++)
            {
                var tempDispIo = new DispIoAbilityLoss();
                tempDispIo.statDamaged = attribute;
                tempDispIo.flags |= 0x19;
                tempDispIo.fieldC = 1;
                tempDispIo.spellId = evt.GetConditionArg1();
                tempDispIo.result = 0;
                var damageAmount = - evt.objHndCaller.DispatchGetAbilityLoss(tempDispIo);
                if (damageAmount > 0)
                {
                    var statName = GameSystems.Stat.GetStatName(tempDispIo.statDamaged);
                    Logger.Info(
                        "Healed {0} points of temporary ({1}) damage",
                        damageAmount, statName);
                    var suffix = $": {statName} [{damageAmount}]";
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20035, TextFloaterColor.White, suffix:suffix);
                }
            }

            // Heal permanent damage for the selected attribute
            var dispIo = new DispIoAbilityLoss();
            dispIo.statDamaged = (Stat) evt.GetConditionArg3();
            dispIo.flags |= 0x1A;
            dispIo.fieldC = 1;
            dispIo.spellId = evt.GetConditionArg1();
            dispIo.result = 0;
            var permDamage = - evt.objHndCaller.DispatchGetAbilityLoss(dispIo);
            if (permDamage > 0)
            {
                var statName = GameSystems.Stat.GetStatName(dispIo.statDamaged);
                Logger.Info("Healed {0} points of permanent({1}) damage", permDamage, statName);
                var suffix = $": {statName} [{permDamage}]";
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20035, TextFloaterColor.White, suffix:suffix);
            }

            // Restore one drained level of experience
            var shouldHaveLevels = evt.objHndCaller.GetArrayLength(obj_f.critter_level_idx);
            var currentXp = evt.objHndCaller.GetInt32(obj_f.critter_experience);
            int nextLevelForXp = GameSystems.Level.GetNextLevelForXp(currentXp);
            if (nextLevelForXp < shouldHaveLevels)
            {
                var newXp = GameSystems.Level.GetExperienceForLevel(nextLevelForXp + 1);
                evt.objHndCaller.SetInt32(obj_f.critter_experience, newXp);

                GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Experience_Awarded, newXp);
            }
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100c6dc0)]
        public static void ConfusionStartTurn(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20038, TextFloaterColor.Red);
            var currentConfusionState = evt.GetConditionArg3();
            if (currentConfusionState == 5 || currentConfusionState == 7)
            {
                GameSystems.AI.StopFleeing(evt.objHndCaller);
            }

            // Make a percentage roll to determine new confusion state
            var roll = Dice.D100.Roll();
            int newConfusionState;
            if (roll <= 10)
            {
                // Attack caster
                newConfusionState = 8;
            }
            else if (roll <= 20)
            {
                // Act normally
                newConfusionState = 0;
            }
            else if (roll <= 50)
            {
                // Do nothing
                newConfusionState = 6;
            }
            else if (roll <= 70)
            {
                // Flee away from caster
                newConfusionState = 5;
            }
            else
            {
                // Attack nearest
                newConfusionState = 9;
            }

            evt.SetConditionArg3(newConfusionState);
            GameSystems.AI.AiProcess(evt.objHndCaller);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ca140)]
        public static void SanctuaryAttemptSave(in DispatcherCallbackArgs evt)
        {
            if (!(evt.GetDispIoD20Signal().obj is D20Action action))
            {
                return;
            }

            if (action.d20APerformer == evt.objHndCaller)
            {
                return;
            }

            if (!GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget))
            {
                return;
            }

            // TODO: Shouldn't this be part of the IsOffensive check???
            if (action.d20ActType == D20ActionType.CAST_SPELL)
            {
                var spellEnum = action.d20SpellData.SpellEnum;
                if (!GameSystems.Spell.IsSpellHarmful(spellEnum, action.d20APerformer, action.d20ATarget))
                {
                    return;
                }
            }

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            var success = GameSystems.D20.Combat.SavingThrowSpell(action.d20APerformer, spellPkt.caster,
                spellPkt.dc, SavingThrowType.Will, 0, spellId);
            if (success)
            {
                GameSystems.Spell.FloatSpellLine(action.d20APerformer, 30001, TextFloaterColor.White);
                action.d20APerformer.AddCondition("sp-Sanctuary Save Succeeded", spellId, 0, 0);
            }
            else
            {
                GameSystems.Spell.FloatSpellLine(action.d20APerformer, 30002, TextFloaterColor.White);
                action.d20APerformer.AddCondition("sp-Sanctuary Save Failed", spellId, 0, 0);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd370)]
        public static void sub_100CD370(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20010, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100c78d0)]
        public static void entangleMoveRestrict(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            if (!evt.objHndCaller.HasCondition(SpellEffects.SpellControlPlantsDisentangle)
                && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                dispIo.bonlist.SetOverallCap(1, 0, 0, data2);
                dispIo.bonlist.SetOverallCap(2, 0, 0, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cfaf0)]
        public static void sub_100CFAF0(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20015, TextFloaterColor.Red);
        }
    }
}