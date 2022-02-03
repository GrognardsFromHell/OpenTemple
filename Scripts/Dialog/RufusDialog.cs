
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
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

namespace Scripts.Dialog;

[DialogScript(18)]
public class RufusDialog : Rufus, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
            case 321:
            case 322:
                originalScript = "not npc.has_met(pc)";
                return !npc.HasMet(pc);
            case 4:
            case 323:
            case 333:
            case 341:
                originalScript = "npc.has_met(pc) and game.story_state <= 1";
                return npc.HasMet(pc) && StoryState <= 1;
            case 5:
            case 324:
            case 334:
            case 342:
                originalScript = "npc.has_met(pc) and game.story_state >= 2";
                return npc.HasMet(pc) && StoryState >= 2;
            case 6:
            case 8:
            case 325:
            case 326:
            case 343:
            case 344:
                originalScript = "game.global_flags[31] == 1 and game.quests[15].state != qs_completed";
                return GetGlobalFlag(31) && GetQuestState(15) != QuestState.Completed;
            case 51:
                originalScript = "game.quests[15].state == qs_unknown";
                return GetQuestState(15) == QuestState.Unknown;
            case 52:
                originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[2] == 0 and game.quests[15].state != qs_unknown";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(2) && GetQuestState(15) != QuestState.Unknown;
            case 53:
                originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[2] == 0 and game.quests[15].state != qs_unknown";
                return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(2) && GetQuestState(15) != QuestState.Unknown;
            case 82:
            case 86:
                originalScript = "pc.money_get() >= 1000";
                return pc.GetMoney() >= 1000;
            case 83:
            case 87:
                originalScript = "pc.money_get() >= 5000";
                return pc.GetMoney() >= 5000;
            case 84:
            case 88:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 17";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 17;
            case 102:
            case 107:
                originalScript = "game.quests[15].state == qs_unknown or game.areas[2] == 0";
                return GetQuestState(15) == QuestState.Unknown || !IsAreaKnown(2);
            case 103:
            case 108:
                originalScript = "game.quests[15].state == qs_mentioned";
                return GetQuestState(15) == QuestState.Mentioned;
            case 104:
            case 109:
                originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.story_state == 0";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && StoryState == 0;
            case 105:
            case 110:
                originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.story_state == 0";
                return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && StoryState == 0;
            case 121:
            case 126:
                originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[3] == 0 and game.global_vars[562] != 2";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(3) && GetGlobalVar(562) != 2;
            case 122:
            case 127:
                originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[3] == 0 and game.global_vars[562] == 2";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(3) && GetGlobalVar(562) == 2;
            case 123:
            case 128:
                originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[3] == 1 and game.global_vars[562] == 2";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && IsAreaKnown(3) && GetGlobalVar(562) == 2;
            case 124:
            case 129:
                originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 0 and game.global_vars[562] != 2";
                return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(3) && GetGlobalVar(562) != 2;
            case 125:
                originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.global_vars[562] == 2";
                return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && GetGlobalVar(562) == 2;
            case 130:
                originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 0 and game.global_vars[562] == 2";
                return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(3) && GetGlobalVar(562) == 2;
            case 131:
                originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 1 and game.global_vars[562] == 2";
                return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && IsAreaKnown(3) && GetGlobalVar(562) == 2;
            case 134:
            case 135:
                originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL) and game.story_state >= 4 and game.global_flags[839] == 0 and game.global_flags[988] == 0";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL) && StoryState >= 4 && !GetGlobalFlag(839) && !GetGlobalFlag(988);
            case 183:
            case 184:
                originalScript = "pc.follower_atmax() == 0";
                return !pc.HasMaxFollowers();
            case 185:
            case 186:
                originalScript = "pc.follower_atmax() == 1";
                return pc.HasMaxFollowers();
            case 191:
            case 192:
                originalScript = "game.party[0].reputation_has( 23 ) == 0 and game.global_flags[814] == 0 and game.global_flags[815] == 0";
                return !PartyLeader.HasReputation(23) && !GetGlobalFlag(814) && !GetGlobalFlag(815);
            case 193:
            case 194:
                originalScript = "game.party[0].reputation_has( 23 ) == 1 or (game.global_flags[814] == 1 and game.global_flags[815] == 1)";
                return PartyLeader.HasReputation(23) || (GetGlobalFlag(814) && GetGlobalFlag(815));
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1:
                originalScript = "game.global_vars[108] = 1";
                SetGlobalVar(108, 1);
                break;
            case 60:
                originalScript = "game.quests[15].state = qs_mentioned";
                SetQuestState(15, QuestState.Mentioned);
                break;
            case 61:
            case 62:
            case 103:
            case 108:
                originalScript = "game.quests[15].state = qs_accepted";
                SetQuestState(15, QuestState.Accepted);
                break;
            case 70:
            case 140:
            case 270:
                originalScript = "game.areas[2] = 1; game.story_state = 1; game.quests[72].state = qs_mentioned";
                MakeAreaKnown(2);
                StoryState = 1;
                SetQuestState(72, QuestState.Mentioned);
                ;
                break;
            case 71:
            case 72:
            case 143:
            case 144:
            case 273:
            case 274:
                originalScript = "game.quests[72].state = qs_accepted; game.global_vars[562] = 2";
                SetQuestState(72, QuestState.Accepted);
                SetGlobalVar(562, 2);
                ;
                break;
            case 73:
            case 74:
            case 141:
            case 142:
            case 271:
            case 272:
                originalScript = "game.quests[72].state = qs_accepted; game.global_vars[562] = 2; game.worldmap_travel_by_dialog(2)";
                SetQuestState(72, QuestState.Accepted);
                SetGlobalVar(562, 2);
                WorldMapTravelByDialog(2);
                break;
            case 82:
            case 86:
                originalScript = "pc.money_adj(-1000)";
                pc.AdjustMoney(-1000);
                break;
            case 83:
            case 87:
                originalScript = "pc.money_adj(-5000)";
                pc.AdjustMoney(-5000);
                break;
            case 150:
            case 160:
                originalScript = "game.areas[3] = 1; game.story_state = 3";
                MakeAreaKnown(3);
                StoryState = 3;
                ;
                break;
            case 151:
            case 152:
            case 163:
            case 164:
            case 281:
            case 282:
            case 291:
            case 292:
            case 303:
            case 304:
            case 313:
            case 314:
                originalScript = "game.worldmap_travel_by_dialog(3)";
                WorldMapTravelByDialog(3);
                break;
            case 183:
            case 184:
                originalScript = "pc.follower_add( npc )";
                pc.AddFollower(npc);
                break;
            case 191:
            case 192:
            case 193:
            case 194:
                originalScript = "game.global_flags[694] = 0; game.quests[15].state = qs_completed";
                SetGlobalFlag(694, false);
                SetQuestState(15, QuestState.Completed);
                ;
                break;
            case 195:
            case 196:
                originalScript = "game.global_flags[694] = 1; pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                SetGlobalFlag(694, true);
                pc.AddCondition("Fallen_Paladin", 0, 0);
                ;
                break;
            case 200:
            case 260:
                originalScript = "pc.money_adj(+10000)";
                pc.AdjustMoney(+10000);
                break;
            case 221:
            case 231:
                originalScript = "pc.follower_remove( npc )";
                pc.RemoveFollower(npc);
                break;
            case 280:
            case 300:
                originalScript = "game.areas[3] = 1; game.story_state = 3; game.quests[72].state = qs_completed; game.global_vars[562] = 6";
                MakeAreaKnown(3);
                StoryState = 3;
                SetQuestState(72, QuestState.Completed);
                SetGlobalVar(562, 6);
                ;
                break;
            case 290:
            case 310:
                originalScript = "game.quests[72].state = qs_completed; game.global_vars[562] = 6";
                SetQuestState(72, QuestState.Completed);
                SetGlobalVar(562, 6);
                ;
                break;
            case 22000:
                originalScript = "game.global_vars[912] = 32";
                SetGlobalVar(912, 32);
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
            case 84:
            case 88:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 17);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}