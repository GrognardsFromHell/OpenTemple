
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
    [DialogScript(92)]
    public class TownElderDialog : TownElder, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 7:
                case 18:
                case 27:
                case 205:
                case 206:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && !GetGlobalFlag(67);
                case 11:
                case 20:
                    originalScript = "game.party_alignment == LAWFUL_GOOD and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.LAWFUL_GOOD && !GetGlobalFlag(67);
                case 12:
                case 21:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD && !GetGlobalFlag(67);
                case 14:
                case 23:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL && !GetGlobalFlag(67);
                case 15:
                case 24:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(67);
                case 16:
                case 25:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD && !GetGlobalFlag(67);
                case 17:
                case 26:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL && !GetGlobalFlag(67);
                case 19:
                case 28:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL && !GetGlobalFlag(67);
                case 33:
                case 34:
                case 211:
                case 212:
                    originalScript = "game.quests[7].state == qs_accepted";
                    return GetQuestState(7) == QuestState.Accepted;
                case 123:
                case 124:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 2 and anyone( pc.group_list(), \"has_follower\", 8000 ) and game.global_flags[44] == 0 and game.global_flags[45] == 0";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 2 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000)) && !GetGlobalFlag(44) && !GetGlobalFlag(45);
                case 125:
                case 126:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 2 and not anyone( pc.group_list(), \"has_follower\", 8000 ) and game.global_flags[934] == 0";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 2 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000)) && !GetGlobalFlag(934);
                case 136:
                case 137:
                    originalScript = "game.global_vars[750] == 0 and game.global_vars[751] == 0 and game.global_flags[814] == 0 and game.global_flags[815] == 0";
                    return GetGlobalVar(750) == 0 && GetGlobalVar(751) == 0 && !GetGlobalFlag(814) && !GetGlobalFlag(815);
                case 171:
                case 172:
                case 321:
                case 322:
                    originalScript = "game.global_vars[562] == 0";
                    return GetGlobalVar(562) == 0;
                case 173:
                case 174:
                case 323:
                case 324:
                    originalScript = "game.global_vars[562] != 0";
                    return GetGlobalVar(562) != 0;
                case 181:
                case 182:
                case 351:
                case 352:
                    originalScript = "game.story_state == 0";
                    return StoryState == 0;
                case 183:
                case 184:
                    originalScript = "game.story_state == 1 and game.quests[72].state == qs_unknown";
                    return StoryState == 1 && GetQuestState(72) == QuestState.Unknown;
                case 185:
                case 186:
                    originalScript = "game.story_state == 1 and game.quests[72].state != qs_unknown";
                    return StoryState == 1 && GetQuestState(72) != QuestState.Unknown;
                case 187:
                case 188:
                case 361:
                case 362:
                    originalScript = "game.story_state == 2";
                    return StoryState == 2;
                case 189:
                case 190:
                case 363:
                case 364:
                    originalScript = "game.story_state >= 3";
                    return StoryState >= 3;
                case 207:
                case 208:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL and (game.story_state == 2 or game.story_state == 3) and game.global_vars[562] != 5";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && (StoryState == 2 || StoryState == 3) && GetGlobalVar(562) != 5;
                case 209:
                case 210:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL and (game.story_state == 2 or game.story_state == 3) and game.global_vars[562] == 5";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && (StoryState == 2 || StoryState == 3) && GetGlobalVar(562) == 5;
                case 213:
                case 214:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL and game.story_state >= 4";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && StoryState >= 4;
                case 216:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL and game.story_state == 1 and game.global_flags[55] == 1";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && StoryState == 1 && GetGlobalFlag(55);
                case 251:
                    originalScript = "game.global_flags[411] == 0";
                    return !GetGlobalFlag(411);
                case 331:
                case 332:
                case 341:
                case 342:
                    originalScript = "game.areas[3] == 1";
                    return IsAreaKnown(3);
                case 333:
                case 334:
                case 343:
                case 344:
                    originalScript = "game.areas[3] == 0";
                    return !IsAreaKnown(3);
                case 353:
                case 354:
                case 371:
                case 372:
                    originalScript = "game.global_vars[562] == 1 and game.story_state <= 1";
                    return GetGlobalVar(562) == 1 && StoryState <= 1;
                case 355:
                case 356:
                case 373:
                case 374:
                    originalScript = "game.global_vars[562] == 2 and game.story_state <= 1";
                    return GetGlobalVar(562) == 2 && StoryState <= 1;
                case 357:
                case 358:
                case 375:
                case 376:
                    originalScript = "game.global_vars[562] == 3 and game.story_state <= 1";
                    return GetGlobalVar(562) == 3 && StoryState <= 1;
                case 359:
                case 360:
                case 377:
                case 378:
                    originalScript = "game.global_vars[562] == 4 and game.story_state <= 1";
                    return GetGlobalVar(562) == 4 && StoryState <= 1;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                case 6:
                case 101:
                case 102:
                    originalScript = "npc.reaction_adj( pc,-10 )";
                    npc.AdjustReaction(pc, -10);
                    break;
                case 18:
                case 27:
                case 205:
                case 206:
                    originalScript = "game.global_flags[67] = 1";
                    SetGlobalFlag(67, true);
                    break;
                case 170:
                    originalScript = "game.quests[25].state = qs_completed";
                    SetQuestState(25, QuestState.Completed);
                    break;
                case 183:
                case 184:
                case 198:
                case 272:
                    originalScript = "game.quests[72].state = qs_accepted; game.global_vars[562] = 5";
                    SetQuestState(72, QuestState.Accepted);
                    SetGlobalVar(562, 5);
                    ;
                    break;
                case 196:
                case 270:
                    originalScript = "game.areas[2] = 1; game.story_state = 1; game.quests[72].state = qs_mentioned";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    SetQuestState(72, QuestState.Mentioned);
                    ;
                    break;
                case 197:
                case 271:
                    originalScript = "game.quests[72].state = qs_accepted; game.global_vars[562] = 5; game.worldmap_travel_by_dialog(2)";
                    SetQuestState(72, QuestState.Accepted);
                    SetGlobalVar(562, 5);
                    WorldMapTravelByDialog(2);
                    break;
                case 230:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 231:
                case 232:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    WorldMapTravelByDialog(3);
                    break;
                case 340:
                    originalScript = "game.quests[72].state = qs_completed; game.global_vars[562] = 6";
                    SetQuestState(72, QuestState.Completed);
                    SetGlobalVar(562, 6);
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
                case 123:
                case 124:
                case 125:
                case 126:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 2);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
