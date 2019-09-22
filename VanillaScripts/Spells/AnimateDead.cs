
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
    [SpellScript(11)]
    public class AnimateDead : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Animate Dead OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Animate Dead OnSpellEffect");
            spell.duration = 0;

            var target_item = spell.Targets[0];

            if ((target_item.Object.IsMonsterCategory(MonsterCategory.humanoid) || target_item.Object.IsMonsterCategory(MonsterCategory.fey) || target_item.Object.IsMonsterCategory(MonsterCategory.giant) || target_item.Object.IsMonsterCategory(MonsterCategory.monstrous_humanoid)))
            {
                target_item.Object.AddCondition("sp-Animate Dead", spell.spellId, spell.duration, spell.GetMenuArg(RadialMenuParam.MinSetting));
                target_item.ParticleSystem = AttachParticles("sp-Animate Dead", target_item.Object);

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target_item.Object.FloatMesFileLine("mes/spell.mes", 31009);
                AttachParticles("Fizzle", target_item.Object);
            }

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Animate Dead OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Animate Dead OnEndSpellCast");
        }


    }
}
