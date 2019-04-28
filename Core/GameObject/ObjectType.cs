using System;
using System.Linq;

namespace SpicyTemple.Core.GameObject
{
    public enum ObjectType
    {
        portal = 0,
        container = 1,
        scenery = 2,
        projectile = 3,
        weapon = 4,
        ammo = 5,
        armor = 6,
        money = 7,
        food = 8,
        scroll = 9,
        key = 10,
        written = 11,
        generic = 12,
        pc = 13,
        npc = 14,
        trap = 15,
        bag = 16
    }

    public static class ObjectTypes
    {

        public static readonly ObjectType[] All = Enum.GetValues(typeof(ObjectType)).OfType<ObjectType>().ToArray();

        public static readonly int Count = All.Length;

    }

}