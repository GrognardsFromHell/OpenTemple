
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

[SpellScript(223)]
public class HeatMetal : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Heat Metal OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Heat Metal OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = 7;
        var soundfizzle = 0;
        foreach (var target_item in spell.Targets)
        {
            var (xx, yy) = target_item.Object.GetLocation();
            if (target_item.Object.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                target_item.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                AttachParticles("swirled gas", target_item.Object);
                soundfizzle = 1;
                remove_list.Add(target_item.Object);
            }
            else
            {
                var armor_obj = target_item.Object.ItemWornAt(EquipSlot.Armor);
                if ((armor_obj != null) && (armor_obj.GetMaterial() == Material.metal))
                {
                    // wearing metal armor
                    var return_val = target_item.Object.AddCondition("sp-Heat Metal", spell.spellId, spell.duration, 0);
                    if (return_val)
                    {
                        target_item.ParticleSystem = AttachParticles("sp-Heat Metal", target_item.Object);
                    }

                }
                else
                {
                    // no metal armor
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31010);
                    remove_list.Add(target_item.Object);
                }

            }

        }

        if (soundfizzle == 1)
        {
            Sound(7581, 1);
            Sound(7581, 1);
        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Heat Metal OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Heat Metal OnEndSpellCast");
    }

}