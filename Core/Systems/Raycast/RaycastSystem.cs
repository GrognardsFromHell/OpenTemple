using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Raycast
{
    public class RaycastSystem : IDisposable
    {
        private readonly List<GoalDestination> _goalDestinations = new List<GoalDestination>();

        [TempleDllLocation(0x100bac20)]
        public void GoalDestinationsAdd(GameObjectBody obj, LocAndOffsets loc)
        {
            _goalDestinations.Add(new GoalDestination(obj, loc));
        }

        [TempleDllLocation(0x100bac80)]
        public void GoalDestinationsRemove(GameObjectBody obj)
        {
            _goalDestinations.RemoveAll(gd => gd.obj == obj);
        }

        [TempleDllLocation(0x100bacc0)]
        public void GoalDestinationsClear()
        {
            _goalDestinations.Clear();
        }

        private struct GoalDestination
        {
            public readonly GameObjectBody obj;

            public readonly LocAndOffsets loc;

            public GoalDestination(GameObjectBody obj, LocAndOffsets loc)
            {
                this.obj = obj;
                this.loc = loc;
            }
        }

        public void Dispose()
        {
        }
    }
}