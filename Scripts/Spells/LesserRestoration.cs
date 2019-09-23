
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
    [SpellScript(272)]
    public class LesserRestoration : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Lesser Restoration OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Lesser Restoration OnSpellEffect");
            spell.duration = 0;
            var target_item = spell.Targets[0];
            var abilityRestored = 1;
            var oldAbility = 0;
            // get the ability type (from radial menu, -1 to offset index and D20Strength == 0)
            var ability_type = (spell.GetMenuArg(RadialMenuParam.MinSetting) - 1);
            // Solves Radial menu problem for Wands/NPCs
            // if ability_type != 0 and ability_type != 1 and ability_type != 2 and ability_type != 3 and ability_type != 4 and ability_type != 5:
            // ability_type = game.random_range(1,6)
            // ability_type = ability_type - 1
            if (ability_type != 0 && ability_type != 1 && ability_type != 2 && ability_type != 3 && ability_type != 4 && ability_type != 5)
            {
                ability_type = 0;
                abilityRestored = 0;
            }

            // If no ability_type was selected, then that means that the spell used an item.  In this case, rather than picking a random item,
            // we cycle through all abilities until we find one that is actually healed.
            if (target_item.Object.IsFriendly(spell.caster))
            {
                target_item.ParticleSystem = AttachParticles("sp-Lesser Restoration", target_item.Object);
                if (abilityRestored == 1)
                {
                    target_item.Object.AddCondition("sp-Lesser Restoration", spell.spellId, spell.duration, ability_type);
                }
                else
                {
                    if (abilityRestored == 0)
                    {
                        ability_type = 0;
                        oldAbility = target_item.Object.GetStat(Stat.strength);
                        target_item.Object.AddCondition("sp-Lesser Restoration", spell.spellId, spell.duration, ability_type);
                        if (target_item.Object.GetStat(Stat.strength) > oldAbility)
                        {
                            abilityRestored = 1;
                        }

                    }

                    if (abilityRestored == 0)
                    {
                        ability_type = 1;
                        oldAbility = target_item.Object.GetStat(Stat.dexterity);
                        target_item.Object.AddCondition("sp-Lesser Restoration", spell.spellId, spell.duration, ability_type);
                        if (target_item.Object.GetStat(Stat.dexterity) > oldAbility)
                        {
                            abilityRestored = 1;
                        }

                    }

                    if (abilityRestored == 0)
                    {
                        ability_type = 2;
                        oldAbility = target_item.Object.GetStat(Stat.constitution);
                        target_item.Object.AddCondition("sp-Lesser Restoration", spell.spellId, spell.duration, ability_type);
                        if (target_item.Object.GetStat(Stat.constitution) > oldAbility)
                        {
                            abilityRestored = 1;
                        }

                    }

                    if (abilityRestored == 0)
                    {
                        ability_type = 3;
                        oldAbility = target_item.Object.GetStat(Stat.intelligence);
                        target_item.Object.AddCondition("sp-Lesser Restoration", spell.spellId, spell.duration, ability_type);
                        if (target_item.Object.GetStat(Stat.intelligence) > oldAbility)
                        {
                            abilityRestored = 1;
                        }

                    }

                    if (abilityRestored == 0)
                    {
                        ability_type = 4;
                        oldAbility = target_item.Object.GetStat(Stat.wisdom);
                        target_item.Object.AddCondition("sp-Lesser Restoration", spell.spellId, spell.duration, ability_type);
                        if (target_item.Object.GetStat(Stat.wisdom) > oldAbility)
                        {
                            abilityRestored = 1;
                        }

                    }

                    if (abilityRestored == 0)
                    {
                        ability_type = 5;
                        oldAbility = target_item.Object.GetStat(Stat.charisma);
                        target_item.Object.AddCondition("sp-Lesser Restoration", spell.spellId, spell.duration, ability_type);
                        if (target_item.Object.GetStat(Stat.charisma) > oldAbility)
                        {
                            abilityRestored = 1;
                        }

                    }

                }

            }
            else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw unsuccesful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.Object.AddCondition("sp-Lesser Restoration", spell.spellId, spell.duration, ability_type);
                target_item.ParticleSystem = AttachParticles("sp-Lesser Restoration", target_item.Object);
            }
            else
            {
                // saving throw successful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target_item.Object);
            }

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Lesser Restoration OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Lesser Restoration OnEndSpellCast");
        }

    }
}
