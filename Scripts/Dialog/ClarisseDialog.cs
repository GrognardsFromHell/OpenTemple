
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
    [DialogScript(289)]
    public class ClarisseDialog : Clarisse, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 9037)");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(9037));
                case 14:
                    Trace.Assert(originalScript == "game.quests[96].state != qs_accepted");
                    return GetQuestState(96) != QuestState.Accepted;
                case 16:
                    Trace.Assert(originalScript == "game.quests[96].state == qs_accepted");
                    return GetQuestState(96) == QuestState.Accepted;
                case 21:
                case 22:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 23:
                case 24:
                    Trace.Assert(originalScript == "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == LAWFUL_EVIL)");
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 28:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_female and game.global_flags[68] == 1");
                    return pc.GetGender() == Gender.Female && GetGlobalFlag(68);
                case 55:
                case 56:
                case 73:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) == 16");
                    return pc.GetStat(Stat.deity) == 16;
                case 61:
                case 64:
                case 65:
                    Trace.Assert(originalScript == "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL)");
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 83:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 91:
                case 92:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_cleric) >= 1");
                    return pc.GetStat(Stat.level_cleric) >= 1;
                case 93:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_cleric) <= 0");
                    return pc.GetStat(Stat.level_cleric) <= 0;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                case 24:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 31:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 9037 )");
                    Utilities.party_transfer_to(npc, 9037);
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.quests[96].state = qs_mentioned");
                    SetQuestState(96, QuestState.Mentioned);
                    break;
                case 80:
                case 90:
                    Trace.Assert(originalScript == "game.quests[96].state = qs_accepted");
                    SetQuestState(96, QuestState.Accepted);
                    break;
                case 100:
                    Trace.Assert(originalScript == "run_off( npc, pc )");
                    throw new NotSupportedException("Clarisse's Python script doesn't implement run_off.");
                case 101:
                    Trace.Assert(originalScript == "game.quests[96].state = qs_botched");
                    SetQuestState(96, QuestState.Botched);
                    break;
                case 110:
                    Trace.Assert(originalScript == "game.particles( 'sp-Bless Water', npc )");
                    AttachParticles("sp-Bless Water", npc);
                    break;
                case 111:
                    Trace.Assert(originalScript == "create_item_in_inventory(12898,pc); game.quests[96].state = qs_completed;  npc.destroy()");
                    Utilities.create_item_in_inventory(12898, pc);
                    SetQuestState(96, QuestState.Completed);
                    npc.Destroy();
                    ;
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
                case 83:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}