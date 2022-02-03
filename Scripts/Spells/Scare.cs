
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

[SpellScript(408)]
public class Scare : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Scare OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Scare OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = spell.casterLevel;
        foreach (var target_item in spell.Targets)
        {
            // check for vermin
            if (target_item.Object.IsMonsterCategory(MonsterCategory.vermin))
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30019);
                target_item.Object.FloatMesFileLine("mes/bonus.mes", 319);
                remove_list.Add(target_item.Object);
            }
            // check target HD
            else if ((GameSystems.Critter.GetHitDiceNum(target_item.Object) < 6))
            {
                // allow Will saving throw to negate
                if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    // target is SHAKEN
                    target_item.Object.AddCondition("sp-Cause Fear", spell.spellId, 1, 1);
                    target_item.ParticleSystem = AttachParticles("sp-Scare", target_item.Object);
                }
                else
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // target is FRIGHTENED
                    target_item.Object.AddCondition("sp-Cause Fear", spell.spellId, spell.duration, 0);
                    target_item.ParticleSystem = AttachParticles("sp-Scare", target_item.Object);
                }

            }
            else
            {
                // target HD is 6 or more
                AttachParticles("Fizzle", target_item.Object);
                remove_list.Add(target_item.Object);
            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Scare OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Scare OnEndSpellCast");
    }

}