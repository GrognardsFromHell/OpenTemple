using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.TimeEvents;

namespace SpicyTemple.Core.Systems
{
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

    }
}