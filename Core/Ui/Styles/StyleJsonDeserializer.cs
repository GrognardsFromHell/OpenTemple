
using System.Text.Json;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Ui.Styles;

public static partial class StyleJsonDeserializer
{

    private static string? ReadString(in JsonElement element) => element.GetString();

    private static int? ReadInt(in JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Null ? null : element.GetInt32();
    }

    private static float? ReadFloat(in JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Null ? null : element.GetSingle();
    }

    private static bool? ReadBool(in JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Null ? null : element.GetBoolean();
    }

    private static PackedLinearColorA? ReadColor(in JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Null ? null : element.GetColor();
    }

}