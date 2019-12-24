
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
    [SpellScript(364)]
    public class ProduceFlame : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Produce Flame OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Produce Flame OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;

            var target = spell.Targets[0];

            target.Object.AddCondition("sp-Produce Flame", spell.spellId, spell.duration, 0);
            target.ParticleSystem = AttachParticles("sp-Produce Flame", target.Object);

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Produce Flame OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Produce Flame OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-Produce Flame-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Produce Flame OnEndProjectile");
            spell.caster.D20SendSignalEx(D20DispatcherKey.SIG_TouchAttack, spell.Targets[index_of_target].Object);
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Produce Flame OnEndSpellCast");
        }


    }
}
