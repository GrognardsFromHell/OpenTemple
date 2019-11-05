using System;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.Ui
{
    public class PartyPoolUi : IResetAwareSystem, ISaveGameAwareGameSystem
    {
        [TempleDllLocation(0x10163720)]
        public bool IsVisible
        {
            get
            {
                return false; // TODO
            }
        }

        [TempleDllLocation(0x10165e60)]
        public void Show(bool ingame)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10165cd0)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10165d10)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10165da0)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }
    }
}