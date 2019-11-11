using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.DebugUI
{
    public static class ObjectEditors
    {
        private static readonly List<ObjectEditor> Editors = new List<ObjectEditor>();

        public static void Edit(GameObjectBody obj)
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
            foreach (var editor in Editors)
            {
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
}