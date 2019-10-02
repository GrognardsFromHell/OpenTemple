
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
    [ObjectScript(84)]
    public class WeaverDaughter : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11001, triggerer);
            }
            else
            {
                attachee.TurnTowards(triggerer);
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(698) == 5))
            {
                GameSystems.MapObject.CreateObject(14699, new locXY(476, 595));
                SetGlobalVar(698, GetGlobalVar(698) + 1);
            }

            return RunDefault;
        }
        public static bool argue(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8007);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 30);
            }

            return SkipDefault;
        }
        public static bool make_hate(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var r = attachee.GetReaction(triggerer);
            if ((r > 20))
            {
                var n = 20 - r;
                attachee.AdjustReaction(triggerer, n);
            }

            return SkipDefault;
        }

    }
}