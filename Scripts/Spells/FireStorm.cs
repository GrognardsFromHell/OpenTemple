
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

[SpellScript(174)]
public class FireStorm : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Fire Storm OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Fire Storm OnSpellEffect");
        var remove_list = new List<GameObject>();
        var damage_list = new List<GameObject>();
        var dam = Dice.D6;
        dam = dam.WithCount(Math.Min(20, spell.casterLevel));
        var (xx, yy) = spell.caster.GetLocation(); // caster is in chamber
        if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
        {
            // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
            AttachParticles("swirled gas", spell.caster);
            spell.caster.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
            Sound(7581, 1);
            Sound(7581, 1);
            spell.EndSpell();
            return;
        }

        foreach (var target_item in spell.Targets)
        {
            (xx, yy) = target_item.Object.GetLocation(); // target in chamber
            if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                var tro = GameSystems.MapObject.CreateObject(14070, target_item.Object.GetLocation());
                SpawnParticles("swirled gas", target_item.Object.GetLocation());
                // spell.caster.float_mesfile_line( 'mes\\skill_ui.mes', 2000 , 1 )
                tro.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                tro.Destroy();
                Sound(7581, 1);
                Sound(7581, 1);
                remove_list.Add(target_item.Object);
                continue;

            }
            else
            {
                // create a rectangle around target for two 10 x 10 ft. cubes (10 x 20 ft. area of effect)
                var (target_xx, target_yy) = target_item.Object.GetLocation();
                // generate location of the rectangle (centered on the target) and randomize the placement of it (since you can not place it)
                var a = RandomRange(1, 2);
                var b = RandomRange(1, 2);
                int targetX, targetY;
                if (a == 1)
                {
                    targetX = 5;
                    if (b == 1)
                    {
                        targetY = 7;
                    }
                    else
                    {
                        targetY = 10;
                    }

                }
                else
                {
                    targetY = 5;
                    if (b == 1)
                    {
                        targetX = 7;
                    }
                    else
                    {
                        targetX = 10;
                    }

                }

                SpawnParticles("sp-Flame Strike", new locXY(target_xx - (int)(targetX / 2), target_yy - (int)(targetY / 2)));
                SpawnParticles("sp-Flame Strike", new locXY(target_xx + (int)(targetX / 2), target_yy + (int)(targetY / 2)));
                // create damage list
                foreach (var critter in ObjList.ListVicinity(target_item.Object.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    (xx, yy) = critter.GetLocation();
                    if ((xx >= (target_xx - targetX) && xx <= (target_xx + targetX)) && (yy >= (target_yy - targetY) && yy <= (target_yy + targetY)))
                    {
                        if (critter.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                        {
                            // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                            critter.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                            AttachParticles("swirled gas", critter);
                            Sound(7581, 1);
                            Sound(7581, 1);
                            continue;

                        }
                        else
                        {
                            if (!(damage_list).Contains(critter))
                            {
                                damage_list.Add(critter);
                            }

                        }

                    }

                }

            }

            remove_list.Add(target_item.Object);
        }

        // deal damage
        foreach (var damage_target in damage_list)
        {
            if (!damage_target.D20Query(D20DispatcherKey.QUE_Dead))
            {
                AttachParticles("hit-FIRE-burst", damage_target);
                if (damage_target.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                {
                    // saving throw successful
                    damage_target.FloatMesFileLine("mes/spell.mes", 30001);
                }
                else
                {
                    // saving throw unsuccessful
                    damage_target.FloatMesFileLine("mes/spell.mes", 30002);
                }

            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Fire Storm OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Fire Storm OnEndSpellCast");
    }

}