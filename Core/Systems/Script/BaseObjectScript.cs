using System;
using System.Diagnostics.Contracts;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
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

        public virtual bool OnUse(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnDestroy(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnUnlock(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnGet(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnDying(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnExitCombat(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnEndCombat(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnLeaderKilling(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnInsertItem(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnWieldOn(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnWieldOff(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnNewSector(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnRemoveItem(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnTransfer(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnCaughtThief(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnJoin(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnDisband(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnNewMap(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnTrap(TrapSprungEvent evt, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnUnlockAttempt(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnSpellCast(GameObject attachee, GameObject triggerer, SpellPacketBody spell)
        {
            return true;
        }

        public virtual bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            return true;
        }

        public virtual bool OnTrueSeeing(GameObject attachee, GameObject triggerer)
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
                    return OnUnlockAttempt(invocation.attachee, invocation.triggerer);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}