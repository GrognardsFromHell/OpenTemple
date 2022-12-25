using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f7b74)]
internal class VoiceSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_VOICE";

    public ChargenStages Stage => ChargenStages.CG_Stage_Voice;

    public WidgetContainer Container { get; }

    private CharEditorSelectionPacket _pkt;

    [TempleDllLocation(0x1017e220)]
    public VoiceSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/voice_ui.json");
        Container = doc.GetRootContainer();
        Container.Visible = false;

        // TODO int result;
        // TODO int v2;
        // TODO string meslineValue;
        // TODO int meslineKey;
// TODO
        // TODO stru_10C0C700 /*0x10c0c700*/.textColor = (ColorRect*) &unk_102FCDF8 /*0x102fcdf8*/;
        // TODO stru_10C0C700 /*0x10c0c700*/.colors4 = (ColorRect*) &unk_102FCDF8 /*0x102fcdf8*/;
        // TODO stru_10C0C700 /*0x10c0c700*/.colors2 = (ColorRect*) &unk_102FCDF8 /*0x102fcdf8*/;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.textColor = (ColorRect*) &unk_102FCDF8 /*0x102fcdf8*/;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.colors4 = (ColorRect*) &unk_102FCDF8 /*0x102fcdf8*/;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.colors2 = (ColorRect*) &unk_102FCDF8 /*0x102fcdf8*/;
        // TODO stru_10C0C700 /*0x10c0c700*/.flags = 0x4000;
        // TODO stru_10C0C700 /*0x10c0c700*/.field2c = -1;
        // TODO stru_10C0C700 /*0x10c0c700*/.shadowColor = (ColorRect*) &unk_102FCE18 /*0x102fce18*/;
        // TODO stru_10C0C700 /*0x10c0c700*/.field0 = 0;
        // TODO stru_10C0C700 /*0x10c0c700*/.kerning = 1;
        // TODO stru_10C0C700 /*0x10c0c700*/.leading = 0;
        // TODO stru_10C0C700 /*0x10c0c700*/.tracking = 3;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.flags = 0x4000;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.field2c = -1;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.shadowColor = (ColorRect*) &unk_102FCE18 /*0x102fce18*/;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.field0 = 0;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.kerning = 1;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.leading = 0;
        // TODO stru_10C0CD90 /*0x10c0cd90*/.tracking = 3;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.flags = 0x4000;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.field2c = -1;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.textColor = (ColorRect*) &unk_102FCE08 /*0x102fce08*/;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.shadowColor = (ColorRect*) &unk_102FCE18 /*0x102fce18*/;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.colors4 = (ColorRect*) &unk_102FCE08 /*0x102fce08*/;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.colors2 = (ColorRect*) &unk_102FCE08 /*0x102fce08*/;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.field0 = 0;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.kerning = 1;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.leading = 0;
        // TODO stru_10C0C7E0 /*0x10c0c7e0*/.tracking = 3;
        // TODO stru_10C0C680 /*0x10c0c680*/.flags = 0x4000;
        // TODO stru_10C0C680 /*0x10c0c680*/.field2c = -1;
        // TODO stru_10C0C680 /*0x10c0c680*/.textColor = (ColorRect*) &unk_102FCE28 /*0x102fce28*/;
        // TODO stru_10C0C680 /*0x10c0c680*/.shadowColor = (ColorRect*) &unk_102FCE18 /*0x102fce18*/;
        // TODO stru_10C0C680 /*0x10c0c680*/.colors4 = (ColorRect*) &unk_102FCE08 /*0x102fce08*/;
        // TODO stru_10C0C680 /*0x10c0c680*/.colors2 = (ColorRect*) &unk_102FCE08 /*0x102fce08*/;
        // TODO stru_10C0C680 /*0x10c0c680*/.field0 = 0;
        // TODO stru_10C0C680 /*0x10c0c680*/.kerning = 1;
        // TODO stru_10C0C680 /*0x10c0c680*/.leading = 0;
        // TODO stru_10C0C680 /*0x10c0c680*/.tracking = 3;
        // TODO dword_10C0C834 /*0x10c0c834*/ = Globals.UiAssets.LoadImg("art\\interface\\pc_creation\\bigvoicebox.img");
        // TODO if (dword_10C0C834 /*0x10c0c834*/
        // TODO     && (meslineKey = 23000, Mesfile_GetLine /*0x101e6760*/(pc_creationMes /*0x11e72ef0*/, &mesline))
        // TODO     && (dword_10C0C7C0 /*0x10c0c7c0*/ = (string) meslineValue, meslineKey = 23001,
        // TODO         Mesfile_GetLine /*0x101e6760*/(pc_creationMes /*0x11e72ef0*/, &mesline))
        // TODO     && (dword_10C0C830 /*0x10c0c830*/ = (string) meslineValue, meslineKey = 23002,
        // TODO         Mesfile_GetLine /*0x101e6760*/(pc_creationMes /*0x11e72ef0*/, &mesline)))
        // TODO {
        // TODO     v2 = conf.height;
        // TODO     dword_10C0D304 /*0x10c0d304*/ = (string) meslineValue;
        // TODO     result = UiPcCreationNameWidgetsInit /*0x1017de80*/(conf.width, v2) != 0;
        // TODO }
        // TODO else
        // TODO {
        // TODO     result = 0;
        // TODO }

    }

    [TempleDllLocation(0x1017d5a0)]
    public void Reset(CharEditorSelectionPacket pkt)
    {
        _pkt = pkt;

        _pkt.voiceFile = null;
        _pkt.voiceId = -1;
    }

    [TempleDllLocation(0x1017dbf0)]
    public void Activate()
    {
        // TODO int v0;
        // TODO int i_1;
        // TODO int v2;
        // TODO int v3;
        // TODO int i;
        // TODO CHAR v6;
        // TODO _WORD* v7;
        // TODO CHAR v8;
// TODO
        // TODO voiceFileLen /*0x10c0c570*/ = strlen(_pkt.voiceFile);
        // TODO i = 0;
        // TODO do
        // TODO {
        // TODO     v6 = _pkt.voiceFile[i];
        // TODO     byte_10C0C9F0 /*0x10c0c9f0*/[i++] = v6;
        // TODO } while (v6);
// TODO
        // TODO v7 = &byte_10C0C9F0 /*0x10c0c9f0*/[-1];
        // TODO do
        // TODO {
        // TODO     v8 = *((_BYTE*) v7 + 1);
        // TODO     v7 = (_WORD*) ((string) v7 + 1);
        // TODO } while (v8);
// TODO
        // TODO *v7 = consoleMarker /*0x10299060*/;
        // TODO v0 = sub_10034880 /*0x10034880*/();
        // TODO i_1 = 0;
        // TODO uiPcCreationNamesScrollbarY /*0x10c0c6e4*/ = 0;
        // TODO for (dword_10C0C574 /*0x10c0c574*/ = 0; i_1 <= v0; ++i_1)
        // TODO {
        // TODO     if (sub_100347D0 /*0x100347d0*/(UiSystems.PCCreation.charEditorObjHnd, i_1))
        // TODO     {
        // TODO         voiceIds /*0x10c0c578*/[dword_10C0C574 /*0x10c0c574*/] = i_1;
        // TODO         v2 = sub_10034840 /*0x10034840*/(i_1);
        // TODO         v3 = dword_10C0C574 /*0x10c0c574*/;
        // TODO         *(_DWORD*) &dword_10C0C838 /*0x10c0c838*/[4 * dword_10C0C574 /*0x10c0c574*/] = v2;
        // TODO         dword_10C0C574 /*0x10c0c574*/ = v3 + 1;
        // TODO     }
        // TODO }
// TODO
        // TODO j_WidgetCopy /*0x101f87a0*/(uiPcCreationNamesScrollbarId /*0x10c0c9e8*/,
        // TODO     (LgcyWidget*) &uiPcCreationNamesScrollbar /*0x10c0c938*/);
        // TODO uiPcCreationNamesScrollbar /*0x10c0c938*/.yMax =
        // TODO     (dword_10C0C574 /*0x10c0c574*/ - 7) & ((dword_10C0C574 /*0x10c0c574*/ - 7 < 0) - 1);
        // TODO uiPcCreationNamesScrollbar /*0x10c0c938*/.scrollbarY = uiPcCreationNamesScrollbar /*0x10c0c938*/.yMin;
        // TODO uiPcCreationNamesScrollbarY /*0x10c0c6e4*/ = uiPcCreationNamesScrollbar /*0x10c0c938*/.yMin;
        // TODO return j_ui_widget_set /*0x101f87b0*/(uiPcCreationNamesScrollbarId /*0x10c0c9e8*/,
        // TODO     &uiPcCreationNamesScrollbar /*0x10c0c938*/);
    }

    [TempleDllLocation(0x1017d600)]
    public bool CheckComplete()
    {
        return _pkt.voiceFile != null && _pkt.voiceId != -1;
    }

    [TempleDllLocation(0x1017d620)]
    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        // TODO SetPcPlayerNameField /*0x100a0490*/(*playerObj, obj_f.pc_player_name, charSpec.voiceFile);
        // TODO PcVoiceSet /*0x100347b0*/(*playerObj, charSpec.voiceId);
    }

    [TempleDllLocation(0x1017d660)]
    private void UpdateDescriptionBox()
    {
        UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
    }
}