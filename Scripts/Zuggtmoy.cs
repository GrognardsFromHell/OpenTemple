
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

[ObjectScript(176)]
public class Zuggtmoy : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(181)))
        {
            SetGlobalFlag(181, false);
            transform_into_demon_form(attachee, triggerer, 80);
        }
        else if ((triggerer.FindItemByName(2203) != null))
        {
            triggerer.BeginDialog(attachee, 160);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(359)))
        {
            if ((attachee.GetStat(Stat.hp_max) > 111))
            {
                attachee.SetInt(obj_f.hp_damage, 0);
                attachee.SetBaseStat(Stat.hp_max, 111);
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

        if ((GetGlobalFlag(183)))
        {
            StartTimer(5000, () => zuggtmoy_banish(attachee, triggerer));
        }
        else
        {
            StartTimer(5000, () => zuggtmoy_die(attachee, triggerer));
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(181, false);
        transform_into_demon_form(attachee, triggerer, -1);
        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        while ((attachee.FindItemByName(8903) != null))
        {
            attachee.FindItemByName(8903).Destroy();
        }

        // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
        // create_item_in_inventory( 8903, attachee )
        if ((Utilities.obj_percent_hp(attachee) < 20))
        {
            GameObject nearby_pc = null;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.type == ObjectType.pc)
                {
                    attachee.AIRemoveFromShitlist(pc);
                }

            }

            foreach (var pc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((attachee.HasLineOfSight(pc)))
                {
                    if ((nearby_pc == null))
                    {
                        nearby_pc = pc;
                    }

                    if (pc.FindItemByName(2203) != null)
                    {
                        DetachScript();
                        pc.BeginDialog(attachee, 330);
                        return RunDefault;
                    }

                }

            }

            if ((nearby_pc != null))
            {
                DetachScript();
                nearby_pc.BeginDialog(attachee, 330);
                return SkipDefault;
            }

        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            GameObject nearby_unmet_pc = null;
            GameObject distant_pc = null;
            var found_close_pc = false;
            foreach (var pc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if (((GetGlobalFlag(181)) && (!found_close_pc)))
                {
                    if ((distant_pc == null))
                    {
                        if ((attachee.DistanceTo(pc) > 30))
                        {
                            distant_pc = pc;
                        }
                        else
                        {
                            found_close_pc = true;
                            distant_pc = null;
                        }

                    }

                }

                if ((!attachee.HasMet(pc)))
                {
                    if ((Utilities.is_safe_to_talk(attachee, pc)))
                    {
                        if ((nearby_unmet_pc == null))
                        {
                            nearby_unmet_pc = pc;
                        }

                        if ((!GetGlobalFlag(193)) && (pc.FindItemByName(2203) != null))
                        {
                            pc.BeginDialog(attachee, 160);
                            return RunDefault;
                        }

                    }

                }

            }

            if (((!GetGlobalFlag(193)) && (nearby_unmet_pc != null)))
            {
                nearby_unmet_pc.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(181)))
            {
                if ((distant_pc != null))
                {
                    SetGlobalFlag(181, false);
                    transform_into_demon_form(attachee, distant_pc, 80);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnTrueSeeing(GameObject attachee, GameObject triggerer)
    {
        // HTN - 12/03/02, reversed flag logic, moved "transform" call and SKIP_DEFAULT inside if/else block
        // only transform once, if flag is unset
        if ((GetGlobalFlag(181)))
        {
            // set flag
            SetGlobalFlag(181, false);
            // perform transform and go into dialog
            transform_into_demon_form(attachee, triggerer, 300);
            // SKIP ==> tells true_seeing script to put particle f/x on target (zuggtmoy)
            return SkipDefault;
        }

        // tells true_seeing script not to put particle f/x on target (zuggtmoy)
        return RunDefault;
    }
    public static bool transform_into_demon_form(GameObject zuggtmoy, GameObject pc, int line)
    {
        if ((!GetGlobalFlag(193)))
        {
            SetGlobalFlag(193, true);
            StoryState = 6;
            var loc = zuggtmoy.GetLocation();
            zuggtmoy.Destroy();
            loc = new locXY(536, 499);
            var new_zuggtmoy = GameSystems.MapObject.CreateObject(14265, loc);
            if ((new_zuggtmoy != null))
            {
                if ((GetGlobalFlag(359)))
                {
                    if ((new_zuggtmoy.GetStat(Stat.hp_max) > 111))
                    {
                        new_zuggtmoy.SetInt(obj_f.hp_damage, 0);
                        new_zuggtmoy.SetBaseStat(Stat.hp_max, 111);
                    }

                }

                new_zuggtmoy.Rotation = 3.9269908f;
                new_zuggtmoy.SetConcealed(true);
                new_zuggtmoy.Unconceal();
                AttachParticles("mon-zug-appear", new_zuggtmoy);
                if ((line != -1))
                {
                    pc.BeginDialog(new_zuggtmoy, line);
                }
                else
                {
                    new_zuggtmoy.Attack(SelectedPartyLeader);
                }

            }

        }

        return RunDefault;
    }
    public static bool crone_wait(GameObject zuggtmoy, GameObject pc)
    {
        // turn zuggtmoy invisible
        zuggtmoy.AddCondition("Invisible", 0, 0);
        zuggtmoy.SetObjectFlag(ObjectFlag.DONTDRAW);
        SetGlobalFlag(181, true);
        return RunDefault;
    }
    public static bool zuggtmoy_pc_persuade(GameObject zuggtmoy, GameObject pc, int success, int failure)
    {
        if ((!pc.SavingThrow(10, SavingThrowType.Will, D20SavingThrowFlag.NONE)))
        {
            pc.BeginDialog(zuggtmoy, failure);
        }
        else
        {
            pc.BeginDialog(zuggtmoy, success);
        }

        return SkipDefault;
    }
    public static bool zuggtmoy_pc_charm(GameObject zuggtmoy, GameObject pc)
    {
        // auto dire charm the PC
        pc.Dominate(zuggtmoy); // nope, it doesn't work for adding PCs back into the party :(
        if ((GameSystems.Party.PlayerCharactersSize == 1))
        {
            zuggtmoy_end_game(zuggtmoy, pc);
        }
        else
        {
            zuggtmoy.Attack(pc);
        }

        return SkipDefault;
    }
    public static bool zuggtmoy_regenerate_and_attack(GameObject zuggtmoy, GameObject pc)
    {
        // zuggtmoy.obj_set_int( obj_f_hp_damage, 0 )
        var dice = Dice.Parse("1d10+1000");
        zuggtmoy.Heal(null, dice);
        zuggtmoy.HealSubdual(null, dice);
        zuggtmoy.Attack(pc);
        return RunDefault;
    }
    public static bool zuggtmoy_banish(GameObject zuggtmoy, GameObject pc)
    {
        SetGlobalFlag(188, true);
        SetGlobalFlag(372, true);
        // play banishment movie
        Fade(0, 0, 301, 0);
        if ((!GetGlobalFlag(500)))
        {
            zuggtmoy_end_game(zuggtmoy, pc);
        }
        else if ((GetGlobalFlag(500)))
        {
            zuggtmoy_end_game_nc(zuggtmoy, pc);
        }

        zuggtmoy.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool zuggtmoy_die(GameObject zuggtmoy, GameObject pc)
    {
        SetGlobalFlag(189, true);
        SetGlobalFlag(372, true);
        // play death movie
        Fade(0, 0, 302, 0);
        if ((!GetGlobalFlag(500)))
        {
            zuggtmoy_end_game(zuggtmoy, pc);
        }
        else if ((GetGlobalFlag(500)))
        {
            zuggtmoy_end_game_nc(zuggtmoy, pc);
        }

        return RunDefault;
    }
    public static bool zuggtmoy_end_game(GameObject zuggtmoy, GameObject pc)
    {
        // play slides and end game
        Utilities.set_end_slides(zuggtmoy, pc);
        GameSystems.Movies.MovieQueuePlayAndEndGame();
        return RunDefault;
    }
    public static bool zuggtmoy_end_game_nc(GameObject zuggtmoy, GameObject pc)
    {
        // play slides and don't end game
        Utilities.set_end_slides_nc(zuggtmoy, pc);
        GameSystems.Movies.MovieQueuePlay();
        Utilities.create_item_in_inventory(11074, PartyLeader);
        PartyLeader.AddReputation(91);
        MakeAreaKnown(14);
        FadeAndTeleport(0, 0, 0, 5121, 228, 507);
        return RunDefault;
    }
    public static bool zuggtmoy_pillar_gone(GameObject zuggtmoy, GameObject pc)
    {
        // get rid of pillar
        foreach (var obj in ObjList.ListVicinity(zuggtmoy.GetLocation(), ObjectListFilter.OLC_SCENERY))
        {
            if ((obj.GetNameId() == 1619))
            {
                obj.SetObjectFlag(ObjectFlag.OFF);
                return RunDefault;
            }

        }

        return RunDefault;
    }

}