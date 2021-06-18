using OpenTemple.Core.Ui.PartyCreation;
using System;

namespace OpenTemple.Core.Ui.CharSheet
{
    public interface ICharEditorSystem : IDisposable
    {
        string Name { get; }

        void ResetSystem();

        void Resize();

        void Show();

        void Hide();

        void CheckComplete();

        void Complete();

        void Reset(CharEditorSelectionPacket selPkt);

        void Activate();
    }
}