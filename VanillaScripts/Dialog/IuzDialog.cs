
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

namespace VanillaScripts.Dialog
{
    [DialogScript(172)]
    public class IuzDialog : Iuz, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
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
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 124:
                case 125:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 126:
                case 127:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                    originalScript = "iuz_pc_persuade(npc,pc,50,60)";
                    iuz_pc_persuade(npc, pc, 50, 60);
                    break;
                case 32:
                    originalScript = "iuz_pc_persuade(npc,pc,50,230)";
                    iuz_pc_persuade(npc, pc, 50, 230);
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
                case 141:
                case 151:
                case 161:
                case 171:
                case 181:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 201:
                    originalScript = "switch_to_cuthbert(npc,pc,10)";
                    switch_to_cuthbert(npc, pc, 10);
                    break;
                case 211:
                    originalScript = "switch_to_cuthbert(npc,pc,20)";
                    switch_to_cuthbert(npc, pc, 20);
                    break;
                case 220:
                    originalScript = "iuz_animate_troops(npc,pc)";
                    iuz_animate_troops(npc, pc);
                    break;
                case 221:
                    originalScript = "switch_to_cuthbert(npc,pc,30)";
                    switch_to_cuthbert(npc, pc, 30);
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
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                case 124:
                case 125:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                case 126:
                case 127:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
