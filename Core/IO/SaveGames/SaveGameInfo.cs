using System;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.IO.SaveGames
{
    public enum SaveGameType
    {
        Normal,
        QuickSave,
        AutoSave,
        IronMan
    }

    public class SaveGameInfo
    {
        public SaveGameType Type { get; set; }

        public int Slot { get; set; }

        public string BasePath { get; set; }

        public string Name { get; set; }

        public string ModuleName { get; set; }

        public string LeaderName { get; set; }

        public string Path { get; set; }

        public DateTime LastModified { get; set; }

        public int LeaderPortrait { get; set; }

        public int LeaderLevel { get; set; }

        public int MapId { get; set; }

        public GameTime GameTime { get; set; }

        public locXY LeaderLoc { get; set; }

        public string SmallScreenshotPath { get; set; }

        public string LargeScreenshotPath { get; set; }

    }
}