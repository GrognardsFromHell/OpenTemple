using System;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Ui.CharSheet.Portrait
{
    public class CharSheetPortraitUi : IDisposable
    {
        [TempleDllLocation(0x101a5b40)]
        public CharSheetPortraitUi()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101552a0)]
        [TempleDllLocation(0x10155290)]
        [TempleDllLocation(0x10BEECB8)]
        public GameObjectBody CurrentCritter { get; set; }

        [TempleDllLocation(0x101a3150)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101a3180)]
        public void Show()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101a3260)]
        public void Hide()
        {
            Stub.TODO();
        }

        public void Reset()
        {
        }
    }
}