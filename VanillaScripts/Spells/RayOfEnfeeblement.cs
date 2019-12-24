
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(383)]
    public class RayOfEnfeeblement : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Ray of Enfeeblement OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Ray of Enfeeblement OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Ray of Enfeeblement OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Ray of Enfeeblement OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-Ray of Enfeeblement", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Ray of Enfeeblement OnEndProjectile");
            var dice = Dice.D6;

            var dam_amount = dice.Roll();

            dam_amount += (spell.casterLevel / 2);
            Logger.Info("amount={0}", dam_amount);
            spell.duration = 10 * spell.casterLevel;

            EndProjectileParticles(projectile);
            var target_item = spell.Targets[0];

            if (spell.caster.PerformTouchAttack(target_item.Object) != 0)
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 20022, TextFloaterColor.Red);
                target_item.Object.AddCondition("sp-Ray of Enfeeblement", spell.spellId, spell.duration, dam_amount);
                target_item.ParticleSystem = AttachParticles("sp-Ray of Enfeeblement-Hit", target_item.Object);

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Ray of Enfeeblement OnEndSpellCast");
        }


    }
}
