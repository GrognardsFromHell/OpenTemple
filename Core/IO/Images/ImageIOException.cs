using System;

namespace OpenTemple.Core.IO.Images
{
    public class ImageIOException : Exception
    {
        public ImageIOException(string message) : base(message)
        {
        }

        public ImageIOException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}