
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
    [ObjectScript(185)]
    public class Knight : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                GameObjectBody good_pc = null;

                GameObjectBody near_pc = null;

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
        public static bool distribute_magic_items(GameObjectBody npc, GameObjectBody pc)
        {
            foreach (var obj in pc.GetPartyMembers())
            {
                obj.AdjustMoney(2000000);
                Utilities.create_item_in_inventory(8007, obj);
                Utilities.create_item_in_inventory(6082, obj);
            }

            return RunDefault;
        }
        public static bool transfer_scrolls(GameObjectBody npc, GameObjectBody pc)
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
        public static bool knight_party(GameObjectBody npc, GameObjectBody pc)
        {
            pc.AddReputation(22);
            foreach (var obj in pc.GetPartyMembers())
            {
                Utilities.create_item_in_inventory(6128, obj);
                Utilities.create_item_in_inventory(6129, obj);
            }

            return RunDefault;
        }
        public static void run_off(GameObjectBody npc, GameObjectBody pc)
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
