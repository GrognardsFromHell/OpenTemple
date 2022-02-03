using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;

namespace OpenTemple.Core.Ui;

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
        UiSystems.Dialog.CancelDialog();

        if (GameSystems.MapObject.GlobalStashedObject != null)
        {
            GameSystems.MapObject.GlobalStashedObject = null;
        }
    }

    [TempleDllLocation(0x1014deb0)]
    public void OpenContainer(GameObject actor, GameObject container)
    {
        if (container.GetSpellFlags().HasFlag(SpellFlag.STONED))
        {
            return;
        }

        if (container.type == ObjectType.container
            && GameSystems.Party.IsInParty(actor)
            && GameSystems.Item.HasKey(actor, container.GetInt32(obj_f.container_key_id))
            || !container.NeedsToBeUnlocked())
        {
            UiSystems.Logbook.Hide();
            if (container.type == ObjectType.container)
            {
                GameSystems.Anim.PushAnimate(container, NormalAnimType.Open);
                var containerFlags = container.GetContainerFlags();
                if (!containerFlags.HasFlag(ContainerFlag.HAS_BEEN_OPENED))
                {
                    container.SetContainerFlags(containerFlags | ContainerFlag.HAS_BEEN_OPENED);
                    if (GetContainerTotalGoldWorth(container) >= 1000)
                    {
                        GameSystems.Dialog.PlayTreasureLootingVoiceLine();
                    }
                }
            }

            container.GetFlags();
            if (UiSystems.CharSheet.HasCurrentCritter)
            {
                if (UiSystems.CharSheet.State != CharInventoryState.Looting)
                {
                    UiSystems.CharSheet.State = CharInventoryState.Looting;
                    UiSystems.CharSheet.Looting.Show(container);
                }
            }
            else
            {
                UiSystems.CharSheet.State = CharInventoryState.Looting;
                UiSystems.CharSheet.Show(actor);
                UiSystems.CharSheet.Looting.Show(container);
            }
        }
        else
        {
            var text = _translation[1];
            GameSystems.TextFloater.FloatLine(container, TextFloaterCategory.Generic, TextFloaterColor.Blue,
                text);
        }
    }

    [TempleDllLocation(0x1010ea60)]
    private int GetContainerTotalGoldWorth(GameObject container)
    {
        var totalWorth = 0;
        if (container.type == ObjectType.container)
        {
            foreach (var item in container.EnumerateChildren())
            {
                if (item.type.IsEquipment())
                {
                    totalWorth += item.GetInt32(obj_f.item_worth);
                }
            }
        }

        return totalWorth / 100;
    }

    [TempleDllLocation(0x1014e050)]
    public void InitiateDialog(GameObject actor, GameObject npc)
    {
        if (GameSystems.Script.ExecuteObjectScript(actor, npc, ObjScriptEvent.Dialog) == 1)
        {
            if (GameSystems.AI.HasSurrendered(npc, out var surrenderedTo)
                && GameSystems.Combat.AffiliationSame(surrenderedTo, actor))
            {
                GameSystems.AI.FleeFrom(npc, actor);
            }
            else
            {
                UiSystems.Dialog.SayDefaultResponse(actor, npc);
            }
        }
    }

    [TempleDllLocation(0x1014e190)]
    public void Render(IGameViewport viewport)
    {
        UiSystems.InGameSelect.RenderMovementTargets(viewport);
        UiSystems.InGameSelect.RenderMouseoverOrSth(viewport);
        UiSystems.RadialMenu.Render(); // TODO: Radial Menu should not become a "game" render aspect, but rather a UI render aspect
        UiSystems.InGameSelect.RenderPickers(viewport);
    }
}