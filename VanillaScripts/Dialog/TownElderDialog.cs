
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
    [DialogScript(92)]
    public class TownElderDialog : TownElder, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 20:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.LAWFUL_GOOD && !GetGlobalFlag(67);
                case 12:
                case 21:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD && !GetGlobalFlag(67);
                case 14:
                case 23:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL && !GetGlobalFlag(67);
                case 15:
                case 24:
                    Trace.Assert(originalScript == "game.party_alignment == TRUE_NEUTRAL and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(67);
                case 16:
                case 25:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD && !GetGlobalFlag(67);
                case 17:
                case 26:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL && !GetGlobalFlag(67);
                case 18:
                case 27:
                case 205:
                case 206:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && !GetGlobalFlag(67);
                case 19:
                case 28:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL && !GetGlobalFlag(67);
                case 33:
                case 34:
                case 209:
                case 210:
                    Trace.Assert(originalScript == "game.quests[7].state == qs_accepted");
                    return GetQuestState(7) == QuestState.Accepted;
                case 123:
                case 124:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 2 and anyone( pc.group_list(), \"has_follower\", 8000 )");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 2 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 125:
                case 126:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 2 and not anyone( pc.group_list(), \"has_follower\", 8000 )");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 2 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 181:
                case 182:
                    Trace.Assert(originalScript == "game.story_state == 0");
                    return StoryState == 0;
                case 183:
                case 184:
                    Trace.Assert(originalScript == "game.story_state == 1");
                    return StoryState == 1;
                case 185:
                case 186:
                    Trace.Assert(originalScript == "game.story_state == 2");
                    return StoryState == 2;
                case 187:
                case 188:
                    Trace.Assert(originalScript == "game.story_state >= 3");
                    return StoryState >= 3;
                case 207:
                case 208:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL and (game.story_state == 2 or game.story_state == 3)");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && (StoryState == 2 || StoryState == 3);
                case 211:
                case 212:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL and game.story_state >= 4");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && StoryState >= 4;
                case 221:
                case 222:
                    Trace.Assert(originalScript == "game.areas[3] == 1");
                    return IsAreaKnown(3);
                case 223:
                case 224:
                    Trace.Assert(originalScript == "game.areas[3] == 0");
                    return !IsAreaKnown(3);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                case 6:
                case 101:
                case 102:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,-10 )");
                    npc.AdjustReaction(pc, -10);
                    break;
                case 18:
                case 27:
                case 205:
                case 206:
                    Trace.Assert(originalScript == "game.global_flags[67] = 1");
                    SetGlobalFlag(67, true);
                    break;
                case 170:
                    Trace.Assert(originalScript == "game.quests[25].state = qs_completed");
                    SetQuestState(25, QuestState.Completed);
                    break;
                case 196:
                case 270:
                    Trace.Assert(originalScript == "game.areas[2] = 1; game.story_state = 1");
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 197:
                case 271:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(2)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 230:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 231:
                case 232:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
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
