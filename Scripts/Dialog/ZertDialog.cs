
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
    [DialogScript(71)]
    public class ZertDialog : Zert, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 51:
                case 54:
                    Trace.Assert(originalScript == "game.areas[2] == 0");
                    return !IsAreaKnown(2);
                case 52:
                case 55:
                    Trace.Assert(originalScript == "game.areas[2] == 1");
                    return IsAreaKnown(2);
                case 53:
                case 56:
                    Trace.Assert(originalScript == "game.story_state >= 2");
                    return StoryState >= 2;
                case 71:
                case 72:
                case 111:
                case 112:
                    Trace.Assert(originalScript == "not pc.follower_atmax() and pc.stat_level_get(stat_level_paladin) == 0");
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 73:
                case 74:
                case 113:
                case 114:
                    Trace.Assert(originalScript == "pc.follower_atmax() and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 75:
                case 76:
                case 115:
                case 116:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) > 0");
                    return pc.GetStat(Stat.level_paladin) > 0;
                case 92:
                case 95:
                    Trace.Assert(originalScript == "game.story_state >= 3");
                    return StoryState >= 3;
                case 93:
                case 96:
                    Trace.Assert(originalScript == "game.story_state >= 4");
                    return StoryState >= 4;
                case 101:
                case 102:
                    Trace.Assert(originalScript == "(game.story_state == 1) and (game.areas[2] == 1)");
                    return (StoryState == 1) && (IsAreaKnown(2));
                case 121:
                case 122:
                    Trace.Assert(originalScript == "npc.area != 1 and npc.area != 3");
                    return npc.GetArea() != 1 && npc.GetArea() != 3;
                case 123:
                case 124:
                    Trace.Assert(originalScript == "npc.area == 1 or npc.area == 3");
                    return npc.GetArea() == 1 || npc.GetArea() == 3;
                case 125:
                case 126:
                    Trace.Assert(originalScript == "npc.leader_get() != OBJ_HANDLE_NULL");
                    return npc.GetLeader() != null;
                case 501:
                case 502:
                    Trace.Assert(originalScript == "npc.hit_dice_num > 2 and (npc.item_find(6047) or npc.item_find(6060) or npc.item_find(4036))");
                    return GameSystems.Critter.GetHitDiceNum(npc) > 2 && (npc.FindItemByName(6047) != null || npc.FindItemByName(6060) != null || npc.FindItemByName(4036) != null);
                case 503:
                case 504:
                    Trace.Assert(originalScript == "npc.hit_dice_num == 2");
                    return GameSystems.Critter.GetHitDiceNum(npc) == 2;
                case 551:
                case 552:
                    Trace.Assert(originalScript == "pc.money_get() >= 9000");
                    return pc.GetMoney() >= 9000;
                case 601:
                case 611:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 602:
                case 612:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    Trace.Assert(originalScript == "game.global_vars[106] = 1");
                    SetGlobalVar(106, 1);
                    break;
                case 60:
                    Trace.Assert(originalScript == "game.story_state = 1; game.areas[2] = 1");
                    StoryState = 1;
                    MakeAreaKnown(2);
                    ;
                    break;
                case 62:
                case 63:
                case 101:
                case 102:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(2)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 100:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 140:
                case 320:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 331:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 570:
                    Trace.Assert(originalScript == "equip_transfer( npc, pc )");
                    equip_transfer(npc, pc);
                    break;
                case 571:
                case 572:
                    Trace.Assert(originalScript == "pc.money_adj(-9000)");
                    pc.AdjustMoney(-9000);
                    break;
                case 601:
                case 602:
                case 603:
                    Trace.Assert(originalScript == "switch_to_turuko( npc, pc, 610)");
                    switch_to_turuko(npc, pc, 610);
                    break;
                case 611:
                case 612:
                case 613:
                    Trace.Assert(originalScript == "switch_to_turuko( npc, pc, 630)");
                    switch_to_turuko(npc, pc, 630);
                    break;
                case 621:
                    Trace.Assert(originalScript == "they_attack(npc, pc)");
                    they_attack(npc, pc);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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
