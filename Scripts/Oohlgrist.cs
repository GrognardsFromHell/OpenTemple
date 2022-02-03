
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(126)]
    public class Oohlgrist : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 420);
            }
            else if ((GetGlobalFlag(121) || GetGlobalFlag(122) || GetGlobalFlag(123)))
            {
                triggerer.BeginDialog(attachee, 150);
            }
            else
            {
                triggerer.BeginDialog(attachee, 160);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            // if should_modify_CR( attachee ):
            // modify_CR( attachee, get_av_level() )
            ScriptDaemon.record_time_stamp(518);
            SetGlobalFlag(110, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.AddCondition("Rend")))
            {
                Logger.Info("Added Rend to Oohlgrist");
            }

            if (((Utilities.obj_percent_hp(attachee) < 50) && (!GetGlobalFlag(350)) && ((ScriptDaemon.get_v(454) & (0x20 + (1 << 7))) == 0))) // if he hasn't already been intimidated or regrouped
            {
                GameObject found_pc = null;
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.type == ObjectType.pc)
                    {
                        found_pc = pc;
                        attachee.AIRemoveFromShitlist(pc);
                    }

                }

                if (found_pc != null)
                {
                    SetGlobalFlag(349, true);
                    found_pc.BeginDialog(attachee, 70);
                    return SkipDefault;
                }

            }

            // THIS IS USED FOR BREAK FREE
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((obj.DistanceTo(attachee) <= 9 && obj.GetStat(Stat.hp_current) >= -9))
                {
                    return RunDefault;
                }

            }

            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // attachee.d20_send_signal(S_BreakFree)
            if (attachee.GetLeader() != null) // Don't wanna fuck up charmed enemies
            {
                return RunDefault;
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(110, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    if ((Utilities.obj_percent_hp(attachee) > 70))
                    {
                        if ((Utilities.group_percent_hp(leader) < 30))
                        {
                            attachee.FloatLine(460, leader);
                            attachee.Attack(leader);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObject attachee, GameObject triggerer)
        {
            // game.global_flags[112] = 1		### removed by Livonya
            return RunDefault;
        }
        public static bool TalkAern(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8033);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 480);
            }

            return SkipDefault;
        }
        public static bool join_temple(string temple_name_input)
        {
            var temple_name = temple_name_input.ToString();
            if (temple_name == "water")
            {
                SetGlobalFlag(112, true); // Oohlgrist has joined water temple
                if ((ScriptDaemon.get_v(454) & 2) == 2) // Water has already regrouped
                {
                    ScriptDaemon.set_v(454, ScriptDaemon.get_v(454) | 0x40);
                }

            }
            else if (temple_name == "fire")
            {
                SetGlobalFlag(118, true); // Oohlgrist has joined fire temple
                if ((ScriptDaemon.get_v(454) & 0x8) == 0x8) // Fire has already regrouped
                {
                    ScriptDaemon.set_v(454, ScriptDaemon.get_v(454) | 0x10);
                }

            }
            else
            {
                SelectedPartyLeader.Damage(null, DamageType.Subdual, Dice.Parse("500d1"));
                SelectedPartyLeader.FloatMesFileLine("mes/skill_ui.mes", 155);
            }

            return SkipDefault;
        }

    }
}
