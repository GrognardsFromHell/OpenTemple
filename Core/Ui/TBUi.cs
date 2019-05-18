using System;
using System.Collections.Generic;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui
{
    public class TBUi : IDisposable
    {
        [TempleDllLocation(0x10BEC354)]
        private readonly Dictionary<int, string> _translation;

        [TempleDllLocation(0x1014e1f0)]
        public TBUi()
        {
            // TODO: GameUiBridge is being initialized here
            Stub.TODO();

            _translation = Tig.FS.ReadMesFile("mes/inven_ui.mes");
        }

        [TempleDllLocation(0x1014de70)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1014e170)]
        public void OnAfterMapLoad()
        {
            UiSystems.InGame.Recovery(0);
            UiSystems.InGame.Recovery(0);

            if (GameSystems.MapObject.GlobalStashedObject != null)
            {
                GameSystems.MapObject.GlobalStashedObject = null;
            }
        }
    }
}