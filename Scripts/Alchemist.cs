
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [ObjectScript(477)]
    public class Alchemist : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(897)))
            {
                StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
                SetGlobalFlag(897, true);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public static void respawn(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1078);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
            return;
        }
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var cap1 = attachee.GetNameId();
            var cap2 = triggerer.ItemWornAt(EquipSlot.Bracers);
            if (cap2 != null && cap2.GetNameId() == cap1)
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    triggerer.FloatMesFileLine("mes/combat.mes", 6015);
                    var holder = GameSystems.MapObject.CreateObject(1004, triggerer.GetLocation());
                    holder.GetItem(attachee);
                    triggerer.GetItem(attachee);
                    holder.Destroy();
                    UiSystems.CharSheet.Hide();
                    return SkipDefault;
                }
                else
                {
                    StartTimer(1000, () => get_rid_of_it(attachee, triggerer));
                }

            }

            return RunDefault;
        }
        public static void get_rid_of_it(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var cap1 = attachee.GetNameId();
            var cap2 = triggerer.ItemWornAt(EquipSlot.Bracers);
            if (cap2 != null && cap2.GetNameId() == cap1)
            {
                attachee.Destroy();
            }

            // game.particles( "sp-summon monster I", game.party[0] )
            return;
        }

    }
}
