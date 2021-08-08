using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace OpenTemple.Core.Systems.Protos
{
    internal readonly struct EnumIntMapping
    {
        private struct MappingEntry
        {
            public byte[] EncodedKey;
            public string Key;
            public int Value;
        }

        private readonly MappingEntry[] _mappingEntries;

        private EnumIntMapping(MappingEntry[] mappingEntries)
        {
            _mappingEntries = mappingEntries;
        }

        [Pure]
        public bool TryGetValue(ReadOnlySpan<byte> literal, out int value)
        {
            for (int j = 0; j < _mappingEntries.Length; j++)
            {
                ref var mapping = ref _mappingEntries[j];
                ReadOnlySpan<byte> key = mapping.EncodedKey;
                if (key.SequenceEqual(literal))
                {
                    value = mapping.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }


        [Pure]
        public bool TryGetValue(ReadOnlySpan<char> literal, out int value)
        {
            for (int j = 0; j < _mappingEntries.Length; j++)
            {
                ref var mapping = ref _mappingEntries[j];
                if (literal.Equals(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    value = mapping.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        [Pure]
        public bool TryGetValueIgnoreCase(ReadOnlySpan<byte> literal, out int value)
        {
            for (int j = 0; j < _mappingEntries.Length; j++)
            {
                ref var mapping = ref _mappingEntries[j];
                ReadOnlySpan<byte> key = mapping.EncodedKey;
                if (CompareIgnoringCase(key, literal))
                {
                    value = mapping.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool CompareIgnoringCase(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                var charA = a[i];
                var charB = a[i];
                if (charA != charB && char.ToUpperInvariant((char) charA) != char.ToUpperInvariant((char) charB))
                {
                    return false;
                }
            }

            return true;
        }

        public static EnumIntMapping Create<T>(Dictionary<string, T> mapping) where T : Enum, IConvertible
        {
            var mappingEntries = new MappingEntry[mapping.Count];
            int idx = 0;
            foreach (var (key, value) in mapping)
            {
                mappingEntries[idx].EncodedKey = Encoding.Default.GetBytes(key);
                mappingEntries[idx].Key = key;
                mappingEntries[idx].Value = unchecked((int) value.ToInt64(null));
                idx++;
            }

            return new EnumIntMapping(mappingEntries);
        }

    }
}