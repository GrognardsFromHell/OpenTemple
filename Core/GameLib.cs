using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script.Hooks;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core
{
    public class GameLib
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x103072B8)]
        private bool _ironmanGame = false;

        [TempleDllLocation(0x10306F44)]
        private static int _ironmanSaveSlot;

        [TempleDllLocation(0x103072C0)]
        private string _ironmanSaveName;

        [TempleDllLocation(0x10002E20)]
        [TempleDllLocation(0x103072D4)]
        public bool IsLoading { get; private set; }

        [TempleDllLocation(0x102abee0)]
        [TempleDllLocation(0x10002810)]
        public string ModuleName { get; set; }

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
        public void IronmanSave()
        {
            if (_ironmanGame && _ironmanSaveName != null)
            {
                var filename = Path.Join(Globals.GameFolders.SaveFolder, $"iron{_ironmanSaveSlot:0000}");
                SaveGame(filename, _ironmanSaveName);
            }
        }

        [TempleDllLocation(0x100048d0)]
        public bool KillIronmanSave()
        {
            if (_ironmanGame && _ironmanSaveName != null)
            {
                var save = GetSaveGames().Find(s => s.Type == SaveGameType.IronMan
                                                    && s.Name == _ironmanSaveName);
                if (save != null)
                {
                    Logger.Info("Deleting Ironman savegame {0} upon total party kill.");
                    if (DeleteSave(save))
                    {
                        return true;
                    }
                }
                else
                {
                    Logger.Warn("Failed to find irongame save game '{0}'", _ironmanSaveName);
                }
            }

            return false;
        }

        // Makes a savegame.
        [TempleDllLocation(0x100042c0)]
        public void SaveGame(string filename, string displayName)
        {
            Logger.Info("Saving game to {0} (Display: {1})", filename, displayName);

            var sw = Stopwatch.StartNew();

            var currentSaveFolder = Globals.GameFolders.CurrentSaveFolder;

            if (!Directory.Exists(currentSaveFolder))
            {
                throw new InvalidOperationException("Cannot save the game because no current game folder exists: "
                                                    + currentSaveFolder);
            }

            // Capture game system state
            var saveGameFile = new SaveGameFile();
            saveGameFile.GameState = new SavedGameState
            {
                IsIronmanSave = IsIronmanGame,
                IronmanSaveName = _ironmanSaveName,
                IronmanSlotNumber = _ironmanSaveSlot
            };

            GameSystems.SaveGameState(saveGameFile.GameState);

            saveGameFile.UiState = new SavedUiState();
            UiSystems.SaveGameState(saveGameFile.UiState);

            // Allow mods to load their own data from the savegame
            var saveGameHook = GameSystems.Script.GetHook<ISaveGameHook>();
            saveGameHook?.OnAfterSave(Globals.GameFolders.CurrentSaveFolder, saveGameFile);

            saveGameFile.Save(filename, currentSaveFolder);

            var saveInfo = new SaveGameInfo();
            saveInfo.Name = displayName;
            saveInfo.ModuleName = "ToEE";
            saveInfo.GameTime = GameSystems.TimeEvent.GameTime.ToGameTime();
            saveInfo.MapId = GameSystems.Map.GetCurrentMapId();
            var leader = GameSystems.Party.GetLeader();
            if (leader != null)
            {
                saveInfo.LeaderName = GameSystems.MapObject.GetDisplayName(leader);
                saveInfo.LeaderPortrait = leader.GetInt32(obj_f.critter_portrait);
                saveInfo.LeaderLevel = leader.GetStat(Stat.level);
                saveInfo.LeaderLoc = leader.GetLocation();
            }
            else
            {
                saveInfo.LeaderName = "";
            }
            SaveGameInfoWriter.Write(filename + displayName + ".gsi", saveInfo);

            Tig.MainWindow.TakeScreenshot(Path.Join(filename + "l.jpg"), 256, 192);
            Tig.MainWindow.TakeScreenshot(Path.Join(filename + "s.jpg"), 64, 48);

            Logger.Info("Saved in {0}ms", sw.ElapsedMilliseconds);

        }

        [TempleDllLocation(0x100028d0)]
        public bool LoadGame(SaveGameInfo saveGame)
        {
            Logger.Debug("Loading savegame {0}", saveGame.Path);

            IsLoading = true;

            try
            {
                Stub.TODO("Call to old main menu function here"); // TODO 0x1009a590

                var currentSaveFolder = Globals.GameFolders.CurrentSaveFolder;
                Logger.Debug("Removing current save directory {0}", currentSaveFolder);
                try
                {
                    Directory.Delete(currentSaveFolder, true);
                }
                catch (IOException e)
                {
                    Logger.Error("Error clearing folder {0}: {1}", currentSaveFolder, e);
                    return false;
                }

                try
                {
                    Directory.CreateDirectory(currentSaveFolder);
                }
                catch (IOException e)
                {
                    Logger.Error("Error re-creating folder {0}: {1}", currentSaveFolder, e);
                    return false;
                }

                Logger.Info("Restoring save archive...");

                SaveGameFile saveGameFile;
                try
                {
                    saveGameFile = SaveGameFile.Load(saveGame.BasePath, currentSaveFolder);
                }
                catch (Exception e)
                {
                    Logger.Error("Error loading save game {0}: {1}", saveGame.Path, e);
                    return false;
                }

                var gameState = saveGameFile.GameState;

                _ironmanGame = gameState.IsIronmanSave;
                _ironmanSaveSlot = gameState.IronmanSlotNumber;
                _ironmanSaveName = gameState.IronmanSaveName;

                Stub.TODO("Old main menu related call here"); //  TODO 0x1009a5a0

                Logger.Info("Loading game state from save game.");
                GameSystems.LoadGameState(saveGameFile.GameState);

                Logger.Info("Loading UI data from save game.");
                UiSystems.LoadGameState(saveGameFile.UiState);

                Stub.TODO("Old main menu related call here"); //  TODO 0x1009a5a0

                Logger.Info("Completed loading of save game");

                UiSystems.Party.Update();

                // Allow mods to load their own data from the savegame
                var saveGameHook = GameSystems.Script.GetHook<ISaveGameHook>();
                saveGameHook?.OnAfterLoad(currentSaveFolder, saveGameFile);

// todo              if (temple.Dll.GetInstance().HasCo8Hooks())
//               {
//                   // Co8 load hook
//                   var loadHookArgs = Py_BuildValue("(s)", filename.c_str());
//                   GameSystems.Script.ExecuteScript("templeplus.savehook", "load", loadHookArgs);
//                   Py_DECREF(loadHookArgs);
//
//                   if (modSupport.IsCo8NCEdition())
//                   {
//                       modSupport.SetNCGameFlag(true);
//                   }
//                   else
//                   {
//                       modSupport.SetNCGameFlag(false);
//                   }
//               }
            }
            finally
            {
                IsLoading = false;
            }

            return true;
        }

        [TempleDllLocation(0x10002d30)]
        public bool DeleteSave(SaveGameInfo saveGame)
        {
            Logger.Info("Deleting save {0}", saveGame.Path);

            static bool TryDelete(string path)
            {
                try
                {
                    Logger.Debug("Deleting {0}", path);
                    File.Delete(path);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to delete save file {0}: {1}", path, e);
                    return false;
                }
            }

            var success = TryDelete(saveGame.Path);
            if (saveGame.LargeScreenshotPath != null && !TryDelete(saveGame.LargeScreenshotPath))
            {
                success = false;
            }

            if (saveGame.SmallScreenshotPath != null && !TryDelete(saveGame.SmallScreenshotPath))
            {
                success = false;
            }

            if (!TryDelete(saveGame.BasePath + ".tfaf"))
            {
                success = false;
            }

            if (!TryDelete(saveGame.BasePath + ".tfai"))
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
        public bool IsAutosaveBetweenMaps => Globals.Config.AutoSaveBetweenMaps;

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

        [TempleDllLocation(0x10002f00)]
        public List<SaveGameInfo> GetSaveGames()
        {
            var result = new List<SaveGameInfo>();

            foreach (var path in Directory.EnumerateFileSystemEntries(Globals.GameFolders.SaveFolder, "*.gsi"))
            {
                var info = SaveGameInfoReader.Read(path);
                if (info != null)
                {
                    result.Add(info);
                }
            }

            return result;
        }
    }

    public enum SaveGameOrder
    {
        LastModifiedAutoFirst,
        LastModified,
        SlotNumberDescending
    }

    public static class SaveGameInfoExtensions
    {
        private static readonly Comparison<SaveGameInfo> SlotComparison =
            (a, b) => b.Slot.CompareTo(a.Slot);

        private static readonly Comparison<SaveGameInfo> LastModifiedComparison =
            (a, b) => b.LastModified.CompareTo(a.LastModified);

        private static readonly Comparison<SaveGameInfo> TypeThenLastModifiedComparison =
            (a, b) =>
            {
                if (a.Type != b.Type)
                {
                    return b.Type.CompareTo(a.Type);
                }

                return b.LastModified.CompareTo(a.LastModified);
            };

        [TempleDllLocation(0x100049f0)]
        public static void Sort(this List<SaveGameInfo> saveGames, SaveGameOrder sortType)
        {
            switch (sortType)
            {
                case SaveGameOrder.LastModifiedAutoFirst:
                    saveGames.Sort(TypeThenLastModifiedComparison);
                    break;
                case SaveGameOrder.LastModified:
                    saveGames.Sort(LastModifiedComparison);
                    break;
                case SaveGameOrder.SlotNumberDescending:
                    saveGames.Sort(SlotComparison);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null);
            }
        }
    }
}
