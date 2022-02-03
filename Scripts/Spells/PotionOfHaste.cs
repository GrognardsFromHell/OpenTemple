
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

[SpellScript(702)]
public class PotionOfHaste : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Potion of Haste OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Potion of Haste OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = 1 * spell.casterLevel;
        var npc = spell.caster; // added so NPC's can use potion
        if (npc.type != ObjectType.pc && npc.GetLeader() == null)
        {
            spell.duration = 10;
            spell.casterLevel = 10;
        }

        Logger.Info("spell.duration = {0}", spell.duration);
        foreach (var target_item in spell.Targets)
        {
            if (target_item.Object.IsFriendly(spell.caster))
            {
                var return_val = target_item.Object.AddCondition("sp-Haste", spell.spellId, spell.duration, 1);
                target_item.ParticleSystem = AttachParticles("sp-Haste", target_item.Object);
                // dont allow multiple adds (WIP! - until intgame select prevents MULTI (same target)
                if (!return_val)
                {
                    remove_list.Add(target_item.Object);
                }

            }
            else
            {
                if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    var return_val = target_item.Object.AddCondition("sp-Potion of Haste", spell.spellId, spell.duration, 1);
                    target_item.ParticleSystem = AttachParticles("sp-Haste", target_item.Object);
                    // dont allow multiple adds (WIP! - until intgame select prevents MULTI (same target)
                    if (!return_val)
                    {
                        remove_list.Add(target_item.Object);
                    }

                }
                else
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target_item.Object);
                    remove_list.Add(target_item.Object);
                }

            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Potion of Haste OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Potion of Haste OnEndSpellCast");
    }

}