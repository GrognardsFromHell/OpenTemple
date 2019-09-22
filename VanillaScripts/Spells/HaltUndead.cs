
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

namespace VanillaScripts
{
    [SpellScript(217)]
    public class HaltUndead : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Halt Undead OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Halt Undead OnSpellEffect");
            var remove_list = new List<GameObjectBody>();

            spell.duration = 1 * spell.casterLevel;

            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    if (target_item.Object.GetStat(Stat.intelligence) > 0)
                    {
                        if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                            target_item.Object.AddCondition("sp-Halt Undead", spell.spellId, spell.duration, 0);
                            target_item.ParticleSystem = AttachParticles("sp-Halt Undead", target_item.Object);

                        }
                        else
                        {
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            AttachParticles("Fizzle", target_item.Object);
                            remove_list.Add(target_item.Object);
                        }

                    }
                    else
                    {
                        target_item.Object.AddCondition("sp-Halt Undead", spell.spellId, spell.duration, 0);
                        target_item.ParticleSystem = AttachParticles("sp-Halt Undead", target_item.Object);

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
            Logger.Info("Halt Undead OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Halt Undead OnEndSpellCast");
        }


    }
}
