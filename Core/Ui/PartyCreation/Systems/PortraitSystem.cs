using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f7b48)]
internal class PortraitSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_PORTRAIT";

    public ChargenStages Stage => ChargenStages.CG_Stage_Portrait;

    public WidgetContainer Container { get; }

    private CharEditorSelectionPacket _pkt;

    [TempleDllLocation(0x1017ea70)]
    public PortraitSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/portrait_ui.json");
        Container = doc.GetRootContainer();
        Container.Visible = false;

        // TODO chargenPortraitCapacity /*0x10c0eaa8*/ = 0;
        // TODO chargenPortraitCount /*0x10c0eb64*/ = 0;
        // TODO chargenPortraitIds /*0x10c0ed28*/ = 0;
        // TODO if (RegisterUiTexture /*0x101ee7b0*/("art\\interface\\pc_creation\\portrait_frame.tga",
        // TODO     &chargenPortrait_portrait_frame /*0x10c0eef0*/))
        // TODO {
        // TODO     result = 0;
        // TODO }
        // TODO else
        // TODO {
        // TODO     result = ChargenPortraitWidgetsInit /*0x1017e800*/(a1.width, a1.height) != 0;
        // TODO }
    }

    [TempleDllLocation(0x1017e420)]
    public void Reset(CharEditorSelectionPacket pkt)
    {
        _pkt = pkt;
        _pkt.portraitId = 0;
    }

    [TempleDllLocation(0x1017e7d0)]
    public void Activate()
    {
        // TODO ChargenPortraitActivateImpl /*0x1017e700*/();
    }

    [TempleDllLocation(0x1017e470)]
    public bool CheckComplete()
    {
        return _pkt.portraitId != 0;
    }

    [TempleDllLocation(0x1017e480)]
    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        playerObj.SetInt32(obj_f.critter_portrait, charSpec.portraitId);
    }

    [TempleDllLocation(0x1017e4b0)]
    public void UpdateDescriptionBox()
    {
        UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
    }
}