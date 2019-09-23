
using System;
using System.Collections.Generic;
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

namespace Scripts
{

    public class Utilities
    {
        // Neccessary for timedEventAdd to work across saves and loads - CtB

        // function: party_transfer_to( target, oname )
        // arguments: target - PyObjHandle (the recepient of the item)
        // oname - the internal name of the object to be trasnfered
        // from the party
        // returns: the PyObjHandle of the object found, OBJ_HANDLE_NULL if not found

        public static GameObjectBody party_transfer_to(GameObjectBody target, int oname)
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
        public static GameObjectBody find_npc_near(GameObjectBody obj, int name)
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
        public static GameObjectBody find_container_near(GameObjectBody obj, int name)
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
        public static int group_average_level(GameObjectBody pc)
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

            // Old method###############
            // level = level + ( count / 2 )
            // avg = level / count
            var avg = (level / count) + (count - 4) / 2;
            return avg;
        }
        public static int group_wilderness_lore(GameObjectBody pc)
        {
            var high = 0;
            var level = 0;
            foreach (var obj in pc.GetPartyMembers())
            {
                level = obj.GetSkillLevel(SkillId.wilderness_lore);
                if ((level > high))
                {
                    high = level;
                }

            }

            if ((high == 0))
            {
                return 1;
            }

            return high;
        }
        public static int obj_percent_hp(GameObjectBody obj)
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
        public static int group_percent_hp(GameObjectBody pc)
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
        public static int group_pc_percent_hp(GameObjectBody attachee, GameObjectBody pc)
        {
            var percent = 0;
            var cnt = 0;
            foreach (var obj in pc.GetPartyMembers())
            {
                if ((obj != attachee))
                {
                    percent = percent + obj_percent_hp(obj);
                    cnt = cnt + 1;
                }

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
        public static int group_kobort_percent_hp(GameObjectBody attachee, GameObjectBody pc)
        {
            var percent = 0;
            var cnt = 0;
            foreach (var obj in pc.GetPartyMembers())
            {
                if ((obj != attachee && obj.GetNameId() != 8005))
                {
                    percent = percent + obj_percent_hp(obj);
                    cnt = cnt + 1;
                }

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
        public static GameObjectBody create_item_in_inventory(int item_proto_num, GameObjectBody npc)
        {
            var item = GameSystems.MapObject.CreateObject(item_proto_num, npc.GetLocation());
            if ((item != null))
            {
                npc.GetItem(item);
            }

            return item;
        }
        public static bool is_daytime()
        {
            return GameSystems.TimeEvent.IsDaytime;
        }
        public static bool is_safe_to_talk(GameObjectBody speaker, GameObjectBody listener, int distt = 15, bool pc_only = true)
        {
            if ((speaker.HasLineOfSight(listener) && (listener.type == ObjectType.pc || !pc_only)))
            {
                if ((speaker.DistanceTo(listener) <= distt))
                {
                    return true;
                }

            }

            return false;
        }
        public static void start_game_with_quest(int quest_number)
        {
            SetQuestState(quest_number, QuestState.Accepted);
            // hommlet movies start at 1009 and go to 1017
            // but the quests start at 22, so offset by 987
            FadeAndTeleport(0, 0, 0, 5001, 711, 521);
            return;
        }
        public static void start_game_with_botched_quest(int quest_number)
        {
            SetQuestState(quest_number, QuestState.Mentioned);
            SetQuestState(quest_number, QuestState.Botched);
            // hommlet no-voice movies start at 1018 and go to 1026
            // but the quests start at 22, so offset by 996
            FadeAndTeleport(0, 0, 0, 5001, 711, 521);
            return;
        }
        public static int critter_is_unconscious(GameObjectBody npc)
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
        // HTN - returns true if obj is an "item" (obj.h)

        public static bool obj_is_item(GameObjectBody obj)
        {
            return (obj.type == ObjectType.projectile) || (obj.type == ObjectType.weapon) || (obj.type == ObjectType.ammo) || (obj.type == ObjectType.armor) || (obj.type == ObjectType.scroll) || (obj.type == ObjectType.bag);
        }
        public static List<GameObjectBody> pyspell_targetarray_copy_to_obj_list(SpellPacketBody spell)
        {
            var copy_list = new List<GameObjectBody>();
            foreach (var item in spell.Targets)
            {
                copy_list.Add(item.Object);
            }

            return copy_list;
        }

        // ENDSLIDES FOR STANDARD GAME
        // Function: set_end_slides( npc, pc )
        // Author  : Tom Decker
        // Returns : nada
        // Purpose : queues up all the end slides for the end game

        public static void set_end_slides(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // ZUGGTMOY
            if (GetGlobalFlag(189))
            {
                // zuggtmoy dead
                if (GetGlobalFlag(183))
                {
                    // took pillar
                    GameSystems.Movies.MovieQueueAdd(204);
                }
                // Killed after her pillar of platinum was accepted and used to break open the enchanted seal, Zuggtmoy's spirit is banished to the Abyss for 40 years. But she will return to the Prime Material for vengeance...
                else if (!GetGlobalFlag(326))
                {
                    // haven't destroyed orb
                    GameSystems.Movies.MovieQueueAdd(200);
                }
                else
                {
                    // Zuggtmoy was killed while still trapped on the Prime Material plane. Her spirit destroyed, a major force for evil is now gone from the world of men.
                    GameSystems.Movies.MovieQueueAdd(201);
                }

            }
            // Weakened by the destruction of the Orb of Golden Death, Zuggtmoy was killed while still trapped on the Prime Material plane. Her spirit destroyed, a major force for evil is now gone from the world of men.
            else if (GetGlobalFlag(188))
            {
                // zuggtmoy banished
                GameSystems.Movies.MovieQueueAdd(202);
            }
            // Her throne defaced, Zuggtmoy is banished to Abyss for 66 years. She plans to reform the Temple on her return.
            else if (GetGlobalFlag(326))
            {
                // destroyed orb
                GameSystems.Movies.MovieQueueAdd(203);
            }
            // Four days after the destruction of the Orb of Golden Death, a weakened Zuggtmoy is banished to Abyss for 40 years. But she will return to the Prime Material for vengeance...
            else if (GetGlobalFlag(186))
            {
                // zuggtmoy surrendered
                GameSystems.Movies.MovieQueueAdd(205);
            }
            // As your unwilling follower, Zuggtmoy and her temple raze the surrounding countryside, and all good people whisper your name with fear.
            else if (GetGlobalFlag(187))
            {
                // zuggtmoy gave treasure
                GameSystems.Movies.MovieQueueAdd(206);
            }
            // For a while, Zuggtmoy remains in her lair, but nature is demonic and she cannot be contained for long. She gathers her strength, and soon her evil machinations are loose in the world again.
            else if (GetGlobalFlag(184))
            {
                // surrendered to zugtmoy
                GameSystems.Movies.MovieQueueAdd(207);
            }
            // With your help, the temple rapidly gains power, and Furyondy and Veluna are threatened by its aggression. The evil one Iuz attacks from the north in a Clamp of Evil that changes the face of Greyhawk.
            else if ((GetGlobalFlag(190)) || (GetGlobalFlag(191)))
            {
                // gave orb to zuggtmoy for pillar or zuggtmoy charmed a PC
                GameSystems.Movies.MovieQueueAdd(208);
            }

            // You are a thrall of Zuggtmoy. You are used on the frontline in the temple's raids and die an anonymous death on some nameless battlefield.
            // IUZ & CUTHBERT
            if (GetGlobalFlag(327))
            {
                // iuz dead
                GameSystems.Movies.MovieQueueAdd(209);
            }
            // In an almost unbelievable turn of events, you have managed to destroy Iuz. However, he is not killed but has immediately returned to his soul object on Zuggtmoy's Abyssal plane. It will take him years to recover his powers, during which time his country is overrun and his followers are scattered. Iuz fears you.
            else if (GetGlobalFlag(326))
            {
                // destroyed orb
                GameSystems.Movies.MovieQueueAdd(211);
            }

            // Iuz is weakened by the Orb's destruction. He now ranks you among his personal enemies and plans a revenge suitable for this annoyance.
            if (GetGlobalFlag(328))
            {
                // cuthbert showed up
                GameSystems.Movies.MovieQueueAdd(210);
            }

            // Iuz and St. Cuthbert meet. Their ???discussion??? is not for mortals to witness, but the enmity between their followers escalates.
            // PRINCE THROMMEL
            if (GetGlobalFlag(150))
            {
                // thrommel dead
                GameSystems.Movies.MovieQueueAdd(215);
            }
            // With the death of Prince Thrommel, divined by the priests of Furyondy, that kingdom suffers greatly in the wars to come.
            else if ((GetGlobalFlag(151)) || (GetGlobalFlag(152)) || (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031))))
            {
                // thrommel freed and out OR thrommel reward scheduled OR thrommel in party
                GameSystems.Movies.MovieQueueAdd(214);
            }
            else
            {
                // Prince Thrommel is rescued and marries Princess Jolene of Veluna. The kingdoms of Furyondy and Veluna unite, and you are made knights of the kingdom.
                GameSystems.Movies.MovieQueueAdd(216);
            }

            // Left inside the temple dungeons, Prince Thrommel eventually becomes a vampire and strikes terror into the local populace. His sword Fragarach is lost in the mists of time...
            // TEMPLE & ASSOCIATES
            if ((GetGlobalFlag(146) && GetGlobalFlag(104) && GetGlobalFlag(105) && GetGlobalFlag(106) && GetGlobalFlag(107)))
            {
                // hedrack dead, romag dead, belsornig dead, kelno dead, alrrem dead
                GameSystems.Movies.MovieQueueAdd(224);
            }

            // With no high priests left alive after your raids, the temple of elemental evil loses its remaining worshippers, who disband and scatter to the four winds.
            if (GetGlobalFlag(339))
            {
                // you are hedrack's bitch
                GameSystems.Movies.MovieQueueAdd(251);
            }

            // With your help, the Temple rises in power and begins anew its attacks on neighboring countries.
            if ((!GetGlobalFlag(146) && !GetGlobalFlag(189)))
            {
                // hedrack alive
                GameSystems.Movies.MovieQueueAdd(236);
            }

            // Supreme Commander Hedrack escaped from the Temple of Elemental Evil, but he had earned the displeasure of Iuz for many years to come.
            if (!GetGlobalFlag(147))
            {
                // senshock alive
                GameSystems.Movies.MovieQueueAdd(225);
            }

            // Senshock, the Lord Wizard of the Temple, escapes to Verbobonc where he secretly plans a return to power.
            if (!GetGlobalFlag(104))
            {
                // romag alive
                GameSystems.Movies.MovieQueueAdd(238);
            }

            // Romag, high priest of the powerful Earth Temple, escaped and was too ashamed to ever return to the Temple of Elemental Evil.
            if (!GetGlobalFlag(105))
            {
                // belsornig alive
                GameSystems.Movies.MovieQueueAdd(237);
            }

            // Belsornig, the plotting high priest of the Water temple, managed to escape from the Temple of Elemental Evil unharmed. Surely, he is somewhere on Oerth planning new diabolical schemes even now...
            if (!GetGlobalFlag(107))
            {
                // alrrem alive
                GameSystems.Movies.MovieQueueAdd(240);
            }

            // Alrrem, high priest of the Fire Temple, escaped from the Temple of Elemental Evil, but not with his sanity intact. He still roams the countryside as a raving madman, his mind shattered.
            if (!GetGlobalFlag(106))
            {
                // kelno alive
                GameSystems.Movies.MovieQueueAdd(239);
            }

            // Kelno, high priest of the Air Temple, escaped from the Temple of Elemental Evil, but without funds or friends, he lives out his days as a beggar on the streets of Mitrik, fearful and paranoid of being discovered by good or evil alike.
            if (!GetGlobalFlag(335))
            {
                // falrinth alive
                GameSystems.Movies.MovieQueueAdd(241);
            }

            // Falrinth the wizard managed to escape the Temple of Elemental Evil. He dares not contact any former ally, having made too many enemies while in the service of evil there. He is currently slinking through the underdark, trying to regain the favor of the demoness Lolth, his patron.
            if (!GetGlobalFlag(338))
            {
                // smigmal alive
                GameSystems.Movies.MovieQueueAdd(249);
            }

            // Smigmal Redhand, the temple assassin and femme fatale, escaped and returned to the orc tribe where she was spawned to become its leader, the first time a female ever ruled an orc tribe. Her burning hatred for humanity never dimmed. She was eventually eaten by an ogre, who suffered great indigestion afterwards.
            if (!GetGlobalFlag(177))
            {
                // feldrin alive
                GameSystems.Movies.MovieQueueAdd(226);
            }

            // Feldrin flees over the Lortmil Mountains and starts his own rogue's guild in the Principality of Ulek. He is eventually killed during a skirmish with a rival guild.
            if (!GetGlobalFlag(37) && (!(triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8002)))))
            {
                // lareth alive AND not in party
                GameSystems.Movies.MovieQueueAdd(212);
            }
            // Lareth the Beautiful escapes and starts his own temple of evil, dedicated to Llolth, the Spider Goddess. She is not placated and eventually turns Lareth into a drider.
            else if (GetGlobalFlag(37))
            {
                // lareth dead
                GameSystems.Movies.MovieQueueAdd(213);
            }

            // Lareth is killed, and you gain the enmity of his patron goddess Llolth. Over the years, she dispatches many drow assassins to kill you. All fail.
            // NULB & ASSOCIATES
            if ((GetGlobalFlag(186) || GetGlobalFlag(184) || GetGlobalFlag(190) || GetGlobalFlag(191)))
            {
                // zuggtmoy surrendered or you surrendered to zuggtmoy or you gave orb to zuggtmoy for pillar or zuggtmoy charmed you
                GameSystems.Movies.MovieQueueAdd(246);
            }
            // Following the growth and prosperity of the Temple, Nulb expands into a great city, where the evil and wicked join forces and plot their evil schemes.
            else if (GetGlobalFlag(187))
            {
                // zuggtmoy gave you treasure
                GameSystems.Movies.MovieQueueAdd(247);
            }
            // As Zuggtmoy was spared, so was Nulb. Its wicked inhabitants continued their evil ways unchecked.
            else if ((GetGlobalFlag(189) || GetGlobalFlag(188)))
            {
                // zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(248);
            }

            // With the destruction of the Temple of Elemental Evil, the fall of the village of Nulb came soon thereafter.
            if ((GetGlobalFlag(97) && GetGlobalFlag(329)))
            {
                // tolub dead and grud dead
                GameSystems.Movies.MovieQueueAdd(223);
            }

            // With the deaths of Tolub and Grud Squinteye, the river pirates lose direction and eventually disband.
            if ((GetGlobalFlag(90) && GetGlobalFlag(324) && !GetGlobalFlag(365)))
            {
                // gay bertram free AND gay bertram met AND gay betram alive
                GameSystems.Movies.MovieQueueAdd(218);
            }

            // You and Bertram are married in a small ceremony, and he opens a dentistry office in Verbobonc. You live happily ever after.
            if ((GetGlobalFlag(83) && !GetGlobalFlag(330)))
            {
                // exclusive with jenelda and jenelda alive
                GameSystems.Movies.MovieQueueAdd(231);
            }

            // After your beautiful wedding, you and Jenelda travel throughout Oerth. You were last seen in the east in the Grandwood Forest.
            // HOMMLET & ASSOCIATES
            if ((!GetGlobalFlag(299) && !GetGlobalFlag(337) && !GetGlobalFlag(336) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // terjon alive and jaroo alive and burne and rufus alive and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(243);
            }
            // In the quiet years that followed the fall of the temple of elemental evil, Hommlet grows into a bustling and prosperous town under the guidance of its elder citizens.
            else if (((GetGlobalFlag(299) || GetGlobalFlag(337) || GetGlobalFlag(336)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // terjon dead or jaroo dead or burne or rufus dead and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(244);
            }
            // As is often the case in sleepy, little villages, the excitement of the destruction of the temple soon died down, and life in Hommlet continued unaffected by such portentous events.
            else if ((GetGlobalFlag(186) || GetGlobalFlag(187) || GetGlobalFlag(184) || GetGlobalFlag(190) || GetGlobalFlag(191)))
            {
                // zuggtmoy surrendered or gave you treasure or you surrendered to zuggtmoy or you gave orb to zuggtmoy for pillar or zuggtmoy charmed you
                GameSystems.Movies.MovieQueueAdd(245);
            }

            // Hommlet does not survive Zuggtmoy's rampages and is burned to the ground. Those who did not flee early were subjected to torture and death.
            if ((GetQuestState(15) != QuestState.Completed && !GetGlobalFlag(336)))
            {
                // didn't complete agent revealed quest and burne or rufus dead
                GameSystems.Movies.MovieQueueAdd(242);
            }

            // The tower of Burne and Rufus remained under construction for many years, plagued by mysterious delays and inauspicious accidents.
            if (GetQuestState(20) == QuestState.Completed)
            {
                // rescued paida
                GameSystems.Movies.MovieQueueAdd(250);
            }

            // In returning Paida to Valden, you have overcome their history of misery and pain and allowed true love to prosper.
            if (GetGlobalFlag(61))
            {
                // furnok retired
                GameSystems.Movies.MovieQueueAdd(222);
            }

            // Furnok of Ferd uses his wealth to become a well-known and well-respected citizen of Verbobonc.
            if ((GetQuestState(12) == QuestState.Completed && !GetGlobalFlag(364)))
            {
                // completed one bride for one player quest AND fruella alive
                GameSystems.Movies.MovieQueueAdd(219);
            }

            // As you promised, you and Fruella are married in the church of St. Cuthbert. She tames your wild, adventuring nature, and you settle down as a farmer in Hommlet and have many, many, many children.
            if ((GetGlobalFlag(68) && !GetGlobalFlag(331)))
            {
                // married laszlo and laszlo alive
                GameSystems.Movies.MovieQueueAdd(230);
            }

            // You marry Laszlo. You settle down with him, live the life of a herdsman's wife and have many children.
            if ((GetGlobalFlag(46) && !GetGlobalFlag(196) && !GetGlobalFlag(318)))
            {
                // married meleny and meleny not dead or mistreated
                if (GetGlobalFlag(332))
                {
                    // killed an ochre jelly
                    GameSystems.Movies.MovieQueueAdd(232);
                }
                else
                {
                    // You marry Meleny and take her on all sorts of grand adventures.
                    GameSystems.Movies.MovieQueueAdd(233);
                }

            }

            // You marry Meleny and take her on all sorts of grand adventures, until an ochre jelly eats her.
            if ((GetQuestState(6) == QuestState.Completed && !GetGlobalFlag(333) && !GetGlobalFlag(334)))
            {
                // completed cupid's arrow quest and filliken alive and mathilde alive
                if (GetGlobalFlag(332))
                {
                    // killed an ochre jelly
                    GameSystems.Movies.MovieQueueAdd(234);
                }
                else
                {
                    // You convince Filliken and Mathilde to marry, and they enjoy many happy years together in Hommlet.
                    GameSystems.Movies.MovieQueueAdd(235);
                }

            }

            // You convince Filliken and Mathilde to marry, and they enjoy many happy years together in Hommlet, until they are eaten by ochre jellies.
            // JOINABLE NPCS
            if (GetGlobalVar(29) >= 10)
            {
                // 10 or more NPCs killed while in party
                GameSystems.Movies.MovieQueueAdd(217);
            }

            // You abuse your faithful followers, many of whom lose their lives in the dungeons beneath the temple.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8060)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // morgan in party and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(263);
            }

            // Morgan survives the encounter with Zuggtmoy and thanks you most profusely for saving his life. He returns to his life as a pirate, and eventually becomes a feared captain of his own ship on the high seas.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8023)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                // oohlgrist in party and zuggtmoy dead or banished or surrendered
                GameSystems.Movies.MovieQueueAdd(265);
            }

            // Like the coward he is, Oohlgrist sneaks off soon after your encounter with Zuggtmoy. He is never heard from again.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8034)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                // scorrp in party and zuggtmoy dead or banished or surrendered
                GameSystems.Movies.MovieQueueAdd(266);
            }

            // To show his gratitude, Scorpp swears loyalty to you and is evermore your faithful servant.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8061)) && triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8029)) && triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8030)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                // ted, ed, and ed in party and zuggtmoy dead or banished or surrendered
                GameSystems.Movies.MovieQueueAdd(264);
            }

