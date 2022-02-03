
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

[SpellScript(7)]
public class AnimalGrowth : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Animal Growth OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Animal Growth OnSpellEffect");
        // Dar's check for ranger caster level no longer needed thanks to Spellslinger's dll hack
        // if spell.caster_class == 14:
        // if spell.spell_level < 4:#added to check for proper ranger slot level (darmagon)
        // spell.caster.float_mesfile_line('mes\\spell.mes', 16008)
        // spell.spell_end(spell.id)
        // return
        var remove_list = new List<GameObject>();
        spell.duration = 10 * spell.casterLevel;
        foreach (var target_item in spell.Targets)
        {
            if ((target_item.Object.IsMonsterCategory(MonsterCategory.animal)))
            {
                if (target_item.Object.IsFriendly(spell.caster))
                {
                    target_item.Object.AddCondition("sp-Animal Growth", spell.spellId, spell.duration, 0);
                    target_item.ParticleSystem = AttachParticles("sp-Animal Growth", target_item.Object);
                }
                else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    target_item.Object.AddCondition("sp-Animal Growth", spell.spellId, spell.duration, 0);
                    target_item.ParticleSystem = AttachParticles("sp-Animal Growth", target_item.Object);
                }
                else
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target_item.Object);
                    remove_list.Add(target_item.Object);
                }

            }
            else
            {
                // not an animal
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target_item.Object.FloatMesFileLine("mes/spell.mes", 31002);
                AttachParticles("Fizzle", target_item.Object);
                remove_list.Add(target_item.Object);
            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Animal Growth OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Animal Growth OnEndSpellCast");
    }

}