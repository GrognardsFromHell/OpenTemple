
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells;

[SpellScript(275)]
public class LightningBolt : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Lightning Bolt OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Lightning Bolt OnSpellEffect");
        var remove_list = new List<GameObject>();

        var damage_dice = Dice.D6;

        damage_dice = damage_dice.WithCount(Math.Min(1 * spell.casterLevel, 10));
        SpawnParticles("sp-Lightning Bolt", spell.aoeCenter);
        GameSystems.Vfx.LightningBolt(spell.caster, spell.aoeCenter);
        foreach (var target_item in spell.Targets)
        {
            if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
            }

            remove_list.Add(target_item.Object);
        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Lightning Bolt OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Lightning Bolt OnEndSpellCast");
    }


}