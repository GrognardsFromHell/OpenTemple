using System;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.Ui
{
    public class PopupUi : IDisposable, IResetAwareSystem
    {
        [TempleDllLocation(0x10171df0)]
        public PopupUi()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10171510)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10171e70)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10171a70)]
        public bool IsAnyOpen()
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x10171e40)]
        public void CloseAll()
        {
            Stub.TODO();
        }
    }
}