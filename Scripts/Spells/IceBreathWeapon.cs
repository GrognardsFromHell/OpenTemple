
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

[SpellScript(741)]
public class IceBreathWeapon : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Frozen Breath OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Frozen Breath OnSpellEffect");
        var remove_list = new List<GameObject>();
        var dam = Dice.D6;
        dam = dam.WithCount(spell.spellKnownSlotLevel);
        if (dam.Count > 6)
        {
            dam = dam.WithCount(6);
        }

        AttachParticles("sp-Cone of Cold", spell.caster);
        var npc = spell.caster;
        spell.dc = spell.dc + 5;
        if (npc.GetNameId() == 14999) // Old White Dragon
        {
            dam = dam.WithCount(8);
            spell.dc = 27;
        }

        // range = 25 + 5 * int(spell.caster_level/2)
        var range = 60;
        var target_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, range, -30, 60);
        foreach (var obj in target_list)
        {
            if (obj == spell.caster)
            {
                continue;
            }
            if (obj.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Cold, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
            {
                // saving throw successful
                obj.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                // saving throw unsuccessful
                obj.FloatMesFileLine("mes/spell.mes", 30002);
            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Frozen Breath OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Frozen Breath OnEndSpellCast");
    }

}