using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OpenTemple.Core.Utils
{
    public class VanillaElfHashes
    {
        private static readonly Dictionary<int, string> Hashes;

        static VanillaElfHashes()
        {
            using var stream =
                typeof(VanillaElfHashes).Assembly.GetManifestResourceStream(typeof(VanillaElfHashes), "elfhashes.json");

            var jsonDocument = JsonDocument.Parse(stream);
            Hashes = new Dictionary<int, string>(jsonDocument.RootElement.GetArrayLength());
            foreach (var pair in jsonDocument.RootElement.EnumerateArray())
            {
                int hash = pair[0].GetInt32();
                var text = pair[1].GetString();
                Hashes[hash] = text;
            }
        }

        public static bool TryGetVanillaString(int elfHash, out string vanillaString)
        {
            return Hashes.TryGetValue(elfHash, out vanillaString);
        }
    }
}