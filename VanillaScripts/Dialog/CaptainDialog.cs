
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
    [DialogScript(6)]
    public class CaptainDialog : Captain, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 163:
                case 164:
                case 173:
                case 174:
                    Trace.Assert(originalScript == "game.quests[1].state == qs_accepted and npc.has_met( pc )");
                    return GetQuestState(1) == QuestState.Accepted && npc.HasMet(pc);
                case 4:
                case 5:
                case 161:
                case 162:
                case 171:
                case 172:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 9:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 21:
                case 28:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8000 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 22:
                case 26:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 7;
                case 23:
                case 25:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 9");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 9;
                case 24:
                case 27:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 4");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4;
                case 101:
                case 102:
                    Trace.Assert(originalScript == "game.global_flags[67] == 0");
                    return !GetGlobalFlag(67);
                case 103:
                case 104:
                    Trace.Assert(originalScript == "game.quests[1].state == qs_accepted");
                    return GetQuestState(1) == QuestState.Accepted;
                case 105:
                case 106:
                    Trace.Assert(originalScript == "game.global_flags[70] == 0");
                    return !GetGlobalFlag(70);
                case 111:
                case 113:
                case 151:
                case 152:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 8000 )");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 112:
                case 114:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 8000 ) and game.global_flags[70] == 0");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000)) && !GetGlobalFlag(70);
                case 501:
                case 510:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    Trace.Assert(originalScript == "game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 30:
                case 60:
                    Trace.Assert(originalScript == "game.quests[1].state = qs_completed");
                    SetQuestState(1, QuestState.Completed);
                    break;
                case 80:
                    Trace.Assert(originalScript == "game.quests[1].state = qs_completed; npc.reaction_adj( pc,-10)");
                    SetQuestState(1, QuestState.Completed);
                    npc.AdjustReaction(pc, -10);
                    ;
                    break;
                case 90:
                    Trace.Assert(originalScript == "game.quests[1].state = qs_completed; game.global_vars[1] = 1");
                    SetQuestState(1, QuestState.Completed);
                    SetGlobalVar(1, 1);
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
                case 22:
                case 26:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 7);
                    return true;
                case 23:
                case 25:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 9);
                    return true;
                case 24:
                case 27:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
