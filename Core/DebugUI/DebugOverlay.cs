using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;


namespace OpenTemple.Core.DebugUI
{

    

    public static class DebugOverlay
    {

        static bool isActive = false;

        public static void Render()
        {
            
            if (ImGui.Begin( "Debug Overlay", ref isActive)){

                var mousePt = TigSubsystems.Tig.Mouse.GetPos();

                var camera = TigSubsystems.Tig.RenderingDevice.GetCamera();
                var worldCoord = camera.ScreenToTile(mousePt.X, mousePt.Y);
                ImGui.Text($"{worldCoord}");
                ImGui.End();
            }
        }
    }
}
