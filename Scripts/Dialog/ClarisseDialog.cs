
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
    [DialogScript(289)]
    public class ClarisseDialog : Clarisse, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 9037)";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(9037));
                case 14:
                    originalScript = "game.quests[96].state != qs_accepted";
                    return GetQuestState(96) != QuestState.Accepted;
                case 16:
                    originalScript = "game.quests[96].state == qs_accepted";
                    return GetQuestState(96) == QuestState.Accepted;
                case 21:
                case 22:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 23:
                case 24:
                    originalScript = "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == LAWFUL_EVIL)";
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 28:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female and game.global_flags[68] == 1";
                    return pc.GetGender() == Gender.Female && GetGlobalFlag(68);
                case 55:
                case 56:
                case 73:
                    originalScript = "pc.stat_level_get( stat_deity ) == 16";
                    return pc.GetStat(Stat.deity) == 16;
                case 61:
                case 64:
                case 65:
                    originalScript = "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL)";
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 83:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 91:
                case 92:
                    originalScript = "pc.stat_level_get(stat_level_cleric) >= 1";
                    return pc.GetStat(Stat.level_cleric) >= 1;
                case 93:
                    originalScript = "pc.stat_level_get(stat_level_cleric) <= 0";
                    return pc.GetStat(Stat.level_cleric) <= 0;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                case 24:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 31:
                    originalScript = "party_transfer_to( npc, 9037 )";
                    Utilities.party_transfer_to(npc, 9037);
                    break;
                case 50:
                    originalScript = "game.quests[96].state = qs_mentioned";
                    SetQuestState(96, QuestState.Mentioned);
                    break;
                case 80:
                case 90:
                    originalScript = "game.quests[96].state = qs_accepted";
                    SetQuestState(96, QuestState.Accepted);
                    break;
                case 100:
                    originalScript = "run_off( npc, pc )";
                    throw new NotSupportedException("Clarisse's Python script doesn't implement run_off.");
                case 101:
                    originalScript = "game.quests[96].state = qs_botched";
                    SetQuestState(96, QuestState.Botched);
                    break;
                case 110:
                    originalScript = "game.particles( 'sp-Bless Water', npc )";
                    AttachParticles("sp-Bless Water", npc);
                    break;
                case 111:
                    originalScript = "create_item_in_inventory(12898,pc); game.quests[96].state = qs_completed;  npc.destroy()";
                    Utilities.create_item_in_inventory(12898, pc);
                    SetQuestState(96, QuestState.Completed);
                    npc.Destroy();
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
