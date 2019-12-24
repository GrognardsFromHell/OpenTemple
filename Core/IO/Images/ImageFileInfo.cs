namespace OpenTemple.Core.IO.Images
{
    /// <summary>
    /// Defines the file formats supported by the imaging functions.
    /// </summary>
    public enum ImageFileFormat
    {
        Unknown,
        BMP,
        JPEG,
        TGA,
        FNTART,
        IMG
    };

    public struct ImageFileInfo
    {
        public ImageFileFormat format;
        public int width;
        public int height;
        public bool hasAlpha;
    }
}