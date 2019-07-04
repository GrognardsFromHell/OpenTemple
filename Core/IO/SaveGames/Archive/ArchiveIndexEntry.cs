namespace SpicyTemple.Core.IO.SaveGames.Archive
{

    public struct ArchiveIndexEntry
    {

        public bool Directory { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Full path to the parent of this archive entry. No leading or trailing backslashes are present.
        /// </summary>
        public string ParentPath { get; set; }

        public string Path
        {
            get
            {
                if (ParentPath.Length == 0)
                {
                    return Name;
                }
                else
                {
                    return ParentPath + @"\" + Name;
                }
            }
        }

        /// <summary>
        /// File size in bytes. Does not apply to directories.
        /// </summary>
        public int Size { get; set; }

        public override string ToString()
        {
            return Path;
        }

    }

}