            // The stars flicker, the gods pause, the winds stop. Throughout the world there is silence, as if the world has stopped to catch its breath. For you have survived the game with Ted, Ed and Ed. Congratulations.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // otis in party and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(259);
            }

            // Otis is very pleased with the results of your adventures together. His reports to Furyondy and Veluna gain you even more favor in those countries.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8021)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // y'dey in party and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(260);
            }

            // Y'dey is overjoyed to have adventured with you. Her reports to the Archcleric of Veluna feature you most prominently, and the Archcleric is impressed with your results.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8062)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                // zaxis in party and zuggtmoy dead or banished or surrendered
                GameSystems.Movies.MovieQueueAdd(267);
            }

            // Shortly after your Temple adventure, Zaxis leaves your party to continue looking for his sister. He finally finishes his song about your grand adventure, although he never does find a word that rhymes with Zuggtmoy.
            // VIGNETTE RESOLUTION
            if ((GetQuestState(23) == QuestState.Completed && GetGlobalFlag(306)))
            {
                // completed NG vignette quest and discovered y'dey
                GameSystems.Movies.MovieQueueAdd(252);
            }

            // Discovering Cannoness Y'dey alive proved to be a high point in your group's adventures.
            if (GetQuestState(24) == QuestState.Completed)
            {
                // completed CG opening vignette quest
                GameSystems.Movies.MovieQueueAdd(221);
            }

            // Countess Tillahi encourages Queen Yolande of Celene to join forces with Veluna to move against Iuz and his city of Molag.
            if ((GetQuestState(25) == QuestState.Completed && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // completed LN vignette quest and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(253);
            }

            // In recognition of your completion of the arduous task of destroying the Temple of Elemental Evil, the Lord Mayor of Greyhawk grants you knighthood.
            if ((GetQuestState(26) == QuestState.Completed && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // completed TN opening vignette quest AND zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(220);
            }

            // As life returns to normal after the events of the last weeks, the druids of the Gnarley Woods move through Hommlet in an effort to rebalance the area.
            if ((GetQuestState(27) == QuestState.Completed))
            {
                // completed CN vignette quest
                GameSystems.Movies.MovieQueueAdd(254);
            }

            // Your tasks near Hommlet are complete, and you move onward to new and bigger adventures.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // thrommel in party and zuggtmoy dead or banished
                if ((PartyAlignment == Alignment.LAWFUL_EVIL))
                {
                    GameSystems.Movies.MovieQueueAdd(262);
                    // Prince Thrommel is most useful in helping overcome Zuggtmoy.  He is most surprised when you kill him afterwards for his sword.
                    GameSystems.Movies.MovieQueueAdd(255);
                }
                else
                {
                    // With the recovery of the artifact sword Fragarach and for safely conveying it a representative of the temple of Hextor, you have advanced the cause of Hextor tremendously. 			# You are granted of favor the god Hextor.
                    GameSystems.Movies.MovieQueueAdd(261);
                }

            }
            // Prince Thrommel is most useful in helping overcome Zuggtmoy.
            else if ((PartyAlignment == Alignment.LAWFUL_EVIL))
            {
                if ((GetQuestState(28) == QuestState.Completed))
                {
                    // completed LE vignette quest
                    GameSystems.Movies.MovieQueueAdd(255);
                }
                else
                {
                    // With the recovery of the artifact sword Fragarach and for safely conveying it a representative of the temple of Hextor, you have advanced the cause of Hextor tremendously. 			# You are granted of favor the god Hextor.
                    GameSystems.Movies.MovieQueueAdd(256);
                }

            }

            // For failing to deliver the sword Fragarach to the representative of Hextor, you have earned the enmity of the god. The shadow of his curse follows you for years to come.
            if ((GetQuestState(29) == QuestState.Completed && !GetGlobalFlag(190) && !GetGlobalFlag(191)))
            {
                // completed NE vignette quest and didn't give orb to zuggtmoy and zuggtmoy didn't charm a PC
                GameSystems.Movies.MovieQueueAdd(257);
            }

            // You are a richer and more powerful than ever. Yes, this has been an excellent time for evil.
            if ((GetQuestState(30) == QuestState.Completed && !GetGlobalFlag(190) && !GetGlobalFlag(191)))
            {
                // completed CE vignette quest and didn't give orb to zuggtmoy and zuggtmoy didn't charm a PC
                GameSystems.Movies.MovieQueueAdd(258);
            }

            // You killed a lot of good and a lot of evil. You did whatever you wanted and in the end, you bowed to no one.
            return;
        }
        // SLIDES FOR NC MID-GAME
        // Function: set_end_slides_nc( npc, pc )
        // Author  : Tom Decker
        // Returns : nada
        // Purpose : queues up all the end slides before going to verbobonc/co8

        public static void set_end_slides_nc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // ZUGGTMOY
            if (GetGlobalFlag(189))
            {
                // zuggtmoy dead
                if (GetGlobalFlag(183))
                {
                    // took pillar
                    GameSystems.Movies.MovieQueueAdd(204);
                }
                // Killed after her pillar of platinum was accepted and used to break open the enchanted seal, Zuggtmoy's spirit is banished to the Abyss for 40 years. But she will return to the Prime Material for vengeance...
                else if (!GetGlobalFlag(326))
                {
                    // haven't destroyed orb
                    GameSystems.Movies.MovieQueueAdd(200);
                }
                else
                {
                    // Zuggtmoy was killed while still trapped on the Prime Material plane. Her spirit destroyed, a major force for evil is now gone from the world of men.
                    GameSystems.Movies.MovieQueueAdd(201);
                }

            }
            // Weakened by the destruction of the Orb of Golden Death, Zuggtmoy was killed while still trapped on the Prime Material plane. Her spirit destroyed, a major force for evil is now gone from the world of men.
            else if (GetGlobalFlag(188))
            {
                // zuggtmoy banished
                GameSystems.Movies.MovieQueueAdd(202);
            }
            // Her throne defaced, Zuggtmoy is banished to Abyss for 66 years. She plans to reform the Temple on her return.
            else if (GetGlobalFlag(326))
            {
                // destroyed orb
                GameSystems.Movies.MovieQueueAdd(203);
            }
            // Four days after the destruction of the Orb of Golden Death, a weakened Zuggtmoy is banished to Abyss for 40 years. But she will return to the Prime Material for vengeance...
            else if (GetGlobalFlag(187))
            {
                // zuggtmoy gave treasure
                GameSystems.Movies.MovieQueueAdd(206);
            }

            // For a while, Zuggtmoy remains in her lair, but nature is demonic and she cannot be contained for long. She gathers her strength, and soon her evil machinations are loose in the world again.
            // IUZ & CUTHBERT
            if (GetGlobalFlag(328))
            {
                // cuthbert showed up
                GameSystems.Movies.MovieQueueAdd(210);
            }

            // Iuz and St. Cuthbert meet. Their ???discussion??? is not for mortals to witness, but the enmity between their followers escalates.
            // TEMPLE & ASSOCIATES
            if ((GetGlobalFlag(146) && GetGlobalFlag(104) && GetGlobalFlag(105) && GetGlobalFlag(106) && GetGlobalFlag(107)))
            {
                // hedrack dead, romag dead, belsornig dead, kelno dead, alrrem dead
                GameSystems.Movies.MovieQueueAdd(224);
            }

            // With no high priests left alive after your raids, the temple of elemental evil loses its remaining worshippers, who disband and scatter to the four winds.
            if ((!GetGlobalFlag(146) && !GetGlobalFlag(189)))
            {
                // hedrack alive and zuggtmoy alive
                GameSystems.Movies.MovieQueueAdd(236);
            }

            // Supreme Commander Hedrack escaped from the Temple of Elemental Evil, but he had earned the displeasure of Iuz for many years to come.
            if (!GetGlobalFlag(147))
            {
                // senshock alive
                GameSystems.Movies.MovieQueueAdd(225);
            }

            // Senshock, the Lord Wizard of the Temple, escapes to Verbobonc where he secretly plans a return to power.
            if (!GetGlobalFlag(104))
            {
                // romag alive
                GameSystems.Movies.MovieQueueAdd(238);
            }

            // Romag, high priest of the powerful Earth Temple, escaped and was too ashamed to ever return to the Temple of Elemental Evil.
            if (!GetGlobalFlag(105))
            {
                // belsornig alive
                GameSystems.Movies.MovieQueueAdd(237);
            }

            // Belsornig, the plotting high priest of the Water temple, managed to escape from the Temple of Elemental Evil unharmed. Surely, he is somewhere on Oerth planning new diabolical schemes even now...
            if (!GetGlobalFlag(107))
            {
                // alrrem alive
                GameSystems.Movies.MovieQueueAdd(240);
            }

            // Alrrem, high priest of the Fire Temple, escaped from the Temple of Elemental Evil, but not with his sanity intact. He still roams the countryside as a raving madman, his mind shattered.
            if (!GetGlobalFlag(106))
            {
                // kelno alive
                GameSystems.Movies.MovieQueueAdd(239);
            }

            // Kelno, high priest of the Air Temple, escaped from the Temple of Elemental Evil, but without funds or friends, he lives out his days as a beggar on the streets of Mitrik, fearful and paranoid of being discovered by good or evil alike.
            if (!GetGlobalFlag(335))
            {
                // falrinth alive
                GameSystems.Movies.MovieQueueAdd(241);
            }

            // Falrinth the wizard managed to escape the Temple of Elemental Evil. He dares not contact any former ally, having made too many enemies while in the service of evil there. He is currently slinking through the underdark, trying to regain the favor of the demoness Lolth, his patron.
            if (!GetGlobalFlag(338))
            {
                // smigmal alive
                GameSystems.Movies.MovieQueueAdd(249);
            }

            // Smigmal Redhand, the temple assassin and femme fatale, escaped and returned to the orc tribe where she was spawned to become its leader, the first time a female ever ruled an orc tribe. Her burning hatred for humanity never dimmed. She was eventually eaten by an ogre, who suffered great indigestion afterwards.
            if (!GetGlobalFlag(177))
            {
                // feldrin alive
                GameSystems.Movies.MovieQueueAdd(226);
            }

            // Feldrin flees over the Lortmil Mountains and starts his own rogue's guild in the Principality of Ulek. He is eventually killed during a skirmish with a rival guild.
            if (!GetGlobalFlag(37) && (!(triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8002)))))
            {
                // lareth alive AND not in party
                GameSystems.Movies.MovieQueueAdd(212);
            }
            // Lareth the Beautiful escapes and starts his own temple of evil, dedicated to Llolth, the Spider Goddess. She is not placated and eventually turns Lareth into a drider.
            else if (GetGlobalFlag(37))
            {
                // lareth dead
                GameSystems.Movies.MovieQueueAdd(213);
            }

            // Lareth is killed, and you gain the enmity of his patron goddess Llolth. Over the years, she dispatches many drow assassins to kill you. All fail.
            // VIGNETTE RESOLUTION
            if ((GetQuestState(23) == QuestState.Completed && GetGlobalFlag(306)))
            {
                // completed NG vignette quest and discovered y'dey
                GameSystems.Movies.MovieQueueAdd(252);
            }

            // Discovering Cannoness Y'dey alive proved to be a high point in your group's adventures.
            if (GetQuestState(24) == QuestState.Completed)
            {
                // completed CG opening vignette quest
                GameSystems.Movies.MovieQueueAdd(221);
            }

            // Countess Tillahi encourages Queen Yolande of Celene to join forces with Veluna to move against Iuz and his city of Molag.
            if ((GetQuestState(25) == QuestState.Completed && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // completed LN vignette quest and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(253);
            }

            // In recognition of your completion of the arduous task of destroying the Temple of Elemental Evil, the Lord Mayor of Greyhawk grants you knighthood.
            if ((GetQuestState(26) == QuestState.Completed && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // completed TN opening vignette quest AND zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(220);
            }

            // As life returns to normal after the events of the last weeks, the druids of the Gnarley Woods move through Hommlet in an effort to rebalance the area.
            if ((GetQuestState(29) == QuestState.Completed && !GetGlobalFlag(190) && !GetGlobalFlag(191)))
            {
                // completed NE vignette quest and didn't give orb to zuggtmoy and zuggtmoy didn't charm a PC
                GameSystems.Movies.MovieQueueAdd(257);
            }

            // You are a richer and more powerful than ever. Yes, this has been an excellent time for evil.
            if ((GetQuestState(30) == QuestState.Completed && !GetGlobalFlag(190) && !GetGlobalFlag(191)))
            {
                // completed CE vignette quest and didn't give orb to zuggtmoy and zuggtmoy didn't charm a PC
                GameSystems.Movies.MovieQueueAdd(258);
            }

            // You killed a lot of good and a lot of evil. You did whatever you wanted and in the end, you bowed to no one.
            // MOVING ON
            if (GetGlobalFlag(500))
            {
                // playing NC
                GameSystems.Movies.MovieQueueAdd(254);
            }

            // Your tasks near Hommlet are complete, and you move onward to new and bigger adventures.
            return;
        }
        // ENDSLIDES FOR NC END-GAME
        // Function: set_co8_slides( npc, pc )
        // Author  : Tom Decker
        // Returns : nada
        // Purpose : queues up all the co8 slides for the NC end game

        public static void set_end_slides_co8(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // INTRO
            GameSystems.Movies.MovieQueueAdd(700);
            // IUZ & CUTHBERT & ALZOLL
            if ((GetGlobalFlag(327) || (GetQuestState(102) == QuestState.Completed && GetGlobalVar(694) == 4)))
            {
                // iuz dead or demons and demigods quest completed and iuz's 4 avatars dead
                GameSystems.Movies.MovieQueueAdd(533);
                // In an almost unbelievable turn of events, you have managed to destroy Iuz. However, he is not killed but has immediately returned to his soul object on Zuggtmoy's Abyssal plane. It will take him years to recover his powers, during which time his country is overrun and his followers are scattered. Iuz fears you.
                if ((!GetGlobalFlag(884) && GetGlobalVar(898) == 1))
                {
                    // asherah alive and asherah saved from iuz
                    GameSystems.Movies.MovieQueueAdd(536);
                }
                // You saved Asherah from her dismal fate at the hands of Iuz. In the years to come, she becomes one of the greatest philanthropists the city has ever known.
                else if ((!GetGlobalFlag(884) && GetGlobalVar(898) == 2))
                {
                    // asherah alive and asherah enslaved by iuz
                    GameSystems.Movies.MovieQueueAdd(537);
                }
                // You seemingly saved Asherah from her dismal fate at the hands of Iuz, only to find that Iuz's subterfuge had brought about her ruination. She now serves her new master in misery, while Iuz ignores you.
                else if ((GetGlobalFlag(884) && GetGlobalVar(898) == 3))
                {
                    // asherah dead and asherah killed by iuz
                    GameSystems.Movies.MovieQueueAdd(538);
                }

            }
            // You seemingly saved Asherah from her dismal fate at the hands of Iuz, only to find that Iuz, in his subterfuge, had killed her anyway and exacted his revenge. He eyes you next.
            else if (GetGlobalFlag(326))
            {
                // destroyed orb
                GameSystems.Movies.MovieQueueAdd(211);
            }

            // Iuz is weakened by the Orb's destruction. He now ranks you among his personal enemies and plans a revenge suitable for this annoyance.
            if ((GetGlobalFlag(884) && GetGlobalVar(898) == 4))
            {
                // asherah dead and you ran away from iuz
                GameSystems.Movies.MovieQueueAdd(539);
            }

            // You ran away from Iuz. In your wake, Iuz visited Asherah and had his revenge. She is dead.
            if ((GetQuestState(102) == QuestState.Completed && GetGlobalVar(695) == 4))
            {
                // demons and demigods quest completed and cuthbert's 4 avatars dead
                GameSystems.Movies.MovieQueueAdd(535);
                // In an almost unbelievable turn of events, you have managed to destroy St. Cuthbert. However, he is not killed but has immediately returned to his Bastion of Law, the Basilica of Saint Cuthbert. It will take him years to recover his powers, during which time his church is weakened and his followers are lessened. St. Cuthbert fears you.
                if ((!GetGlobalFlag(884) && GetGlobalVar(898) == 5))
                {
                    // asherah alive and asherah saved from cuthbert
                    GameSystems.Movies.MovieQueueAdd(544);
                }
                // You delivered Asherah from her grim fate at the hands of St. Cuthbert. In the years to come, she becomes one of the most cutthroat financial provocateurs the city has ever known.
                else if ((!GetGlobalFlag(884) && GetGlobalVar(898) == 6))
                {
                    // asherah alive and asherah enslaved by cuthbert
                    GameSystems.Movies.MovieQueueAdd(545);
                }
                // You seemingly saved Asherah from her grim fate at the hands of St. Cuthbert, only to find that Cuthbert's subterfuge had brought about her ruination. She now serves her new master in misery, while Cuthbert ignores you.
                else if ((GetGlobalFlag(884) && GetGlobalVar(898) == 7))
                {
                    // asherah dead and asherah killed by cuthbert
                    GameSystems.Movies.MovieQueueAdd(546);
                }

            }

            // You seemingly saved Asherah from her grim fate at the hands of St. Cuthbert, only to find that Cuthbert, in his subterfuge, had killed her anyway and exacted his revenge. He eyes you next.
            if ((GetGlobalFlag(884) && GetGlobalVar(898) == 8))
            {
                // asherah dead and you ran away from cuthbert
                GameSystems.Movies.MovieQueueAdd(547);
            }

            // You ran away from St. Cuthbert. In your wake, Cuthbert visited Asherah and had his revenge. She is dead.
            if ((GetQuestState(102) == QuestState.Completed && GetGlobalVar(749) == 4))
            {
                // demons and demigods quest completed and alzoll and company dead
                GameSystems.Movies.MovieQueueAdd(534);
                // In an incredible turn of events, you have managed to destroy the balor Alzoll and its cohorts Errtu, Ter-Soth, and Wendonai. The demon lords, klurichirs, and myrmyxicus of the Abyss now fear you as well.
                if ((!GetGlobalFlag(884) && GetGlobalVar(898) == 9))
                {
                    // asherah alive and asherah saved from alzoll
                    GameSystems.Movies.MovieQueueAdd(540);
                }
                // You saved Asherah from her dire fate at the hands of Alzoll. In the years to come, she goes on to prosper even more greatly, to the bane of some and the boon of others.
                else if ((!GetGlobalFlag(884) && GetGlobalVar(898) == 10))
                {
                    // asherah alive and asherah enslaved by alzoll
                    GameSystems.Movies.MovieQueueAdd(541);
                }
                // You seemingly saved Asherah from her dire fate at the hands of Alzoll, only to find that his subterfuge had brought about her ruination. She now serves her new master in misery, while Alzoll mocks you from The Abyss.
                else if ((GetGlobalFlag(884) && GetGlobalVar(898) == 11))
                {
                    // asherah dead and asherah killed by alzoll
                    GameSystems.Movies.MovieQueueAdd(542);
                }

            }

            // You seemingly saved Asherah from her dire fate at the hands of Alzoll, only to find that in his subterfuge, he had killed her anyway and exacted his revenge. Alzoll eyes you next.
            if ((GetGlobalFlag(884) && GetGlobalVar(898) == 12))
            {
                // asherah dead and you ran away from alzoll
                GameSystems.Movies.MovieQueueAdd(543);
            }

            // You ran away from Alzoll. In your wake, Alzoll visited Asherah and had his revenge. She is dead.
            // VERBOBONC & ASSOCIATES
            if ((!GetGlobalFlag(935) && !GetGlobalFlag(992) && !GetGlobalFlag(971) && GetQuestState(74) == QuestState.Completed && GetQuestState(78) == QuestState.Completed && GetQuestState(97) == QuestState.Completed))
            {
                // wilfrick alive and elysia alive and frozen assets quest completed and slave traders quest completed and war of the golden skull defender quest completed
                GameSystems.Movies.MovieQueueAdd(503);
            }

            // Lord Viscount Wilfrick is hailed as protector of Verbobonc for thwarting the plans of its worst enemies; a year later, a downturn in the economy prompts his scheming daughter Elysia to gain support and force him to abdicate the seat of power.
            if ((GetGlobalFlag(995) && GetQuestState(74) == QuestState.Completed))
            {
                // white dragon dead and frozen assets quest completed
                GameSystems.Movies.MovieQueueAdd(504);
            }
            // The death of the ice dragon is eventually discovered by other dragons, and in their fury over the slaughter, dragon attacks quickly become a common threat when traveling across the countryside.
            else if ((!GetGlobalFlag(995) && GetQuestState(74) != QuestState.Completed))
            {
                // white dragon alive and frozen assets quest not completed
                GameSystems.Movies.MovieQueueAdd(505);
            }

            // Many Verbobonc merchants are put out of business due to mysterious cold weather circumstances, but after the area becomes a known danger, trading routes are reworked and the threat becomes no more.
            if ((GetGlobalFlag(949) && GetQuestState(78) == QuestState.Completed))
            {
                // tarah dead and slave traders quest completed
                GameSystems.Movies.MovieQueueAdd(506);
            }
            // The fissures of Emridy Meadows become a popular destination for thrill-seekers wishing to tap into whatever dark static energy remains from your legendary liberation of the slave children from Tarah Ka Turah. Many never return, though the fissures themselves never divulge what secrets they swallow, nor say who waits for visitors below.
            else if ((GetQuestState(78) == QuestState.Botched))
            {
                // slave traders quest botched
                GameSystems.Movies.MovieQueueAdd(507);
            }

            // The fissures of Emridy Meadows become a popular destination for thrill-seekers wishing to tap into whatever dark static energy remains from your attack on the liberators of the slave children. Many never return, though the fissures themselves never divulge what secrets they swallow, nor say who waits for visitors below.
            if ((GetQuestState(97) == QuestState.Completed && GetGlobalFlag(501)))
            {
                // war of the golden skull defender quest completed and wakefield dead
                GameSystems.Movies.MovieQueueAdd(555);
            }
            // You defended Hommlet from the vicious attack by Wakefield and his militant band of Hextorites. Hextor mourns not the loss of his instrument Wakefield, but resigns himself to the loss of the Golden Skull and the power that would have come with it.
            else if ((GetQuestState(103) == QuestState.Completed))
            {
                // war of the golden skull attacker quest completed
                GameSystems.Movies.MovieQueueAdd(556);
            }

            // You helped Wakefield and the Hextorites pillage Hommlet and murder the townsfolk. For many years afterward, no one dares resettle the village for fear of awakening its sleeping ghosts.
            if (((GetQuestState(76) == QuestState.Completed || GetQuestState(87) == QuestState.Completed || GetQuestState(88) == QuestState.Completed) || GetGlobalFlag(989)))
            {
                // catching the cousin quest or snitching on the sb quest or narcing on the sb quest completed OR darlia dead
                GameSystems.Movies.MovieQueueAdd(528);
                // You put a halt to the Scarlet Brotherhood's dubious plots in Verbobonc. And having failed at their attempts for revenge, they regress to a third rate fencing operation within the viscounty.
                GameSystems.Movies.MovieQueueAdd(529);
            }
            // With the Scarlet Brotherhood run out of Verbobonc, crime statistics reach an all time low. But the labyrinth network of tunnels below the city causes new outbreaks of disease and plague.
            else if ((GetQuestState(77) == QuestState.Completed || GetGlobalFlag(935) || GetGlobalFlag(992)))
            {
                // removing wilfrick quest completed or wilfrick dead
                GameSystems.Movies.MovieQueueAdd(527);
            }

            // You murdered Lord Viscount Wilfrick, allowing the deceitful Lerrik and the Scarlet Brotherhood to assume control of the city. Verbobonc wallows in political corruption for years to come, and its citizens and those of the viscounty suffer profoundly.
            if ((GetQuestState(84) == QuestState.Completed && GetGlobalFlag(844)))
            {
                // contract on canon thaddeus quest completed and thaddeus dead
                GameSystems.Movies.MovieQueueAdd(530);
            }
            // You murdered Canon Thaddeus on his way from Narwell to Verbobonc. The following season, his corpse is reanimated by wights. Now the Canon roams the lonely roads by night, looking for converts to his "new religion."
            else if ((GetQuestState(86) == QuestState.Completed))
            {
                // ratting out the hextorites quest completed
                GameSystems.Movies.MovieQueueAdd(531);
            }

            // You saved Canon Thaddeus from being killed by the Hextorites. But the following season, his travel caravan is overrun by wights. Now the Canon roams the lonely roads by night, looking for converts to his "new religion."
            if ((GetQuestState(90) == QuestState.Completed && GetGlobalFlag(929)))
            {
                // gremlich quest completed and gremlich dead
                GameSystems.Movies.MovieQueueAdd(532);
            }

            // With the dreaded Gremlich banished, for now, you're left to wonder what may be in store for you and your descendants 49 years into your retirement.
            if ((GetQuestState(69) == QuestState.Completed && !GetGlobalFlag(885)))
            {
                // under attack from underground quest completed and didn't get proof with holly
                GameSystems.Movies.MovieQueueAdd(508);
            }
            // You cleared the Drow from the Gnome Tunnels beneath the city. While rumors of your actions and the purported existence of Drow persist, most view such stories as old wives' tales.
            else if ((GetQuestState(69) == QuestState.Completed && GetGlobalFlag(885)))
            {
                // under attack from underground quest completed and got proof with holly
                GameSystems.Movies.MovieQueueAdd(509);
            }

            // You cleared the Drow from the Gnome Tunnels beneath the city. Corporal Holly's testimony lends credence to this tale, and citizens are left to wonder what brought the Drow to Verbobonc in the first place.
            if ((GetGlobalFlag(996)))
            {
                // bought the CotL
                if ((GetGlobalVar(765) >= 1))
                {
                    // killed the big 3
                    if ((GetQuestState(83) == QuestState.Completed))
                    {
                        // completed fear of ghosts quest
                        GameSystems.Movies.MovieQueueAdd(512);
                    }
                    // In your lavish retirement, you outgrow the Castle of the Lords. Years later, the gnome quarter buys it out from you and gives it to the the city as a museum. It remains untroubled by spirits.
                    else if ((GetQuestState(83) != QuestState.Completed))
                    {
                        // didn't complete fear of ghosts quest
                        GameSystems.Movies.MovieQueueAdd(511);
                    }

                }
                // Your dreams remain troubled in the Castle of the Lords. Years later, the gnome quarter buys it out from you and gives it to the the city as a museum. Visitors report strange sights and sounds.
                else if ((GetGlobalVar(765) == 0))
                {
                    // didn't kill the big 3
                    GameSystems.Movies.MovieQueueAdd(513);
                }

                // In your lavish retirement, you outgrow the Castle of the Lords. Years later, the gnome quarter buys it out from you and gives it to the the city as a museum.
                if ((GetGlobalVar(886) == 1))
                {
                    // met orrengaard
                    GameSystems.Movies.MovieQueueAdd(510);
                }

            }
            // The mechanism controlling the panel that leads to the Rabbit Hole in the Castle of the Lords is mysteriously broken, preventing any further visits to that intangible locale. Custodians of the castle insist no such panel ever existed, and the gutting of the stairwell reveals no hidden chamber. But the faint sounds of gnomish merrymaking can sometimes be heard by visitors to the castle basement in the wee hours of the night.
            else if ((!GetGlobalFlag(996)))
            {
                // didn't buy the CotL
                GameSystems.Movies.MovieQueueAdd(514);
            }

            // After standing vacant for years, the gnome quarter buys out the Castle of the Lords from the Silver Consortium and gives it to the the city as a museum. It's repossessed after the gnomes fail to make timely payments.
            if ((GetQuestState(79) == QuestState.Completed && GetGlobalFlag(956)))
            {
                // wanted gladstone quest completed and gladstone dead
                GameSystems.Movies.MovieQueueAdd(518);
            }
            // With Gunter Gladstone dead by your hand, the Viscounty is spared numerous future robberies, rapes, and torturings.
            else if ((GetQuestState(79) == QuestState.Completed && !GetGlobalFlag(956)))
            {
                // wanted gladstone quest completed and gladstone alive
                GameSystems.Movies.MovieQueueAdd(519);
            }
            // Gunter Gladstone is incarcerated. He remains so for 30 days before mysteriously dying in his sleep.
            else if (((GetQuestState(79) == QuestState.Mentioned || GetQuestState(79) == QuestState.Accepted) && !GetGlobalFlag(956)))
            {
                // wanted gladstone quest mentioned or accepted and gladstone alive
                GameSystems.Movies.MovieQueueAdd(520);
            }

            // Gunter Gladstone flees to Dyvers, where he soon joins with an organization devoted to supplying the burgeoning demands of the child slavery underground.
            if ((GetQuestState(80) == QuestState.Completed && GetGlobalFlag(957)))
            {
                // wanted commonworth quest completed and commonworth dead
                GameSystems.Movies.MovieQueueAdd(521);
            }
            // With Kendrew Commonworth dead by your hand, the wild places of the Viscounty are that much safer for good folk, and children's tales of terror by night are reduced to legend.
            else if ((GetQuestState(80) == QuestState.Completed && !GetGlobalFlag(957)))
            {
                // wanted commonworth quest completed and commonworth alive
                GameSystems.Movies.MovieQueueAdd(522);
            }
            // Kendrew Commonworth is incarcerated. He is eventually killed by an unlikely team of jailers and prisoners when he breaks out of his cell and sets out to kill them all rather than flee.
            else if (((GetQuestState(80) == QuestState.Mentioned || GetQuestState(80) == QuestState.Accepted) && !GetGlobalFlag(957)))
            {
                // wanted commonworth quest mentioned or accepted and commonworth alive
                GameSystems.Movies.MovieQueueAdd(523);
            }

            // Kendrew Commonworth remains hidden in the wilds. He occasionally preys upon a remote farmstead or woodsman's cottage to sustain his grotesque needs.
            if ((GetQuestState(81) == QuestState.Completed && GetGlobalFlag(958)))
            {
                // wanted corpus quest completed and corpus dead
                GameSystems.Movies.MovieQueueAdd(524);
            }
            // With Quintus Corpus dead by your hand, a new 'visionary' of extreme redemption arises and begins to administer his brand of purification, morbidly inspired by Corpus' memory and deeds.
            else if ((GetQuestState(81) == QuestState.Completed && !GetGlobalFlag(958)))
            {
                // wanted corpus quest completed and corpus alive
                GameSystems.Movies.MovieQueueAdd(525);
            }
            // Quintus Corpus is incarcerated. He becomes something of a messiah to many of his fellow inmates, preaching incessantly about the need to purge the evils from good men and women.
            else if (((GetQuestState(81) == QuestState.Mentioned || GetQuestState(81) == QuestState.Accepted) && !GetGlobalFlag(958)))
            {
                // wanted corpus quest mentioned or accepted and corpus alive
                GameSystems.Movies.MovieQueueAdd(526);
            }

            // Quintus Corpus returns to Celene in a vain attempt to formally reclaim his role of 'paladin.' He is captured and put to death in the town square the following morning.
            if ((GetQuestState(109) == QuestState.Completed && GetGlobalVar(542) == 1 && !GetGlobalFlag(540)))
            {
                // what lies beneath quest completed and boroquin guilty and boroquin alive
                GameSystems.Movies.MovieQueueAdd(515);
            }
            // Boroquin finishes his days incarcerated in the Verbobonc lockup. He dies late one night, raving about two children at his bedside though jailer reports insist he was alone.
            else if ((GetQuestState(109) == QuestState.Completed && GetGlobalVar(542) == 2 && !GetGlobalFlag(809)))
            {
                // what lies beneath quest completed and panathaes guilty and panathaes alive
                GameSystems.Movies.MovieQueueAdd(517);
            }
            // Panathaes is readmitted to the Verbobonc asylum, where he regales his fellow inmates daily with tales of his psychic bond with the spirit of Abel Mol. He doesn't share the fact that every night he hears giggling voices from the drainage grate in his cell beckoning him to come back down.
            else if ((GetQuestState(109) == QuestState.Completed && GetGlobalVar(542) == 3 && !GetGlobalFlag(539)))
            {
                // what lies beneath quest completed and rakham guilty and rakham alive
                GameSystems.Movies.MovieQueueAdd(516);
            }

            // Rakham is transferred to a labor camp, where he breaks rocks for seven years before being granted parole. He retires to seclusion and takes work as a night watchman.
            if ((GetGlobalFlag(562) && GetGlobalFlag(564)))
            {
                // angra mainyu dead and phylactery destroyed
                GameSystems.Movies.MovieQueueAdd(553);
            }
            // You defeated the lich Angra Mainyu at Hickory Branch. With its phylactery destroyed, a terrible force of evil has been removed from the region.
            else if ((GetGlobalFlag(562) && !GetGlobalFlag(564)))
            {
                // angra mainyu dead and phylactery not destroyed
                GameSystems.Movies.MovieQueueAdd(554);
            }

            // You defeated the lich Angra Mainyu at Hickory Branch. But with its phylactery intact, you're left to wonder when the monster will return.
            if ((GetQuestState(62) == QuestState.Completed && GetGlobalFlag(560) && GetGlobalFlag(561) && GetGlobalFlag(562)))
            {
                // hickory branch quest completed and hungous dead and noostoron dead and angra mainyu dead
                GameSystems.Movies.MovieQueueAdd(551);
                // Having routed the orc invasion force, the hickory branch area becomes a safe rest stop area for travelers once again. It's discovered to be a location rich in natural ores, presenting opportunity for a new quarry.
                if ((GetGlobalFlag(572)))
                {
                    // you delayed finishing hb
                    GameSystems.Movies.MovieQueueAdd(552);
                }

            }

            // However, the start-stop rhythm of your assault on Hickory has allowed its former inhabitants ample time for enacting their contingency plan. Cursed are you now; for the fruit of your loins shall all be Orcs, burning with militaristic ambition.
            if ((GetQuestState(62) == QuestState.Completed && !GetGlobalFlag(826)))
            {
                // hickory branch quest completed and bethany alive
                GameSystems.Movies.MovieQueueAdd(549);
                // You assisted Captain Bethany in validating her findings and discrediting Captain Asaph, leading to her promotion to head of the Regional Patrol. In the years that follow, the countryside becomes safer and better policed, but the institution itself is more greatly influenced by political maneuvering. Bethany, meanwhile, profits greatly.
                if ((!GetGlobalFlag(962) && !GetGlobalFlag(419)))
                {
                    // holly alive and absalom alive
                    GameSystems.Movies.MovieQueueAdd(550);
                }

            }

            // Corporal Holly is soon promoted to Captain of the Watch in Verbobonc, leapfrogging a number of lieutenants and sergeants. Due to her new found power and influence, no one dares suggest that Captain Bethany had anything to do with her friend's ascension, though former Captain Absalom, demoted to sergeant, has his suspicions.
            if ((GetQuestState(96) == QuestState.Completed && GetGlobalFlag(506)))
            {
                // of castles and quarries quest completed and battlehammer dead
                GameSystems.Movies.MovieQueueAdd(557);
            }

            // Following your sacking of the dwarves at the Hommlet quarry, dwarven emissaries from the Lortmil Mountains appear in Verbobonc to account for King Battlehammer and his men, now revealed to be a rogue contingent of miners expelled from the Lortmils for disavowing their true king.
            if ((GetQuestState(95) == QuestState.Completed && GetGlobalFlag(249)))
            {
                // season of the witch quest completed and MR witch dead
                GameSystems.Movies.MovieQueueAdd(548);
            }

            // The moathouse, having served and deposed three different masters in the course of its life, is slowly reclaimed by its final proprietor, mother nature.
            // PRINCE THROMMEL
            if (GetGlobalFlag(150))
            {
                // thrommel dead
                GameSystems.Movies.MovieQueueAdd(215);
            }
            // With the death of Prince Thrommel, divined by the priests of Furyondy, that kingdom suffers greatly in the wars to come.
            else if ((GetGlobalFlag(151)) || (GetGlobalFlag(152)) || (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031))))
            {
                // thrommel freed and out OR thrommel reward scheduled OR thrommel in party
                GameSystems.Movies.MovieQueueAdd(214);
            }
            else
            {
                // Prince Thrommel is rescued and marries Princess Jolene of Veluna. The kingdoms of Furyondy and Veluna unite, and you are made knights of the kingdom.
                GameSystems.Movies.MovieQueueAdd(216);
            }

            // Left inside the temple dungeons, Prince Thrommel eventually becomes a vampire and strikes terror into the local populace. His sword Fragarach is lost in the mists of time...
            // NULB & ASSOCIATES
            if (GetGlobalFlag(187))
            {
                // zuggtmoy gave you treasure
                GameSystems.Movies.MovieQueueAdd(247);
            }
            // As Zuggtmoy was spared, so was Nulb. Its wicked inhabitants continued their evil ways unchecked.
            else if ((GetGlobalFlag(189) || GetGlobalFlag(188)))
            {
                // zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(248);
            }

            // With the destruction of the Temple of Elemental Evil, the fall of the village of Nulb came soon thereafter.
            if ((GetGlobalFlag(97) && GetGlobalFlag(329)))
            {
                // tolub dead and grud dead
                GameSystems.Movies.MovieQueueAdd(223);
            }

            // With the deaths of Tolub and Grud Squinteye, the river pirates lose direction and eventually disband.
            if ((GetGlobalFlag(90) && GetGlobalFlag(324) && !GetGlobalFlag(365)))
            {
                // gay bertram free AND gay bertram met AND gay betram alive
                GameSystems.Movies.MovieQueueAdd(218);
            }

            // You and Bertram are married in a small ceremony, and he opens a dentistry office in Verbobonc. You live happily ever after.
            if ((GetGlobalFlag(83) && !GetGlobalFlag(330)))
            {
                // exclusive with jenelda and jenelda alive
                GameSystems.Movies.MovieQueueAdd(231);
            }

            // After your beautiful wedding, you and Jenelda travel throughout Oerth. You were last seen in the east in the Grandwood Forest.
            // HOMMLET & ASSOCIATES
            if ((!GetGlobalFlag(299) && !GetGlobalFlag(337) && !GetGlobalFlag(336) && (GetGlobalFlag(189) || GetGlobalFlag(188)) && GetQuestState(103) != QuestState.Completed))
            {
                // terjon alive and jaroo alive and burne and rufus alive and zuggtmoy dead or banished and didn't complete wotgs attacker
                GameSystems.Movies.MovieQueueAdd(243);
            }
            // In the quiet years that followed the fall of the temple of elemental evil, Hommlet grows into a bustling and prosperous town under the guidance of its elder citizens.
            else if (((GetGlobalFlag(299) || GetGlobalFlag(337) || GetGlobalFlag(336)) && (GetGlobalFlag(189) || GetGlobalFlag(188)) && GetQuestState(103) != QuestState.Completed))
            {
                // terjon dead or jaroo dead or burne or rufus dead and zuggtmoy dead or banished and didn't complete wotgs attacker
                GameSystems.Movies.MovieQueueAdd(244);
            }

            // As is often the case in sleepy, little villages, the excitement of the destruction of the temple soon died down, and life in Hommlet continued unaffected by such portentous events.
            if (GetQuestState(65) == QuestState.Completed)
            {
                // completed hero's prize quest
                GameSystems.Movies.MovieQueueAdd(502);
            }

            // You have risen to the challenge of the Arena of Heroes, and emerged victorious, a champion of the arena. Many years would pass before a new group of adventurers, seeking to stamp out an emerging cult of Tharizdun, comes to entertain the ethereal crowd of the arena once more.
            if ((GetQuestState(73) == QuestState.Completed && GetGlobalFlag(976)))
            {
                // completed welkwood bog quest and mathel dead
                GameSystems.Movies.MovieQueueAdd(501);
            }

            // Mathel, the would-be lich of Welkwood Bog, is no more. The people of Hommlet pray for his redemption; Beory takes heed, and reincarnates his soul as a Swamp Lotus.
            if ((GetQuestState(15) != QuestState.Completed && !GetGlobalFlag(336) && GetQuestState(103) != QuestState.Completed))
            {
                // didn't complete agent revealed quest and burne or rufus dead and didn't complete wotgs attacker
                GameSystems.Movies.MovieQueueAdd(242);
            }

            // The tower of Burne and Rufus remained under construction for many years, plagued by mysterious delays and inauspicious accidents.
            if ((GetQuestState(20) == QuestState.Completed && GetQuestState(103) != QuestState.Completed))
            {
                // rescued paida and didn't complete wotgs attacker
                GameSystems.Movies.MovieQueueAdd(250);
            }

            // In returning Paida to Valden, you have overcome their history of misery and pain and allowed true love to prosper.
            if ((GetGlobalFlag(61) && GetQuestState(103) != QuestState.Completed))
            {
                // furnok retired and didn't complete wotgs attacker
                GameSystems.Movies.MovieQueueAdd(222);
            }

            // Furnok of Ferd uses his wealth to become a well-known and well-respected citizen of Verbobonc.
            if ((GetQuestState(12) == QuestState.Completed && !GetGlobalFlag(364) && GetQuestState(103) != QuestState.Completed))
            {
                // completed one bride for one player quest AND fruella alive and didn't complete wotgs attacker
                GameSystems.Movies.MovieQueueAdd(219);
            }

            // As you promised, you and Fruella are married in the church of St. Cuthbert. She tames your wild, adventuring nature, and you settle down as a farmer in Hommlet and have many, many, many children.
            if ((GetGlobalFlag(68) && !GetGlobalFlag(331) && GetQuestState(103) != QuestState.Completed))
            {
                // married laszlo and laszlo alive and didn't complete wotgs attacker
                GameSystems.Movies.MovieQueueAdd(230);
            }

            // You marry Laszlo. You settle down with him, live the life of a herdsman's wife and have many children.
            if ((GetGlobalFlag(46) && !GetGlobalFlag(196) && !GetGlobalFlag(318) && GetQuestState(103) != QuestState.Completed))
            {
                // married meleny and meleny not dead or mistreated and didn't complete wotgs attacker
                if (GetGlobalFlag(332))
                {
                    // killed an ochre jelly
                    GameSystems.Movies.MovieQueueAdd(232);
                }
                else
                {
                    // You marry Meleny and take her on all sorts of grand adventures.
                    GameSystems.Movies.MovieQueueAdd(233);
                }

            }

            // You marry Meleny and take her on all sorts of grand adventures, until an ochre jelly eats her.
            if ((GetQuestState(6) == QuestState.Completed && !GetGlobalFlag(333) && !GetGlobalFlag(334) && GetQuestState(103) != QuestState.Completed))
            {
                // completed cupid's arrow quest and filliken alive and mathilde alive and didn't complete wotgs attacker
                if (GetGlobalFlag(332))
                {
                    // killed an ochre jelly
                    GameSystems.Movies.MovieQueueAdd(234);
                }
                else
                {
                    // You convince Filliken and Mathilde to marry, and they enjoy many happy years together in Hommlet.
                    GameSystems.Movies.MovieQueueAdd(235);
                }

            }

            // You convince Filliken and Mathilde to marry, and they enjoy many happy years together in Hommlet, until they are eaten by ochre jellies.
            // JOINABLE NPCS
            if (GetGlobalVar(29) >= 10)
            {
                // 10 or more NPCs killed while in party
                GameSystems.Movies.MovieQueueAdd(217);
            }

            // You abuse your faithful followers, many of whom lose their lives in the dungeons beneath the temple.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8060)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // morgan in party and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(263);
            }

            // Morgan survives the encounter with Zuggtmoy and thanks you most profusely for saving his life. He returns to his life as a pirate, and eventually becomes a feared captain of his own ship on the high seas.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8023)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                // oohlgrist in party and zuggtmoy dead or banished or surrendered
                GameSystems.Movies.MovieQueueAdd(265);
            }

            // Like the coward he is, Oohlgrist sneaks off soon after your encounter with Zuggtmoy. He is never heard from again.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8034)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                // scorrp in party and zuggtmoy dead or banished or surrendered
                GameSystems.Movies.MovieQueueAdd(266);
            }

            // To show his gratitude, Scorpp swears loyalty to you and is evermore your faithful servant.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8061)) && triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8029)) && triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8030)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                // ted, ed, and ed in party and zuggtmoy dead or banished or surrendered
                GameSystems.Movies.MovieQueueAdd(264);
            }

            // The stars flicker, the gods pause, the winds stop. Throughout the world there is silence, as if the world has stopped to catch its breath. For you have survived the game with Ted, Ed and Ed. Congratulations.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // otis in party and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(259);
            }

            // Otis is very pleased with the results of your adventures together. His reports to Furyondy and Veluna gain you even more favor in those countries.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8021)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // y'dey in party and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(260);
            }

            // Y'dey is overjoyed to have adventured with you. Her reports to the Archcleric of Veluna feature you most prominently, and the Archcleric is impressed with your results.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8062)) && (GetGlobalFlag(189) || GetGlobalFlag(188) || GetGlobalFlag(185))))
            {
                // zaxis in party and zuggtmoy dead or banished or surrendered
                GameSystems.Movies.MovieQueueAdd(267);
            }

            // Shortly after your Temple adventure, Zaxis leaves your party to continue looking for his sister. He finally finishes his song about your grand adventure, although he never does find a word that rhymes with Zuggtmoy.
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8730)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // ronald in party and zuggtmoy dead or banished
                GameSystems.Movies.MovieQueueAdd(500);
            }

            // At the conclusion of your adventures, Ronald returns to his position at the Church of St. Cuthbert in Hommlet. He serves as a much higher ranking official than when he left, eventually moving to and joining a larger church in Mitrik.
            // VIGNETTE RESOLUTION
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031)) && (GetGlobalFlag(189) || GetGlobalFlag(188))))
            {
                // thrommel in party and zuggtmoy dead or banished
                if ((PartyAlignment == Alignment.LAWFUL_EVIL))
                {
                    GameSystems.Movies.MovieQueueAdd(262);
                    // Prince Thrommel is most useful in helping overcome Zuggtmoy.  He is most surprised when you kill him afterwards for his sword.
                    GameSystems.Movies.MovieQueueAdd(255);
                }
                else
                {
                    // With the recovery of the artifact sword Fragarach and for safely conveying it a representative of the temple of Hextor, you have advanced the cause of Hextor tremendously. You are granted of favor the god Hextor.
                    GameSystems.Movies.MovieQueueAdd(261);
                }

            }
            // Prince Thrommel is most useful in helping overcome Zuggtmoy.
            else if ((PartyAlignment == Alignment.LAWFUL_EVIL))
            {
                if ((GetQuestState(28) == QuestState.Completed))
                {
                    // completed LE vignette quest
                    GameSystems.Movies.MovieQueueAdd(255);
                }
                else
                {
                    // With the recovery of the artifact sword Fragarach and for safely conveying it a representative of the temple of Hextor, you have advanced the cause of Hextor tremendously. You are granted of favor the god Hextor.
                    GameSystems.Movies.MovieQueueAdd(256);
                }

            }

            // For failing to deliver the sword Fragarach to the representative of Hextor, you have earned the enmity of the god. The shadow of his curse follows you for years to come.
            return;
        }
        // Function: set_join_slides( npc, pc )
        // Author  : Tom Decker
        // Returns : nada
        // Purpose : queues up the end slides if you join the temple

        public static void set_join_slides(GameObjectBody attachee, GameObjectBody triggerer)
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
        // Function: should_heal_hp_on( obj )
        // Author  : Tom Decker
        // Returns : 1 if character is not at full health, else 0
        // Purpose : to heal only characters that need it

        public static int should_heal_hp_on(GameObjectBody obj)
        {
            var cur = obj.GetStat(Stat.hp_current);
            var max = obj.GetStat(Stat.hp_max);
            if ((!(cur == max)) && (obj.GetStat(Stat.hp_current) >= -9))
            {
                return 1;
            }

            return 0;
        }
        // Function: should_heal_disease_on( obj )
        // Author  : Tom Decker
        // Returns : 1 if obj is diseased, else 0
        // Purpose : to heal only characters that need it

        public static int should_heal_disease_on(GameObjectBody obj)
        {
            // check if obj is diseased
            if ((obj.GetStat(Stat.hp_current) >= -9))
            {
                return 1;
            }

            return 0;
        }
        // Function: should_heal_poison_on( obj )
        // Author  : Tom Decker
        // Returns : 1 if obj is poisoned, else 0
        // Purpose : to heal only characters that need it

        public static int should_heal_poison_on(GameObjectBody obj)
        {
            // check if obj has poison on them
            if ((obj.GetStat(Stat.hp_current) >= -9))
            {
                return 1;
            }

            return 0;
        }
        // Function: should_resurrect_on( obj )
        // Author  : Tom Decker
        // Returns : 1 if obj is dead, else 0
        // Purpose : to heal only characters that need it

        public static int should_resurrect_on(GameObjectBody obj)
        {
            if ((obj.GetStat(Stat.hp_current) <= -10))
            {
                return 1;
            }

            return 0;
        }
        public static bool zap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var damage_dice = Dice.Parse("2d4");
            AttachParticles("sp-Shocking Grasp", triggerer);
            if (triggerer.ReflexSaveAndDamage(null, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Electricity, D20AttackPower.UNSPECIFIED))
            {
                // saving throw successful
                triggerer.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                // saving throw unsuccessful
                triggerer.FloatMesFileLine("mes/spell.mes", 30002);
            }

            return RunDefault;
        }
        // TESTING UTILITIES

        public static void flash_signal(int on_whom, int effect_type)
        {
            var party_size = GameSystems.Party.PartySize;
            if (effect_type == -1)
            {
                effect_type = on_whom;
            }

            while (on_whom > party_size)
            {
                on_whom = on_whom - party_size;
            }

            SelectedPartyLeader.FloatMesFileLine("mes/test.mes", effect_type);
            if (effect_type == 0)
            {
                AttachParticles("sp-summon monster I", GameSystems.Party.GetPartyGroupMemberN(on_whom));
            }
            else if (effect_type == 1)
            {
                AttachParticles("Orb-Summon-Air-Elemental", GameSystems.Party.GetPartyGroupMemberN(on_whom));
            }
            else if (effect_type == 2)
            {
                AttachParticles("Orb-Summon-Fire-Elemental", GameSystems.Party.GetPartyGroupMemberN(on_whom));
            }
            else if (effect_type == 3)
            {
                AttachParticles("sp-Righteous Might", GameSystems.Party.GetPartyGroupMemberN(on_whom));
            }
            else if (effect_type == 4)
            {
                AttachParticles("sp-Dimension Door", GameSystems.Party.GetPartyGroupMemberN(on_whom));
            }

        }

        public static void restup()
        {
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                pc.PendingSpellsToMemorized(); // Memorizes Spells
                pc.SetInt(obj_f.hp_damage, 0); // Removes all damage (doesn't work for companions?)
                if (pc.GetStat(Stat.level_bard) >= 1)
                {
                    pc.ResetCastSpells(Stat.level_bard);
                }

                if (pc.GetStat(Stat.level_sorcerer) >= 1)
                {
                    pc.ResetCastSpells(Stat.level_sorcerer);
                }

            }

        }
        public static void rsetup()
        {
            // def rsetup(): # I commonly make this typo :P - SA
            restup();
        }
        public static void reward_pc(int proto_id, GameObjectBody pc, GameObjectBody npc)
        {
            var aaa = GameSystems.MapObject.CreateObject(proto_id, SelectedPartyLeader.GetLocation());
            if (pc.GetItem(aaa))
            {
                return;
            }

            foreach (var obj in GameSystems.Party.PartyMembers)
            {
                if (obj.GetItem(aaa))
                {
                    return;
                }

            }

            return;
        }
        
        // mode select: 0 - PCs only; 1 - Controllable PCs and NPCs (summons excluded); 2- group_list
        public static GameObjectBody party_closest(GameObjectBody attachee, bool conscious_only = true, int mode_select = 1, bool exclude_warded = false, bool exclude_charmed = true, bool exclude_spiritual_weapon = true)
        {
            GameObjectBody closest_one = null;
            foreach (var obj in SelectedPartyLeader.GetPartyMembers())
            {
                if (((!obj.IsUnconscious()) || !conscious_only) && (obj.type == ObjectType.pc || mode_select >= 1) && (!obj.D20Query(D20DispatcherKey.QUE_ExperienceExempt) || mode_select == 2 || obj.type == ObjectType.pc))
                {
                    if (closest_one == null)
                    {
                        if (((!obj.HasCondition(SpellEffects.SpellOtilukesResilientSphere) && !obj.HasCondition(SpellEffects.SpellMeldIntoStone)) || !exclude_warded) && (!obj.D20Query(D20DispatcherKey.QUE_Critter_Is_Charmed) || !exclude_charmed) && ((!((new[] { 14370, 14604, 14621, 14629 }).Contains(obj.GetNameId()))) || !exclude_spiritual_weapon))
                        {
                            closest_one = obj;
                        }

                    }
                    else if (attachee.DistanceTo(obj) <= attachee.DistanceTo(closest_one))
                    {
                        if (((!obj.HasCondition(SpellEffects.SpellOtilukesResilientSphere) && !obj.HasCondition(SpellEffects.SpellMeldIntoStone)) || !exclude_warded) && (!obj.D20Query(D20DispatcherKey.QUE_Critter_Is_Charmed) || !exclude_charmed) && ((!((new[] { 14370, 14604, 14621, 14629 }).Contains(obj.GetNameId()))) || !exclude_spiritual_weapon))
                        {
                            closest_one = obj;
                        }

                    }

                }

            }

            return closest_one;
        }
        // meant for NPCs, to see if they should be capable of manoeuvring

        public static int willing_and_capable(GameObjectBody attachee)
        {
            // def willing_and_capable( attachee ): # meant for NPCs, to see if they should be capable of manoeuvring
            if (attachee.GetLeader() == null && !attachee.IsUnconscious() && !attachee.D20Query(D20DispatcherKey.QUE_Is_BreakFree_Possible) && !attachee.D20Query(D20DispatcherKey.QUE_Prone) && !attachee.D20Query(D20DispatcherKey.QUE_Helpless) && !attachee.HasCondition(SpellEffects.SpellOtilukesResilientSphere))
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
        public static GameObjectBody GetDelegatePc(GameObjectBody attachee, int distt = 20, bool requireLos = false)
        {
            GameObjectBody delegatePc = null;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.type == ObjectType.pc && !pc.IsUnconscious() && !pc.D20Query(D20DispatcherKey.QUE_Prone) && attachee.DistanceTo(pc) <= distt && (requireLos || attachee.HasLineOfSight(pc))))
                {
                    delegatePc = pc;
                }

            }

            return delegatePc;
        }

    }
}
