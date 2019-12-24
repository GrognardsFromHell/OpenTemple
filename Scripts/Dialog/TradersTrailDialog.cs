
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Scripts.Dialog
{
    [DialogScript(434)]
    public class TradersTrailDialog : TradersTrail, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2001:
                    originalScript = "pc.stat_level_get(stat_strength) >= 31";
                    return pc.GetStat(Stat.strength) >= 31;
                case 2002:
                    originalScript = "pc.stat_level_get(stat_strength) < 31";
                    return pc.GetStat(Stat.strength) < 31;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
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
                    originalScript = "traders_runoff(npc)";
                    traders_runoff(npc);
                    break;
                case 21:
                case 22:
                    originalScript = "traders_reveal(pc,1)";
                    traders_reveal(pc, 1);
                    break;
                case 121:
                case 122:
                    originalScript = "traders_reveal(pc,2)";
                    traders_reveal(pc, 2);
                    break;
                case 221:
                case 222:
                    originalScript = "traders_reveal(pc,3)";
                    traders_reveal(pc, 3);
                    break;
                case 511:
                case 512:
                    originalScript = "traders_reveal(pc,4)";
                    traders_reveal(pc, 4);
                    break;
                case 611:
                    originalScript = "game.timevent_add(set_pad_3_bit,(npc, 14806, 3),12601500); game.fade(12600,0,0,1)";
                    // TODO Function does not exist StartTimer(12601500, () => set_pad_3_bit(npc, 14806, 3));
                    Fade(12600, 0, 0, 1);
                    break;
                case 621:
                    originalScript = "npc.destroy()";
                    npc.Destroy();
                    break;
                case 661:
                    originalScript = "game.global_vars[437] = 100; game.encounter_queue.append(3159); traders_runoff(npc); game.worldmap_travel_by_dialog(3)";
                    SetGlobalVar(437, 100);
                    QueueRandomEncounter(3159);
                    traders_runoff(npc);
                    // FIXME: worldmap_travel_by_dialog;
                    ;
                    break;
                case 701:
                    originalScript = "game.timevent_add(set_pad_3_bit,(npc, 14806, 2),72020000); game.fade(7200,0,0,1)";
                    // TODO Function does not exist StartTimer(72020000, () => set_pad_3_bit(npc, 14806, 2));
                    Fade(7200, 0, 0, 1);
                    ;
                    break;
                case 711:
                    originalScript = "traders_runoff(npc); npc.destroy()";
                    traders_runoff(npc);
                    npc.Destroy();
                    ;
                    break;
                case 2000:
                    originalScript = "game.global_vars[437] = 102; game.story_state = 3; game.areas[3] = 1";
                    SetGlobalVar(437, 102);
                    StoryState = 3;
                    MakeAreaKnown(3);
                    ;
                    break;
                default:
                    originalScript = null;
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
