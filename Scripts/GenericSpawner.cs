
using System;
using System.Collections.Generic;
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

namespace Scripts
{
    [ObjectScript(334)]
    public class GenericSpawner : BaseObjectScript
    {
        // Contained in this script:
        // -Temple exterior: Flags Burnt Farm, Ruined Temple House, Temple Tower visited
        // -Temple lvl2: Fix for giving Countess Tillahi instructions if you've been taken blindfolded into the Temple

        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001 || attachee.GetMap() == 5002 || attachee.GetMap() == 5051 || attachee.GetMap() == 5062 || attachee.GetMap() == 5068 || attachee.GetMap() == 5069 || attachee.GetMap() == 5091 || attachee.GetMap() == 5112 || attachee.GetMap() == 5094 || attachee.GetMap() == 5113 || attachee.GetMap() == 5108 || attachee.GetMap() == 5093 || attachee.GetMap() == 5095 || attachee.GetMap() == 5111 || attachee.GetMap() == 5121 || attachee.GetMap() == 5132))
            {
                // All Worldmap Locations
                // 5001 = hommlet exterior - game area 1
                // 5002 = moathouse exterior - game area 2
                // 5051 = nulb exterior - game area 3
                // 5062 = temple entrance - game area 4
                // 5068 = imeryds run - game area 6
                // 5069 = decklo grove - game area 10
                // 5091 = moathouse exit tunnel - game area 8
                // 5112 = temple deserted farm - game area 11
                // 5094 = emridy meadows - game area 5
                // 5113 = ogre cave exterior - game area 12
                // 5108 = quarry exterior - game area 16
                // 5093 = welkwood bog exterior - game area 7
                // 5095 = hickory branch exterior - game area 9
                // 5121 = verbobonc exterior - game area 14
                // 5132 = verbobonc cave exit - game area 15
                if ((attachee.GetMap() == 5001))
                {
                    // Hommlet Exterior
                    hommlet_movie_check();
                }

                if ((attachee.GetMap() == 5002))
                {
                    // Moathouse Exterior
                    moathouse_respawn_check();
                    moathouse_movie_check();
                }

                if ((attachee.GetMap() == 5051))
                {
                    // Nulb Exterior
                    nulb_movie_check();
                }

                if ((attachee.GetMap() == 5062))
                {
                    // Temple Entrance
                    temple_movie_check();
                }

                if ((attachee.GetMap() == 5093))
                {
                    // Welkwood Exterior
                    welkwood_movie_check();
                }

                if ((attachee.GetMap() == 5094))
                {
                    // Emridy Meadows
                    emridy_movie_check();
                }

                if ((attachee.GetMap() == 5095))
                {
                    // Hickory Branch Exterior
                    hickory_movie_check();
                }

                if ((attachee.GetMap() == 5121))
                {
                    // Verbobonc Exterior
                    verbo_tax(attachee, triggerer);
                    verbobonc_movie_check();
                }

                if ((attachee.GetMap() != 5001))
                {
                    // not Hommlet Exterior
                    hextor_wins();
                }

                if ((attachee.GetMap() != 5002))
                {
                    // not Moathouse Exterior
                    party_death_by_poison();
                    if ((attachee.GetMap() != 5091))
                    {
                        // not Moathouse Cave Exit
                        big_3_check();
                    }

                }

                if ((attachee.GetMap() != 5095))
                {
                    // not Hickory Branch
                    hickory_branch_check();
                }

                // Sitra stuff below - next, check if there's a PC possessed by the new_map script, and if not - assign it to one
                var scriptbearer_found = 0;
                foreach (var scriptbearer in GameSystems.Party.PartyMembers)
                {
                    if (scriptbearer.GetScriptId(ObjScriptEvent.NewMap) == 439)
                    {
                        scriptbearer_found = 1;
                    }

                    if (scriptbearer.GetScriptId(ObjScriptEvent.NewMap) != 439 && scriptbearer.GetStat(Stat.hp_current) > -10 && scriptbearer_found == 0 && scriptbearer.type == ObjectType.pc)
                    {
                        scriptbearer.SetScriptId(ObjScriptEvent.Dying, 439); // san_dying
                        scriptbearer.SetScriptId(ObjScriptEvent.ExitCombat, 439); // san_exit_combat
                        scriptbearer.SetScriptId(ObjScriptEvent.NewMap, 439); // san_new_map
                        scriptbearer_found = 1;
                    }

                }

            }
            else if ((attachee.GetMap() == 5067))
            {
                // Temple Level 2
                bin_thr_dun_tht();
            }
            else if ((attachee.GetMap() == 5064 || attachee.GetMap() == 5105))
            {
                // Temple Int and Level 3 Lower
                give_directions();
            }
            else if ((attachee.GetMap() == 5107))
            {
                // shop map
                money_script();
                shopmap_movie_check();
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5112))
            {
                // Temple Deserted Farm
                MakeAreaKnown(11);
            }
            else if ((attachee.GetMap() == 5091))
            {
                // Moathouse Cave Exit
                MakeAreaKnown(8);
            }
            else if ((attachee.GetMap() == 5132))
            {
                // Verbobonc Cave Exit
                MakeAreaKnown(15);
            }

            return SkipDefault;
        }
        public static void money_script()
        {
            // money script by Agetian
            // strip all money
            SetGlobalFlag(500, true);
            // added by Gaear - denotes you are playing NC
            if ((SelectedPartyLeader.GetMoney() >= 50000))
            {
                PartyLeader.AdjustMoney(-PartyLeader.GetMoney());
                // add 200-300 gold pocket money
                PartyLeader.AdjustMoney(RandomRange(20000, 30000));
            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                var lsw = pc.FindItemByProto(4036);
                var scm = pc.FindItemByProto(4045);
                // get rid of an extra longsword (4036) and scimitar(4045)
                for (var pp = 0; pp < 24; pp++)
                {
                    var invItem = pc.GetInventoryItem(pp);
                    if ((invItem.ProtoId == 4036 && invItem != lsw))
                    {
                        invItem.Destroy();
                    }

                    if ((invItem.ProtoId == 4045 && invItem != scm))
                    {
                        invItem.Destroy();
                    }

                }

                // give money per PHB (for each class)
                if ((pc.GetStat(Stat.level_barbarian) > 0))
                {
                    pc.AdjustMoney(RandomRange(4000, 16000));
                }

                if ((pc.GetStat(Stat.level_bard) > 0))
                {
                    pc.AdjustMoney(RandomRange(4000, 16000));
                }

                if ((pc.GetStat(Stat.level_cleric) > 0))
                {
                    pc.AdjustMoney(RandomRange(5000, 20000));
                }

                if ((pc.GetStat(Stat.level_druid) > 0))
                {
                    pc.AdjustMoney(RandomRange(2000, 8000));
                }

                if ((pc.GetStat(Stat.level_fighter) > 0))
                {
                    pc.AdjustMoney(RandomRange(6000, 24000));
                }

                if ((pc.GetStat(Stat.level_monk) > 0))
                {
                    pc.AdjustMoney(RandomRange(5000, 20000));
                }

                if ((pc.GetStat(Stat.level_paladin) > 0))
                {
                    pc.AdjustMoney(RandomRange(6000, 24000));
                }

                if ((pc.GetStat(Stat.level_ranger) > 0))
                {
                    pc.AdjustMoney(RandomRange(6000, 24000));
                }

                if ((pc.GetStat(Stat.level_rogue) > 0))
                {
                    pc.AdjustMoney(RandomRange(5000, 20000));
                }

                if ((pc.GetStat(Stat.level_sorcerer) > 0))
                {
                    pc.AdjustMoney(RandomRange(3000, 12000));
                }

                if ((pc.GetStat(Stat.level_wizard) > 0))
                {
                    pc.AdjustMoney(RandomRange(3000, 12000));
                }

            }

            return;
        }
        public static void bin_thr_dun_tht()
        {
            // bin_thr_dun_tht script by rufnredde - Function: Sets ggf 286 if you have been to level 2 and resets ggf 287 enter temple blindfolded if you have entered/exited normally ggv 55
            SetGlobalFlag(286, true);
            if ((GetGlobalFlag(287) && GetGlobalVar(55) >= 2 && GetGlobalFlag(286)))
            {
                SetGlobalFlag(287, false);
            }

            return;
        }
        public static void give_directions()
        {
            // give_directions script by rufnredde - Function: Checks to make sure you have entered the temple ggv 55 other than blindfolded ggf 287 and have been to level 2 ggf 286 allows you to give directions out of the temple
            if ((!GetGlobalFlag(287) && GetGlobalVar(55) <= 1 && !GetGlobalFlag(286)))
            {
                SetGlobalVar(55, 1);
            }

            if ((!GetGlobalFlag(287) && GetGlobalVar(55) <= 1 && GetGlobalFlag(286)))
            {
                SetGlobalVar(55, GetGlobalVar(55) + 1);
            }

            if ((GetGlobalFlag(287) && GetGlobalVar(55) <= 1))
            {
                SetGlobalVar(55, GetGlobalVar(55) + 1);
            }

            if ((GetGlobalFlag(287)) && ((GetQuestState(53) == QuestState.Completed || GetQuestState(53) == QuestState.Accepted) || (GetGlobalFlag(996) || GetGlobalFlag(998))))
            {
                SetGlobalVar(55, GetGlobalVar(55) + 2);
            }

            if ((GetGlobalFlag(287) && GetGlobalVar(55) >= 2))
            {
                SetGlobalFlag(287, false);
            }

            return;
        }
        public static void big_3_check()
        {
            if ((GetGlobalFlag(37)) && (GetGlobalVar(765) == 0))
            {
                SetGlobalFlag(283, true);
            }

            return;
        }
        public static void hickory_branch_check()
        {
            if ((GetGlobalVar(982) == 1 || GetGlobalVar(983) == 1 || GetGlobalVar(984) == 1 || GetGlobalVar(985) == 1))
            {
                if ((GetGlobalVar(980) != 3))
                {
                    SetGlobalVar(980, 2);
                }

                if ((GetGlobalVar(981) != 3))
                {
                    SetGlobalVar(981, 2);
                }

                if ((GetGlobalVar(982) != 3))
                {
                    SetGlobalVar(982, 2);
                }

                if ((GetGlobalVar(983) != 3))
                {
                    SetGlobalVar(983, 2);
                }

                if ((GetGlobalVar(984) != 3))
                {
                    SetGlobalVar(984, 2);
                }

            }

            return;
        }
        public static void moathouse_respawn_check()
        {
            if ((GetQuestState(106) == QuestState.Mentioned))
            {
                SetQuestState(106, QuestState.Completed);
                SetQuestState(95, QuestState.Mentioned);
                Sound(4166, 1);
            }

            return;
        }
        public static void shopmap_movie_check()
        {
            if ((!GetGlobalFlag(602)))
            {
                set_shopmap_slides();
                SetGlobalFlag(602, true);
            }

            return;
        }
        public static bool set_shopmap_slides()
        {
            GameSystems.Movies.MovieQueueAdd(400);
            GameSystems.Movies.MovieQueueAdd(401);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static void hommlet_movie_check()
        {
            if ((!GetGlobalFlag(603)))
            {
                set_hommlet_slides();
                SetGlobalFlag(603, true);
            }

            return;
        }
        public static bool set_hommlet_slides()
        {
            GameSystems.Movies.MovieQueueAdd(402);
            GameSystems.Movies.MovieQueueAdd(403);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static void welkwood_movie_check()
        {
            if ((!GetGlobalFlag(605)))
            {
                set_welkwood_slides();
                SetGlobalFlag(605, true);
            }

            return;
        }
        public static bool set_welkwood_slides()
        {
            GameSystems.Movies.MovieQueueAdd(404);
            GameSystems.Movies.MovieQueueAdd(405);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static void emridy_movie_check()
        {
            if ((!GetGlobalFlag(607)))
            {
                set_emridy_slides();
                SetGlobalFlag(607, true);
            }

            return;
        }
        public static bool set_emridy_slides()
        {
            GameSystems.Movies.MovieQueueAdd(406);
            GameSystems.Movies.MovieQueueAdd(407);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static void moathouse_movie_check()
        {
            if ((!GetGlobalFlag(604)))
            {
                set_moathouse_slides();
                SetGlobalFlag(604, true);
            }

            return;
        }
        public static bool set_moathouse_slides()
        {
            GameSystems.Movies.MovieQueueAdd(408);
            GameSystems.Movies.MovieQueueAdd(409);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static void nulb_movie_check()
        {
            if ((!GetGlobalFlag(609)))
            {
                set_nulb_slides();
                SetGlobalFlag(609, true);
            }

            return;
        }
        public static bool set_nulb_slides()
        {
            GameSystems.Movies.MovieQueueAdd(410);
            GameSystems.Movies.MovieQueueAdd(411);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static void temple_movie_check()
        {
            if ((!GetGlobalFlag(610)))
            {
                set_temple_slides();
                SetGlobalFlag(610, true);
            }

            return;
        }
        public static bool set_temple_slides()
        {
            GameSystems.Movies.MovieQueueAdd(412);
            GameSystems.Movies.MovieQueueAdd(413);
            GameSystems.Movies.MovieQueueAdd(414);
            GameSystems.Movies.MovieQueueAdd(415);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static void verbobonc_movie_check()
        {
            if ((!GetGlobalFlag(606)))
            {
                set_verbobonc_slides();
                SetGlobalFlag(606, true);
            }

            return;
        }
        public static bool set_verbobonc_slides()
        {
            GameSystems.Movies.MovieQueueAdd(416);
            GameSystems.Movies.MovieQueueAdd(417);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static void hickory_movie_check()
        {
            if ((!GetGlobalFlag(608)))
            {
                set_hickory_slides();
                SetGlobalFlag(608, true);
            }

            return;
        }
        public static bool set_hickory_slides()
        {
            GameSystems.Movies.MovieQueueAdd(418);
            GameSystems.Movies.MovieQueueAdd(419);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }
        public static int party_death_by_poison()
        {
            if ((GetGlobalVar(958) == 6))
            {
                SetGlobalVar(958, 7);
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    pc.KillWithDeathEffect();
                    AttachParticles("sp-Poison", pc);
                }

            }

            return 1;
        }
        public static void hextor_wins()
        {
            if (((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6) && GetQuestState(97) != QuestState.Completed) || (GetGlobalVar(510) == 1))
            {
                // you leave hommlet without killing all the hextorites after invasion has started or you leave Hommlet after turning down Ariakas
                SetQuestState(97, QuestState.Botched);
                PartyLeader.AddReputation(53);
                SetGlobalVar(510, 2);
                SetGlobalFlag(504, true);
            }

            return;
        }
        public static void verbo_tax(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(549) && !GetGlobalFlag(826) && !GetGlobalFlag(260)))
            {
                if ((PartyLeader.GetMoney() >= 20000))
                {
                    PartyLeader.AdjustMoney(-20000);
                    Sound(4178, 1);
                    PartyLeader.FloatMesFileLine("mes/float.mes", 4);
                }
                else
                {
                    SetGlobalVar(567, GetGlobalVar(567) + 1);
                }

            }

            SetGlobalFlag(260, false);
            return;
        }
        // EVERYTHING BELOW HAS TO DO WITH SITRA'S RANNOS AND GREMAG MOD AS FAR AS I KNOW #

        public static int council_heartbeat(GameObjectBody attachee)
        {
            var c_time = Council.council_time();
            // 1 - between 22:00 and 22:30 on council day
            // 2 - between 22:30 and 23:00
            // 3 - between 19:00 and 22:00
            // 4 - after council events ( >23:00 and beyond that day), but without ordinary council
            // 5 - ordinary council time
            // 0 - otherwise
            if (Council.traders_awol() == 1)
            {
                ScriptDaemon.set_v(435, 6);
            }

            if (c_time == 5)
            {
                ScriptDaemon.set_v(440, 1);
            }
            else if (c_time == 1)
            {
                if (ScriptDaemon.get_v(435) == 1)
                {
                    ScriptDaemon.set_v(435, 3);
                }
                else if (ScriptDaemon.get_v(435) >= 5 || ScriptDaemon.get_v(435) == 0 && ScriptDaemon.get_v(440) == 0)
                {
                    ScriptDaemon.set_v(440, 1);
                }
                else if (ScriptDaemon.get_v(435) == 4)
                {
                    // council_events()
                    var dummy = 1;
                }

            }
            else if (c_time == 2)
            {
                if (ScriptDaemon.get_v(435) == 3 || ScriptDaemon.get_v(435) == 1)
                {
                    ScriptDaemon.set_v(435, 4);
                    ScriptDaemon.set_v(436, 1);
                    SetGlobalFlag(432, true);
                }

            }
            // council_events()
            else if (c_time == 3)
            {
                if (ScriptDaemon.get_v(435) == 2)
                {
                    ScriptDaemon.set_v(435, 5);
                    ScriptDaemon.set_v(436, 5);
                    SetGlobalVar(750, 3);
                    SetGlobalVar(751, 3);
                    if ((!PartyLeader.HasReputation(23) && (!GetGlobalFlag(814) || !GetGlobalFlag(815))))
                    {
                        PartyLeader.AddReputation(23);
                    }

                }

            }
            else if ((c_time == 0 || c_time == 4))
            {
                if (c_time == 0 && GetGlobalFlag(336) && (ScriptDaemon.get_v(435) == 1 || ScriptDaemon.get_v(435) == 2))
                {
                    ScriptDaemon.set_v(435, 0);
                }
                else if (ScriptDaemon.get_v(435) == 3 || ScriptDaemon.get_v(435) == 4 || (ScriptDaemon.get_v(435) == 1 && c_time == 4))
                {
                    // chiefly used for the case where the whole thing played out without you
                    if (ScriptDaemon.get_v(436) == 0)
                    {
                        ScriptDaemon.set_v(436, 1);
                    }

                    ScriptDaemon.set_v(435, 5);
                    if ((!PartyLeader.HasReputation(23) && (!GetGlobalFlag(814) || !GetGlobalFlag(815))))
                    {
                        PartyLeader.AddReputation(23);
                    }

                }

                if (!ScriptDaemon.npc_get(attachee, 10) && attachee.GetMap() == 5001 && c_time == 4)
                {
                    hommletans_go_home();
                    ScriptDaemon.npc_set(attachee, 10);
                }

                if (ScriptDaemon.get_v(435) == 2 && c_time == 4)
                {
                    ScriptDaemon.set_v(435, 5);
                    ScriptDaemon.set_v(436, 5);
                    SetGlobalVar(750, 3);
                    SetGlobalVar(751, 3);
                    if ((!PartyLeader.HasReputation(23) && (!GetGlobalFlag(814) || !GetGlobalFlag(815))))
                    {
                        PartyLeader.AddReputation(23);
                    }

                }

                ScriptDaemon.set_v(440, 0);
            }

            return 1;
        }
        public static void council_events()
        {
            // this script is fired from first_heartbeat in the Hommlet Exterior map 5001
            if ((SelectedPartyLeader.GetMap() == 5001 && GetGlobalVar(435) == 4 && !GetGlobalFlag(435) && GetGlobalFlag(432)))
            {
                // spawns everyone that was inside Towne Hall
                SetGlobalFlag(435, true);
                var burne = GameSystems.MapObject.CreateObject(14453, new locXY(578, 406));
                burne.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                burne.RemoveScript(ObjScriptEvent.Heartbeat);
                burne.SetInt(obj_f.critter_description_unknown, 20000);
                burne.Move(new locXY(578, 406), 0f, 0f);
                burne.Rotation = 1;
                ScriptDaemon.destroy_weapons(burne, 4058, 0, 0);
                var jaroo = GameSystems.MapObject.CreateObject(14005, new locXY(583, 412));
                jaroo.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                jaroo.RemoveScript(ObjScriptEvent.Heartbeat);
                jaroo.SetInt(obj_f.critter_description_unknown, 20001);
                jaroo.Move(new locXY(583, 412), 0f, 0f);
                jaroo.Rotation = 0;
                ScriptDaemon.destroy_weapons(jaroo, 4047, 4111, 0);
                var rufus = GameSystems.MapObject.CreateObject(14006, new locXY(571, 407));
                rufus.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                rufus.RemoveScript(ObjScriptEvent.Heartbeat);
                rufus.SetInt(obj_f.critter_description_unknown, 8071);
                rufus.Move(new locXY(571, 407), 0f, 0f);
                rufus.Rotation = 3;
                var terjon = GameSystems.MapObject.CreateObject(14007, new locXY(570, 412));
                terjon.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                terjon.RemoveScript(ObjScriptEvent.Heartbeat);
                terjon.SetInt(obj_f.critter_description_unknown, 20003);
                terjon.Move(new locXY(570, 412), 0f, 0f);
                terjon.Rotation = 4.5f;
                ScriptDaemon.destroy_weapons(terjon, 4124, 6054, 0);
                var renton = GameSystems.MapObject.CreateObject(14012, new locXY(583, 409));
                renton.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                renton.RemoveScript(ObjScriptEvent.Heartbeat);
                renton.SetInt(obj_f.critter_description_unknown, 20007);
                renton.Move(new locXY(583, 409), 0f, 0f);
                renton.Rotation = 1;
                ScriptDaemon.destroy_weapons(renton, 4096, 4036, 6074);
                var nevets = GameSystems.MapObject.CreateObject(14102, new locXY(576, 407));
                nevets.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                nevets.RemoveScript(ObjScriptEvent.Heartbeat);
                nevets.SetInt(obj_f.critter_description_unknown, 20058);
                nevets.Move(new locXY(576, 407), 0f, 0f);
                nevets.Rotation = 3;
                var miller = GameSystems.MapObject.CreateObject(14031, new locXY(571, 412));
                miller.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                miller.RemoveScript(ObjScriptEvent.Heartbeat);
                miller.SetInt(obj_f.critter_description_unknown, 20026);
                miller.Move(new locXY(571, 412), 3f, 0f);
                miller.Rotation = 3;
                miller.AddCondition("Prone", 0, 0);
                var gundi = GameSystems.MapObject.CreateObject(14016, new locXY(582, 411));
                gundi.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                gundi.RemoveScript(ObjScriptEvent.Heartbeat);
                gundi.SetInt(obj_f.critter_description_unknown, 20011);
                gundi.Move(new locXY(582, 411), -10f, -10f);
                gundi.Rotation = 3;
                gundi.AddCondition("Prone", 0, 0);
                var crybaby = GameSystems.MapObject.CreateObject(14002, new locXY(575, 417));
                crybaby.Move(new locXY(575, 417), 0f, 0f);
                crybaby.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                crybaby.RemoveScript(ObjScriptEvent.Heartbeat);
                crybaby.Rotation = 5.5f;
                crybaby.RemoveScript(ObjScriptEvent.Heartbeat);
                crybaby.ClearObjectFlag(ObjectFlag.OFF);
                if ((GetGlobalVar(436) == 4))
                {
                    StartTimer(2000, () => proactivity(crybaby, 3000));
                }
                else
                {
                    StartTimer(7000, () => proactivity(crybaby, 3000));
                }

                var randy1 = RandomRange(1, 2);
                randy1 = 2;
                // remove randomness for testing purposes
                if ((randy1 == 1))
                {
                    var gremag = GameSystems.MapObject.CreateObject(14014, new locXY(365, 653));
                    gremag.AddCondition("Invisible", 0, 0);
                    gremag.SetObjectFlag(ObjectFlag.DONTDRAW);
                    var rannos = GameSystems.MapObject.CreateObject(14018, new locXY(366, 655));
                    rannos.AddCondition("Invisible", 0, 0);
                    rannos.SetObjectFlag(ObjectFlag.DONTDRAW);
                    var dlg_popup = GameSystems.MapObject.CreateObject(14806, new locXY(364, 653));
                }
                else
                {
                    var gremag = GameSystems.MapObject.CreateObject(14014, new locXY(318, 495));
                    gremag.AddCondition("Invisible", 0, 0);
                    gremag.SetObjectFlag(ObjectFlag.DONTDRAW);
                    var rannos = GameSystems.MapObject.CreateObject(14018, new locXY(320, 496));
                    rannos.AddCondition("Invisible", 0, 0);
                    rannos.SetObjectFlag(ObjectFlag.DONTDRAW);
                    var dlg_popup = GameSystems.MapObject.CreateObject(14806, new locXY(317, 494));
                }

                // dlg_popup.object_flag_unset(OF_DONTDRAW)
                // dlg_popup.object_flag_unset(OF_CLICK_THROUGH)
                // damage section, tough ones:
                if ((GetGlobalVar(436) != 4))
                {
                    lightly_damage(renton);
                    lightly_damage(terjon);
                    lightly_damage(rufus);
                    lightly_damage(jaroo);
                    lightly_damage(burne);
                    StartTimer(8500, () => heal_script(terjon, rufus));
                    StartTimer(9500, () => heal_script(jaroo, renton));
                }

                // damage section, frail but important ones:
                if ((GetGlobalVar(436) != 4))
                {
                    if ((GetGlobalVar(436) != 6 && GetGlobalVar(436) != 7))
                    {
                        lightly_damage(nevets);
                        lightly_damage(gundi);
                        StartTimer(700, () => heal_script(jaroo, gundi));
                        StartTimer(5800, () => heal_script(jaroo, nevets));
                    }
                    else
                    {
                        heavily_damage(nevets);
                        heavily_damage(gundi);
                    }

                }

                // damage section, frail and unimportant ones:
                if ((ScriptDaemon.get_v(436) != 4))
                {
                    if ((ScriptDaemon.get_v(436) == 3))
                    {
                        lightly_damage(miller);
                        StartTimer(3500, () => float_comment(terjon, 3000));
                        StartTimer(3510, () => heal_script(terjon, miller));
                    }
                    else
                    {
                        heavily_damage(miller);
                    }

                }

            }

            return;
        }
        public static bool is_follower(int name)
        {
            foreach (var obj in GameSystems.Party.PartyMembers)
            {
                if ((obj.GetNameId() == name))
                {
                    return true;
                }

            }

            return false;
        }
        public static void hommletans_go_home()
        {
            var moshe = GameSystems.MapObject.CreateObject(14460, new locXY(577, 412));
            foreach (var npc in ObjList.ListVicinity(moshe.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((to_be_deleted_outdoors(npc) == 1))
                {
                    npc.Destroy();
                }

            }

            moshe.Destroy();
            moshe = GameSystems.MapObject.CreateObject(14460, new locXY(365, 653));
            foreach (var npc in ObjList.ListVicinity(moshe.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((to_be_deleted_outdoors(npc) == 1))
                {
                    npc.Destroy();
                }

            }

            moshe.Destroy();
            moshe = GameSystems.MapObject.CreateObject(14460, new locXY(318, 495));
            foreach (var npc in ObjList.ListVicinity(moshe.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((to_be_deleted_outdoors(npc) == 1))
                {
                    npc.Destroy();
                }

            }

            moshe.Destroy();
            return;
        }
        public static int to_be_deleted_outdoors(GameObjectBody npc)
        {
            // 8008 - Gundigoot
            // 8048 - Rannos
            // 8049 - Gremag
            // 8054 - burne (this is the correct name)
            // 8071 - Rufus
            // 14031 - Miller
            // 14102 - Nevets
            // 14371 - badger (DO NOT DELETE THEM OUTDOORS!)
            // 14806 - Script device
            // 20001 - Jaroo
            // 20003 - Terjon
            // 20007 - Renton
            if ((npc.GetNameId() == 8008 || npc.GetNameId() == 8048 || npc.GetNameId() == 8049 || npc.GetNameId() == 8054 || npc.GetNameId() == 8071 || npc.GetNameId() == 14031 || npc.GetNameId() == 14102 || npc.GetNameId() == 20001 || npc.GetNameId() == 20003 || npc.GetNameId() == 20007))
            {
                if ((npc.GetLeader() == null))
                {
                    return 1;
                }

            }

            return 0;
        }
        public static void lightly_damage(GameObjectBody npc)
        {
            npc.Damage(null, DamageType.Poison, Dice.D6);
            npc.Damage(null, DamageType.Subdual, Dice.Parse("3d3"), D20AttackPower.NORMAL);
            return;
        }
        public static void heavily_damage(GameObjectBody npc)
        {
            // note: this script kills an NPC
            // since the san_dying is triggered, it makes the game think you killed him
            // so to avoid problems, reduce global_vars[23] (which counts the # of Hommeletans killed) beforehand
            bool flag;
            if ((GetGlobalVar(23) == 0))
            {
                flag = false;
            }
            else
            {
                flag = true;
                SetGlobalVar(23, GetGlobalVar(23) - 1);
            }

            npc.Damage(null, DamageType.Poison, Dice.Parse("30d1"));
            npc.Damage(null, DamageType.Subdual, Dice.Parse("15d1"));
            if ((!flag && GetGlobalVar(23) > 0))
            {
                SetGlobalVar(23, GetGlobalVar(23) - 1);
            }

            if ((GetGlobalVar(23) < 2 && PartyLeader.HasReputation(92)))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    pc.RemoveReputation(92);
                }

            }

            return;
        }
        public static void destroy_weapons(GameObjectBody npc, int item1, int item2, int item3)
        {
            if ((item1 != 0))
            {
                var moshe = npc.FindItemByName(item1);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            if ((item2 != 0))
            {
                var moshe = npc.FindItemByName(item2);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            if ((item3 != 0))
            {
                var moshe = npc.FindItemByName(item3);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            return;
        }
        public static void heal_script(GameObjectBody healer, GameObjectBody target)
        {
            if ((Utilities.obj_percent_hp(target) < 100))
            {
                if ((healer.GetNameId() == 20003 && target.GetNameId() == 14031))
                {
                    healer.CastSpell(WellKnownSpells.CureModerateWounds, target);
                }

                if ((healer.GetNameId() == 20003 && target.GetNameId() == 8071))
                {
                    healer.CastSpell(WellKnownSpells.CureLightWounds, target);
                }

                if ((healer.GetNameId() == 20001 && target.GetNameId() == 8008))
                {
                    healer.CastSpell(WellKnownSpells.CureCriticalWounds, target);
                }

                if ((healer.GetNameId() == 20001 && target.GetNameId() == 14102))
                {
                    healer.CastSpell(WellKnownSpells.CureSeriousWounds, target);
                }

                if ((healer.GetNameId() == 20001 && target.GetNameId() == 20007))
                {
                    healer.CastSpell(WellKnownSpells.CureModerateWounds, target);
                }

                StartTimer(700, () => heal_script(healer, target));
            }

            return;
        }
        public static void float_comment(GameObjectBody attachee, int line)
        {
            attachee.FloatLine(line, SelectedPartyLeader);
            return;
        }
        public static void proactivity(GameObjectBody npc, int line_no)
        {
            npc.TurnTowards(PartyLeader);
            if ((!Utilities.critter_is_unconscious(PartyLeader) && PartyLeader.type == ObjectType.pc && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone) && npc.HasLineOfSight(PartyLeader)))
            {
                PartyLeader.BeginDialog(npc, line_no);
            }
            else
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    npc.TurnTowards(pc);
                    if ((!Utilities.critter_is_unconscious(pc) && pc.type == ObjectType.pc && !pc.D20Query(D20DispatcherKey.QUE_Prone) && npc.HasLineOfSight(pc)))
                    {
                        pc.BeginDialog(npc, line_no);
                    }

                }

            }

            return;
        }

    }
}
