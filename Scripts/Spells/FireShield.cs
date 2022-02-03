
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

namespace Scripts.Spells;

[SpellScript(173)]
public class FireShield : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Fire Shield OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Fire Shield OnSpellEffect");
        // Solves Radial menu problem for Wands/NPCs
        var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
        if (spell_arg != 1 && spell_arg != 2)
        {
            spell_arg = RandomRange(1, 2);
        }

        SpellDescriptor element_type;
        string partsys_type;
        if (spell_arg == 1)
        {
            element_type = SpellDescriptor.COLD;
            partsys_type = "sp-Fire Shield-Cold";
        }
        else if (spell_arg == 2)
        {
            element_type = SpellDescriptor.FIRE;
            partsys_type = "sp-Fire Shield-Warm";
        }
        else
        {
            Logger.Error("Fire shield cast with an unknown spell arg: {0}", spell_arg);
            return;
        }

        spell.duration = 1 * spell.casterLevel;
        var npc = spell.caster; // added so NPC's can use wand/potion/scroll
        if (npc.type != ObjectType.pc && npc.GetLeader() == null && spell.duration <= 0)
        {
            spell.duration = 10;
            spell.casterLevel = 10;
        }

        var target_item = spell.Targets[0];
        var (xx, yy) = target_item.Object.GetLocation();
        if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
        {
            // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
            AttachParticles("swirled gas", target_item.Object);
            target_item.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
            Sound(7581, 1);
            Sound(7581, 1);
        }
        else
        {
            (xx, yy) = spell.caster.GetLocation();
            if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                AttachParticles("swirled gas", spell.caster);
                spell.caster.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                Sound(7581, 1);
                Sound(7581, 1);
            }
            else
            {
                target_item.Object.AddCondition("sp-Fire Shield", spell.spellId, spell.duration, element_type);
                target_item.ParticleSystem = AttachParticles(partsys_type, target_item.Object);
            }

        }

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Fire Shield OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Fire Shield OnEndSpellCast");
    }

}