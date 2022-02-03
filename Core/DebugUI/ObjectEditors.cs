using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.DebugUI;

public static class ObjectEditors
{
    private static readonly List<ObjectEditor> Editors = new();

    public static void Edit(GameObject obj)
    {
        foreach (var otherEditor in Editors)
        {
            if (otherEditor.Object == obj)
            {
                otherEditor.Active = true;
                return;
            }
        }

        Editors.Add(new ObjectEditor(obj));
    }

    public static void Render()
    {
        // i-based loop because the list can be modified
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < Editors.Count; i++)
        {
            var editor = Editors[i];
            ImGui.SetNextWindowSize(new Vector2(400, 600));

            var flags = ImGuiWindowFlags.AlwaysVerticalScrollbar
                        | ImGuiWindowFlags.NoCollapse
                        | ImGuiWindowFlags.NoSavedSettings;
            if (ImGui.Begin(editor.Title + "###ObjEditor" + editor.Object.id, ref editor.Active, flags))
            {
                editor.Render();
                ImGui.End();
            }
        }

        // Clean up closed editors
        for (var i = Editors.Count - 1; i >= 0; i--)
        {
            if (!Editors[i].Active)
            {
                Editors.RemoveAt(i);
            }
        }
    }
}