
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
    [ObjectScript(290)]
    public class AlchemicalCapsules : BaseObjectScript
    {
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // game.particles( "sp-summon monster I", game.party[0] )
            var cap2 = attachee.ItemWornAt(EquipSlot.Bracers);
            if (cap2 != null && (cap2.GetNameId() == 12754 || cap2.GetNameId() == 12755 || cap2.GetNameId() == 12756 || cap2.GetNameId() == 12757))
            {
                var scid = cap2.GetInt(obj_f.item_pad_i_1);
                ReplaceCurrentScript(scid);
                cap2.Destroy();
                AttachParticles("sp-Fireball-Hit", attachee);
            }

            return RunDefault;
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
                    StartTimer(1000, () => get_rid_of_it(attachee));
                }

            }

            return RunDefault;
        }
        public static void get_rid_of_it(GameObjectBody attachee)
        {
            attachee.Destroy();
            // game.particles( "sp-summon monster I", game.party[0] )
            return;
        }
        public static void spare_stuff(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var cap1 = attachee.GetNameId();
            var cap3 = triggerer.ItemWornAt(EquipSlot.Shield);
            if (cap3 != null && cap3.GetNameId() == cap1)
            {
                if (!GameSystems.Combat.IsCombatActive())
                {
                    triggerer.FloatMesFileLine("mes/combat.mes", 6015);
                    var holder = GameSystems.MapObject.CreateObject(1004, triggerer.GetLocation());
                    holder.GetItem(attachee);
                    triggerer.GetItem(attachee);
                    holder.Destroy();
                    UiSystems.CharSheet.Hide();
                    return;
                }
                else
                {
                    StartTimer(1000, () => get_rid_of_it(attachee));
                }

            }

            var cap4 = triggerer.ItemWornAt(EquipSlot.RingSecondary);
            if (cap4 != null && cap4.GetNameId() == cap1)
            {
                if (!GameSystems.Combat.IsCombatActive())
                {
                    triggerer.FloatMesFileLine("mes/combat.mes", 6015);
                    var holder = GameSystems.MapObject.CreateObject(1004, triggerer.GetLocation());
                    holder.GetItem(attachee);
                    triggerer.GetItem(attachee);
                    holder.Destroy();
                    UiSystems.CharSheet.Hide();
                    return;
                }
                else
                {
                    StartTimer(1000, () => get_rid_of_it(attachee));
                }

            }

            return;
        }

    }
}
