
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
    [SpellScript(28)]
    public class BestowCurse : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Bestow Curse OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Bestow Curse OnSpellEffect");
            spell.duration = 0;

            var target_item = spell.Targets[0];

            if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.ParticleSystem = AttachParticles("sp-Bestow Curse", target_item.Object);

                if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 1)
                {
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 0);
                }
                else if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 2)
                {
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 1);
                }
                else if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 3)
                {
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 2);
                }
                else if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 4)
                {
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 3);
                }
                else if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 5)
                {
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 4);
                }
                else if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 6)
                {
                    target_item.Object.AddCondition("sp-Bestow Curse Ability", spell.spellId, spell.duration, 5);
                }
                else if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 7)
                {
                    target_item.Object.AddCondition("sp-Bestow Curse Rolls", spell.spellId, spell.duration, 6);
                }
                else if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 8)
                {
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
