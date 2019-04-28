using System;

namespace SpicyTemple.Core.TigSubsystems
{
    public class TigException : Exception
    {
        public TigException(string message) : base(message)
        {
        }
    }
}