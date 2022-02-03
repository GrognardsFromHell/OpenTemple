
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

[SpellScript(22)]
public class Atonement : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Atonement OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Atonement OnSpellEffect");
        spell.duration = 0;
        var XP_cost = 500;
        var target_item = spell.Targets[0];
        var caster_deity = spell.caster.GetDeity();
        var deity_command = 0;
        var caster_XP = spell.caster.GetStat(Stat.experience);
        var caster_level = spell.caster.GetStat(Stat.level);
        // fix for spell caster NOT in game party
        if ((spell.caster.type != ObjectType.pc) && (spell.caster.GetLeader() == null))
        {
            XP_cost = 0;
            caster_XP = 0;
            caster_level = 1;
        }

        // check if spell caster has enough XP
        if (has_enoughXP(caster_XP, caster_level, XP_cost))
        {
            // check if target is among the living
            if ((!target_item.Object.D20Query(D20DispatcherKey.QUE_Dead)) && (!target_item.Object.IsMonsterCategory(MonsterCategory.undead) || !target_item.Object.IsMonsterCategory(MonsterCategory.construct)))
            {
                // check if target is a paladin
                if (target_item.Object.GetStat(Stat.level_paladin) >= 1)
                {
                    // check if paladin has fallen
                    if (target_item.Object.D20Query(D20DispatcherKey.QUE_IsFallenPaladin))
                    {
                        // check deity
                        if (GameSystems.Deity.AllowsAtonement(caster_deity))
                        {
                            var text = GameSystems.Deity.GetPrayerHeardMessage(caster_deity);
                            target_item.Object.FloatLine(text, TextFloaterColor.LightBlue);
                            AttachParticles("Paladin-Atoned", target_item.Object);
                            target_item.Object.AtoneFallenPaladin();
                            if ((spell.caster.type == ObjectType.pc) || (spell.caster.GetLeader() != null))
                            {
                                spell.caster.SetBaseStat(Stat.experience, caster_XP - XP_cost);
                            }
                        }
                        else
                        {
                            if (caster_deity == DeityId.NONE)
                            {
                                target_item.Object.FloatMesFileLine("mes/item.mes", 201, TextFloaterColor.Green);
                            }
                            else
                            {
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30019);
                                target_item.Object.FloatMesFileLine("mes/deity.mes", (int) caster_deity, TextFloaterColor.Red);
                            }

                        }

                    }
                    else
                    {
                        // not fallen
                        AttachParticles("Fizzle", target_item.Object);
                    }

                }
                else
                {
                    // target is not a paladin
                    AttachParticles("Fizzle", target_item.Object);
                }

            }
            else
            {
                // target is not among the living
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                AttachParticles("Fizzle", target_item.Object);
            }

        }
        else
        {
            // not enough XP
            spell.caster.FloatMesFileLine("mes/action.mes", 1019);
        }

        spell.RemoveTarget(target_item.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Atonement OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Atonement OnEndSpellCast");
    }

    private bool has_enoughXP(int caster_XP, int caster_level, int XP_cost)
    {
        Logger.Info("has_enoughXP");
        var XP = 0;
        var level = 1;
        while ((level <= caster_level))
        {
            XP = ((level - 1) * 1000) + XP;
            if (level == caster_level)
            {
                if ((caster_XP >= (XP + XP_cost)))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            level = level + 1;
        }

        return false;
    }

}