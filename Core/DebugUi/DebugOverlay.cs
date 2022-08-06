﻿using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.DebugUi;

public static class DebugOverlay
{

    static bool isActive = false;

    public static void Render(IGameViewport viewport)
    {
            
        if (ImGui.Begin( "Debug Overlay", ref isActive)){

            var mousePt = Globals.UiManager.Mouse.GetPos();

            var worldCoord = viewport.ScreenToTile(mousePt.X, mousePt.Y);
            ImGui.Text($"{worldCoord}");
            ImGui.End();
        }
    }
}