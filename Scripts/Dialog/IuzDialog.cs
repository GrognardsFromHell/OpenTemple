
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
    [DialogScript(172)]
    public class IuzDialog : Iuz, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                    originalScript = "not pc.has_feat(feat_slippery_mind)";
                    return !pc.HasFeat(FeatId.SLIPPERY_MIND);
                case 32:
                    originalScript = "pc.has_feat(feat_slippery_mind)";
                    return pc.HasFeat(FeatId.SLIPPERY_MIND);
                case 92:
                    originalScript = "pc.stat_level_get(stat_level_wizard) == 0 and pc.stat_level_get(stat_level_sorcerer) == 0";
                    return pc.GetStat(Stat.level_wizard) == 0 && pc.GetStat(Stat.level_sorcerer) == 0;
                case 93:
                case 94:
                    originalScript = "pc.stat_level_get(stat_level_wizard) > 0 or pc.stat_level_get(stat_level_sorcerer) > 0";
                    return pc.GetStat(Stat.level_wizard) > 0 || pc.GetStat(Stat.level_sorcerer) > 0;
                case 95:
                    originalScript = "pc.stat_level_get(stat_alignment) == LAWFUL_EVIL or pc.stat_level_get(stat_alignment) == NEUTRAL_EVIL or pc.stat_level_get(stat_alignment) == CHAOTIC_EVIL";
                    return pc.GetAlignment() == Alignment.LAWFUL_EVIL || pc.GetAlignment() == Alignment.NEUTRAL_EVIL || pc.GetAlignment() == Alignment.CHAOTIC_EVIL;
                case 96:
                    originalScript = "pc.stat_level_get(stat_alignment) == LAWFUL_GOOD or pc.stat_level_get(stat_alignment) == NEUTRAL_GOOD or pc.stat_level_get(stat_alignment) == CHAOTIC_GOOD";
                    return pc.GetAlignment() == Alignment.LAWFUL_GOOD || pc.GetAlignment() == Alignment.NEUTRAL_GOOD || pc.GetAlignment() == Alignment.CHAOTIC_GOOD;
                case 97:
                    originalScript = "(pc.stat_level_get(stat_alignment) == LAWFUL_EVIL or pc.stat_level_get(stat_alignment) == NEUTRAL_EVIL or pc.stat_level_get(stat_alignment) == CHAOTIC_EVIL) and pc.stat_level_get(stat_level_wizard) == 0 and pc.stat_level_get(stat_level_sorcerer) == 0";
                    return (pc.GetAlignment() == Alignment.LAWFUL_EVIL || pc.GetAlignment() == Alignment.NEUTRAL_EVIL || pc.GetAlignment() == Alignment.CHAOTIC_EVIL) && pc.GetStat(Stat.level_wizard) == 0 && pc.GetStat(Stat.level_sorcerer) == 0;
                case 122:
                case 123:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 124:
                case 125:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
                case 126:
                case 127:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 12;
                case 221:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 14681 )";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 222:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 14681 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 100:
                case 130:
                    originalScript = "unshit(npc,pc); game.global_flags[371] = 1";
                    unshit(npc, pc);
                    SetGlobalFlag(371, true);
                    ;
                    break;
                case 10:
                case 20:
                case 30:
                case 40:
                case 50:
                case 60:
                case 70:
                case 80:
                case 90:
                case 110:
                case 140:
                case 150:
                case 160:
                case 170:
                case 180:
                case 200:
                case 210:
                case 220:
                case 230:
                case 240:
                    originalScript = "unshit(npc,pc)";
                    unshit(npc, pc);
                    break;
                case 31:
                    originalScript = "iuz_pc_persuade(npc,pc,50,60)";
                    iuz_pc_persuade(npc, pc, 50, 60);
                    break;
                case 32:
                    originalScript = "iuz_pc_persuade(npc,pc,50,230)";
                    iuz_pc_persuade(npc, pc, 50, 230);
                    break;
                case 51:
                case 141:
                case 151:
                case 161:
                case 171:
                case 181:
                    originalScript = "npc.attack(pc); game.global_flags[544] = 0";
                    npc.Attack(pc);
                    SetGlobalFlag(544, false);
                    ;
                    break;
                case 71:
                    originalScript = "pc.item_transfer_to(npc,2203); iuz_pc_charm(npc,pc)";
                    pc.TransferItemByNameTo(npc, 2203);
                    iuz_pc_charm(npc, pc);
                    ;
                    break;
                case 101:
                    originalScript = "switch_to_hedrack(npc,pc)";
                    switch_to_hedrack(npc, pc);
                    break;
                case 120:
                    originalScript = "orientation(npc,pc); unshit(npc,pc)";
                    orientation(npc, pc);
                    unshit(npc, pc);
                    ;
                    break;
                case 201:
                    originalScript = "switch_to_cuthbert(npc,pc,10)";
                    switch_to_cuthbert(npc, pc, 10);
                    break;
                case 211:
                    originalScript = "switch_to_cuthbert(npc,pc,20)";
                    switch_to_cuthbert(npc, pc, 20);
                    break;
                case 221:
                    originalScript = "iuz_animate_troops(npc,pc); switch_to_cuthbert(npc,pc,30)";
                    iuz_animate_troops(npc, pc);
                    switch_to_cuthbert(npc, pc, 30);
                    ;
                    break;
                case 222:
                    originalScript = "iuz_animate_troops(npc,pc); find_ron(npc,pc,620)";
                    iuz_animate_troops(npc, pc);
                    find_ron(npc, pc, 620);
                    ;
                    break;
                case 231:
                    originalScript = "iuz_pc_persuade(npc,pc,50,240)";
                    iuz_pc_persuade(npc, pc, 50, 240);
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
                case 122:
                case 123:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 12);
                    return true;
                case 124:
                case 125:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                    return true;
                case 126:
                case 127:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
