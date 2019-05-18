using System;
using SpicyTemple.Core.Platform;

namespace SpicyTemple.Core.Ui
{
    public class ManagerUi : AbstractUi, IDisposable
    {

        [TempleDllLocation(0x10BE8CF0)]
        private bool _modalIsOpen;

        [field: TempleDllLocation(0x10BE8CF4)]
        [TempleDllLocation(0x101431e0)]
        [TempleDllLocation(0x101431d0)]
        public bool AlwaysFalse { get; set; }

        [TempleDllLocation(0x10143bd0)]
        public ManagerUi()
        {
            _modalIsOpen = false;
            AlwaysFalse = false;
        }

        [TempleDllLocation(0x101431a0)]
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10143190)]
        public override void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10143d60)]
        public void HandleKeyEvent(MessageKeyStateChangeArgs args)
        {
            Stub.TODO();
        }
    }
}