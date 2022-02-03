using System;
using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.DebugUI;

public class DebugObjectGraph
{
    private static readonly ObjectType[] ObjectTypes = (ObjectType[]) Enum.GetValues(typeof(ObjectType));

    public void Render()
    {
        if (ImGui.Begin("Objects"))
        {
            if (ImGui.CollapsingHeader("Dynamic Objects"))
            {
                foreach (var obj in GameSystems.Object.EnumerateNonProtos())
                {
                    if (obj.HasFlag(ObjectFlag.INVENTORY) || obj.IsStatic())
                    {
                        continue;
                    }

                    RenderObjectNode(obj);
                }
            }

            if (ImGui.CollapsingHeader("Static Objects"))
            {
                foreach (var obj in GameSystems.Object.EnumerateNonProtos())
                {
                    if (obj.HasFlag(ObjectFlag.INVENTORY) || !obj.IsStatic())
                    {
                        continue;
                    }

                    RenderObjectNode(obj);
                }
            }

            if (ImGui.CollapsingHeader("Prototypes"))
            {
                foreach (var objectType in ObjectTypes)
                {
                    if (ImGui.CollapsingHeader(objectType.ToString()))
                    {
                        foreach (var proto in GameSystems.Proto.EnumerateProtos(objectType))
                        {
                            var header = proto.id.PrototypeId + " - " +
                                         GameSystems.MapObject.GetDisplayName(proto);
                            if (ImGui.TreeNode(header))
                            {
                                RenderObjectInfo(proto);

                                ImGui.TreePop();
                            }
                        }
                    }
                }
            }

            ImGui.End();
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class IdAttachment
    {
        private static long _counter = 1;

        private static readonly ConditionalWeakTable<GameObject, IdAttachment> Table
            = new ConditionalWeakTable<GameObject, IdAttachment>();

        public long Id { get; }

        public IdAttachment()
        {
            Id = _counter++;
        }

        public static long GetId(GameObject obj)
        {
            return Table.GetOrCreateValue(obj).Id;
        }
    }

    private void RenderObjectNode(GameObject obj)
    {
        var header = obj.type + " - " + GameSystems.MapObject.GetDisplayName(obj);

        long id = IdAttachment.GetId(obj);
        ImGui.PushID(id.ToString());

        var expanded = ImGui.TreeNode("TreeNode", header);
        ImGui.SameLine(0, 5);

        if (ImGui.SmallButton("Edit"))
        {
            ObjectEditors.Edit(obj);
        }

        if (expanded)
        {

            RenderObjectInfo(obj);

            foreach (var childObj in obj.EnumerateChildren())
            {
                RenderObjectNode(childObj);
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void RenderObjectInfo(GameObject obj)
    {
        if (!obj.IsProto())
        {
            ImGui.Text($"Proto: {obj.ProtoId}");
        }

        foreach (var field in ObjectFields.GetTypeFields(obj.type))
        {
            if (!obj.HasOwnDataForField(field))
            {
                continue;
            }

            // Special field handling
            switch (field)
            {
                case obj_f.location:
                {
                    var loc = obj.GetLocation();
                    ImGui.Text($"Location: X: {loc.locx}  Y: {loc.locy}");
                }
                    continue;
            }

            var labelText = field + ": ";

            var fieldType = ObjectFields.GetType(field);
            switch (fieldType)
            {
                case ObjectFieldType.Int32:
                {
                    var value = obj.GetInt32(field);
                    ImGui.Text(labelText + value);
                }
                    break;
                case ObjectFieldType.Int64:
                {
                    var value = obj.GetInt64(field);
                    ImGui.Text(labelText + value);
                }
                    break;
                case ObjectFieldType.AbilityArray:
                    break;
                case ObjectFieldType.UnkArray:
                    break;
                case ObjectFieldType.Int32Array:
                    break;
                case ObjectFieldType.Int64Array:
                    break;
                case ObjectFieldType.ScriptArray:
                    break;
                case ObjectFieldType.Unk2Array:
                    break;
                case ObjectFieldType.String:
                {
                    var value = obj.GetString(field);
                    ImGui.Text(labelText + value);
                }
                    break;
                case ObjectFieldType.Obj:
                {
                    var value = obj.GetObject(field);
                    ImGui.Text(labelText + value);
                }
                    break;
                case ObjectFieldType.ObjArray:
                    break;
                case ObjectFieldType.SpellArray:
                    break;
                case ObjectFieldType.Float32:
                {
                    var value = obj.GetFloat(field);
                    ImGui.Text(labelText + value);
                }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}