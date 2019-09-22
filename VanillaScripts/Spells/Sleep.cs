
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
    [SpellScript(438)]
    public class Sleep : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sleep OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
            spell.SortTargets(TargetListOrder.HitDiceThenDist, TargetListOrderDirection.Ascending);
            Logger.Info("target_list sorted by hitdice and dist from target_Loc (least to greatest): {0}", spell.Targets);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Sleep OnSpellEffect");
            var remove_list = new List<GameObjectBody>();

            spell.duration = 10 * spell.casterLevel;

            var hit_dice_max = 4;

            SpawnParticles("sp-Sleep", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item.Object);

                if ((obj_hit_dice < 5) && (hit_dice_max >= obj_hit_dice))
                {
                    hit_dice_max = hit_dice_max - obj_hit_dice;

                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item.Object);
                        remove_list.Add(target_item.Object);
                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.AddCondition("sp-Sleep", spell.spellId, spell.duration, 0);
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
            Logger.Info("Sleep OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sleep OnEndSpellCast");
        }


    }
}
