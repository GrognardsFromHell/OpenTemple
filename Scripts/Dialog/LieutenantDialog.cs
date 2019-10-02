
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
    [DialogScript(76)]
    public class LieutenantDialog : Lieutenant, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "game.global_flags[48] == 1");
                    return GetGlobalFlag(48);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "game.quests[16].state == qs_completed and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)");
                    return GetQuestState(16) == QuestState.Completed && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 6:
                case 7:
                    Trace.Assert(originalScript == "game.quests[16].state == qs_completed and (game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL)");
                    return GetQuestState(16) == QuestState.Completed && (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 10:
                    Trace.Assert(originalScript == "pc != game.party[0] and game.party[0].distance_to(pc) <= 40 and not critter_is_unconscious(game.party[0]) and anyone(game.party[0].group_list(), \"has_wielded\", 3005)");
                    return pc != PartyLeader && PartyLeader.DistanceTo(pc) <= 40 && !Utilities.critter_is_unconscious(PartyLeader) && PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
                case 31:
                case 32:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 7;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    Trace.Assert(originalScript == "call_leader(npc, pc)");
                    call_leader(npc, pc);
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.global_flags[52] = 1");
                    SetGlobalFlag(52, true);
                    break;
                case 81:
                case 101:
                case 102:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
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
                case 31:
                case 32:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
