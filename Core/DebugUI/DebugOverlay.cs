using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.DebugUI;

public static class DebugOverlay
{
    private static bool _isActive;

    public static void Render(IGameViewport viewport)
    {
        if (ImGui.Begin("Debug Overlay", ref _isActive))
        {
            var mousePt = Globals.UiManager.MousePos;
            var gameViewRect = ((GameView) viewport).GetViewportBorderArea();
            mousePt.X -= gameViewRect.X;
            mousePt.Y -= gameViewRect.Y;

            var mouseWorldPos = viewport.ScreenToTile(mousePt.X, mousePt.Y);
            var centerWorldPos = viewport.ScreenToTile(gameViewRect.Width / 2, gameViewRect.Height / 2);
            ImGui.Text($"Mouse @ {mouseWorldPos}");
            ImGui.Text($"Center @ {centerWorldPos}");
            ImGui.End();

            // Draw a small "cross-hair" at the center of the game view
            var centerOfViewX = gameViewRect.X + gameViewRect.Width / 2f;
            var centerOfViewY = gameViewRect.X + gameViewRect.Height / 2f;
            Span<Line2d> lines = stackalloc Line2d[2]
            {
                new Line2d(
                    new Vector2(centerOfViewX - 10, centerOfViewY),
                    new Vector2(centerOfViewX + 10, centerOfViewY),
                    PackedLinearColorA.White
                ),
                new Line2d(
                    new Vector2(centerOfViewX, centerOfViewY - 10),
                    new Vector2(centerOfViewX, centerOfViewY + 10),
                    PackedLinearColorA.White
                ),
            };
            Tig.ShapeRenderer2d.DrawLines(lines);
        }
    }
}