using System;

namespace OpenTemple.Core.AAS
{
    public class AasException : Exception
    {
        public AasException(string message) : base(message)
        {
        }

        public AasException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}