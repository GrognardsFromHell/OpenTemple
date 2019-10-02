
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
    [DialogScript(172)]
    public class IuzDialog : Iuz, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                    Trace.Assert(originalScript == "not pc.has_feat(feat_slippery_mind)");
                    return !pc.HasFeat(FeatId.SLIPPERY_MIND);
                case 32:
                    Trace.Assert(originalScript == "pc.has_feat(feat_slippery_mind)");
                    return pc.HasFeat(FeatId.SLIPPERY_MIND);
                case 92:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_wizard) == 0 and pc.stat_level_get(stat_level_sorcerer) == 0");
                    return pc.GetStat(Stat.level_wizard) == 0 && pc.GetStat(Stat.level_sorcerer) == 0;
                case 93:
                case 94:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_wizard) > 0 or pc.stat_level_get(stat_level_sorcerer) > 0");
                    return pc.GetStat(Stat.level_wizard) > 0 || pc.GetStat(Stat.level_sorcerer) > 0;
                case 95:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_alignment) == LAWFUL_EVIL or pc.stat_level_get(stat_alignment) == NEUTRAL_EVIL or pc.stat_level_get(stat_alignment) == CHAOTIC_EVIL");
                    return pc.GetAlignment() == Alignment.LAWFUL_EVIL || pc.GetAlignment() == Alignment.NEUTRAL_EVIL || pc.GetAlignment() == Alignment.CHAOTIC_EVIL;
                case 96:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_alignment) == LAWFUL_GOOD or pc.stat_level_get(stat_alignment) == NEUTRAL_GOOD or pc.stat_level_get(stat_alignment) == CHAOTIC_GOOD");
                    return pc.GetAlignment() == Alignment.LAWFUL_GOOD || pc.GetAlignment() == Alignment.NEUTRAL_GOOD || pc.GetAlignment() == Alignment.CHAOTIC_GOOD;
                case 97:
                    Trace.Assert(originalScript == "(pc.stat_level_get(stat_alignment) == LAWFUL_EVIL or pc.stat_level_get(stat_alignment) == NEUTRAL_EVIL or pc.stat_level_get(stat_alignment) == CHAOTIC_EVIL) and pc.stat_level_get(stat_level_wizard) == 0 and pc.stat_level_get(stat_level_sorcerer) == 0");
                    return (pc.GetAlignment() == Alignment.LAWFUL_EVIL || pc.GetAlignment() == Alignment.NEUTRAL_EVIL || pc.GetAlignment() == Alignment.CHAOTIC_EVIL) && pc.GetStat(Stat.level_wizard) == 0 && pc.GetStat(Stat.level_sorcerer) == 0;
                case 122:
                case 123:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 124:
                case 125:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
                case 126:
                case 127:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 12;
                case 221:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 14681 )");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 222:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 14681 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 100:
                case 130:
                    Trace.Assert(originalScript == "unshit(npc,pc); game.global_flags[371] = 1");
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
                    Trace.Assert(originalScript == "unshit(npc,pc)");
                    unshit(npc, pc);
                    break;
                case 31:
                    Trace.Assert(originalScript == "iuz_pc_persuade(npc,pc,50,60)");
                    iuz_pc_persuade(npc, pc, 50, 60);
                    break;
                case 32:
                    Trace.Assert(originalScript == "iuz_pc_persuade(npc,pc,50,230)");
                    iuz_pc_persuade(npc, pc, 50, 230);
                    break;
                case 51:
                case 141:
                case 151:
                case 161:
                case 171:
                case 181:
                    Trace.Assert(originalScript == "npc.attack(pc); game.global_flags[544] = 0");
                    npc.Attack(pc);
                    SetGlobalFlag(544, false);
                    ;
                    break;
                case 71:
                    Trace.Assert(originalScript == "pc.item_transfer_to(npc,2203); iuz_pc_charm(npc,pc)");
                    pc.TransferItemByNameTo(npc, 2203);
                    iuz_pc_charm(npc, pc);
                    ;
                    break;
                case 101:
                    Trace.Assert(originalScript == "switch_to_hedrack(npc,pc)");
                    switch_to_hedrack(npc, pc);
                    break;
                case 120:
                    Trace.Assert(originalScript == "orientation(npc,pc); unshit(npc,pc)");
                    orientation(npc, pc);
                    unshit(npc, pc);
                    ;
                    break;
                case 201:
                    Trace.Assert(originalScript == "switch_to_cuthbert(npc,pc,10)");
                    switch_to_cuthbert(npc, pc, 10);
                    break;
                case 211:
                    Trace.Assert(originalScript == "switch_to_cuthbert(npc,pc,20)");
                    switch_to_cuthbert(npc, pc, 20);
                    break;
                case 221:
                    Trace.Assert(originalScript == "iuz_animate_troops(npc,pc); switch_to_cuthbert(npc,pc,30)");
                    iuz_animate_troops(npc, pc);
                    switch_to_cuthbert(npc, pc, 30);
                    ;
                    break;
                case 222:
                    Trace.Assert(originalScript == "iuz_animate_troops(npc,pc); find_ron(npc,pc,620)");
                    iuz_animate_troops(npc, pc);
                    find_ron(npc, pc, 620);
                    ;
                    break;
                case 231:
                    Trace.Assert(originalScript == "iuz_pc_persuade(npc,pc,50,240)");
                    iuz_pc_persuade(npc, pc, 50, 240);
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
