
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

namespace Scripts;

[ObjectScript(484)]
public class Wakefield : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            attachee.FloatLine(10000, triggerer);
        }
        else if ((PartyLeader.HasReputation(61)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else
        {
            triggerer.BeginDialog(attachee, 100);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

        }

        SetGlobalFlag(501, true);
        SetGlobalVar(511, GetGlobalVar(511) + 1);
        if ((GetGlobalVar(511) >= 12 && GetGlobalFlag(501)))
        {
            SetGlobalFlag(511, true);
            if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
            {
                SetQuestState(97, QuestState.Completed);
                PartyLeader.AddReputation(52);
                SetGlobalVar(501, 7);
            }

        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(505) == 0))
        {
            StartTimer(7200000, () => out_of_time(attachee, triggerer)); // 2 hours
            SetGlobalVar(505, 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(501, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((PartyLeader.HasReputation(61)))
        {
            if ((attachee.GetLeader() != null))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    var leader = attachee.GetLeader();
                    leader.RemoveFollower(attachee);
                    PartyLeader.BeginDialog(attachee, 1);
                    DetachScript();
                }
            }
        }
        else if ((GetQuestState(97) == QuestState.Botched))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else if ((GetGlobalVar(501) == 5))
        {
            if ((attachee.GetLeader() == null))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            attachee.TurnTowards(PartyLeader);
                            PartyLeader.BeginDialog(attachee, 100);
                        }

                    }

                }

            }

        }
        // game.new_sid = 0
        else if ((GetGlobalVar(501) == 4))
        {
            if ((attachee.GetLeader() == null))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    Sound(4132, 2);
                    Sound(4136, 1);
                    GameSystems.Scroll.ShakeScreen(75, 3200);
                    if ((GetGlobalVar(504) == 0))
                    {
                        if ((PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL))
                        {
                            attachee.CastSpell(WellKnownSpells.MagicCircleAgainstChaos, attachee);
                        }
                        else
                        {
                            attachee.CastSpell(WellKnownSpells.DivinePower, attachee);
                        }

                        SetGlobalVar(504, 1);
                    }

                    StartTimer(3000, () => set_var_501(attachee, triggerer));
                }

            }

        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(527, true);
        var rod = attachee.FindItemByName(4232);
        rod.SetItemFlag(ItemFlag.NO_TRANSFER);
        var armor = attachee.FindItemByName(6334);
        armor.SetItemFlag(ItemFlag.NO_TRANSFER);
        var boots = attachee.FindItemByName(6045);
        boots.SetItemFlag(ItemFlag.NO_TRANSFER);
        var gloves = attachee.FindItemByName(6046);
        gloves.SetItemFlag(ItemFlag.NO_TRANSFER);
        var helm = attachee.FindItemByName(6335);
        helm.SetItemFlag(ItemFlag.NO_TRANSFER);
        var shield = attachee.FindItemByName(6079);
        shield.SetItemFlag(ItemFlag.NO_TRANSFER);
        var cloak = attachee.FindItemByName(6233);
        cloak.SetItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(527, false);
        var rod = attachee.FindItemByName(4232);
        rod.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var armor = attachee.FindItemByName(6334);
        armor.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var boots = attachee.FindItemByName(6045);
        boots.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var gloves = attachee.FindItemByName(6046);
        gloves.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var helm = attachee.FindItemByName(6335);
        helm.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var shield = attachee.FindItemByName(6079);
        shield.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var cloak = attachee.FindItemByName(6233);
        cloak.ClearItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public static void hextor_buff_2(GameObject attachee, GameObject triggerer)
    {
        attachee.CastSpell(WellKnownSpells.RighteousMight, attachee);
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((obj.GetNameId() == 8753))
            {
                obj.CastSpell(WellKnownSpells.WindWall, obj);
            }

            if ((obj.GetNameId() == 8754))
            {
                obj.CastSpell(WellKnownSpells.Blur, obj);
            }

        }

        return;
    }
    public static bool set_var_501(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(501, 5);
        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 40))
            {
                return true;
            }

        }

        return false;
    }
    public static void out_of_time(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(505, 3);
        return;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return RunDefault;
    }
    public static bool flag_no_transfer(GameObject attachee, GameObject triggerer)
    {
        var orb = attachee.FindItemByName(2203);
        orb.SetItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public static bool td_attack(GameObject attachee, GameObject triggerer)
    {
        StartTimer(6000, () => defense_attack(attachee, triggerer));
        return RunDefault;
    }
    public static bool defense_attack(GameObject attachee, GameObject triggerer)
    {
        SpawnParticles("sp-Fireball-Hit", new locXY(493, 469));
        SpawnParticles("ef-fireburning", new locXY(493, 469));
        SpawnParticles("ef-FirePit", new locXY(493, 469));
        SpawnParticles("sp-Fireball-Hit", new locXY(494, 461));
        SpawnParticles("ef-fireburning", new locXY(494, 461));
        SpawnParticles("ef-FirePit", new locXY(494, 461));
        Sound(4136, 1);
        GameSystems.Scroll.ShakeScreen(75, 3200);
        StartTimer(12000, () => defense_attack_followup());
        return RunDefault;
    }
    public static bool defense_attack_followup()
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            var random_x = RandomRange(465, 494);
            var random_y = RandomRange(446, 469);
            SpawnParticles("sp-Fireball-Hit", new locXY(random_x, random_y));
            SpawnParticles("ef-fireburning", new locXY(random_x, random_y));
            SpawnParticles("ef-FirePit", new locXY(random_x, random_y));
            Sound(4135, 1);
            GameSystems.Scroll.ShakeScreen(50, 1600);
            StartTimer(12000, () => defense_attack_followup());
        }

        return RunDefault;
    }

}