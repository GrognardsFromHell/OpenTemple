using System;
using System.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core
{
    public class GameLib
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x103072B8)]
        private bool _ironmanGame = false;

        [TempleDllLocation(0x10306F44)]
        private static int mIronmanSaveNumber;

        [TempleDllLocation(0x103072C0)]
        private string _ironmanSaveName;

        [TempleDllLocation(0x10002E20)]
        [TempleDllLocation(0x103072D4)]
        public bool IsLoading { get; private set; }

        [TempleDllLocation(0x10003860)]
        public bool IsIronmanGame
        {
            get => _ironmanGame;
            set => _ironmanGame = value;
        }

        [TempleDllLocation(0x10003870)]
        public string IronmanSaveName => _ironmanSaveName;

        [TempleDllLocation(0x10004b70)]
        public void SetIronmanSaveName(string name)
        {
            _ironmanSaveName = name;

            throw new NotImplementedException();
//            GsiListCreate /*0x10003070*/(&gsiList);
//            GsiListSort /*0x100049f0*/(&gsiList, 1, 0);
//            v1 = gsiList.count;
//            if (!gsiList.count
//                || (
//                    (v2 = gsiList.savenames, !toupper(*((_BYTE*) *gsiList.savenames + 4) != 65))
//                        ? (v1 <= 1 ? (v3 = "iron0000") : (v3 = v2[1]))
//                        : (v3 = *v2),
//                    strncpy(&v5, v3, 8),
//                    v7 = 0,
//                    GameLib.mIronmanSaveNumber = j__atol__string2num /*0x10253d8a*/(&v6) + 1,
//                    GameLib.mIronmanSaveNumber > 9999))
//            {
//                GameLib.mIronmanSaveNumber = 0;
//            }
//
//            GameSaveInfoClear /*0x10003140*/(&gsiList);
        }

        [TempleDllLocation(0x10004870)]
        public bool IronmanSave()
        {
            if (_ironmanGame && _ironmanSaveName != null)
            {
                var filename = $"iron{mIronmanSaveNumber:D4}";
                return SaveGame(filename, _ironmanSaveName);
            }

            return false;
        }

        [TempleDllLocation(0x100048d0)]
        public bool KillIronmanSave()
        {
            if (_ironmanGame && _ironmanSaveName != null)
            {
                var saveName = $"iron{mIronmanSaveNumber:D4}{_ironmanSaveName}";
                Logger.Info("Deleting Ironman savegame {0} upon total party kill.");
                if (DeleteSave(saveName))
                {
                    return true;
                }
            }

            return false;
        }

        // Makes a savegame.
        [TempleDllLocation(0x100042c0)]
        public static bool SaveGame(string filename, string displayName)
        {
            throw new NotImplementedException(); // TODO
        }

        // Loads a game.
        [TempleDllLocation(0x100028d0)]
        public static bool LoadGame(string filename)
        {
            throw new NotImplementedException(); // TODO
        }

        [TempleDllLocation(0x10002d30)]
        public bool DeleteSave(string saveName)
        {
            if (saveName == "SlotQwikQuick-Save" || saveName == "SlotAutoAuto-Save")
            {
                return false;
            }

            bool TryDelete(string filename)
            {
                var path = Path.Join(Globals.GameFolders.SaveFolder, filename);
                try
                {
                    File.Delete(path);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to delete save file {0}: {1}", path, e);
                    return false;
                }
            }

            var success = TryDelete(saveName + ".gsi");
            if (!TryDelete(saveName + "l.jpg"))
            {
                success = false;
            }

            if (!TryDelete(saveName + "s.jpg"))
            {
                success = false;
            }

            if (!TryDelete(saveName + "s.tfaf"))
            {
                success = false;
            }

            if (!TryDelete(saveName + "s.tfai"))
            {
                success = false;
            }

            return success;
        }

        [TempleDllLocation(0x10001db0)]
        public void Reset()
        {
            GameSystems.ResetGame();

            // TODO: Not fully implemented
            _ironmanSaveName = null;
            _ironmanGame = false;
        }

        [TempleDllLocation(0x10002e50)]
        public bool IsAutosaveBetweenMaps => Globals.Config.GetVanillaInt("autosave_between_maps") != 0;

        [TempleDllLocation(0x10004990)]
        public void MakeAutoSave()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10004930)]
        public void QuickSave()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10005680)]
        public void QuickLoad()
        {
            throw new NotImplementedException();
        }

    }
}