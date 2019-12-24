
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
    [ObjectScript(182)]
    public class Assassin : BaseObjectScript
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

            SetGlobalFlag(836, true);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // if (not attachee.has_wielded(4082) or not attachee.has_wielded(4112)):
            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4112)))
            {
                attachee.WieldBestInAllSlots();
            }

            // game.new_sid = 0
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // if (not attachee.has_wielded(4082) or not attachee.has_wielded(4112)):
            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4112)))
            {
                attachee.WieldBestInAllSlots();
            }

            // game.new_sid = 0
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(836, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4112)))
            {
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
            }

            if ((attachee.GetMap() != 5085 && !GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    attachee.TurnTowards(obj);
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        obj.BeginDialog(attachee, 1);
                        DetachScript();
                    }

                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody npc, GameObjectBody pc)
        {
            npc.TransferItemByProtoTo(pc, 11002);
            npc.RunOff();
            return RunDefault;
        }

    }
}
