using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.TimeEvents;

namespace SpicyTemple.Core.Systems
{
    public delegate void AiCancelDialog(GameObjectBody critter);

    public delegate void AiShowTextBubble(GameObjectBody critter, GameObjectBody speakingTo,
        string text, int speechId);

    public class AiSystem : IGameSystem, IModuleAwareSystem
    {
        public AiSystem()
        {
            Stub.TODO();
        }

        public void Dispose()
        {
            Stub.TODO();
        }

        public void LoadModule()
        {
            Stub.TODO();
        }

        public void UnloadModule()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1005d5e0)]
        public void AddAiTimer(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10AA4BC8)]
        private AiCancelDialog _cancelDialog;

        [TempleDllLocation(0x10AA73B0)]
        private AiShowTextBubble _showTextBubble;

        [TempleDllLocation(0x10056ef0)]
        public void SetDialogFunctions(AiCancelDialog cancelDialog, AiShowTextBubble showTextBubble)
        {
            _cancelDialog = cancelDialog;
            _showTextBubble = showTextBubble;
        }

        [TempleDllLocation(0x1005be60)]
        public void AddOrReplaceAiTimer(GameObjectBody obj, int unknownFlag)
        {
            GameSystems.TimeEvent.Remove(TimeEventType.AI, evt => evt.arg1.handle == obj);

            var newEvt = new TimeEvent(TimeEventType.AI);
            newEvt.arg1.handle = obj;
            newEvt.arg2.int32 = unknownFlag;

            GameSystems.TimeEvent.ScheduleNow(newEvt);
        }

        [TempleDllLocation(0x1005b5f0)]
        public void FollowerAddWithTimeEvent(GameObjectBody obj, bool forceFollower)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100588d0)]
        public void RemoveAiTimer(GameObjectBody obj)
        {
            GameSystems.TimeEvent.Remove(TimeEventType.AI, evt => evt.arg1.handle == obj);
        }

        [TempleDllLocation(0x100e5460)]
        public void OnAddToInitiative(GameObjectBody obj)
        {
            Stub.TODO();
        }

        /// <summary>
        /// Moves back ex-followers to their original intended map and location.
        /// </summary>
        [TempleDllLocation(0x10058620)]
        public void MoveExFollowers()
        {
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                if (obj.IsNPC())
                {
                    if (obj.GetNPCFlags().HasFlag(NpcFlag.EX_FOLLOWER))
                    {
                        var mapId = GameSystems.Critter.GetTeleportMap(obj);
                        if (GetCurrentStandpoint(obj, out var location))
                        {
                            GameSystems.MapObject.MoveToMap(obj, mapId, new LocAndOffsets(location, 0, 0));
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100584f0)]
        private bool GetCurrentStandpoint(GameObjectBody obj, out locXY location)
        {
            if (GameSystems.Critter.GetLeader(obj) != null)
            {
                location = default;
                return false;
            }

            StandPoint standPoint;
            var v2 = obj.GetNPCFlags();
            if (v2.HasFlag(NpcFlag.USE_ALERTPOINTS))
            {
                if (!GameSystems.Script.GetGlobalFlag(144))
                {
                    GetStandpoint(obj, 0, out standPoint);
                }
                else
                {
                    GetStandpoint(obj, 1, out standPoint);
                }
            }
            else
            {
                if (GameSystems.TimeEvent.IsDaytime)
                {
                    // Daytime standpoint
                    GetStandpoint(obj, 0, out standPoint);
                }
                else
                {
                    // Nighttime standpoint
                    GetStandpoint(obj, 1, out standPoint);
                }
            }

            location = standPoint.location.location;
            return location.locx != 0 || location.locy != 0;
        }

        [TempleDllLocation(0x100ba890)]
        private void GetStandpoint(GameObjectBody obj, int n, out StandPoint standPoint)
        {
            Debugger.Break();

            var standpointArray = obj.GetInt64Array(obj_f.npc_standpoints);

            Span<long> packedStandpoint = stackalloc long[10];

            for (int i = 0; i < 10; i++)
            {
                packedStandpoint[i] = standpointArray[10 * n + i];
            }

            standPoint = MemoryMarshal.Read<StandPoint>(MemoryMarshal.Cast<long, byte>(packedStandpoint));
        }

        [TempleDllLocation(0x1005e8d0)]
        public void ProvokeHostility(GameObjectBody agitator, GameObjectBody provokedNpc, int rangeType, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005df40)]
        public void SetNoFlee(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        // NO idea why this is in the AI subsystem
        [TempleDllLocation(0x1005bf20)]
        public PortalLockStatus AttemptToOpenDoor(GameObjectBody actor, GameObjectBody portal)
        {
            if (GameSystems.MapObject.IsBusted(portal))
            {
                return PortalLockStatus.PLS_OPEN;
            }

            if (!actor.IsCritter())
            {
                return PortalLockStatus.PLS_INVALID_OPENER;
            }

            if (GameSystems.Script.ExecuteObjectScript(actor, portal, ObjScriptEvent.Unlock) == 0)
            {
                return PortalLockStatus.PLS_DENIED_BY_SCRIPT;
            }

            if (portal.type != ObjectType.portal)
            {
                return PortalLockStatus.PLS_OPEN;
            }

            var portalFlags = portal.GetPortalFlags();
            if (portalFlags.HasFlag(PortalFlag.JAMMED))
            {
                return PortalLockStatus.PLS_JAMMED;
            }

            if (portalFlags.HasFlag(PortalFlag.MAGICALLY_HELD))
            {
                return PortalLockStatus.PLS_MAGICALLY_HELD;
            }

            if (!portalFlags.HasFlag(PortalFlag.ALWAYS_LOCKED))
            {
                if (actor.IsNPC())
                {
                    var leader = GameSystems.Critter.GetLeaderRecursive(actor);
                    if (leader != null)
                    {
                        if (portal.IsPortalOpen())
                        {
                            return PortalLockStatus.PLS_OPEN;
                        }
                    }
                }
                else if (portal.IsPortalOpen())
                {
                    return PortalLockStatus.PLS_OPEN;
                }
            }

            if (!portal.NeedsToBeUnlocked())
            {
                return PortalLockStatus.PLS_OPEN;
            }

            var keyId = portal.GetInt32(obj_f.portal_key_id);
            if (GameSystems.Item.HasKey(actor, keyId))
            {
                GameUiBridge.MarkKeyUsed(keyId, GameSystems.TimeEvent.GameTime);
                return PortalLockStatus.PLS_OPEN;
            }

            if (portal.IsUndetectedSecretDoor())
            {
                return PortalLockStatus.PLS_SECRET_UNDISCOVERED;
            }

            return PortalLockStatus.PLS_LOCKED;
        }

        [TempleDllLocation(0x1005a640)]
        public bool ForceSpreadOut(GameObjectBody critter, locXY? optionalLocation = null)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005a170)]
        public int GetTalkingDistance(GameObjectBody critter)
        {
            return critter.IsPC() ? 5 : 10;
        }

        [TempleDllLocation(0x10057790)]
        public void sub_10057790(GameObjectBody actor, GameObjectBody target)
        {
            if (actor.id != target.id)
            {
                _cancelDialog?.Invoke(actor);
                if (!GameSystems.Critter.IsCombatModeActive(actor))
                {
                    GameSystems.Combat.EnterCombat(actor);
                }

                if (!GameSystems.Anim.HasRunSlot(actor) || GameSystems.Anim.IsIdleOrFidgeting(actor))
                {
                    if (Globals.Config.AutoAttack)
                    {
                        if (!GameSystems.Anim.IsRunningGoal(actor, AnimGoalType.attempt_attack, out _))
                        {
                            GameSystems.Anim.PushAttackOther(actor, target);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100583e0)]
        public void ClearWaypointDelay(GameObjectBody critter)
        {
            if (critter.IsNPC())
            {
                critter.AiFlags &= ~AiFlag.WaypointDelay;
            }
        }
    }

    public enum PortalLockStatus
    {
        PLS_OPEN = 0,
        PLS_LOCKED = 1,
        PLS_JAMMED = 2,
        PLS_MAGICALLY_HELD = 3,
        PLS_DENIED_BY_SCRIPT = 4,
        PLS_INVALID_OPENER = 5,
        PLS_SECRET_UNDISCOVERED = 6,
    }
}