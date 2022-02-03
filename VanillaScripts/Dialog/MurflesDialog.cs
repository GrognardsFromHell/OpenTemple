
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

namespace VanillaScripts.Dialog
{
    [DialogScript(112)]
    public class MurflesDialog : Murfles, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 8:
                case 9:
                case 357:
                case 358:
                    originalScript = "game.quests[31].state == qs_completed";
                    return GetQuestState(31) == QuestState.Completed;
                case 22:
                case 23:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 7;
                case 53:
                case 54:
                case 121:
                case 122:
                case 135:
                case 136:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 55:
                case 56:
                case 131:
                case 132:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_NEUTRAL) or (game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 61:
                case 62:
                    originalScript = "game.story_state == 3";
                    return StoryState == 3;
                case 63:
                case 64:
                    originalScript = "game.story_state == 4";
                    return StoryState == 4;
                case 65:
                case 66:
                    originalScript = "game.story_state == 5";
                    return StoryState == 5;
                case 67:
                case 68:
                case 253:
                case 254:
                    originalScript = "game.story_state == 6";
                    return StoryState == 6;
                case 81:
                case 82:
                case 91:
                case 92:
                case 101:
                case 102:
                    originalScript = "game.global_flags[98] == 0";
                    return !GetGlobalFlag(98);
                case 83:
                case 84:
                case 93:
                case 94:
                case 103:
                case 104:
                    originalScript = "game.global_flags[98] == 1";
                    return GetGlobalFlag(98);
                case 123:
                case 124:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 125:
                case 126:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 133:
                case 134:
                    originalScript = "pc.stat_level_get( stat_level_druid ) >= 1";
                    return pc.GetStat(Stat.level_druid) >= 1;
                case 195:
                case 196:
                    originalScript = "game.global_flags[306] == 1 and game.global_flags[307] == 0";
                    return GetGlobalFlag(306) && !GetGlobalFlag(307);
                case 321:
                case 322:
                    originalScript = "game.party_npc_size() <= 2";
                    return GameSystems.Party.NPCFollowersSize <= 2;
                case 325:
                case 326:
                    originalScript = "game.party_npc_size() >= 3";
                    return GameSystems.Party.NPCFollowersSize >= 3;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    originalScript = "npc.reaction_adj( pc,-10)";
                    npc.AdjustReaction(pc, -10);
                    break;
                case 43:
                    originalScript = "buttin(npc,pc,380)";
                    buttin(npc, pc, 380);
                    break;
                case 141:
                case 142:
                    originalScript = "game.global_flags[98] = 1; npc.item_transfer_to_by_proto(pc,5008)";
                    SetGlobalFlag(98, true);
                    npc.TransferItemByProtoTo(pc, 5008);
                    ;
                    break;
                case 171:
                case 172:
                case 231:
                case 232:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 173:
                case 174:
                    originalScript = "make_hate( npc, pc )";
                    make_hate(npc, pc);
                    break;
                case 195:
                case 196:
                    originalScript = "game.global_flags[307] = 1";
                    SetGlobalFlag(307, true);
                    break;
                case 230:
                    originalScript = "game.global_flags[313] = 1";
                    SetGlobalFlag(313, true);
                    break;
                case 330:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
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
                case 22:
                case 23:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
