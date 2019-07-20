using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public enum TextFloaterColor
    {
        White = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 4,
        LightBlue = 5
    }

    [Flags]
    public enum TextFloaterCategory
    {
        None = 0,
        Generic = 1,
        Damage = 2
    }

    public class TextFloaterSystem : IGameSystem, IBufferResettingSystem, IResetAwareSystem, ITimeAwareSystem,
        IMapCloseAwareGameSystem
    {
        [TempleDllLocation(0x102CDF50)]
        private static readonly Dictionary<TextFloaterColor, PackedLinearColorA> Colors =
            new Dictionary<TextFloaterColor, PackedLinearColorA>
            {
                {TextFloaterColor.White, PackedLinearColorA.White},
                {TextFloaterColor.Red, new PackedLinearColorA(255, 0, 0 , 255)},
                {TextFloaterColor.Green, new PackedLinearColorA(0, 255, 0 , 255)},
                {TextFloaterColor.Blue, new PackedLinearColorA(64, 64, 255 , 255)},
                {TextFloaterColor.Yellow, new PackedLinearColorA(255, 255, 0, 255)},
                {TextFloaterColor.LightBlue, new PackedLinearColorA(128, 226, 255 , 255)},
            };

        [TempleDllLocation(0x100a2040)]
        public TextFloaterSystem()
        {
        }

        [TempleDllLocation(0x100a2980)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x100a1df0)]
        public void ResetBuffers()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100a2970)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100a2480)]
        public void AdvanceTime(TimePoint time)
        {
        }

        [TempleDllLocation(0x100a2970)]
        public void CloseMap()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100a2870)]
        public void Remove(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100a2200)]
        public void FloatLine(GameObjectBody obj, TextFloaterCategory category, TextFloaterColor color, string text)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100a27d0)]
        public void CritterDied(GameObjectBody critter)
        {
            throw new NotImplementedException();
        }
    }
}