
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
    [ObjectScript(162)]
    public class FemalePrisoner2 : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetGlobalFlag(131)) && !(attachee.GetMap() == 5121)))
            {
                triggerer.BeginDialog(attachee, 80);
            }
            else
            {
                triggerer.BeginDialog(attachee, 90);
            }

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
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5121))
            {
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 100);
                }

            }

            return SkipDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
            }

            attachee.RunOff();
            return RunDefault;
        }
        public static bool npc_check1(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 14250);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 90);
            }

            return SkipDefault;
        }
        public static bool npc_check2(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 14430);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 60);
            }

            return SkipDefault;
        }
        public static bool npc_check3(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 14431);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 60);
            }

            return SkipDefault;
        }
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(7))
            {
                triggerer.AddReputation(7);
            }

            SetGlobalVar(25, GetGlobalVar(25) + 1);
            if ((GetGlobalVar(25) >= 3 && !triggerer.HasReputation(8)))
            {
                triggerer.AddReputation(8);
            }

            return RunDefault;
        }
        public static bool free_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(16))
            {
                triggerer.AddReputation(16);
            }

            SetGlobalVar(26, GetGlobalVar(26) + 1);
            if ((GetGlobalVar(26) >= 3 && !triggerer.HasReputation(17)))
            {
                triggerer.AddReputation(17);
            }

            return RunDefault;
        }

    }
}
