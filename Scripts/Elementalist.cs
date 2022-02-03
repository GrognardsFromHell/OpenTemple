
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
    [ObjectScript(482)]
    public class Elementalist : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            triggerer.BeginDialog(attachee, 100);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(501) == 3 && !Utilities.is_daytime()))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalVar(511, GetGlobalVar(511) + 1);
            if ((GetGlobalVar(511) >= 24 && GetGlobalFlag(501)))
            {
                SetGlobalFlag(511, true);
                if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                {
                    SetQuestState(97, QuestState.Completed);
                    PartyLeader.AddReputation(52);
                    SetGlobalVar(501, 7);
                }

            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(505) == 0))
            {
                StartTimer(7200000, () => out_of_time(attachee, triggerer)); // 2 hours
                SetGlobalVar(505, 1);
            }

            if ((triggerer.type == ObjectType.pc))
            {
                if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8736)))
                {
                    var wakefield = Utilities.find_npc_near(triggerer, 8736);
                    if ((wakefield != null))
                    {
                        triggerer.RemoveFollower(wakefield);
                        wakefield.FloatLine(20000, triggerer);
                        wakefield.Attack(triggerer);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetQuestState(97) == QuestState.Botched))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(503) == 0))
            {
                attachee.CastSpell(WellKnownSpells.Stoneskin, attachee);
                SetGlobalVar(503, 1);
            }

            return RunDefault;
        }
        public static bool switch_to_ariakas(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8731);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool dump_old_man(GameObject attachee, GameObject triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            Sound(4035, 1);
            AttachParticles("sp-Teleport", attachee);
            return RunDefault;
        }
        public static void shake(GameObject attachee, GameObject triggerer)
        {
            Sound(4130);
            GameSystems.Scroll.ShakeScreen(50, 6400);
            return;
        }
        public static void out_of_time(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(505, 3);
            return;
        }

    }
}
