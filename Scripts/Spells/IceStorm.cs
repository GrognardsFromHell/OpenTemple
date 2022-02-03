
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

[SpellScript(237)]
public class IceStorm : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Ice Storm OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Ice Storm OnSpellEffect");
        spell.duration = 0;
        // spawn one spell_object object
        var spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
        // add to d20initiative
        var caster_init_value = spell.caster.GetInitiative();
        spell_obj.InitD20Status();
        spell_obj.SetInitiative(caster_init_value);
        // put sp-Ice Storm condition on obj
        var spell_obj_partsys_id = AttachParticles("sp-Ice Storm", spell_obj);
        spell_obj.AddCondition("sp-Ice Storm", spell.spellId, spell.duration, 0, spell_obj_partsys_id);
    }
    // spell_obj.condition_add_arg_x( 3, spell_obj_partsys_id )
    // objectevent_id = spell_obj.condition_get_arg_x( 2 )

    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Ice Storm OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Ice Storm OnEndSpellCast");
    }
    public override void OnAreaOfEffectHit(SpellPacketBody spell)
    {
        Logger.Info("Ice Storm OnAreaOfEffectHit");
    }
    public override void OnSpellStruck(SpellPacketBody spell)
    {
        Logger.Info("Ice Storm OnSpellStruck");
    }

}