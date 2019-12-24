
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
    [ObjectScript(170)]
    public class Taki : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 200); // taki in party
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8040))))
                {
                    triggerer.BeginDialog(attachee, 1); // have not met and ashrem is in party
                }
                else
                {
                    triggerer.BeginDialog(attachee, 120); // have not met
                }

            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8040))))
            {
                triggerer.BeginDialog(attachee, 170); // ashrem in party
            }
            else if ((GetGlobalVar(908) == 32))
            {
                triggerer.BeginDialog(attachee, 250); // have attacked 3 or more farm animals with taki in party
            }
            else
            {
                triggerer.BeginDialog(attachee, 190); // none of the above
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(285)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(12014, triggerer);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.FloatLine(12023, triggerer);
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
            var leader = attachee.GetLeader();
            if ((leader != null))
            {
                if ((triggerer.type == ObjectType.npc))
                {
                    if (((triggerer.GetAlignment() == Alignment.LAWFUL_GOOD) || (triggerer.GetAlignment() == Alignment.NEUTRAL_GOOD) || (triggerer.GetAlignment() == Alignment.CHAOTIC_GOOD)))
                    {
                        attachee.FloatLine(RandomRange(220, 222), leader);
                        return SkipDefault;
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalVar(908) >= 3))
                {
                    if ((attachee != null))
                    {
                        var leader = attachee.GetLeader();
                        if ((leader != null))
                        {
                            leader.RemoveFollower(attachee);
                            attachee.FloatLine(22000, triggerer);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool switch_to_ashrem(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var ashrem = Utilities.find_npc_near(attachee, 8040);
            if ((ashrem != null))
            {
                triggerer.BeginDialog(ashrem, line);
                ashrem.TurnTowards(attachee);
                attachee.TurnTowards(ashrem);
            }

            return SkipDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            SetGlobalFlag(285, true);
            return RunDefault;
        }

    }
}
