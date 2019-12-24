using System;
using System.Diagnostics;
using System.IO;
using OpenTemple.Core.IO.SaveGames.Archive;
using OpenTemple.Core.IO.SaveGames.Co8State;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.IO.SaveGames.UiState;

namespace OpenTemple.Core.IO.SaveGames
{
    public class SaveGameFile
    {
        public SavedGameState GameState { get; set; }

        public SavedUiState UiState { get; set; }

        // Keep in mind that this is optional
        public SavedCo8State Co8State { get; set; }

        private static void CopyStreamToFile(Stream stream, int length, string path)
        {
            using var outputStream = new FileStream(path, FileMode.Create);
            Span<byte> buffer = stackalloc byte[4096];
            while (length > 0)
            {
                var actuallyCopied = stream.Read(length > buffer.Length ? buffer : buffer.Slice(0, length));

                if (actuallyCopied == 0)
                {
                    throw new EndOfStreamException();
                }

                outputStream.Write(buffer.Slice(0, actuallyCopied));
                length -= actuallyCopied;
            }
        }

        public static SaveGameFile Load(string basePath, string currentSaveDir)
        {
            var result = new SaveGameFile();

            var indexPath = basePath + ".tfai";

            var archiveIndex = ArchiveIndexReader.ReadIndex(indexPath);
            using var dataStream = new FileStream(basePath + ".tfaf", FileMode.Open);

            byte[] gameStateData = null;
            byte[] spellPacketData = null;
            byte[] partyConfigData = null;
            byte[] mapFleeData = null;
            byte[] uiStateData = null;

            bool GrabData(ArchiveIndexEntry entry, string filename, ref byte[] bufferOut)
            {
                if (entry.Path == filename)
                {
                    if (bufferOut != null)
                    {
                        throw new CorruptSaveException($"File {filename} exists twice in the save game!");
                    }

                    var buffer = new byte[entry.Size];
                    dataStream.Read(buffer);
                    if (Debugger.IsAttached)
                    {
                        var fullPath = Path.Join(currentSaveDir, entry.Path);
                        File.WriteAllBytes(fullPath, buffer);
                    }

                    bufferOut = buffer;
                    return true;
                }

                return false;
            }

            foreach (var entry in archiveIndex)
            {
                var fullPath = Path.Join(currentSaveDir, entry.Path);

                if (entry.Directory)
                {
                    Directory.CreateDirectory(fullPath);
                    continue;
                }

                if (GrabData(entry, "data.sav", ref gameStateData)
                    || GrabData(entry, "action_sequencespellpackets.bin", ref spellPacketData)
                    || GrabData(entry, "partyconfig.bin", ref partyConfigData)
                    || GrabData(entry, "map_mapflee.bin", ref mapFleeData)
                    || GrabData(entry, "data2.sav", ref uiStateData))
                {
                    continue;
                }

                CopyStreamToFile(dataStream, entry.Size, fullPath);
            }

            if (gameStateData == null)
            {
                throw new CorruptSaveException("Save file is missing data.sav");
            }

            if (spellPacketData == null)
            {
                throw new CorruptSaveException("Save file is missing action_sequencespellpackets.bin");
            }

            if (partyConfigData == null)
            {
                throw new CorruptSaveException("Save file is missing partyconfig.bin");
            }

            if (mapFleeData == null)
            {
                throw new CorruptSaveException("Save file is missing map_mapflee.bin");
            }

            if (uiStateData == null)
            {
                throw new CorruptSaveException("Save file is missing data2.sav");
            }

            result.GameState = SavedGameState.Load(gameStateData, spellPacketData, partyConfigData, mapFleeData);

            result.UiState = SavedUiState.Load(uiStateData);

            // Load the optional Co8 data if it exists
            var co8Path = basePath + ".co8";
            if (File.Exists(co8Path))
            {
                result.Co8State = SavedCo8State.Load(co8Path);
            }

            return result;
        }
    }

    public class CorruptSaveException : Exception
    {
        public CorruptSaveException(string message) : base(message)
        {
        }
    }
}