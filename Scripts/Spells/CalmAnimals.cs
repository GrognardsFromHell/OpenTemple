
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

namespace Scripts.Spells
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
            var remove_list = new List<GameObject>();
            var dice = Dice.Parse("2d4");
            var hd_remaining = dice.Roll();
            hd_remaining = hd_remaining + spell.casterLevel;
            spell.duration = 10 * spell.casterLevel;
            // dire animal list includes 2 Dire Rats, Dire Bat, Dire Wolf, 2 Dire Lizards, 2 Dire Bears, 2 Dire Boars
            var dire_list = new[] { 14056, 14390, 14391, 14450, 14506, 14507, 14978, 14979, 14981, 14998 };
            var dire_animal = 0;
            SpawnParticles("sp-Calm Animals", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                // check for dire animals
                foreach (var target in dire_list)
                {
                    if (target == target_item.Object.GetNameId())
                    {
                        dire_animal = 1;
                    }

                }

                var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item.Object);
                if ((obj_hit_dice <= hd_remaining) && target_item.Object.IsMonsterCategory(MonsterCategory.animal) && ((target_item.Object.GetStat(Stat.intelligence) == 1) || (target_item.Object.GetStat(Stat.intelligence) == 2)))
                {
                    hd_remaining = hd_remaining - obj_hit_dice;
                    // dire animals are allowed a saving throw; other animals are not
                    if (dire_animal == 1)
                    {
                        if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            // saving throw successful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            AttachParticles("Fizzle", target_item.Object);
                            remove_list.Add(target_item.Object);
                        }
                        else
                        {
                            // saving throw unsuccessful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                            target_item.Object.AddCondition("sp-Calm Animals", spell.spellId, spell.duration, 0);
                            target_item.ParticleSystem = AttachParticles("sp-Calm Animals-HIT", target_item.Object);
                        }

                    }
                    else
                    {
                        // not a dire animal
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
