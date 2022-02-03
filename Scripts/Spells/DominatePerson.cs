
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

[SpellScript(141)]
public class DominatePerson : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Dominate Person OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Dominate Person OnSpellEffect");
        spell.duration = 14400 * spell.casterLevel;
        var target_item = spell.Targets[0];
        if (!target_item.Object.IsFriendly(spell.caster))
        {
            if ((target_item.Object.IsMonsterCategory(MonsterCategory.humanoid)))
            {
                if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    target_item.Object.AddCondition("sp-Dominate Person", spell.spellId, spell.duration, GameSystems.Critter.GetHitDiceNum(target_item.Object));
                    target_item.ParticleSystem = AttachParticles("sp-Dominate Person", target_item.Object);
                    // add target to initiative, just in case
                    target_item.Object.AddToInitiative();
                    UiSystems.Combat.Initiative.UpdateIfNeeded();
                }
                else
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }

            }
            else
            {
                // not an humanoid
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target_item.Object.FloatMesFileLine("mes/spell.mes", 31004);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

        }
        else
        {
            // can't target friendlies
            AttachParticles("Fizzle", target_item.Object);
            spell.RemoveTarget(target_item.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Dominate Person OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Dominate Person OnEndSpellCast");
    }

}