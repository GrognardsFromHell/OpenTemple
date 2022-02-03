
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
    [DialogScript(4)]
    public class BurneDialog : Burne, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 1001:
                case 1002:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 855:
                case 1003:
                case 1103:
                case 1151:
                case 1171:
                    originalScript = "npc.has_met(pc) and game.story_state <= 1";
                    return npc.HasMet(pc) && StoryState <= 1;
                case 5:
                case 851:
                case 1004:
                case 1104:
                case 1152:
                case 1172:
                    originalScript = "npc.has_met(pc) and game.story_state >= 2";
                    return npc.HasMet(pc) && StoryState >= 2;
                case 6:
                case 7:
                case 852:
                case 853:
                case 1005:
                case 1006:
                case 1153:
                case 1154:
                case 1173:
                case 1174:
                    originalScript = "game.global_flags[31] == 1 and game.quests[15].state != qs_completed";
                    return GetGlobalFlag(31) && GetQuestState(15) != QuestState.Completed;
                case 14:
                case 18:
                case 52:
                case 55:
                case 181:
                    originalScript = "game.quests[15].state == qs_unknown";
                    return GetQuestState(15) == QuestState.Unknown;
                case 21:
                case 28:
                    originalScript = "game.party_alignment == LAWFUL_GOOD and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.LAWFUL_GOOD && !GetGlobalFlag(67);
                case 22:
                case 29:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD && !GetGlobalFlag(67);
                case 23:
                case 30:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL and game.global_flags[1] == 0";
                    return PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(1);
                case 24:
                case 31:
                    originalScript = "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == NEUTRAL_GOOD) and game.global_flags[67] == 0";
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.NEUTRAL_GOOD) && !GetGlobalFlag(67);
                case 25:
                case 32:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && !GetGlobalFlag(67);
                case 26:
                case 33:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL && !GetGlobalFlag(67);
                case 27:
                case 34:
                    originalScript = "game.party_alignment == LAWFUL_EVIL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.LAWFUL_EVIL && !GetGlobalFlag(67);
                case 35:
                case 36:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL && !GetGlobalFlag(67);
                case 37:
                case 38:
                    originalScript = "game.global_flags[67] == 1";
                    return GetGlobalFlag(67);
                case 41:
                case 57:
                case 81:
                case 151:
                case 163:
                case 241:
                case 263:
                case 271:
                case 281:
                case 291:
                case 301:
                case 311:
                case 321:
                case 391:
                case 431:
                case 441:
                case 461:
                case 501:
                case 511:
                case 531:
                case 551:
                case 771:
                case 781:
                case 811:
                case 941:
                case 951:
                case 1013:
                case 1081:
                case 1113:
                    originalScript = "game.story_state <= 1";
                    return StoryState <= 1;
                case 42:
                case 58:
                case 82:
                case 152:
                case 164:
                case 242:
                case 264:
                case 272:
                case 282:
                case 292:
                case 302:
                case 312:
                case 322:
                case 392:
                case 432:
                case 442:
                case 462:
                case 502:
                case 512:
                case 532:
                case 552:
                case 772:
                case 782:
                case 812:
                case 942:
                case 952:
                case 1014:
                case 1082:
                case 1114:
                    originalScript = "game.story_state >= 2";
                    return StoryState >= 2;
                case 61:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 62:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 102:
                case 108:
                    originalScript = "game.quests[15].state == qs_unknown or game.areas[2] == 0";
                    return GetQuestState(15) == QuestState.Unknown || !IsAreaKnown(2);
                case 103:
                case 109:
                    originalScript = "game.quests[15].state == qs_mentioned";
                    return GetQuestState(15) == QuestState.Mentioned;
                case 105:
                case 110:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.story_state == 0";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && StoryState == 0;
                case 106:
                case 111:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.story_state == 0";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && StoryState == 0;
                case 112:
                case 113:
                case 184:
                    originalScript = "game.quests[15].state != qs_unknown and game.areas[2] == 1 and game.story_state <= 1";
                    return GetQuestState(15) != QuestState.Unknown && IsAreaKnown(2) && StoryState <= 1;
                case 123:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male and pc.skill_level_get(npc,skill_diplomacy) >= 10";
                    return pc.GetGender() == Gender.Male && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 124:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female and pc.skill_level_get(npc,skill_intimidate) >= 14";
                    return pc.GetGender() == Gender.Female && pc.GetSkillLevel(npc, SkillId.intimidate) >= 14;
                case 126:
                case 1093:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 11002 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(11002));
                case 141:
                case 142:
                    originalScript = "game.party[0].reputation_has( 23 ) == 0 and game.global_flags[814] == 0 and game.global_flags[815] == 0";
                    return !PartyLeader.HasReputation(23) && !GetGlobalFlag(814) && !GetGlobalFlag(815);
                case 143:
                case 144:
                    originalScript = "game.party[0].reputation_has( 23 ) == 1 or (game.global_flags[814] == 1 and game.global_flags[815] == 1)";
                    return PartyLeader.HasReputation(23) || (GetGlobalFlag(814) && GetGlobalFlag(815));
                case 171:
                case 174:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                    return npc.GetLeader() == null;
                case 172:
                case 173:
                case 175:
                case 176:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 182:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[2] == 0 and game.quests[15].state != qs_unknown";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(2) && GetQuestState(15) != QuestState.Unknown;
                case 183:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[2] == 0 and game.quests[15].state != qs_unknown";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(2) && GetQuestState(15) != QuestState.Unknown;
                case 202:
                case 206:
                    originalScript = "pc.money_get() >= 1000";
                    return pc.GetMoney() >= 1000;
                case 203:
                case 207:
                    originalScript = "pc.money_get() >= 5000";
                    return pc.GetMoney() >= 5000;
                case 204:
                case 208:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 14";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 14;
                case 233:
                case 234:
                case 903:
                case 904:
                case 1143:
                case 1144:
                    originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 253:
                case 254:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 255:
                case 256:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                case 401:
                case 402:
                    originalScript = "is_daytime()";
                    return Utilities.is_daytime();
                case 403:
                case 404:
                    originalScript = "not is_daytime()";
                    return !Utilities.is_daytime();
                case 541:
                    originalScript = "pc.money_get() >= 30000";
                    return pc.GetMoney() >= 30000;
                case 542:
                    originalScript = "game.quests[61].state != qs_botched";
                    return GetQuestState(61) != QuestState.Botched;
                case 571:
                case 581:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetGender() == Gender.Male;
                case 572:
                case 582:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                case 591:
                    originalScript = "game.story_state >= 2 and pc.stat_level_get( stat_gender ) == gender_female and pc.skill_level_get(npc,skill_intimidate) >= 1";
                    return StoryState >= 2 && pc.GetGender() == Gender.Female && pc.GetSkillLevel(npc, SkillId.intimidate) >= 1;
                case 592:
                case 593:
                case 594:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female and pc.skill_level_get(npc,skill_intimidate) >= 1";
                    return pc.GetGender() == Gender.Female && pc.GetSkillLevel(npc, SkillId.intimidate) >= 1;
                case 595:
                    originalScript = "game.story_state >= 2 and pc.stat_level_get( stat_gender ) == gender_male";
                    return StoryState >= 2 && pc.GetGender() == Gender.Male;
                case 597:
                    originalScript = "game.story_state <= 1 and pc.stat_level_get( stat_gender ) == gender_female and pc.skill_level_get(npc,skill_intimidate) >= 1";
                    return StoryState <= 1 && pc.GetGender() == Gender.Female && pc.GetSkillLevel(npc, SkillId.intimidate) >= 1;
                case 598:
                    originalScript = "game.story_state <= 1 and pc.stat_level_get( stat_gender ) == gender_male";
                    return StoryState <= 1 && pc.GetGender() == Gender.Male;
                case 821:
                    originalScript = "pc.money_get() >= 2000";
                    return pc.GetMoney() >= 2000;
                case 862:
                    originalScript = "game.quests[61].state == qs_unknown or game.quests[61].state == qs_mentioned";
                    return GetQuestState(61) == QuestState.Unknown || GetQuestState(61) == QuestState.Mentioned;
                case 863:
                    originalScript = "game.quests[61].state == qs_accepted";
                    return GetQuestState(61) == QuestState.Accepted;
                case 864:
                case 870:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[3] == 0 and game.global_vars[562] != 1";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(3) && GetGlobalVar(562) != 1;
                case 865:
                case 871:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[3] == 0 and game.global_vars[562] == 1";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(3) && GetGlobalVar(562) == 1;
                case 866:
                case 872:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[3] == 1 and game.global_vars[562] == 1";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && IsAreaKnown(3) && GetGlobalVar(562) == 1;
                case 867:
                case 873:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 0 and game.global_vars[562] != 1";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(3) && GetGlobalVar(562) != 1;
                case 868:
                case 874:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 0 and game.global_vars[562] == 1";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(3) && GetGlobalVar(562) == 1;
                case 869:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 0";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(3);
                case 875:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 1 and game.global_vars[562] == 1";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && IsAreaKnown(3) && GetGlobalVar(562) == 1;
                case 876:
                    originalScript = "game.global_flags[277] == 1 and game.global_flags[428] == 0 and game.global_flags[815] == 0 and game.global_flags[814] == 0 and game.quests[64].state == qs_unknown";
                    return GetGlobalFlag(277) && !GetGlobalFlag(428) && !GetGlobalFlag(815) && !GetGlobalFlag(814) && GetQuestState(64) == QuestState.Unknown;
                case 877:
                    originalScript = "game.global_flags[428] == 0 and game.quests[64].state == qs_mentioned and game.global_flags[815] == 0 and game.global_flags[814] == 0";
                    return !GetGlobalFlag(428) && GetQuestState(64) == QuestState.Mentioned && !GetGlobalFlag(815) && !GetGlobalFlag(814);
                case 878:
                    originalScript = "game.global_flags[428] == 1 and game.quests[64].state == qs_accepted";
                    return GetGlobalFlag(428) && GetQuestState(64) == QuestState.Accepted;
                case 879:
                    originalScript = "game.global_flags[428] == 1 and game.quests[64].state == qs_completed";
                    return GetGlobalFlag(428) && GetQuestState(64) == QuestState.Completed;
                case 880:
                    originalScript = "game.global_flags[428] == 1 and game.quests[64].state == qs_botched";
                    return GetGlobalFlag(428) && GetQuestState(64) == QuestState.Botched;
                case 881:
                    originalScript = "game.global_flags[428] == 1 and (game.quests[64].state == qs_unknown or game.quests[64].state == qs_mentioned) and (game.global_flags[815] == 1 or game.global_flags[814] == 1)";
                    return GetGlobalFlag(428) && (GetQuestState(64) == QuestState.Unknown || GetQuestState(64) == QuestState.Mentioned) && (GetGlobalFlag(815) || GetGlobalFlag(814));
                case 882:
                    originalScript = "game.global_flags[883] == 1";
                    return GetGlobalFlag(883);
                case 883:
                case 884:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL) and game.story_state >= 4 and game.global_flags[195] == 0 and game.global_flags[839] == 0";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL) && StoryState >= 4 && !GetGlobalFlag(195) && !GetGlobalFlag(839);
                case 885:
                case 886:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL) and game.story_state >= 4 and game.global_flags[195] == 1";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL) && StoryState >= 4 && GetGlobalFlag(195);
                case 887:
                case 888:
                    originalScript = "game.global_flags[195] == 0 and (anyone( pc.group_list(), \"item_find\", 2203 ))";
                    return !GetGlobalFlag(195) && (pc.GetPartyMembers().Any(o => o.FindItemByName(2203) != null));
                case 889:
                case 890:
                    originalScript = "game.global_flags[195] == 1 and game.global_flags[358] == 0";
                    return GetGlobalFlag(195) && !GetGlobalFlag(358);
                case 891:
                    originalScript = "game.global_flags[195] == 1 and game.global_flags[358] == 1 and game.global_flags[359] == 0 and (game.quests[61].state == qs_unknown or game.quests[61].state == qs_botched)";
                    return GetGlobalFlag(195) && GetGlobalFlag(358) && !GetGlobalFlag(359) && (GetQuestState(61) == QuestState.Unknown || GetQuestState(61) == QuestState.Botched);
                case 961:
                    originalScript = "game.quests[64].state == qs_mentioned";
                    return GetQuestState(64) == QuestState.Mentioned;
                case 962:
                    originalScript = "game.quests[64].state == qs_unknown";
                    return GetQuestState(64) == QuestState.Unknown;
                case 1091:
                    originalScript = "game.global_flags[292] == 1";
                    return GetGlobalFlag(292);
                case 1092:
                    originalScript = "game.global_flags[292] == 0";
                    return !GetGlobalFlag(292);
                case 1121:
                    originalScript = "game.quests[64].state == qs_accepted and game.global_flags[815] == 1 and game.global_flags[814] == 1";
                    return GetQuestState(64) == QuestState.Accepted && GetGlobalFlag(815) && GetGlobalFlag(814);
                case 1122:
                    originalScript = "game.quests[64].state == qs_accepted and game.global_flags[815] == 0 and game.global_flags[814] == 1";
                    return GetQuestState(64) == QuestState.Accepted && !GetGlobalFlag(815) && GetGlobalFlag(814);
                case 1123:
                    originalScript = "game.quests[64].state == qs_accepted and game.global_flags[815] == 1 and game.global_flags[814] == 0";
                    return GetQuestState(64) == QuestState.Accepted && GetGlobalFlag(815) && !GetGlobalFlag(814);
                case 1124:
                    originalScript = "game.quests[64].state == qs_accepted and game.global_flags[815] == 0 and game.global_flags[814] == 0";
                    return GetQuestState(64) == QuestState.Accepted && !GetGlobalFlag(815) && !GetGlobalFlag(814);
                case 1125:
                    originalScript = "(game.quests[64].state == qs_unknown or game.quests[64].state == qs_mentioned) and game.global_flags[815] == 1 and game.global_flags[814] == 1 and game.global_flags[883] == 0";
                    return (GetQuestState(64) == QuestState.Unknown || GetQuestState(64) == QuestState.Mentioned) && GetGlobalFlag(815) && GetGlobalFlag(814) && !GetGlobalFlag(883);
                case 1126:
                    originalScript = "(game.quests[64].state == qs_unknown or game.quests[64].state == qs_mentioned) and game.global_flags[815] == 0 and game.global_flags[814] == 1 and game.global_flags[883] == 0";
                    return (GetQuestState(64) == QuestState.Unknown || GetQuestState(64) == QuestState.Mentioned) && !GetGlobalFlag(815) && GetGlobalFlag(814) && !GetGlobalFlag(883);
                case 1127:
                    originalScript = "(game.quests[64].state == qs_unknown or game.quests[64].state == qs_mentioned) and game.global_flags[815] == 1 and game.global_flags[814] == 0 and game.global_flags[883] == 0";
                    return (GetQuestState(64) == QuestState.Unknown || GetQuestState(64) == QuestState.Mentioned) && GetGlobalFlag(815) && !GetGlobalFlag(814) && !GetGlobalFlag(883);
                case 1128:
                    originalScript = "game.global_flags[883] == 1 and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return GetGlobalFlag(883) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 1129:
                    originalScript = "game.global_flags[883] == 1 and (game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL)";
                    return GetGlobalFlag(883) && (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL);
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
                    originalScript = "game.global_vars[107] = 1";
                    SetGlobalVar(107, 1);
                    break;
                case 73:
                    originalScript = "game.quests[15].state = qs_mentioned";
                    SetQuestState(15, QuestState.Mentioned);
                    break;
                case 74:
                case 75:
                case 103:
                case 109:
                    originalScript = "game.quests[15].state = qs_accepted";
                    SetQuestState(15, QuestState.Accepted);
                    break;
                case 141:
                case 142:
                case 143:
                case 144:
                    originalScript = "game.global_flags[694] = 0; game.quests[15].state = qs_completed";
                    SetGlobalFlag(694, false);
                    SetQuestState(15, QuestState.Completed);
                    ;
                    break;
                case 145:
                case 146:
                    originalScript = "game.global_flags[694] = 1; pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                    SetGlobalFlag(694, true);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    ;
                    break;
                case 150:
                case 1080:
                    originalScript = "pc.money_adj(+5000); npc.reaction_adj( pc,+20)";
                    pc.AdjustMoney(+5000);
                    npc.AdjustReaction(pc, +20);
                    ;
                    break;
                case 190:
                case 210:
                    originalScript = "game.areas[2] = 1; game.story_state = 1; game.quests[72].state = qs_mentioned";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    SetQuestState(72, QuestState.Mentioned);
                    ;
                    break;
                case 191:
                case 192:
                case 213:
                case 214:
                case 223:
                case 224:
                    originalScript = "game.quests[72].state = qs_accepted; game.global_vars[562] = 1";
                    SetQuestState(72, QuestState.Accepted);
                    SetGlobalVar(562, 1);
                    ;
                    break;
                case 193:
                case 194:
                case 211:
                case 212:
                case 221:
                case 222:
                    originalScript = "game.quests[72].state = qs_accepted; game.global_vars[562] = 1; game.worldmap_travel_by_dialog(2)";
                    SetQuestState(72, QuestState.Accepted);
                    SetGlobalVar(562, 1);
                    WorldMapTravelByDialog(2);
                    break;
                case 202:
                case 206:
                    originalScript = "pc.money_adj(-1000)";
                    pc.AdjustMoney(-1000);
                    break;
                case 203:
                case 207:
                    originalScript = "pc.money_adj(-5000)";
                    pc.AdjustMoney(-5000);
                    break;
                case 220:
                    originalScript = "game.areas[2] = 1; game.story_state = 1; npc.reaction_adj( pc,-10); game.quests[72].state = qs_mentioned";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    npc.AdjustReaction(pc, -10);
                    SetQuestState(72, QuestState.Mentioned);
                    ;
                    break;
                case 230:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 231:
                case 232:
                case 261:
                case 262:
                case 901:
                case 902:
                case 1011:
                case 1012:
                case 1111:
                case 1112:
                case 1141:
                case 1142:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    WorldMapTravelByDialog(3);
                    break;
                case 253:
                case 254:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 260:
                    originalScript = "game.areas[3] = 1; game.story_state = 3; npc.reaction_adj( pc,+10)";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    npc.AdjustReaction(pc, +10);
                    ;
                    break;
                case 341:
                case 481:
                case 482:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
                    break;
                case 420:
                    originalScript = "game.global_flags[358] = 1";
                    SetGlobalFlag(358, true);
                    break;
                case 450:
                case 470:
                    originalScript = "game.global_flags[195] = 1";
                    SetGlobalFlag(195, true);
                    break;
                case 520:
                case 540:
                    originalScript = "game.quests[61].state = qs_mentioned";
                    SetQuestState(61, QuestState.Mentioned);
                    break;
                case 523:
                case 542:
                    originalScript = "game.quests[61].state = qs_accepted";
                    SetQuestState(61, QuestState.Accepted);
                    break;
                case 541:
                    originalScript = "pc.money_adj(-30000); create_item_in_inventory( 9171, pc )";
                    pc.AdjustMoney(-30000);
                    Utilities.create_item_in_inventory(9171, pc);
                    ;
                    break;
                case 561:
                    originalScript = "game.party[0].reputation_add(27)";
                    PartyLeader.AddReputation(27);
                    break;
                case 582:
                    originalScript = "game.party[0].reputation_add(28)";
                    PartyLeader.AddReputation(28);
                    break;
                case 593:
                    originalScript = "pc.money_adj(+1000)";
                    pc.AdjustMoney(+1000);
                    break;
                case 680:
                case 690:
                case 700:
                    originalScript = "game.quests[61].state = qs_botched";
                    SetQuestState(61, QuestState.Botched);
                    break;
                case 720:
                    originalScript = "game.quests[61].state = qs_botched;";
                    SetQuestState(61, QuestState.Botched);
                    ;
                    break;
                case 770:
                case 780:
                    originalScript = "create_item_in_inventory( 9196, pc ); game.quests[61].state = qs_botched";
                    Utilities.create_item_in_inventory(9196, pc);
                    SetQuestState(61, QuestState.Botched);
                    ;
                    break;
                case 810:
                    originalScript = "create_item_in_inventory( 9196, pc ); create_item_in_inventory( 9171, pc )";
                    Utilities.create_item_in_inventory(9196, pc);
                    Utilities.create_item_in_inventory(9171, pc);
                    ;
                    break;
                case 821:
                    originalScript = "pc.money_adj(-2000)";
                    pc.AdjustMoney(-2000);
                    break;
                case 840:
                    originalScript = "npc.reaction_adj( pc,-10)";
                    npc.AdjustReaction(pc, -10);
                    break;
                case 878:
                case 879:
                case 880:
                case 881:
                    originalScript = "game.global_flags[428] = 0";
                    SetGlobalFlag(428, false);
                    break;
                case 900:
                    originalScript = "game.areas[3] = 1; game.story_state = 3; game.quests[72].state = qs_completed; game.global_vars[562] = 6";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    SetQuestState(72, QuestState.Completed);
                    SetGlobalVar(562, 6);
                    ;
                    break;
                case 910:
                    originalScript = "game.quests[64].state = qs_mentioned";
                    SetQuestState(64, QuestState.Mentioned);
                    break;
                case 912:
                case 921:
                case 961:
                    originalScript = "game.quests[64].state = qs_accepted";
                    SetQuestState(64, QuestState.Accepted);
                    break;
                case 940:
                    originalScript = "game.quests[64].state = qs_botched";
                    SetQuestState(64, QuestState.Botched);
                    break;
                case 1010:
                    originalScript = "game.areas[3] = 1; game.story_state = 3; npc.reaction_adj( pc,+10); game.quests[72].state = qs_completed; game.global_vars[562] = 6";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    npc.AdjustReaction(pc, +10);
                    SetQuestState(72, QuestState.Completed);
                    SetGlobalVar(562, 6);
                    ;
                    break;
                case 1110:
                    originalScript = "npc.reaction_adj( pc,+10); game.quests[72].state = qs_completed; game.global_vars[562] = 6";
                    npc.AdjustReaction(pc, +10);
                    SetQuestState(72, QuestState.Completed);
                    SetGlobalVar(562, 6);
                    ;
                    break;
                case 1121:
                case 1122:
                case 1123:
                case 1124:
                    originalScript = "game.quests[64].state = qs_completed";
                    SetQuestState(64, QuestState.Completed);
                    break;
                case 1128:
                case 1129:
                    originalScript = "game.global_flags[883] = 0";
                    SetGlobalFlag(883, false);
                    break;
                case 1140:
                    originalScript = "game.quests[72].state = qs_completed; game.global_vars[562] = 6";
                    SetQuestState(72, QuestState.Completed);
                    SetGlobalVar(562, 6);
                    ;
                    break;
                case 1211:
                    originalScript = "game.quests[61].state = qs_completed";
                    SetQuestState(61, QuestState.Completed);
                    break;
                case 22000:
                    originalScript = "game.global_vars[909] = 32";
                    SetGlobalVar(909, 32);
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
                case 123:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                case 124:
                case 204:
                case 208:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 14);
                    return true;
                case 591:
                case 592:
                case 593:
                case 594:
                case 597:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 1);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
