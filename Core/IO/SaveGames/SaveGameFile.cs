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
        private const string MainStateFile = "data.sav";

        private const string UiStateFile = "data2.sav";

        private const string ActionSequencesSpellsFile = "action_sequencespellpackets.bin";

        private const string PartyConfigFile = "partyconfig.bin";

        private const string MapFleeFile = "map_mapflee.bin";

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

        public void Save(string basePath, string currentSaveDir)
        {
            if (GameState == null)
            {
                throw new CorruptSaveException("Missing GameState.");
            }

            if (UiState == null)
            {
                throw new CorruptSaveException("Missing UiState.");
            }

            var serializedGameState = SavedGameState.Save(GameState, Co8State != null);

            var serializedUiState = SavedUiState.Save(UiState, Co8State != null);

            // Write the files to the current save directory so they are added to the save archive afterwards
            WriteStateToDirectory(serializedGameState, serializedUiState, currentSaveDir);

            try
            {
                ArchiveWriter.Compress(basePath + ".tfai", basePath + ".tfaf", currentSaveDir);
            }
            finally
            {
                DeleteStateFromDirectory(currentSaveDir);
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

                if (GrabData(entry, MainStateFile, ref gameStateData)
                    || GrabData(entry, ActionSequencesSpellsFile, ref spellPacketData)
                    || GrabData(entry, PartyConfigFile, ref partyConfigData)
                    || GrabData(entry, MapFleeFile, ref mapFleeData)
                    || GrabData(entry, UiStateFile, ref uiStateData))
                {
                    continue;
                }

                CopyStreamToFile(dataStream, entry.Size, fullPath);
            }

            if (gameStateData == null)
            {
                throw new CorruptSaveException($"Save file is missing {MainStateFile}");
            }

            if (spellPacketData == null)
            {
                throw new CorruptSaveException($"Save file is missing {ActionSequencesSpellsFile}");
            }

            if (partyConfigData == null)
            {
                throw new CorruptSaveException($"Save file is missing {PartyConfigFile}");
            }

            if (mapFleeData == null)
            {
                throw new CorruptSaveException($"Save file is missing {MapFleeFile}");
            }

            if (uiStateData == null)
            {
                throw new CorruptSaveException($"Save file is missing {UiStateFile}");
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

        private void WriteStateToDirectory(SerializedGameState gameState, byte[] uiState, string currentSaveDir)
        {
            File.WriteAllBytes(Path.Join(currentSaveDir, MainStateFile), gameState.MainContent);
            File.WriteAllBytes(Path.Join(currentSaveDir, ActionSequencesSpellsFile), gameState.ActionSequenceSpells);
            File.WriteAllBytes(Path.Join(currentSaveDir, PartyConfigFile), gameState.PartyConfig);
            File.WriteAllBytes(Path.Join(currentSaveDir, MapFleeFile), gameState.FleeData);
            File.WriteAllBytes(Path.Join(currentSaveDir, UiStateFile), uiState);
        }

        private void DeleteStateFromDirectory(string currentSaveDir)
        {
            File.Delete(Path.Join(currentSaveDir, MainStateFile));
            File.Delete(Path.Join(currentSaveDir, ActionSequencesSpellsFile));
            File.Delete(Path.Join(currentSaveDir, PartyConfigFile));
            File.Delete(Path.Join(currentSaveDir, MapFleeFile));
            File.Delete(Path.Join(currentSaveDir, UiStateFile));
        }
    }

    public class CorruptSaveException : Exception
    {
        public CorruptSaveException(string message) : base(message)
        {
        }
    }
}