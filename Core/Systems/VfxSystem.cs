using System;
using System.Numerics;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems
{
    public class VfxSystem : IDisposable
    {
        [TempleDllLocation(0x10b397b4)]
        private bool pfx_lightning_render;

        [TempleDllLocation(0x10b397b8)]
        private Vector2 lightning_scratch;

        // Initializes several random tables, vectors and shuffled int lists as well as the lightning material.
        [TempleDllLocation(0x10087220)]
        public VfxSystem()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10087440)]
        public void StartLightning(LocAndOffsets location)
        {
            lightning_scratch = location.ToInches2D();
            pfx_lightning_render = true;
        }

        [TempleDllLocation(0x10087e60)]
        public void Render()
        {
            Stub.TODO();
        }

        public void Dispose()
        {
        }
    }
}