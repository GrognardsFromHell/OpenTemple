using System;
using System.IO;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Platform;

namespace SpicyTemple.Core.Systems.D20
{
    public class HotkeySystem : IDisposable
    {
        [TempleDllLocation(0x100f3b80)]
        public HotkeySystem()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100f3bc0)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x100f3bd0)]
        public int SaveHotkeys(BinaryWriter writer)
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x100f3c80)]
        public int LoadHotkeys(BinaryReader reader)
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x100f4030)]
        public void HotkeyAssignCallback(bool cancelFlag)
        {
            if (!cancelFlag)
            {
                Stub.TODO();
            }
        }

        [TempleDllLocation(0x100F3ED0)]
        public bool IsReservedHotkey(DIK dinputKey)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x100F3F20)]
        public int HotkeyReservedPopup(DIK dinputKey)
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x100F3D20)]
        public bool IsNormalNonreservedHotkey(DIK dinputKey)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x100F3D60)]
        public bool RadmenuHotkeySthg(GameObjectBody obj, DIK key)
        {
            throw new NotImplementedException();
        }

    }
}