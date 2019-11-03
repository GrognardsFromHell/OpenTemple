using System.Numerics;
using ImGuiNET;

namespace SpicyTemple.Core.DebugUI
{
    public static class DebugUiUtils
    {

        public static void RenderNameLabelPairs(params (string, string)[] pairs)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
            ImGui.Columns(pairs.Length, null, false);

            foreach (var (_, value) in pairs)
            {
                ImGui.Text(value ?? "");
                ImGui.NextColumn();
            }

            for (var index = 0; index < pairs.Length; index++)
            {
                var drawList = ImGui.GetWindowDrawList();
                var pos = ImGui.GetCursorScreenPos();
                drawList.AddLine(new Vector2(pos.X - 9999, pos.Y), new Vector2(pos.X + 9999, pos.Y), 0xFF999999);
                ImGui.NextColumn();
            }

            ImGui.SetWindowFontScale(0.8f);
            foreach (var (label, _) in pairs)
            {
                ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1), label);
                ImGui.NextColumn();
            }

            ImGui.SetWindowFontScale(1.0f);
            ImGui.Columns(1);
            ImGui.PopStyleVar();
        }

    }
}