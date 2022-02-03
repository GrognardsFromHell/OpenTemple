
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

namespace Scripts.Spells;

[SpellScript(201)]
public class GreaterCommand : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Greater Command OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Greater Command OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = 1 * spell.casterLevel;
        // Solves Radial menu problem for Wands/NPCs
        var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
        if (spell_arg != 1 && spell_arg != 2 && spell_arg != 3 && spell_arg != 4)
        {
            spell_arg = 2;
        }

        var npc = spell.caster;
        if (npc.type != ObjectType.pc && npc.GetLeader() == null)
        {
            spell_arg = 2;
        }

        foreach (var target_item in spell.Targets)
        {
            if (!target_item.Object.IsFriendly(spell.caster))
            {
                if ((target_item.Object.type == ObjectType.pc) || (target_item.Object.type == ObjectType.npc))
                {
                    if (!target_item.Object.IsMonsterCategory(MonsterCategory.animal))
                    {
                        if (GameSystems.Stat.DispatchGetSizeCategory(target_item.Object) < SizeCategory.Large)
                        {
                            if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                            {
                                // saving throw unsuccessful
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                                target_item.Object.AddCondition("sp-Command", spell.spellId, spell.duration, spell_arg);
                                target_item.ParticleSystem = AttachParticles("sp-Command", target_item.Object);
                            }
                            else
                            {
                                // add target to initiative, just in case
                                // target_item.obj.add_to_initiative()
                                // game.update_combat_ui()
                                // saving throw successful
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                                AttachParticles("Fizzle", target_item.Object);
                                remove_list.Add(target_item.Object);
                            }

                        }
                        else
                        {
                            // not medium sized or smaller
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 31005);
                            AttachParticles("Fizzle", target_item.Object);
                            remove_list.Add(target_item.Object);
                        }

                    }
                    else
                    {
                        // a monster
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 31004);
                        AttachParticles("Fizzle", target_item.Object);
                        remove_list.Add(target_item.Object);
                    }

                }
                else
                {
                    // not a person
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31001);
                    AttachParticles("Fizzle", target_item.Object);
                    remove_list.Add(target_item.Object);
                }

            }
            else
            {
                // can't target friendlies
                AttachParticles("Fizzle", target_item.Object);
                remove_list.Add(target_item.Object);
            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Greater Command OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Greater Command OnEndSpellCast");
    }

}