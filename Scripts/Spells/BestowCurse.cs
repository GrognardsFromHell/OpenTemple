
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
    [SpellScript(28)]
    public class BestowCurse : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Bestow Curse OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Bestow Curse OnSpellEffect");
            spell.duration = 0;
            var target_item = spell.Targets[0];
            // Solves Radial menu problem for Wands/NPCs
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            if (spell_arg != 1 && spell_arg != 2 && spell_arg != 3 && spell_arg != 4 && spell_arg != 5 && spell_arg != 6 && spell_arg != 7 && spell_arg != 8)
            {
                spell_arg = RandomRange(1, 8);
            }

            var npc = spell.caster;
            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
            {
                spell_arg = 8;
            }

            // allow Willpower saving throw to negate
            if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw successful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }
            else
            {
                // saving throw unsuccessful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.ParticleSystem = AttachParticles("sp-Bestow Curse", target_item.Object);
                if (spell_arg == 1)
                {
                    // print "str"
                    // apply ability damage str
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 0);
                }
                else if (spell_arg == 2)
                {
                    // apply ability damage dex
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 1);
                }
                else if (spell_arg == 3)
                {
                    // apply ability damage con
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 2);
                }
                else if (spell_arg == 4)
                {
                    // apply ability damage int
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 3);
                }
                else if (spell_arg == 5)
                {
                    // apply ability damage wis
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 4);
                }
                else if (spell_arg == 6)
                {
                    // apply ability damage cha
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 5);
                }
                else if (spell_arg == 7)
                {
                    // apply curse rolls/checks
                    target_item.Object.AddCondition("sp-Bestow Curse Rolls", spell.spellId, spell.duration, 6);
                }
                else if (spell_arg == 8)
                {
                    // apply curse actions
                    target_item.Object.AddCondition("sp-Bestow Curse Actions", spell.spellId, spell.duration, 7);
                }

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Bestow Curse OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Bestow Curse OnEndSpellCast");
        }

    }
}
