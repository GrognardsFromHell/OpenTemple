
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
    [ObjectScript(361)]
    public class PelorCleric : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 10);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5093 && GetGlobalVar(960) == 3))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
            }
            else if ((attachee.GetMap() == 5156 && GetGlobalVar(704) == 3 && Utilities.is_daytime() && GetQuestState(76) != QuestState.Accepted))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                attachee.CastSpell(WellKnownSpells.SpellResistance, attachee);
            }
            else if ((attachee.GetMap() == 5137 && !GetGlobalFlag(922)))
            {
                StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
                SetGlobalFlag(922, true);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.AddCondition("fallen_paladin");
            }

            if ((attachee.GetMap() == 5093))
            {
                ditch_rings(attachee, triggerer);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(956) == 1))
            {
                attachee.SetInt(obj_f.critter_strategy, 411);
            }

            return RunDefault;
        }
        public static void respawn(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
            return;
        }
        public static void ditch_rings(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var acid_major = attachee.FindItemByName(12635);
            var cold_major = attachee.FindItemByName(12634);
            var electricity_major = attachee.FindItemByName(12632);
            var fire_major = attachee.FindItemByName(12631);
            var sonic_major = attachee.FindItemByName(12633);
            acid_major.Destroy();
            cold_major.Destroy();
            electricity_major.Destroy();
            fire_major.Destroy();
            sonic_major.Destroy();
            return;
        }

    }
}
