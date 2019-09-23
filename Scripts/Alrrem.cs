
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(122)]
    public class Alrrem : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            ScriptDaemon.record_time_stamp(517);
            triggerer.TurnTowards(attachee); // added by Livonya
            attachee.TurnTowards(triggerer); // added by Livonya
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031))))
            {
                // Prince Thrommel in your party
                triggerer.BeginDialog(attachee, 700);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8040)) && (!GetGlobalFlag(192))))
            {
                // Ashrem in your party
                triggerer.BeginDialog(attachee, 860);
            }
            else if (((GetGlobalFlag(115)) && (GetGlobalFlag(116)) && (!GetGlobalFlag(125))))
            {
                // Tubal and Antonio are dead (116 & 115 respectively), and you haven't bluffed him yet (125)
                triggerer.BeginDialog(attachee, 400);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                if ((GetGlobalFlag(92)))
                {
                    // Recruited via Wat
                    triggerer.BeginDialog(attachee, 200);
                }
                else
                {
                    // Waltzing In (TM)
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else
            {
                // "What news have you for me"
                triggerer.BeginDialog(attachee, 300);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(372)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(713, 0);
                }

                if ((GetGlobalFlag(312)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalFlag(107, true);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(107, true);
            ScriptDaemon.record_time_stamp(459);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(344, false);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(107, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((!attachee.HasMet(obj)))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            ScriptDaemon.record_time_stamp(517);
                            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031))))
                            {
                                // Thrommel in Party
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 700);
                            }
                            else if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8040)) && (!GetGlobalFlag(192))))
                            {
                                // Ashrem in Party
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 860);
                            }
                            else if (((GetGlobalFlag(104)) || (GetGlobalFlag(105)) || (GetGlobalFlag(106))))
                            {
                                // Killed one of the other priests
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 730);
                            }
                            else if ((GetGlobalFlag(92)))
                            {
                                // Recruited by Wat
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 200);
                            }
                            else
                            {
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 1);
                            }

                        }

                    }

                }

            }

            // game.new_sid = 0			## removed by Livonya
            // Prebuffing self and Underpriests			#
            // Added by Livonya, modified by S.A. Oct-2009		#
            if ((GetGlobalVar(713) == 2 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.ResistElements, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(713) == 6 && !GameSystems.Combat.IsCombatActive()))
            {
                var Tubal = Utilities.find_npc_near(attachee, 14212);
                if (Tubal != null && Tubal.GetLeader() == null)
                {
                    Tubal.CastSpell(WellKnownSpells.ResistElements, Tubal);
                    Tubal.PendingSpellsToMemorized();
                }

                var Antonio = Utilities.find_npc_near(attachee, 14211);
                if (Antonio != null && Antonio.GetLeader() == null)
                {
                    Antonio.CastSpell(WellKnownSpells.ResistElements, Antonio);
                    Antonio.PendingSpellsToMemorized();
                }

            }

            if ((GetGlobalVar(713) == 8 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.ProtectionFromElements, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(713) == 10 && !GameSystems.Combat.IsCombatActive()))
            {
                var Tubal = Utilities.find_npc_near(attachee, 14212);
                if (Tubal != null && Tubal.GetLeader() == null)
                {
                    Tubal.CastSpell(WellKnownSpells.EndureElements, Tubal);
                    Tubal.PendingSpellsToMemorized();
                }

            }

            if ((GetGlobalVar(713) == 11 && !GameSystems.Combat.IsCombatActive()))
            {
                var Antonio = Utilities.find_npc_near(attachee, 14211);
                if (Antonio != null && Antonio.GetLeader() == null)
                {
                    Antonio.CastSpell(WellKnownSpells.EndureElements, Antonio);
                    Antonio.PendingSpellsToMemorized();
                }

            }

            if ((GetGlobalVar(713) == 12 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.FreedomOfMovement, attachee);
                attachee.PendingSpellsToMemorized();
            }

            // Prebuffing other critters when on Fire Temple Alert	#
            // Added by S.A. Oct-2009				#
            if ((ScriptDaemon.get_v(454) & 8) != 0 && GetGlobalVar(713) > 12 && GetGlobalVar(713) < 100)
            {
                if ((GetGlobalVar(713) == 16 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    var buff_list = new[] { 14344, 14195, 14343 }; // Werewolf, Oohlgrist, Hydra
                    var mang1 = ScriptDaemon.buffee(attachee.GetLocation(), 20, buff_list, new List<GameObjectBody>());
                    if (mang1 == null)
                    {
                        SetGlobalVar(713, GetGlobalVar(713) + 1);
                        return RunDefault;
                    }

                    attachee.CastSpell(WellKnownSpells.FreedomOfMovement, mang1);
                    attachee.PendingSpellsToMemorized();
                }

                if ((GetGlobalVar(713) == 17 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    var buff_list = new[] { 14344, 14195, 14343 }; // Werewolf, Oohlgrist, Hydra
                    var Tubal = Utilities.find_npc_near(attachee, 14212);
                    var mang1 = ScriptDaemon.buffee(Tubal.GetLocation(), 20, buff_list, new List<GameObjectBody>());
                    if (mang1 == null)
                    {
                        SetGlobalVar(713, GetGlobalVar(713) + 1);
                        return RunDefault;
                    }

                    if (Tubal != null && Tubal.GetLeader() == null)
                    {
                        Tubal.CastSpell(WellKnownSpells.EndureElements, mang1);
                        Tubal.PendingSpellsToMemorized();
                    }

                }

                if ((GetGlobalVar(713) == 18 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    var buff_list = new[] { 14344, 14224 }; // Werewolf, Aern
                    var Antonio = Utilities.find_npc_near(attachee, 14211);
                    var mang1 = ScriptDaemon.buffee(Antonio.GetLocation(), 20, buff_list, new List<GameObjectBody>());
                    if (mang1 == null)
                    {
                        SetGlobalVar(713, GetGlobalVar(713) + 1);
                        return RunDefault;
                    }

                    if (Antonio != null && Antonio.GetLeader() == null)
                    {
                        Antonio.CastSpell(WellKnownSpells.EndureElements, mang1);
                        Antonio.PendingSpellsToMemorized();
                    }

                }

                if ((GetGlobalVar(713) == 20 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    var buff_list = new[] { 14344, 14195, 14343 }; // Werewolf, Oohlgrist, Hydra
                    var mang1 = ScriptDaemon.buffee(attachee.GetLocation(), 20, buff_list, new List<GameObjectBody>());
                    if (mang1 == null)
                    {
                        SetGlobalVar(713, GetGlobalVar(713) + 1);
                        return RunDefault;
                    }

                    attachee.CastSpell(WellKnownSpells.ResistElements, mang1);
                    attachee.PendingSpellsToMemorized();
                }

                if ((GetGlobalVar(713) == 24 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    var Tubal = Utilities.find_npc_near(attachee, 14212);
                    Tubal.TurnTowards(attachee);
                    var buff_list = new[] { 14344, 14195, 14343 }; // Werewolf, Oohlgrist, Hydra
                    var mang1 = ScriptDaemon.buffee(attachee.GetLocation(), 20, buff_list, new List<GameObjectBody>());
                    if (mang1 == null)
                    {
                        SetGlobalVar(713, GetGlobalVar(713) + 1);
                        return RunDefault;
                    }

                    attachee.CastSpell(WellKnownSpells.MagicCircleAgainstGood, mang1);
                    attachee.PendingSpellsToMemorized();
                }

                if ((GetGlobalVar(713) == 28 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    var buff_list = new[] { 14344, 14195, 14343 }; // Werewolf, Oohlgrist, Hydra
                    var mang1 = ScriptDaemon.buffee(attachee.GetLocation(), 20, buff_list, new List<GameObjectBody>());
                    var mang2 = ScriptDaemon.buffee(attachee.GetLocation(), 20, buff_list, new[] { mang1 });
                    if (mang2 == null)
                    {
                        SetGlobalVar(713, GetGlobalVar(713) + 1);
                        return RunDefault;
                    }

                    attachee.CastSpell(WellKnownSpells.EndureElements, mang2);
                    attachee.PendingSpellsToMemorized();
                }

                if ((GetGlobalVar(713) == 32 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    var Antonio = Utilities.find_npc_near(attachee, 14211);
                    if (Antonio != null)
                    {
                        attachee.TurnTowards(Antonio);
                    }

                }

            }

            // End of section					#
            SetGlobalVar(713, GetGlobalVar(713) + 1);
            return RunDefault;
        }
        public static bool escort_below(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // game.global_flags[144] = 1
            SetGlobalVar(691, 3);
            FadeAndTeleport(0, 0, 0, 5080, 478, 451);
            return RunDefault;
        }
        public static bool talk_Ashrem(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var ashrem = Utilities.find_npc_near(attachee, 8040);
            if ((ashrem != null))
            {
                triggerer.BeginDialog(ashrem, line);
                ashrem.TurnTowards(attachee);
                attachee.TurnTowards(ashrem);
            }

            return SkipDefault;
        }

    }
}
