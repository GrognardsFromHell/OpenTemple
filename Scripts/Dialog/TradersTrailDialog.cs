
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Scripts.Dialog
{
    [DialogScript(434)]
    public class TradersTrailDialog : TradersTrail, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2001:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_strength) >= 31");
                    return pc.GetStat(Stat.strength) >= 31;
                case 2002:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_strength) < 31");
                    return pc.GetStat(Stat.strength) < 31;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 102:
                case 202:
                case 502:
                case 602:
                case 652:
                case 702:
                case 710:
                case 1001:
                case 2001:
                case 2002:
                    Trace.Assert(originalScript == "traders_runoff(npc)");
                    traders_runoff(npc);
                    break;
                case 21:
                case 22:
                    Trace.Assert(originalScript == "traders_reveal(pc,1)");
                    traders_reveal(pc, 1);
                    break;
                case 121:
                case 122:
                    Trace.Assert(originalScript == "traders_reveal(pc,2)");
                    traders_reveal(pc, 2);
                    break;
                case 221:
                case 222:
                    Trace.Assert(originalScript == "traders_reveal(pc,3)");
                    traders_reveal(pc, 3);
                    break;
                case 511:
                case 512:
                    Trace.Assert(originalScript == "traders_reveal(pc,4)");
                    traders_reveal(pc, 4);
                    break;
                case 611:
                    Trace.Assert(originalScript == "game.timevent_add(set_pad_3_bit,(npc, 14806, 3),12601500); game.fade(12600,0,0,1)");
                    // TODO Function does not exist StartTimer(12601500, () => set_pad_3_bit(npc, 14806, 3));
                    Fade(12600, 0, 0, 1);
                    break;
                case 621:
                    Trace.Assert(originalScript == "npc.destroy()");
                    npc.Destroy();
                    break;
                case 661:
                    Trace.Assert(originalScript == "game.global_vars[437] = 100; game.encounter_queue.append(3159); traders_runoff(npc); game.worldmap_travel_by_dialog(3)");
                    SetGlobalVar(437, 100);
                    QueueRandomEncounter(3159);
                    traders_runoff(npc);
                    // FIXME: worldmap_travel_by_dialog;
                    ;
                    break;
                case 701:
                    Trace.Assert(originalScript == "game.timevent_add(set_pad_3_bit,(npc, 14806, 2),72020000); game.fade(7200,0,0,1)");
                    // TODO Function does not exist StartTimer(72020000, () => set_pad_3_bit(npc, 14806, 2));
                    Fade(7200, 0, 0, 1);
                    ;
                    break;
                case 711:
                    Trace.Assert(originalScript == "traders_runoff(npc); npc.destroy()");
                    traders_runoff(npc);
                    npc.Destroy();
                    ;
                    break;
                case 2000:
                    Trace.Assert(originalScript == "game.global_vars[437] = 102; game.story_state = 3; game.areas[3] = 1");
                    SetGlobalVar(437, 102);
                    StoryState = 3;
                    MakeAreaKnown(3);
                    ;
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
