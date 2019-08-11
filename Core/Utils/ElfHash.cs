using System;
using System.Text;

namespace SpicyTemple.Core.Utils
{
    public static class ElfHash
    {
        public static int Hash(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
            {
                return 0;
            }

            Span<byte> encodedString = stackalloc byte[Encoding.Default.GetMaxByteCount(text.Length)];
            var actualBytes = Encoding.Default.GetBytes(text, encodedString);

            return Hash(encodedString.Slice(0, actualBytes));
        }

        [TempleDllLocation(0x101ebb00)]
        public static int Hash(ReadOnlySpan<byte> text)
        {
            uint hash = 0, g;

            if (text.IsEmpty)
            {
                return 0;
            }

            foreach (var encodedCh in text)
            {
                var ch = encodedCh;

                // ToEE uses upper case elf hashes
                if (ch >= 'a' && ch <= 'z')
                {
                    ch -= 32;
                }

                hash = (hash << 4) + ch;
                g = (uint) (hash & 0xF0000000L);
                if (g != 0)
                {
                    hash ^= g >> 24;
                }

                hash &= ~g;
            }

            return unchecked((int) hash);
        }
    }
}