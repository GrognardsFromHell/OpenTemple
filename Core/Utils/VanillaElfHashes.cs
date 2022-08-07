using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace OpenTemple.Core.Utils;

public class VanillaElfHashes
{
    private static readonly Dictionary<int, string> Hashes;

    static VanillaElfHashes()
    {
        using var stream =
            typeof(VanillaElfHashes).Assembly.GetManifestResourceStream(typeof(VanillaElfHashes), "elfhashes.json");

        if (stream == null)
        {
            throw new FileNotFoundException("Missing elfhashes.json in Assembly");
        }

        var jsonDocument = JsonDocument.Parse(stream);
        Hashes = new Dictionary<int, string>(jsonDocument.RootElement.GetArrayLength());
        foreach (var pair in jsonDocument.RootElement.EnumerateArray())
        {
            int hash = pair[0].GetInt32();
            var text = pair[1].GetString() ?? throw new JsonException($"Value for Elf-Hash ${hash} is null.");
            Hashes[hash] = text;
        }
    }

    public static bool TryGetVanillaString(int elfHash, [MaybeNullWhen(false)] out string vanillaString)
    {
        return Hashes.TryGetValue(elfHash, out vanillaString);
    }
}