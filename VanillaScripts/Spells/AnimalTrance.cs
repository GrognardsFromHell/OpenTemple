
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

namespace VanillaScripts.Spells
{
    [SpellScript(10)]
    public class AnimalTrance : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Animal Trance OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
            spell.SortTargets(TargetListOrder.DistFromCaster, TargetListOrderDirection.Ascending);
            Logger.Info("target_list sorted by dist from [{0}] (closest to farthest): {1}", spell.caster, spell.Targets);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Animal Trance OnSpellEffect");
            var dice = Dice.Parse("2d6");

            var hd_remaining = dice.Roll();

            var remove_list = new List<GameObject>();

            spell.duration = 0;

            spell.casterPartSys = AttachParticles("sp-Animal Trance", spell.caster);

            foreach (var target_item in spell.Targets)
            {
                var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item.Object);

                if ((target_item.Object.IsMonsterCategory(MonsterCategory.animal)) && (target_item.Object.GetStat(Stat.intelligence) < 3) && (obj_hit_dice <= hd_remaining))
                {
                    hd_remaining = hd_remaining - obj_hit_dice;

                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item.Object);
                        remove_list.Add(target_item.Object);
                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.AddCondition("sp-Animal Trance", spell.spellId, spell.duration, 0);
                    }

                }
                else
                {
                    AttachParticles("Fizzle", target_item.Object);
                    remove_list.Add(target_item.Object);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Animal Trance OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Animal Trance OnEndSpellCast");
        }


    }
}
