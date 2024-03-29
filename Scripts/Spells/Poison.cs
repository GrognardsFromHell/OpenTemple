
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

[SpellScript(352)]
public class Poison : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Poison OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Poison OnSpellEffect");
        // 20 == SPELL_POISON, 10 == 1 minute til secondary damage
        var poison_index = 23;
        var time_to_secondary = 10;
        var poison_dc = 10 + (spell.casterLevel / 2) + spell.caster.GetStat(Stat.wis_mod);
        // print "poison-dc=", poison_dc
        spell.duration = 0;
        var target_item = spell.Targets[0];
        // if not target_item.obj.saving_throw_spell( spell.dc, D20_Save_Fortitude, D20STD_F_NONE, spell.caster, spell.id ):
        // saving throw unsuccesful
        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
        // target_item.obj.condition_add_with_args( 'sp-Poison', spell.id, spell.duration, disease_index )
        target_item.Object.AddCondition("Poisoned", poison_index, time_to_secondary, poison_dc);
        target_item.ParticleSystem = AttachParticles("sp-Poison", target_item.Object);
        // else:
        // saving throw successful
        // target_item.obj.float_mesfile_line( 'mes\\spell.mes', 30001 )
        // game.particles( 'Fizzle', target_item.obj )
        spell.RemoveTarget(target_item.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Poison OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Poison OnEndSpellCast");
    }

}