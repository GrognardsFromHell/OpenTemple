
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
using System.Numerics;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(123)]
    public class DimensionDoor : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dimension Door OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Dimension Door OnSpellEffect");
            AttachParticles("sp-Dimension Door", spell.caster);
            var target = spell.caster;
            if (spell.caster.GetNameId() == 8042 && spell.caster.GetMap() == 5121) // Iuz teleport in Verbobonc
            {
                var stop_condition = 0;
                var target_loc = Utilities.party_closest(spell.caster, exclude_warded:true).GetLocation();
                var (xx_o, yy_o) = target_loc;
                var xx = xx_o;
                var yy = yy_o;
                var attempt_count = 0;
                while (stop_condition == 0 && attempt_count < 15)
                {
                    attempt_count += 1;
                    var loc_list = new List<locXY>();
                    foreach (var obj in ObjList.ListVicinity(target_loc, ObjectListFilter.OLC_NPC | ObjectListFilter.OLC_PC))
                    {
                        if (!obj.IsUnconscious())
                        {
                            loc_list.Add(obj.GetLocation());
                        }

                    }

                    if (dist_from_set(xx, yy, loc_list) < 2 * locXY.INCH_PER_TILE || (xx >= 516 && xx <= 528 && yy >= 629 && yy <= 641))
                    {
                        xx = xx_o + RandomRange(-4, 3);
                        yy = yy_o + RandomRange(-4, 3);
                    }
                    else if (attempt_count >= 15)
                    {
                        xx = xx_o;
                        yy = yy_o;
                    }
                    else
                    {
                        // game.global_vars[498] = (xx_o - xx)**2 + 1000* (yy_o-yy)**2
                        // game.global_vars[499] = attempt_count
                        stop_condition = 1;
                    }

                }

                target_loc = new locXY(xx, yy);
                target.FadeTo(0, 10, 40);
                // WIP! SMM: added timeevent to trigger fadein (in realtime)
                AttachParticles("sp-Dimension Door", target);
                var target_loc_precise = new LocAndOffsets(target_loc);
                StartTimer(750, () => fade_back_in(target, target_loc_precise, spell), true);
            }
            else if (!target.HasCondition(SpellEffects.SpellDimensionalAnchor))
            {
                target.FadeTo(0, 10, 40);
                // WIP! SMM: added timeevent to trigger fadein (in realtime)
                AttachParticles("sp-Dimension Door", target);
                StartTimer(750, () => fade_back_in(target, spell.aoeCenter, spell), true);
            }
            else
            {
                target.FloatMesFileLine("mes/spell.mes", 30011);
                AttachParticles("Fizzle", target);
                spell.RemoveTarget(target);
                spell.EndSpell();
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Dimension Door OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dimension Door OnEndSpellCast");
        }
        public static void fade_back_in(GameObject target, LocAndOffsets loc, SpellPacketBody spell)
        {
            target.Move(loc);
            AttachParticles("sp-Dimension Door", target);
            target.FadeTo(255, 10, 5);
            spell.RemoveTarget(target);
            spell.EndSpell();
            target.SetInt(obj_f.critter_strategy, 453);
        }
        public static float dist_from_set(int xx, int yy, IEnumerable<locXY> loc_list)
        {
            var dist_min = 100000.0f;
            var loc = new locXY(xx, yy).ToInches2D();
            foreach (var loc_tup in loc_list)
            {
                var dist_xy = Vector2.DistanceSquared(loc, loc_tup.ToInches2D());
                if (dist_xy < dist_min)
                {
                    dist_min = dist_xy;
                }

            }

            return dist_min;
        }

    }
}
