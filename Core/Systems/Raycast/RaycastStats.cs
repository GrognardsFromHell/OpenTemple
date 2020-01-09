using System;
using ImGuiNET;

namespace OpenTemple.Core.Systems.Raycast
{
    public static class RaycastStats
    {
        public static long RaycastTotal { get; private set; }
        public static long PointChecks { get; private set; }

        public static TimeSpan NewRaycastTime { get; set; }
        public static TimeSpan LegacyRaycastTime { get; set; }

        public static void RecordRaycast()
        {
            RaycastTotal++;
        }

        public static void Render()
        {
            if (ImGui.Begin("Raycast Stats"))
            {
                ImGui.LabelText("Raycast Count:", RaycastTotal.ToString());
                ImGui.LabelText("Points Checked:", PointChecks.ToString());
                ImGui.LabelText("New raycast time:", NewRaycastTime.ToString());
                ImGui.LabelText("Old raycast time:", LegacyRaycastTime.ToString());
                ImGui.End();
            }
        }

        public static void RecordIsPointCloseToSegment()
        {
            PointChecks++;
        }
    }
}