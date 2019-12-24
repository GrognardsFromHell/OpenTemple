using System.Globalization;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.DebugUI;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.D20.Actions
{
    public static class ActionsDebugUi
    {
        private static bool _showCurrentSequence;

        public static void RenderMenuOptions()
        {
            if (ImGui.BeginMenu("Actions"))
            {
                ImGui.MenuItem("Current Sequence", null, ref _showCurrentSequence);
                ImGui.EndMenu();
            }
        }

        public static void Render()
        {
            if (_showCurrentSequence)
            {
                RenderCurrentSequence();
            }
        }

        private static void RenderCurrentSequence()
        {
            if (ImGui.Begin("Current Sequence"))
            {
                var seq = GameSystems.D20.Actions.CurrentSequence;

                if (seq == null)
                {
                    ImGui.Text("NONE");
                }
                else
                {
                    DebugUiUtils.RenderNameLabelPairs(
                        ("Serial", seq.Serial.ToString()),
                        ("Performer", seq.performer?.ToString()),
                        ("Target", seq.targetObj?.ToString())
                    );

                    DebugUiUtils.RenderNameLabelPairs(
                        ("Interrupted", seq.IsInterrupted ? "Yes" : "No"),
                        ("Performing", seq.IsPerforming ? "Yes" : "No"),
                        ("Ignore LoS", seq.ignoreLos ? "Yes" : "No")
                    );

                    for (var i = 0; i < seq.d20ActArray.Count; i++)
                    {
                        var action = seq.d20ActArray[i];

                        ImGui.Text($"#{i + 1}: {action.d20ActType}");
                        if (i == seq.d20aCurIdx)
                        {
                            ImGui.SameLine();
                            ImGui.TextColored(new Vector4(0, 0.8f, 0, 1), "[CURRENT]");
                        }

                        ImGui.Separator();

                        DebugUiUtils.RenderNameLabelPairs(
                            ("Flags", action.d20Caf.ToString()),
                            ("Data", action.data1.ToString())
                        );
                        DebugUiUtils.RenderNameLabelPairs(
                            ("Spell ID", action.spellId.ToString()),
                            ("Anim ID", action.animID.ToString())
                        );
                        DebugUiUtils.RenderNameLabelPairs(
                            ("Spell ID", action.spellId.ToString()),
                            ("Anim ID", action.animID.ToString())
                        );
                        DebugUiUtils.RenderNameLabelPairs(
                            ("Performer", action.d20APerformer?.ToString()),
                            ("Target", action.d20ATarget?.ToString())
                        );
                        DebugUiUtils.RenderNameLabelPairs(
                            ("Location", action.destLoc != LocAndOffsets.Zero ? action.destLoc.ToString() : "-"),
                            ("Path Nodes", action.path?.nodes.Count.ToString()),
                            ("Dist. Traveled", action.distTraversed.ToString(CultureInfo.InvariantCulture))
                        );
                    }
                }

                ImGui.End();
            }
        }
    }
}