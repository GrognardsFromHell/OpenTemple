
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(108)]
    public class Destruction : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Destruction OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Destruction OnSpellEffect");
            var dam = Dice.Parse("10d6");
            spell.duration = 0;
            // changed_con = 0
            var target_item = spell.Targets[0];
            // determine focus
            var focus = 0;
            if ((spell.caster.type != ObjectType.pc) && (spell.caster.GetLeader() == null))
            {
                // for NPCs not in game party
                focus = 1;
            }
            else
            {
                if (spell.caster.GetMoney() >= 50000)
                {
                    focus = 1;
                }

            }

            // check for focus
            if (focus == 1)
            {
                AttachParticles("sp-Disintegrate-Hit", target_item.Object);
                if ((target_item.Object.GetNameId() == 14629 || target_item.Object.GetNameId() == 14621 || target_item.Object.GetNameId() == 14604) && !Co8.is_spell_flag_set(target_item.Object, Co8SpellFlag.FlamingSphere))
                {
                    SpawnParticles("sp-Stoneskin", target_item.Object.GetLocation());
                    target_item.Object.Destroy();
                }
                else if (target_item.Object.IsMonsterCategory(MonsterCategory.construct) || target_item.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    AttachParticles("Fizzle", target_item.Object);
                }
                // if target_item.obj.stat_base_get(stat_constitution) < 0:
                // target_item.obj.stat_base_set(stat_constitution, 10)
                // changed_con = 1
                else if ((target_item.Object.type == ObjectType.pc) || (target_item.Object.type == ObjectType.npc))
                {
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target_item.Object.DealSpellDamage(spell.caster, DamageType.Force, dam, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        // So you'll get awarded XP for the kill
                        if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item.Object)))
                        {
                            target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                        }

                        target_item.Object.KillWithDeathEffect();
                        target_item.Object.AddCondition("sp-Animate Dead", spell.spellId, spell.duration, 3);
                        AttachParticles("sp-Stoneskin", target_item.Object);
                    }

                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                    AttachParticles("Fizzle", target_item.Object);
                }

            }
            else
            {
                // no focus
                spell.caster.FloatMesFileLine("mes/spell.mes", 16009);
            }

            // if changed_con == 1:
            // target_item.obj.stat_base_set(stat_constitution, -1)
            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Destruction OnEndSpellCast");
        }

    }
}
