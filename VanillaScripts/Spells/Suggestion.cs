
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
    [SpellScript(466)]
    public class Suggestion : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Suggestion OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Suggestion OnSpellEffect");
            spell.duration = 600 * spell.casterLevel;

            var target_item = spell.Targets[0];

            if (!target_item.Object.IsFriendly(spell.caster))
            {
                if ((target_item.Object.type == ObjectType.pc) || (target_item.Object.type == ObjectType.npc))
                {
                    if (!target_item.Object.IsMonsterCategory(MonsterCategory.animal))
                    {
                        if (GameSystems.Stat.DispatchGetSizeCategory(target_item.Object) < SizeCategory.Huge)
                        {
                            if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                            {
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                                spell.caster.AddAIFollower(target_item.Object);
                                target_item.Object.AddCondition("sp-Suggestion", spell.spellId, spell.duration, 0);
                                target_item.ParticleSystem = AttachParticles("sp-Suggestion", target_item.Object);

                                target_item.Object.AddToInitiative();
                                UiSystems.Combat.Initiative.UpdateIfNeeded();
                            }
                            else
                            {
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                                AttachParticles("Fizzle", target_item.Object);
                                spell.RemoveTarget(target_item.Object);
                            }

                        }
                        else
                        {
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 31005);
                            AttachParticles("Fizzle", target_item.Object);
                            spell.RemoveTarget(target_item.Object);
                        }

                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 31004);
                        AttachParticles("Fizzle", target_item.Object);
                        spell.RemoveTarget(target_item.Object);
                    }

                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31001);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }

            }
            else
            {
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Suggestion OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Suggestion OnEndSpellCast");
        }


    }
}