
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

namespace VanillaScripts.Dialog
{
    [DialogScript(71)]
    public class ZertDialog : Zert, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 6:
                case 7:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 51:
                case 54:
                    originalScript = "game.areas[2] == 0";
                    return !IsAreaKnown(2);
                case 52:
                case 55:
                    originalScript = "game.areas[2] == 1";
                    return IsAreaKnown(2);
                case 53:
                case 56:
                    originalScript = "game.story_state >= 2";
                    return StoryState >= 2;
                case 71:
                case 72:
                case 111:
                case 112:
                    originalScript = "not pc.follower_atmax() and pc.stat_level_get(stat_level_paladin) == 0";
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 73:
                case 74:
                case 113:
                case 114:
                    originalScript = "pc.follower_atmax() and pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 75:
                case 76:
                case 115:
                case 116:
                    originalScript = "pc.stat_level_get(stat_level_paladin) > 0";
                    return pc.GetStat(Stat.level_paladin) > 0;
                case 92:
                case 95:
                    originalScript = "game.story_state >= 3";
                    return StoryState >= 3;
                case 93:
                case 96:
                    originalScript = "game.story_state >= 4";
                    return StoryState >= 4;
                case 101:
                case 102:
                    originalScript = "(game.story_state == 1) and (game.areas[2] == 1)";
                    return (StoryState == 1) && (IsAreaKnown(2));
                case 121:
                case 122:
                    originalScript = "npc.area != 1 and npc.area != 3";
                    return npc.GetArea() != 1 && npc.GetArea() != 3;
                case 123:
                case 124:
                    originalScript = "npc.area == 1 or npc.area == 3";
                    return npc.GetArea() == 1 || npc.GetArea() == 3;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 60:
                    originalScript = "game.story_state = 1; game.areas[2] = 1";
                    StoryState = 1;
                    MakeAreaKnown(2);
                    ;
                    break;
                case 62:
                case 63:
                case 101:
                case 102:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 100:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 140:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 331:
                    originalScript = "npc.attacks(pc)";
                    throw new NotSupportedException("Conversion failed.");
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
