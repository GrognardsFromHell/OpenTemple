
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

[SpellScript(73)]
public class Confusion : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Confusion OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Confusion OnSpellEffect");
        var remove_list = new List<GameObject>();
        var npc = spell.caster;
        if (npc.GetNameId() == 14359) // Glabrezu Guardian
        {
            spell.dc = 19;
        }
        else if (npc.GetNameId() == 14958) // Nightwalker
        {
            spell.dc = 18;
        }

        spell.duration = 1 * spell.casterLevel;
        SpawnParticles("sp-Confusion", spell.aoeCenter);
        foreach (var target_item in spell.Targets)
        {
            if (target_item.Object.GetNameId() == 14303 && target_item.Object.GetMap() == 5085 && npc.GetNameId() == 14607)
            {
                remove_list.Add(target_item.Object);
            }
            else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw unsuccesful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.Object.AddCondition("sp-Confusion", spell.spellId, spell.duration, 0);
                target_item.ParticleSystem = AttachParticles("sp-Confusion-Hit", target_item.Object);
            }
            else
            {
                // saving throw successful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target_item.Object);
                remove_list.Add(target_item.Object);
            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Confusion OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Confusion OnEndSpellCast");
    }

}