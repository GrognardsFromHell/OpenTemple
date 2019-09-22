using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.Script
{
    public abstract class BaseSpellScript
    {
        protected readonly ILogger Logger = new ConsoleLogger();


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

        public virtual void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int targetIndex)
        {
        }

        public virtual void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int targetIndex)
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