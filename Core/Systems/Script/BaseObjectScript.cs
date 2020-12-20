using System;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.Script
{
    public abstract class BaseObjectScript
    {

        private ObjScriptInvocation _currentInvocation;

        protected ILogger Logger = LoggingSystem.CreateLogger();

        protected void DetachScript()
        {
            var currentlyAttached = _currentInvocation.attachee.GetScriptId(_currentInvocation.eventId);
            Logger.Info("Detaching {0} script from {1} (Is: {2})",
                _currentInvocation.eventId,
                _currentInvocation.attachee,
                currentlyAttached
            );
            _currentInvocation.attachee.SetScriptId(_currentInvocation.eventId, 0);
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

        public bool Invoke(ref ObjScriptInvocation invocation)
        {
            _currentInvocation = invocation;

            switch (invocation.eventId)
            {
                case ObjScriptEvent.Use:
                    return OnUse(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Destroy:
                    return OnDestroy(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Unlock:
                    return OnUnlock(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Get:
                    return OnGet(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Dialog:
                    return OnDialog(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.FirstHeartbeat:
                    return OnFirstHeartbeat(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Dying:
                    return OnDying(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.EnterCombat:
                    return OnEnterCombat(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.ExitCombat:
                    return OnExitCombat(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.StartCombat:
                    return OnStartCombat(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.EndCombat:
                    return OnEndCombat(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Resurrect:
                    return OnResurrect(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Heartbeat:
                    return OnHeartbeat(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.LeaderKilling:
                    return OnLeaderKilling(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.InsertItem:
                    return OnInsertItem(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.WillKos:
                    return OnWillKos(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.WieldOn:
                    return OnWieldOn(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.WieldOff:
                    return OnWieldOff(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.NewSector:
                    return OnNewSector(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.RemoveItem:
                    return OnRemoveItem(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Transfer:
                    return OnTransfer(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.CaughtThief:
                    return OnCaughtThief(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Join:
                    return OnJoin(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Disband:
                    return OnDisband(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.NewMap:
                    return OnNewMap(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.Trap:
                    return OnTrap(invocation.trapEvent, invocation.triggerer);
                case ObjScriptEvent.TrueSeeing:
                    return OnTrueSeeing(invocation.attachee, invocation.triggerer);
                case ObjScriptEvent.SpellCast:
                    return OnSpellCast(invocation.attachee, invocation.triggerer, invocation.spell);
                case ObjScriptEvent.UnlockAttempt:
                    return OnUnlock(invocation.attachee, invocation.triggerer);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}