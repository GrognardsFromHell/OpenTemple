
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
    [ObjectScript(187)]
    public class CaravanSurvivor : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                attachee.TurnTowards(obj);
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    obj.BeginDialog(attachee, 1);
                    DetachScript();
                    return RunDefault;
                }

            }

            return RunDefault;
        }
        public static void loot_caravan_bandits(GameObjectBody pc, FIXME charity = 0)
        {
            var pc_index = 0;
            foreach (var obj in ObjList.ListVicinity(pc.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (obj.GetNameId() == 14317) // Caravan Bandits
                {
                    var listt = new[] { 6042, 6043, 6044, 6045, 6046, 4074, 4071, 4067, 4116, 4036, 4096, 6034 };
                    if (charity == 0)
                    {
                        listt.append/*Unknown*/(7001);
                    }

                    foreach (var item_number in listt)
                    {
                        var countt = 0;
                        while (obj.FindItemByName(item_number) != null && countt <= 20 && pc_index < GameSystems.Party.PartyMembers.Count)
                        {
                            // while obj.item_find(item_number) != OBJ_HANDLE_NULL and countt <= 20 and pc_index < len(game.party): ## count <= added as failsafe (in case PC is overloaded and something freaky happens...)
                            var tempp = GameSystems.Party.GetPartyGroupMemberN(pc_index).GetItem(obj.FindItemByName(item_number));
                            if (!tempp)
                            {
                                pc_index += 1;
                            }

                            countt += 1;
                        }

                    }

                }

            }

            Fade(0, 0, 1009, 0);
            Utilities.start_game_with_quest(22);
        }

    }
}
