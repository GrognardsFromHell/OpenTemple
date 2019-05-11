using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public class CombatSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10AA8418)]
        private bool _active;

        [TempleDllLocation(0x10062eb0)]
        public void Dispose()
        {
            CombatEnd();
        }

        [TempleDllLocation(0x10062ed0)]
        public void Reset()
        {
            CombatEnd(true);
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        public void AdvanceTime(TimePoint time)
        {
            // TODO
        }

        [TempleDllLocation(0x100628d0)]
        public bool IsCombatActive()
        {
            return _active;
        }

        [TempleDllLocation(0x100634e0)]
        public void AdvanceTurn(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100630f0)]
        public void CritterLeaveCombat(GameObjectBody obj)
        {
            if (!GameSystems.Party.IsPlayerControlled(obj))
            {
                var critterFlags = obj.GetCritterFlags();
                if (critterFlags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
                {
                    obj.SetCritterFlags(critterFlags & ~CritterFlag.COMBAT_MODE_ACTIVE);
                }

                return;
            }

            if (!IsCombatActive())
                return;

            if (GameSystems.Party.GetConsciousLeader() != obj)
            {
                return;
            }

            if (GameSystems.Critter.IsDeadNullDestroyed(obj))
            {
                if (GameSystems.Party.GetLivingPartyMemberCount() >= 1)
                {
                    return;
                }
            }

            if (!CombatEnd())
            {
                return;
            }

            GameUiBridge.OnExitCombat();

            GameSystems.SoundGame.StopCombatMusic(obj);

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                var critterFlags = partyMember.GetCritterFlags();
                if (critterFlags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
                {
                    partyMember.SetCritterFlags(critterFlags & ~CritterFlag.COMBAT_MODE_ACTIVE);
                }
            }
        }

        [TempleDllLocation(0x10062a30)]
        private bool CombatEnd(bool resetting = false)
        {
            if (!IsCombatActive())
            {
                return true;
            }

            GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Combat_End);
            _active = false;
            GameSystems.Anim.SetAllGoalsClearedCallback(null);
            if (!GameSystems.Anim.InterruptAllForTbCombat())
            {
                Logger.Debug("CombatEnd: Anim goal interrupt FAILED!");
            }

            GameSystems.D20.Actions.ActionSequencesResetOnCombatEnd();
            if (!resetting)
            {
                GameSystems.D20.Initiative.OnExitCombat();
            }

            GameSystems.D20.EndTurnBasedCombat();

            if (!resetting)
            {
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    AutoReloadCrossbow(partyMember);
                }

                CombatGiveXPRewards();
            }

            return true;
        }

        [TempleDllLocation(0x100B70A0)]
        private void AutoReloadCrossbow(GameObjectBody critter)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100b88c0)]
        private void CombatGiveXPRewards()
        {
            Stub.TODO();
        }

        public bool HasLineOfAttack(GameObjectBody obj, GameObjectBody target)
        {
            using var objIt = new RaycastPacket();
            objIt.origin = obj.GetLocationFull();
            LocAndOffsets tgtLoc = target.GetLocationFull();
            objIt.targetLoc = tgtLoc;
            objIt.flags = RaycastFlag.StopAfterFirstBlockerFound | RaycastFlag.ExcludeItemObjects |
                          RaycastFlag.HasTargetObj | RaycastFlag.HasSourceObj | RaycastFlag.HasRadius;
            objIt.radius = 0.1f;
            bool blockerFound = false;
            if (objIt.Raycast() > 0)
            {
                foreach (var resultItem in objIt)
                {
                    var resultObj = resultItem.obj;
                    if (resultObj == null)
                    {
                        if (resultItem.flags.HasFlag(RaycastResultFlag.BlockerSubtile))
                        {
                            blockerFound = true;
                        }

                        continue;
                    }

                    if (resultObj.type == ObjectType.portal)
                    {
                        if (!resultObj.IsPortalOpen())
                        {
                            blockerFound = true;
                        }
                        continue;
                    }

                    if (resultObj.IsCritter())
                    {
                        if (GameSystems.Critter.IsDeadOrUnconscious(resultObj)
                            || GameSystems.D20.D20Query(resultObj, D20DispatcherKey.QUE_Prone) == 1)
                        {
                            continue;
                        }

                        // TODO: flag for Cover
                    }
                }
            }

            return !blockerFound;
        }

        [TempleDllLocation(0x1004e730)]
        public void DispatchBeginRound(GameObjectBody obj, int numRounds)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher != null)
            {
                var dispIo = new DispIoD20Signal();
                dispIo.data1 = numRounds;
                dispatcher.Process(DispatcherType.BeginRound, D20DispatcherKey.NONE, dispIo);
                GameSystems.Spell.ObjOnSpellBeginRound(obj);
            }
        }
    }
}