
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(338)]
    public class Viscount : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if (GetGlobalVar(923) == 0)
            {
                var tempp = 0;
                for (var p = 0; p < 12; p++)
                {
                    tempp += RandomRange(0, 8);
                }

                tempp -= 24;
                if (tempp < 5)
                {
                    tempp = 5;
                }

                SetGlobalVar(923, tempp);
            }
            else if (!ScriptDaemon.tpsts("s_ranths_bandits_1", 0))
            {
                ScriptDaemon.record_time_stamp("s_ranths_bandits_1");
            }

            attachee.TurnTowards(triggerer);
            if ((GetQuestState(78) == QuestState.Completed && GetQuestState(107) == QuestState.Unknown && GetQuestState(112) == QuestState.Mentioned))
            {
                triggerer.BeginDialog(attachee, 430);
            }

            if ((GetQuestState(74) == QuestState.Completed && GetQuestState(78) == QuestState.Unknown && GetQuestState(111) == QuestState.Mentioned))
            {
                triggerer.BeginDialog(attachee, 450);
            }
            else if ((GetGlobalVar(993) == 7))
            {
                triggerer.BeginDialog(attachee, 630);
            }
            else if ((GetGlobalVar(993) == 9))
            {
                triggerer.BeginDialog(attachee, 710);
            }
            else if ((attachee.GetMap() == 5156))
            {
                triggerer.BeginDialog(attachee, 910);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(992)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5156 && GetGlobalVar(704) == 3 && Utilities.is_daytime() && GetQuestState(76) != QuestState.Accepted))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5170 && GetGlobalVar(979) == 2))
            {
                if ((Utilities.is_daytime()))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else if ((!Utilities.is_daytime()))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5135 && GetGlobalVar(979) == 2))
            {
                if ((Utilities.is_daytime()))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((!Utilities.is_daytime()))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.AddCondition("fallen_paladin");
            }

            if ((attachee.GetMap() == 5170 || attachee.GetMap() == 5135))
            {
                SetGlobalFlag(992, true);
                SetGlobalFlag(935, true);
                PartyLeader.AddReputation(44);
            }
            else if ((attachee.GetMap() == 5156))
            {
                if ((GetGlobalFlag(940)))
                {
                    SetGlobalFlag(935, true);
                    PartyLeader.AddReputation(44);
                }

                SetGlobalFlag(992, true);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 8703))
            {
                if ((attachee.GetMap() == 5156))
                {
                    attachee.FloatLine(5000, triggerer);
                }

                if ((attachee.GetMap() == 5170))
                {
                    var samson = GameSystems.MapObject.CreateObject(14660, new locXY(501, 484));
                    samson.TurnTowards(triggerer);
                    samson.Attack(PartyLeader);
                    var goliath = GameSystems.MapObject.CreateObject(14661, new locXY(498, 484));
                    goliath.TurnTowards(triggerer);
                    goliath.Attack(PartyLeader);
                    var bathsheba = GameSystems.MapObject.CreateObject(14659, new locXY(495, 484));
                    bathsheba.TurnTowards(triggerer);
                    bathsheba.FloatLine(1000, triggerer);
                    bathsheba.Attack(PartyLeader);
                }

                if ((attachee.GetMap() == 5135 && attachee.GetNameId() == 8703))
                {
                    var samson = GameSystems.MapObject.CreateObject(14660, new locXY(494, 488));
                    samson.TurnTowards(triggerer);
                    samson.Attack(PartyLeader);
                    var goliath = GameSystems.MapObject.CreateObject(14661, new locXY(494, 491));
                    goliath.TurnTowards(triggerer);
                    goliath.Attack(PartyLeader);
                    var bathsheba = GameSystems.MapObject.CreateObject(14659, new locXY(481, 496));
                    bathsheba.TurnTowards(triggerer);
                    bathsheba.FloatLine(1000, triggerer);
                    bathsheba.Attack(PartyLeader);
                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            SetCounter(0, GetCounter(0) + 1);
            if ((GetCounter(0) == 1))
            {
                attachee.FloatLine(1000, triggerer);
                return SkipDefault;
            }
            else if ((GetCounter(0) == 2))
            {
                overseers_show_up(attachee, triggerer);
                SetGlobalVar(704, 4);
            }
            else if ((GetCounter(0) == 3))
            {
                attachee.FloatLine(2000, triggerer);
                return SkipDefault;
            }
            else if ((GetCounter(0) == 4))
            {
                guards_show_up(attachee, triggerer);
                SetGlobalVar(704, 5);
            }
            else if ((GetCounter(0) == 5))
            {
                attachee.FloatLine(4000, triggerer);
                return SkipDefault;
            }
            else if ((GetCounter(0) == 6))
            {
                guardian_show_up(attachee, triggerer);
                SetGlobalVar(704, 6);
            }
            else if ((GetCounter(0) == 7))
            {
                attachee.FloatLine(3000, triggerer);
                return SkipDefault;
            }
            else if ((GetCounter(0) == 8))
            {
                mages_show_up(attachee, triggerer);
                SetGlobalVar(704, 7);
            }
            else if ((GetCounter(0) == 9))
            {
                SetGlobalVar(704, 8);
            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            if ((PartyLeader.HasReputation(34)))
            {
                return RunDefault;
            }
            else if ((!GetGlobalFlag(992)))
            {
                return SkipDefault;
            }

            return RunDefault;
        }
        public static bool distribute_verbobonc_uniform(GameObject npc, GameObject pc)
        {
            foreach (var obj in pc.GetPartyMembers())
            {
                Utilities.create_item_in_inventory(6498, obj);
                Utilities.create_item_in_inventory(6269, obj);
            }

            return RunDefault;
        }
        public static bool overseers_show_up(GameObject attachee, GameObject triggerer)
        {
            var samson = GameSystems.MapObject.CreateObject(14660, new locXY(482, 494));
            samson.TurnTowards(triggerer);
            samson.FloatLine(1000, triggerer);
            var goliath = GameSystems.MapObject.CreateObject(14661, new locXY(484, 495));
            goliath.TurnTowards(triggerer);
            samson.Attack(PartyLeader);
            goliath.Attack(PartyLeader);
            return RunDefault;
        }
        public static bool guards_show_up(GameObject attachee, GameObject triggerer)
        {
            var guard1 = GameSystems.MapObject.CreateObject(14644, new locXY(481, 493));
            guard1.TurnTowards(triggerer);
            guard1.FloatLine(1000, triggerer);
            var guard2 = GameSystems.MapObject.CreateObject(14644, new locXY(483, 495));
            guard2.TurnTowards(triggerer);
            var guard3 = GameSystems.MapObject.CreateObject(14644, new locXY(479, 493));
            guard3.TurnTowards(triggerer);
            var guard4 = GameSystems.MapObject.CreateObject(14644, new locXY(481, 495));
            guard4.TurnTowards(triggerer);
            guard1.Attack(PartyLeader);
            guard2.Attack(PartyLeader);
            guard3.Attack(PartyLeader);
            guard4.Attack(PartyLeader);
            return RunDefault;
        }
        public static bool guardian_show_up(GameObject attachee, GameObject triggerer)
        {
            var bathsheba = GameSystems.MapObject.CreateObject(14659, new locXY(484, 494));
            bathsheba.TurnTowards(triggerer);
            bathsheba.FloatLine(2000, triggerer);
            bathsheba.Attack(PartyLeader);
            return RunDefault;
        }
        public static bool mages_show_up(GameObject attachee, GameObject triggerer)
        {
            var mage1 = GameSystems.MapObject.CreateObject(14658, attachee.GetLocation().OffsetTiles(-4, 0));
            AttachParticles("sp-Teleport", mage1);
            mage1.TurnTowards(triggerer);
            mage1.FloatLine(1000, triggerer);
            var mage2 = GameSystems.MapObject.CreateObject(14658, attachee.GetLocation().OffsetTiles(-4, 0));
            AttachParticles("sp-Teleport", mage2);
            mage2.TurnTowards(triggerer);
            Sound(4035, 1);
            foreach (var obj in ObjList.ListVicinity(mage1.GetLocation(), ObjectListFilter.OLC_PC))
            {
                mage1.Attack(obj);
            }

            foreach (var obj in ObjList.ListVicinity(mage2.GetLocation(), ObjectListFilter.OLC_PC))
            {
                mage2.Attack(obj);
            }

            return RunDefault;
        }
        public static void ditch_captains(GameObject attachee, GameObject triggerer)
        {
            var abiram = Utilities.find_npc_near(attachee, 8706);
            abiram.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
            var absalom = Utilities.find_npc_near(attachee, 8707);
            absalom.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
            var achan = Utilities.find_npc_near(attachee, 8708);
            achan.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
            return;
        }
        public static bool switch_to_captain(GameObject attachee, GameObject triggerer, int line)
        {
            var abiram = Utilities.find_npc_near(attachee, 8706);
            var absalom = Utilities.find_npc_near(attachee, 8707);
            var achan = Utilities.find_npc_near(attachee, 8708);
            if ((abiram != null))
            {
                triggerer.BeginDialog(abiram, line);
            }

            if ((absalom != null))
            {
                triggerer.BeginDialog(absalom, line);
            }

            if ((achan != null))
            {
                triggerer.BeginDialog(achan, line);
            }

            return SkipDefault;
        }
        public static bool schedule_bandits_1(GameObject attachee, GameObject triggerer)
        {
            var tempp = GetGlobalVar(923);
            if (GetGlobalVar(923) == 0)
            {
                for (var p = 0; p < 12; p++)
                {
                    tempp += RandomRange(0, 8);
                }

                tempp -= 24;
                if (tempp < 5)
                {
                    tempp = 5;
                }

                // approximate a gaussian distribution by adding together 12 uniformly distributed random variables
                // average result will be 24 days, standard deviation will be 8 days
                // it is then truncated at 5 days minimum (feel free to change) (roughly 1% of results might reach 5 or lower otherwise, even negative is possible though rare)
                SetGlobalVar(923, tempp);
            }

            StartTimer(tempp * 24 * 60 * 60 * 1000, () => set_bandits());
            ScriptDaemon.record_time_stamp("s_ranths_bandits_1");
            return RunDefault;
        }
        public static bool set_bandits()
        {
            QueueRandomEncounter(3434);
            ScriptDaemon.set_f("s_ranths_bandits_scheduled");
            return RunDefault;
        }
        public static void slavers_movie_setup(GameObject attachee, GameObject triggerer)
        {
            set_slavers_slides();
            return;
        }
        public static bool set_slavers_slides()
        {
            GameSystems.Movies.MovieQueueAdd(601);
            GameSystems.Movies.MovieQueueAdd(602);
            GameSystems.Movies.MovieQueueAdd(603);
            GameSystems.Movies.MovieQueueAdd(604);
            GameSystems.Movies.MovieQueueAdd(605);
            GameSystems.Movies.MovieQueueAdd(606);
            GameSystems.Movies.MovieQueueAdd(607);
            GameSystems.Movies.MovieQueueAdd(608);
            GameSystems.Movies.MovieQueueAdd(609);
            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }

    }
}
