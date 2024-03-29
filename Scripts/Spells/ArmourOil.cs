
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

[SpellScript(790)]
public class ArmourOil : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Armour Oil OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Armour Oil OnSpellEffect");
        var target = spell.Targets[0];
        if (target.Object.type == ObjectType.armor)
        {
            if ((target.Object.GetInt(obj_f.item_wear_flags) == 32))
            {
                if (target.Object.GetMaterial() == Material.metal)
                {
                    if (target.Object.GetInt(obj_f.armor_pad_i_1) == 0)
                    {
                        var x = target.Object.GetInt(obj_f.armor_armor_check_penalty);
                        x = x + 1;
                        target.Object.SetInt(obj_f.armor_armor_check_penalty, x);
                        target.Object.SetInt(obj_f.armor_pad_i_1, 1);
                        target.ParticleSystem = AttachParticles("sp-Knock", spell.caster);
                    }
                    else
                    {
                        spell.caster.FloatMesFileLine("mes/spell.mes", 16012);
                    }

                }
                else
                {
                    spell.caster.FloatMesFileLine("mes/spell.mes", 31010);
                }

            }
            else
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 16013);
            }

        }
        else
        {
            AttachParticles("Fizzle", target.Object);
            spell.caster.FloatMesFileLine("mes/spell.mes", 16013);
        }

        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Armour Oil OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Armour Oil OnEndSpellCast");
    }

}