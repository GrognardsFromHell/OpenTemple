
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
    [DialogScript(99)]
    public class JeneldaDialog : Jenelda, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 4:
                    Trace.Assert(originalScript == "game.global_flags[76] == 1 and pc.stat_level_get( stat_gender ) == gender_male");
                    return GetGlobalFlag(76) && pc.GetGender() == Gender.Male;
                case 5:
                case 6:
                    Trace.Assert(originalScript == "game.global_flags[76] == 1 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetGlobalFlag(76) && pc.GetGender() == Gender.Female;
                case 21:
                    Trace.Assert(originalScript == "game.global_flags[76] == 0 or (game.global_flags[82] == 0 and game.quests[33].state == qs_completed)");
                    return !GetGlobalFlag(76) || (!GetGlobalFlag(82) && GetQuestState(33) == QuestState.Completed);
                case 22:
                case 26:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male and game.quests[33].state != qs_completed and game.global_flags[76] == 1");
                    return pc.GetGender() == Gender.Male && GetQuestState(33) != QuestState.Completed && GetGlobalFlag(76);
                case 23:
                case 27:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_female and game.quests[33].state != qs_completed and game.global_flags[76] == 1");
                    return pc.GetGender() == Gender.Female && GetQuestState(33) != QuestState.Completed && GetGlobalFlag(76);
                case 24:
                case 28:
                    Trace.Assert(originalScript == "game.global_flags[82] == 1 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetGlobalFlag(82) && pc.GetGender() == Gender.Female;
                case 25:
                case 29:
                    Trace.Assert(originalScript == "game.global_flags[83] == 1 and game.global_flags[76] == 1 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetGlobalFlag(83) && GetGlobalFlag(76) && pc.GetGender() == Gender.Female;
                case 30:
                    Trace.Assert(originalScript == "game.global_flags[83] == 1 and game.global_flags[76] == 0");
                    return GetGlobalFlag(83) && !GetGlobalFlag(76);
                case 31:
                case 32:
                    Trace.Assert(originalScript == "game.quests[33].state == qs_accepted and game.global_flags[76] == 1");
                    return GetQuestState(33) == QuestState.Accepted && GetGlobalFlag(76);
                case 33:
                case 34:
                    Trace.Assert(originalScript == "game.global_flags[83] == 1 and game.global_flags[76] == 0 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetGlobalFlag(83) && !GetGlobalFlag(76) && pc.GetGender() == Gender.Female;
                case 43:
                case 44:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidation) >= 9");
                    throw new NotSupportedException("Conversion failed.");
                case 45:
                case 46:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 52:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 61:
                case 62:
                case 123:
                case 124:
                case 133:
                case 134:
                    Trace.Assert(originalScript == "game.quests[33].state == qs_accepted");
                    return GetQuestState(33) == QuestState.Accepted;
                case 63:
                case 64:
                case 131:
                case 132:
                    Trace.Assert(originalScript == "game.quests[33].state <= qs_mentioned");
                    return GetQuestState(33) <= QuestState.Mentioned;
                case 191:
                case 192:
                case 201:
                case 202:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male");
                    return pc.GetGender() == Gender.Male;
                case 193:
                case 194:
                case 203:
                case 204:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_female");
                    return pc.GetGender() == Gender.Female;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 24:
                case 28:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+10)");
                    npc.AdjustReaction(pc, +10);
                    break;
                case 25:
                case 29:
                    Trace.Assert(originalScript == "game.global_flags[76] = 0");
                    SetGlobalFlag(76, false);
                    break;
                case 50:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,-10)");
                    npc.AdjustReaction(pc, -10);
                    break;
                case 51:
                    Trace.Assert(originalScript == "game.fade(28800,0,0,4)");
                    Fade(28800, 0, 0, 4);
                    break;
                case 52:
                    Trace.Assert(originalScript == "game.fade(28800,4047,0,4); get_rep( npc, pc )");
                    Fade(28800, 4047, 0, 4);
                    get_rep(npc, pc);
                    ;
                    break;
                case 80:
                    Trace.Assert(originalScript == "game.quests[33].state = qs_mentioned");
                    SetQuestState(33, QuestState.Mentioned);
                    break;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "game.quests[33].state = qs_accepted");
                    SetQuestState(33, QuestState.Accepted);
                    break;
                case 100:
                    Trace.Assert(originalScript == "game.global_flags[85] = 1; game.global_flags[75] = 1");
                    SetGlobalFlag(85, true);
                    SetGlobalFlag(75, true);
                    ;
                    break;
                case 135:
                case 136:
                case 143:
                case 144:
                case 161:
                case 162:
                case 181:
                case 182:
                case 183:
                case 193:
                case 194:
                    Trace.Assert(originalScript == "game.fade(28800,4047,0,4)");
                    Fade(28800, 4047, 0, 4);
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.global_flags[82] = 0");
                    SetGlobalFlag(82, false);
                    break;
                case 170:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+30)");
                    npc.AdjustReaction(pc, +30);
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
                case 45:
                case 46:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
