
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

[SpellScript(313)]
public class Miracle : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Miracle OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        foreach (var target_item in spell.Targets)
        {
            if (!target_item.Object.IsFriendly(spell.caster))
            {
                spell.RemoveTarget(target_item.Object);
            }

        }

    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Miracle OnSpellEffect");
        spell.duration = 0;
        foreach (var target_item in spell.Targets)
        {
            if (target_item.Object.IsFriendly(spell.caster))
            {
                if (target_item.Object.GetStat(Stat.hp_current) <= -10)
                {
                    target_item.Object.AddCondition("sp-Raise Dead", spell.spellId, spell.duration, 0);
                }
                else
                {
                    target_item.Object.AddCondition("sp-Heal", spell.spellId, spell.duration, 0);
                    AttachParticles("sp-Cure Critical Wounds", target_item.Object);
                }

            }

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Miracle OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Miracle OnEndSpellCast");
    }

}