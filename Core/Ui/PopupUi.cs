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

        #region vanilla_ui

        [TempleDllLocation(0x1017cf20)]
        public void ConfirmBox(string body, string title, int i, int i1, int i2)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}