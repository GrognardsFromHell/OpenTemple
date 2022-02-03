
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

namespace VanillaScripts;

[ObjectScript(172)]
public class Iuz : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((triggerer.FindItemByName(2203) != null))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else if ((Utilities.find_npc_near(attachee, 8032) != null))
        {
            triggerer.BeginDialog(attachee, 100);
        }
        else
        {
            triggerer.BeginDialog(attachee, 130);
        }

        return SkipDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GetGlobalFlag(361)))
        {
            SetGlobalFlag(361, true);
            AttachParticles("mon-iuz", attachee);
        }

        GameObject nearby_pc = null;

        if ((!GameSystems.Combat.IsCombatActive()))
        {
            foreach (var pc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, pc)))
                {
                    if ((nearby_pc == null))
                    {
                        nearby_pc = pc;

                    }

                    if ((pc.FindItemByName(2203) != null))
                    {
                        DetachScript();

                        pc.BeginDialog(attachee, 1);
                        return RunDefault;
                    }

                }

            }

            if ((nearby_pc != null))
            {
                if ((Utilities.find_npc_near(attachee, 8032) != null))
                {
                    DetachScript();

                    nearby_pc.BeginDialog(attachee, 100);
                }
                else
                {
                    DetachScript();

                    nearby_pc.BeginDialog(attachee, 130);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(327, true);
        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(32, GetGlobalVar(32) + 1);
        if ((GetGlobalVar(32) >= 4))
        {
            var loc = attachee.GetLocationFull();
            loc.location.locx -= 2;

            var cuthbert = GameSystems.MapObject.CreateObject(14267, loc);

            if ((cuthbert != null))
            {
                foreach (var pc in ObjList.ListVicinity(cuthbert.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.critter_is_unconscious(pc) == 0))
                    {
                        pc.BeginDialog(cuthbert, 1);
                        DetachScript();

                        return RunDefault;
                    }

                }

                cuthbert.Destroy();
            }

        }

        return RunDefault;
    }
    public static bool iuz_pc_persuade(GameObject iuz, GameObject pc, int success, int failure)
    {
        if ((!pc.SavingThrow(10, SavingThrowType.Will, D20SavingThrowFlag.NONE)))
        {
            pc.BeginDialog(iuz, failure);
        }
        else
        {
            pc.BeginDialog(iuz, success);
        }

        return SkipDefault;
    }
    public static bool iuz_pc_charm(GameObject iuz, GameObject pc)
    {
        pc.Dominate(iuz);
        if ((GameSystems.Party.PlayerCharactersSize == 1))
        {
            Utilities.set_end_slides(iuz, pc);
            GameSystems.Movies.MovieQueuePlayAndEndGame();
        }
        else
        {
            iuz.Attack(pc);
        }

        return SkipDefault;
    }
    public static bool switch_to_hedrack(GameObject iuz, GameObject pc)
    {
        var hedrack = Utilities.find_npc_near(iuz, 8032);

        if ((hedrack != null))
        {
            pc.BeginDialog(hedrack, 200);
            hedrack.TurnTowards(iuz);
            iuz.TurnTowards(hedrack);
        }
        else
        {
            pc.BeginDialog(iuz, 120);
        }

        return SkipDefault;
    }
    public static bool switch_to_cuthbert(GameObject iuz, GameObject pc, int line)
    {
        var cuthbert = Utilities.find_npc_near(iuz, 8043);

        if ((cuthbert != null))
        {
            pc.BeginDialog(cuthbert, line);
            cuthbert.TurnTowards(iuz);
            iuz.TurnTowards(cuthbert);
        }
        else
        {
            iuz.SetObjectFlag(ObjectFlag.OFF);
        }

        return SkipDefault;
    }
    public static bool iuz_animate_troops(GameObject iuz, GameObject pc)
    {
        foreach (var npc in ObjList.ListVicinity(iuz.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((npc.GetNameId() == 8032))
            {
                if ((npc.GetStat(Stat.hp_current) <= -10))
                {
                    npc.Resurrect(ResurrectionType.CuthbertResurrection, 0);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");

                    npc.Heal(null, dice);
                    npc.HealSubdual(null, dice);
                }

            }
            else if ((npc.GetStat(Stat.hp_current) <= -10))
            {
                if ((!npc.IsFriendly(pc)))
                {
                    var zombie = GameSystems.MapObject.CreateObject(14123, npc.GetLocation());

                    AttachParticles("sp-Iuz Making Zombies", zombie);
                    npc.Destroy();
                }

            }

        }

        return SkipDefault;
    }


}