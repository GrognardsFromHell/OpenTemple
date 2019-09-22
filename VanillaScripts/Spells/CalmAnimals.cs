
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(47)]
    public class CalmAnimals : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Calm Animals OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Calm Animals OnSpellEffect");
            var dice = Dice.Parse("2d4");

            var hd_remaining = dice.Roll();

            hd_remaining = hd_remaining + spell.casterLevel;

            var remove_list = new List<GameObjectBody>();

            spell.duration = 60 * spell.casterLevel;

            SpawnParticles("sp-Calm Animals", spell.aoeCenter);
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
                        target_item.Object.AddCondition("sp-Calm Animals", spell.spellId, spell.duration, 0);
                        target_item.ParticleSystem = AttachParticles("sp-Calm Animals-HIT", target_item.Object);

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
            Logger.Info("Calm Animals OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Calm Animals OnEndSpellCast");
        }


    }
}
