
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

namespace Scripts.Spells
{
    [SpellScript(76)]
    public class Contagion : BaseSpellScript
    {
        private static readonly int[] diseases = {0, 0, 1, 4, 5, 7, 8, 9};
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Contagion OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Contagion OnSpellEffect");
            spell.duration = 0;
            var target_item = spell.Targets[0];
            var npc = spell.caster;
            var disease_index = spell.GetMenuArg(RadialMenuParam.MinSetting);
            // Solves Radial menu problem for Wands/NPCs
            if (disease_index != 1 && disease_index != 2 && disease_index != 3 && disease_index != 4 && disease_index != 5 && disease_index != 6 && disease_index != 7)
            {
                disease_index = RandomRange(1, 7);
            }

            if (((target_item.Object.GetStat(Stat.level_paladin) >= 3) && (!target_item.Object.D20Query(D20DispatcherKey.QUE_IsFallenPaladin))) || target_item.Object.IsMonsterCategory(MonsterCategory.construct) || target_item.Object.IsMonsterCategory(MonsterCategory.undead))
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 32000);
                AttachParticles("Fizzle", target_item.Object);
            }
            else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw unsuccesful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                // target_item.obj.condition_add_with_args( 'sp-Contagion', spell.id, spell.duration, disease_index )
                // target_item.obj.condition_add_with_args( 'NSDiseased', 1, disease_index - 1, 0 )
                target_item.Object.AddCondition("Incubating_Disease", 1, diseases[disease_index], 0);
                target_item.ParticleSystem = AttachParticles("ef-MinoCloud", target_item.Object);
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
            Logger.Info("Contagion OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Contagion OnEndSpellCast");
        }

    }
}
