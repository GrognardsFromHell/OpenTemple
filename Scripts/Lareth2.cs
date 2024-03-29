
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

[ObjectScript(308)]
public class Lareth2 : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(840) && attachee.GetNameId() == 14614))
        {
            triggerer.BeginDialog(attachee, 500);
        }
        else if ((GetGlobalFlag(840) && attachee.GetNameId() == 14617))
        {
            triggerer.BeginDialog(attachee, 600);
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 200);
        }
        else if ((GetGlobalFlag(837)))
        {
            triggerer.BeginDialog(attachee, 100);
        }
        else
        {
            var rr = RandomRange(1, 13);
            rr = rr + 79;
            attachee.FloatLine(rr, triggerer);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(758, 0);
        SetGlobalVar(726, 0);
        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
            SetGlobalFlag(37, true);
            if ((StoryState <= 1))
            {
                StoryState = 2;
            }

            return RunDefault;
        }

        SetGlobalFlag(37, true);
        if ((StoryState <= 1))
        {
            StoryState = 2;
        }

        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            if ((pc.HasReputation(18)))
            {
                pc.RemoveReputation(18);
            }

        }

        PartyLeader.AddReputation(15);
        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 14618))
        {
            attachee.Destroy();
        }

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
        if ((attachee.GetNameId() == 14618))
        {
            attachee.Destroy();
        }

        if ((!GetGlobalFlag(837)))
        {
            return SkipDefault;
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(37, false);
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            if ((pc.HasReputation(15)))
            {
                pc.RemoveReputation(15);
            }

        }

        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            if ((!pc.HasReputation(18)))
            {
                pc.AddReputation(18);
            }

        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 14618))
        {
            attachee.Destroy();
        }

        if ((!GetGlobalFlag(837)))
        {
            if ((GetGlobalVar(758) == 0))
            {
                AttachParticles("sp-Hold Person", attachee);
            }

            if ((GetGlobalVar(758) == 1))
            {
                AttachParticles("sp-Bestow Curse", attachee);
            }

            SetGlobalVar(758, GetGlobalVar(758) + 1);
            if ((GetGlobalVar(758) >= 13))
            {
                SetGlobalVar(758, 1);
            }

        }

        if ((GetGlobalVar(726) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive() && attachee.GetMap() == 5065))
        {
            attachee.CastSpell(WellKnownSpells.Endurance, attachee);
            attachee.PendingSpellsToMemorized();
        }

        if ((GetGlobalVar(726) == 4 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive() && attachee.GetMap() == 5065))
        {
            attachee.CastSpell(WellKnownSpells.MagicCircleAgainstEvil, attachee);
            attachee.PendingSpellsToMemorized();
        }

        if ((GetGlobalVar(726) == 8 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive() && attachee.GetMap() == 5065))
        {
            attachee.CastSpell(WellKnownSpells.OwlsWisdom, attachee);
            attachee.PendingSpellsToMemorized();
        }

        SetGlobalVar(726, GetGlobalVar(726) + 1);
        return RunDefault;
    }
    public override bool OnWillKos(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(840) && attachee.GetMap() == 5065))
        {
            return SkipDefault;
        }

        return RunDefault;
    }
    public override bool OnSpellCast(GameObject attachee, GameObject triggerer, SpellPacketBody spell)
    {
        if ((spell.spellEnum == WellKnownSpells.DispelMagic && attachee.GetMap() == 5014 && !GetGlobalFlag(837)))
        {
            SetGlobalFlag(837, true);
            var loc = attachee.GetLocation();
            var cur = attachee.GetStat(Stat.hp_current);
            var max = attachee.GetStat(Stat.hp_max);
            var newHp = max - cur;
            var damage_dice = Dice.Parse("1d1");
            damage_dice = damage_dice.WithCount(newHp);
            attachee.Destroy();
            var npc = GameSystems.MapObject.CreateObject(14614, new locXY(490, 483));
            // while (i < total):
            npc.Damage(null, DamageType.Bludgeoning, damage_dice);
            if ((GetGlobalFlag(838)))
            {
                PartyLeader.BeginDialog(npc, 100);
            }
            else
            {
                PartyLeader.BeginDialog(npc, 120);
            }

        }

        if ((attachee.GetMap() == 5014 && !GetGlobalFlag(837)))
        {
            triggerer.FloatMesFileLine("mes/spell.mes", 30000);
            return SkipDefault;
        }

        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        if ((!GetGlobalFlag(806)))
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    npc.AIRemoveFromShitlist(pc);
                    npc.SetReaction(pc, 50);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5065))
        {
            var leader = attachee.GetLeader();
            if ((leader != null))
            {
                leader.BeginDialog(attachee, 400);
            }

        }

        if ((attachee.GetMap() != 5014 && attachee.GetMap() != 5015 && attachee.GetMap() != 5016 && attachee.GetMap() != 5017 && attachee.GetMap() != 5018 && attachee.GetMap() != 5019 && attachee.GetMap() != 5065))
        {
            var leader = attachee.GetLeader();
            if ((leader != null))
            {
                leader.BeginDialog(attachee, 300);
            }

        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
            attachee.SetReaction(pc, 50);
        }

        attachee.RunOff();
        return RunDefault;
    }
    public static bool create_store(GameObject attachee, GameObject triggerer)
    {
        var loc = attachee.GetLocation();
        var target = GameSystems.MapObject.CreateObject(14618, loc);
        // triggerer.barter(target)
        triggerer.BeginDialog(target, 700);
        return SkipDefault;
    }
    public static bool remove_combat(GameObject attachee, GameObject triggerer)
    {
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            if ((pc.type == ObjectType.pc))
            {
                attachee.AIRemoveFromShitlist(pc);
            }

        }

        return RunDefault;
    }

}