
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

namespace Scripts.Dialog
{
    [DialogScript(176)]
    public class ZuggtmoyDialog : Zuggtmoy, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 45:
                case 46:
                case 185:
                case 186:
                case 193:
                case 194:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 101:
                case 102:
                    originalScript = "game.global_flags[146] == 0 and ((game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL))";
                    return !GetGlobalFlag(146) && ((PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL));
                case 103:
                case 104:
                    originalScript = "game.global_flags[146] == 1 and ((game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL))";
                    return GetGlobalFlag(146) && ((PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL));
                case 105:
                case 106:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 261:
                case 311:
                    originalScript = "not pc.has_feat(feat_slippery_mind)";
                    return !pc.HasFeat(FeatId.SLIPPERY_MIND);
                case 262:
                case 312:
                    originalScript = "pc.has_feat(feat_slippery_mind)";
                    return pc.HasFeat(FeatId.SLIPPERY_MIND);
                case 301:
                case 302:
                    originalScript = "pc.item_find(2203) == OBJ_HANDLE_NULL";
                    return pc.FindItemByName(2203) == null;
                case 303:
                case 304:
                    originalScript = "pc.item_find(2203) != OBJ_HANDLE_NULL";
                    return pc.FindItemByName(2203) != null;
                case 331:
                case 332:
                    originalScript = "pc.item_find(2203) != OBJ_HANDLE_NULL and (game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)";
                    return pc.FindItemByName(2203) != null && (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 371:
                    originalScript = "game.global_flags[500] == 0";
                    return !GetGlobalFlag(500);
                case 372:
                    originalScript = "game.global_flags[500] == 1";
                    return GetGlobalFlag(500);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 51:
                    originalScript = "game.global_flags[183] = 1; transform_into_demon_form(npc,pc,70)";
                    SetGlobalFlag(183, true);
                    transform_into_demon_form(npc, pc, 70);
                    ;
                    break;
                case 61:
                    originalScript = "crone_wait(npc,pc)";
                    crone_wait(npc, pc);
                    break;
                case 107:
                case 108:
                case 109:
                case 151:
                case 335:
                case 336:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 141:
                    originalScript = "game.global_flags[184] = 1; zuggtmoy_end_game(npc,pc)";
                    SetGlobalFlag(184, true);
                    zuggtmoy_end_game(npc, pc);
                    ;
                    break;
                case 211:
                case 212:
                    originalScript = "pc.item_transfer_to(npc,2203); game.global_flags[190] = 1; transform_into_demon_form(npc,pc,230)";
                    pc.TransferItemByNameTo(npc, 2203);
                    SetGlobalFlag(190, true);
                    transform_into_demon_form(npc, pc, 230);
                    ;
                    break;
                case 221:
                    originalScript = "transform_into_demon_form(npc,pc,260)";
                    transform_into_demon_form(npc, pc, 260);
                    break;
                case 241:
                    originalScript = "zuggtmoy_pillar_gone(npc,pc)";
                    zuggtmoy_pillar_gone(npc, pc);
                    break;
                case 251:
                    originalScript = "zuggtmoy_pc_charm(npc,pc)";
                    zuggtmoy_pc_charm(npc, pc);
                    break;
                case 261:
                case 311:
                case 391:
                    originalScript = "zuggtmoy_pc_persuade(npc,pc,270,280)";
                    zuggtmoy_pc_persuade(npc, pc, 270, 280);
                    break;
                case 262:
                case 312:
                    originalScript = "zuggtmoy_pc_persuade(npc,pc,270,390)";
                    zuggtmoy_pc_persuade(npc, pc, 270, 390);
                    break;
                case 291:
                    originalScript = "pc.item_transfer_to(npc,2203); game.global_flags[191] = 1; zuggtmoy_pc_charm(npc,pc)";
                    pc.TransferItemByNameTo(npc, 2203);
                    SetGlobalFlag(191, true);
                    zuggtmoy_pc_charm(npc, pc);
                    ;
                    break;
                case 321:
                    originalScript = "zuggtmoy_banish(npc,pc)";
                    zuggtmoy_banish(npc, pc);
                    break;
                case 330:
                    originalScript = "game.global_flags[185] = 1";
                    SetGlobalFlag(185, true);
                    break;
                case 340:
                    originalScript = "game.global_flags[186] = 1";
                    SetGlobalFlag(186, true);
                    break;
                case 341:
                case 342:
                case 371:
                    originalScript = "zuggtmoy_end_game(npc,pc)";
                    zuggtmoy_end_game(npc, pc);
                    break;
                case 370:
                    originalScript = "game.global_flags[187] = 1; game.global_flags[372] = 1";
                    SetGlobalFlag(187, true);
                    SetGlobalFlag(372, true);
                    ;
                    break;
                case 372:
                    originalScript = "zuggtmoy_pillar_gone(npc,pc); pc.money_adj(51764400)";
                    zuggtmoy_pillar_gone(npc, pc);
                    pc.AdjustMoney(51764400);
                    ;
                    break;
                case 381:
                    originalScript = "zuggtmoy_regenerate_and_attack(npc,pc)";
                    zuggtmoy_regenerate_and_attack(npc, pc);
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
                case 45:
                case 46:
                case 185:
                case 186:
                case 193:
                case 194:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                case 105:
                case 106:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
