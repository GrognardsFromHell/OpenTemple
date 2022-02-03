using System;

namespace OpenTemple.Core.TigSubsystems;

public class TigException : Exception
{
    public TigException(string message) : base(message)
    {
    }
}