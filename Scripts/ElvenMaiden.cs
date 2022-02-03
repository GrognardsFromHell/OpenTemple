
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
    [ObjectScript(184)]
    public class ElvenMaiden : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                GameObject near_pc = null;
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        near_pc = obj;
                        if (((obj.GetAlignment() == Alignment.LAWFUL_GOOD) || (obj.GetAlignment() == Alignment.NEUTRAL_GOOD) || (obj.GetAlignment() == Alignment.CHAOTIC_GOOD)))
                        {
                            obj.BeginDialog(attachee, 1);
                            DetachScript();
                            return RunDefault;
                        }

                    }

                }

                if ((near_pc != null))
                {
                    near_pc.BeginDialog(attachee, 1);
                    DetachScript();
                }

            }

            return RunDefault;
        }
        public static bool money_handout(GameObject npc, GameObject pc)
        {
            foreach (var obj in pc.GetPartyMembers())
            {
                obj.AdjustMoney(100000);
            }

            return RunDefault;
        }
        public static void all_run_off(GameObject npc, GameObject pc)
        {
            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetLeader() == null && !((SelectedPartyLeader.GetPartyMembers()).Contains(obj))))
                {
                    obj.RunOff();
                }

            }

            return;
        }

    }
}
