
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(185)]
    public class Knight : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((triggerer.FindItemByName(3014) != null))
            {
                triggerer.BeginDialog(attachee, 50);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                GameObject good_pc = null;

                GameObject near_pc = null;

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        near_pc = obj;

                        if (((obj.GetAlignment() == Alignment.LAWFUL_GOOD) || (obj.GetAlignment() == Alignment.NEUTRAL_GOOD) || (obj.GetAlignment() == Alignment.CHAOTIC_GOOD)))
                        {
                            good_pc = obj;

                        }

                        if ((obj.FindItemByName(3014) != null))
                        {
                            obj.BeginDialog(attachee, 50);
                            DetachScript();

                            return RunDefault;
                        }

                    }

                }

                if ((good_pc != null))
                {
                    good_pc.BeginDialog(attachee, 1);
                    DetachScript();

                }

                if ((near_pc != null))
                {
                    near_pc.BeginDialog(attachee, 1);
                    DetachScript();

                }

            }

            return RunDefault;
        }
        public static bool distribute_magic_items(GameObject npc, GameObject pc)
        {
            foreach (var obj in pc.GetPartyMembers())
            {
                obj.AdjustMoney(2000000);
                Utilities.create_item_in_inventory(8007, obj);
                Utilities.create_item_in_inventory(6082, obj);
            }

            return RunDefault;
        }
        public static bool transfer_scrolls(GameObject npc, GameObject pc)
        {
            Utilities.create_item_in_inventory(9288, pc);
            Utilities.create_item_in_inventory(9280, pc);
            Utilities.create_item_in_inventory(9438, pc);
            Utilities.create_item_in_inventory(9431, pc);
            Utilities.create_item_in_inventory(9383, pc);
            Utilities.create_item_in_inventory(9509, pc);
            Utilities.create_item_in_inventory(9467, pc);
            Utilities.create_item_in_inventory(9333, pc);
            Utilities.create_item_in_inventory(9238, pc);
            Utilities.create_item_in_inventory(9229, pc);
            Utilities.create_item_in_inventory(9159, pc);
            Utilities.create_item_in_inventory(9056, pc);
            return RunDefault;
        }
        public static bool knight_party(GameObject npc, GameObject pc)
        {
            pc.AddReputation(22);
            foreach (var obj in pc.GetPartyMembers())
            {
                Utilities.create_item_in_inventory(6128, obj);
                Utilities.create_item_in_inventory(6129, obj);
            }

            return RunDefault;
        }
        public static void run_off(GameObject npc, GameObject pc)
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
