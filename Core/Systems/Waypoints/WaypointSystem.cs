using System;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Waypoints
{
    public class WaypointSystem : IGameSystem, IBufferResettingSystem
    {
        [TempleDllLocation(0x100533c0)]
        public WaypointSystem()
        {
        }

        [TempleDllLocation(0x10053410)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10053430)]
        public void ResetBuffers()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10053460)]
        public int GetWaypointCount(GameObjectBody critter)
        {
            return (int) critter.GetInt64(obj_f.npc_waypoints_idx, 0);
        }

        [TempleDllLocation(0x10058370)]
        public bool CritterIsAtWaypoint(GameObjectBody critter, in Waypoint waypoint)
        {
            var distance = critter.DistanceToLocInFeet(waypoint.Location);
            if (waypoint.HasAnimations)
            {
                // We do need to be closer if the tile is animating. But it's still a weird metric.
                return distance < locXY.INCH_PER_SUBTILE / locXY.INCH_PER_FEET;
            }
            else
            {
                // This actually seems kinda wrong since it's in feet...
                return distance < locXY.INCH_PER_SUBTILE / 2;
            }
        }

        [TempleDllLocation(0x100534a0)]
        public Waypoint GetWaypoint(GameObjectBody critter, int index)
        {
            // It's logically made up of 16 integers per waypoint, packed into 8 int64's
            Span<long> values = stackalloc long[8];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = critter.GetInt64(obj_f.npc_waypoints_idx, 8 * index + 2 + i);
            }

            ReadOnlySpan<int> intValues = MemoryMarshal.Cast<long, int>(values);

            var flags = (WaypointFlag) intValues[0];

            var location = new LocAndOffsets(
                intValues[1],
                intValues[2],
                BitConverter.Int32BitsToSingle(intValues[3]),
                BitConverter.Int32BitsToSingle(intValues[4])
            );

            var rotation = BitConverter.Int32BitsToSingle(intValues[5]);

            EncodedAnimId[] actualAnimIds;
            if ((flags & WaypointFlag.Animate) != default)
            {
                var animIds = MemoryMarshal.Cast<int, byte>(intValues.Slice(6, 2));
                int notZero = 0;
                foreach (var animId in animIds)
                {
                    if (animId != 0)
                    {
                        notZero++;
                    }
                }

                actualAnimIds = new EncodedAnimId[notZero];
                var idx = 0;
                foreach (var animId in animIds)
                {
                    if (animId > 0 && animId < 64)
                    {
                        actualAnimIds[idx++] = new EncodedAnimId(WeaponAnim.CombatFidget + animId);
                    }
                    else if (animId >= 64)
                    {
                        actualAnimIds[idx++] = new EncodedAnimId(NormalAnimType.Falldown + (animId - 64));
                    }
                }
            }
            else
            {
                actualAnimIds = Array.Empty<EncodedAnimId>();
            }

            var delay = TimeSpan.FromMilliseconds(intValues[8]);

            return new Waypoint(
                location,
                (flags & WaypointFlag.FixedRotation) != default,
                rotation,
                (flags & WaypointFlag.Delay) != default,
                delay,
                actualAnimIds
            );
        }
    }

    [Flags]
    public enum WaypointFlag
    {
        FixedRotation = 1,
        Delay = 2,
        Animate = 4
    }

    public readonly struct Waypoint
    {
        public readonly bool HasFixedRotation;
        public readonly bool HasDelay;

        public readonly LocAndOffsets Location;
        public readonly float Rotation;
        public readonly TimeSpan Delay;
        public readonly EncodedAnimId[] Anims;

        public bool HasAnimations => Anims.Length != 0;

        public Waypoint(LocAndOffsets location, bool hasFixedRotation, float rotation,
            bool hasDelay, TimeSpan delay, EncodedAnimId[] anims)
        {
            Location = location;
            HasFixedRotation = hasFixedRotation;
            Rotation = rotation;
            HasDelay = hasDelay;
            Delay = delay;
            Anims = anims;
        }
    }
}