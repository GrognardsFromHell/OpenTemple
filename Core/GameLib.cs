using System;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core
{
    public class GameLib
    {
        [TempleDllLocation(0x103072B8)]
        private bool _ironmanGame = false;

        [TempleDllLocation(0x10306F44)]
        private static int mIronmanSaveNumber;

        [TempleDllLocation(0x103072C0)]
        private static string mIronmanSaveName;

        [TempleDllLocation(0x10002E20)]
        [TempleDllLocation(0x103072D4)]
        public bool IsLoading { get; private set; }

        [TempleDllLocation(0x10003860)]
        public bool IsIronmanGame
        {
            get => _ironmanGame;
            set => _ironmanGame = value;
        }

        [TempleDllLocation(0x10004870)]
        public bool IronmanSave()
        {
            if (_ironmanGame && mIronmanSaveName != null)
            {
                var filename = $"iron{mIronmanSaveNumber:D4}";
                return SaveGame(filename, mIronmanSaveName);
            }

            return false;
        }

        // Makes a savegame.
        public static bool SaveGame(string filename, string displayName)
        {
            throw new NotImplementedException(); // TODO
        }


        // Loads a game.
        public static bool LoadGame(string filename)
        {
            throw new NotImplementedException(); // TODO
        }

        [TempleDllLocation(0x10001db0)]
        public void Reset()
        {
            GameSystems.ResetGame();

            // TODO: Not fully implemented
            mIronmanSaveName = null;
            _ironmanGame = false;
        }

        [TempleDllLocation(0x10002e50)]
        public bool IsAutosaveBetweenMaps => Globals.Config.GetVanillaInt("autosave_between_maps") != 0;

        [TempleDllLocation(0x10004990)]
        public void MakeAutoSave()
        {
            Stub.TODO();
        }
    }
}