
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
    [ObjectScript(365)]
    public class IuzVerbobonc : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(697) == 1))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                Sound(4137, 1);
            }
            else if ((GetGlobalVar(697) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if (attachee.GetNameId() == 8042) // iuz
            {
                attachee.SetScriptId(ObjScriptEvent.StartCombat, 365);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(694, GetGlobalVar(694) + 1);
            if ((GetGlobalVar(694) == 4))
            {
                SetQuestState(102, QuestState.Completed);
                PartyLeader.AddReputation(58);
                random_fate();
            }

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
            // attachee.d20_send_signal(S_BreakFree)
            if (attachee.GetLeader() != null) // Don't wanna fuck up charmed enemies
            {
                return RunDefault;
            }

            var closest_jones = Utilities.party_closest(attachee, exclude_warded:true);
            if (attachee.DistanceTo(closest_jones) > 30 && !attachee.HasCondition(SpellEffects.SpellDimensionalAnchor))
            {
                attachee.PendingSpellsToMemorized();
                attachee.SetInt(obj_f.critter_strategy, 110);
            }
            else if (closest_jones == Utilities.party_closest(attachee, true, 2))
            {
                attachee.SetInt(obj_f.critter_strategy, 453);
            }
            else
            {
                CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
            }

            return RunDefault;
        }
        public override bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5121))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalVar(697, 2);
            }

            return RunDefault;
        }
        public static bool random_fate()
        {
            var pendulum = RandomRange(1, 5);
            if ((pendulum == 1 || pendulum == 2 || pendulum == 3))
            {
                SetGlobalVar(508, 1);
            }
            else if ((pendulum == 4))
            {
                SetGlobalVar(508, 2);
            }
            else if ((pendulum == 5))
            {
                SetGlobalVar(508, 3);
            }

            return RunDefault;
        }

    }
}
