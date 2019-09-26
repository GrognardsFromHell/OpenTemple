
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
    [SpellScript(40)]
    public class BlindnessDeafness : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Blindness/Deafness OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Blindness/Deafness OnSpellEffect");
            spell.duration = 14400;
            var target_item = spell.Targets[0];
            // Solves Radial menu problem for Wands/NPCs
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            if (spell_arg != 1 && spell_arg != 2)
            {
                spell_arg = RandomRange(1, 2);
            }

            var npc = spell.caster;
            if (npc.GetNameId() == 14609) // special for Zuggtmoy Priest
            {
                spell_arg = 2;
            }

            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
            {
                if (!Utilities.critter_is_unconscious(target_item.Object) && !target_item.Object.D20Query(D20DispatcherKey.QUE_Prone) && (target_item.Object.GetStat(Stat.level_wizard) >= 3 || target_item.Object.GetStat(Stat.level_sorcerer) >= 3 || target_item.Object.GetStat(Stat.level_bard) >= 3))
                {
                    npc = spell.caster;
                }
                else
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (!Utilities.critter_is_unconscious(obj) && !obj.D20Query(D20DispatcherKey.QUE_Prone) && (obj.GetStat(Stat.level_wizard) >= 3 || obj.GetStat(Stat.level_sorcerer) >= 3 || obj.GetStat(Stat.level_bard) >= 3))
                        {
                            target_item.Object = obj;
                        }

                    }

                }

            }

            // allow Fortitude saving throw to negate
            if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
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
                if (spell_arg == 1)
                {
                    // apply blindness
                    var return_val = target_item.Object.AddCondition("sp-Blindness", spell.spellId, spell.duration, 0);
                    if (return_val)
                    {
                        target_item.ParticleSystem = AttachParticles("sp-Blindness-Deafness", target_item.Object);
                    }

                }
                else
                {
                    // apply deafness
                    var return_val = target_item.Object.AddCondition("sp-Deafness", spell.spellId, spell.duration, 0);
                    if (return_val)
                    {
                        target_item.ParticleSystem = AttachParticles("sp-Blindness-Deafness", target_item.Object);
                    }

                }

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Blindness/Deafness OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Blindness/Deafness OnEndSpellCast");
        }

    }
}
