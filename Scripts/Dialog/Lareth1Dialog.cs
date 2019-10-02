
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
    [DialogScript(60)]
    public class Lareth1Dialog : Lareth1, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 15:
                case 16:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 3005 )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3005));
                case 21:
                case 22:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 101:
                case 102:
                case 6171:
                case 6172:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and not anyone( pc.group_list(), \"has_follower\", 14681 )");
                    return !pc.HasMaxFollowers() && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 105:
                case 106:
                case 183:
                case 184:
                case 213:
                case 214:
                case 215:
                case 216:
                case 263:
                case 264:
                case 6175:
                case 6176:
                case 6723:
                case 6724:
                case 6783:
                case 6784:
                case 6785:
                case 6786:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                case 107:
                case 6177:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and anyone( pc.group_list(), \"has_follower\", 14681 )");
                    return !pc.HasMaxFollowers() && pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 163:
                case 164:
                case 6103:
                case 6104:
                    Trace.Assert(originalScript == "game.global_flags[62] == 1");
                    return GetGlobalFlag(62);
                case 165:
                case 166:
                case 6105:
                case 6106:
                    Trace.Assert(originalScript == "game.global_flags[62] == 0");
                    return !GetGlobalFlag(62);
                case 181:
                case 182:
                case 211:
                case 212:
                case 6721:
                case 6722:
                case 6781:
                case 6782:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) == 0 and not anyone( pc.group_list(), \"has_follower\", 14681 )");
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 185:
                case 186:
                case 217:
                case 218:
                case 6725:
                case 6726:
                case 6787:
                case 6788:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) >= 1");
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) >= 1;
                case 187:
                case 219:
                case 6727:
                case 6789:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) == 0 and anyone( pc.group_list(), \"has_follower\", 14681 )");
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 225:
                case 6805:
                    Trace.Assert(originalScript == "game.global_flags[53] == 0");
                    return !GetGlobalFlag(53);
                case 261:
                case 262:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
                case 333:
                case 334:
                case 601:
                case 602:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 335:
                case 336:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD) or (game.party_alignment == LAWFUL_NEUTRAL)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD) || (PartyAlignment == Alignment.LAWFUL_NEUTRAL);
                case 6123:
                case 6124:
                    Trace.Assert(originalScript == "game.party_alignment & ALIGNMENT_EVIL != 0");
                    return PartyAlignment.IsEvil();
                case 6125:
                case 6126:
                    Trace.Assert(originalScript == "(game.party_alignment & ALIGNMENT_GOOD != 0)  or (game.party_alignment == LAWFUL_NEUTRAL)");
                    return (PartyAlignment.IsGood()) || (PartyAlignment == Alignment.LAWFUL_NEUTRAL);
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
                case 10:
                case 20:
                    Trace.Assert(originalScript == "game.global_flags[834] = 1; game.global_flags[62] = 1; game.global_vars[116] = 1");
                    SetGlobalFlag(834, true);
                    SetGlobalFlag(62, true);
                    SetGlobalVar(116, 1);
                    ;
                    break;
                case 33:
                case 34:
                case 45:
                case 46:
                case 51:
                case 52:
                case 53:
                case 54:
                case 65:
                case 66:
                case 73:
                case 74:
                case 83:
                case 84:
                case 93:
                case 94:
                case 103:
                case 104:
                case 111:
                case 112:
                case 131:
                case 132:
                case 151:
                case 152:
                case 701:
                case 6153:
                case 6154:
                case 6173:
                case 6174:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "pc.money_adj(-10000)");
                    pc.AdjustMoney(-10000);
                    break;
                case 101:
                case 102:
                case 6171:
                case 6172:
                    Trace.Assert(originalScript == "pc.follower_add( npc ); game.global_flags[53] = 1; pc.reputation_add( 18 )");
                    pc.AddFollower(npc);
                    SetGlobalFlag(53, true);
                    pc.AddReputation(18);
                    ;
                    break;
                case 107:
                case 6177:
                    Trace.Assert(originalScript == "game.global_flags[198] = 1; argue_ron(npc,pc,550)");
                    SetGlobalFlag(198, true);
                    argue_ron(npc, pc, 550);
                    ;
                    break;
                case 140:
                case 330:
                case 6120:
                    Trace.Assert(originalScript == "game.global_flags[834] = 1; game.global_flags[62] = 1");
                    SetGlobalFlag(834, true);
                    SetGlobalFlag(62, true);
                    ;
                    break;
                case 160:
                case 8000:
                    Trace.Assert(originalScript == "game.global_flags[834] = 1");
                    SetGlobalFlag(834, true);
                    break;
                case 181:
                case 182:
                case 6721:
                case 6722:
                    Trace.Assert(originalScript == "pc.follower_add( npc ); pc.reputation_add( 18 ); record_time_stamp(425)");
                    pc.AddFollower(npc);
                    pc.AddReputation(18);
                    ScriptDaemon.record_time_stamp(425);
                    ;
                    break;
                case 187:
                case 6727:
                    Trace.Assert(originalScript == "argue_ron(npc,pc,370)");
                    argue_ron(npc, pc, 370);
                    break;
                case 200:
                case 6760:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto(pc,6100)");
                    npc.TransferItemByProtoTo(pc, 6100);
                    break;
                case 201:
                case 202:
                case 361:
                case 6761:
                case 6762:
                    Trace.Assert(originalScript == "game.story_state = 2; run_off( npc, pc )");
                    StoryState = 2;
                    run_off(npc, pc);
                    ;
                    break;
                case 211:
                case 212:
                case 6781:
                case 6782:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto(pc,6100); pc.follower_add( npc ); pc.reputation_add( 18 ); record_time_stamp(425)");
                    npc.TransferItemByProtoTo(pc, 6100);
                    pc.AddFollower(npc);
                    pc.AddReputation(18);
                    ScriptDaemon.record_time_stamp(425);
                    ;
                    break;
                case 219:
                case 6789:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto(pc,6100); argue_ron(npc,pc,270)");
                    npc.TransferItemByProtoTo(pc, 6100);
                    argue_ron(npc, pc, 270);
                    ;
                    break;
                case 220:
                case 6800:
                    Trace.Assert(originalScript == "record_time_stamp(425)");
                    ScriptDaemon.record_time_stamp(425);
                    break;
                case 223:
                case 224:
                case 281:
                case 6803:
                case 6804:
                    Trace.Assert(originalScript == "game.fade_and_teleport( 14400, 0, 0, 5111, 481, 501 )");
                    FadeAndTeleport(14400, 0, 0, 5111, 481, 501);
                    break;
                case 231:
                case 603:
                case 604:
                case 6821:
                    Trace.Assert(originalScript == "game.global_flags[37] = 0; game.story_state = 2; pc.follower_remove( npc ); run_off( npc, pc )");
                    SetGlobalFlag(37, false);
                    StoryState = 2;
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    ;
                    break;
                case 234:
                case 235:
                case 271:
                case 311:
                case 324:
                case 325:
                case 605:
                case 6824:
                case 6825:
                    Trace.Assert(originalScript == "pc.follower_remove( npc ); npc.attack( pc )");
                    pc.RemoveFollower(npc);
                    npc.Attack(pc);
                    ;
                    break;
                case 250:
                case 265:
                    Trace.Assert(originalScript == "game.global_flags[198] = 1");
                    SetGlobalFlag(198, true);
                    break;
                case 260:
                    Trace.Assert(originalScript == "game.global_flags[198] = 0");
                    SetGlobalFlag(198, false);
                    break;
                case 261:
                case 262:
                    Trace.Assert(originalScript == "pc.follower_add( npc ); pc.reputation_add( 18 )");
                    pc.AddFollower(npc);
                    pc.AddReputation(18);
                    ;
                    break;
                case 266:
                    Trace.Assert(originalScript == "buff_npc_three(npc,pc)");
                    buff_npc_three(npc, pc);
                    break;
                case 268:
                    Trace.Assert(originalScript == "buff_npc(npc,pc)");
                    buff_npc(npc, pc);
                    break;
                case 270:
                    Trace.Assert(originalScript == "buff_npc_two(npc,pc); game.global_flags[833] = 1");
                    buff_npc_two(npc, pc);
                    SetGlobalFlag(833, true);
                    ;
                    break;
                case 301:
                    Trace.Assert(originalScript == "game.global_flags[37] = 0; pc.follower_remove( npc ); run_off( npc, pc )");
                    SetGlobalFlag(37, false);
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    ;
                    break;
                case 371:
                case 381:
                    Trace.Assert(originalScript == "demo_end_game(npc,pc)");
                    demo_end_game(npc, pc);
                    break;
                case 631:
                    Trace.Assert(originalScript == "game.global_flags[37] = 0; game.story_state = 2; pc.follower_remove( npc ); run_off( npc, pc ); argue_ron(npc,pc,25)");
                    SetGlobalFlag(37, false);
                    StoryState = 2;
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    argue_ron(npc, pc, 25);
                    ;
                    break;
                case 700:
                    Trace.Assert(originalScript == "game.particles( 'sp-Curse Water', npc ); create_spiders(npc, pc)");
                    AttachParticles("sp-Curse Water", npc);
                    create_spiders(npc, pc);
                    ;
                    break;
                case 1000:
                case 1100:
                case 1200:
                case 1300:
                case 1400:
                    Trace.Assert(originalScript == "game.global_flags[806] = 1; pc.follower_remove( npc ); party_transfer_to( npc, 11003 ); pc.follower_add( npc ); game.global_flags[806] = 0");
                    SetGlobalFlag(806, true);
                    pc.RemoveFollower(npc);
                    Utilities.party_transfer_to(npc, 11003);
                    pc.AddFollower(npc);
                    SetGlobalFlag(806, false);
                    ;
                    break;
                case 1003:
                case 1103:
                case 1203:
                case 1303:
                case 1403:
                case 1503:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 1500:
                    Trace.Assert(originalScript == "game.global_flags[806] = 1; pc.follower_remove( npc ); party_transfer_to( npc, 11003 ); game.global_flags[806] = 0");
                    SetGlobalFlag(806, true);
                    pc.RemoveFollower(npc);
                    Utilities.party_transfer_to(npc, 11003);
                    SetGlobalFlag(806, false);
                    ;
                    break;
                case 6100:
                    Trace.Assert(originalScript == "game.global_flags[834] = 1; game.sound(4200, 1)");
                    SetGlobalFlag(834, true);
                    Sound(4200, 1);
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
                case 21:
                case 22:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
