namespace OpenTemple.Core.Config
{
    public class WindowConfig
    {
        public bool Windowed { get; set; }

        public int MinWidth = 1024;

        public int MinHeight = 768;

        public int Width { get; set; } = 1024;

        public int Height { get; set; } = 768;

        public WindowConfig Copy()
        {
            return (WindowConfig) MemberwiseClone();
        }
    }
}