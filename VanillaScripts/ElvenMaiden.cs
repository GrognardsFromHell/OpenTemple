
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(184)]
    public class ElvenMaiden : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                GameObjectBody near_pc = null;

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
        public static bool money_handout(GameObjectBody npc, GameObjectBody pc)
        {
            foreach (var obj in pc.GetPartyMembers())
            {
                obj.AdjustMoney(100000);
            }

            return RunDefault;
        }
        public static void all_run_off(GameObjectBody npc, GameObjectBody pc)
        {
            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetLeader() == null))
                {
                    obj.RunOff();
                }

            }

            return;
        }


    }
}
