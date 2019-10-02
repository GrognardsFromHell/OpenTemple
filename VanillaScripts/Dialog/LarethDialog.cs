
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

namespace VanillaScripts.Dialog
{
    [DialogScript(60)]
    public class LarethDialog : Lareth, IDialogScript
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
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 101:
                case 102:
                case 261:
                case 262:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
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
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                case 162:
                case 163:
                    Trace.Assert(originalScript == "game.global_flags[62] == 1");
                    return GetGlobalFlag(62);
                case 164:
                case 165:
                    Trace.Assert(originalScript == "game.global_flags[62] == 0");
                    return !GetGlobalFlag(62);
                case 181:
                case 182:
                case 211:
                case 212:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) == 0");
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 185:
                case 186:
                case 217:
                case 218:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) >= 1");
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) >= 1;
                case 225:
                    Trace.Assert(originalScript == "game.global_flags[53] == 0");
                    return !GetGlobalFlag(53);
                case 333:
                case 334:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 335:
                case 336:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD) or (game.party_alignment == LAWFUL_NEUTRAL)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD) || (PartyAlignment == Alignment.LAWFUL_NEUTRAL);
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
                case 140:
                case 330:
                    Trace.Assert(originalScript == "game.global_flags[62] = 1");
                    SetGlobalFlag(62, true);
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
                case 141:
                case 142:
                case 151:
                case 152:
                case 161:
                case 175:
                case 176:
                case 183:
                case 184:
                case 185:
                case 186:
                case 195:
                case 196:
                case 213:
                case 214:
                case 217:
                case 218:
                case 331:
                case 332:
                case 351:
                case 352:
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
                    Trace.Assert(originalScript == "pc.follower_add( npc ); game.global_flags[53] = 1; pc.reputation_add( 18 )");
                    pc.AddFollower(npc);
                    SetGlobalFlag(53, true);
                    pc.AddReputation(18);
                    ;
                    break;
                case 181:
                case 182:
                case 261:
                case 262:
                    Trace.Assert(originalScript == "pc.follower_add( npc ); pc.reputation_add( 18 )");
                    pc.AddFollower(npc);
                    pc.AddReputation(18);
                    ;
                    break;
                case 200:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto(pc,6100)");
                    npc.TransferItemByProtoTo(pc, 6100);
                    break;
                case 201:
                case 202:
                case 361:
                    Trace.Assert(originalScript == "game.story_state = 2; run_off( npc, pc )");
                    StoryState = 2;
                    run_off(npc, pc);
                    ;
                    break;
                case 211:
                case 212:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto(pc,6100); pc.follower_add( npc ); pc.reputation_add( 18 )");
                    npc.TransferItemByProtoTo(pc, 6100);
                    pc.AddFollower(npc);
                    pc.AddReputation(18);
                    ;
                    break;
                case 223:
                case 224:
                case 281:
                    Trace.Assert(originalScript == "game.fade_and_teleport( 14400, 0, 0, 5113, 481, 501 )");
                    FadeAndTeleport(14400, 0, 0, 5113, 481, 501);
                    break;
                case 231:
                    Trace.Assert(originalScript == "game.story_state = 2; pc.follower_remove( npc ); run_off( npc, pc )");
                    StoryState = 2;
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
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
                case 271:
                case 311:
                    Trace.Assert(originalScript == "pc.follower_remove( npc ); npc.attack( pc )");
                    pc.RemoveFollower(npc);
                    npc.Attack(pc);
                    ;
                    break;
                case 301:
                    Trace.Assert(originalScript == "pc.follower_remove( npc ); run_off( npc, pc )");
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    ;
                    break;
                case 371:
                case 381:
                    Trace.Assert(originalScript == "demo_end_game(npc,pc)");
                    demo_end_game(npc, pc);
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
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 5);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
