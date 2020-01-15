using System;
using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.IO.SaveGames.GameState
{
    public class SavedAreaState
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public ISet<int> DiscoveredAreas { get; set; } = new HashSet<int>();

        // ID of the area that was discovered last (or 0 if none)
        public int AreaDiscoveredLast { get; set; }

        [TempleDllLocation(0x1006e8d0)]
        public static SavedAreaState Read(BinaryReader reader)
        {
            // This reader will handle it's own sentinel because we need it to figure out the area count
            // without reading gamearea.mes. Sadly it was never stored in the save game to begin with.
            Span<byte> buffer = stackalloc byte[256 + 4 + 4]; // We will support at most 256 areas
            var bufferLen = 0;

            // Read bytes until the trailing bytes in the buffer are 0xBE_EF_CA_FE
            while (!BufferEndIsSentinel(buffer, bufferLen))
            {
                if (bufferLen >= buffer.Length)
                {
                    throw new CorruptSaveException("Couldn't find sentinel in the area save data after reading "
                                                   + $"{bufferLen} bytes.");
                }

                buffer[bufferLen++] = reader.ReadByte();
            }

            // Subtract the 32-bit int containing the last discovered area and the size of the sentinel
            var areaCount = bufferLen - 4 - 4;

            var result = new SavedAreaState();
            for (var areaId = 0; areaId < areaCount; areaId++)
            {
                if (buffer[areaId] != 0)
                {
                    result.DiscoveredAreas.Add(areaId);
                }
            }

            result.AreaDiscoveredLast = BitConverter.ToInt32(buffer.Slice(areaCount, 4));
            return result;
        }

        public void Write(BinaryWriter writer, bool co8Extensions)
        {
            // Co8 uses a hacked DLL with more areas
            var areaCount = co8Extensions ? 20 : 13;
            Span<byte> areasVisited = stackalloc byte[areaCount];
            foreach (var areaId in DiscoveredAreas)
            {
                if (areaId < areasVisited.Length)
                {
                    areasVisited[areaId] = 1;
                }
                else
                {
                    Logger.Error("Cannot save visited area {0} because it's larger than the supported area count {1}",
                        areaId, areaCount);
                }
            }

            writer.Write(AreaDiscoveredLast); // Never used anyway
        }

        private static bool BufferEndIsSentinel(ReadOnlySpan<byte> buffer, int length)
        {
            if (length < 4)
            {
                return false;
            }

            return buffer[length - 1] == 0xBE
                   && buffer[length - 2] == 0xEF
                   && buffer[length - 3] == 0xCA
                   && buffer[length - 4] == 0xFE;
        }
    }
}