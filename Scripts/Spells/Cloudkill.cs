
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

[SpellScript(65)]
public class Cloudkill : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Cloudkill OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Cloudkill OnSpellEffect");
        spell.duration = 10 * spell.casterLevel;
        // added so you'll get awarded XP for the kill
        foreach (var target_item in spell.Targets)
        {
            if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item.Object)))
            {
                if ((target_item.Object.GetObjectFlags() & ObjectFlag.INVULNERABLE) == 0)
                {
                    target_item.Object.SetObjectFlag(ObjectFlag.INVULNERABLE);
                    target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                    target_item.Object.ClearObjectFlag(ObjectFlag.INVULNERABLE);
                }
                else
                {
                    target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                }

            }

        }

        // spawn one Cloudkill scenery object
        var cloudkill_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
        // add to d20initiative
        var caster_init_value = spell.caster.GetInitiative();
        cloudkill_obj.InitD20Status();
        cloudkill_obj.SetInitiative(caster_init_value);
        // put sp-cloudkill_obj condition on obj
        var cloudkill_obj_partsys_id = AttachParticles("sp-Cloudkill", cloudkill_obj);
        cloudkill_obj.AddCondition("sp-Cloudkill", spell.spellId, spell.duration, 0, cloudkill_obj_partsys_id);
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Cloudkill OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Cloudkill OnEndSpellCast");
    }
    public override void OnAreaOfEffectHit(SpellPacketBody spell)
    {
        Logger.Info("Cloudkill OnAreaOfEffectHit");
    }
    public override void OnSpellStruck(SpellPacketBody spell)
    {
        Logger.Info("Cloudkill OnSpellStruck");
    }

}