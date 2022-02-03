
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
    [ObjectScript(340)]
    public class AssassinLeader : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5172))
            {
                if ((GetGlobalVar(945) == 1 || GetGlobalVar(945) == 2 || GetGlobalVar(945) == 3))
                {
                    triggerer.BeginDialog(attachee, 660);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 670);
                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(989)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5160))
            {
                if ((GetGlobalFlag(982)))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    if ((GetGlobalVar(944) >= 1))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }

                }

            }
            else if ((attachee.GetMap() == 5172))
            {
                if ((GetGlobalVar(945) == 1 || GetGlobalVar(945) == 2 || GetGlobalVar(945) == 3))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalFlag(943) || GetGlobalVar(945) == 28 || GetGlobalVar(945) == 29 || GetGlobalVar(945) == 30))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(989, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(989, false);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            // if (not attachee.has_wielded(4082) or not attachee.has_wielded(4112)):
            if ((!attachee.HasEquippedByName(4700) || !attachee.HasEquippedByName(4701)))
            {
                attachee.WieldBestInAllSlots();
            }

            // game.new_sid = 0
            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // if (not attachee.has_wielded(4082) or not attachee.has_wielded(4112)):
            if ((!attachee.HasEquippedByName(4700) || !attachee.HasEquippedByName(4701)))
            {
                attachee.WieldBestInAllSlots();
            }

            // game.new_sid = 0
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!attachee.HasEquippedByName(4700) || !attachee.HasEquippedByName(4701)))
            {
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
            }

            return RunDefault;
        }
        public static bool run_off(GameObject npc, GameObject pc)
        {
            npc.TransferItemByProtoTo(pc, 7003);
            npc.RunOff();
            return RunDefault;
        }
        public static int party_spot_check()
        {
            var highest_spot = -999;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.GetSkillLevel(SkillId.spot) > highest_spot)
                {
                    highest_spot = pc.GetSkillLevel(SkillId.spot);
                }

            }

            return highest_spot;
        }
        public static bool wilfrick_countdown(GameObject attachee, GameObject triggerer)
        {
            StartTimer(172800000, () => stop_watch()); // 2 days
            return RunDefault;
        }
        public static bool stop_watch()
        {
            SetGlobalVar(704, 2);
            return RunDefault;
        }
        public static bool darlia_release(GameObject attachee, GameObject triggerer)
        {
            StartTimer(345600000, () => cut_loose()); // 4 days
            return RunDefault;
        }
        public static bool cut_loose()
        {
            SetGlobalFlag(943, true);
            return RunDefault;
        }
        public static bool schedule_sb_retaliation_for_snitch(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(945, 4);
            StartTimer(864000000, () => sb_retaliation_for_snitch()); // 864000000ms is 10 days
            ScriptDaemon.record_time_stamp("s_sb_retaliation_for_snitch");
            return RunDefault;
        }
        public static bool schedule_sb_retaliation_for_narc(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(945, 5);
            StartTimer(518400000, () => sb_retaliation_for_narc()); // 518400000ms is 6 days
            ScriptDaemon.record_time_stamp("s_sb_retaliation_for_narc");
            return RunDefault;
        }
        public static bool schedule_sb_retaliation_for_whistleblower(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(945, 6);
            StartTimer(1209600000, () => sb_retaliation_for_whistleblower()); // 1209600000ms is 14 days
            ScriptDaemon.record_time_stamp("s_sb_retaliation_for_whistleblower");
            return RunDefault;
        }
        public static bool sb_retaliation_for_snitch()
        {
            QueueRandomEncounter(3435);
            ScriptDaemon.set_f("s_sb_retaliation_for_snitch_scheduled");
            return RunDefault;
        }
        public static bool sb_retaliation_for_narc()
        {
            QueueRandomEncounter(3435);
            ScriptDaemon.set_f("s_sb_retaliation_for_narc_scheduled");
            return RunDefault;
        }
        public static bool sb_retaliation_for_whistleblower()
        {
            QueueRandomEncounter(3435);
            ScriptDaemon.set_f("s_sb_retaliation_for_whistleblower_scheduled");
            return RunDefault;
        }

    }
}
