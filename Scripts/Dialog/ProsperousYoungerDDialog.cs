
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
    [DialogScript(17)]
    public class ProsperousYoungerDDialog : ProsperousYoungerD, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 6:
                case 7:
                    originalScript = "game.quests[7].state == qs_accepted";
                    return GetQuestState(7) == QuestState.Accepted;
                case 8:
                case 9:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 16 and pc.stat_level_get( stat_gender ) == gender_male and game.global_flags[304] == 1 and game.quests[7].state <= qs_mentioned and game.global_flags[305] == 0";
                    return pc.GetStat(Stat.charisma) >= 16 && pc.GetGender() == Gender.Male && GetGlobalFlag(304) && GetQuestState(7) <= QuestState.Mentioned && !GetGlobalFlag(305);
                case 10:
                case 11:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 16 and pc.stat_level_get( stat_gender ) == gender_male and game.global_flags[304] == 1 and game.quests[7].state <= qs_mentioned and game.global_flags[305] == 1";
                    return pc.GetStat(Stat.charisma) >= 16 && pc.GetGender() == Gender.Male && GetGlobalFlag(304) && GetQuestState(7) <= QuestState.Mentioned && GetGlobalFlag(305);
                case 15:
                case 17:
                case 245:
                case 246:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 16 and pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetStat(Stat.charisma) >= 16 && pc.GetGender() == Gender.Male;
                case 22:
                    originalScript = "game.quests[6].state == qs_mentioned or game.quests[6].state == qs_accepted";
                    return GetQuestState(6) == QuestState.Mentioned || GetQuestState(6) == QuestState.Accepted;
                case 23:
                case 61:
                case 62:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 16 and pc.stat_level_get( stat_gender ) == gender_male and game.global_flags[303] == 0";
                    return pc.GetStat(Stat.charisma) >= 16 && pc.GetGender() == Gender.Male && !GetGlobalFlag(303);
                case 45:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                    return npc.GetLeader() == null;
                case 46:
                case 403:
                case 404:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 65:
                case 66:
                    originalScript = "game.global_flags[303] == 0 and game.global_flags[304] == 0";
                    return !GetGlobalFlag(303) && !GetGlobalFlag(304);
                case 67:
                case 68:
                    originalScript = "game.global_flags[303] == 1 and game.global_flags[304] == 0";
                    return GetGlobalFlag(303) && !GetGlobalFlag(304);
                case 121:
                case 122:
                case 151:
                case 152:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 123:
                case 124:
                case 153:
                case 154:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                case 141:
                case 142:
                    originalScript = "npc.area != 1";
                    return npc.GetArea() != 1;
                case 144:
                case 145:
                    originalScript = "npc.area == 1";
                    return npc.GetArea() == 1;
                case 183:
                case 184:
                    originalScript = "npc.area == 3 and (game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return npc.GetArea() == 3 && (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 405:
                    originalScript = "not pc.stat_level_get(stat_level_wizard) > 0";
                    return !(pc.GetStat(Stat.level_wizard) > 0);
                case 406:
                    originalScript = "pc.stat_level_get(stat_level_wizard) > 0";
                    return pc.GetStat(Stat.level_wizard) > 0;
                case 462:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 482:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8020 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8020));
                case 501:
                case 502:
                    originalScript = "game.global_flags[867] == 0";
                    return !GetGlobalFlag(867);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_vars[111] = 1";
                    SetGlobalVar(111, 1);
                    break;
                case 80:
                    originalScript = "game.quests[7].state = qs_mentioned";
                    SetQuestState(7, QuestState.Mentioned);
                    break;
                case 81:
                case 84:
                case 315:
                case 316:
                    originalScript = "game.global_flags[305] = 1";
                    SetGlobalFlag(305, true);
                    break;
                case 82:
                case 83:
                    originalScript = "game.quests[7].state = qs_accepted";
                    SetQuestState(7, QuestState.Accepted);
                    break;
                case 120:
                    originalScript = "game.global_flags[46] = 1";
                    SetGlobalFlag(46, true);
                    break;
                case 121:
                case 122:
                case 151:
                    originalScript = "pc.follower_add( npc ); buttin3(npc,pc,450)";
                    pc.AddFollower(npc);
                    buttin3(npc, pc, 450);
                    ;
                    break;
                case 144:
                case 145:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
                    break;
                case 152:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 191:
                case 200:
                case 320:
                    originalScript = "game.global_flags[196] = 1; pc.follower_remove( npc )";
                    SetGlobalFlag(196, true);
                    pc.RemoveFollower(npc);
                    ;
                    break;
                case 201:
                case 202:
                    originalScript = "buttin(npc,pc,675)";
                    buttin(npc, pc, 675);
                    break;
                case 230:
                    originalScript = "game.areas[10] = 1";
                    MakeAreaKnown(10);
                    break;
                case 235:
                case 236:
                case 243:
                case 244:
                    originalScript = "game.worldmap_travel_by_dialog(10)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 245:
                case 246:
                case 251:
                case 252:
                    originalScript = "game.global_flags[303] = 1";
                    SetGlobalFlag(303, true);
                    break;
                case 261:
                case 262:
                    originalScript = "game.global_flags[304] = 1";
                    SetGlobalFlag(304, true);
                    break;
                case 321:
                case 322:
                    originalScript = "buttin2(npc,pc,460)";
                    buttin2(npc, pc, 460);
                    break;
                case 341:
                    originalScript = "switch_to_tarah( npc, pc, 280)";
                    switch_to_tarah(npc, pc, 280);
                    break;
                case 351:
                    originalScript = "switch_to_tarah( npc, pc, 300)";
                    switch_to_tarah(npc, pc, 300);
                    break;
                case 482:
                    originalScript = "buttin4(npc,pc,400)";
                    buttin4(npc, pc, 400);
                    break;
                case 503:
                case 504:
                    originalScript = "game.global_flags[806] = 1";
                    SetGlobalFlag(806, true);
                    break;
                case 550:
                    originalScript = "pc.follower_remove( npc ); pc.follower_add( npc ); game.global_flags[806] = 0";
                    pc.RemoveFollower(npc);
                    pc.AddFollower(npc);
                    SetGlobalFlag(806, false);
                    ;
                    break;
                case 561:
                    originalScript = "game.global_flags[867] = 1; equip_transfer( npc, pc )";
                    SetGlobalFlag(867, true);
                    equip_transfer(npc, pc);
                    ;
                    break;
                case 22000:
                    originalScript = "game.global_vars[904] = 32";
                    SetGlobalVar(904, 32);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
