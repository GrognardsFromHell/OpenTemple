
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
    [DialogScript(9)]
    public class ChurchServingWomanDialog : ChurchServingWoman, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 61:
                case 62:
                    Trace.Assert(originalScript == "game.story_state >= 1");
                    return StoryState >= 1;
                case 161:
                case 162:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 191:
                case 192:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_unknown");
                    return GetQuestState(8) == QuestState.Unknown;
                case 193:
                case 194:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_mentioned");
                    return GetQuestState(8) == QuestState.Mentioned;
                case 195:
                case 196:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_completed and game.quests[98].state == qs_accepted and game.quests[97].state != qs_completed");
                    return GetQuestState(8) == QuestState.Completed && GetQuestState(98) == QuestState.Accepted && GetQuestState(97) != QuestState.Completed;
                case 197:
                    Trace.Assert(originalScript == "game.global_flags[11] == 0");
                    return !GetGlobalFlag(11);
                case 198:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 12899) and game.quests[97].state != qs_completed");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(12899)) && GetQuestState(97) != QuestState.Completed;
                case 199:
                case 200:
                    Trace.Assert(originalScript == "game.quests[8].state >= qs_mentioned");
                    return GetQuestState(8) >= QuestState.Mentioned;
                case 501:
                case 502:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_GOOD or game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 515:
                case 516:
                case 521:
                case 522:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    Trace.Assert(originalScript == "game.quests[8].state = qs_mentioned");
                    SetQuestState(8, QuestState.Mentioned);
                    break;
                case 110:
                case 121:
                case 141:
                    Trace.Assert(originalScript == "game.quests[8].state = qs_accepted; npc.reaction_adj( pc,+10)");
                    SetQuestState(8, QuestState.Accepted);
                    npc.AdjustReaction(pc, +10);
                    ;
                    break;
                case 170:
                    Trace.Assert(originalScript == "game.global_flags[11] = 1");
                    SetGlobalFlag(11, true);
                    break;
                case 510:
                    Trace.Assert(originalScript == "game.quests[97].state = qs_mentioned");
                    SetQuestState(97, QuestState.Mentioned);
                    break;
                case 513:
                case 514:
                    Trace.Assert(originalScript == "game.quests[97].state = qs_accepted; game.fade_and_teleport(0,0,0,5001,624,301)");
                    SetQuestState(97, QuestState.Accepted);
                    FadeAndTeleport(0, 0, 0, 5001, 624, 301);
                    ;
                    break;
                case 515:
                case 516:
                case 517:
                case 518:
                case 521:
                case 522:
                case 525:
                    Trace.Assert(originalScript == "game.quests[97].state = qs_accepted");
                    SetQuestState(97, QuestState.Accepted);
                    break;
                case 523:
                case 524:
                    Trace.Assert(originalScript == "game.quests[97].state = qs_accepted; game.fade_and_teleport( 0,0,0,5120,489,485 )");
                    SetQuestState(97, QuestState.Accepted);
                    FadeAndTeleport(0, 0, 0, 5120, 489, 485);
                    ;
                    break;
                case 530:
                    Trace.Assert(originalScript == "game.quests[97].state = qs_completed");
                    SetQuestState(97, QuestState.Completed);
                    break;
                case 531:
                case 532:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 12899 )");
                    Utilities.party_transfer_to(npc, 12899);
                    break;
                case 541:
                case 542:
                case 543:
                    Trace.Assert(originalScript == "create_item_in_inventory(12894,pc)");
                    Utilities.create_item_in_inventory(12894, pc);
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
