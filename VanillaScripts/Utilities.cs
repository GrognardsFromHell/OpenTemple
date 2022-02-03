
using System;
using System.Collections.Generic;
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{

    public class Utilities
    {

        public static GameObject party_transfer_to(GameObject target, int oname)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                var item = pc.FindItemByName(oname);

                if (item != null)
                {
                    pc.TransferItemByNameTo(target, oname);
                    return item;
                }

            }

            return null;
        }
        public static GameObject find_npc_near(GameObject obj, int name)
        {
            foreach (var npc in ObjList.ListVicinity(obj.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((npc.GetNameId() == name))
                {
                    return npc;
                }

            }

            return null;
        }
        public static GameObject find_container_near(GameObject obj, int name)
        {
            foreach (var container in ObjList.ListVicinity(obj.GetLocation(), ObjectListFilter.OLC_CONTAINER))
            {
                if ((container.GetNameId() == name))
                {
                    return container;
                }

            }

            return null;
        }
        public static int group_average_level(GameObject pc)
        {
            var count = 0;

            var level = 0;

            foreach (var obj in pc.GetPartyMembers())
            {
                count = count + 1;

                level = level + obj.GetStat(Stat.level);

            }

            if ((count == 0))
            {
                return 1;
            }

            level = level + (count / 2);

            var avg = level / count;

            return avg;
        }
        public static int obj_percent_hp(GameObject obj)
        {
            var curr = obj.GetStat(Stat.hp_current);

            var max = obj.GetStat(Stat.hp_max);

            if ((max == 0))
            {
                return 100;
            }

            if ((curr > max))
            {
                curr = max;

            }

            if ((curr < 0))
            {
                curr = 0;

            }

            var percent = (curr * 100) / max;

            return percent;
        }
        public static int group_percent_hp(GameObject pc)
        {
            var percent = 0;

            var cnt = 0;

            foreach (var obj in pc.GetPartyMembers())
            {
                percent = percent + obj_percent_hp(obj);

                cnt = cnt + 1;

            }

            if ((cnt == 0))
            {
                percent = 100;

            }
            else if ((percent < 0))
            {
                percent = 0;

            }
            else
            {
                percent = percent / cnt;

            }

            return percent;
        }
        public static void create_item_in_inventory(int item_proto_num, GameObject npc)
        {
            var item = GameSystems.MapObject.CreateObject(item_proto_num, npc.GetLocation());

            if ((item != null))
            {
                npc.GetItem(item);
            }

            return;
        }
        public static bool is_daytime()
        {
            return GameSystems.TimeEvent.IsDaytime;
        }
        public static bool is_safe_to_talk(GameObject speaker, GameObject listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 15))
                {
                    return true;
                }

            }

            return false;
        }
        public static void start_game_with_quest(int quest_number)
        {
            SetQuestState(quest_number, QuestState.Accepted);
            FadeAndTeleport(0, 0, 987 + quest_number, 5001, 711, 521);
            return;
        }
        public static void start_game_with_botched_quest(int quest_number)
        {
            SetQuestState(quest_number, QuestState.Mentioned);
            SetQuestState(quest_number, QuestState.Botched);
            FadeAndTeleport(0, 0, 996 + quest_number, 5001, 711, 521);
            return;
        }
        public static int critter_is_unconscious(GameObject npc)
        {
            var curr = npc.GetStat(Stat.hp_current);

            if ((curr < 0))
            {
                return 1;
            }

            if ((npc.GetStat(Stat.subdual_damage) > curr))
            {
                return 1;
            }

            return 0;
        }
        public static bool obj_is_item(GameObject obj)
        {
            return (obj.type == ObjectType.projectile) || (obj.type == ObjectType.weapon) || (obj.type == ObjectType.ammo) || (obj.type == ObjectType.armor) || (obj.type == ObjectType.scroll) || (obj.type == ObjectType.bag);
        }
        public static void set_end_slides(GameObject attachee, GameObject triggerer)
        {
            if (GetGlobalFlag(189))
            {
                if (GetGlobalFlag(183))
                {
                    GameSystems.Movies.MovieQueueAdd(204);
                }
                else if (!GetGlobalFlag(326))
                {
                    GameSystems.Movies.MovieQueueAdd(200);
                }
                else
                {
                    GameSystems.Movies.MovieQueueAdd(201);
                }

            }
            else if (GetGlobalFlag(188))
            {
                GameSystems.Movies.MovieQueueAdd(202);
            }
            else if (GetGlobalFlag(326))
            {
                GameSystems.Movies.MovieQueueAdd(203);
            }
            else if (GetGlobalFlag(186))
            {
                GameSystems.Movies.MovieQueueAdd(205);
            }
            else if (GetGlobalFlag(187))
            {
                GameSystems.Movies.MovieQueueAdd(206);
            }
            else if (GetGlobalFlag(184))
            {
                GameSystems.Movies.MovieQueueAdd(207);
            }
            else if ((GetGlobalFlag(190)) || (GetGlobalFlag(191)))
            {
                GameSystems.Movies.MovieQueueAdd(208);
            }

            if (GetGlobalFlag(327))
            {
                GameSystems.Movies.MovieQueueAdd(209);
            }
            else if (GetGlobalFlag(326))
            {
                GameSystems.Movies.MovieQueueAdd(211);
            }

            if (GetGlobalFlag(328))
            {
                GameSystems.Movies.MovieQueueAdd(210);
            }

            if (!GetGlobalFlag(37) && (!(triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8002)))))
            {
                GameSystems.Movies.MovieQueueAdd(212);
            }
            else if (GetGlobalFlag(37))
            {
                GameSystems.Movies.MovieQueueAdd(213);
            }

            if (GetGlobalFlag(150))
            {
                GameSystems.Movies.MovieQueueAdd(215);
            }
            else if ((GetGlobalFlag(151)) || (GetGlobalFlag(152)) || (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031))))
            {
                GameSystems.Movies.MovieQueueAdd(214);
            }
            else
            {
                GameSystems.Movies.MovieQueueAdd(216);
            }

            if (GetGlobalVar(29) >= 10)
            {
                GameSystems.Movies.MovieQueueAdd(217);
            }

            if ((GetGlobalFlag(90) && GetGlobalFlag(324) && !GetGlobalFlag(365)))
            {
                GameSystems.Movies.MovieQueueAdd(218);
            }

            if ((GetQuestState(12) == QuestState.Completed && !GetGlobalFlag(364)))
            {
                GameSystems.Movies.MovieQueueAdd(219);
            }

            if ((GetQuestState(26) == QuestState.Completed && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(220);
            }

            if (GetQuestState(24) == QuestState.Completed)
            {
                GameSystems.Movies.MovieQueueAdd(221);
            }

            if (GetGlobalFlag(61))
            {
                GameSystems.Movies.MovieQueueAdd(222);
            }

            if ((GetGlobalFlag(97) && GetGlobalFlag(329)))
            {
                GameSystems.Movies.MovieQueueAdd(223);
            }

            if ((GetGlobalFlag(146) && GetGlobalFlag(104) && GetGlobalFlag(105) && GetGlobalFlag(106) && GetGlobalFlag(107)))
            {
                GameSystems.Movies.MovieQueueAdd(224);
            }

            if (!GetGlobalFlag(147))
            {
                GameSystems.Movies.MovieQueueAdd(225);
            }

            if (!GetGlobalFlag(177))
            {
                GameSystems.Movies.MovieQueueAdd(226);
            }

            if ((GetGlobalFlag(68) && !GetGlobalFlag(331)))
            {
                GameSystems.Movies.MovieQueueAdd(230);
            }

            if ((GetGlobalFlag(83) && !GetGlobalFlag(330)))
            {
                GameSystems.Movies.MovieQueueAdd(231);
            }

            if ((GetGlobalFlag(46) && !GetGlobalFlag(196) && !GetGlobalFlag(318)))
            {
                if (GetGlobalFlag(332))
                {
                    GameSystems.Movies.MovieQueueAdd(232);
                }
                else
                {
                    GameSystems.Movies.MovieQueueAdd(233);
                }

            }

            if ((GetQuestState(6) == QuestState.Completed && !GetGlobalFlag(333) && !GetGlobalFlag(334)))
            {
                if (GetGlobalFlag(332))
                {
                    GameSystems.Movies.MovieQueueAdd(234);
                }
                else
                {
                    GameSystems.Movies.MovieQueueAdd(235);
                }

            }

            if (!GetGlobalFlag(146))
            {
                GameSystems.Movies.MovieQueueAdd(236);
            }

            if (!GetGlobalFlag(105))
            {
                GameSystems.Movies.MovieQueueAdd(237);
            }

            if (!GetGlobalFlag(104))
            {
                GameSystems.Movies.MovieQueueAdd(238);
            }

            if (!GetGlobalFlag(106))
            {
                GameSystems.Movies.MovieQueueAdd(239);
            }

            if (!GetGlobalFlag(107))
            {
                GameSystems.Movies.MovieQueueAdd(240);
            }

            if (!GetGlobalFlag(335))
            {
                GameSystems.Movies.MovieQueueAdd(241);
            }

            if ((GetQuestState(15) != QuestState.Completed && !GetGlobalFlag(336)))
            {
                GameSystems.Movies.MovieQueueAdd(242);
            }

            if ((!GetGlobalFlag(299) && !GetGlobalFlag(337) && !GetGlobalFlag(336) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(243);
            }
            else if (((GetGlobalFlag(299) || GetGlobalFlag(337) || GetGlobalFlag(336)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(244);
            }
            else if ((GetGlobalFlag(186) || GetGlobalFlag(187) || GetGlobalFlag(184) || GetGlobalFlag(190) || GetGlobalFlag(191)))
            {
                GameSystems.Movies.MovieQueueAdd(245);
            }

            if ((GetGlobalFlag(186) || GetGlobalFlag(184) || GetGlobalFlag(190) || GetGlobalFlag(191)))
            {
                GameSystems.Movies.MovieQueueAdd(246);
            }
            else if (GetGlobalFlag(187))
            {
                GameSystems.Movies.MovieQueueAdd(247);
            }
            else if ((GetGlobalFlag(189) || GetGlobalFlag(188)))
            {
                GameSystems.Movies.MovieQueueAdd(248);
            }

            if (!GetGlobalFlag(338))
            {
                GameSystems.Movies.MovieQueueAdd(249);
            }

            if (GetQuestState(20) == QuestState.Completed)
            {
                GameSystems.Movies.MovieQueueAdd(250);
            }

            if (GetGlobalFlag(339))
            {
                GameSystems.Movies.MovieQueueAdd(251);
            }

            if ((GetQuestState(23) == QuestState.Completed && GetGlobalFlag(306)))
            {
                GameSystems.Movies.MovieQueueAdd(252);
            }

            if ((GetQuestState(25) == QuestState.Completed && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(253);
            }

            if (GetQuestState(27) == QuestState.Completed)
            {
                GameSystems.Movies.MovieQueueAdd(254);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                if ((PartyAlignment == Alignment.LAWFUL_EVIL))
                {
                    GameSystems.Movies.MovieQueueAdd(262);
                    GameSystems.Movies.MovieQueueAdd(255);
                }
                else
                {
                    GameSystems.Movies.MovieQueueAdd(261);
                }

            }
            else if ((PartyAlignment == Alignment.LAWFUL_EVIL))
            {
                if ((GetQuestState(28) == QuestState.Completed))
                {
                    GameSystems.Movies.MovieQueueAdd(255);
                }
                else
                {
                    GameSystems.Movies.MovieQueueAdd(256);
                }

            }

            if ((GetQuestState(29) == QuestState.Completed && !GetGlobalFlag(190) && !GetGlobalFlag(191)))
            {
                GameSystems.Movies.MovieQueueAdd(257);
            }

            if ((GetQuestState(30) == QuestState.Completed && !GetGlobalFlag(190) && !GetGlobalFlag(191)))
            {
                GameSystems.Movies.MovieQueueAdd(258);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(259);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8021)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(260);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8060)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(263);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8061)) && triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8029)) && triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8030)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                GameSystems.Movies.MovieQueueAdd(264);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8023)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                GameSystems.Movies.MovieQueueAdd(265);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8023)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                GameSystems.Movies.MovieQueueAdd(266);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8062)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                GameSystems.Movies.MovieQueueAdd(267);
            }

            return;
        }
        public static void set_join_slides(GameObject attachee, GameObject triggerer)
        {
            if (GetGlobalFlag(327))
            {
                GameSystems.Movies.MovieQueueAdd(209);
            }
            else if (GetGlobalFlag(326))
            {
                GameSystems.Movies.MovieQueueAdd(211);
            }

            if (GetGlobalFlag(328))
            {
                GameSystems.Movies.MovieQueueAdd(210);
            }

            if (!GetGlobalFlag(37) && (!(triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8002)))))
            {
                GameSystems.Movies.MovieQueueAdd(212);
            }
            else if (GetGlobalFlag(37))
            {
                GameSystems.Movies.MovieQueueAdd(213);
            }

            if (GetGlobalFlag(150))
            {
                GameSystems.Movies.MovieQueueAdd(215);
            }

            if (GetGlobalVar(29) >= 10)
            {
                GameSystems.Movies.MovieQueueAdd(217);
            }

            if ((GetGlobalFlag(90) && GetGlobalFlag(324) && !GetGlobalFlag(365)))
            {
                GameSystems.Movies.MovieQueueAdd(218);
            }

            if ((GetQuestState(12) == QuestState.Completed && !GetGlobalFlag(364)))
            {
                GameSystems.Movies.MovieQueueAdd(219);
            }

            if ((GetQuestState(26) == QuestState.Completed && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(220);
            }

            if (GetQuestState(24) == QuestState.Completed)
            {
                GameSystems.Movies.MovieQueueAdd(221);
            }

            if (GetGlobalFlag(61))
            {
                GameSystems.Movies.MovieQueueAdd(222);
            }

            if ((GetGlobalFlag(97) && GetGlobalFlag(329)))
            {
                GameSystems.Movies.MovieQueueAdd(223);
            }

            if ((GetGlobalFlag(68) && !GetGlobalFlag(331)))
            {
                GameSystems.Movies.MovieQueueAdd(230);
            }

            if ((GetGlobalFlag(83) && !GetGlobalFlag(330)))
            {
                GameSystems.Movies.MovieQueueAdd(231);
            }

            if ((GetGlobalFlag(46) && !GetGlobalFlag(196) && !GetGlobalFlag(318)))
            {
                if (GetGlobalFlag(332))
                {
                    GameSystems.Movies.MovieQueueAdd(232);
                }
                else
                {
                    GameSystems.Movies.MovieQueueAdd(233);
                }

            }

            if ((GetQuestState(6) == QuestState.Completed && !GetGlobalFlag(333) && !GetGlobalFlag(334)))
            {
                if (GetGlobalFlag(332))
                {
                    GameSystems.Movies.MovieQueueAdd(234);
                }
                else
                {
                    GameSystems.Movies.MovieQueueAdd(235);
                }

            }

            if ((GetQuestState(15) != QuestState.Completed && !GetGlobalFlag(336)))
            {
                GameSystems.Movies.MovieQueueAdd(242);
            }

            GameSystems.Movies.MovieQueueAdd(245);
            GameSystems.Movies.MovieQueueAdd(246);
            if (GetQuestState(20) == QuestState.Completed)
            {
                GameSystems.Movies.MovieQueueAdd(250);
            }

            if (GetGlobalFlag(339))
            {
                GameSystems.Movies.MovieQueueAdd(251);
            }

            if ((GetQuestState(23) == QuestState.Completed && GetGlobalFlag(306)))
            {
                GameSystems.Movies.MovieQueueAdd(252);
            }

            if ((GetQuestState(25) == QuestState.Completed && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                GameSystems.Movies.MovieQueueAdd(253);
            }

            if (GetQuestState(27) == QuestState.Completed)
            {
                GameSystems.Movies.MovieQueueAdd(254);
            }

            if ((PartyAlignment == Alignment.LAWFUL_EVIL))
            {
                if ((GetQuestState(28) == QuestState.Completed))
                {
                    GameSystems.Movies.MovieQueueAdd(255);
                }
                else
                {
                    GameSystems.Movies.MovieQueueAdd(256);
                }

            }

            if ((GetQuestState(29) == QuestState.Completed && !GetGlobalFlag(190) && !GetGlobalFlag(191)))
            {
                GameSystems.Movies.MovieQueueAdd(257);
            }

            if ((GetQuestState(30) == QuestState.Completed && !GetGlobalFlag(190) && !GetGlobalFlag(191)))
            {
                GameSystems.Movies.MovieQueueAdd(258);
            }

            GameSystems.Movies.MovieQueueAdd(206);
            return;
        }
        public static int should_heal_hp_on(GameObject obj)
        {
            var cur = obj.GetStat(Stat.hp_current);

            var max = obj.GetStat(Stat.hp_max);

            if ((!(cur == max)) && (obj.GetStat(Stat.hp_current) >= -9))
            {
                return 1;
            }

            return 0;
        }
        public static int should_heal_disease_on(GameObject obj)
        {
            if ((obj.GetStat(Stat.hp_current) >= -9))
            {
                return 1;
            }

            return 0;
        }
        public static int should_heal_poison_on(GameObject obj)
        {
            if ((obj.GetStat(Stat.hp_current) >= -9))
            {
                return 1;
            }

            return 0;
        }
        public static int should_resurrect_on(GameObject obj)
        {
            if ((obj.GetStat(Stat.hp_current) <= -10))
            {
                return 1;
            }

            return 0;
        }


    }
}
