using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.Script
{
    public abstract class BaseSpellScript
    {
        protected readonly ILogger Logger = LoggingSystem.CreateLogger();


        public virtual void OnSpellEffect(SpellPacketBody spell)
        {
        }

        public virtual void OnBeginSpellCast(SpellPacketBody spell)
        {
        }

        public virtual void OnEndSpellCast(SpellPacketBody spell)
        {
        }

        public virtual void OnBeginRound(SpellPacketBody spell)
        {
        }

        public virtual void OnEndRound(SpellPacketBody spell)
        {
        }

        public virtual void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int targetIndex)
        {
        }

        public virtual void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int targetIndex)
        {
        }

        public virtual void OnBeginRoundD20Ping(SpellPacketBody spell)
        {
        }

        public virtual void OnEndRoundD20Ping(SpellPacketBody spell)
        {
        }

        public virtual void OnAreaOfEffectHit(SpellPacketBody spell)
        {
        }

        public virtual void OnSpellStruck(SpellPacketBody spell)
        {
        }
    }
}