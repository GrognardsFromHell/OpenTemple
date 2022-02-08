using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.GameObjects;

namespace ConvertMapToText;

public static class ObjectSerializer
{
    public static void WriteProperties(Utf8JsonWriter writer,
        Dictionary<obj_f, object> properties,
        string debugName)
    {
        var sortedKeys = properties.Keys.ToList();
        sortedKeys.Sort();

        foreach (var field in sortedKeys)
        {
            if (IgnoredFields.Contains(field))
            {
                continue;
            }

            try
            {
                WriteField(writer, properties, field);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to write field {field} for {debugName}: {e}");
            }
        }
    }

    private static void WriteField(Utf8JsonWriter writer, Dictionary<obj_f, object> properties, obj_f field)
    {
        // Special handling that spans different fields
        switch (field)
        {
            case obj_f.permanent_mods:
                writer.WritePropertyName("permanent_mod_args");
                WriteConditions(writer, obj_f.permanent_mods, obj_f.permanent_mod_data, properties);
                break;
            case obj_f.permanent_mod_data:
                // Handled by permanent_mods
                break;
            case obj_f.item_pad_wielder_condition_array:
                writer.WritePropertyName("item_wielder_conditions");
                WriteConditions(writer,
                    obj_f.item_pad_wielder_condition_array,
                    obj_f.item_pad_wielder_argument_array,
                    properties);
                break;
            case obj_f.item_pad_wielder_argument_array:
                // Handled by item_pad_wielder_condition_array
                break;
            case obj_f.npc_standpoints:
                WriteNpcStandpoints(writer, properties);
                break;
            case obj_f.armor_flags:
                WriteArmorFlags(writer, (ArmorFlag) (int) properties[field]);
                break;
            default:
                writer.WriteField(field, properties[field]);
                break;
        }
    }

    private static void WriteArmorFlags(Utf8JsonWriter writer, ArmorFlag flags)
    {
        switch (flags.GetArmorType())
        {
            case ArmorFlag.TYPE_LIGHT:
                writer.WriteString("armor_type", "light");
                break;
            case ArmorFlag.TYPE_MEDIUM:
                writer.WriteString("armor_type", "medium");
                break;
            case ArmorFlag.TYPE_HEAVY:
                writer.WriteString("armor_type", "heavy");
                break;
            case ArmorFlag.TYPE_SHIELD:
                writer.WriteString("armor_type", "shield");
                break;
            case ArmorFlag.TYPE_NONE:
                writer.WriteString("armor_type", "none");
                break;
        }

        switch (flags & ArmorFlag.HELM_BITMASK)
        {
            case ArmorFlag.HELM_TYPE_SMALL:
                writer.WriteString("helmet_type", "light");
                break;
            case ArmorFlag.HELM_TYPE_MEDIUM:
                writer.WriteString("helmet_type", "medium");
                break;
            case ArmorFlag.HELM_TYPE_LARGE:
                writer.WriteString("helmet_type", "large");
                break;
        }
    }

    private static void WriteConditions(Utf8JsonWriter writer,
        obj_f namesField,
        obj_f argsField,
        Dictionary<obj_f, object> properties)
    {
        var modNames = (IReadOnlyList<int>) properties[namesField];
        var modData = (IReadOnlyList<int>) properties.GetValueOrDefault(argsField, new int[0]);

        writer.WriteStartArray();

        var argIdx = 0;
        foreach (var modNameHash in modNames)
        {
            var condition = GameSystems.D20.Conditions.GetByHash(modNameHash);

            if (condition.numArgs == 0)
            {
                writer.WriteStringValue(condition.condName);
                continue;
            }

            writer.WriteStartArray();
            writer.WriteStringValue(condition.condName);
            for (var i = 0; i < condition.numArgs; i++)
            {
                writer.WriteNumberValue(modData[argIdx++]);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }

    private static void WriteNpcStandpoints(Utf8JsonWriter writer, Dictionary<obj_f, object> properties)
    {
        if (!properties.Remove(obj_f.npc_standpoints, out var standpointsObj))
        {
            return;
        }

        var standpoints = (IReadOnlyList<long>) standpointsObj;

        var standPointDay = GameObject.DeserializeStandpoint(standpoints, 0);
        var standPointNight = GameObject.DeserializeStandpoint(standpoints, 1);
        var standPointScout = GameObject.DeserializeStandpoint(standpoints, 2);

        writer.WriteStartObject("npc_standpoints");
        writer.WriteStartObject("day");
        WriteStandPoint(writer, standPointDay);
        writer.WriteEndObject();

        writer.WriteStartObject("night");
        WriteStandPoint(writer, standPointNight);
        writer.WriteEndObject();

        if (standPointScout.location != LocAndOffsets.Zero)
        {
            writer.WriteStartObject("scout");
            WriteStandPoint(writer, standPointScout);
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    private static void WriteStandPoint(Utf8JsonWriter writer, StandPoint standPoint)
    {
        writer.WriteNumber("mapId", standPoint.mapId);
        writer.WritePropertyName("location");
        // While World-Ed generally supports setting off_x, off_z (in some cases)
        // many objects just contain junk in the offset fields.
        writer.WriteTile(standPoint.location.location);
        writer.WriteNumber("jumpPointId", standPoint.jumpPointId);
    }

    private static readonly ISet<obj_f> IgnoredFields = new HashSet<obj_f>
    {
        // This just contains stale data, no idea why this was ever saved
        obj_f.dispatcher,
        obj_f.critter_inventory_num
    };
}