
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
    [SpellScript(771)]
    public class RayOfEntropy : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Ray of Entropy OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Ray of Entropy OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Ray of Entropy OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
        {
            Logger.Info("Ray of Entropy OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Ray of Enfeeblement', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Ray of Enfeeblement", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
        {
            Logger.Info("Ray of Entropy OnEndProjectile");
            var target_item = spell.Targets[0];
            var dice = Dice.Parse("4d1");
            var dam_amount = dice.Roll();
            dam_amount = -dam_amount;
            Logger.Info("amount={0}", dam_amount);
            spell.duration = 10 * spell.casterLevel;
            EndProjectileParticles(projectile);
            
            if ((spell.caster.PerformTouchAttack(target_item.Object) & D20CAF.HIT) != D20CAF.NONE)
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 20022, TextFloaterColor.Red);
                target_item.Object.AddCondition("sp-Cats Grace", spell.spellId, spell.duration, dam_amount);
                target_item.Object.AddCondition("sp-Bulls Strength", spell.spellId, spell.duration, dam_amount);
                target_item.Object.AddCondition("sp-Endurance", spell.spellId, spell.duration, dam_amount);
                target_item.ParticleSystem = AttachParticles("sp-Ray of Enfeeblement-Hit", target_item.Object);
            }
            else
            {
                // missed
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Ray of Entropy OnEndSpellCast");
        }

    }
}
