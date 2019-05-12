using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.InGame
{
    public class InGameUi : AbstractUi, IDisposable, ISaveGameAwareUi
    {

        private Dictionary<int, string> _translations;

        [TempleDllLocation(0x10112e70)]
        public InGameUi()
        {
            _translations = Tig.FS.ReadMesFile("mes/intgame.mes");

        }

        [TempleDllLocation(0x10112eb0)]
        public void Dispose()
        {
        }

        public void ResetInput()
        {
            // TODO throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x10113CD0)]
        public int sub_10113CD0()
        {
            // TODO
            return 0;
        }

        [TempleDllLocation(0x10113D40)]
        public int sub_10113D40(int unk)
        {
            // TODO
            return 1;
        }

        [TempleDllLocation(0x10139400)]
        public void FocusClear()
        {
            Stub.TODO();
        }

        public bool SaveGame()
        {
            return true;
        }

        [TempleDllLocation(0x101140c0)]
        public bool LoadGame()
        {
            FocusClear();
            return true;
        }

        [TempleDllLocation(0x10112ec0)]
        public override void ResizeViewport(Size size)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101140b0)]
        public override void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10114EF0)]
        public void HandleMessage(Message msg)
        {
            Stub.TODO();
        }
    }
}