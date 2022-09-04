using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.DebugUI;

public static class DebugOverlay
{
    private static bool _isActive;

    public static void Render(IGameViewport viewport)
    {
        if (ImGui.Begin( "Debug Overlay", ref _isActive))
        {
            var mousePt = Globals.UiManager.MousePos;

            var worldCoord = viewport.ScreenToTile(mousePt.X, mousePt.Y);
            ImGui.Text($"{worldCoord}");
            ImGui.End();
        }
    }
}