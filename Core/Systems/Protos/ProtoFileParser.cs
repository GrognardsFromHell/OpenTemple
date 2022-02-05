using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.TabFiles;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Systems.Protos;

public class ProtoFileParser
{
    private const string UserProtoDir = "rules/protos/";

    private const string TemplePlusProtoFile = "rules/protos_override.tab";

    private const string VanillaProtoFile = "rules/protos.tab";

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly struct ProtoIdRange
    {
        public readonly int Start;
        public readonly int End;

        public ProtoIdRange(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    private static readonly ProtoIdRange[] ProtoIdRanges =
    {
        new(0, 999), // portal
        new(1000, 1999), // container
        new(2000, 2999), // scenery
        new(3000, 3999), // projectile
        new(4000, 4999), // weapon
        new(5000, 5999), // ammo
        new(6000, 6999), // armor
        new(7000, 7999), // money
        new(8000, 8999), // food
        new(9000, 9999), // scroll
        new(10000, 10999), // key
        new(11000, 11999), // written
        new(12000, 12999), // generic
        new(13000, 13999), // pc
        new(14000, 14999), // npc
        new(15000, 15999), // trap
        new(16000, 16999), // bag
    };

    /// <summary>
    /// ToEE does not include an explicit type-column in the protos file. Instead, the type is inferred
    /// from the proto id.
    /// </summary>
    [TempleDllLocation(0x10039220)]
    private static bool GetObjectTypeFromProtoId(int protoId, out ObjectType type)
    {
        for (var i = 0; i < ProtoIdRanges.Length; i++)
        {
            if (protoId >= ProtoIdRanges[i].Start && protoId <= ProtoIdRanges[i].End)
            {
                type = (ObjectType) i;
                return true;
            }
        }

        type = default;
        return false;
    }

    public delegate void ProtoCallback(GameObject protoObj, int protoId);

    public static List<GameObject> Parse(string path, ProtoCallback objectPreprocessor = null)
    {
        var result = new List<GameObject>();

        void ProcessProtoRecord(in TabFileRecord record)
        {
            var protoId = record[0].GetInt();

            if (!GetObjectTypeFromProtoId(protoId, out var type))
            {
                Logger.Error("Failed to determine object type for proto id {0}", protoId);
                return;
            }

            var obj = GameObject.CreateProto(type, protoId);

            objectPreprocessor?.Invoke(obj, protoId);

            ProtoColumns.ParseColumns(protoId, record, obj);

            result.Add(obj);
        }

        TabFile.ParseFile(path, ProcessProtoRecord);

        return result;
    }

    public static IEnumerable<string> EnumerateProtoFiles(IFileSystem fs)
    {
        yield return VanillaProtoFile;

        if (fs.FileExists(TemplePlusProtoFile))
        {
            yield return TemplePlusProtoFile;
        }

        foreach (var protoFilename in fs.Search(UserProtoDir + "*.tab"))
        {
            yield return protoFilename;
        }
    }
}