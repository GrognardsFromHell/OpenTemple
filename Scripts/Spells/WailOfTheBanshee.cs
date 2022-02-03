
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

[SpellScript(521)]
public class WailOfTheBanshee : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Wail of the Banshee OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
        // Sorts the targets by distance, so closest are affected first
        spell.SortTargets(TargetListOrder.Dist, TargetListOrderDirection.Ascending);
        Logger.Info("target_list sorted by dist from target_Loc (least to greatest): {0}", spell.Targets);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Wail of the Banshee OnSpellEffect");
        var remove_list = new List<GameObject>();
        var banshee_targets = spell.casterLevel;
        AttachParticles("sp-Shout", spell.caster);
        SpawnParticles("sp-Dispel Magic - Area", spell.aoeCenter);
        foreach (var target_item in spell.Targets)
        {
            if (banshee_targets > 0)
            {
                // make sure target is alive
                if (target_item.Object.IsMonsterCategory(MonsterCategory.construct) || target_item.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    // not alive
                    AttachParticles("Fizzle", target_item.Object);
                }
                else
                {
                    // allow Fortitude saving throw to negate
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw successful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item.Object);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        // kill target
                        AttachParticles("sp-Shout-Hit", target_item.Object);
                        AttachParticles("sp-Death Knell-Target", target_item.Object);
                        // So you'll get awarded XP for the kill
                        if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item.Object)))
                        {
                            target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                        }

                        target_item.Object.Kill();
                    }

                    banshee_targets = banshee_targets - 1;
                }

            }

            remove_list.Add(target_item.Object);
        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Wail of the Banshee OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Wail of the Banshee OnEndSpellCast");
    }

}