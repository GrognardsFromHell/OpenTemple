
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
    [ObjectScript(485)]
    public class HextorInvader : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            if ((attachee.GetNameId() == 8737))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(511, GetGlobalVar(511) + 1);
                if ((GetGlobalVar(511) >= 24 && GetGlobalFlag(501)))
                {
                    SetGlobalFlag(511, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8738))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(512, GetGlobalVar(512) + 1);
                if ((GetGlobalVar(512) >= 24))
                {
                    SetGlobalFlag(512, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8739))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(513, GetGlobalVar(513) + 1);
                if ((GetGlobalVar(513) >= 24))
                {
                    SetGlobalFlag(513, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8740))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(514, GetGlobalVar(514) + 1);
                if ((GetGlobalVar(514) >= 24))
                {
                    SetGlobalFlag(514, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8741))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(515, GetGlobalVar(515) + 1);
                if ((GetGlobalVar(515) >= 24))
                {
                    SetGlobalFlag(515, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8742))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(516, GetGlobalVar(516) + 1);
                if ((GetGlobalVar(516) >= 12))
                {
                    SetGlobalFlag(516, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8743))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(517, GetGlobalVar(517) + 1);
                if ((GetGlobalVar(517) >= 12))
                {
                    SetGlobalFlag(517, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8744))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(518, GetGlobalVar(518) + 1);
                if ((GetGlobalVar(518) >= 12))
                {
                    SetGlobalFlag(518, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8745))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(519, GetGlobalVar(519) + 1);
                if ((GetGlobalVar(519) >= 12))
                {
                    SetGlobalFlag(519, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8746))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(520, GetGlobalVar(520) + 1);
                if ((GetGlobalVar(520) >= 5))
                {
                    SetGlobalFlag(520, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8747))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(521, GetGlobalVar(521) + 1);
                if ((GetGlobalVar(521) >= 6))
                {
                    SetGlobalFlag(521, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8748))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(522, GetGlobalVar(522) + 1);
                if ((GetGlobalVar(522) >= 6))
                {
                    SetGlobalFlag(522, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(505) == 0))
            {
                StartTimer(7200000, () => out_of_time(attachee, triggerer)); // 2 hours
                SetGlobalVar(505, 1);
            }

            if ((triggerer.type == ObjectType.pc))
            {
                if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8736)))
                {
                    var wakefield = Utilities.find_npc_near(triggerer, 8736);
                    if ((wakefield != null))
                    {
                        triggerer.RemoveFollower(wakefield);
                        wakefield.FloatLine(20000, triggerer);
                        wakefield.Attack(triggerer);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 8738))
            {
                attachee.SetInt(obj_f.critter_strategy, 436);
            }
            else if ((attachee.GetNameId() == 8739))
            {
                attachee.SetInt(obj_f.critter_strategy, 437);
            }
            else if ((attachee.GetNameId() == 8740))
            {
                attachee.SetInt(obj_f.critter_strategy, 438);
            }
            else if ((attachee.GetNameId() == 8741))
            {
                attachee.SetInt(obj_f.critter_strategy, 439);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(97) == QuestState.Botched))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if ((attachee.GetNameId() == 8738))
            {
                if ((!GetGlobalFlag(509)))
                {
                    StartTimer(6000, () => tower_attack(attachee, triggerer));
                    SetGlobalFlag(509, true);
                }

            }
            else if ((attachee.GetNameId() == 8739))
            {
                if ((!GetGlobalFlag(510)))
                {
                    StartTimer(6000, () => church_attack(attachee, triggerer));
                    SetGlobalFlag(510, true);
                }

            }
            else if ((attachee.GetNameId() == 8740))
            {
                if ((!GetGlobalFlag(523)))
                {
                    StartTimer(6000, () => grove_attack(attachee, triggerer));
                    SetGlobalFlag(523, true);
                }

            }
            else if ((attachee.GetNameId() == 8741))
            {
                if ((!GetGlobalFlag(524)))
                {
                    StartTimer(6000, () => wench_attack(attachee, triggerer));
                    SetGlobalFlag(524, true);
                }

            }

            var float_select = RandomRange(1, 6);
            if (attachee.GetScriptId(ObjScriptEvent.Dialog) != 0)
            {
                attachee.FloatLine(float_select, triggerer);
            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(525)))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }
        public static void destroy_gear(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var fighter_longsword = attachee.FindItemByName(4132);
            fighter_longsword.Destroy();
            var fighter_towershield = attachee.FindItemByName(6078);
            fighter_towershield.Destroy();
            var soldier_shield = attachee.FindItemByName(6068);
            soldier_shield.Destroy();
            var cleric_crossbow = attachee.FindItemByName(4178);
            cleric_crossbow.Destroy();
            var archer_longbow = attachee.FindItemByName(4087);
            archer_longbow.Destroy();
            var gold_breastplate = attachee.FindItemByName(6477);
            gold_breastplate.Destroy();
            var gold_chainmail = attachee.FindItemByName(6476);
            gold_chainmail.Destroy();
            var plain_chainmail = attachee.FindItemByName(6454);
            plain_chainmail.Destroy();
            var red_chainmail = attachee.FindItemByName(6019);
            red_chainmail.Destroy();
            var fine_chainmail = attachee.FindItemByName(6475);
            fine_chainmail.Destroy();
            var splintmail = attachee.FindItemByName(6096);
            splintmail.Destroy();
            var black_bandedmail = attachee.FindItemByName(6341);
            black_bandedmail.Destroy();
            var silver_bandedmail = attachee.FindItemByName(6120);
            silver_bandedmail.Destroy();
            var halfplate = attachee.FindItemByName(6158);
            halfplate.Destroy();
            return;
        }
        public static void out_of_time(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(505, 3);
            return;
        }
        public static bool tower_attack(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SpawnParticles("sp-Fireball-Hit", new locXY(455, 609));
            SpawnParticles("ef-fireburning", new locXY(455, 609));
            SpawnParticles("ef-FirePit", new locXY(455, 609));
            SpawnParticles("sp-Fireball-Hit", new locXY(439, 610));
            SpawnParticles("ef-fireburning", new locXY(439, 610));
            SpawnParticles("ef-FirePit", new locXY(439, 610));
            Sound(4134, 1);
            GameSystems.Scroll.ShakeScreen(75, 3200);
            StartTimer(12000, () => tower_attack_followup());
            return RunDefault;
        }
        public static bool church_attack(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SpawnParticles("sp-Fireball-Hit", new locXY(490, 224));
            SpawnParticles("ef-fireburning", new locXY(490, 224));
            SpawnParticles("ef-FirePit", new locXY(490, 224));
            SpawnParticles("sp-Fireball-Hit", new locXY(506, 217));
            SpawnParticles("ef-fireburning", new locXY(506, 217));
            SpawnParticles("ef-FirePit", new locXY(506, 217));
            Sound(4135, 1);
            GameSystems.Scroll.ShakeScreen(75, 3200);
            StartTimer(12000, () => church_attack_followup());
            return RunDefault;
        }
        public static bool grove_attack(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SpawnParticles("sp-Fireball-Hit", new locXY(617, 523));
            SpawnParticles("ef-fireburning", new locXY(617, 523));
            SpawnParticles("ef-FirePit", new locXY(617, 523));
            SpawnParticles("sp-Fireball-Hit", new locXY(616, 515));
            SpawnParticles("ef-fireburning", new locXY(616, 515));
            SpawnParticles("ef-FirePit", new locXY(616, 515));
            Sound(4136, 1);
            GameSystems.Scroll.ShakeScreen(75, 3200);
            StartTimer(12000, () => grove_attack_followup());
            return RunDefault;
        }
        public static bool wench_attack(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SpawnParticles("sp-Fireball-Hit", new locXY(621, 397));
            SpawnParticles("ef-fireburning", new locXY(621, 397));
            SpawnParticles("ef-FirePit", new locXY(621, 397));
            SpawnParticles("sp-Fireball-Hit", new locXY(609, 399));
            SpawnParticles("ef-fireburning", new locXY(609, 399));
            SpawnParticles("ef-FirePit", new locXY(609, 399));
            Sound(4136, 1);
            GameSystems.Scroll.ShakeScreen(75, 3200);
            StartTimer(12000, () => wench_attack_followup());
            return RunDefault;
        }
        public static bool tower_attack_followup()
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var random_x = RandomRange(428, 465);
                var random_y = RandomRange(597, 617);
                SpawnParticles("sp-Fireball-Hit", new locXY(random_x, random_y));
                SpawnParticles("ef-fireburning", new locXY(random_x, random_y));
                SpawnParticles("ef-FirePit", new locXY(random_x, random_y));
                Sound(4135, 1);
                GameSystems.Scroll.ShakeScreen(50, 1600);
                StartTimer(12000, () => tower_attack_followup());
            }

            return RunDefault;
        }
        public static bool church_attack_followup()
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var random_x = RandomRange(478, 509);
                var random_y = RandomRange(207, 235);
                SpawnParticles("sp-Fireball-Hit", new locXY(random_x, random_y));
                SpawnParticles("ef-fireburning", new locXY(random_x, random_y));
                SpawnParticles("ef-FirePit", new locXY(random_x, random_y));
                Sound(4135, 1);
                GameSystems.Scroll.ShakeScreen(50, 1600);
                StartTimer(12000, () => church_attack_followup());
            }

            return RunDefault;
        }
        public static bool grove_attack_followup()
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var random_x = RandomRange(593, 621);
                var random_y = RandomRange(508, 538);
                SpawnParticles("sp-Fireball-Hit", new locXY(random_x, random_y));
                SpawnParticles("ef-fireburning", new locXY(random_x, random_y));
                SpawnParticles("ef-FirePit", new locXY(random_x, random_y));
                Sound(4135, 1);
                GameSystems.Scroll.ShakeScreen(50, 1600);
                StartTimer(12000, () => grove_attack_followup());
            }

            return RunDefault;
        }
        public static bool wench_attack_followup()
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var random_x = RandomRange(590, 641);
                var random_y = RandomRange(370, 404);
                SpawnParticles("sp-Fireball-Hit", new locXY(random_x, random_y));
                SpawnParticles("ef-fireburning", new locXY(random_x, random_y));
                SpawnParticles("ef-FirePit", new locXY(random_x, random_y));
                Sound(4135, 1);
                GameSystems.Scroll.ShakeScreen(50, 1600);
                StartTimer(12000, () => wench_attack_followup());
            }

            return RunDefault;
        }

    }
}
