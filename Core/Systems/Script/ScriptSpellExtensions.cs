using System;
using System.Collections;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Ui;
using SpicyTemple.Core.Ui.InGameSelect;

namespace SpicyTemple.Core.Systems.Script.Extensions
{
    public static class ScriptSpellExtensions
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x100be450)]
        [PythonName("spell_end")]
        public static void EndSpell(this SpellPacketBody activeSpell, bool endDespiteTargetList = false)
        {
            GameSystems.Spell.EndSpell(activeSpell.spellId, endDespiteTargetList);
        }

        [TempleDllLocation(0x100be4d0)]
        [PythonName("spell_remove")]
        public static void RemoveSpell(this SpellPacketBody activeSpell)
        {
            GameSystems.Spell.EndSpellsOfType(activeSpell.spellEnum);
        }

        private static Comparison<SpellTarget> HitDiceComparisonDescending = (a, b) =>
        {
            var hitDiceA = GameSystems.Critter.GetHitDiceNum(a.Object);
            var hitDiceB = GameSystems.Critter.GetHitDiceNum(b.Object);
            return hitDiceB.CompareTo(hitDiceA);
        };

        [TempleDllLocation(0x100c0520)]
        [PythonName("spell_target_list_sort")]
        public static void SortTargets(this SpellPacketBody activeSpell, TargetListOrder order,
            TargetListOrderDirection direction)
        {
            switch (order)
            {
                case TargetListOrder.HitDice:
                    Array.Sort(activeSpell.Targets, (a, b) =>
                    {
                        var hitDiceA = GameSystems.Critter.GetHitDiceNum(a.Object);
                        var hitDiceB = GameSystems.Critter.GetHitDiceNum(b.Object);
                        if (direction == TargetListOrderDirection.Ascending)
                        {
                            return hitDiceA.CompareTo(hitDiceB);
                        }
                        else
                        {
                            return hitDiceB.CompareTo(hitDiceA);
                        }
                    });
                    break;
                case TargetListOrder.HitDiceThenDist:
                    Array.Sort(activeSpell.Targets, (a, b) =>
                    {
                        var diceA = GameSystems.Critter.GetHitDiceNum(a.Object);
                        var diceB = GameSystems.Critter.GetHitDiceNum(a.Object);
                        if (diceA != diceB)
                        {
                            if (direction == TargetListOrderDirection.Ascending)
                            {
                                return diceA.CompareTo(diceB);
                            }
                            else
                            {
                                return diceB.CompareTo(diceA);
                            }
                        }

                        var distA = a.Object.DistanceTo(activeSpell.aoeCenter.location);
                        var distB = b.Object.DistanceTo(activeSpell.aoeCenter.location);
                        if (direction == TargetListOrderDirection.Ascending)
                        {
                            return distA.CompareTo(distB);
                        }
                        else
                        {
                            return distB.CompareTo(distA);
                        }
                    });
                    break;
                case TargetListOrder.Dist:
                    Array.Sort(activeSpell.Targets, (a, b) =>
                    {
                        var distA = a.Object.DistanceTo(activeSpell.aoeCenter.location);
                        var distB = b.Object.DistanceTo(activeSpell.aoeCenter.location);
                        if (direction == TargetListOrderDirection.Ascending)
                        {
                            return distA.CompareTo(distB);
                        }
                        else
                        {
                            return distB.CompareTo(distA);
                        }
                    });
                    break;
                case TargetListOrder.DistFromCaster:
                    Array.Sort(activeSpell.Targets, (a, b) =>
                    {
                        var distA = a.Object.DistanceTo(activeSpell.caster);
                        var distB = b.Object.DistanceTo(activeSpell.caster);
                        if (direction == TargetListOrderDirection.Ascending)
                        {
                            return distA.CompareTo(distB);
                        }
                        else
                        {
                            return distB.CompareTo(distA);
                        }
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, "Invalid target list order.");
            }
        }

        [TempleDllLocation(0x100be510)]
        [PythonName("spell_get_menu_arg")]
        public static int GetMenuArg(this SpellPacketBody activeSpell, RadialMenuParam param)
        {
            var selectedEntry = GameSystems.D20.RadialMenu.SelectedRadialMenuEntry;
            switch (param)
            {
                case RadialMenuParam.MinSetting:
                    return selectedEntry.minArg;
                case RadialMenuParam.MaxSetting:
                    return selectedEntry.maxArg;
                case RadialMenuParam.ActualSetting:
                    return selectedEntry.ArgumentGetter();
                default:
                    throw new ArgumentOutOfRangeException(nameof(param), param, null);
            }
        }

        [TempleDllLocation(0x100be580)]
        [PythonName("is_object_selected")]
        public static bool IsObjectSelected(this SpellPacketBody activeSpell)
        {
            return (activeSpell.pickerResult.flags & PickerResultFlags.PRF_HAS_SELECTED_OBJECT) != 0;
        }

        [TempleDllLocation(0x100be5c0)]
        [PythonName("summon_monsters")]
        public static void SummonMonsters(this SpellPacketBody activeSpell, bool aiFollower, int protoId = 17000)
        {
            var summon = GameSystems.MapObject.CreateObject(protoId, activeSpell.aoeCenter);

            GameSystems.AI.ForceSpreadOut(summon);

            if (!GameSystems.Critter.AddFollower(summon, activeSpell.caster, true, aiFollower))
            {
                Logger.Error("Unable to add new critter as a follower");
                GameSystems.MapObject.RemoveMapObj(summon);
                return;
            }

            GameSystems.D20.Initiative.AddToInitiative(summon);
            var casterIni = GameSystems.D20.Initiative.GetInitiative(activeSpell.caster);
            GameSystems.D20.Initiative.SetInitiative(summon, casterIni);

            UiSystems.Combat.Initiative.Update();
            UiSystems.Party.Update();

            summon.AddCondition(SpellEffects.SpellSummoned, activeSpell.spellId, activeSpell.duration, 0);
            summon.AddCondition(StatusEffects.TimedDisappear, activeSpell.spellId, activeSpell.duration, 0);

            // Add to the target list
            activeSpell.AddTarget(summon);

            // Make NPC summoned monsters attack the party
            if (activeSpell.caster.IsNPC() && !GameSystems.Party.IsInParty(activeSpell.caster))
            {
                // add the summoner's faction
                var factionArr = activeSpell.caster.GetInt32Array(obj_f.npc_faction);
                int numFactions = factionArr.Count;

                for (var i = 0; i < numFactions; i++)
                {
                    var newFac = factionArr[i];
                    if (newFac == 0)
                        continue;
                    if (!GameSystems.Critter.HasFaction(summon, newFac))
                    {
                        GameSystems.Critter.AddFaction(summon, newFac);
                    }
                }

                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    GameSystems.AI.ProvokeHostility(partyMember, summon, 1, 2);
                }
            }

            GameSystems.Anim.Interrupt(summon, AnimGoalPriority.AGP_HIGHEST);
        }
    }
}