using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.Script
{
    public abstract class BaseObjectScript
    {

        protected ILogger Logger = new ConsoleLogger();

        protected void DetachScript()
        {
            // Replaces game.new_sid = 0;
            throw new NotImplementedException();
        }

        protected void ReplaceCurrentScript(int scriptId)
        {
            // Replaces game.new_sid = scriptId;
            throw new NotImplementedException();
        }

        protected void SetCounter(int index, int value)
        {
            throw new NotImplementedException();
        }

        protected int GetCounter(int index)
        {
            throw new NotImplementedException();
        }

        public virtual bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnDestroy(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnUnlock(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnGet(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnLeaderKilling(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnWieldOn(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnWieldOff(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnNewSector(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnRemoveItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnTransfer(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnCaughtThief(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnTrap(TrapSprungEvent evt, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnUnlockAttempt(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            return true;
        }

        public virtual bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

        public virtual bool OnTrueSeeing(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return true;
        }

    }
}