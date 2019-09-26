
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
    [ObjectScript(186)]
    public class LarethItemInsert : BaseObjectScript
    {
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((triggerer.GetNameId() == 8048) || (triggerer.GetNameId() == 8049) || (triggerer.GetNameId() == 1204)))
            {
                QueueRandomEncounter(3000);
            }
            else if (triggerer.GetNameId() == 1004 && SelectedPartyLeader.GetMap() == 5010) // 5010- Trader's shop
            {
                if (!GameSystems.RandomEncounter.IsEncounterQueued(3000))
                {
                    QueueRandomEncounter(3000);
                }

            }
            else if (triggerer.GetNameId() == 1001 && SelectedPartyLeader.GetMap() == 5001 && (new[] { 1, 3006, 4120, 6097, 6098, 6099, 6100 }).Contains(attachee.GetNameId())) // note: for some reason most of Lareth's item have a "name" field of 1, and another has 3006. Will be fixed in the future inside Protos.tab, for now this is a hotfix
            {
                GameObjectBody bro_smith = null;
                foreach (var npc in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if (npc.GetNameId() == 20005)
                    {
                        bro_smith = npc;
                    }

                }

                if (bro_smith == null)
                {
                    return SkipDefault;
                }

                GameObjectBody pc_dude;
                if (Utilities.is_safe_to_talk(bro_smith, SelectedPartyLeader))
                {
                    pc_dude = SelectedPartyLeader;
                }
                else
                {
                    pc_dude = Utilities.party_closest(bro_smith, true, 0);
                }

                if ((GetGlobalVar(452) & (0x40)) == 0)
                {
                    if (pc_dude != null && pc_dude.type == ObjectType.pc)
                    {
                        var cur_money = pc_dude.GetMoney();
                        UiSystems.CharSheet.Hide();
                        StartTimer(450, () => smith_refund(cur_money), true); // give money back to Smith
                        StartTimer(750, () => pc_give_item_back(pc_dude, attachee), true); // get item back
                        SetGlobalVar(452, GetGlobalVar(452) | 0x40);
                        pc_dude.BeginDialog(bro_smith, 1000);
                    }

                }
                else if ((attachee.GetInt(obj_f.item_wear_flags) & 256) != 0 && attachee.GetScriptId(ObjScriptEvent.InsertItem) == 186) // Lareth's plate boots
                {
                    var cur_money = pc_dude.GetMoney();
                    UiSystems.CharSheet.Hide();
                    StartTimer(450, () => smith_refund(cur_money), true); // give money back to Smith
                    StartTimer(750, () => pc_give_item_back(pc_dude, attachee), true); // get item back
                    pc_dude.BeginDialog(bro_smith, 1050);
                }

            }

            return SkipDefault;
        }
        public static void pc_give_item_back(GameObjectBody pc, GameObjectBody item)
        {
            pc.GetItem(item);
        }
        public static void smith_refund(int cur_money)
        {
            SelectedPartyLeader.AdjustMoney(cur_money - SelectedPartyLeader.GetMoney());
        }

    }
}
