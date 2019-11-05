using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Ui.MainMenu;

namespace SpicyTemple.Core.Ui.PartyCreation
{
    public class PCCreationUi : IDisposable
    {
        [TempleDllLocation(0x102f7bf0)]
        public bool uiPcCreationIsHidden = true;

        [TempleDllLocation(0x1011b750)]
        public bool IsVisible => !uiPcCreationIsHidden;

        [TempleDllLocation(0x10bddd1c)]
        public int defaultPartyNum { get; set; }

        [TempleDllLocation(0x11e741a0)]
        private GameObjectBody charEditorObjHnd;

        private PartyAlignmentUi _partyAlignment = new PartyAlignmentUi();

        [TempleDllLocation(0x10120420)]
        public PCCreationUi()
        {
            Stub.TODO();

            _partyAlignment.OnCancel += Cancel;
        }

        [TempleDllLocation(0x1011ebc0)]
        public void Dispose()
        {
            _partyAlignment.Dispose();

            Stub.TODO();
        }

        [TempleDllLocation(0x1011fdc0)]
        public void Start()
        {
            if (defaultPartyNum > 0)
            {
                for (var i = 0; i < defaultPartyNum; i++)
                {
                    var protoId = 13100 + i;
                    charEditorObjHnd = GameSystems.MapObject.CreateObject(protoId, new locXY(640, 480));
                    GameSystems.Critter.GenerateHp(charEditorObjHnd);
                    GameSystems.Party.AddToPCGroup(charEditorObjHnd);
                    GameSystems.Item.GiveStartingEquipment(charEditorObjHnd);

                    var model = charEditorObjHnd.GetOrCreateAnimHandle();
                    charEditorObjHnd.UpdateRenderHeight(model);
                    charEditorObjHnd.UpdateRadius(model);
                }

                UiChargenFinalize();
            }
            else
            {
                _partyAlignment.Reset();
                StartNewParty();
            }
        }

        [TempleDllLocation(0x1011e200)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:216")]
        private void StartNewParty()
        {
            uiPcCreationIsHidden = false;
            UiPcCreationSystemsResetAll();

//           TODO uiPromptType3/*0x10bdd520*/.bodyText = (string )uiPcCreationText_SelectAPartyAlignment/*0x10bdb018*/;
//
//          TODO  WidgetSetHidden/*0x101f9100*/(uiPcCreationMainWndId/*0x10bdd690*/, 0);
//          TODO  WidgetBringToFront/*0x101f8e40*/(uiPcCreationMainWndId/*0x10bdd690*/);

            _partyAlignment.Show();
            UiSystems.UtilityBar.Hide();
        }

        [TempleDllLocation(0x1011ddd0)]
        private void UiPcCreationSystemsResetAll()
        {
//          TODO  WidgetSetHidden/*0x101f9100*/(uiPcCreationWndId/*0x10bddd18*/, 1);

// TODO
//            v0 = &chargenSystems/*0x102f7938*/[0].hide;
//            do
//            {
//                if ( *v0 )
//                {
//                    (*v0)();
//                }
//                v0 += 11;
//            }
//            while ( (int)v0 < (int)&dword_102F7BB8/*0x102f7bb8*/ );
        }

        [TempleDllLocation(0x1011fc30)]
        public void UiChargenFinalize()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1011e270)]
        public void Hide()
        {
            // TODO ScrollboxHideWindow/*0x1018cac0*/(uiPcCreationScrollBox/*0x11e741b4*/);
            // TODO UiPcCreationPortraitsMainHide/*0x10163030*/();
            UiSystems.Popup.CloseAll();
            // TODO WidgetSetHidden/*0x101f9100*/(uiPcCreationMainWndId/*0x10bdd690*/, 1);
            _partyAlignment.Hide();
            UiPcCreationSystemsResetAll();
            uiPcCreationIsHidden = true;
            UiSystems.UtilityBar.Show();
        }

        private void Cancel()
        {
            ClearParty();
            Hide();
            UiSystems.MainMenu.Show(MainMenuPage.Difficulty);
        }

        [TempleDllLocation(0x1011b6f0)]
        private void ClearParty()
        {
            while (GameSystems.Party.PartySize > 0)
            {
                var player = GameSystems.Party.GetPartyGroupMemberN(0);
                GameSystems.Party.RemoveFromAllGroups(player);
            }

            // TODO PcPortraitsDeactivate/*0x10163060*/();
        }
    }
}